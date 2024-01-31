using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.FilterSearch;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Horizon.GraphRepository;
using GPAS.Horizon.Logic;
using GPAS.SearchAround;
using Orient.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.GraphRepositoryPlugins.OrientNet
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private readonly string DataBaseName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.OrientNet.DatabaseName"];
        private ODatabase dataBaseConnection = null;
        private Connection connection = null;
        OntologyMaterial ontologyMaterial = null;
        private static Dictionary<string, string> propertyToClassName = null;
        private Dictionary<long, ODocument> idToORIDMapping = new Dictionary<long, ODocument>();
        private readonly static string IDPropertyKey = "ID";
        private readonly static string ClassificationKey = "Classification";
        private readonly static string TypeURIPropertyKey = "TypeURI";
        private readonly static string InnerPropertyKey = "in";
        private readonly static string OuterPropertyKey = "out";

        public List<string> userGroupsName = new List<string>();
        Dictionary<long, Vertex> verticesToAdd = new Dictionary<long, Vertex>();
        List<VertexProperty> propertiesOfStoredVerticesToAdd = new List<VertexProperty>();
        List<VertexProperty> propertiesOfStoredVerticesToUpdate = new List<VertexProperty>();

        List<Edge> edgesToAdd = new List<Edge>();
        Dictionary<string, List<long>> verticesToDelete = new Dictionary<string, List<long>>();

        Dictionary<long, List<long>> resolvedVerticesToUpdateEdges = new Dictionary<long, List<long>>();

        BatchQueryGenerator batchedQueryGenerator;
        Dictionary<long, VertexOridAccessState> OridStatesPerVerexIDs = new Dictionary<long, VertexOridAccessState>();

        private string GetVertexClassName(string typeUri)
        {
            return string.Format("C{0}", GetMd5HashCode(typeUri));
        }
        private string GetEdgeClassName(string typeUri)
        {
            return string.Format("E{0}", GetMd5HashCode(typeUri));
        }
        private string GetPropertyName(string propertyTypeUri)
        {
            string propertyName = string.Format("P{0}", GetMd5HashCode(propertyTypeUri));
            if (!propertyToClassName.ContainsKey(propertyName))
            {
                propertyToClassName.Add(propertyName, propertyTypeUri);
            }
            return propertyName;
        }
        private string GetMd5HashCode(string str)
        {
            StringBuilder md5Hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(str));
            for (int i = 0; i < bytes.Length; i++)
            {
                md5Hash.Append(bytes[i].ToString("x2"));
            }
            return md5Hash.ToString();
        }

        private const string OrinetDatetimeformat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        private string EncodePropertyValueToUseInRetrieveQuery(string propertyTypeURI, string propertyValue)
        {
            string retrievePropertyValue = string.Empty;
            GraphRepositoryBaseDataTypes propertyBaseDataType = ontologyMaterial.GetBaseDataTypeOfProperty(propertyTypeURI);
            switch (propertyBaseDataType)
            {
                case GraphRepositoryBaseDataTypes.String:
                case GraphRepositoryBaseDataTypes.HdfsURI:
                    retrievePropertyValue = string.Format("'{0}'",
                        Utility.EncodingConverter.GetBase64Encode(propertyValue));
                    break;
                case GraphRepositoryBaseDataTypes.DateTime:
                    retrievePropertyValue = string.Format("'{0}'.asDateTime()", propertyValue);
                    break;
                default:
                    retrievePropertyValue = propertyValue;
                    break;
            }
            return retrievePropertyValue;
        }
        private string EncodePropertyValueToUseInStoreQuery(string propertyTypeURI, string propertyValue)
        {
            GraphRepositoryBaseDataTypes propertyBaseDataType = ontologyMaterial.GetBaseDataTypeOfProperty(propertyTypeURI);
            return EncodePropertyValueToUseInStoreQuery(propertyBaseDataType, propertyValue);
        }
        private string EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes propertyBaseDataType, string propertyValue)
        {
            string retrievePropertyValue = string.Empty;
            switch (propertyBaseDataType)
            {
                case GraphRepositoryBaseDataTypes.String:
                case GraphRepositoryBaseDataTypes.HdfsURI:
                    retrievePropertyValue = Utility.EncodingConverter.GetBase64Encode(propertyValue);
                    break;
                case GraphRepositoryBaseDataTypes.DateTime:
                    retrievePropertyValue = DateTime.Parse(propertyValue).ToString(OrinetDatetimeformat, CultureInfo.InvariantCulture);
                    break;
                default:
                    retrievePropertyValue = propertyValue;
                    break;
            }
            return retrievePropertyValue;
        }

        private OType ConvertPropertyType(GraphRepositoryBaseDataTypes baseDataType)
        {
            OType result;
            switch (baseDataType)
            {
                case GraphRepositoryBaseDataTypes.String:
                case GraphRepositoryBaseDataTypes.HdfsURI:
                case GraphRepositoryBaseDataTypes.None:
                default:
                    result = OType.String;
                    break;
                case GraphRepositoryBaseDataTypes.Double:
                    result = OType.Double;
                    break;
                case GraphRepositoryBaseDataTypes.Long:
                case GraphRepositoryBaseDataTypes.Int:
                    result = OType.Long;
                    break;
                case GraphRepositoryBaseDataTypes.DateTime:
                    result = OType.DateTime;
                    break;
                case GraphRepositoryBaseDataTypes.Boolean:
                    result = OType.Boolean;
                    break;
            }
            return result;
        }

        public MainClass()
        { }
        ~MainClass()
        {
            if (dataBaseConnection != null)
            {
                dataBaseConnection.Close();
            }
        }

        #region Database Management
        public void CreateDataBase()
        {
            Connection connection = new Connection();
            OServer conn = connection.OpenServerConnection();
            try
            {
                if (!conn.DatabaseExist(DataBaseName, OStorageType.PLocal))
                {
                    conn.CreateDatabase(DataBaseName, ODatabaseType.Graph, OStorageType.PLocal);
                }
            }
            finally
            {
                conn.Close();
            }
        }

        public void DropDatabase()
        {
            Connection connection = new Connection();
            connection.OpenServerConnection().DropDatabase(DataBaseName, OStorageType.PLocal);
        }
        public void TruncateDatabase()
        {
            //Truncate Edge Class
            foreach (var typeUri in ontologyMaterial.RelationTypes)
            {
                string className = GetEdgeClassName(typeUri);
                TruncateClassInDatabase(className);
            }

            //Truncate Vertex Class
            foreach (var currentObject in ontologyMaterial.ObjAndRelatedPropTypes)
            {
                string className = GetVertexClassName(currentObject.Key);
                TruncateClassInDatabase(className);
            }
        }

        private void TruncateClassInDatabase(string className)
        {
            dataBaseConnection.Command($"TRUNCATE CLASS {className} UNSAFE ");
        }

        #endregion

        #region Schema Management
        public void InitOntology()
        {
            //localOntology = ontology;
            propertyToClassName = new Dictionary<string, string>();
            propertyToClassName.Add(IDPropertyKey, IDPropertyKey);
            propertyToClassName.Add(TypeURIPropertyKey, TypeURIPropertyKey);
        }

        public void Init(OntologyMaterial ontology)
        {
            try
            {
                //Cache Ontology
                ontologyMaterial = ontology;
                //Create Database
                CreateDataBase();
                //Open Connection
                dataBaseConnection = (new Connection()).OpenConnection();

                SetInitialConfigOnDatabase();

                //Cahe Vertices Properties
                propertyToClassName = new Dictionary<string, string>();
                propertyToClassName.Add(IDPropertyKey, IDPropertyKey);
                propertyToClassName.Add(TypeURIPropertyKey, TypeURIPropertyKey);

                //Create Edges in Database
                GenerateSubClassOfEdgeClass();
                //Create Vertices in Database
                GenerateSubClassOfVertexClass();
            }
            finally
            {
                if (dataBaseConnection != null)
                {
                    dataBaseConnection.Close();
                    dataBaseConnection.Dispose();
                }
            }
        }

        public void SetInitialConfigOnDatabase()
        {
            string query = "ALTER DATABASE DATETIMEFORMAT \"yyyy-MM-dd'T'HH:mm:ss.SSS'Z'\"";
            dataBaseConnection.Command(query);
        }
        private void GenerateSubClassOfVertexClass()
        {
            foreach (var currentOntologyType in ontologyMaterial.ObjAndRelatedPropTypes)
            {
                string className = GetVertexClassName(currentOntologyType.Key);
                CreateVertexInDatabase(className);
                //Add ID Property to Each Vertex
                CreatePropertyForVertexInDatabase(className, IDPropertyKey, OType.Long);
                //Create Index for ID Property for Each Vertex
                CreateIndex(className, IDPropertyKey);
                //Add TypeURI Property to Each Class
                CreatePropertyForVertexInDatabase(className, TypeURIPropertyKey, OType.String);
                foreach (var currentProperty in currentOntologyType.Value)
                {
                    CreateEmbeddedListPropertyForVertexInDatabase(className, GetPropertyName(currentProperty.Key), ConvertPropertyType(currentProperty.Value));
                }
            }
        }
        private void CreateVertexInDatabase(string className)
        {
            if (VertexExistInDatabase(className))
            {
                return;
            }
            dataBaseConnection
                .Create.Class(className)
                    .Extends<OVertex>()
                    .Run();
        }
        private bool VertexExistInDatabase(string className)
        {
            short classID = dataBaseConnection.GetClusterIdFor(className);
            if (classID == -1)
            {
                return false;
            }
            return true;
        }
        private void CreateIndex(string className, string propertyName)
        {
            while (true)
            {
                try
                {
                    string query = string.Format("CREATE INDEX index_{0}_{1} ON {0} ({1}) UNIQUE", className, propertyName);
                    dataBaseConnection.Command(query);
                }
                catch (Exception exp)
                {
                    string pattern = @"^(?=.*\bIndex\b)(?=.*\balready)(?=.*\bexist).*$";
                    Regex regex = new Regex(pattern);
                    Match match = regex.Match(exp.Message.Split(new[] { '\r', '\n' }).FirstOrDefault());
                    if (match.Success)
                    {
                        break;
                    }
                    else
                        continue;
                }
            }
        }

        private void CreatePropertyForVertexInDatabase(string className, string propertyName, OType type)
        {
            if (PropertyOfVertexExistInDatabase(className, propertyName))
            {
                return;
            }
            dataBaseConnection
               .Create
               .Property(propertyName, type)
               .Class(className)
               .Run();
        }
        private void CreateEmbeddedListPropertyForVertexInDatabase(string className, string propertyName, OType type)
        {
            if (PropertyOfVertexExistInDatabase(className, propertyName))
            {
                return;
            }
            dataBaseConnection
            .Create
            .Property(propertyName, OType.EmbeddedList).LinkedType(type)
            .Class(className)
            .Run();
        }
        private bool PropertyOfVertexExistInDatabase(string className, string propertyName)
        {
            var properties = dataBaseConnection.Schema.Properties(className);
            foreach (var item in properties)
            {
                if (item["name"].Equals(propertyName))
                {
                    return true;
                }
            }
            return false;
        }
        private void CreateEdgeInDatabase(string className)
        {
            if (VertexExistInDatabase(className))
            {
                return;
            }
            dataBaseConnection
                .Create.Class(className)
                    .Extends<OEdge>()
                    .Run();
        }
        private void GenerateSubClassOfEdgeClass()
        {
            foreach (var typeUri in ontologyMaterial.RelationTypes)
            {
                string className = GetEdgeClassName(typeUri);
                CreateEdgeInDatabase(className);
            }
        }

        public void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName)
        {
            try
            {
                dataBaseConnection = (new Connection()).OpenConnection();

                foreach (var typeUri in ontologyMaterial.RelationTypes)
                {
                    string className = GetEdgeClassName(typeUri);

                    foreach (var groupName in newGroupsName)
                    {
                        try
                        {
                            dataBaseConnection
                            .Create
                            .Property(groupName, OType.Long)
                            .Class(className)
                            .Run();
                        }
                        catch (Exception exp)
                        {
                            string pattern = @"^(?=.*\b(P|p)roperty\b)(?=.*\balready)(?=.*\b(exist|has)).*$";
                            Regex regex = new Regex(pattern);
                            Match match = regex.Match(exp.Message.Split(new[] { '\r', '\n' }).FirstOrDefault());
                            if (match.Success)
                            {
                                continue;
                            }
                            else
                                throw;
                        }
                    }
                }
            }
            finally
            {
                dataBaseConnection.Close();
                dataBaseConnection.Dispose();
            }
        }
        #endregion

        #region Pend changes
        public void OpenConnetion()
        {
            connection = new Connection();
            dataBaseConnection = connection.OpenConnection();
        }

        public void AddVertices(List<Vertex> vertices)
        {
            foreach (var currentVertex in vertices)
            {
                OridStatesPerVerexIDs.Add(currentVertex.ID, VertexOridAccessState.VertexWillCreate);
                verticesToAdd.Add(currentVertex.ID, currentVertex);
            }
        }

        public void AddVertexProperties(List<VertexProperty> vertexProperties)
        {
            foreach (VertexProperty currentProperty in vertexProperties)
            {
                if (!IsAcceptableByCurrentDriver(currentProperty))
                {
                    // TODO: ثبت لاگ عدم پشتیبانی از تاریخ کنار گذاشته شده
                    continue;
                }

                if (OridStatesPerVerexIDs.ContainsKey(currentProperty.OwnerVertexID))
                {
                    switch (OridStatesPerVerexIDs[currentProperty.OwnerVertexID])
                    {
                        case VertexOridAccessState.OridMustRetrieve:
                            propertiesOfStoredVerticesToAdd.Add(currentProperty);
                            break;
                        case VertexOridAccessState.VertexWillCreate:
                            verticesToAdd[currentProperty.OwnerVertexID].Properties.Add(currentProperty);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    OridStatesPerVerexIDs.Add(currentProperty.OwnerVertexID, VertexOridAccessState.OridMustRetrieve);
                    propertiesOfStoredVerticesToAdd.Add(currentProperty);
                }
            }
        }

        private bool IsAcceptableByCurrentDriver(VertexProperty currentProperty)
        {
            DateTime relatedDateTimeValue;
            if (ontologyMaterial.GetBaseDataTypeOfProperty(currentProperty.TypeUri) == GraphRepositoryBaseDataTypes.DateTime)
            {
                if ((DateTime.TryParse(currentProperty.Value, out relatedDateTimeValue)))
                {
                    if (relatedDateTimeValue >= new DateTime(1970, 1, 1, 0, 0, 1))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void UpsertVertices(List<Vertex> vertices)
        {
            foreach (var currentVertex in vertices)
            {
                UpdateVertexProperties(currentVertex.Properties);
            }
        }

        private void UpdateVertexProperties(List<VertexProperty> vertexProperties)
        {
            foreach (VertexProperty currentProperty in vertexProperties)
            {
                if (OridStatesPerVerexIDs.ContainsKey(currentProperty.OwnerVertexID))
                {
                    switch (OridStatesPerVerexIDs[currentProperty.OwnerVertexID])
                    {
                        case VertexOridAccessState.OridMustRetrieve:
                            propertiesOfStoredVerticesToUpdate.Add(currentProperty);
                            break;
                        case VertexOridAccessState.VertexWillCreate:
                            verticesToAdd[currentProperty.OwnerVertexID].Properties.Add(currentProperty);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    OridStatesPerVerexIDs.Add(currentProperty.OwnerVertexID, VertexOridAccessState.OridMustRetrieve);
                    propertiesOfStoredVerticesToUpdate.Add(currentProperty);
                }
            }
        }

        public List<VertexProperty> GetVertexPropertiesUnion(List<long> ownerVertexIDs)
        {
            List<VertexProperty> vertexProperties = new List<VertexProperty>();
            if (ownerVertexIDs.Count == 0)
                return vertexProperties;
            FillIDToORIDMapping(ownerVertexIDs.Distinct());
            string sqlCommand = string.Empty;
            List<KProperty> properties = new List<KProperty>();
            RetrieveDataClient retrieveDataClient = new RetrieveDataClient();
            properties = retrieveDataClient.RetrievePropertiesOfObjects(ownerVertexIDs);
            foreach (var property in properties)
            {
                vertexProperties.Add
                    (
                        new VertexProperty()
                        {
                            Value = property.Value,
                            TypeUri = property.TypeUri,
                            OwnerVertexTypeURI = property.Owner.TypeUri,
                            OwnerVertexID = property.Owner.Id
                        }
                    );
            }
            return vertexProperties;
        }

        public void UpdateEdge(List<long> verticesID, long masterVertexID, string typeUri)
        {
            if (!OridStatesPerVerexIDs.ContainsKey(masterVertexID))
            {
                OridStatesPerVerexIDs.Add(masterVertexID, VertexOridAccessState.OridMustRetrieve);
            }
            resolvedVerticesToUpdateEdges.Add(masterVertexID, verticesID);
        }

        public void DeleteVertices(string typerUri, List<long> vertexIDs)
        {
            if (verticesToDelete.ContainsKey(GetVertexClassName(typerUri)))
            {
                verticesToDelete[GetVertexClassName(typerUri)].AddRange(vertexIDs);
            }
            else
            {
                verticesToDelete.Add(GetVertexClassName(typerUri), vertexIDs);
            }
        }

        public void AddEdge(List<Edge> edges)
        {
            foreach (var currentEdge in edges)
            {
                if (!OridStatesPerVerexIDs.ContainsKey(currentEdge.SourceVertexID))
                {
                    OridStatesPerVerexIDs.Add(currentEdge.SourceVertexID, VertexOridAccessState.OridMustRetrieve);
                }
                if (!OridStatesPerVerexIDs.ContainsKey(currentEdge.TargetVertexID))
                {
                    OridStatesPerVerexIDs.Add(currentEdge.TargetVertexID, VertexOridAccessState.OridMustRetrieve);
                }
            }
            edgesToAdd.AddRange(edges);
        }
        #endregion

        #region Apply pending changes
        private void FillIDToORIDMapping(IEnumerable<long> ids)
        {
            if (ids == null)
                throw new ArgumentNullException("ids");
            if (!ids.Any())
                return;

            string publishedVerticesID = string.Empty;
            foreach (var id in ids)
            {
                if (!idToORIDMapping.ContainsKey(id))
                {
                    publishedVerticesID += string.Format(" {0} ,", id.ToString());
                }
            }
            if (publishedVerticesID != string.Empty)
            {
                publishedVerticesID = "[" + publishedVerticesID.Remove(publishedVerticesID.Length - 1) + "]";
                //Get ORID From Graph Repository
                string query = "select from V where ID in " + publishedVerticesID;
                var publishedVertices = dataBaseConnection.Command(query).ToList();
                foreach (var document in publishedVertices)
                {
                    object stringID = null;
                    document.TryGetValue(IDPropertyKey, out stringID);
                    long publishedID = long.Parse(stringID.ToString());
                    if (!idToORIDMapping.ContainsKey(publishedID))
                    {
                        idToORIDMapping.Add(publishedID, document);
                    }
                }
            }
        }

        private void FinalizeVerticesToAddQueries()
        {
            foreach (var vertex in verticesToAdd.Values)
            {
                CreateVertex queryGenerator = new CreateVertex(GetVertexClassName(vertex.TypeUri), vertex.ID);
                queryGenerator.AddField(IDPropertyKey, vertex.ID.ToString());
                queryGenerator.AddField(TypeURIPropertyKey, vertex.TypeUri);
                foreach (IGrouping<string, VertexProperty> currentProperty in vertex.Properties.GroupBy(p => p.TypeUri))
                {
                    GraphRepositoryBaseDataTypes propertyBaseDataType = ontologyMaterial.GetBaseDataTypeOfProperty(currentProperty.Key);
                    IEnumerable<string> values = currentProperty.Select(v => EncodePropertyValueToUseInStoreQuery(propertyBaseDataType, v.Value));
                    queryGenerator.AddMultiValueField(GetPropertyName(currentProperty.Key), values);
                }
                batchedQueryGenerator.AddQuery(queryGenerator.GetQueryText());
            }
            verticesToAdd.Clear();
        }

        private void FinalizeEdgeToAddQueries()
        {
            foreach (var currentEdge in edgesToAdd)
            {
                switch (currentEdge.Direction)
                {
                    case LinkDirection.SourceToTarget:
                        GenerateAddEdgeQuery(currentEdge.ID, currentEdge.TypeUri, currentEdge.SourceVertexID, currentEdge.TargetVertexID, currentEdge.Acl);
                        break;
                    case LinkDirection.TargetToSource:
                        GenerateAddEdgeQuery(currentEdge.ID, currentEdge.TypeUri, currentEdge.TargetVertexID, currentEdge.SourceVertexID, currentEdge.Acl);
                        break;
                    case LinkDirection.Bidirectional:
                        GenerateAddEdgeQuery(currentEdge.ID, currentEdge.TypeUri, currentEdge.SourceVertexID, currentEdge.TargetVertexID, currentEdge.Acl);
                        GenerateAddEdgeQuery(currentEdge.ID, currentEdge.TypeUri, currentEdge.TargetVertexID, currentEdge.SourceVertexID, currentEdge.Acl);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            edgesToAdd.Clear();
        }
        private void GenerateAddEdgeQuery(long edgeId, string edgeTypeUri, long sourceVertexId, long targetVertexId, ACL acl)
        {
            VertexOridAccessState sourceVertexState = OridStatesPerVerexIDs[sourceVertexId];
            VertexOridAccessState targetVertexState = OridStatesPerVerexIDs[targetVertexId];
            string edgeClassType = GetEdgeClassName(edgeTypeUri);
            object sourceVertexOrid = null;
            object targetVertexOrid = null;

            if (sourceVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                idToORIDMapping[sourceVertexId].TryGetValue("@ORID", out sourceVertexOrid);
            }
            if (targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                idToORIDMapping[targetVertexId].TryGetValue("@ORID", out targetVertexOrid);
            }

            CreateEdge queryGenerator;
            if (sourceVertexState == VertexOridAccessState.OridMustRetrieve && targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                queryGenerator = new CreateEdge(edgeClassType, ((ORID)sourceVertexOrid).ToString(), ((ORID)targetVertexOrid).ToString());
            }
            else if (sourceVertexState == VertexOridAccessState.OridMustRetrieve && targetVertexState == VertexOridAccessState.VertexWillCreate)
            {
                queryGenerator = new CreateEdge(edgeClassType, ((ORID)sourceVertexOrid).ToString(), targetVertexId);
            }
            else if (sourceVertexState == VertexOridAccessState.VertexWillCreate && targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                queryGenerator = new CreateEdge(edgeClassType, sourceVertexId, ((ORID)targetVertexOrid).ToString());
            }
            else if (sourceVertexState == VertexOridAccessState.VertexWillCreate && targetVertexState == VertexOridAccessState.VertexWillCreate)
            {
                queryGenerator = new CreateEdge(edgeClassType, sourceVertexId, targetVertexId);
            }
            else
            {
                throw new NotSupportedException();
            }
            queryGenerator.AddField(IDPropertyKey, edgeId.ToString());
            queryGenerator.AddField(TypeURIPropertyKey, edgeTypeUri);
            //add group field
            foreach (var permision in acl.Permissions)
            {
                queryGenerator.AddField(permision.GroupName, ((byte)permision.AccessLevel).ToString());
            }
            //add classification field
            queryGenerator.AddField(ClassificationKey, acl.Classification);

            batchedQueryGenerator.AddQuery(queryGenerator.GetQueryText());
        }

        private void FinalizePropertiesOfCurrentlyStoredVertices()
        {
            foreach (IGrouping<long, VertexProperty> propertiesPerOwnerVertex in propertiesOfStoredVerticesToAdd.GroupBy(p => p.OwnerVertexID))
            {
                long ownerVertexID = propertiesPerOwnerVertex.Key;
                ODocument ownerVertexDocument = idToORIDMapping[ownerVertexID];
                UpdateProperty updateProperty = new UpdateProperty(GetVertexClassName(propertiesPerOwnerVertex.First().OwnerVertexTypeURI), propertiesPerOwnerVertex.First().OwnerVertexID);

                if (!idToORIDMapping.ContainsKey(ownerVertexID))
                {
                    throw new InvalidOperationException("Unable to finalizate properties before ORID retrievation");
                }

                foreach (var currentVertexProperty in propertiesPerOwnerVertex.GroupBy(p => p.TypeUri))
                {
                    object propertyValues = null;
                    GraphRepositoryBaseDataTypes propertyBaseDataType = ontologyMaterial.GetBaseDataTypeOfProperty(currentVertexProperty.Key);
                    List<string> retrievePropertyValues = new List<string>();
                    switch (propertyBaseDataType)
                    {
                        // TODO: امکان انتقال فرایندهای تکراری به تابع مجزا بررسی شود
                        case GraphRepositoryBaseDataTypes.String:
                        case GraphRepositoryBaseDataTypes.HdfsURI:
                            List<string> stringPropertyValues = new List<string>();
                            stringPropertyValues.AddRange(currentVertexProperty.Select(v => v.Value));
                            if (ownerVertexDocument.HasField(GetPropertyName(currentVertexProperty.Key)))
                            {
                                ownerVertexDocument.TryGetValue(GetPropertyName(currentVertexProperty.Key), out propertyValues);
                                if (propertyValues is List<object>)
                                {
                                    IEnumerable<string> temp = (propertyValues as List<object>).Cast<string>();
                                    stringPropertyValues.AddRange(temp);
                                }
                                else if (propertyValues is List<string>)
                                {
                                    stringPropertyValues.AddRange((List<string>)propertyValues);
                                }
                            }
                            updateProperty.AddMultiValueField
                                (GetPropertyName(currentVertexProperty.Key)
                                , stringPropertyValues.Select(v => EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.String, v)));
                            break;
                        case GraphRepositoryBaseDataTypes.DateTime:
                            List<DateTime> dateTimePropertyValues = new List<DateTime>();
                            dateTimePropertyValues.AddRange(currentVertexProperty.Select(p => DateTime.Parse(p.Value)));
                            if (ownerVertexDocument.HasField(GetPropertyName(currentVertexProperty.Key)))
                            {
                                ownerVertexDocument.TryGetValue(GetPropertyName(currentVertexProperty.Key), out propertyValues);

                                if (propertyValues is List<object>)
                                {
                                    IEnumerable<DateTime> temp = (propertyValues as List<object>).Cast<DateTime>();
                                    dateTimePropertyValues.AddRange(temp);
                                }
                                else if (propertyValues is List<DateTime>)
                                {
                                    dateTimePropertyValues.AddRange((List<DateTime>)propertyValues);
                                }
                            }
                            updateProperty.AddMultiValueField
                                (GetPropertyName(currentVertexProperty.Key)
                                , dateTimePropertyValues.Select(v => EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.DateTime, v.ToString())));
                            break;
                        case GraphRepositoryBaseDataTypes.Double:
                            List<double> doublePropertyValues = new List<double>();
                            doublePropertyValues.AddRange(currentVertexProperty.Select(v => double.Parse(v.Value)));
                            if (ownerVertexDocument.HasField(GetPropertyName(currentVertexProperty.Key)))
                            {
                                ownerVertexDocument.TryGetValue(GetPropertyName(currentVertexProperty.Key), out propertyValues);
                                if (propertyValues is List<object>)
                                {
                                    IEnumerable<double> temp = (propertyValues as List<object>).Cast<double>();
                                    doublePropertyValues.AddRange(temp);
                                }
                                else if (propertyValues is List<double>)
                                {
                                    doublePropertyValues.AddRange((List<double>)propertyValues);
                                }
                            }
                            updateProperty.AddMultiValueField
                                (GetPropertyName(currentVertexProperty.Key)
                                , doublePropertyValues.Select(v => EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Double, v.ToString())));
                            break;
                        case GraphRepositoryBaseDataTypes.Int:
                        case GraphRepositoryBaseDataTypes.Long:
                            List<long> longPropertyValues = new List<long>();
                            longPropertyValues.AddRange(currentVertexProperty.Select(v => long.Parse(v.Value)));
                            if (ownerVertexDocument.HasField(GetPropertyName(currentVertexProperty.Key)))
                            {
                                ownerVertexDocument.TryGetValue(GetPropertyName(currentVertexProperty.Key), out propertyValues);
                                if (propertyValues is List<object>)
                                {
                                    IEnumerable<long> temp = (propertyValues as List<object>).Cast<long>();
                                    longPropertyValues.AddRange(temp);
                                }
                                else if (propertyValues is List<long>)
                                {
                                    longPropertyValues.AddRange((List<long>)propertyValues);
                                }
                            }
                            updateProperty.AddMultiValueField
                                (GetPropertyName(currentVertexProperty.Key)
                                , longPropertyValues.Select(v => EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Long, v.ToString())));
                            break;
                        case GraphRepositoryBaseDataTypes.Boolean:
                            List<bool> boolPropertyValues = new List<bool>();
                            boolPropertyValues.AddRange(currentVertexProperty.Select(p => bool.Parse(p.Value)));
                            if (ownerVertexDocument.HasField(GetPropertyName(currentVertexProperty.Key)))
                            {
                                ownerVertexDocument.TryGetValue(GetPropertyName(currentVertexProperty.Key), out propertyValues);
                                if (propertyValues is List<object>)
                                {
                                    IEnumerable<bool> temp = (propertyValues as List<object>).Cast<bool>();
                                    boolPropertyValues.AddRange(temp);
                                }
                                else if (propertyValues is List<bool>)
                                {
                                    boolPropertyValues.AddRange((List<bool>)propertyValues);
                                }
                            }
                            updateProperty.AddMultiValueField
                                (GetPropertyName(currentVertexProperty.Key)
                                , boolPropertyValues.Select(v => EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Boolean, v.ToString())));
                            break;
                        default:
                            break;
                    }
                }
                batchedQueryGenerator.AddQuery(updateProperty.GetQueryText());
            }
            propertiesOfStoredVerticesToAdd.Clear();
        }

        private void FinalizeUpdatePropertiesOfCurrentlyStoredVertices()
        {
            foreach (IGrouping<long, VertexProperty> propertiesPerOwnerVertex in propertiesOfStoredVerticesToUpdate.GroupBy(p => p.OwnerVertexID))
            {
                long ownerVertexID = propertiesPerOwnerVertex.Key;
                UpdateProperty updateProperty = new UpdateProperty(GetVertexClassName(propertiesPerOwnerVertex.First().OwnerVertexTypeURI), propertiesPerOwnerVertex.First().OwnerVertexID);
                if (!idToORIDMapping.ContainsKey(ownerVertexID))
                {
                    throw new InvalidOperationException("Unable to finalizate properties before ORID retrievation");
                }
                foreach (var currentVertexProperty in propertiesPerOwnerVertex.GroupBy(p => p.TypeUri))
                {
                    GraphRepositoryBaseDataTypes baseType = ontologyMaterial.GetBaseDataTypeOfProperty(currentVertexProperty.Key);
                    updateProperty.AddMultiValueField
                        (GetPropertyName(currentVertexProperty.Key)
                        , currentVertexProperty.Select(p => EncodePropertyValueToUseInStoreQuery(baseType, p.Value)));
                }
                batchedQueryGenerator.AddQuery(updateProperty.GetQueryText());
            }
            propertiesOfStoredVerticesToUpdate.Clear();
        }

        private void FinalizeEdgeToUpdateQueries()
        {
            if (resolvedVerticesToUpdateEdges.Count == 0)
            {
                return;
            }
            Dictionary<string, ODocument> relatedRelationships = GetRelatedRelationships(resolvedVerticesToUpdateEdges.SelectMany(x => x.Value).ToList());
            if (relatedRelationships.Count == 0)
                return;
            foreach (var item in resolvedVerticesToUpdateEdges)
            {
                long masterVertexID = item.Key;
                VertexOridAccessState masterVertexState = OridStatesPerVerexIDs[masterVertexID];
                foreach (var currentID in item.Value)
                {
                    foreach (var currentRelation in relatedRelationships)
                    {
                        ODocument edge = currentRelation.Value;
                        object sourceVertex = null;
                        object targetVertex = null;
                        object edgeClassType = null;

                        edge.TryGetValue("e" + InnerPropertyKey, out targetVertex);
                        edge.TryGetValue("e" + OuterPropertyKey, out sourceVertex);
                        edge.TryGetValue("e23", out edgeClassType);

                        if (idToORIDMapping[currentID].ORID == (ORID)sourceVertex)
                        {
                            CreateEdge createEdge = new CreateEdge(edgeClassType.ToString(), masterVertexID, targetVertex.ToString());
                            AddEdgeProperties(createEdge, edge);
                            batchedQueryGenerator.AddQuery(createEdge.GetQueryText());
                        }
                        if (idToORIDMapping[currentID].ORID == (ORID)targetVertex)
                        {
                            CreateEdge createEdge = new CreateEdge(edgeClassType.ToString(), sourceVertex.ToString(), masterVertexID);
                            AddEdgeProperties(createEdge, edge);
                            batchedQueryGenerator.AddQuery(createEdge.GetQueryText());
                        }
                    }
                }
            }
            resolvedVerticesToUpdateEdges.Clear();
        }
        private Dictionary<string, ODocument> GetRelatedRelationships(List<long> verticesID)
        {
            Dictionary<string, ODocument> edges = new Dictionary<string, ODocument>();
            List<long> result = new List<long>();
            string sqlCommand = "select e.*,e,e.@class from (MATCH {class: V, as: v1, where: ( ";
            foreach (var vertexID in verticesID)
            {
                sqlCommand += string.Format("ID = {0} OR ", vertexID);
            }
            sqlCommand = sqlCommand.Remove(sqlCommand.Length - 4, 4)
                        + ")}.bothE('E'){as: e} Return e)";
            OCommandResult queryResult = dataBaseConnection.Command(sqlCommand);
            List<ODocument> resultList = queryResult.ToList();
            foreach (ODocument link in resultList)
            {
                object stringID = null;
                var temp = link.TryGetValue("e2", out stringID);
                edges.Add(stringID.ToString(), link);
            }
            return edges;
        }
        private void AddEdgeProperties(CreateEdge createEdge, ODocument edge)
        {
            object fieldValue = null;
            foreach (var key in edge.Keys)
            {
                if (
                     key != "e" + InnerPropertyKey &&
                     key != "e" + OuterPropertyKey &&
                     key != "e2" &&
                     key != "e23" &&
                     (!key.StartsWith("@"))
                    )
                {
                    if (edge.TryGetValue(key, out fieldValue))
                    {
                        string fieldName = key.Remove(0, 1);
                        createEdge.AddField(fieldName, fieldValue.ToString());
                    }
                }
            }
        }

        private void FinalizeDeleteVerticesQueries()
        {
            DeleteVertex queryGenerator = new DeleteVertex();
            if (verticesToDelete.Count == 0)
                return;
            foreach (var item in verticesToDelete)
            {
                foreach (var vertexID in item.Value)
                {
                    queryGenerator.DeleteVertexByID(item.Key, vertexID);
                }
            }
            string newQuery = queryGenerator.GetQueryText();
            batchedQueryGenerator.AddQuery(newQuery);
            verticesToDelete.Clear();
        }

        public void ApplyChanges()
        {
            try
            {
                IEnumerable<long> IdOfVerticesToRetrieve = OridStatesPerVerexIDs
                    .Where(s => s.Value == VertexOridAccessState.OridMustRetrieve)
                    .Select(s => s.Key);
                FillIDToORIDMapping(IdOfVerticesToRetrieve);

                using (batchedQueryGenerator = new BatchQueryGenerator())
                {
                    FinalizeVerticesToAddQueries();
                    FinalizeEdgeToAddQueries();
                    FinalizePropertiesOfCurrentlyStoredVertices();
                    FinalizeUpdatePropertiesOfCurrentlyStoredVertices();
                    FinalizeEdgeToUpdateQueries();
                    FinalizeDeleteVerticesQueries();

                    ExecutePrepairedQueriesAsBatch();
                }
            }
            finally
            {
                dataBaseConnection.Close();
            }
        }
        private void ExecutePrepairedQueriesAsBatch()
        {
            string transactionQuery = batchedQueryGenerator.GetQueryText();
            dataBaseConnection.SqlBatch(transactionQuery).Run();
            batchedQueryGenerator = new BatchQueryGenerator();
        }
        #endregion

        #region Searches
        public List<RelationshipBasedResultsPerSearchedObjects> RetrieveRelatedVertices(long[] searchedVerticesID, 
            string[] destinationVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            List<RelationshipBasedResultsPerSearchedObjects> result = new List<RelationshipBasedResultsPerSearchedObjects>();
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("MATCH {class: V, as: v1, where: ( ");
            foreach (var vertexID in searchedVerticesID)
            {
                sqlCommand.AppendFormat("ID = {0} OR ", vertexID);
            }

            // Generate edge where clause
            StringBuilder edgeWhereClause = new StringBuilder();
            edgeWhereClause.Append("where: ((");
            foreach (var item in authorizationParametters.readableClassifications)
            {
                edgeWhereClause.AppendFormat("Classification = '{0}' OR ", item);
            }

            edgeWhereClause.Remove(edgeWhereClause.Length - 4, 4).Append(") AND (");


            foreach (var item in authorizationParametters.permittedGroupNames)
            {
                edgeWhereClause.AppendFormat("{0} >= {1} OR ", item, (byte)Permission.Read);
            }
            edgeWhereClause.Remove(edgeWhereClause.Length - 4, 4);

            // Continue to query generation
            sqlCommand.Remove(sqlCommand.Length - 4, 4);
            sqlCommand.Append(")}.bothE('E'){as: e1, ").Append(edgeWhereClause.ToString()).Append(" ))}.bothV(){as: v2, where: ((");

            foreach (var vertexTypeURI in destinationVerticesTypeURI)
            {
                sqlCommand.AppendFormat("TypeURI = '{0}' OR ", vertexTypeURI);
            }
            sqlCommand.Remove(sqlCommand.Length - 4, 4);
            sqlCommand.Append(") AND ($matched.v1 != $currentMatch))}");
            sqlCommand.AppendFormat(" Return e1.ID,v1.ID, v2.ID LIMIT {0}", resultLimit);

            OCommandResult queryResult = dataBaseConnection.Command(sqlCommand.ToString());
            List<ODocument> resultList = queryResult.ToList();
            Dictionary<long, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultMapping = new Dictionary<long, List<RelationshipBasedNotLoadedResult>>();
            foreach (ODocument link in resultList)
            {
                object e1_ID = "-1";
                link.TryGetValue("e1_ID", out e1_ID);
                object v1_ID = "-2";
                link.TryGetValue("v1_ID", out v1_ID);
                long searchedObjectID = long.Parse(v1_ID.ToString());
                object v2_ID = "-3";
                link.TryGetValue("v2_ID", out v2_ID);
                if (relationshipBasedNotLoadedResultMapping.ContainsKey(searchedObjectID))
                {
                    relationshipBasedNotLoadedResultMapping[searchedObjectID].Add(
                        new RelationshipBasedNotLoadedResult()
                        {
                            RelationshipID = long.Parse(e1_ID.ToString()),
                            TargetObjectID = long.Parse(v2_ID.ToString())
                        });
                }
                else
                {
                    relationshipBasedNotLoadedResultMapping.Add(
                        searchedObjectID,
                        new List<RelationshipBasedNotLoadedResult>()
                        {
                            new RelationshipBasedNotLoadedResult()
                            {
                                RelationshipID = long.Parse(e1_ID.ToString()),
                                TargetObjectID = long.Parse(v2_ID.ToString())
                             }
                        });

                }
            }
            foreach (long searchedObjectID in relationshipBasedNotLoadedResultMapping.Keys)
            {
                result.Add(
                    new RelationshipBasedResultsPerSearchedObjects()
                    {
                        SearchedObjectID = searchedObjectID,
                        NotLoadedResults = relationshipBasedNotLoadedResultMapping[searchedObjectID].ToArray()
                    });
            }
            return result;
        }

        public List<EventBasedResultsPerSearchedObjects> RetrieveTransitiveRelatedVertices(long[] searchedVerticesID,
            string[] intermediateVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("MATCH {class: V, as: v1, where: ( ");
            foreach (var vertexID in searchedVerticesID)
            {
                sqlCommand.AppendFormat("ID = {0} OR ", vertexID);
            }

            // Generate edge where clause
            StringBuilder edgeWhereClause = new StringBuilder();
            edgeWhereClause.Append("(");
            foreach (var item in authorizationParametters.readableClassifications)
            {
                edgeWhereClause.AppendFormat("Classification = '{0}' OR ", item);
            }
            edgeWhereClause.Remove(edgeWhereClause.Length - 4, 4);
            edgeWhereClause.Append(") AND (");
            foreach (var item in authorizationParametters.permittedGroupNames)
            {
                edgeWhereClause.AppendFormat("{0} >= {1} OR ", item, (byte)Permission.Read);
            }
            edgeWhereClause.Remove(edgeWhereClause.Length - 4, 4);

            sqlCommand.Remove(sqlCommand.Length - 4, 4);
            sqlCommand.Append(")}.bothE('E'){as: e1, where: ( ").Append(edgeWhereClause.ToString()).Append("))}.bothV(){as: v2, where: ((");


            foreach (var vertexTypeURI in intermediateVerticesTypeURI)
            {
                sqlCommand.AppendFormat("TypeURI = '{0}' OR ", vertexTypeURI);
            }

            sqlCommand.Remove(sqlCommand.Length - 4, 4);
            sqlCommand.AppendFormat(") AND ($matched.v1 != $currentMatch))}}.bothE() {{as: e2, where: ( ( {0} )) AND ($matched.e1 != $currentMatch))}}.bothV(){{as: v3, where: (($matched.v2!= $currentMatch) AND ($matched.v1!= $currentMatch))}} Return e1.ID, e2.ID,v1.ID, v3.ID LIMIT {1}", edgeWhereClause, resultLimit);

            OCommandResult result = dataBaseConnection.Command(sqlCommand.ToString());
            List<ODocument> resultList = result.ToList();
            List<EventBasedResultsPerSearchedObjects> resultLinkList = new List<EventBasedResultsPerSearchedObjects>();
            Dictionary<long, List<EventBasedNotLoadedResult>> eventBasedNotLoadedResultMapping = new Dictionary<long, List<EventBasedNotLoadedResult>>();

            foreach (ODocument link in resultList)
            {
                object e1_ID = "-1";
                link.TryGetValue("e1_ID", out e1_ID);
                object e2_ID = "-2";
                link.TryGetValue("e2_ID", out e2_ID);
                object v1_ID = "-3";
                link.TryGetValue("v1_ID", out v1_ID);
                long searchedObjectID = long.Parse(v1_ID.ToString());
                object v3_ID = "-4";
                link.TryGetValue("v3_ID", out v3_ID);

                // از آنجایی که برای ایجاد روابط بدون جهت، دو رابطه‌ی جهت‌دار ساخته می‌شود
                // این شرط از برگداندن نادرست این نوع روابط به عنوان رابطه‌ی تعدی جلوگیری می‌کند
                if (!e1_ID.Equals(e2_ID))
                {
                    if (eventBasedNotLoadedResultMapping.ContainsKey(searchedObjectID))
                    {
                        eventBasedNotLoadedResultMapping[searchedObjectID].Add(
                            new EventBasedNotLoadedResult()
                            {
                                FirstRealationshipID = long.Parse(e1_ID.ToString()),
                                SecondRealationshipID = long.Parse(e2_ID.ToString()),
                                TargetObjectID = long.Parse(v3_ID.ToString())
                            }
                        );
                    }
                    else
                    {
                        eventBasedNotLoadedResultMapping.Add(searchedObjectID, new List<EventBasedNotLoadedResult>() {
                            new EventBasedNotLoadedResult()
                            {
                                FirstRealationshipID = long.Parse(e1_ID.ToString()),
                                SecondRealationshipID = long.Parse(e2_ID.ToString()),
                                TargetObjectID = long.Parse(v3_ID.ToString())
                            }
                        });
                    }
                }
            }
            foreach (var searchedObjectID in eventBasedNotLoadedResultMapping.Keys)
            {
                resultLinkList.Add(new EventBasedResultsPerSearchedObjects()
                {
                    SearchedObjectID = searchedObjectID,
                    NotLoadedResults = eventBasedNotLoadedResultMapping[searchedObjectID].ToArray()
                });
            }
            return resultLinkList;
        }

        public CustomSearchAroundResultIDs[] RetrieveRelatedVerticesWithCustomCriteria(long[] objectIDs, CustomSearchAroundCriteria criteria,
            long resultLimit, AuthorizationParametters authorizationParametters)
        {
            List<CustomSearchAroundResultIDs> customSearchAroundResultIDs = new List<CustomSearchAroundResultIDs>();
            List<KeyValuePair<Guid, string>> customSearchAroundCommandsForEvents = new List<KeyValuePair<Guid, string>>();
            List<KeyValuePair<Guid, string>> customSearchAroundCommandsForRelationship = new List<KeyValuePair<Guid, string>>();

            foreach (var link in criteria.LinksFromSearchSet)
            {
                if (ontologyMaterial.IsEvent(link.LinkTypeUri[0]))
                {
                    customSearchAroundCommandsForEvents.Add
                        (new KeyValuePair<Guid, string>
                            (link.GUID
                            , GenerateTransitveRelatedVerticesRetrieveCommand(objectIDs, link, resultLimit, authorizationParametters)));
                }
                else
                {
                    customSearchAroundCommandsForRelationship.Add
                        (new KeyValuePair<Guid, string>
                            (link.GUID
                            , GenerateRelatedVerticesRetrieveCommand(objectIDs, link, resultLimit, authorizationParametters)));
                }
            }

            for (int i = 0; i < customSearchAroundCommandsForEvents.Count; i++)
            {
                CustomSearchAroundResultIDs customSearchAroundResultID = new CustomSearchAroundResultIDs();
                customSearchAroundResultID.EventBasedNotLoadedResults = new EventBasedResultsPerSearchedObjects[] { };
                customSearchAroundResultID.RelationshipNotLoadedResultIDs = new RelationshipBasedResultsPerSearchedObjects[] { };
                List<EventBasedNotLoadedResult> eventBaseLinks = new List<EventBasedNotLoadedResult>();
                List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects = new List<EventBasedResultsPerSearchedObjects>();

                Dictionary<long, List<EventBasedNotLoadedResult>> eventBasedNotLoadedResultsMapping = new Dictionary<long, List<EventBasedNotLoadedResult>>();

                OCommandResult result = dataBaseConnection.Command(customSearchAroundCommandsForEvents[i].Value);
                List<ODocument> resultList = result.ToList();

                foreach (ODocument link in resultList)
                {
                    object v1_ID = "-1";
                    link.TryGetValue("v1_ID", out v1_ID);
                    long searchedObjectID = long.Parse(v1_ID.ToString());
                    object e1_ID = "-1";
                    link.TryGetValue("e1_ID", out e1_ID);
                    object e2_ID = "-1";
                    link.TryGetValue("e2_ID", out e2_ID);
                    object v3_ID = "-1";
                    link.TryGetValue("v3_ID", out v3_ID);
                    var NotLoadedResults = new EventBasedNotLoadedResult()
                    {
                        FirstRealationshipID = long.Parse(e1_ID.ToString()),
                        SecondRealationshipID = long.Parse(e2_ID.ToString()),
                        TargetObjectID = long.Parse(v3_ID.ToString())
                    };
                    if (eventBasedNotLoadedResultsMapping.ContainsKey(searchedObjectID))
                    {
                        eventBasedNotLoadedResultsMapping[searchedObjectID].Add(NotLoadedResults);
                    }
                    else
                    {
                        eventBasedNotLoadedResultsMapping[searchedObjectID] = new List<EventBasedNotLoadedResult>();
                        eventBasedNotLoadedResultsMapping[searchedObjectID].Add(NotLoadedResults);
                    }
                }

                foreach (var eventBasedNotLoadedResult in eventBasedNotLoadedResultsMapping)
                {
                    eventBasedResultsPerSearchedObjects.Add(
                        new EventBasedResultsPerSearchedObjects()
                        {
                            SearchedObjectID = eventBasedNotLoadedResult.Key,
                            NotLoadedResults = eventBasedNotLoadedResult.Value.ToArray()
                        }
                        );
                }
                customSearchAroundResultID.SearchAroundStepGuid = customSearchAroundCommandsForEvents[i].Key;
                customSearchAroundResultID.EventBasedNotLoadedResults = eventBasedResultsPerSearchedObjects.ToArray();
                customSearchAroundResultIDs.Add(customSearchAroundResultID);
            }


            for (int i = 0; i < customSearchAroundCommandsForRelationship.Count; i++)
            {
                CustomSearchAroundResultIDs customSearchAroundResultID = new CustomSearchAroundResultIDs();
                customSearchAroundResultID.EventBasedNotLoadedResults = new EventBasedResultsPerSearchedObjects[] { };
                customSearchAroundResultID.RelationshipNotLoadedResultIDs = new RelationshipBasedResultsPerSearchedObjects[] { };
                List<RelationshipBasedNotLoadedResult> relationshipBasedNotLoadedResults = new List<RelationshipBasedNotLoadedResult>();

                OCommandResult queryResult = dataBaseConnection.Command(customSearchAroundCommandsForRelationship[i].Value);
                List<ODocument> relationshipBasedResultList = queryResult.ToList();
                Dictionary<long, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultsMapping = new Dictionary<long, List<RelationshipBasedNotLoadedResult>>();
                List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects = new List<RelationshipBasedResultsPerSearchedObjects>();

                foreach (ODocument link in relationshipBasedResultList)
                {
                    object v1_ID = "-1";
                    link.TryGetValue("v1_ID", out v1_ID);
                    long searchedObjectID = long.Parse(v1_ID.ToString());
                    object e1_ID = "-1";
                    link.TryGetValue("e1_ID", out e1_ID);
                    object v2_ID = "-1";
                    link.TryGetValue("v2_ID", out v2_ID);

                    var NotLoadedResults = new RelationshipBasedNotLoadedResult()
                    {
                        RelationshipID = long.Parse(e1_ID.ToString()),
                        TargetObjectID = long.Parse(v2_ID.ToString())
                    };
                    if (relationshipBasedNotLoadedResultsMapping.ContainsKey(searchedObjectID))
                    {
                        relationshipBasedNotLoadedResultsMapping[searchedObjectID].Add(NotLoadedResults);
                    }
                    else
                    {
                        relationshipBasedNotLoadedResultsMapping[searchedObjectID] = new List<RelationshipBasedNotLoadedResult>();
                        relationshipBasedNotLoadedResultsMapping[searchedObjectID].Add(NotLoadedResults);
                    }
                }
                foreach (var relationshipBasedNotLoadedResult in relationshipBasedNotLoadedResultsMapping)
                {
                    relationshipBasedResultsPerSearchedObjects.Add(
                        new RelationshipBasedResultsPerSearchedObjects()
                        {
                            SearchedObjectID = relationshipBasedNotLoadedResult.Key,
                            NotLoadedResults = relationshipBasedNotLoadedResult.Value.ToArray()
                        }
                        );
                }
                customSearchAroundResultID.RelationshipNotLoadedResultIDs = relationshipBasedResultsPerSearchedObjects.ToArray();
                customSearchAroundResultID.SearchAroundStepGuid = customSearchAroundCommandsForRelationship[i].Key;
                customSearchAroundResultIDs.Add(customSearchAroundResultID);
            }
            return customSearchAroundResultIDs.ToArray();
        }

        public List<RelationshipBasedResultsPerSearchedObjects> RetrieveRelatedVertices(Dictionary<string, long[]> searchedVertices, 
            string[] destinationVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            return RetrieveRelatedVertices(searchedVertices.SelectMany(v => v.Value).ToArray(),
                destinationVerticesTypeURI, resultLimit, authorizationParametters);
        }

        public List<EventBasedResultsPerSearchedObjects> RetrieveTransitiveRelatedVertices(Dictionary<string, long[]> searchedVertices,
            string[] intermediateVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            return RetrieveTransitiveRelatedVertices(searchedVertices.SelectMany(v => v.Value).ToArray(),
                intermediateVerticesTypeURI, resultLimit, authorizationParametters);
        }

        public CustomSearchAroundResultIDs[] RetrieveRelatedVerticesWithCustomCriteria(Dictionary<string, long[]> searchedVertices,
            CustomSearchAroundCriteria criteria, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            return RetrieveRelatedVerticesWithCustomCriteria(searchedVertices.SelectMany(v => v.Value).ToArray(),
                 criteria, resultLimit, authorizationParametters);
        }

        public object RetrieveObjectWithId(long objectId)
        {
            throw new NotImplementedException();
        }

        private string GenerateTransitveRelatedVerticesRetrieveCommand(long[] searchedVerticesID, SearchAroundStep searchAroundStep, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("MATCH {class: V, as: v1, where: ( ");
            foreach (var vertexID in searchedVerticesID)
            {
                sqlCommand.AppendFormat("ID = {0} OR ", vertexID);
            }

            // Generate edge where-clause
            StringBuilder edgeWhereCondition = new StringBuilder();
            edgeWhereCondition.Append("(");

            foreach (var item in authorizationParametters.readableClassifications)
            {
                edgeWhereCondition.AppendFormat("Classification = '{0}' OR ", item);
            }
            edgeWhereCondition.Remove(edgeWhereCondition.Length - 4, 4);
            edgeWhereCondition.Append(") AND (");
            foreach (var item in authorizationParametters.permittedGroupNames)
            {
                edgeWhereCondition.AppendFormat("{0} >= {1} OR ", item, (byte)Permission.Read);
            }
            edgeWhereCondition.Remove(edgeWhereCondition.Length - 4, 4);
            string edgeWhereClause = "(" + edgeWhereCondition.ToString() + ")) AND (TypeURI = '" + ontologyMaterial.GetDefaultRelationshipTypeForEventBasedLink("", "", "") + "')";
            // End of Generate edge where-clause
            sqlCommand.Remove(sqlCommand.Length - 4, 4);

            StringBuilder eventTypeCondition = new StringBuilder();
            foreach (var linkType in searchAroundStep.LinkTypeUri)
            {
                eventTypeCondition.AppendFormat("TypeURI = '{0}' OR ", linkType);
            }
            eventTypeCondition.Remove(eventTypeCondition.Length - 4, 4);
            eventTypeCondition.Append(") And ");
            foreach (PropertyValueCriteria property in searchAroundStep.EventObjectPropertyCriterias)
            {
                string value = EncodePropertyValueToUseInRetrieveQuery(property.PropertyTypeUri, property.OperatorValuePair.GetInvarientValue());
                eventTypeCondition.AppendFormat("{0} IN {1} AND ", value, GetPropertyName(property.PropertyTypeUri));
            }
            eventTypeCondition.Remove(eventTypeCondition.Length - 4, 4);
            sqlCommand.AppendFormat(")}}.bothE('E'){{as: e1, where: ( {0} )}}.bothV(){{as: v2, where: (({1}", edgeWhereClause, eventTypeCondition);
            sqlCommand.AppendFormat(" AND ($matched.v1 != $currentMatch))}}.bothE() {{as: e2, where: ( ( {0} ) AND ($matched.e1 != $currentMatch))}}.bothV(){{as: v3, where:", edgeWhereClause);

            // Generate vertex where-clause
            StringBuilder vertexWhereClause = new StringBuilder();
            vertexWhereClause.Append("( (($matched.v1 != $currentMatch) AND ($matched.v2 != $currentMatch)) AND  (");
            foreach (var item in searchAroundStep.TargetObjectTypeUri)
            {
                vertexWhereClause.AppendFormat("TypeURI = '{0}' OR ", item);
            }
            vertexWhereClause.Remove(vertexWhereClause.Length - 4, 4);
            vertexWhereClause.AppendFormat(") AND ", searchAroundStep.TargetObjectTypeUri);
            foreach (PropertyValueCriteria property in searchAroundStep.TargetObjectPropertyCriterias)
            {
                string value = EncodePropertyValueToUseInRetrieveQuery(property.PropertyTypeUri, property.OperatorValuePair.GetInvarientValue());
                vertexWhereClause.AppendFormat("{0} IN {1} AND ", value, GetPropertyName(property.PropertyTypeUri));
            }
            vertexWhereClause.Remove(vertexWhereClause.Length - 4, 4);
            vertexWhereClause.Append(")");
            // End of Generate vetex where-clause
            sqlCommand.Append(vertexWhereClause.ToString());
            sqlCommand.AppendFormat("}} Return v1.ID, e1.ID, e2.ID, v3.ID LIMIT {0}", resultLimit.ToString());

            return sqlCommand.ToString();
        }

        private string GenerateRelatedVerticesRetrieveCommand(long[] searchedVerticesID, SearchAroundStep searchAroundStep, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            StringBuilder sqlCommand = new StringBuilder();
            sqlCommand.Append("MATCH {class: V, as: v1, where: ( ");
            foreach (var vertexID in searchedVerticesID)
            {
                sqlCommand.AppendFormat("ID = {0} OR ", vertexID);
            }
            sqlCommand.Remove(sqlCommand.Length - 4, 4);

            // Generate edge where-clause
            StringBuilder edgeWhereCondition = new StringBuilder();
            edgeWhereCondition.Append("(");
            foreach (var item in authorizationParametters.readableClassifications)
            {
                edgeWhereCondition.AppendFormat("Classification = '{0}' OR ", item);
            }
            edgeWhereCondition.Remove(edgeWhereCondition.Length - 4, 4);
            edgeWhereCondition.Append(") AND (");
            foreach (var item in authorizationParametters.permittedGroupNames)
            {
                edgeWhereCondition.AppendFormat("{0} >= {1} OR ", item, (byte)Permission.Read);
            }
            StringBuilder edgeTypeCondition = new StringBuilder();
            foreach (var linkType in searchAroundStep.LinkTypeUri)
            {
                edgeTypeCondition.AppendFormat("TypeURI = '{0}' OR ", linkType);
            }
            edgeTypeCondition.Remove(edgeTypeCondition.Length - 4, 4);
            string edgeWhereClause = edgeWhereCondition.ToString();
            edgeWhereClause = "(" + edgeWhereClause.Remove(edgeWhereClause.Length - 4, 4) + ")) AND (" + edgeTypeCondition.ToString() + ")";
            // End of Generate edge where-clause
            sqlCommand.AppendFormat(")}}.bothE('E'){{as: e1, where: ( {0} )}}.bothV(){{as: v2, where:", edgeWhereClause);

            // Generate vertex where-clause
            StringBuilder vertexWhereClause = new StringBuilder();
            vertexWhereClause.Append("(((");
            foreach (var item in searchAroundStep.TargetObjectTypeUri)
            {
                vertexWhereClause.AppendFormat("TypeURI = '{0}' OR ", item);
            }
            vertexWhereClause.Remove(vertexWhereClause.Length - 4, 4);

            vertexWhereClause.Append(") AND ");
            foreach (PropertyValueCriteria property in searchAroundStep.TargetObjectPropertyCriterias)
            {
                string value = EncodePropertyValueToUseInRetrieveQuery(property.PropertyTypeUri, property.OperatorValuePair.GetInvarientValue());
                vertexWhereClause.AppendFormat(string.Format(" {0} IN {1} AND ", value, GetPropertyName(property.PropertyTypeUri)));
            }
            vertexWhereClause.Remove(vertexWhereClause.Length - 5, 5);
            vertexWhereClause.Append(")");
            // End of Generate vetex where-clause

            sqlCommand.AppendFormat("{0} AND ($matched.v1 != $currentMatch))}} Return v1.ID, e1.ID, v2.ID LIMIT {1}", vertexWhereClause.ToString(), resultLimit);
            return sqlCommand.ToString();
        }

        #endregion

        public List<IndexModel> GetAllIndexes()
        {
            throw new NotImplementedException();
        }

        public void CreateIndex(IndexModel index)
        {
            throw new NotImplementedException();
        }

        public void EditIndex(IndexModel oldIndex, IndexModel newIndex)
        {
            throw new NotImplementedException();
        }

        public void DeleteIndex(IndexModel index)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllIndexes()
        {
            throw new NotImplementedException();
        }        
    }
}
