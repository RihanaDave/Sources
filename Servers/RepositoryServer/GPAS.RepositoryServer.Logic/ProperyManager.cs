using GPAS.RepositoryServer.Entities;
using System;
using System.Collections.Generic;
using GPAS.RepositoryServer.Entities.Publish;
using Npgsql;
using System.Configuration;
using System.Linq;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.AccessControl;
using GPAS.Ontology;
using GPAS.Utility;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace GPAS.RepositoryServer.Logic
{
    /// <summary>
    /// این کلاس مدیریت و ایجاد و بازیابی ویژگی اشیا در پایگاه را انجام می دهد.
    /// </summary>
    public class ProperyManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public ProperyManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[StoragePluginName];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{StoragePluginName}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }
            // کد ترکیب اسمبلی پلاگین برگرفته از مثال مایکروسافت در آدرس زیر است:
            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        public List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams)
        {
            return StorageClient.GetPropertiesOfObject(dbObject, authParams);
        }

        public List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs)
        {
            return StorageClient.GetPropertiesOfObjectsWithoutAuthorization(objectIDs);
        }

        public List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams)
        {
            return StorageClient.GetPropertiesOfObjects(objectIDs, authParams);
        }

        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authParams)
        {
            return StorageClient.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, specifiedPropertyTypeUris, authParams);
        }

        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams)
        {
            return StorageClient.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyTypeUri, propertyValue, authParams);
        }

        public List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams)
        {
            return StorageClient.GetPropertiesByID(dbPropertyIDs, authParams);
        }


        //------------------------------------------------------------------------------------------------------------
        private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        private static readonly int RequestsTimeout = 10800;

        //public ProperyManager()
        //{
        //}
        ///// <remarks>
        ///// Parameterized SQL query execution codes templated from:
        ///// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        ///// </remarks>
        //internal void AddNewProperties(List<DBProperty> propertiesToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction, long dataSourceID)
        //{
        //    if (propertiesToAdd.Count == 0)
        //        return;

        //    for (int batchIndex = 0; batchIndex <= ((propertiesToAdd.Count - 1) / 1000); batchIndex++)
        //    {
        //        int startIndex = batchIndex * 1000;
        //        int lastIndex = Math.Min(startIndex + 1000, propertiesToAdd.Count) - 1;
        //        string query = string.Format("INSERT INTO DBProperty ({0},{1},{2},{3},{4}) VALUES "
        //            , PropertyTable.id, PropertyTable.typeuri
        //            , PropertyTable.value, PropertyTable.objectid, PropertyTable.dsid);
        //        var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
        //        for (int i = startIndex; i <= lastIndex; i++)
        //        {
        //            DBProperty prop = propertiesToAdd[i];
        //            string valueParameterName = "v" + i.ToString();
        //            query += string.Format("({0},'{1}',:{2},{3},{4}){5}"
        //                , prop.Id, prop.TypeUri, valueParameterName, prop.Owner.Id, dataSourceID
        //                , ((i != lastIndex) ? ',' : ';'));
        //            parameters[i - startIndex] = new NpgsqlParameter(valueParameterName, prop.Value);
        //        }
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //        command.Parameters.AddRange(parameters);
        //        command.ExecuteNonQuery();
        //    }
        //}

        ///// <remarks>
        ///// Parameterized SQL query execution codes templated from:
        ///// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        ///// </remarks>
        //internal void ChangePropertiesOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, DBMatchedProperty[] matchedProperties, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (resolvedObjIDs.Count == 0)
        //        return;

        //    NpgsqlCommand command;
        //    StringUtility stringUtil = new StringUtility();
        //    if (matchedProperties.Length != 0)
        //    {
        //        string MatchPropertiesPartOfWhereClause = string.Empty;
        //        NpgsqlParameter[] matchPropertyValueParameters = new NpgsqlParameter[matchedProperties.Length];
        //        Ontology.Ontology ontology = new Ontology.Ontology();
        //        string labelPropertyTypeUri = ontology.GetDefaultDisplayNamePropertyTypeUri();

        //        for (int i = 0; i < matchedProperties.Length; i++)
        //        {
        //            DBMatchedProperty mp = matchedProperties[i];
        //            if (!mp.TypeUri.Equals(labelPropertyTypeUri))
        //            {
        //                string parameterName = string.Format("v{0}", i.ToString());
        //                MatchPropertiesPartOfWhereClause += string.Format("({0} = '{1}' AND {2} = :{3}) OR "
        //                    , PropertyTable.typeuri, mp.TypeUri
        //                    , PropertyTable.value, parameterName);
        //                matchPropertyValueParameters[i] = new NpgsqlParameter(parameterName, mp.Value);
        //            }
        //        }
        //        MatchPropertiesPartOfWhereClause = MatchPropertiesPartOfWhereClause.Substring(0, MatchPropertiesPartOfWhereClause.Length - 4);

        //        string query = string.Format("UPDATE DBProperty SET {0} = {1} WHERE {0} IN ({2}) AND NOT({3});"
        //            , PropertyTable.objectid, resolutionMasterObjectID
        //            , stringUtil.SeperateIDsByComma(resolvedObjIDs),
        //            MatchPropertiesPartOfWhereClause);
        //        command = new NpgsqlCommand(query, connection, transaction);
        //        command.Parameters.AddRange(matchPropertyValueParameters);
        //    }
        //    else
        //    {
        //        string query = string.Format("UPDATE DBProperty SET objectID = {0} WHERE objectID IN ({1});"
        //               , resolutionMasterObjectID, stringUtil.SeperateIDsByComma(resolvedObjIDs));
        //        command = new NpgsqlCommand(query, connection, transaction);
        //    }
        //    command.ExecuteNonQuery();
        //}

        ///// <remarks>
        ///// Parameterized SQL query execution codes templated from:
        ///// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        ///// </remarks>
        //public void EditProperty(long propertyID, string newValue, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (propertyID <= 0)
        //        throw new ArgumentException("Property ID is invalid.");

        //    string parameterName = "newValue";
        //    string query = string.Format("UPDATE DBProperty SET {0} = :{1} WHERE {2} = {3};"
        //        , PropertyTable.value, parameterName
        //        , PropertyTable.id, propertyID);
        //    NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //    command.Parameters.AddWithValue(parameterName, newValue);
        //    command.ExecuteNonQuery();
        //}

        //public List<DBProperty> GetDBPropertysByID(List<long> dbPropertyIDs, AuthorizationParametters authParams)
        //{
        //    if (dbPropertyIDs.Count == 0)
        //        return new List<DBProperty>();
        //    List<DBProperty> dbProperties = new List<DBProperty>();
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ( {1})"
        //        , PropertyTable.id, stringUtil.SeperateIDsByComma(dbPropertyIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties = NpgsqlDataReaderToDBProperty(reader);
        //        dbProperties = GetReadableSubset(dbProperties, authParams);
        //    }
        //    return dbProperties;
        //}

        //public List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams)
        //{
        //    if (dbObject.Id == 0)
        //        throw new ArgumentNullException(nameof(dbObject), "ObjectId is invalid.");
        //    List<DBProperty> dbProperties = new List<DBProperty>();
        //    string query = string.Format("SELECT * FROM DBProperty WHERE({0} = {1})"
        //        , PropertyTable.objectid, dbObject.Id);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties = NpgsqlDataReaderToDBProperty(reader);
        //        dbProperties = GetReadableSubset(dbProperties, authParams);
        //    }
        //    return dbProperties;
        //}

        //private List<DBProperty> GetReadableSubset(List<DBProperty> dbProperties, AuthorizationParametters authParams)
        //{
        //    long[] dataSourceIDs = dbProperties.Select(p => p.DataSourceID).ToArray();
        //    var uacMan = new UserAccountControlManager();
        //    HashSet<long> readableDataSourceIDs = uacMan.GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
        //    return dbProperties.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        //}
        //public List<long> GetPropertiesOfObject(long ObjectID)
        //{
        //    if (ObjectID == 0)
        //        throw new ArgumentNullException(nameof(ObjectID), "ObjectId is invalid.");
        //    List<long> propertyIDs = new List<long>();
        //    List<DBProperty> dbProperties = new List<DBProperty>();
        //    string query = string.Format("SELECT * FROM DBProperty WHERE({0} = {1})"
        //        , PropertyTable.objectid, ObjectID);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties = NpgsqlDataReaderToDBProperty(reader);
        //        foreach (var dbProperty in dbProperties)
        //            propertyIDs.Add(dbProperty.Id);
        //    }
        //    return propertyIDs;
        //}
        //public List<DBProperty> GetPropertiesOfObjects(List<long> objectsId, List<string> propertiesType, AuthorizationParametters authParams)
        //{
        //    if (objectsId.Count == 0)
        //        throw new ArgumentNullException(nameof(objectsId), "No Object defined to retrieve properties");
        //    string propertiesValue = "";
        //    foreach (var property in propertiesType)
        //    {
        //        propertiesValue += "\'" + property.ToString() + "\'" + ",";
        //    }
        //    propertiesValue = propertiesValue.Remove(propertiesValue.Length - 1);
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ({1}) and {2} in ({3})"
        //        , PropertyTable.objectid, stringUtil.SeperateIDsByComma(objectsId)
        //        , PropertyTable.typeuri, propertiesValue);
        //    List<DBProperty> dbProperties;
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties = NpgsqlDataReaderToDBProperty(reader);
        //        dbProperties = GetReadableSubset(dbProperties, authParams);
        //    }
        //    return dbProperties;
        //}

        //public List<DBProperty> GetPropertiesOfObjects(List<long> objectsId, string propertiesType, string propertiesValue, AuthorizationParametters authParams)
        //{
        //    if (objectsId.Count == 0)
        //        throw new ArgumentNullException(nameof(objectsId), "No Object defined to retrieve properties");

        //    StringUtility stringUtil = new StringUtility();
        //    string valueParameterName = "specifiedValue";
        //    string query =
        //        $"SELECT * FROM {PropertyTable.tableName} WHERE {PropertyTable.objectid} in ({stringUtil.SeperateIDsByComma(objectsId)})"
        //        + $" and {PropertyTable.typeuri} = '{propertiesType}'"
        //        + $" and {PropertyTable.value} = :{valueParameterName}";

        //    List<DBProperty> dbProperties;
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.Parameters.AddWithValue(valueParameterName, propertiesValue);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties = NpgsqlDataReaderToDBProperty(reader);
        //        dbProperties = GetReadableSubset(dbProperties, authParams);
        //    }
        //    return dbProperties;
        //}

        //public List<DBProperty> GetPropertiesOfObjects(long[] objectIDs)
        //{
        //    if (objectIDs.Length == 0)
        //        throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");
        //    List<DBProperty> dbProperties = new List<DBProperty>();
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM DBProperty WHERE {0} in ({1})"
        //        , PropertyTable.objectid, stringUtil.SeperateIDsByComma(objectIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbProperties.AddRange(NpgsqlDataReaderToDBProperty(reader));
        //    }
        //    return dbProperties;
        //}
        //public List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams)
        //{
        //    List<DBProperty> dbProperties = GetPropertiesOfObjects(objectIDs);
        //    return GetReadableSubset(dbProperties, authParams);
        //}
        public List<DBProperty> GetPropertiesOfObjectsWithoutAuthorizationParameters(long[] objectIDs)
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

    }
}
