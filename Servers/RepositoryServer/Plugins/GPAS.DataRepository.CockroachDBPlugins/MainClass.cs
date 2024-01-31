using GPAS.AccessControl;
using GPAS.Logger;
using GPAS.Ontology;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using GPAS.RepositoryServer.Logic;
using GPAS.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataRepository.CockroachDBPlugins
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        private static readonly int RequestsTimeout = 10800;
        private NpgsqlConnection connection = null;
        private NpgsqlTransaction transaction = null;

        #region Objects Retrievation

        public List<DBObject> GetObjects(IEnumerable<long> dbObjectIDs)
        {
            if (!dbObjectIDs.Any())
                return new List<DBObject>();
            List<DBObject> dbObjects = new List<DBObject>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBObject WHERE {0} in ({1})"
                , ObjectTable.id, stringUtil.SeperateIDsByComma(dbObjectIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbObjects = NpgsqlDataReaderToDBObject(reader);
            }
            return dbObjects;
        }

        public List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        {
            List<DBObject> dbObjects = new List<DBObject>();
            string query = string.Format("SELECT * FROM  DBObject WHERE({0} is null) and ({1} BETWEEN {2} AND {3})"
                , ObjectTable.resolvedto, ObjectTable.id, firstID, lastID);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbObjects = NpgsqlDataReaderToDBObject(reader);
            }
            return dbObjects;
        }

        private List<DBObject> NpgsqlDataReaderToDBObject(NpgsqlDataReader reader)
        {
            List<DBObject> dbObjects = new List<DBObject>();
            while (reader.Read())
            {
                long id = long.Parse(reader[ObjectTable.id].ToString());
                long labelPropertyID = long.Parse(reader[ObjectTable.labelPropertyID].ToString());
                string typeUri = reader[ObjectTable.typeuri].ToString();
                bool isGroup = false;
                if (reader[ObjectTable.isgroup].ToString() == "t")
                {
                    isGroup = true;
                }
                long? resolvedTo = (string.IsNullOrEmpty(reader[ObjectTable.resolvedto].ToString())) ? null : new long?(long.Parse(reader[ObjectTable.resolvedto].ToString()));
                dbObjects.Add(
                    new DBObject()
                    {
                        LabelPropertyID = labelPropertyID,
                        Id = id,
                        IsGroup = isGroup,
                        TypeUri = typeUri,
                        ResolvedTo = resolvedTo
                    }
                    );
            }
            return dbObjects;
        }

        #endregion

        #region Properties Retrievation

        public List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams)
        {
            if (dbObject.Id == 0)
                throw new ArgumentNullException(nameof(dbObject), "ObjectId is invalid.");
            List<DBProperty> dbProperties = new List<DBProperty>();
            string query = string.Format("SELECT * FROM DBProperty WHERE({0} = {1})"
                , PropertyTable.objectid, dbObject.Id);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties = NpgsqlDataReaderToDBProperty(reader);
                dbProperties = GetReadableSubset(dbProperties, authParams);
            }
            return dbProperties;
        }

        public List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs)
        {
            if (objectIDs.Length == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");
            List<DBProperty> dbProperties = new List<DBProperty>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ({1})"
                , PropertyTable.objectid, stringUtil.SeperateIDsByComma(objectIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties.AddRange(NpgsqlDataReaderToDBProperty(reader));
            }
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
            string propertiesValue = "";
            foreach (var property in specifiedPropertyTypeUris)
            {
                propertiesValue += "\'" + property.ToString() + "\'" + ",";
            }
            propertiesValue = propertiesValue.Remove(propertiesValue.Length - 1);
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ({1}) and {2} in ({3})"
                , PropertyTable.objectid, stringUtil.SeperateIDsByComma(objectIDs)
                , PropertyTable.typeuri, propertiesValue);
            List<DBProperty> dbProperties;
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties = NpgsqlDataReaderToDBProperty(reader);
                dbProperties = GetReadableSubset(dbProperties, authParams);
            }
            return dbProperties;
        }

        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams)
        {
            if (objectIDs.Count == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            StringUtility stringUtil = new StringUtility();
            string valueParameterName = "specifiedValue";
            string query =
                $"SELECT * FROM {PropertyTable.tableName} WHERE {PropertyTable.objectid} in ({stringUtil.SeperateIDsByComma(objectIDs)})"
                + $" and {PropertyTable.typeuri} = '{propertyTypeUri}'"
                + $" and {PropertyTable.value} = :{valueParameterName}";

            List<DBProperty> dbProperties;
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.AddWithValue(valueParameterName, propertyTypeUri);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties = NpgsqlDataReaderToDBProperty(reader);
                dbProperties = GetReadableSubset(dbProperties, authParams);
            }
            return dbProperties;
        }

        public List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams)
        {
            if (dbPropertyIDs.Count == 0)
                return new List<DBProperty>();
            List<DBProperty> dbProperties = new List<DBProperty>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ( {1})"
                , PropertyTable.id, stringUtil.SeperateIDsByComma(dbPropertyIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties = NpgsqlDataReaderToDBProperty(reader);
                dbProperties = GetReadableSubset(dbProperties, authParams);
            }
            return dbProperties;
        }

        private List<DBProperty> NpgsqlDataReaderToDBProperty(NpgsqlDataReader reader)
        {
            List<DBProperty> dbProperties = new List<DBProperty>();
            HashSet<long> relatedObjectIDs = new HashSet<long>();
            while (reader.Read())
            {
                long objectID = long.Parse(reader[PropertyTable.objectid].ToString());
                if (!relatedObjectIDs.Contains(objectID))
                {
                    relatedObjectIDs.Add(objectID);
                }
                long id = long.Parse(reader[PropertyTable.id].ToString());
                string typeUri = reader[PropertyTable.typeuri].ToString();
                string value = reader[PropertyTable.value].ToString();
                long dataSourceId = long.Parse(reader[PropertyTable.dsid].ToString());
                dbProperties.Add(
                    new DBProperty()
                    {
                        Id = id,
                        Owner = new DBObject() { Id = objectID },
                        TypeUri = typeUri,
                        Value = value,
                        DataSourceID = dataSourceId
                    });
            }
            // بازیابی و مقدار دهی شئ مالک ویژگی‌ها
            ObjectManager objectManager = new ObjectManager();
            Dictionary<long, DBObject> relatedObjectsPerID = objectManager.GetObjects(relatedObjectIDs).ToDictionary(o => o.Id);
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
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ({1})"
                , PropertyTable.objectid, stringUtil.SeperateIDsByComma(objectIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbProperties.AddRange(NpgsqlDataReaderToDBProperty(reader));
            }
            return dbProperties;
        }

        #endregion

        #region Relationships Retrievation

        public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();

            if (!dbRelationshipIDs.Any())
                return relationships;
            StringUtility stringUtil = new StringUtility();
            string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0} in ( {1})"
                , RelationshipTable.id, stringUtil.SeperateIDsByComma(dbRelationshipIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                command.AllResultTypesAreUnknown = true;
                NpgsqlDataReader reader = command.ExecuteReader();
                relationships = NpgsqlDataReaderToDBRelationships(reader);
                relationships = GetReadableSubset(relationships, authParams);
            }
            return relationships;
        }

        public List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();

            if (!dbRelationshipIDs.Any())
                return relationships;
            StringUtility stringUtil = new StringUtility();
            string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0} in ( {1})"
                , RelationshipTable.id, stringUtil.SeperateIDsByComma(dbRelationshipIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                command.AllResultTypesAreUnknown = true;
                NpgsqlDataReader reader = command.ExecuteReader();
                relationships = NpgsqlDataReaderToDBRelationships(reader);
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} "
                , RelationshipTable.typeuri, typeURI
                , RelationshipTable.source, objectID);

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                command.AllResultTypesAreUnknown = true;
                NpgsqlDataReader reader = command.ExecuteReader();
                relationships = NpgsqlDataReaderToDBRelationships(reader);
                relationships = GetReadableSubset(relationships, authParams);
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();
                string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1}"
                , RelationshipTable.source, currentObjectID);

                using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
                    relationships.AddRange(tempRelationships);
                }
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();
                string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1} or {2}={3} "
                , RelationshipTable.source, currentObjectID
                , RelationshipTable.target, currentObjectID);

                using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
                    tempRelationships = GetReadableSubset(tempRelationships, authParams);
                    relationships.AddRange(tempRelationships);
                }
            }
            return relationships;
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            foreach (var currentObjectID in objectIDs)
            {
                List<DBRelationship> tempRelationships = new List<DBRelationship>();
                string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1} or {2}={3} "
                , RelationshipTable.source, currentObjectID
                , RelationshipTable.target, currentObjectID);

                using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    NpgsqlDataReader reader = command.ExecuteReader();
                    tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
                    relationships.AddRange(tempRelationships);
                }
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


            List<DBRelationship> relationships = new List<DBRelationship>();
            string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} "
                , RelationshipTable.typeuri, typeURI
                , RelationshipTable.target, dbObject.Id);

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                command.AllResultTypesAreUnknown = true;
                NpgsqlDataReader reader = command.ExecuteReader();
                relationships = NpgsqlDataReaderToDBRelationships(reader);
                relationships = GetReadableSubset(relationships, authParams);

            }
            return relationships;
        }

        public DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} and {4}={5} and {6}={7}"
                , RelationshipTable.typeuri, typeURI
                , RelationshipTable.source, source
                , RelationshipTable.target, target
                , RelationshipTable.direction, (long)direction);

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
                command.AllResultTypesAreUnknown = true;
                NpgsqlDataReader reader = command.ExecuteReader();
                relationships = NpgsqlDataReaderToDBRelationships(reader);
            }
            return relationships.FirstOrDefault();
        }

        public List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID)
        {
            List<DBRelationship> dbRalationships = new List<DBRelationship>();
            string query = string.Format("SELECT * FROM  DBRelationship WHERE ({0} BETWEEN {1} AND {2})"
                , RelationshipTable.id, firstID, lastID);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbRalationships = NpgsqlDataReaderToDBRelationships(reader);
            }
            return dbRalationships;
        }

        private List<DBRelationship> NpgsqlDataReaderToDBRelationships(NpgsqlDataReader reader)
        {
            List<DBRelationship> relationships = new List<DBRelationship>();
            HashSet<long> relatedObjectIDs = new HashSet<long>();
            while (reader.Read())
            {
                long source = long.Parse(reader[RelationshipTable.source].ToString());
                long target = long.Parse(reader[RelationshipTable.target].ToString());
                long dataSourceID = long.Parse(reader[RelationshipTable.dsid].ToString());

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
                        Id = long.Parse(reader[RelationshipTable.id].ToString()),
                        Description = reader[RelationshipTable.description].ToString(),
                        TypeURI = reader[RelationshipTable.typeuri].ToString(),
                        TimeBegin = GetNullableDateTime(reader, RelationshipTable.timebegin),
                        TimeEnd = GetNullableDateTime(reader, RelationshipTable.timeend),
                        Direction = (RepositoryLinkDirection)long.Parse(reader[RelationshipTable.direction].ToString()),
                        Source = new DBObject() { Id = source },
                        Target = new DBObject() { Id = target },
                        DataSourceID = dataSourceID
                    });
            }
            ObjectManager objectManager = new ObjectManager();
            Dictionary<long, DBObject> relatedObjectsPerID = objectManager.GetObjects(relatedObjectIDs).ToDictionary(o => o.Id);

            foreach (var relationship in relationships)
            {
                relationship.Source = relatedObjectsPerID[relationship.Source.Id];
                relationship.Target = relatedObjectsPerID[relationship.Target.Id];
            }
            return relationships;
        }

        private DateTime? GetNullableDateTime(NpgsqlDataReader reader, string columnName)
        {
            // ãäÈÚ ˜Ï: http://stackoverflow.com/questions/17489960/nullable-datetime-with-sqldatareader

            int columnIndex = reader.GetOrdinal(columnName);
            return reader.IsDBNull(columnIndex) ? (DateTime?)null : (DateTime?)reader.GetDateTime(columnIndex);
        }

        private List<DBRelationship> GetReadableSubset(List<DBRelationship> dbRelationship, AuthorizationParametters authParams)
        {
            long[] dataSourceIDs = dbRelationship.Select(p => p.DataSourceID).ToArray();

            HashSet<long> readableDataSourceIDs = GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
            return dbRelationship.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        }

        #endregion

        #region Publisg

        public void Publish(DBAddedConcepts addedConcepts, DBModifiedConcepts modifiedConcepts, DBResolvedObject[] resolvedObjects, long dataSourceID)
        {
            connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                using (transaction = connection.BeginTransaction())
                {
                    InsertAddedConceptToRepository(addedConcepts, dataSourceID);
                    UpdateModifiedConceptsInRepository(modifiedConcepts);
                    ApplyResolutionChanges(resolvedObjects);
                    transaction.Commit();
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void ApplyResolutionChanges(DBResolvedObject[] resolvedObjects)
        {
            if (resolvedObjects == null || resolvedObjects.Length == 0)
                return;

            ObjectManager objectManager = new ObjectManager();
            ProperyManager propertyManager = new ProperyManager();
            RelationshipManager relationshipManager = new RelationshipManager();

            foreach (DBResolvedObject rObj in resolvedObjects)
            {
                HashSet<long> reolvedObjIDs = new HashSet<long>(rObj.ResolvedObjectIDs);

                SetResolveMasterFor(reolvedObjIDs, rObj.ResolutionMasterObjectID, connection, transaction);
                ChangePropertiesOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, rObj.MatchedProperties, connection, transaction);
                ChangeRelationshipsOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, connection, transaction);
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
                    EditProperty(modifiedProperty.Id, modifiedProperty.NewValue, connection, transaction);
                }
            }

            if (modifiedConcept.DeletedMediaIDList != null)
            {
                MediaManager mediaManager = new MediaManager();
                mediaManager.DeleteMedias(modifiedConcept.DeletedMediaIDList, connection, transaction);
            }
        }

        private void EditProperty(long propertyID, string newValue, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (propertyID <= 0)
                throw new ArgumentException("Property ID is invalid.");

            string parameterName = "newValue";
            string query = string.Format("UPDATE DBProperty SET {0} = :{1} WHERE {2} = {3};"
                , PropertyTable.value, parameterName
                , PropertyTable.id, propertyID);
            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue(parameterName, newValue);
            command.ExecuteNonQuery();
        }

        private void InsertAddedConceptToRepository(DBAddedConcepts addedConcept, long dataSourceID)
        {
            if (addedConcept == null)
                return;

            AddNewObjects(addedConcept.AddedObjectList, connection, transaction);
            AddNewProperties(addedConcept.AddedPropertyList, connection, transaction, dataSourceID);
            AddNewRelationships(addedConcept.AddedRelationshipList, connection, transaction, dataSourceID);
        }

        private void AddNewObjects(List<DBObject> objectsToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (objectsToAdd.Count == 0)
                return;

            for (int batchIndex = 0; batchIndex <= ((objectsToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, objectsToAdd.Count) - 1;

                string query = string.Format("INSERT INTO DBObject ({0},{1},{2},{3},{4}) VALUES "
                    , ObjectTable.id, ObjectTable.labelPropertyID
                    , ObjectTable.typeuri, ObjectTable.isgroup, ObjectTable.resolvedto
                    );
                var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBObject obj = objectsToAdd[i];

                    string labelPropertyIdParameterName = "d" + i.ToString();
                    query += string.Format("({0},:{1},'{2}','{3}',{4}){5}"
                        , obj.Id, labelPropertyIdParameterName, obj.TypeUri, obj.IsGroup
                        , ((obj.ResolvedTo.HasValue) ? obj.ResolvedTo.Value.ToString() : "NULL")
                        , ((i != lastIndex) ? ',' : ';'));
                    parameters[i - startIndex] = new NpgsqlParameter(labelPropertyIdParameterName, obj.LabelPropertyID);

                    // parameters.Add(new NpgsqlParameter(displayNameParameterName, obj.DisplayName));
                }
                NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddRange(parameters.ToArray());
                command.ExecuteNonQuery();
            }
        }

        private void AddNewProperties(List<DBProperty> propertiesToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction, long dataSourceID)
        {
            if (propertiesToAdd.Count == 0)
                return;

            for (int batchIndex = 0; batchIndex <= ((propertiesToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, propertiesToAdd.Count) - 1;
                string query = string.Format("INSERT INTO DBProperty ({0},{1},{2},{3},{4}) VALUES "
                    , PropertyTable.id, PropertyTable.typeuri
                    , PropertyTable.value, PropertyTable.objectid, PropertyTable.dsid);
                var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBProperty prop = propertiesToAdd[i];
                    string valueParameterName = "v" + i.ToString();
                    query += string.Format("({0},'{1}',:{2},{3},{4}){5}"
                        , prop.Id, prop.TypeUri, valueParameterName, prop.Owner.Id, dataSourceID
                        , ((i != lastIndex) ? ',' : ';'));
                    parameters[i - startIndex] = new NpgsqlParameter(valueParameterName, prop.Value);
                }
                NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        private void AddNewRelationships(List<DBRelationship> relationshipsToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction, long dataSourceID)
        {
            if (relationshipsToAdd.Count == 0)
                return;

            for (int batchIndex = 0; batchIndex <= ((relationshipsToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, relationshipsToAdd.Count) - 1;
                string query = string.Format("INSERT INTO DBRelationship ({0},{1},{2},{3},{4},{5},{6},{7},{8}) VALUES "
                    , RelationshipTable.id, RelationshipTable.source, RelationshipTable.target
                    , RelationshipTable.timebegin, RelationshipTable.timeend
                    , RelationshipTable.typeuri, RelationshipTable.description, RelationshipTable.direction, RelationshipTable.dsid);
                var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBRelationship rel = relationshipsToAdd[i];
                    if (rel.TimeBegin.HasValue || rel.TimeEnd.HasValue)
                        throw new NotSupportedException("Save Time Begin/End currently not supported");
                    string descriptionParameterName = "d" + i.ToString();
                    query += string.Format("({0},{1},{2},NULL,NULL,'{3}',:{4},{5},{6}){7}"
                        , rel.Id, rel.Source.Id, rel.Target.Id
                        , rel.TypeURI, descriptionParameterName, (byte)rel.Direction
                        , dataSourceID
                        , ((i != lastIndex) ? ',' : ';'));
                    // Ø¨Ø§ ØªÙˆØ¬Ù‡ Ø¨Ù‡ Ø°Ø®ÛŒØ±Ù‡â€ŒØ³Ø§Ø²ÛŒ Ù…Ø¨ØªÙ†ÛŒ Ø¨Ø± Ø²Ù…Ø§Ù† ÙÛŒÙ„Ø¯Ù‡Ø§ÛŒ Ø²Ù…Ø§Ù† Ø´Ø±ÙˆØ¹ Ùˆ Ø²Ù…Ø§Ù† Ù¾Ø§ÛŒØ§Ù† Ø¯Ø± Ø¨Ø§Ù†Ú© Ø§Ø·Ù„Ø§Ø¹Ø§ØªÛŒ
                    // Ú©Ø¯ Ø²ÛŒØ± Ø®Ø·Ø§ Ù…ÛŒâ€ŒØ¯Ù‡Ø¯ Ùˆ Ú©Ø¯ Ø¨Ø§Ù„Ø§ Ø¬Ø§ÛŒÚ¯Ø²ÛŒÙ† Ø¢Ù† Ø´Ø¯Ù‡ Ø§Ø³ØªØ›
                    // Ù†Ú©ØªÙ‡: Ø¯Ø± ØµÙˆØ±ØªÛŒ Ú©Ù‡ Ø¨Ø®ÙˆØ§Ù‡ÛŒÙ… Ø§Ø² ÙˆØ±ÙˆØ¯ÛŒ Ù…Ø¨ØªÙ†ÛŒ Ø¨Ø± Ù¾Ø§Ø±Ø§Ù…ØªØ± Ø¨Ø±Ø§ÛŒØŒ Ø§Ø³.Ú©ÛŒÙˆ.Ø§Ù„. Ø¨ÛŒØ´ØªØ± Ø§Ø² Û²Û±Û°Û° Ù¾Ø§Ø±Ø§Ù…ØªØ±
                    // Ø±Ø§ Ù‚Ø¨ÙˆÙ„ Ù†Ù…ÛŒâ€ŒÚ©Ù†Ø¯ Ú©Ù‡ Ø¨Ø§ ØªÙˆØ¬Ù‡ Ø¨Ù‡ ÙˆØ¬ÙˆØ¯ Ø³Ù‡ Ù¾Ø§Ø±Ø§Ù…ØªØ± Ø¨Ø±Ø§ÛŒ Ù‡Ø± Ø¯Ø±Ø¬ Ùˆ Ø¯Ø³ØªÙ‡â€ŒÙ‡Ø§ÛŒ 1000 ØªØ§ÛŒÛŒ Ø§Ù…Ú©Ø§Ù† Ø§Ø³ØªÙØ§Ø¯Ù‡ Ø§Ø²
                    // Ø§ÛŒÙ† Ù‚Ø§Ø¨Ù„ÛŒØª Ø¯Ø± Ø­Ø§Ù„ Ø­Ø§Ø¸Ø± Ù…Ù†ØªÙÛŒ Ø§Ø³Øª
                    //
                    //query += string.Format("({0},{1},{2},'{3}',{4},{5},{6},{7}){8}"
                    //    , rel.Id, rel.Source.Id, rel.Target.Id
                    //    , ((rel.TimeBegin.HasValue) ? rel.TimeBegin.Value.ToString(CultureInfo.InvariantCulture) : "NULL")
                    //    , ((rel.TimeEnd.HasValue) ? rel.TimeEnd.Value.ToString(CultureInfo.InvariantCulture) : "NULL")
                    //    , rel.TypeURI, descriptionParameterName, (byte)rel.Direction
                    //    , ((i != lastIndex) ? ',' : ';'));
                    parameters[i - startIndex] = new NpgsqlParameter(descriptionParameterName, rel.Description);
                }
                NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        private void SetResolveMasterFor(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            StringUtility stringUtil = new StringUtility();
            string query = string.Format("UPDATE DBObject SET {0} = {1} WHERE {2} IN ({3});"
                , ObjectTable.resolvedto, resolutionMasterObjectID
                , ObjectTable.id, stringUtil.SeperateIDsByComma(resolvedObjIDs));
            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.ExecuteNonQuery();
        }

        private void ChangePropertiesOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, DBMatchedProperty[] matchedProperties, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            NpgsqlCommand command;
            StringUtility stringUtil = new StringUtility();
            if (matchedProperties.Length != 0)
            {
                string MatchPropertiesPartOfWhereClause = string.Empty;
                NpgsqlParameter[] matchPropertyValueParameters = new NpgsqlParameter[matchedProperties.Length];
                Ontology.Ontology ontology = new Ontology.Ontology();
                string labelPropertyTypeUri = ontology.GetDefaultDisplayNamePropertyTypeUri();

                for (int i = 0; i < matchedProperties.Length; i++)
                {
                    DBMatchedProperty mp = matchedProperties[i];
                    if (!mp.TypeUri.Equals(labelPropertyTypeUri))
                    {
                        string parameterName = string.Format("v{0}", i.ToString());
                        MatchPropertiesPartOfWhereClause += string.Format("({0} = '{1}' AND {2} = :{3}) OR "
                            , PropertyTable.typeuri, mp.TypeUri
                            , PropertyTable.value, parameterName);
                        matchPropertyValueParameters[i] = new NpgsqlParameter(parameterName, mp.Value);
                    }
                }
                MatchPropertiesPartOfWhereClause = MatchPropertiesPartOfWhereClause.Substring(0, MatchPropertiesPartOfWhereClause.Length - 4);

                string query = string.Format("UPDATE DBProperty SET {0} = {1} WHERE {0} IN ({2}) AND NOT({3});"
                    , PropertyTable.objectid, resolutionMasterObjectID
                    , stringUtil.SeperateIDsByComma(resolvedObjIDs),
                    MatchPropertiesPartOfWhereClause);
                command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddRange(matchPropertyValueParameters);
            }
            else
            {
                string query = string.Format("UPDATE DBProperty SET objectID = {0} WHERE objectID IN ({1});"
                       , resolutionMasterObjectID, stringUtil.SeperateIDsByComma(resolvedObjIDs));
                command = new NpgsqlCommand(query, connection, transaction);
            }
            command.ExecuteNonQuery();
        }

        private void ChangeRelationshipsOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            StringUtility stringUtil = new StringUtility();
            string updateSourceQuery = string.Format("UPDATE DBRelationship SET {0} = {1} WHERE {0} IN ({2});"
                , RelationshipTable.source, resolutionMasterObjectID
                , stringUtil.SeperateIDsByComma(resolvedObjIDs));
            NpgsqlCommand updateSourceCommand = new NpgsqlCommand(updateSourceQuery, connection, transaction);
            updateSourceCommand.ExecuteNonQuery();

            string updateTargetQuery = string.Format("UPDATE DBRelationship SET {0} = {1} WHERE {0} IN ({2});"
                , RelationshipTable.target, resolutionMasterObjectID
                , stringUtil.SeperateIDsByComma(resolvedObjIDs));
            NpgsqlCommand updateTargetCommand = new NpgsqlCommand(updateTargetQuery, connection, transaction);
            updateTargetCommand.ExecuteNonQuery();
        }

        #endregion

        #region ID Assignment

        public long GetLastAsignedDataSourceId()
        {
            string query = "select max(id) from dbdatasource;";
            NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                string id = command.ExecuteScalar().ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return 0;
                }
                return long.Parse(id);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

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
            string query = "select max(ID) from DBObject;";
            NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                string id = command.ExecuteScalar().ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return 0;
                }
                return long.Parse(id);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public long GetLastAsignedPropertyId()
        {
            string query = "select max(ID) from DBProperty;";
            NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                string id = command.ExecuteScalar().ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return 0;
                }
                return long.Parse(id);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public long GetLastAsignedRelationshipId()
        {
            string query = "select max(ID) from DBRelationship;";
            NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                string id = command.ExecuteScalar().ToString();
                if (string.IsNullOrEmpty(id))
                {
                    return 0;
                }
                return long.Parse(id);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }


        #endregion

        #region Repository Server Management

        public void Optimize()
        {
            connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                ReindexTable(ObjectTable.tableName);
                ReindexTable(PropertyTable.tableName);
                ReindexTable(RelationshipTable.tableName);
                ReindexTable(GraphTable.tableName);
                ReindexTable(MediaTable.tableName);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void ReindexTable(string tableName)
        {
            string query = string.Format("DBCC DBREINDEX ({0})", tableName);
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

        public void TruncateDatabase()
        {
            connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                TruncateTable(ObjectTable.tableName);
                TruncateTable(PropertyTable.tableName);
                TruncateTable(RelationshipTable.tableName);
                TruncateTable(GraphTable.tableName);
                TruncateTable(MediaTable.tableName);
                TruncateTable(DataSource.tableName);
                TruncateTable(DataSourceACI.tableName);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        private void TruncateTable(string tableName)
        {
            string query = $"TRUNCATE TABLE {tableName} CASCADE ;";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.CommandTimeout = RequestsTimeout;
            command.ExecuteNonQuery();
        }

        #endregion

        #region Init

        private string GetDatabaseQuery()
        {
            return string.Format("CREATE DATABASE IF NOT EXISTS {0};", Database.databaseName);
        }

        private string GetObjectTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} int NOT NULL, {4} STRING(420) NOT NULL, {5} BOOLEAN NOT NULL, {6} int NULL);"
                , Database.databaseName, ObjectTable.tableName, ObjectTable.id, ObjectTable.labelPropertyID, ObjectTable.typeuri, ObjectTable.isgroup, ObjectTable.resolvedto);
        }

        private string GetGraphTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420) NULL, {5} STRING(420) NULL, {6} STRING NOT NULL, {7} STRING NOT NULL, {8} int NOT NULL, {9} int NOT NULL REFERENCES {0}.{10}({11}));"
                , Database.databaseName, GraphTable.tableName, GraphTable.id, GraphTable.title, GraphTable.description, GraphTable.timecreated, GraphTable.graphimage, GraphTable.grapharrangement, GraphTable.nodescount, GraphTable.dsid, DataSource.tableName, DataSource.id);
        }

        private string GetMediaTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420) NOT NULL, {5} int NOT NULL REFERENCES {0}.{6}({7}));"
                , Database.databaseName, MediaTable.tableName, MediaTable.id, MediaTable.description, MediaTable.uri, MediaTable.objectid, ObjectTable.tableName, ObjectTable.id);
        }

        private string GetDataSourceTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420), {5} STRING(420) NOT NULL, {6} INT2 NOT NULL , {7} STRING(420), {8} STRING(420));"
                , Database.databaseName, DataSource.tableName, DataSource.id, DataSource.dsname, DataSource.description, DataSource.classification, DataSource.sourceType, DataSource.createdBy, DataSource.createdTime);
        }

        private string GetDataSourceACITableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL REFERENCES {0}.{3}({4}), {5} STRING(420) NULL, {6} INT2 NOT NULL, PRIMARY KEY({2},{5}));"
                , Database.databaseName, DataSourceACI.tableName, DataSourceACI.dsid, DataSource.tableName, DataSource.id, DataSourceACI.groupname, DataSourceACI.permission);
        }

        private string GetPropertyTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NOT NULL, {4} STRING NOT NULL, {5} int NOT NULL REFERENCES {0}.{6}({7}), {8} int NOT NULL REFERENCES {0}.{9}({10}));"
                , Database.databaseName, PropertyTable.tableName, PropertyTable.id, PropertyTable.typeuri, PropertyTable.value, PropertyTable.objectid, ObjectTable.tableName, ObjectTable.id, PropertyTable.dsid, DataSource.tableName, DataSource.id);
        }

        private string GetRelationshipTableQuery()
        {
            return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} int NOT NULL REFERENCES {0}.{4}({5}), {6} int NOT NULL REFERENCES {0}.{4}({5}), {7} DATE NULL, {8} DATE NULL, {9} STRING(420) NOT NULL, {10} STRING(420) NULL, {11} int NOT NULL, {12} int NOT NULL REFERENCES {0}.{13}({14}));"
                , Database.databaseName, RelationshipTable.tableName, RelationshipTable.id, RelationshipTable.source, ObjectTable.tableName, ObjectTable.id, RelationshipTable.target, RelationshipTable.timebegin, RelationshipTable.timeend, RelationshipTable.typeuri, RelationshipTable.description, RelationshipTable.direction, RelationshipTable.dsid, DataSource.tableName, DataSource.id);
        }

        private string GetMediaTableIndexedQuery()
        {
            return string.Format("CREATE INDEX IF NOT EXISTS index_{1}_{2} ON {0}.{1}({2});"
                , Database.databaseName, MediaTable.tableName, MediaTable.objectid);
        }

        private string GetPropertyTableIndexedQuery()
        {
            return string.Format("CREATE INDEX IF NOT EXISTS index_{1}_{2} ON {0}.{1}({2});"
                , Database.databaseName, PropertyTable.tableName, PropertyTable.objectid);
        }

        private void ExecuteQuery(string query, NpgsqlConnection connection)
        {
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();
        }

        public void Init()
        {
            string repositorydbConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
            string systemDBConnctionString = ConnectionStringManager.GetSystemDBConnectionString();

            NpgsqlConnection repositorydbConnection = new NpgsqlConnection(repositorydbConnctionString);
            NpgsqlConnection systemDBConnection = new NpgsqlConnection(systemDBConnctionString);

            try
            {
                //create Database
                systemDBConnection.Open();
                ExecuteQuery(GetDatabaseQuery(), systemDBConnection);
                //create Tables
                repositorydbConnection.Open();
                ExecuteQuery(GetObjectTableQuery(), repositorydbConnection);
                ExecuteQuery(GetDataSourceTableQuery(), repositorydbConnection);
                ExecuteQuery(GetDataSourceACITableQuery(), repositorydbConnection);
                ExecuteQuery(GetPropertyTableQuery(), repositorydbConnection);
                ExecuteQuery(GetRelationshipTableQuery(), repositorydbConnection);
                ExecuteQuery(GetMediaTableQuery(), repositorydbConnection);
                ExecuteQuery(GetGraphTableQuery(), repositorydbConnection);

                //Create Index
                ExecuteQuery(GetMediaTableIndexedQuery(), repositorydbConnection);
                ExecuteQuery(GetPropertyTableIndexedQuery(), repositorydbConnection);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                throw;
            }
            finally
            {
                if (repositorydbConnection != null)
                    repositorydbConnection.Close();
                if (systemDBConnection != null)
                    systemDBConnection.Close();
            }
        }

        #endregion

        #region ACL

        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        {
            NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
            try
            {
                connection.Open();
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    AddNewDataSource(dsId, name, type, acl, description, createBy, createdTime, connection, transaction);
                    AddDataSourceAcis(dsId, acl.Permissions, connection, transaction);
                    transaction.Commit();
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            switch (conceptType)
            {
                case ConceptType.Media:
                    {
                        return GetSubsetOfTableByPermission(MediaTable.tableName, IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Property:
                    {
                        return GetSubsetOfTableByPermission(PropertyTable.tableName, IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Relationship:
                    {
                        return GetSubsetOfTableByPermission(RelationshipTable.tableName, IDs, groupNames, minimumPermission);
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

        private void AddNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            string nameParameterVariable = "na";
            string descriptionParameterVariable = "ds";
            string ceatedByParameterVariable = "cb";
            string createdTimeParameterVariable = "ct";
            string classificationParameterVariable = "cl";

            string query
                = $"INSERT INTO {DataSource.tableName}"
                + $" ({DataSource.id},{DataSource.dsname},{DataSource.description},{DataSource.classification},{DataSource.sourceType},{DataSource.createdBy},{DataSource.createdTime}) VALUES"
                + $" ({dsId},:{nameParameterVariable},:{descriptionParameterVariable},:{classificationParameterVariable},{(byte)type}, :{ceatedByParameterVariable},:{createdTimeParameterVariable});";

            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue(nameParameterVariable, name); // Parameters are used to avoid strings invalid characters bad effects on query
            command.Parameters.AddWithValue(descriptionParameterVariable, description);
            command.Parameters.AddWithValue(ceatedByParameterVariable, createBy);
            command.Parameters.AddWithValue(createdTimeParameterVariable, createdTime);
            command.Parameters.AddWithValue(classificationParameterVariable, acl.Classification);
            command.ExecuteNonQuery();
        }

        private void AddDataSourceAcis(long dsId, List<ACI> acis, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (acis.Count < 1)
                return;

            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append
                ($"INSERT INTO {DataSourceACI.tableName}"
                + $" ({DataSourceACI.dsid},{DataSourceACI.groupname},{DataSourceACI.permission}) VALUES ");

            var parameters = new List<NpgsqlParameter>(); // Parameters are used to avoid strings invalid characters bad effects on query
            for (int i = 0; i < acis.Count; i++)
            {
                ACI aci = acis[i];

                string groupnameParameterVariable = "g" + i.ToString();
                string permissionParameterVariable = "p" + i.ToString();

                queryBuilder.Append($"({dsId},:{groupnameParameterVariable},{(byte)aci.AccessLevel}),");
                parameters.Add(new NpgsqlParameter(groupnameParameterVariable, aci.GroupName));
            }
            queryBuilder.Remove(queryBuilder.Length - 1, 1);
            queryBuilder.Append(';');

            NpgsqlCommand command = new NpgsqlCommand(queryBuilder.ToString(), connection, transaction);
            command.Parameters.AddRange(parameters.ToArray());
            command.ExecuteNonQuery();
        }

        private long[] GetSubsetOfTableByPermission(string tableName, long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<long> permittedIds = new List<long>();
            if (!ids.Any())
                return permittedIds.ToArray();
            if (groupNames.Length == 0)
            {
                throw new Exception("groupNames is empty.");
            }
            string groupNamesPart = CreateGroupNamePart(groupNames, minimumPermission);
            string groupNamesCommaString = CreateCommaSeprateFromArray(groupNames);
            string idCommaString = CreateCommaSeprateFromArray(ids);
            string query = $"SELECT id FROM {tableName} where id in ({idCommaString}) and dsid in ({groupNamesPart})";
            NpgsqlConnection connection = null;
            try
            {
                using (connection = new NpgsqlConnection(ConnctionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    var reader = command.ExecuteReader();
                    permittedIds = GetIDsFromResult(reader);
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return permittedIds.ToArray();
        }

        private string CreateGroupNamePart(string[] groupNames, Permission minimumPermission)
        {
            string groupNamePart = string.Empty;
            groupNamePart = string.Format("select dsid from {0} WHERE {1} >= {2} and (", DataSourceACI.tableName, DataSourceACI.permission, (byte)minimumPermission);
            groupNamePart += string.Format("dsid in (select dsid from {0} WHERE ", DataSourceACI.tableName);
            foreach (var currentGroup in groupNames)
            {
                groupNamePart += string.Format("{0} = '{1}' or ", DataSourceACI.groupname, currentGroup);
            }
            groupNamePart = groupNamePart.Remove(groupNamePart.Length - 4, 4);
            groupNamePart += "))";
            return groupNamePart;
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

        private List<long> GetIDsFromResult(NpgsqlDataReader reader)
        {
            List<long> ids = new List<long>();
            while (reader.Read())
            {
                ids.Add(long.Parse(reader["id"].ToString()));
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
            long[] propertiesInPermission = GetSubsetOfTableByPermission(PropertyTable.tableName, PropertyToObjectIdMapping.Keys.ToArray(), groupNames, minimumPermission);
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

        private Dictionary<long, List<ACI>> GetACIs(List<long> dataSourceIDs)
        {
            if (!dataSourceIDs.Any())
                return new Dictionary<long, List<ACI>>();
            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM {0} WHERE {1} in ({2})"
                , DataSourceACI.tableName, DataSourceACI.dsid, stringUtil.SeperateIDsByComma(dataSourceIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                acisDic = NpgsqlDataReaderToDataSourceACI(reader);
            }
            return acisDic;
        }

        private List<DBDataSourceACL> GetDBDataSourceACLs(List<long> dataSourceIDs, Dictionary<long, List<ACI>> acisDic)
        {
            if (!dataSourceIDs.Any())
                return new List<DBDataSourceACL>();
            List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM {0} WHERE {1} in ({2})"
                , DataSource.tableName, DataSource.id, stringUtil.SeperateIDsByComma(dataSourceIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dataSources = NpgsqlDataReaderToDataSource(reader, acisDic);
            }
            return dataSources;
        }

        private Dictionary<long, List<ACI>> NpgsqlDataReaderToDataSourceACI(NpgsqlDataReader reader)
        {
            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
            while (reader.Read())
            {
                long dsid = long.Parse(reader[DataSourceACI.dsid].ToString());
                string groupname = reader[DataSourceACI.groupname].ToString();
                long permission = long.Parse(reader[DataSourceACI.permission].ToString());
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

        private List<DBDataSourceACL> NpgsqlDataReaderToDataSource(NpgsqlDataReader reader, Dictionary<long, List<ACI>> acisDic)
        {
            List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();
            while (reader.Read())
            {
                long id = long.Parse(reader[DataSource.id].ToString());
                string classification = reader[DataSource.classification].ToString();
                //long sourcetype = long.Parse(reader[DataSource.sourceType].ToString());
                //string description = reader[DataSource.description].ToString();
                //string datasourceName = reader[DataSource.dsname].ToString();
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

        private List<long> GetTopAcis(long topN)
        {
            if (topN <= 0)
                return new List<long>();
            List<long> dataSourceIds = new List<long>();
            string query = string.Format("SELECT {0} FROM {1} LIMIT({2})"
                , DataSource.id, DataSource.tableName, topN);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dataSourceIds = NpgsqlDataReaderToDataSourceId(reader);
            }
            return dataSourceIds;
            throw new NotImplementedException();
        }

        private List<long> NpgsqlDataReaderToDataSourceId(NpgsqlDataReader reader)
        {
            List<long> dataSourceIds = new List<long>();
            while (reader.Read())
            {
                long id = long.Parse(reader[DataSource.id].ToString());
                dataSourceIds.Add(id);
            }
            return dataSourceIds;
        }

        private List<DataSourceInfo> GetDataSourcesInformationByIDRange(long firstID, long lastID)
        {
            List<DataSourceInfo> dataSourcesInformation;
            string query = $"SELECT * FROM {DataSource.tableName} WHERE ( {DataSource.id} >= {firstID} and {DataSource.id} <= {lastID} )";
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dataSourcesInformation = NpgsqlDataReaderToDataSourceInfo(reader);
            }

            return dataSourcesInformation;
        }

        private List<DataSourceInfo> NpgsqlDataReaderToDataSourceInfo(NpgsqlDataReader reader)
        {
            List<DataSourceInfo> dataSourcesInformation = new List<DataSourceInfo>();
            while (reader.Read())
            {
                List<long> ids = new List<long>() { long.Parse(reader[DataSource.id].ToString()) };
                DataSourceInfo dataSource = new DataSourceInfo()
                {
                    Id = long.Parse(reader[DataSource.id].ToString()),
                    Description = reader[DataSource.description].ToString(),
                    Name = reader[DataSource.dsname].ToString(),
                    Type = long.Parse(reader[DataSource.sourceType].ToString()),
                    CreatedBy = reader[DataSource.createdBy].ToString(),
                    CreatedTime = reader[DataSource.createdTime].ToString(),
                    Acl = new ACL()
                    {
                        Classification = reader[DataSource.classification].ToString(),
                        Permissions = GetACIs(ids)[ids.FirstOrDefault()]
                    }
                };
                dataSourcesInformation.Add(dataSource);
            }
            return dataSourcesInformation;
        }

        private List<DataSourceInfo> GetDataSourcesInformationByIDs(List<long> ids)
        {
            List<DataSourceInfo> dataSourcesInformation;
            StringUtility stringUtil = new StringUtility();
            string query = $"SELECT * FROM {DataSource.tableName} WHERE  {DataSource.id} IN ( {stringUtil.SeperateIDsByComma(ids)} )";
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.CommandTimeout = RequestsTimeout;
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dataSourcesInformation = NpgsqlDataReaderToDataSourceInfo(reader);
            }

            return dataSourcesInformation;
        }

        private HashSet<long> GetReadableDataSourceIDsByAuthParams(long[] dataSourceIDs, AuthorizationParametters authParams)
        {
            HashSet<long> readableIds = new HashSet<long>();
            if (!dataSourceIDs.Any())
                return readableIds;
            string commaSperatedClassification = CreateCommaSeprateFromArray(authParams.readableClassifications.ToArray());
            string commaSepratedDataSourceId = CreateCommaSeprateFromArray(dataSourceIDs);
            string commaSepratedGroupNames = CreateCommaSeprateFromArray(authParams.permittedGroupNames.ToArray());
            string query = $"SELECT {DataSource.id} FROM {DataSource.tableName} where ({DataSource.classification} in ({commaSperatedClassification})) and " +
                $"({DataSource.id} in (select {DataSourceACI.dsid} from {DataSourceACI.tableName} where ({DataSourceACI.dsid} in ({commaSepratedDataSourceId})) and ({DataSourceACI.permission} >= {(byte)Permission.Read}) and ({DataSourceACI.groupname} in ({commaSepratedGroupNames}))))";
            NpgsqlConnection connection = null;
            try
            {
                using (connection = new NpgsqlConnection(ConnctionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    var reader = command.ExecuteReader();
                    readableIds = new HashSet<long>(GetIDsFromResult(reader));
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return readableIds;
        }

        #endregion

        public void IsAvailable()
        {
            throw new NotImplementedException();
        }
   
    }
}
