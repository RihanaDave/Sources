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
using GPAS.Utility;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.GraphRepositoryPlugins.RestAPI
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private readonly string GraphRepositoryIP = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.RestAPI.RepositoryIP"];
        private readonly string UserName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.RestAPI.RepositoryUser"];
        private readonly string Password = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.RestAPI.RepositoryPassword"];
        private readonly string DatabaseName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.RestAPI.DatabaseName"];
        private readonly int Port = 2480;

        static OntologyMaterial ontologyMaterial = null;
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
        Dictionary<long, RestAPI.VertexOridAccessState> OridStatesPerVerexIDs = new Dictionary<long, RestAPI.VertexOridAccessState>();

        private string GetVertexClassName(string typeUri)
        {
            return string.Format("C{0}", EncodingConverter.GetMd5HashCode(typeUri));
        }
        private string GetEdgeClassName(string typeUri)
        {
            return string.Format("E{0}", EncodingConverter.GetMd5HashCode(typeUri));
        }
        private string GetPropertyName(string propertyTypeUri)
        {
            string propertyName = string.Format("P{0}", EncodingConverter.GetMd5HashCode(propertyTypeUri));
            if (!propertyToClassName.ContainsKey(propertyName))
            {
                propertyToClassName.Add(propertyName, propertyTypeUri);
            }
            return propertyName;
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
                case GraphRepositoryBaseDataTypes.Double:
                    if (propertyValue.Contains('.'))
                    {
                        retrievePropertyValue = propertyValue;
                    }
                    else
                    {
                        retrievePropertyValue = propertyValue + ".0";
                    }
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

        private IRestResponse SendRequestToGraphRepository(string restCommand, string requestBody, Method method)
        {
            var client = new RestClient(string.Format("http://{0}:{1}", GraphRepositoryIP, Port));
            var request = new RestRequest(restCommand, method);
            client.Authenticator = new HttpBasicAuthenticator(UserName, Password);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response;
        }

        #region Database Management
        public void CreateDataBase()
        {
            if (!DatabaseExist(DatabaseName))
            {
                string databaseCreationCommand = string.Format("/database/{0}/plocal", DatabaseName);
                var response = SendRequestToGraphRepository(databaseCreationCommand, string.Empty, Method.POST);
                CheckResponseUnsuccessfullness(response);
            }
        }

        private void CheckResponseUnsuccessfullness(IRestResponse response)
        {
            if (!response.IsSuccessful)
            {
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                else
                {
                    throw new Exception(response.Content);
                }
            }
        }

        public void SetInitialConfigOnDatabase()
        {
            string query = "ALTER DATABASE DATETIMEFORMAT \"yyyy-MM-dd'T'HH:mm:ss.SSS'Z'\"";

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", query));

            string alterDatabaseDateTimeFormatCommand = string.Format("/command/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(alterDatabaseDateTimeFormatCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);
        }

        public void DropDatabase()
        {
            string databaseCreationCommand = string.Format("/database/{0}/plocal", DatabaseName);

            var response = SendRequestToGraphRepository(databaseCreationCommand, string.Empty, Method.DELETE);
            CheckResponseUnsuccessfullness(response);
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
            string query = $"TRUNCATE CLASS {className} UNSAFE ";

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", query));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);
        }

        #endregion

        #region Schema Management
        public void Init(OntologyMaterial ontology)
        {
            //Cache Ontology
            ontologyMaterial = ontology;
            //Create Database
            CreateDataBase();

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

            string query = string.Format("CREATE CLASS {0} EXTENDS V", className);

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", query));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);

            var response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);
        }

        private bool DatabaseExist(string databaseName)
        {
            bool result = false;

            string databaseExistanceCommand = string.Format("/listDatabases");

            IRestResponse response = SendRequestToGraphRepository(databaseExistanceCommand, string.Empty, Method.GET);
            CheckResponseUnsuccessfullness(response);

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }

            var databasesJArray = responseJson["databases"];

            List<object> databases = databasesJArray.ToObject<List<object>>();

            foreach (var currentDatabase in databases)
            {
                if (databaseName.Equals(currentDatabase.ToString()))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool VertexExistInDatabase(string className)
        {
            string databaseCreationCommand = string.Format("/class/{0}/{1}", DatabaseName, className);

            IRestResponse response = SendRequestToGraphRepository(databaseCreationCommand, string.Empty, Method.GET);

            // به جای بررسی معمول نتیجه‌ی بازگشتی از سرور، کد خاص قابل استفاده در این تابع افزوده شد است
            //CheckResponseUnsuccessfullness(response);
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                if (response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                else
                {
                    throw new Exception(response.Content);
                }
            }

            if (response.Content.Equals(string.Empty))
            {
                return false;
            }
            return true;
        }
        private void CreateIndex(string className, string propertyName)
        {
            while (true)
            {
                string query = string.Format("CREATE INDEX index_{0}_{1} ON {0} ({1}) UNIQUE", className, propertyName);
                JObject jsonRequest = new JObject();
                jsonRequest.Add(new JProperty("command", query));

                string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
                IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);

                JObject responseJson;
                try
                {
                    responseJson = JObject.Parse(response.Content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
                }

                var responseErrors = responseJson["errors"];
                if (responseErrors != null)
                {
                    string responseContent = responseErrors.Children().ToList().First()["content"].ToString();
                    string pattern = @"^(?=.*\bIndex\b)(?=.*\balready)(?=.*\bexist).*$";
                    Regex regex = new Regex(pattern);
                    Match match = regex.Match(responseContent.Split(new[] { '\r', '\n' }).FirstOrDefault());
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

            string propertyCreationCommand = string.Format("/property/{0}/{1}/{2}/{3}", DatabaseName, className, propertyName, type.ToString().ToUpper());
            var response = SendRequestToGraphRepository(propertyCreationCommand, string.Empty, Method.POST);
            CheckResponseUnsuccessfullness(response);
        }

        private void CreateEmbeddedListPropertyForVertexInDatabase(string className, string propertyName, OType type)
        {
            if (PropertyOfVertexExistInDatabase(className, propertyName))
            {
                return;
            }

            string query = string.Format("CREATE PROPERTY {0}.{1} {2} {3}", className, propertyName, "EMBEDDEDLIST", type.ToString().ToUpper());

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", query));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);

            var response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);
        }
        private bool PropertyOfVertexExistInDatabase(string className, string propertyName)
        {
            string propertyExistanceCommand = string.Format("/class/{0}/{1}", DatabaseName, className);
            IRestResponse response = SendRequestToGraphRepository(propertyExistanceCommand, string.Empty, Method.GET);
            CheckResponseUnsuccessfullness(response);

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }
            var propertyDocuments = responseJson["properties"];
            if (propertyDocuments == null)
            {
                return false;
            }

            List<string> properties = new List<string>();

            foreach (var currentDocument in propertyDocuments.Children())
            {
                properties.Add(currentDocument["name"].ToString());
            }

            if (properties.Contains(propertyName))
            {
                return true;
            }

            return false;
        }
        private void CreateEdgeInDatabase(string className)
        {
            if (VertexExistInDatabase(className))
            {
                return;
            }

            string query = string.Format("CREATE CLASS {0} EXTENDS E", className);

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", query));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);

            //string databaseCreationCommand = string.Format("/class/{0}/{1}", DatabaseName, className);
            var response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);
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
            foreach (var typeUri in ontologyMaterial.RelationTypes)
            {
                string className = GetEdgeClassName(typeUri);

                foreach (var groupName in newGroupsName)
                {
                    try
                    {
                        string propertyCreationCommand = string.Format("/property/{0}/{1}/{2}/{3}", DatabaseName, className, groupName, OType.Long.ToString().ToUpper());
                        IRestResponse response = SendRequestToGraphRepository(propertyCreationCommand, string.Empty, Method.POST);
                        CheckResponseUnsuccessfullness(response);
                    }
                    catch (Exception exp)
                    {
                        string pattern = @"^(?=.*\b(P|p)roperty\b)(?=.*\balready)(?=.*\b(exist|has)).*$";
                        Regex regex = new Regex(pattern);
                        bool matched = false;
                        foreach (string messagePart in exp.Message.Split(new[] { '\r', '\n' }))
                        {
                            Match match = regex.Match(messagePart);
                            if (match.Success)
                            {
                                matched = true;
                            }
                        }

                        if (matched)
                        {
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }
        #endregion

        #region Pend changes
        public void OpenConnetion()
        {

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

                JObject jsonRequest = new JObject();
                jsonRequest.Add(new JProperty("command", query));

                string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
                IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
                CheckResponseUnsuccessfullness(response);

                JObject responseJson;
                try
                {
                    responseJson = JObject.Parse(response.Content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
                }

                var publishedVertexDocuments = responseJson["result"];

                foreach (var document in publishedVertexDocuments.Children<JObject>())
                {
                    ODocument oDocument = new ODocument();
                    foreach (var currentProperty in document.Properties())
                    {
                        OProperty oProperty = null;
                        if (currentProperty.Value is JArray)
                        {
                            List<object> propertyValues = currentProperty.Value.ToObject<List<object>>();
                            oProperty = new OProperty()
                            {
                                Type = currentProperty.Name.ToString(),
                                Values = propertyValues
                            };
                        }
                        else if (currentProperty.Value is JValue)
                        {
                            List<object> propertyValue = new List<object>();
                            propertyValue.Add(currentProperty.Value.ToString());
                            oProperty = new OProperty()
                            {
                                Type = currentProperty.Name.ToString(),
                                Values = propertyValue
                            };
                        }

                        oDocument.Properties.Add(oProperty);
                    }
                    long publishedID = long.Parse(document["ID"].ToString());

                    if (!idToORIDMapping.ContainsKey(publishedID))
                    {
                        idToORIDMapping.Add(publishedID, oDocument);
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
            string sourceVertexOrid = null;
            string targetVertexOrid = null;

            if (sourceVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                if (idToORIDMapping.ContainsKey(sourceVertexId))
                {
                    ODocument oDocument = idToORIDMapping[sourceVertexId];
                    sourceVertexOrid = oDocument.GetPropertyValue("@rid").FirstOrDefault().ToString();
                }
            }
            if (targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                if (idToORIDMapping.ContainsKey(targetVertexId))
                {
                    ODocument oDocument = idToORIDMapping[targetVertexId];
                    targetVertexOrid = oDocument.GetPropertyValue("@rid").FirstOrDefault().ToString();
                }
            }

            CreateEdge queryGenerator;
            if (sourceVertexState == VertexOridAccessState.OridMustRetrieve && targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                queryGenerator = new CreateEdge(edgeClassType, sourceVertexOrid, targetVertexOrid);
            }
            else if (sourceVertexState == VertexOridAccessState.OridMustRetrieve && targetVertexState == VertexOridAccessState.VertexWillCreate)
            {
                queryGenerator = new CreateEdge(edgeClassType, sourceVertexOrid, targetVertexId);
            }
            else if (sourceVertexState == VertexOridAccessState.VertexWillCreate && targetVertexState == VertexOridAccessState.OridMustRetrieve)
            {
                queryGenerator = new CreateEdge(edgeClassType, sourceVertexId, targetVertexOrid);
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


        // TODO - Convert properties accourding to their types ... 
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
                    List<object> propertyValues = null;
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
                                propertyValues = ownerVertexDocument.GetPropertyValue(GetPropertyName(currentVertexProperty.Key));
                                IEnumerable<string> temp = (propertyValues as List<object>).Cast<string>();
                                stringPropertyValues.AddRange(temp);
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
                                propertyValues = ownerVertexDocument.GetPropertyValue(GetPropertyName(currentVertexProperty.Key));
                                IEnumerable<DateTime> temp = (propertyValues as List<object>).Cast<DateTime>();
                                dateTimePropertyValues.AddRange(temp);
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
                                propertyValues = ownerVertexDocument.GetPropertyValue(GetPropertyName(currentVertexProperty.Key));
                                IEnumerable<double> temp = (propertyValues as List<object>).Cast<double>();
                                doublePropertyValues.AddRange(temp);
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
                                propertyValues = ownerVertexDocument.GetPropertyValue(GetPropertyName(currentVertexProperty.Key));
                                IEnumerable<long> temp = (propertyValues as List<object>).Cast<long>();
                                longPropertyValues.AddRange(temp);
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
                                propertyValues = ownerVertexDocument.GetPropertyValue(GetPropertyName(currentVertexProperty.Key));
                                IEnumerable<bool> temp = (propertyValues as List<object>).Cast<bool>();
                                boolPropertyValues.AddRange(temp);
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
                        string sourceVertex = null;
                        string targetVertex = null;
                        string edgeClassType = null;

                        targetVertex = edge.GetPropertyValue("e" + InnerPropertyKey).FirstOrDefault().ToString();
                        sourceVertex = edge.GetPropertyValue("e" + OuterPropertyKey).FirstOrDefault().ToString();
                        edgeClassType = edge.GetPropertyValue("e23").FirstOrDefault().ToString();

                        if (idToORIDMapping[currentID].GetPropertyValue("@rid").FirstOrDefault().Equals(sourceVertex))
                        {
                            CreateEdge createEdge = new CreateEdge(edgeClassType.ToString(), masterVertexID, targetVertex.ToString());
                            AddEdgeProperties(createEdge, edge);
                            batchedQueryGenerator.AddQuery(createEdge.GetQueryText());
                        }
                        if (idToORIDMapping[currentID].GetPropertyValue("@rid").FirstOrDefault().Equals(targetVertex))
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


            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", sqlCommand));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }

            var publishedEdgeDocuments = responseJson["result"];



            foreach (var document in publishedEdgeDocuments.Children<JObject>())
            {
                ODocument oDocument = new ODocument();
                foreach (var currentProperty in document.Properties())
                {
                    List<object> propertyValue = new List<object>();
                    propertyValue.Add(currentProperty.Value.ToString());

                    OProperty oProperty = new OProperty()
                    {
                        Type = currentProperty.Name.ToString(),
                        Values = propertyValue
                    };
                    oDocument.Properties.Add(oProperty);
                }
                string publishedID = document["e2"].ToString();

                if (!edges.ContainsKey(publishedID))
                {
                    edges.Add(publishedID, oDocument);
                }
            }
            return edges;
        }
        private void AddEdgeProperties(CreateEdge createEdge, ODocument edge)
        {
            string fieldValue = null;
            foreach (var key in edge.Properties)
            {
                if (
                     key.Type != "e" + InnerPropertyKey &&
                     key.Type != "e" + OuterPropertyKey &&
                     key.Type != "e2" &&
                     key.Type != "e23" &&
                     (!key.Type.StartsWith("@"))
                    )
                {
                    fieldValue = edge.GetPropertyValue(key.Type).FirstOrDefault().ToString();
                    if (fieldValue != null)
                    {
                        string fieldName = key.Type.Remove(0, 1);
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
        private void ExecutePrepairedQueriesAsBatch()
        {
            string transactionQuery = batchedQueryGenerator.GetQueryText();

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("transaction", true));

            JArray transactionQueryJArray = new JArray();
            transactionQueryJArray.Add(transactionQuery);


            JObject jarrayObj = new JObject();
            jarrayObj.Add(new JProperty("type", "script"));
            jarrayObj.Add(new JProperty("language", "sql"));
            jarrayObj.Add(new JProperty("script", transactionQueryJArray));

            JArray operationJArray = new JArray();
            operationJArray.Add(jarrayObj);
            jsonRequest.Add(new JProperty("operations", operationJArray));

            string executeQueryCommand = string.Format("/batch/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);

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

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", sqlCommand.ToString()));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);

            Dictionary<long, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultMapping = new Dictionary<long, List<RelationshipBasedNotLoadedResult>>();

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }

            var publishedEdgeDocuments = responseJson["result"];

            foreach (var document in publishedEdgeDocuments.Children<JObject>())
            {
                ODocument oDocument = new ODocument();
                foreach (var currentProperty in document.Properties())
                {
                    List<object> propertyValue = new List<object>();
                    propertyValue.Add(currentProperty.Value.ToString());

                    OProperty oProperty = new OProperty()
                    {
                        Type = currentProperty.Name.ToString(),
                        Values = propertyValue
                    };
                    oDocument.Properties.Add(oProperty);
                }

                object e1_ID = "-1";
                e1_ID = oDocument.GetPropertyValue("e1_ID").FirstOrDefault();
                object v1_ID = "-2";
                v1_ID = oDocument.GetPropertyValue("v1_ID").FirstOrDefault();
                long searchedObjectID = long.Parse(v1_ID.ToString());
                object v2_ID = "-3";
                v2_ID = oDocument.GetPropertyValue("v2_ID").FirstOrDefault();
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

            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("command", sqlCommand.ToString()));

            string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);

            List<EventBasedResultsPerSearchedObjects> resultLinkList = new List<EventBasedResultsPerSearchedObjects>();
            Dictionary<long, List<EventBasedNotLoadedResult>> eventBasedNotLoadedResultMapping = new Dictionary<long, List<EventBasedNotLoadedResult>>();

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }

            var publishedEdgeDocuments = responseJson["result"];

            foreach (var document in publishedEdgeDocuments.Children<JObject>())
            {
                ODocument oDocument = new ODocument();
                foreach (var currentProperty in document.Properties())
                {
                    List<object> propertyValue = new List<object>();
                    propertyValue.Add(currentProperty.Value.ToString());

                    OProperty oProperty = new OProperty()
                    {
                        Type = currentProperty.Name.ToString(),
                        Values = propertyValue
                    };
                    oDocument.Properties.Add(oProperty);
                }


                object e1_ID = "-1";
                e1_ID = oDocument.GetPropertyValue("e1_ID").FirstOrDefault();
                object e2_ID = "-2";
                e2_ID = oDocument.GetPropertyValue("e2_ID").FirstOrDefault();
                object v1_ID = "-3";
                v1_ID = oDocument.GetPropertyValue("v1_ID").FirstOrDefault();
                long searchedObjectID = long.Parse(v1_ID.ToString());
                object v3_ID = "-4";
                v3_ID = oDocument.GetPropertyValue("v3_ID").FirstOrDefault();

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


                JObject jsonRequest = new JObject();
                jsonRequest.Add(new JProperty("command", customSearchAroundCommandsForEvents[i].Value));

                string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
                IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
                CheckResponseUnsuccessfullness(response);

                JObject responseJson;
                try
                {
                    responseJson = JObject.Parse(response.Content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
                }

                var publishedEdgeDocuments = responseJson["result"];

                foreach (var document in publishedEdgeDocuments.Children<JObject>())
                {
                    ODocument oDocument = new ODocument();

                    foreach (var currentProperty in document.Properties())
                    {
                        List<object> propertyValue = new List<object>();
                        propertyValue.Add(currentProperty.Value.ToString());

                        OProperty oProperty = new OProperty()
                        {
                            Type = currentProperty.Name.ToString(),
                            Values = propertyValue
                        };
                        oDocument.Properties.Add(oProperty);
                    }

                    object v1_ID = "-1";
                    v1_ID = oDocument.GetPropertyValue("v1_ID").FirstOrDefault();
                    long searchedObjectID = long.Parse(v1_ID.ToString());
                    object e1_ID = "-1";
                    e1_ID = oDocument.GetPropertyValue("e1_ID").FirstOrDefault();
                    object e2_ID = "-1";
                    e2_ID = oDocument.GetPropertyValue("e2_ID").FirstOrDefault();
                    object v3_ID = "-1";
                    v3_ID = oDocument.GetPropertyValue("v3_ID").FirstOrDefault();
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


                JObject jsonRequest = new JObject();
                jsonRequest.Add(new JProperty("command", customSearchAroundCommandsForRelationship[i].Value));

                string executeQueryCommand = string.Format("/command/{0}/sql", DatabaseName);
                IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
                CheckResponseUnsuccessfullness(response);

                Dictionary<long, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultsMapping = new Dictionary<long, List<RelationshipBasedNotLoadedResult>>();
                List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects = new List<RelationshipBasedResultsPerSearchedObjects>();

                JObject responseJson;
                try
                {
                    responseJson = JObject.Parse(response.Content);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
                }

                var publishedEdgeDocuments = responseJson["result"];

                foreach (var document in publishedEdgeDocuments.Children<JObject>())
                {
                    ODocument oDocument = new ODocument();
                    foreach (var currentProperty in document.Properties())
                    {
                        List<object> propertyValue = new List<object>();
                        propertyValue.Add(currentProperty.Value.ToString());

                        OProperty oProperty = new OProperty()
                        {
                            Type = currentProperty.Name.ToString(),
                            Values = propertyValue
                        };
                        oDocument.Properties.Add(oProperty);
                    }

                    object v1_ID = "-1";
                    v1_ID = oDocument.GetPropertyValue("v1_ID").FirstOrDefault();
                    long searchedObjectID = long.Parse(v1_ID.ToString());
                    object e1_ID = "-1";
                    e1_ID = oDocument.GetPropertyValue("e1_ID").FirstOrDefault();
                    object v2_ID = "-1";
                    v2_ID = oDocument.GetPropertyValue("v2_ID").FirstOrDefault();

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
            string sqlCommand = $"SELECT * FROM V WHERE ID = {objectId.ToString()}";

            JObject jsonRequest = new JObject
            {
                new JProperty("command", sqlCommand)
            };

            string executeQueryCommand = $"/command/{DatabaseName}/sql";
            IRestResponse response = SendRequestToGraphRepository(executeQueryCommand, jsonRequest.ToString(), Method.POST);
            CheckResponseUnsuccessfullness(response);

            try
            {
                JObject responseJson = JObject.Parse(response.Content);
                return responseJson;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }
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
