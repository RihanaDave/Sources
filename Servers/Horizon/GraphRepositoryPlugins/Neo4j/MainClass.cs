using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Horizon.GraphRepository;
using GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators;
using GPAS.Horizon.Logic;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        public static OntologyMaterial Ontology { get; private set; }

        private readonly string DatabaseName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.Neo4j.DatabaseName"];
        private readonly int edgesBatchLimit = 1000;
        private readonly int verticesBatchLimit = 10000;

        private readonly static string IDPropertyKey = "ID";
        private readonly static string ClassificationKey = "Classification";

        private readonly Dictionary<long, Vertex> verticesToAdd = new Dictionary<long, Vertex>();
        private readonly Dictionary<string, List<long>> verticesToDelete = new Dictionary<string, List<long>>();
        private readonly List<VertexProperty> propertiesOfStoredVerticesToAdd = new List<VertexProperty>();
        private readonly List<VertexProperty> propertiesOfStoredVerticesToUpdate = new List<VertexProperty>();
        private readonly List<ResolveModel> resolvedVerticesToUpdateEdges = new List<ResolveModel>();
        private readonly Dictionary<long, INode> retrievedRecords = new Dictionary<long, INode>();

        private readonly List<Edge> edgesToAdd = new List<Edge>();

        private BatchQueryGenerator batchedQueryGenerator;
        private readonly Dictionary<long, VertexAccessState> StatesPerVerexIDs = new Dictionary<long, VertexAccessState>();

        private readonly List<string> addEgesQueries = new List<string>();
        private readonly List<string> addVerticesQueries = new List<string>();
        private readonly List<string> addIndexQueries = new List<string>();

        public void AddEdge(List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                if (!StatesPerVerexIDs.ContainsKey(edge.SourceVertexID))
                {
                    StatesPerVerexIDs.Add(edge.SourceVertexID, VertexAccessState.MustRetrieve);
                }
                if (!StatesPerVerexIDs.ContainsKey(edge.TargetVertexID))
                {
                    StatesPerVerexIDs.Add(edge.TargetVertexID, VertexAccessState.MustRetrieve);
                }
            }

            edgesToAdd.AddRange(edges);
        }

        public void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName)
        {
            // Method intentionally left empty.
        }

        public void AddVertexProperties(List<VertexProperty> vertexProperies)
        {
            foreach (VertexProperty property in vertexProperies)
            {
                if (StatesPerVerexIDs.ContainsKey(property.OwnerVertexID))
                {
                    switch (StatesPerVerexIDs[property.OwnerVertexID])
                    {
                        case VertexAccessState.MustRetrieve:
                            propertiesOfStoredVerticesToAdd.Add(property);
                            break;
                        case VertexAccessState.WillCreate:
                            verticesToAdd[property.OwnerVertexID].Properties.Add(property);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    StatesPerVerexIDs.Add(property.OwnerVertexID, VertexAccessState.MustRetrieve);
                    propertiesOfStoredVerticesToAdd.Add(property);
                }
            }
        }

        public void AddVertices(List<Vertex> vertices)
        {
            foreach (var vertex in vertices)
            {
                StatesPerVerexIDs.Add(vertex.ID, VertexAccessState.WillCreate);
                verticesToAdd.Add(vertex.ID, vertex);
            }
        }

        public void ApplyChanges()
        {
            IEnumerable<long> IdOfVerticesToRetrieve = StatesPerVerexIDs
                .Where(s => s.Value == VertexAccessState.MustRetrieve).Select(s => s.Key);

            _ = Task.Run(async () => await FillRetrievedRecords(IdOfVerticesToRetrieve)).Result;

            batchedQueryGenerator = new BatchQueryGenerator();

            try
            {
                FinalizeCreateIndexQueries();
                FinalizeVerticesToAddQueries();
                FinalizeEdgeToAddQueries();
                FinalizeAddPropertiesToStoredVertices();
                FinalizeUpdatePropertiesOfCurrentlyStoredVertices();
                FinalizeEdgeToUpdateQueries();
                FinalizeDeleteVerticesQueries();
            }
            catch (Exception e)
            {
                string f = e.Message;
            }


            if (addVerticesQueries.Count != 0)
            {
                foreach (var query in addVerticesQueries)
                {
                    _ = Task.Run(async () => await RunQuery(query, true)).Result;
                }

                if (addIndexQueries.Count != 0)
                {
                    foreach (var query in addIndexQueries)
                    {
                        _ = Task.Run(async () => await RunQuery(query, true)).Result;
                    }
                }
            }

            if (addEgesQueries.Count != 0)
            {
                foreach (var query in addEgesQueries)
                {
                    _ = Task.Run(async () => await RunQuery(query, true)).Result;
                }
            }

            string finalQuery = batchedQueryGenerator.GetFinalQuery();
            batchedQueryGenerator.Reset();

            if (finalQuery.Length >= 1)
                _ = Task.Run(async () => await RunQuery(finalQuery, true)).Result;
        }

        public void DeleteVertices(string typerUri, List<long> vertices)
        {
            if (verticesToDelete.ContainsKey(typerUri))
            {
                verticesToDelete[typerUri].AddRange(vertices);
            }
            else
            {
                verticesToDelete.Add(typerUri, vertices);
            }
        }

        public void DropDatabase()
        {
            _ = Task.Run(async () => await RunQuery($"DROP DATABASE {DatabaseName} IF EXISTS")).Result;
        }

        public List<VertexProperty> GetVertexPropertiesUnion(List<long> ownerVertexIDs)
        {
            List<VertexProperty> vertexProperties = new List<VertexProperty>();

            if (ownerVertexIDs.Count == 0)
                return vertexProperties;

            Task.Run(async () => await FillRetrievedRecords(ownerVertexIDs));

            RetrieveDataClient retrieveDataClient = new RetrieveDataClient();
            List<KProperty> properties = retrieveDataClient.RetrievePropertiesOfObjects(ownerVertexIDs);

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

        public void Init(OntologyMaterial ontologyMaterial)
        {
            Ontology = ontologyMaterial;
            Serialization.Ontology = ontologyMaterial;

            //_ = Task.Run(async () => await RunQuery($"CREATE DATABASE {DatabaseName} IF NOT EXISTS")).Result;
        }

        public void OpenConnetion()
        {
            // Method intentionally left empty.
        }

        #region Search

        public List<RelationshipBasedResultsPerSearchedObjects> RetrieveRelatedVertices(Dictionary<string, long[]> searchedVertices,
            string[] destinationVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            List<RelationshipBasedResultsPerSearchedObjects> result = new List<RelationshipBasedResultsPerSearchedObjects>();

            GetRelatedVertices getRelatedVertices = new GetRelatedVertices(searchedVertices, destinationVerticesTypeURI,
                resultLimit, authorizationParametters, IDPropertyKey, ClassificationKey);

            var retrivedRecords = Task.Run(async () => await RunRetriveQuery(getRelatedVertices.GetQuery())).Result;

            Dictionary<string, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultMapping =
                new Dictionary<string, List<RelationshipBasedNotLoadedResult>>();

            foreach (var record in retrivedRecords)
            {
                string sID = record["sID"].ToString();
                string rID = record["rID"].ToString();
                string tID = record["tID"].ToString();

                if (relationshipBasedNotLoadedResultMapping.ContainsKey(sID))
                {
                    relationshipBasedNotLoadedResultMapping[sID].Add(
                        new RelationshipBasedNotLoadedResult()
                        {
                            RelationshipID = long.Parse(rID),
                            TargetObjectID = long.Parse(tID)
                        });
                }
                else
                {
                    relationshipBasedNotLoadedResultMapping.Add(sID,
                        new List<RelationshipBasedNotLoadedResult>()
                        {
                            new RelationshipBasedNotLoadedResult()
                            {
                                RelationshipID = long.Parse(rID),
                                TargetObjectID = long.Parse(tID)
                             }
                        });
                }
            }

            foreach (string searchedObjectID in relationshipBasedNotLoadedResultMapping.Keys)
            {
                result.Add(new RelationshipBasedResultsPerSearchedObjects()
                {
                    SearchedObjectID = long.Parse(searchedObjectID),
                    NotLoadedResults = relationshipBasedNotLoadedResultMapping[searchedObjectID].ToArray()
                });
            }

            return result;
        }

        public List<EventBasedResultsPerSearchedObjects> RetrieveTransitiveRelatedVertices(Dictionary<string, long[]> searchedVertices,
            string[] intermediateVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            List<EventBasedResultsPerSearchedObjects> resultLinkList = new List<EventBasedResultsPerSearchedObjects>();

            GetTransitiveRelatedVertices getTransitiveRelatedVertices = new GetTransitiveRelatedVertices(searchedVertices,
                intermediateVerticesTypeURI, resultLimit, authorizationParametters, IDPropertyKey, ClassificationKey);

            var retrivedRecords = Task.Run(async () => await RunRetriveQuery(getTransitiveRelatedVertices.GetQuery())).Result;

            Dictionary<string, List<EventBasedNotLoadedResult>> eventBasedNotLoadedResultMapping =
                new Dictionary<string, List<EventBasedNotLoadedResult>>();

            foreach (var record in retrivedRecords)
            {
                string sID = record["sID"].ToString();
                string r1ID = record["r1ID"].ToString();
                string r2ID = record["r2ID"].ToString();
                string tID = record["tID"].ToString();

                // از آنجایی که برای ایجاد روابط بدون جهت، دو رابطه‌ی جهت‌دار ساخته می‌شود
                // این شرط از برگداندن نادرست این نوع روابط به عنوان رابطه‌ی تعدی جلوگیری می‌کند

                if (!sID.Equals(tID))
                {
                    if (eventBasedNotLoadedResultMapping.ContainsKey(sID))
                    {
                        eventBasedNotLoadedResultMapping[sID].Add(
                            new EventBasedNotLoadedResult()
                            {
                                FirstRealationshipID = long.Parse(r1ID.ToString()),
                                SecondRealationshipID = long.Parse(r2ID.ToString()),
                                TargetObjectID = long.Parse(tID.ToString())
                            }
                        );
                    }
                    else
                    {
                        eventBasedNotLoadedResultMapping.Add(sID, new List<EventBasedNotLoadedResult>() {
                            new EventBasedNotLoadedResult()
                            {
                                FirstRealationshipID = long.Parse(r1ID.ToString()),
                                SecondRealationshipID = long.Parse(r2ID.ToString()),
                                TargetObjectID = long.Parse(tID.ToString())
                            }
                        });
                    }
                }
            }

            foreach (var searchedObjectID in eventBasedNotLoadedResultMapping.Keys)
            {
                resultLinkList.Add(new EventBasedResultsPerSearchedObjects()
                {
                    SearchedObjectID = long.Parse(searchedObjectID),
                    NotLoadedResults = eventBasedNotLoadedResultMapping[searchedObjectID].ToArray()
                });
            }

            return resultLinkList;
        }

        public CustomSearchAroundResultIDs[] RetrieveRelatedVerticesWithCustomCriteria(Dictionary<string, long[]> searchedVertices,
            SearchAround.CustomSearchAroundCriteria criteria, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            List<CustomSearchAroundResultIDs> customSearchAroundResultIDs = new List<CustomSearchAroundResultIDs>();
            List<KeyValuePair<Guid, string>> customSearchAroundCommandsForEvents = new List<KeyValuePair<Guid, string>>();
            List<KeyValuePair<Guid, string>> customSearchAroundCommandsForRelationship = new List<KeyValuePair<Guid, string>>();

            foreach (var link in criteria.LinksFromSearchSet)
            {
                if (Ontology.IsEvent(link.LinkTypeUri[0]))
                {
                    GetTransitiveRelatedVerticesWithCustomCriteria relatedWithEvent =
                        new GetTransitiveRelatedVerticesWithCustomCriteria(searchedVertices, link, resultLimit, authorizationParametters,
                        IDPropertyKey, ClassificationKey);
                    customSearchAroundCommandsForEvents.Add(new KeyValuePair<Guid, string>(link.GUID, relatedWithEvent.GetQuery()));
                }
                else
                {
                    GetRelatedVerticesWithCustomCriteria relatedWithRelation =
                        new GetRelatedVerticesWithCustomCriteria(searchedVertices, link, resultLimit, authorizationParametters,
                        IDPropertyKey, ClassificationKey);
                    customSearchAroundCommandsForRelationship.Add(new KeyValuePair<Guid, string>(link.GUID, relatedWithRelation.GetQuery()));
                }
            }

            for (int i = 0; i < customSearchAroundCommandsForEvents.Count; i++)
            {
                CustomSearchAroundResultIDs customSearchAroundResultID = new CustomSearchAroundResultIDs
                {
                    EventBasedNotLoadedResults = new EventBasedResultsPerSearchedObjects[] { },
                    RelationshipNotLoadedResultIDs = new RelationshipBasedResultsPerSearchedObjects[] { }
                };

                List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects =
                    new List<EventBasedResultsPerSearchedObjects>();

                Dictionary<long, List<EventBasedNotLoadedResult>> eventBasedNotLoadedResultsMapping =
                    new Dictionary<long, List<EventBasedNotLoadedResult>>();

                var retrivedRecords = Task.Run(async () =>
                await RunRetriveQuery(customSearchAroundCommandsForEvents[i].Value)).Result;

                foreach (var record in retrivedRecords)
                {
                    long sID = long.Parse(record["sID"].ToString());
                    long r1ID = long.Parse(record["r1ID"].ToString());
                    long r2ID = long.Parse(record["r2ID"].ToString());
                    long tID = long.Parse(record["tID"].ToString());

                    if (sID != tID)
                    {
                        var NotLoadedResults = new EventBasedNotLoadedResult()
                        {
                            FirstRealationshipID = r1ID,
                            SecondRealationshipID = r2ID,
                            TargetObjectID = tID
                        };

                        if (eventBasedNotLoadedResultsMapping.ContainsKey(sID))
                        {
                            eventBasedNotLoadedResultsMapping[sID].Add(NotLoadedResults);
                        }
                        else
                        {
                            eventBasedNotLoadedResultsMapping[sID] = new List<EventBasedNotLoadedResult>
                            {
                                NotLoadedResults
                            };
                        }
                    }
                }

                foreach (var eventBasedNotLoadedResult in eventBasedNotLoadedResultsMapping)
                {
                    eventBasedResultsPerSearchedObjects.Add(new EventBasedResultsPerSearchedObjects()
                    {
                        SearchedObjectID = eventBasedNotLoadedResult.Key,
                        NotLoadedResults = eventBasedNotLoadedResult.Value.ToArray()
                    });
                }

                if (eventBasedResultsPerSearchedObjects.Count != 0)
                {
                    customSearchAroundResultID.SearchAroundStepGuid = customSearchAroundCommandsForEvents[i].Key;
                    customSearchAroundResultID.EventBasedNotLoadedResults = eventBasedResultsPerSearchedObjects.ToArray();
                    customSearchAroundResultIDs.Add(customSearchAroundResultID);
                }
            }

            for (int i = 0; i < customSearchAroundCommandsForRelationship.Count; i++)
            {
                CustomSearchAroundResultIDs customSearchAroundResultID = new CustomSearchAroundResultIDs
                {
                    EventBasedNotLoadedResults = new EventBasedResultsPerSearchedObjects[] { },
                    RelationshipNotLoadedResultIDs = new RelationshipBasedResultsPerSearchedObjects[] { }
                };

                Dictionary<long, List<RelationshipBasedNotLoadedResult>> relationshipBasedNotLoadedResultsMapping =
                   new Dictionary<long, List<RelationshipBasedNotLoadedResult>>();

                List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects =
                    new List<RelationshipBasedResultsPerSearchedObjects>();

                var retrivedRecords = Task.Run(async () =>
                await RunRetriveQuery(customSearchAroundCommandsForRelationship[i].Value)).Result;

                foreach (var record in retrivedRecords)
                {
                    long sID = long.Parse(record["sID"].ToString());
                    long rID = long.Parse(record["rID"].ToString());
                    long tID = long.Parse(record["tID"].ToString());

                    var NotLoadedResults = new RelationshipBasedNotLoadedResult()
                    {
                        RelationshipID = rID,
                        TargetObjectID = tID
                    };

                    if (relationshipBasedNotLoadedResultsMapping.ContainsKey(sID))
                    {
                        relationshipBasedNotLoadedResultsMapping[sID].Add(NotLoadedResults);
                    }
                    else
                    {
                        relationshipBasedNotLoadedResultsMapping[sID] = new List<RelationshipBasedNotLoadedResult>
                        {
                            NotLoadedResults
                        };
                    }
                }

                foreach (var relationshipBasedNotLoadedResult in relationshipBasedNotLoadedResultsMapping)
                {
                    relationshipBasedResultsPerSearchedObjects.Add(new RelationshipBasedResultsPerSearchedObjects()
                    {
                        SearchedObjectID = relationshipBasedNotLoadedResult.Key,
                        NotLoadedResults = relationshipBasedNotLoadedResult.Value.ToArray()
                    });
                }

                if (relationshipBasedResultsPerSearchedObjects.Count != 0)
                {
                    customSearchAroundResultID.RelationshipNotLoadedResultIDs = relationshipBasedResultsPerSearchedObjects.ToArray();
                    customSearchAroundResultID.SearchAroundStepGuid = customSearchAroundCommandsForRelationship[i].Key;
                    customSearchAroundResultIDs.Add(customSearchAroundResultID);
                }
            }

            return customSearchAroundResultIDs.ToArray();
        }

        public object RetrieveObjectWithId(long objectId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Index

        public List<IndexModel> GetAllIndexes()
        {
            List<IndexModel> result = new List<IndexModel>();
            IndexQueries indexQueries = new IndexQueries();
            var retrivedRecords = Task.Run(async () => await RunRetriveQuery(indexQueries.GetAllIndexesQuery())).Result;

            if (retrivedRecords == null)
                return result;

            string[] indexNames = null;

            foreach (var record in retrivedRecords)
            {
                string indexName = record["name"].ToString();

                if (indexName.StartsWith("Index"))
                    continue;

                indexNames = indexName.Split(new string[] { "xandx" }, StringSplitOptions.None);

                List<string> propertiesType = new List<string>();
                propertiesType.AddRange(indexNames.Skip(1));

                IndexModel index = new IndexModel()
                {
                    NodeType = indexNames[0],
                    PropertiesType = propertiesType.ToArray()
                };

                result.Add(index);
            }

            return result;
        }

        public void CreateIndex(IndexModel index)
        {
            IndexQueries indexQueries = new IndexQueries();
            _ = Task.Run(async () => await RunQuery(indexQueries.CreateIndexQuery(index), true)).Result;
        }

        public void EditIndex(IndexModel oldIndex, IndexModel newIndex)
        {
            DeleteIndex(oldIndex);
            CreateIndex(newIndex);
        }

        public void DeleteIndex(IndexModel index)
        {
            IndexQueries indexQueries = new IndexQueries();
            _ = Task.Run(async () => await RunQuery(indexQueries.DeleteIndexQuery(index), true)).Result;
        }

        public void DeleteAllIndexes()
        {
            IndexQueries indexQueries = new IndexQueries();
            var retrivedRecords = Task.Run(async () => await RunRetriveQuery(indexQueries.GetAllIndexesQuery())).Result;

            if (retrivedRecords == null)
                return;

            List<string> indexesName = new List<string>();

            foreach (var record in retrivedRecords)
            {
                string indexName = record["name"].ToString();

                if (indexName.StartsWith("Index"))
                    continue;

                indexesName.Add(indexName);
            }

            if (indexesName.Count <= 0)
                return;

            foreach (string index in indexesName)
            {
                _ = Task.Run(async () => await RunQuery(indexQueries.DeleteIndexQuery(index), true)).Result;
            }
        }

        #endregion

        public void TruncateDatabase()
        {
            string truncateQuery = "call apoc.periodic.iterate(\"MATCH(n) return n\", \"DETACH DELETE n\", {batchSize:1000})" +
                " yield batches, total return batches, total";
            _ = Task.Run(async () => await RunQuery(truncateQuery, true)).Result;
        }

        public void UpdateEdge(List<long> verticesID, long masterVertexID, string typeUri)
        {
            if (!StatesPerVerexIDs.ContainsKey(masterVertexID))
                StatesPerVerexIDs.Add(masterVertexID, VertexAccessState.MustRetrieve);

            resolvedVerticesToUpdateEdges.Add(new ResolveModel()
            {
                MasterVertexID = masterVertexID,
                VerticesID = verticesID,
                TypeUri = typeUri
            });
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
                if (StatesPerVerexIDs.ContainsKey(currentProperty.OwnerVertexID))
                {
                    switch (StatesPerVerexIDs[currentProperty.OwnerVertexID])
                    {
                        case VertexAccessState.MustRetrieve:
                            propertiesOfStoredVerticesToUpdate.Add(currentProperty);
                            break;
                        case VertexAccessState.WillCreate:
                            verticesToAdd[currentProperty.OwnerVertexID].Properties.Add(currentProperty);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    StatesPerVerexIDs.Add(currentProperty.OwnerVertexID, VertexAccessState.MustRetrieve);
                    propertiesOfStoredVerticesToUpdate.Add(currentProperty);
                }
            }
        }

        private void FinalizeCreateIndexQueries()
        {
            if (verticesToAdd.Count <= 0)
                return;

            foreach (string typeUri in verticesToAdd.GroupBy(x => x.Value.TypeUri).Select(y => y.Key))
            {
                addIndexQueries.Add(batchedQueryGenerator.MakeCreateIndexQuery(typeUri, IDPropertyKey));
            }
        }

        private void FinalizeVerticesToAddQueries()
        {
            if (verticesToAdd.Count <= 0)
                return;

            int i = 0;
            StringBuilder queryBody = new StringBuilder();

            foreach (var vertex in verticesToAdd.Values)
            {
                if (i == verticesBatchLimit)
                {
                    addVerticesQueries.Add(batchedQueryGenerator.MakeCreateVerticesQuery(queryBody.ToString()));
                    queryBody = new StringBuilder();
                    i = 0;
                }

                CreateVertex queryGenerator = new CreateVertex(vertex.TypeUri);
                queryGenerator.AddProperty(IDPropertyKey, $"'{vertex.ID}'");

                foreach (VertexProperty currentProperty in vertex.Properties)
                {
                    GraphRepositoryBaseDataTypes propertyBaseDataType = Ontology.GetBaseDataTypeOfProperty(currentProperty.TypeUri);

                    if (propertyBaseDataType == GraphRepositoryBaseDataTypes.GeoPoint)
                    {
                        GeoPointModel value = Serialization.DeserializJson<GeoPointModel>(currentProperty.Value);
                        queryGenerator.AddGeoProperty(currentProperty.TypeUri, value);
                    }
                    else
                    {
                        string value = Serialization.EncodePropertyValueToUseInStoreQuery(propertyBaseDataType, currentProperty.Value);
                        queryGenerator.AddProperty(currentProperty.TypeUri, value);
                    }
                }

                queryBody.AppendLine(queryGenerator.GetQuery());
                i++;
            }

            if (queryBody.Length != 0)
                addVerticesQueries.Add(batchedQueryGenerator.MakeCreateVerticesQuery(queryBody.ToString()));

            verticesToAdd.Clear();
        }

        private void FinalizeEdgeToAddQueries()
        {
            if (edgesToAdd.Count <= 0)
                return;

            int i = 0;
            StringBuilder queryBody = new StringBuilder();

            foreach (var edge in edgesToAdd)
            {
                if (i == edgesBatchLimit)
                {
                    addEgesQueries.Add(batchedQueryGenerator.MakeCreateEdgesQuery(queryBody.ToString()));
                    queryBody = new StringBuilder();
                    i = 0;
                }

                switch (edge.Direction)
                {
                    case LinkDirection.SourceToTarget:
                        queryBody.AppendLine(GenerateAddEdgeQuery(edge.ID, edge.TypeUri, edge.SourceVertexID, edge.TargetVertexID,
                            edge.SourceVertexTypeUri, edge.TargetVertexTypeUri, edge.Acl));
                        break;
                    case LinkDirection.TargetToSource:
                        queryBody.AppendLine(GenerateAddEdgeQuery(edge.ID, edge.TypeUri, edge.TargetVertexID, edge.SourceVertexID,
                            edge.TargetVertexTypeUri, edge.SourceVertexTypeUri, edge.Acl));
                        break;
                    case LinkDirection.Bidirectional:
                        queryBody.AppendLine(GenerateAddEdgeQuery(edge.ID, edge.TypeUri, edge.SourceVertexID, edge.TargetVertexID,
                            edge.SourceVertexTypeUri, edge.TargetVertexTypeUri, edge.Acl));
                        queryBody.AppendLine(GenerateAddEdgeQuery(edge.ID, edge.TypeUri, edge.TargetVertexID, edge.SourceVertexID,
                            edge.TargetVertexTypeUri, edge.SourceVertexTypeUri, edge.Acl));
                        break;
                    default:
                        throw new NotSupportedException();
                }

                i++;
            }

            if (queryBody.Length != 0)
                addEgesQueries.Add(batchedQueryGenerator.MakeCreateEdgesQuery(queryBody.ToString()));

            edgesToAdd.Clear();
        }

        public IEnumerable<T> MakePropertyNewValues<T>(IGrouping<string, VertexProperty> item, INode node)
        {
            object propertyOldValues = null;
            List<T> propertyValues = new List<T>();
            propertyValues.AddRange(item.Select(v => (T)Convert.ChangeType(v.Value, typeof(T))));

            if (NodeOperation.HasProperty(node, item.Key))
            {
                NodeOperation.TryGetPropertyValue(node, item.Key, out propertyOldValues);

                if (propertyOldValues != null)
                {
                    var values = propertyOldValues.ToString().Split(',');
                    propertyValues.AddRange(values.Select(p => (T)Convert.ChangeType(p, typeof(T))));
                }
            }

            return propertyValues;
        }

        private void FinalizeAddPropertiesToStoredVertices()
        {
            if (propertiesOfStoredVerticesToAdd.Count == 0)
                return;

            StringBuilder queryBody = new StringBuilder();

            foreach (IGrouping<long, VertexProperty> propertiesPerOwnerVertex in propertiesOfStoredVerticesToAdd.GroupBy(p => p.OwnerVertexID))
            {
                long ownerVertexID = propertiesPerOwnerVertex.Key;

                if (!retrievedRecords.ContainsKey(ownerVertexID))
                    throw new InvalidOperationException("Unable to finalize properties before node retrieving");

                UpdateProperty updateProperty = new UpdateProperty(
                    propertiesPerOwnerVertex.First().OwnerVertexTypeURI,
                    IDPropertyKey,
                    propertiesPerOwnerVertex.First().OwnerVertexID);

                foreach (var property in propertiesPerOwnerVertex)
                {
                    GraphRepositoryBaseDataTypes propertyBaseDataType = Ontology.GetBaseDataTypeOfProperty(property.TypeUri);

                    switch (propertyBaseDataType)
                    {
                        case GraphRepositoryBaseDataTypes.String:
                        case GraphRepositoryBaseDataTypes.HdfsURI:
                            updateProperty.AddProperty(property.TypeUri,
                                Serialization.EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.String, property.Value));
                            break;
                        case GraphRepositoryBaseDataTypes.DateTime:
                            updateProperty.AddProperty(property.TypeUri,
                                Serialization.EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.DateTime, property.Value));
                            break;
                        case GraphRepositoryBaseDataTypes.Double:
                            updateProperty.AddProperty(property.TypeUri,
                                Serialization.EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Double, property.Value));
                            break;
                        case GraphRepositoryBaseDataTypes.Int:
                        case GraphRepositoryBaseDataTypes.Long:
                            updateProperty.AddProperty(property.TypeUri,
                                Serialization.EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Long, property.Value));
                            break;
                        case GraphRepositoryBaseDataTypes.Boolean:
                            updateProperty.AddProperty(property.TypeUri,
                                Serialization.EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes.Boolean, property.Value));
                            break;
                        case GraphRepositoryBaseDataTypes.GeoPoint:
                            updateProperty.AddGeoProperty(property.TypeUri, Serialization.DeserializJson<GeoPointModel>(property.Value));
                            break;
                        default:
                            break;
                    }
                }

                queryBody.AppendLine(updateProperty.GetQuery());
            }

            batchedQueryGenerator.AddUpdatePropertiesQuery(queryBody.ToString());
            propertiesOfStoredVerticesToAdd.Clear();
        }

        private void FinalizeUpdatePropertiesOfCurrentlyStoredVertices()
        {
            if (propertiesOfStoredVerticesToUpdate.Count == 0)
                return;

            StringBuilder queryBody = new StringBuilder();

            foreach (IGrouping<long, VertexProperty> propertiesPerOwnerVertex in propertiesOfStoredVerticesToUpdate.GroupBy(p => p.OwnerVertexID))
            {
                long ownerVertexID = propertiesPerOwnerVertex.Key;

                if (!retrievedRecords.ContainsKey(ownerVertexID))
                    throw new InvalidOperationException("Unable to finalize properties before node retrieving");

                UpdateProperty updateProperty = new UpdateProperty(
                    propertiesPerOwnerVertex.First().OwnerVertexTypeURI,
                    IDPropertyKey,
                    propertiesPerOwnerVertex.First().OwnerVertexID);

                foreach (var property in propertiesPerOwnerVertex)
                {
                    GraphRepositoryBaseDataTypes baseType = Ontology.GetBaseDataTypeOfProperty(property.TypeUri);

                    if (baseType == GraphRepositoryBaseDataTypes.GeoPoint)
                    {
                        updateProperty.AddGeoProperty(property.TypeUri, Serialization.DeserializJson<GeoPointModel>(property.Value));
                    }
                    else
                    {
                        updateProperty.AddProperty(property.TypeUri,
                            Serialization.EncodePropertyValueToUseInStoreQuery(baseType, property.Value));
                    }

                }

                queryBody.AppendLine(updateProperty.GetQuery());
            }

            batchedQueryGenerator.AddUpdatePropertiesQuery(queryBody.ToString());
            propertiesOfStoredVerticesToUpdate.Clear();
        }

        private void FinalizeEdgeToUpdateQueries()
        {
            if (resolvedVerticesToUpdateEdges.Count == 0)
                return;

            Dictionary<string, IPath> relatedRelationships =
                GetRelatedRelationships(resolvedVerticesToUpdateEdges.SelectMany(x => x.VerticesID).ToList());

            if (relatedRelationships.Count == 0)
                return;

            int i = 0;
            StringBuilder queryBody = new StringBuilder();

            foreach (var item in resolvedVerticesToUpdateEdges)
            {
                foreach (var currentID in item.VerticesID)
                {
                    foreach (var currentRelation in relatedRelationships)
                    {
                        if (i == edgesBatchLimit)
                        {
                            addEgesQueries.Add(batchedQueryGenerator.MakeCreateEdgesQuery(queryBody.ToString()));
                            queryBody = new StringBuilder();
                            i = 0;
                        }

                        string sourceId = currentRelation.Value.Start.Properties[IDPropertyKey].ToString();
                        string targetId = currentRelation.Value.End.Properties[IDPropertyKey].ToString();
                        string sourceTypeUri = currentRelation.Value.Start.Labels.FirstOrDefault();
                        string targetTypeUri = currentRelation.Value.End.Labels.FirstOrDefault();
                        string edgeType = currentRelation.Value.Relationships.FirstOrDefault().Type;

                        if (retrievedRecords.TryGetValue(currentID, out INode retValue))
                        {
                            if (retValue.Properties[IDPropertyKey].ToString().Equals(sourceId))
                            {
                                CreateEdge queryGenerator = new CreateEdge(edgeType, item.TypeUri, targetTypeUri, IDPropertyKey,
                                    item.MasterVertexID, long.Parse(targetId));

                                foreach (var property in currentRelation.Value.Relationships.FirstOrDefault().Properties)
                                {
                                    queryGenerator.AddProperty(property.Key, property.Value.ToString());
                                }

                                queryBody.AppendLine(queryGenerator.GetQuery());
                            }

                            if (retValue.Properties[IDPropertyKey].ToString().Equals(targetId))
                            {
                                CreateEdge queryGenerator = new CreateEdge(edgeType, sourceTypeUri,
                                    item.TypeUri,
                                    IDPropertyKey, long.Parse(sourceId), item.MasterVertexID);

                                foreach (var property in currentRelation.Value.Relationships.FirstOrDefault().Properties)
                                {
                                    queryGenerator.AddProperty(property.Key, property.Value.ToString());
                                }

                                queryBody.AppendLine(queryGenerator.GetQuery());
                            }
                        }
                    }
                }
            }

            if (queryBody.Length != 0)
                addEgesQueries.Add(batchedQueryGenerator.MakeCreateEdgesQuery(queryBody.ToString()));

            resolvedVerticesToUpdateEdges.Clear();
        }

        private void FinalizeDeleteVerticesQueries()
        {
            if (verticesToDelete.Count == 0)
                return;

            DeleteVertex queryGenerator = new DeleteVertex();
            StringBuilder queryBody = new StringBuilder();

            foreach (var item in verticesToDelete)
            {
                queryBody.AppendLine(queryGenerator.MakeQuery(IDPropertyKey, item.Key, item.Value));
            }

            batchedQueryGenerator.AddDeleteVerticesQuery(queryBody.ToString());
            verticesToDelete.Clear();
        }

        private string GenerateAddEdgeQuery(long id, string typeUri, long sourceVertexId, long targetVertexId,
            string sourceVertexTypeUri, string targetVertexTypeUri, ACL acl)
        {
            CreateEdge queryGenerator = new CreateEdge(typeUri, sourceVertexTypeUri, targetVertexTypeUri,
                IDPropertyKey, sourceVertexId, targetVertexId);

            queryGenerator.AddProperty(IDPropertyKey, id.ToString());
            queryGenerator.AddProperty(ClassificationKey, acl.Classification);

            foreach (var permision in acl.Permissions)
            {
                queryGenerator.AddProperty(permision.GroupName, ((byte)permision.AccessLevel).ToString());
            }

            return queryGenerator.GetQuery();
        }

        private async Task<bool> FillRetrievedRecords(IEnumerable<long> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (!ids.Any())
                return false;

            StringBuilder verticesIdsToRetrive = new StringBuilder();
            foreach (var id in ids)
            {
                if (!retrievedRecords.ContainsKey(id))
                    verticesIdsToRetrive.Append(string.Format("'{0}',", id.ToString()));
            }

            if (verticesIdsToRetrive.Length > 1)
            {
                verticesIdsToRetrive.Length--;
                var retrivedRecords = await RunRetriveQuery($"MATCH (n) WHERE n.{IDPropertyKey} IN [{verticesIdsToRetrive}] RETURN n");

                foreach (var record in retrivedRecords)
                {
                    var node = record["n"].As<INode>();
                    long publishedID = long.Parse(node.Properties[IDPropertyKey].ToString());
                    if (!retrievedRecords.ContainsKey(publishedID))
                    {
                        retrievedRecords.Add(publishedID, node);
                    }
                }
            }

            return true;
        }

        private async Task<bool> RunQuery(string query, bool executeInDatabase = false)
        {
            Connection connection = new Connection();
            IAsyncSession session = executeInDatabase ? connection.OpenDatabaseSession() : connection.OpenServerSession();

            try
            {
                await session.WriteTransactionAsync(async tx =>
                {
                    await tx.RunAsync(query);
                });
            }
            catch (Exception ec)
            {
                string d = ec.Message;
            }
            finally
            {
                await session.CloseAsync();
            }

            return true;
        }

        private async Task<List<IRecord>> RunRetriveQuery(string query)
        {
            Connection connection = new Connection();
            IAsyncSession session = connection.OpenDatabaseSession();

            try
            {
                var queryResults = await session.WriteTransactionAsync(async tx =>
                {
                    var records = new List<IRecord>();
                    var result = await tx.RunAsync(query);

                    while (await result.FetchAsync())
                    {
                        records.Add(result.Current);
                    }

                    return records;
                });

                return queryResults;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        private Dictionary<string, IPath> GetRelatedRelationships(List<long> verticesID)
        {
            string queryAlias = "q";
            Dictionary<string, IPath> edges = new Dictionary<string, IPath>();

            GetRelatedEdges getRelatedEdges = new GetRelatedEdges(verticesID.ToArray(), IDPropertyKey, queryAlias);

            var retrivedRecords = Task.Run(async () => await RunRetriveQuery(getRelatedEdges.GetQuery())).Result;

            foreach (var record in retrivedRecords)
            {
                var item = record[queryAlias].As<IPath>();
                string publishedID = item.Relationships.FirstOrDefault().Properties[IDPropertyKey].ToString();

                if (!edges.ContainsKey(publishedID))
                {
                    edges.Add(publishedID, item);
                }
            }

            return edges;
        }
    }
}
