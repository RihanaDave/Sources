using GPAS.AccessControl;
using GPAS.Ontology;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using GPAS.RepositoryServer.Logic;
using GPAS.Utility;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataRepository.MongoDBPlugins
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private readonly string RepositorydbIP = ConfigurationManager.AppSettings["GPAS.DataRepository.MongoDBPlugins.RepositorydbIP"];
        private readonly string RepositorydbPort = ConfigurationManager.AppSettings["GPAS.DataRepository.MongoDBPlugins.RepositorydbPort"];
        private readonly string RepositorydbDatabase = ConfigurationManager.AppSettings["GPAS.DataRepository.MongoDBPlugins.RepositorydbDatabase"];

        IMongoDatabase db;
        MongoClient client;

        public MainClass()
        {
            //client = new MongoClient(string.Format("mongodb://{0}", RepositorydbIP));
            client = new MongoClient(string.Format("mongodb://{0}:{1}", RepositorydbIP, RepositorydbPort));
            db = client.GetDatabase(RepositorydbDatabase);
        }

        #region Objects Retrievation

        public List<DBObject> GetObjects(IEnumerable<long> dbObjectIDs)
        {
            if (!dbObjectIDs.Any())
                return new List<DBObject>();

            List<DBObject> dbObjects = new List<DBObject>();

            IMongoCollection<DBObject> collection = db.GetCollection<DBObject>(ObjectTable.tableName);

            var builder = Builders<DBObject>.Filter;
            var filter = builder.In(p => p.Id, dbObjectIDs);
            var list = collection.Find(filter).ToList();

            dbObjects = MongoDBDataReaderToDBObject(list);

            return dbObjects;
        }

        public List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        {
            List<DBObject> dbObjects = new List<DBObject>();

            IMongoCollection<DBObject> collection = db.GetCollection<DBObject>(ObjectTable.tableName);

            var builder = Builders<DBObject>.Filter;
            var filter = builder.Where(p => p.ResolvedTo != null && p.Id >= firstID && p.Id <= lastID);
            var list = collection.Find(filter).ToList();

            dbObjects = MongoDBDataReaderToDBObject(list);
            return dbObjects;
        }

        private List<DBObject> MongoDBDataReaderToDBObject(List<DBObject> collection)
        {
            List<DBObject> dbObjects = new List<DBObject>();

            foreach (var item in collection)
            {
                long id = item.Id;
                long? labelPropertyID = item.LabelPropertyID;
                string typeUri = item.TypeUri;
                bool isGroup = item.IsGroup;
                long? resolvedTo = (string.IsNullOrEmpty(item.ResolvedTo.ToString())) ? null : new long?(long.Parse(item.ResolvedTo.ToString()));

                dbObjects.Add(new DBObject() { Id = id, LabelPropertyID = labelPropertyID, TypeUri = typeUri, IsGroup = isGroup, ResolvedTo = resolvedTo });
            }

            return dbObjects;
        }

        #endregion

        #region Properties Retrievation

        public List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams)
        {
            if (dbObject.Id == 0)
                throw new ArgumentNullException(nameof(dbObject), "ObjectId is invalid.");

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            List<DBProperty> dbProperties = new List<DBProperty>();

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.Eq(p => p.ObjectId, dbObject.Id);
            var list = collection.Find(filter).ToList();

            dbProperties = MongoDBDDataReaderToDBProperty(list);
            dbProperties = GetReadableSubset(dbProperties, authParams);

            return dbProperties;
        }

        public List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs)
        {
            if (objectIDs.Length == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            List<DBProperty> dbProperties = new List<DBProperty>();

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In(MongoDbPropertyTable.objectid, objectIDs);
            var list = collection.Find(filter).ToList();

            dbProperties.AddRange(MongoDBDDataReaderToDBProperty(list));
            return dbProperties;
        }

        public List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams)
        {
            List<DBProperty> dbProperties = GetPropertiesOfObjects(objectIDs);
            return GetReadableSubset(dbProperties, authParams);
        }

        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authParams)
        {
            if (objectIDs.Count == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");


            List<string> properties = new List<string>();
            foreach (var property in specifiedPropertyTypeUris)
            {
                properties.Add( property );
            }

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In(MongoDbPropertyTable.objectid, objectIDs);
            filter = filter & builder.In(MongoDbPropertyTable.typeuri, properties);
            var list = collection.Find(filter).ToList();

            List<DBProperty> dbProperties;
            dbProperties = MongoDBDDataReaderToDBProperty(list);
            dbProperties = GetReadableSubset(dbProperties, authParams);
            return dbProperties;
        }

        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams)
        {
            if (objectIDs.Count == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            string valueParameterName = "specifiedValue";
            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            List<DBProperty> dbProperties;
            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In(MongoDbPropertyTable.objectid, objectIDs);
            filter = filter & builder.Eq(MongoDbPropertyTable.typeuri, propertyTypeUri);
            filter = filter & builder.Eq(MongoDbPropertyTable.value, valueParameterName);
            var list = collection.Find(filter).ToList();

            dbProperties = MongoDBDDataReaderToDBProperty(list);
            dbProperties = GetReadableSubset(dbProperties, authParams);

            return dbProperties;
        }

        public List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams)
        {
            if (dbPropertyIDs.Count == 0)
                return new List<DBProperty>();

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            List<DBProperty> dbProperties = new List<DBProperty>();

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In(p => p.Id, dbPropertyIDs);
            var list = collection.Find(filter).ToList();

            dbProperties = MongoDBDDataReaderToDBProperty(list);
            dbProperties = GetReadableSubset(dbProperties, authParams);

            return dbProperties;
        }

        private List<DBProperty> MongoDBDDataReaderToDBProperty(List<DBPropertyMongoDbSchema> collection)
        {
            List<DBProperty> dbProperties = new List<DBProperty>();
            HashSet<long> relatedObjectIDs = new HashSet<long>();

            foreach (var item in collection)
            {
                long objectID = item.ObjectId; //item.Owner.Id;
                if (!relatedObjectIDs.Contains(objectID))
                {
                    relatedObjectIDs.Add(objectID);
                }
                long id = item.Id;
                string typeUri = item.TypeUri;
                string value = item.Value;
                long dataSourceId = item.DataSourceID;

                dbProperties.Add(new DBProperty()
                {
                    Id = id,
                    Owner = new DBObject() { Id = objectID },
                    TypeUri = typeUri,
                    Value = value,
                    DataSourceID = dataSourceId,
                    ObjectId = objectID
                });
            }

            // بازیابی و مقدار دهی شئ مالک ویژگی‌ها

            Dictionary<long, DBObject> relatedObjectsPerID = GetObjects(relatedObjectIDs).ToDictionary(o => o.Id);
            var t = relatedObjectIDs;

            foreach (var prop in dbProperties)
            {
                prop.Owner = relatedObjectsPerID[prop.Owner.Id];
            }
            return dbProperties;
        }

        private List<DBProperty> GetReadableSubset(List<DBProperty> dbProperties, AuthorizationParametters authParams)
        {
            long[] dataSourceIDs = dbProperties.Select(p => p.DataSourceID).ToArray();

            HashSet<long> readableDataSourceIDs = GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
            return dbProperties.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        }

        private List<DBProperty> GetPropertiesOfObjects(long[] objectIDs)
        {
            if (objectIDs.Length == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            List<DBProperty> dbProperties = new List<DBProperty>();

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In(MongoDbPropertyTable.objectid, objectIDs);
            var list = collection.Find(filter).ToList();

            dbProperties.AddRange(MongoDBDDataReaderToDBProperty(list));
            return dbProperties;
        }

        #endregion

        #region Relationships Retrievation

        public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();

            if (!dbRelationshipIDs.Any())
                return relationships;

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);
            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.In(p => p.Id, dbRelationshipIDs);
            var list = collection.Find(filter).ToList();

            relationships = MongoDBDataReaderToDBRelationship(list);
            relationships = GetReadableSubset(relationships, authParams);
            return relationships;
        }

        public List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();

            if (!dbRelationshipIDs.Any())
                return relationships;

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.In(MongoDbRelationshipTable.id, dbRelationshipIDs);
            var list = collection.Find(filter).ToList();

            relationships = MongoDBDataReaderToDBRelationship(list);
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.Eq(MongoDbRelationshipTable.typeuri, typeURI);
            filter = filter & builder.Eq(MongoDbRelationshipTable.source, objectID);
            var list = collection.Find(filter).ToList();

            relationships = MongoDBDataReaderToDBRelationship(list);
            relationships = GetReadableSubset(relationships, authParams);
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams)
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();

                var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
                var filter = builder.Eq(MongoDbRelationshipTable.source, currentObjectID);
                filter = filter | builder.Eq(MongoDbRelationshipTable.target, currentObjectID);
                var list = collection.Find(filter).ToList();

                relationships = MongoDBDataReaderToDBRelationship(list);
                relationships = GetReadableSubset(relationships, authParams);
                relationships.AddRange(tempRelationships);
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();

                var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
                var filter = builder.Eq(MongoDbRelationshipTable.source, currentObjectID);
                var list = collection.Find(filter).ToList();

                relationships = MongoDBDataReaderToDBRelationship(list);
                relationships.AddRange(tempRelationships);
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();

                var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
                var filter = builder.Eq(MongoDbRelationshipTable.source, currentObjectID);
                filter = filter | builder.Eq(MongoDbRelationshipTable.target, currentObjectID);
                var list = collection.Find(filter).ToList();

                relationships = MongoDBDataReaderToDBRelationship(list);
                relationships.AddRange(tempRelationships);
            }
            return relationships;
        }

        public List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject), "Object is invalid.");
            if (dbObject.Id == 0)
                throw new ArgumentNullException(nameof(dbObject.Id), "Object ID is invalid.");
            if (typeURI == null)
                throw new ArgumentNullException(nameof(typeURI), "TypeURI of relationship is invalid.");

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);
            List<DBRelationship> relationships = new List<DBRelationship>();

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.Eq(MongoDbRelationshipTable.typeuri, typeURI);
            filter = filter | builder.Eq(MongoDbRelationshipTable.target, dbObject.Id);
            var list = collection.Find(filter).ToList();

            relationships = MongoDBDataReaderToDBRelationship(list);
            relationships = GetReadableSubset(relationships, authParams);
            return relationships;
        }

        public DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams)
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            List<DBRelationship> relationships = new List<DBRelationship>();

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.Eq(MongoDbRelationshipTable.typeuri, typeURI);
            filter = filter & builder.Eq(MongoDbRelationshipTable.source, source);
            filter = filter & builder.Eq(MongoDbRelationshipTable.target, target);
            filter = filter & builder.Eq(MongoDbRelationshipTable.direction, (long)direction);
            var list = collection.Find(filter).ToList();

            relationships = MongoDBDataReaderToDBRelationship(list);
            return relationships.FirstOrDefault();
        }

        public List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID)
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            List<DBRelationship> dbRalationships = new List<DBRelationship>();

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.Where(p => p.Id >= firstID && p.Id <= lastID);
            var list = collection.Find(filter).ToList();

            dbRalationships = MongoDBDataReaderToDBRelationship(list);
            return dbRalationships;
        }

        private List<DBRelationship> MongoDBDataReaderToDBRelationship(List<DBRelationshipMongoDbSchema> collection)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            HashSet<long> relatedObjectIDs = new HashSet<long>();

            foreach (var item in collection)
            {
                long source = item.Source;// item.Source.Id;
                long target = item.Target;// item.Target.Id;
                long dataSourceID = item.DataSourceID;

                if (!relatedObjectIDs.Contains(source))
                {
                    relatedObjectIDs.Add(source);
                }
                if (!relatedObjectIDs.Contains(target))
                {
                    relatedObjectIDs.Add(target);
                }

                relationships.Add(
                    new DBRelationship()
                    {
                        Id = item.Id,
                        Description = item.Description,
                        TypeURI = item.TypeURI,
                        TimeBegin = DateTime.Parse(item.TimeBegin), //item.TimeBegin,
                        TimeEnd = DateTime.Parse(item.TimeEnd),  //item.TimeEnd,
                        Direction = (RepositoryLinkDirection)item.Direction,
                        Source = new DBObject() { Id = source },
                        Target = new DBObject() { Id = target },
                        DataSourceID = dataSourceID
                    });
            }

            Dictionary<long, DBObject> relatedObjectsPerID = GetObjects(relatedObjectIDs).ToDictionary(o => o.Id);

            foreach (var relationship in relationships)
            {
                relationship.Source = relatedObjectsPerID[relationship.Source.Id];
                relationship.Target = relatedObjectsPerID[relationship.Target.Id];
            }
            return relationships;
        }

        private List<DBRelationship> GetReadableSubset(List<DBRelationship> dbRelationship, AuthorizationParametters authParams)
        {
            long[] dataSourceIDs = dbRelationship.Select(p => p.DataSourceID).ToArray();

            HashSet<long> readableDataSourceIDs = GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
            return dbRelationship.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        }

        #endregion

        #region Publish

        public void Publish(DBAddedConcepts addedConcept, DBModifiedConcepts modifiedConcept, DBResolvedObject[] resolvedObjects, long dataSourceID)
        {
            InsertAddedConceptToRepository(addedConcept, dataSourceID);
            UpdateModifiedConceptsInRepository(modifiedConcept);
            ApplyResolutionChanges(resolvedObjects);
        }

        private void InsertAddedConceptToRepository(DBAddedConcepts addedConcept, long dataSourceID)
        {
            if (addedConcept == null)
                return;

            AddNewObjects(addedConcept.AddedObjectList);
            AddNewProperties(addedConcept.AddedPropertyList, dataSourceID);
            AddNewRelationships(addedConcept.AddedRelationshipList, dataSourceID);
        }

        private void AddNewObjects(List<DBObject> objectsToAdd)
        {
            if (objectsToAdd.Count == 0)
                return;

            IMongoCollection<DBObject> collection = db.GetCollection<DBObject>(ObjectTable.tableName);

            for (int batchIndex = 0; batchIndex <= ((objectsToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, objectsToAdd.Count) - 1;
                //for (int i = startIndex; i <= lastIndex; i++)
                //{
                //DBObject obj = objectsToAdd[i];
                //collection.InsertOne(obj);
                //}

                List<DBObject> list = new List<DBObject>();
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBObject obj = objectsToAdd[i];
                    obj._id = new ObjectId();
                    list.Add(obj);
                }

                collection.InsertMany(list);
                list.Clear();
            }
        }

        private void UpdateModifiedConceptsInRepository(DBModifiedConcepts modifiedConcept)
        {
            if (modifiedConcept == null)
                return;

            if (modifiedConcept.ModifiedPropertyList != null)
            {
                foreach (var modifiedProperty in modifiedConcept.ModifiedPropertyList)
                {
                    EditProperty(modifiedProperty.Id, modifiedProperty.NewValue);
                }
            }
        }

        private void EditProperty(long propertyID, string newValue)
        {
            if (propertyID <= 0)
                throw new ArgumentException("Property ID is invalid.");

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.Where(p => p.Id == propertyID);
            var update = Builders<DBPropertyMongoDbSchema>.Update.Set(p => p.Value, newValue);
            collection.UpdateOne(filter, update);
        }

        private void ApplyResolutionChanges(DBResolvedObject[] resolvedObjects)
        {
            if (resolvedObjects == null || resolvedObjects.Length == 0)
                return;

            foreach (DBResolvedObject rObj in resolvedObjects)
            {
                HashSet<long> reolvedObjIDs = new HashSet<long>(rObj.ResolvedObjectIDs);

                SetResolveMasterFor(reolvedObjIDs, rObj.ResolutionMasterObjectID);
                ChangePropertiesOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, rObj.MatchedProperties);
                ChangeRelationshipsOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID);
            }
        }

        private void SetResolveMasterFor(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            IMongoCollection<DBObject> collection = db.GetCollection<DBObject>(ObjectTable.tableName);

            var builder = Builders<DBObject>.Filter;
            var filter = builder.In(ObjectTable.id, resolvedObjIDs);
            var update = Builders<DBObject>.Update.Set(p => p.ResolvedTo, resolutionMasterObjectID);

            collection.UpdateOne(filter, update);
        }

        private void ChangePropertiesOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, DBMatchedProperty[] matchedProperties)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            StringUtility stringUtil = new StringUtility();
            if (matchedProperties.Length != 0)
            {
                string MatchPropertiesPartOfWhereClause = string.Empty;
                Ontology.Ontology ontology = new Ontology.Ontology();
                string labelPropertyTypeUri = ontology.GetDefaultDisplayNamePropertyTypeUri();

                for (int i = 0; i < matchedProperties.Length; i++)
                {
                    DBMatchedProperty mp = matchedProperties[i];
                    if (!mp.TypeUri.Equals(labelPropertyTypeUri))
                    {
                        string parameterName = string.Format("v{0}", i.ToString());
                        MatchPropertiesPartOfWhereClause += string.Format("({0} = '{1}' AND {2} = :{3}) OR "
                            , MongoDbPropertyTable.typeuri, mp.TypeUri
                            , MongoDbPropertyTable.value, parameterName);
                        //matchPropertyValueParameters[i] = new NpgsqlParameter(parameterName, mp.Value);
                    }
                }

                MatchPropertiesPartOfWhereClause = MatchPropertiesPartOfWhereClause.Substring(0, MatchPropertiesPartOfWhereClause.Length - 4);

                //string query = string.Format("UPDATE DBProperty SET {0} = {1} WHERE {0} IN ({2}) AND NOT({3});"
                //, PropertyTable.objectid, resolutionMasterObjectID
                //, stringUtil.SeperateIDsByComma(resolvedObjIDs),
                //MatchPropertiesPartOfWhereClause);

                var builder1 = Builders<DBPropertyMongoDbSchema>.Filter;
                var filter1 = builder1.In(p => p.ObjectId, resolvedObjIDs);
                var update1 = Builders<DBPropertyMongoDbSchema>.Update.Set(p => p.ObjectId, resolutionMasterObjectID);

                collection.UpdateOne(filter1, update1);

                //command = new NpgsqlCommand(query, connection, transaction);
                //command.Parameters.AddRange(matchPropertyValueParameters);
            }
            else
            {
                //string query = string.Format("UPDATE DBProperty SET objectID = {0} WHERE objectID IN ({1});"
                //, resolutionMasterObjectID, stringUtil.SeperateIDsByComma(resolvedObjIDs));

                var builder2 = Builders<DBPropertyMongoDbSchema>.Filter;
                var filter2 = builder2.In(p => p.ObjectId, resolvedObjIDs);
                var update2 = Builders<DBPropertyMongoDbSchema>.Update.Set(p => p.ObjectId, resolutionMasterObjectID);

                collection.UpdateOne(filter2, update2);
            }
        }

        private void ChangeRelationshipsOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            var builder1 = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter1 = builder1.In(MongoDbRelationshipTable.source, resolvedObjIDs);
            var update1 = Builders<DBRelationshipMongoDbSchema>.Update.Set(MongoDbRelationshipTable.source, resolutionMasterObjectID);
            collection.UpdateOne(filter1, update1);

            var builder2 = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter2 = builder2.In(MongoDbRelationshipTable.target, resolvedObjIDs);
            var update2 = Builders<DBRelationshipMongoDbSchema>.Update.Set(MongoDbRelationshipTable.target, resolutionMasterObjectID);
            collection.UpdateOne(filter2, update2);
        }

        private void AddNewProperties(List<DBProperty> propertiesToAdd, long dataSourceID)
        {
            if (propertiesToAdd.Count == 0)
                return;

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            for (int batchIndex = 0; batchIndex <= ((propertiesToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, propertiesToAdd.Count) - 1;

                //for (int i = startIndex; i <= lastIndex; i++)
                //{
                //    DBProperty prop = propertiesToAdd[i];

                //    DBProperty property = new DBProperty()
                //    {
                //        Id = prop.Id,
                //        ObjectId = prop.Owner.Id,
                //        TypeUri = prop.TypeUri,
                //        Value = prop.Value,
                //        DataSourceID = dataSourceID
                //    };

                //    collection.InsertOne(property);
                //}

                List<DBPropertyMongoDbSchema> list = new List<DBPropertyMongoDbSchema>();
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBProperty prop = propertiesToAdd[i];

                    DBPropertyMongoDbSchema property = new DBPropertyMongoDbSchema()
                    {
                        Id = prop.Id,
                        ObjectId = prop.Owner.Id, //prop.Owner.Id,
                        TypeUri = prop.TypeUri,
                        Value = prop.Value,
                        DataSourceID = dataSourceID
                    };

                    list.Add(property);
                }

                collection.InsertMany(list);
                list.Clear();
            }
        }

        private void AddNewRelationships(List<DBRelationship> relationshipsToAdd, long dataSourceID)
        {
            if (relationshipsToAdd.Count == 0)
                return;

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            for (int batchIndex = 0; batchIndex <= ((relationshipsToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, relationshipsToAdd.Count) - 1;

                //for (int i = startIndex; i <= lastIndex; i++)
                //{
                //    DBRelationship rel = relationshipsToAdd[i];
                //    rel.DataSourceID = dataSourceID;

                //    if (rel.TimeBegin.HasValue || rel.TimeEnd.HasValue)
                //        throw new NotSupportedException("Save Time Begin/End currently not supported");

                //    collection.InsertOne(rel);
                //}

                List<DBRelationshipMongoDbSchema> list = new List<DBRelationshipMongoDbSchema>();
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBRelationship rel = relationshipsToAdd[i];
                    //rel.DataSourceID = dataSourceID;

                    if (rel.TimeBegin.HasValue || rel.TimeEnd.HasValue)
                        throw new NotSupportedException("Save Time Begin/End currently not supported");


                    

                    DBRelationshipMongoDbSchema s = new DBRelationshipMongoDbSchema();
                    s._id = new ObjectId();
                    s.TypeURI = rel.TypeURI;
                    s.TimeBegin = "";
                    s.TimeEnd = "";
                    s.Source = rel.Source.Id;
                    s.Target = rel.Target.Id;
                    s.DataSourceID = rel.DataSourceID;
                    s.Description = rel.Description;
                    s.Direction = (int)rel.Direction;



                    //list.Add(rel);
                    list.Add(s);
                }

                collection.InsertMany(list);
                list.Clear();
            }
        }

        #endregion

        #region ID Assignment

        public long GetLastAsignedGraphId()
        {
            return 1000;
        }

        public long GetLastAsignedMediaId()
        {
            return 1000;
        }

        public long GetLastAsignedObjectId()
        {
            IMongoCollection<DBObject> collection = db.GetCollection<DBObject>(ObjectTable.tableName);
            return (from o in collection.AsQueryable().AsEnumerable()
                    select o).Max().Id;
        }

        public long GetLastAsignedPropertyId()
        {
            IMongoCollection<DBProperty> collection = db.GetCollection<DBProperty>(MongoDbPropertyTable.tableName);
            return (from o in collection.AsQueryable().AsEnumerable()
                    select o).Max().Id;
        }

        public long GetLastAsignedRelationshipId()
        {
            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);
            return (from o in collection.AsQueryable().AsEnumerable()
                    select o).Max().Id;
        }

        public long GetLastAsignedDataSourceId()
        {
            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
            return (from o in collection.AsQueryable().AsEnumerable()
                    select o).Max().Id;
        }

        #endregion

        #region Repository Server Management

        public void Optimize()
        {
            throw new NotImplementedException();
        }

        public void TruncateDatabase()
        {
            IMongoCollection<DBObject> objectCollection = db.GetCollection<DBObject>(ObjectTable.tableName);
            var objectFilter = Builders<DBObject>.Filter.Where(p => true);
            objectCollection.DeleteMany(objectFilter);

            IMongoCollection<DBProperty> propertyCollection = db.GetCollection<DBProperty>(MongoDbPropertyTable.tableName);
            var propertyFilter = Builders<DBProperty>.Filter.Where(p => true);
            propertyCollection.DeleteMany(propertyFilter);

            IMongoCollection<DBRelationship> relationCollection = db.GetCollection<DBRelationship>(MongoDbRelationshipTable.tableName);
            var relationFilter = Builders<DBRelationship>.Filter.Where(p => true);
            relationCollection.DeleteMany(relationFilter);

            IMongoCollection<DBDataSource> datasourceCollection = db.GetCollection<DBDataSource>(DataSource.tableName);
            var datasourceFilter = Builders<DBDataSource>.Filter.Where(p => true);
            datasourceCollection.DeleteMany(datasourceFilter);

            IMongoCollection<DBDataSourceACL> datasourceaciCollection = db.GetCollection<DBDataSourceACL>(DataSourceACI.tableName);
            var datasourceaciFilter = Builders<DBDataSourceACL>.Filter.Where(p => true);
            datasourceaciCollection.DeleteMany(datasourceaciFilter);
        }

        #endregion

        #region Init

        public void Init()
        {
            MongoServer server = client.GetServer();
            MongoDatabase db = server.GetDatabase(RepositorydbDatabase);

            IMongoDatabase db1 = client.GetDatabase(RepositorydbDatabase);

            bool isObjectTableExists = db.GetCollection(ObjectTable.tableName).Exists();
            if (!isObjectTableExists)
            {
                db.CreateCollection(ObjectTable.tableName );

                IMongoCollection<DBObject> objectCollection = db1.GetCollection<DBObject>(ObjectTable.tableName);
                objectCollection.Indexes.CreateOne(new BsonDocument("Id", 1));
                objectCollection.Indexes.CreateOne(new BsonDocument("ResolvedTo", 2));
            }

            bool isPropertyTableExists = db.GetCollection(MongoDbPropertyTable.tableName).Exists();
            if (!isPropertyTableExists)
            {
                db.CreateCollection(MongoDbPropertyTable.tableName);

                IMongoCollection<DBPropertyMongoDbSchema> propertyCollection = db1.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);
                propertyCollection.Indexes.CreateOne(new BsonDocument("Id", 1));
                propertyCollection.Indexes.CreateOne(new BsonDocument("ObjectId", 2));
                propertyCollection.Indexes.CreateOne(new BsonDocument("TypeUri", 3));
                propertyCollection.Indexes.CreateOne(new BsonDocument("Value", 4));
            }

            bool isRelationshipTableExists = db.GetCollection(MongoDbRelationshipTable.tableName).Exists();
            if (!isRelationshipTableExists)
            {
                db.CreateCollection(MongoDbRelationshipTable.tableName);

                IMongoCollection<DBRelationshipMongoDbSchema> relationShipCollection = db1.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);
                relationShipCollection.Indexes.CreateOne(new BsonDocument("Id", 1));
                relationShipCollection.Indexes.CreateOne(new BsonDocument("TypeUri", 2));
                relationShipCollection.Indexes.CreateOne(new BsonDocument("Source", 3));
                relationShipCollection.Indexes.CreateOne(new BsonDocument("Target", 4));
                relationShipCollection.Indexes.CreateOne(new BsonDocument("Direction", 5));
            }

            bool isDataSourceTableExists = db.GetCollection(DataSource.tableName).Exists();
            if (!isDataSourceTableExists)
            {
                db.CreateCollection(DataSource.tableName);
                IMongoCollection<DBDataSourceMongoDbSchema> dataSourceCollection = db1.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
                dataSourceCollection.Indexes.CreateOne(new BsonDocument("Id", 1));
            }

            bool isDataSourceAciTableExists = db.GetCollection(DataSourceACI.tableName).Exists();
            if (!isDataSourceAciTableExists)
                db.CreateCollection(DataSourceACI.tableName);


            var dbObjectQuery = new QueryDocument("Id", 0);
            DBObject dBObject = db.GetCollection<DBObject>(ObjectTable.tableName).FindOne(dbObjectQuery);
            if (dBObject == null)
            {
                DBObject newDbObject = new DBObject() { Id = 0, IsGroup = false, LabelPropertyID = 0, ResolvedTo = null, TypeUri = "" };
                db.GetCollection<DBObject>(ObjectTable.tableName).Insert(newDbObject);
            }

            var dbPropertyQuery = new QueryDocument("Id", 0);
            DBPropertyMongoDbSchema dBProperty = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName).FindOne(dbPropertyQuery);
            if (dBProperty == null)
            {
                DBPropertyMongoDbSchema newProperty = new DBPropertyMongoDbSchema() { Id = 0, ObjectId = 0, TypeUri = "", Value = "", DataSourceID = 0 };
                db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName).Insert(newProperty);
            }

            var dbRelationshipQuery = new QueryDocument("Id", 0);
            DBRelationshipMongoDbSchema dBRelationship = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName).FindOne(dbRelationshipQuery);
            if (dBRelationship == null)
            {
                DBRelationshipMongoDbSchema newRelation = new DBRelationshipMongoDbSchema() { Id = 0, Description = "", Direction = 3, Source = 0, Target = 0, TimeBegin = DateTime.Now.ToString(), TimeEnd = DateTime.Now.ToString(), TypeURI = "" };
                db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName).Insert(newRelation);
            }

            //var dbDatasourceQuery = new QueryDocument("Id", 0);
            //DBDataSourceMongoDbSchema dBDataSource = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName).FindOne(dbDatasourceQuery);
            //if (dBDataSource == null)
            //{
            //    DBDataSourceMongoDbSchema newDatasource = new DBDataSourceMongoDbSchema() { Id = 0, classification = "N", createdBy = "admin", createdTime = DateTime.Now.ToString(), description = "", dsname = "" };
            //    db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName).Insert(newDatasource);
            //}
        }
    

        #endregion

        #region ACL

        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        {
            AddNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
            AddDataSourceAcis(dsId, acl.Permissions);
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            switch (conceptType)
            {
                case ConceptType.Property:
                    {
                        return GetSubsetOfTableByPermissionPropertyTable(MongoDbPropertyTable.tableName, IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Relationship:
                    {
                        return GetSubsetOfTableByPermissionRelationTable(MongoDbRelationshipTable.tableName, IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Object:
                    {
                        return GetSubsetOfObjectConceptByPermission(IDs, groupNames, minimumPermission);
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        public List<DBDataSourceACL> RetrieveDataSourceACLs(long[] DataSourceIDs)
        {
            Dictionary<long, List<ACI>> acisDic = GetACIs(DataSourceIDs.ToList());
            return GetDBDataSourceACLs(DataSourceIDs.ToList(), acisDic);
        }

        public List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            List<long> topAcisList = GetTopAcis(topN);
            Dictionary<long, List<ACI>> acisDic = GetACIs(topAcisList.ToList());
            return GetDBDataSourceACLs(topAcisList.ToList(), acisDic);
        }

        public List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID)
        {
            if (firstID > lastID)
            {
                throw new InvalidOperationException("Wrong Range");
            }
            return GetDataSourcesInformationByIDRange(firstID, lastID);
        }

        public List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids)
        {
            if (!ids.Any())
            {
                throw new InvalidOperationException("id list is empty");
            }
            return GetDataSourcesInformationByIDs(ids);
        }

        private List<DataSourceInfo> GetDataSourcesInformationByIDs(List<long> ids)
        {
            List<DataSourceInfo> dataSourcesInformation;

            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
            var builder = Builders<DBDataSourceMongoDbSchema>.Filter;
            var filter = builder.In(p => p.Id, ids);
            var list = collection.Find(filter).ToList();

            dataSourcesInformation = MongoDBDataReaderToDataSourceInfo(list);
            return dataSourcesInformation;
        }

        private void AddNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        {
            //DBDataSource dbDataSource = new DBDataSource();
            //dbDataSource.Id = dsId;
            //dbDataSource.dsname = name;
            //dbDataSource.description = description;
            //dbDataSource.classification = acl.Classification;
            //dbDataSource.sourcetype = (byte)type;
            //dbDataSource.createdBy = createBy;
            //dbDataSource.createdTime = DateTime.Parse(createdTime);

            //IMongoCollection<DBDataSource> collection = db.GetCollection<DBDataSource>(DataSource.tableName);
            //collection.InsertOne(dbDataSource);
            //----------------------------------

            DBDataSourceMongoDbSchema dbDataSource = new DBDataSourceMongoDbSchema();
            dbDataSource.Id = dsId;
            dbDataSource.dsname = name;
            dbDataSource.description = description;
            dbDataSource.classification = acl.Classification;
            dbDataSource.sourcetype = (byte)type;
            dbDataSource.createdBy = createBy;
            dbDataSource.createdTime = createdTime;

            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
            collection.InsertOne(dbDataSource);
        }

        private void AddDataSourceAcis(long dsId, List<ACI> acis)
        {
            if (acis.Count < 1)
                return;

            IMongoCollection<DBDataSourceACI> collection = db.GetCollection<DBDataSourceACI>(DataSourceACI.tableName);

            for (int i = 0; i < acis.Count; i++)
            {
                ACI aci = acis[i];

                DBDataSourceACI dbDataSource = new DBDataSourceACI();
                dbDataSource.dsid = dsId;
                dbDataSource.groupname = aci.GroupName;
                dbDataSource.permission = (int)aci.AccessLevel;
    
                collection.InsertOne(dbDataSource);
            }
        }

        private Dictionary<long, List<ACI>> GetACIs(List<long> dataSourceIDs)
        {
            if (!dataSourceIDs.Any())
                return new Dictionary<long, List<ACI>>();

            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();

            IMongoCollection<DBDataSourceACI> collection = db.GetCollection<DBDataSourceACI>(DataSourceACI.tableName);
            var filter = Builders<DBDataSourceACI>.Filter.In(DataSourceACI.dsid, dataSourceIDs);
            var t = collection.Find(filter).ToList();

            acisDic = MongoDBDataReaderToDataSourceACI(t);

            return acisDic;
        }

        private List<DBDataSourceACL> GetDBDataSourceACLs(List<long> dataSourceIDs, Dictionary<long, List<ACI>> acisDic)
        {
            if (!dataSourceIDs.Any())
                return new List<DBDataSourceACL>();
            List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();

            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
            var filter = Builders<DBDataSourceMongoDbSchema>.Filter.In(p => p.Id, dataSourceIDs);
            var t = collection.Find(filter).ToList();

            dataSources = MongoDBDataReaderToDataSource(t, acisDic);

            return dataSources;
        }

        private Dictionary<long, List<ACI>> MongoDBDataReaderToDataSourceACI(List<DBDataSourceACI> collection)
        {
            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
            foreach (var item in collection)
            {
                long dsid = item.dsid; 
                string groupname = item.groupname;  
                long permission = item.permission;  
                if (!acisDic.ContainsKey(dsid))
                {
                    List<ACI> aciTempList = new List<ACI>();
                    aciTempList.Add(new ACI()
                    {
                        GroupName = groupname,
                        AccessLevel = (Permission)permission
                    });
                    acisDic.Add(dsid, aciTempList);
                }
                else
                {
                    acisDic[dsid].Add(new ACI()
                    {
                        GroupName = groupname,
                        AccessLevel = (Permission)permission
                    });
                }
            }
            return acisDic;
        }

        private List<DBDataSourceACL> MongoDBDataReaderToDataSource(List<DBDataSourceMongoDbSchema> collection, Dictionary<long, List<ACI>> acisDic)
        {
            List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();
            foreach (var item in collection)
            {
                long id = item.Id; 
                string classification = item.classification;
                dataSources.Add(
                    new DBDataSourceACL()
                    {
                        Id = id,
                        Acl = new ACL()
                        {
                            Classification = classification,
                            Permissions = acisDic[id]
                        }
                    }
                    );
            }
            return dataSources;
        }

        private HashSet<long> GetReadableDataSourceIDsByAuthParams(long[] dataSourceIDs, AuthorizationParametters authParams)
        {
            HashSet<long> readableIds = new HashSet<long>();
            if (!dataSourceIDs.Any())
                return readableIds;

            IMongoCollection<DBDataSourceMongoDbSchema> dataSourceCollection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);
            var builder1 = Builders<DBDataSourceMongoDbSchema>.Filter;
            var filter1 =  builder1.In(p => p.classification, authParams.readableClassifications);
            var list1 = dataSourceCollection.Find(filter1).ToList();

            IMongoCollection<DBDataSourceACI> dataSourceACICollection = db.GetCollection<DBDataSourceACI>(DataSourceACI.tableName);
            var builder2 = Builders<DBDataSourceACI>.Filter;
            var filter2 = builder2.In(p => p.dsid, dataSourceIDs);
            filter2 = filter2 & builder2.Where(p => p.permission >= (byte)Permission.Read);
            filter2 = filter2 & builder2.In(p => p.groupname, authParams.permittedGroupNames);
            var list2 = dataSourceACICollection.Find(filter2).ToList();

            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    if (item1.Id == item2.dsid)
                        readableIds.Add(item1.Id);
                }
            }

            return readableIds;
        }

        private string CreateCommaSeprateFromArray<T>(T[] array)
        {
            string commaSeprateString = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                commaSeprateString += $"'{array[i]}'";
                if (i < array.Length - 1)
                    commaSeprateString += " , ";
            }
            return commaSeprateString;
        }

        private List<long> GetTopAcis(long topN)
        {
            if (topN <= 0)
                return new List<long>();

            List<long> dataSourceIds = new List<long>();

            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);

            var builder = Builders<DBDataSourceMongoDbSchema>.Filter;
            var list = collection.Find(x => true).Limit((int)topN).ToList();

            dataSourceIds = MonoDBDataReaderToDataSourceId(list);

            return dataSourceIds;
        }

        private List<DataSourceInfo> GetDataSourcesInformationByIDRange(long firstID, long lastID)
        {
            List<DataSourceInfo> dataSourcesInformation;

            IMongoCollection<DBDataSourceMongoDbSchema> collection = db.GetCollection<DBDataSourceMongoDbSchema>(DataSource.tableName);

            var builder = Builders<DBDataSourceMongoDbSchema>.Filter;
            var filter = builder.Where(p => p.Id >= firstID && p.Id <= lastID);
            var list = collection.Find(filter).ToList();

            dataSourcesInformation = MongoDBDataReaderToDataSourceInfo(list);

            return dataSourcesInformation;
        }

        private List<long> GetIDsFromResultPropertyTable(List<DBPropertyMongoDbSchema> collection)
        {
            List<long> ids = new List<long>();
            foreach (var item in collection)
            {
                ids.Add(item.Id);
            }
            return ids;
        }

        private List<long> GetIDsFromResultRelationsshipTable(List<DBRelationshipMongoDbSchema> collection)
        {
            List<long> ids = new List<long>();
            foreach (var item in collection)
            {
                ids.Add(item.Id);
            }
            return ids;
        }

        private List<long> MonoDBDataReaderToDataSourceId(List<DBDataSourceMongoDbSchema> collection)
        {
            List<long> dataSourceIds = new List<long>();
            foreach (var item in collection)
            {
                long id = item.Id;
                dataSourceIds.Add(id);
            }
            return dataSourceIds;
        }

        private List<DataSourceInfo> MongoDBDataReaderToDataSourceInfo(List<DBDataSourceMongoDbSchema> collection)
        {
            List<DataSourceInfo> dataSourcesInformation = new List<DataSourceInfo>();
            foreach (var item in collection)
            {
                List<long> ids = new List<long>() { item.Id };
                DataSourceInfo dataSource = new DataSourceInfo()
                {
                    Id = item.Id,
                    Description = item.description,
                    Name = item.dsname,
                    Type = item.sourcetype,
                    CreatedBy = item.createdBy, 
                    CreatedTime = item.createdTime.ToString(), 
                    Acl = new ACL()
                    {
                        Classification = item.classification, 
                        Permissions = GetACIs(ids)[ids.FirstOrDefault()]
                    }
                };
                dataSourcesInformation.Add(dataSource);
            }
            return dataSourcesInformation;
        }

        private long[] GetSubsetOfTableByPermissionPropertyTable(string tableName, long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<long> permittedIds = new List<long>();
            if (!ids.Any())
                return permittedIds.ToArray();
            if (groupNames.Length == 0)
            {
                throw new Exception("groupNames is empty.");
            }

            IMongoCollection<DBPropertyMongoDbSchema> collection = db.GetCollection<DBPropertyMongoDbSchema>(MongoDbPropertyTable.tableName);

            var builder = Builders<DBPropertyMongoDbSchema>.Filter;
            var filter = builder.In("id", ids);  
            filter = filter & builder.In("dsid", CreateGroupNamePart(groupNames, minimumPermission));
            var list = collection.Find(filter).ToList();

            permittedIds = GetIDsFromResultPropertyTable(list);
        
            
            return permittedIds.ToArray();
        }

        private long[] GetSubsetOfTableByPermissionRelationTable(string tableName, long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<long> permittedIds = new List<long>();
            if (!ids.Any())
                return permittedIds.ToArray();
            if (groupNames.Length == 0)
            {
                throw new Exception("groupNames is empty.");
            }

            IMongoCollection<DBRelationshipMongoDbSchema> collection = db.GetCollection<DBRelationshipMongoDbSchema>(MongoDbRelationshipTable.tableName);

            var builder = Builders<DBRelationshipMongoDbSchema>.Filter;
            var filter = builder.In("id", ids);
            filter = filter & builder.In("dsid", CreateGroupNamePart(groupNames, minimumPermission));
            var list = collection.Find(filter).ToList();

            permittedIds = GetIDsFromResultRelationsshipTable(list);

            return permittedIds.ToArray();
        }

        private List<long> CreateGroupNamePart(string[] groupNames, Permission minimumPermission)
        {
            List<long> ids = new List<long>();

            IMongoCollection<DBDataSourceACI> collection = db.GetCollection<DBDataSourceACI>(DataSourceACI.tableName);

            var builder = Builders<DBDataSourceACI>.Filter;

            var filter1 = builder.Where(p => p.permission >= (byte)minimumPermission);
            var list1 = collection.Find(filter1).ToList();

            var filter2 = builder.In(p => p.groupname , groupNames);
            var list2 = collection.Find(filter2).ToList();

            foreach (var item1 in list1)
            {
                foreach (var item2 in list2)
                {
                    if (item1.dsid == item2.dsid)
                        ids.Add(item1.dsid);

                }
            }

            return ids;
        }

        private long[] GetSubsetOfObjectConceptByPermission(long[] ids, string[] groupNames, Permission minimumPermission)
        {
            ProperyManager propertyManager = new ProperyManager();
            List<DBProperty> propertiesOfObjectIDs = propertyManager.GetPropertiesOfObjectsWithoutAuthorizationParameters(ids);
            Dictionary<long, long> PropertyToObjectIdMapping = new Dictionary<long, long>();
            foreach (var currentProperty in propertiesOfObjectIDs)
            {
                PropertyToObjectIdMapping.Add(currentProperty.Id, currentProperty.Owner.Id);
            }
            long[] propertiesInPermission = GetSubsetOfTableByPermissionPropertyTable(MongoDbPropertyTable.tableName, PropertyToObjectIdMapping.Keys.ToArray(), groupNames, minimumPermission);
            HashSet<long> permittedObjectIds = new HashSet<long>();
            foreach (long currentPropertyId in propertiesInPermission)
            {
                long objectId;
                if (PropertyToObjectIdMapping.ContainsKey(currentPropertyId))
                {
                    PropertyToObjectIdMapping.TryGetValue(currentPropertyId, out objectId);
                    permittedObjectIds.Add(objectId);
                }

            }
            return permittedObjectIds.ToArray();
        }

        #endregion

        public void IsAvailable()
        {
            throw new NotImplementedException();
        }

    }
}
