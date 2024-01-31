using GPAS.RepositoryServer.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Data.SqlClient;
using Npgsql;
using System.Configuration;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.AccessControl;
using GPAS.Utility;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace GPAS.RepositoryServer.Logic
{
    /// <summary>
    /// Ø§ÛŒÙ† Ú©Ù„Ø§Ø³ Ù…Ø¯ÛŒØ±ÛŒØª Ø§ÛŒØ¬Ø§Ø¯ Ù„ÛŒÙ†Ú© Ø¯Ø± Ù¾Ø§ÛŒÚ¯Ø§Ù‡ Ø¯Ø§Ø¯Ù‡ Ø±Ø§ Ø§Ù†Ø¬Ø§Ù… Ù…ÛŒ Ø¯Ù‡Ø¯.
    /// </summary>
    public class RelationshipManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public RelationshipManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[StoragePluginName];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{StoragePluginName}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }
            
            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams)
        {
            return StorageClient.GetRelationships(dbRelationshipIDs, authParams);
        }
        public List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs)
        {
            return StorageClient.RetrieveRelationships(dbRelationshipIDs);
        }

        public List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams)
        {
            return StorageClient.GetRelationshipsBySourceObject(objectID, typeURI, authParams);
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams)
        {
            return StorageClient.GetRelationshipsBySourceOrTargetObject(objectIDs, authParams);
        }

        public List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        {
            return StorageClient.GetRelationshipsBySourceObjectWithoutAuthParams(objectIDs);
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            return StorageClient.GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(objectIDs);
        }
   
        public List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams)
        {
            return StorageClient.GetSourceLink(dbObject, typeURI, authParams);
        }

        public DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams)
        {
            return StorageClient.GetExistingRelationship(typeURI, source, target, direction, authParams);
        }

        public List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID)
        {
            return StorageClient.RetrieveRelationshipsSequentialByIDRange(firstID, lastID);
        }


        //----------------------------------------------------------------------------------------------------------------
        //private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //private static readonly int RequestsTimeout = 10800;

        ///// <remarks>
        ///// Parameterized SQL query execution codes templated from:
        ///// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        ///// </remarks>
        //internal void AddNewRelationships(List<DBRelationship> relationshipsToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction, long dataSourceID)
        //{
        //    if (relationshipsToAdd.Count == 0)
        //        return;

        //    for (int batchIndex = 0; batchIndex <= ((relationshipsToAdd.Count - 1) / 1000); batchIndex++)
        //    {
        //        int startIndex = batchIndex * 1000;
        //        int lastIndex = Math.Min(startIndex + 1000, relationshipsToAdd.Count) - 1;
        //        string query = string.Format("INSERT INTO DBRelationship ({0},{1},{2},{3},{4},{5},{6},{7},{8}) VALUES "
        //            , RelationshipTable.id, RelationshipTable.source, RelationshipTable.target
        //            , RelationshipTable.timebegin, RelationshipTable.timeend
        //            , RelationshipTable.typeuri, RelationshipTable.description, RelationshipTable.direction, RelationshipTable.dsid);
        //        var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
        //        for (int i = startIndex; i <= lastIndex; i++)
        //        {
        //            DBRelationship rel = relationshipsToAdd[i];
        //            if (rel.TimeBegin.HasValue || rel.TimeEnd.HasValue)
        //                throw new NotSupportedException("Save Time Begin/End currently not supported");
        //            string descriptionParameterName = "d" + i.ToString();
        //            query += string.Format("({0},{1},{2},NULL,NULL,'{3}',:{4},{5},{6}){7}"
        //                , rel.Id, rel.Source.Id, rel.Target.Id
        //                , rel.TypeURI, descriptionParameterName, (byte)rel.Direction
        //                , dataSourceID
        //                , ((i != lastIndex) ? ',' : ';'));
        //            // Ø¨Ø§ ØªÙˆØ¬Ù‡ Ø¨Ù‡ Ø°Ø®ÛŒØ±Ù‡â€ŒØ³Ø§Ø²ÛŒ Ù…Ø¨ØªÙ†ÛŒ Ø¨Ø± Ø²Ù…Ø§Ù† ÙÛŒÙ„Ø¯Ù‡Ø§ÛŒ Ø²Ù…Ø§Ù† Ø´Ø±ÙˆØ¹ Ùˆ Ø²Ù…Ø§Ù† Ù¾Ø§ÛŒØ§Ù† Ø¯Ø± Ø¨Ø§Ù†Ú© Ø§Ø·Ù„Ø§Ø¹Ø§ØªÛŒ
        //            // Ú©Ø¯ Ø²ÛŒØ± Ø®Ø·Ø§ Ù…ÛŒâ€ŒØ¯Ù‡Ø¯ Ùˆ Ú©Ø¯ Ø¨Ø§Ù„Ø§ Ø¬Ø§ÛŒÚ¯Ø²ÛŒÙ† Ø¢Ù† Ø´Ø¯Ù‡ Ø§Ø³ØªØ›
        //            // Ù†Ú©ØªÙ‡: Ø¯Ø± ØµÙˆØ±ØªÛŒ Ú©Ù‡ Ø¨Ø®ÙˆØ§Ù‡ÛŒÙ… Ø§Ø² ÙˆØ±ÙˆØ¯ÛŒ Ù…Ø¨ØªÙ†ÛŒ Ø¨Ø± Ù¾Ø§Ø±Ø§Ù…ØªØ± Ø¨Ø±Ø§ÛŒØŒ Ø§Ø³.Ú©ÛŒÙˆ.Ø§Ù„. Ø¨ÛŒØ´ØªØ± Ø§Ø² Û²Û±Û°Û° Ù¾Ø§Ø±Ø§Ù…ØªØ±
        //            // Ø±Ø§ Ù‚Ø¨ÙˆÙ„ Ù†Ù…ÛŒâ€ŒÚ©Ù†Ø¯ Ú©Ù‡ Ø¨Ø§ ØªÙˆØ¬Ù‡ Ø¨Ù‡ ÙˆØ¬ÙˆØ¯ Ø³Ù‡ Ù¾Ø§Ø±Ø§Ù…ØªØ± Ø¨Ø±Ø§ÛŒ Ù‡Ø± Ø¯Ø±Ø¬ Ùˆ Ø¯Ø³ØªÙ‡â€ŒÙ‡Ø§ÛŒ 1000 ØªØ§ÛŒÛŒ Ø§Ù…Ú©Ø§Ù† Ø§Ø³ØªÙØ§Ø¯Ù‡ Ø§Ø²
        //            // Ø§ÛŒÙ† Ù‚Ø§Ø¨Ù„ÛŒØª Ø¯Ø± Ø­Ø§Ù„ Ø­Ø§Ø¸Ø± Ù…Ù†ØªÙÛŒ Ø§Ø³Øª
        //            //
        //            //query += string.Format("({0},{1},{2},'{3}',{4},{5},{6},{7}){8}"
        //            //    , rel.Id, rel.Source.Id, rel.Target.Id
        //            //    , ((rel.TimeBegin.HasValue) ? rel.TimeBegin.Value.ToString(CultureInfo.InvariantCulture) : "NULL")
        //            //    , ((rel.TimeEnd.HasValue) ? rel.TimeEnd.Value.ToString(CultureInfo.InvariantCulture) : "NULL")
        //            //    , rel.TypeURI, descriptionParameterName, (byte)rel.Direction
        //            //    , ((i != lastIndex) ? ',' : ';'));
        //            parameters[i - startIndex] = new NpgsqlParameter(descriptionParameterName, rel.Description);
        //        }
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //        command.Parameters.AddRange(parameters);
        //        command.ExecuteNonQuery();
        //    }
        //}

        //internal void ChangeRelationshipsOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (resolvedObjIDs.Count == 0)
        //        return;

        //    StringUtility stringUtil = new StringUtility();
        //    string updateSourceQuery = string.Format("UPDATE DBRelationship SET {0} = {1} WHERE {0} IN ({2});"
        //        , RelationshipTable.source, resolutionMasterObjectID
        //        , stringUtil.SeperateIDsByComma(resolvedObjIDs));
        //    NpgsqlCommand updateSourceCommand = new NpgsqlCommand(updateSourceQuery, connection, transaction);
        //    updateSourceCommand.ExecuteNonQuery();

        //    string updateTargetQuery = string.Format("UPDATE DBRelationship SET {0} = {1} WHERE {0} IN ({2});"
        //        , RelationshipTable.target, resolutionMasterObjectID
        //        , stringUtil.SeperateIDsByComma(resolvedObjIDs));
        //    NpgsqlCommand updateTargetCommand = new NpgsqlCommand(updateTargetQuery, connection, transaction);
        //    updateTargetCommand.ExecuteNonQuery();
        //}

        //public List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams)
        //{

        //    if (dbObject == null)
        //        throw new ArgumentNullException(nameof(dbObject), "Object is invalid.");
        //    if (dbObject.Id == 0)
        //        throw new ArgumentNullException(nameof(dbObject.Id), "Object ID is invalid.");
        //    if (typeURI == null)
        //        throw new ArgumentNullException(nameof(typeURI), "TypeURI of relationship is invalid.");


        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} "
        //        , RelationshipTable.typeuri, typeURI
        //        , RelationshipTable.target, dbObject.Id);

        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //        command.AllResultTypesAreUnknown = true;
        //        NpgsqlDataReader reader = command.ExecuteReader();
        //        relationships = NpgsqlDataReaderToDBRelationships(reader);
        //        relationships = GetReadableSubset(relationships, authParams);

        //    }
        //    return relationships;
        //}

        //private DateTime? GetNullableDateTime(NpgsqlDataReader reader, string columnName)
        //{
        //    // ãäÈÚ ˜Ï: http://stackoverflow.com/questions/17489960/nullable-datetime-with-sqldatareader

        //    int columnIndex = reader.GetOrdinal(columnName);
        //    return reader.IsDBNull(columnIndex) ? (DateTime?)null : (DateTime?)reader.GetDateTime(columnIndex);
        //}

        //private List<DBRelationship> NpgsqlDataReaderToDBRelationships(NpgsqlDataReader reader)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    HashSet<long> relatedObjectIDs = new HashSet<long>();
        //    while (reader.Read())
        //    {
        //        long source = long.Parse(reader[RelationshipTable.source].ToString());
        //        long target = long.Parse(reader[RelationshipTable.target].ToString());
        //        long dataSourceID = long.Parse(reader[RelationshipTable.dsid].ToString());

        //        if (!relatedObjectIDs.Contains(source))
        //        {
        //            relatedObjectIDs.Add(source);
        //        }
        //        if (!relatedObjectIDs.Contains(target))
        //        {
        //            relatedObjectIDs.Add(target);
        //        }
        //        relationships.Add(
        //            new DBRelationship()
        //            {
        //                Id = long.Parse(reader[RelationshipTable.id].ToString()),
        //                Description = reader[RelationshipTable.description].ToString(),
        //                TypeURI = reader[RelationshipTable.typeuri].ToString(),
        //                TimeBegin = GetNullableDateTime(reader, RelationshipTable.timebegin),
        //                TimeEnd = GetNullableDateTime(reader, RelationshipTable.timeend),
        //                Direction = (RepositoryLinkDirection)long.Parse(reader[RelationshipTable.direction].ToString()),
        //                Source = new DBObject() { Id = source },
        //                Target = new DBObject() { Id = target },
        //                DataSourceID = dataSourceID
        //            });
        //    }
        //    ObjectManager objectManager = new ObjectManager();
        //    Dictionary<long, DBObject> relatedObjectsPerID = objectManager.GetObjects(relatedObjectIDs).ToDictionary(o => o.Id);

        //    foreach (var relationship in relationships)
        //    {
        //        relationship.Source = relatedObjectsPerID[relationship.Source.Id];
        //        relationship.Target = relatedObjectsPerID[relationship.Target.Id];
        //    }
        //    return relationships;
        //}

        //public List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID)
        //{
        //    List<DBRelationship> dbRalationships = new List<DBRelationship>();
        //    string query = string.Format("SELECT * FROM  DBRelationship WHERE ({0} BETWEEN {1} AND {2})"
        //        , RelationshipTable.id, firstID, lastID);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbRalationships = NpgsqlDataReaderToDBRelationships(reader);
        //    }
        //    return dbRalationships;
        //}

        ///// <summary>
        ///// Ø§ÛŒÙ† ØªØ§Ø¨Ø¹ Ù„ÛŒØ³ØªÛŒ Ø§Ø² Ù„ÛŒÙ†Ú© Ù‡Ø§ Ø±Ø§ Ú©Ù‡ Ø¢ÛŒØ¯ÛŒ Ø¢Ù†Ø§Ù† Ø¯Ø± Ù„ÛŒØ³Øª ÙˆØ±ÙˆØ¯ÛŒ Ù…ÙˆØ¬ÙˆØ¯ Ø§Ø³Øª Ø±Ø§ Ø¨Ø± Ù…ÛŒ Ú¯Ø±Ø¯Ø§Ù†Ø¯. 
        ///// </summary>
        ///// <param name="dbRelationshipIDs">   Ù„ÛŒØ³ØªÛŒ Ø§Ø² Ø¢ÛŒØ¯ÛŒ Ù„ÛŒÙ†Ú© Ù‡Ø§ Ù…ÛŒ Ø¨Ø§Ø´Ø¯.     </param>
        ///// <returns>  Ø±Ø§ Ø¨Ø± Ù…ÛŒÚ¯Ø±Ø¯Ø§Ù†Ø¯. DBRelationship Ù„ÛŒØ³ØªÛŒ Ø§Ø² Ø§Ø² Ù†ÙˆØ¹    </returns>
        //public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();

        //    if (!dbRelationshipIDs.Any())
        //        return relationships;
        //    StringUtility stringUtil = new StringUtility();
        //    string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0} in ( {1})"
        //        , RelationshipTable.id, stringUtil.SeperateIDsByComma(dbRelationshipIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //        command.AllResultTypesAreUnknown = true;
        //        NpgsqlDataReader reader = command.ExecuteReader();
        //        relationships = NpgsqlDataReaderToDBRelationships(reader);
        //        relationships = GetReadableSubset(relationships, authParams);
        //    }
        //    return relationships;
        //}

        //public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();

        //    if (!dbRelationshipIDs.Any())
        //        return relationships;
        //    StringUtility stringUtil = new StringUtility();
        //    string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0} in ( {1})"
        //        , RelationshipTable.id, stringUtil.SeperateIDsByComma(dbRelationshipIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //        command.AllResultTypesAreUnknown = true;
        //        NpgsqlDataReader reader = command.ExecuteReader();
        //        relationships = NpgsqlDataReaderToDBRelationships(reader);
        //    }
        //    return relationships;
        //}

        ///// <summary>
        ///// Ù„ÛŒÙ†Ú© Ù…ÙˆØ¬ÙˆØ¯ Ø¨Ø§ Ø´Ø±Ø§ÛŒØ· Ø¯Ø±ÛŒØ§ÙØªÛŒ Ø±Ø§ Ø¯Ø± ØµÙˆØ±Øª Ù…ÙˆØ¬ÙˆØ¯ Ø¨Ø±Ù…ÛŒÚ¯Ø±Ø¯Ø§Ù†Ø¯.
        ///// </summary>
        ///// <param name="typeURI"> Ù†ÙˆØ¹ Ø±Ø§Ø¨Ø·Ù‡ Ø±Ø§ Ù…Ø´Ø®Øµ Ù…ÛŒÚ©Ù†Ø¯. </param>
        ///// <param name="source"> Ø´ÛŒ Ù…Ø¨Ø¯Ø§ Ø±Ø§ ØªØ¹ÛŒÛŒÙ† Ù…ÛŒÚ©Ù†Ø¯. </param>
        ///// <param name="target">  Ø´ÛŒ Ù…Ù‚ØµØ¯ Ø±Ø§ ØªØ¹ÛŒÛŒÙ† Ù…ÛŒÚ©Ù†Ø¯. </param>
        ///// <param name="direction">  Ø¬Ù‡Øª Ø±Ø§Ø¨Ø·Ù‡ Ø±Ø§ ØªØ¹ÛŒÛŒÙ† Ù…ÛŒÚ©Ù†Ø¯.  </param>
        ///// <returns>   Ø¯Ø± ØµÙˆØ±Øª ÙˆØ¬ÙˆØ¯ Ø±Ø§Ø¨Ø·Ù‡ Ù‡Ù…Ø§Ù† Ù„ÛŒÙ†Ú© Ø±Ø§ Ø¨Ø±Ù…ÛŒ Ú¯Ø±Ø¯Ø§Ù†Ø¯ Ø¯Ø± ØºÛŒØ± ÛŒÙ† ØµÙˆØ±Øª null Ø¨Ø±Ù…ÛŒ Ú¯Ø±Ø¯Ø§Ù†Ø¯.    </returns>

        //public DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction)
        //{

        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} and {4}={5} and {6}={7}"
        //        , RelationshipTable.typeuri, typeURI
        //        , RelationshipTable.source, source
        //        , RelationshipTable.target, target
        //        , RelationshipTable.direction, (long)direction);

        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //        command.AllResultTypesAreUnknown = true;
        //        NpgsqlDataReader reader = command.ExecuteReader();
        //        relationships = NpgsqlDataReaderToDBRelationships(reader);
        //    }
        //    return relationships.FirstOrDefault();
        //}

        ///// <summary>
        ///// Ø§ÛŒÙ† ØªØ§Ø¨Ø¹ Ù„ÛŒÙ†Ú© Ù‡Ø§ÛŒÛŒ Ú©Ù‡ Ù…Ø¨Ø¯Ø§ Ø¢Ù† Ø¨Ø§ Ø´ÛŒ Ù…ÙˆØ±Ø¯ Ù†Ø¸Ø± Ø¨Ø±Ø§Ø¨Ø± Ø§Ø³Øª Ø±Ø§ Ø¨Ø±Ù…ÛŒ Ú¯Ø±Ø¯Ø§Ù†Ø¯
        ///// </summary>
        ///// <param name="objectID">    Ø¢ÛŒ Ø¯ÛŒ Ø´ÛŒ Ø±Ø§ ØªØ¹ÛŒÛŒÙ† Ù…ÛŒ Ú©Ù†Ø¯.     </param>
        ///// <param name="typeURI">      Ù†ÙˆØ¹ Ù„ÛŒÙ†Ú© Ø±Ø§ Ù…Ø´Ø®Øµ Ù…ÛŒ Ú©Ù†Ø¯.    </param>
        ///// <returns>      Ù„ÛŒÙ†Ú© Ø§Ø² Ù†ÙˆØ¹ DBRelationship Ø±Ø§ Ø¨Ø±Ù…ÛŒ Ú¯Ø±Ø¯Ø§Ù†Ø¯.     </returns>
        //public List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}='{1}' and {2}={3} "
        //        , RelationshipTable.typeuri, typeURI
        //        , RelationshipTable.source, objectID);

        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //        command.AllResultTypesAreUnknown = true;
        //        NpgsqlDataReader reader = command.ExecuteReader();
        //        relationships = NpgsqlDataReaderToDBRelationships(reader);
        //        relationships = GetReadableSubset(relationships, authParams);
        //    }
        //    return relationships;
        //}

        //public List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    foreach (var currentObjectID in objectIDs)
        //    {
        //        List<DBRelationship> tempRelationships = new List<DBRelationship>();
        //        string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1} or {2}={3} "
        //        , RelationshipTable.source, currentObjectID
        //        , RelationshipTable.target, currentObjectID);

        //        using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //        {
        //            conn.Open();
        //            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //            command.CommandTimeout = RequestsTimeout;
        //            command.AllResultTypesAreUnknown = true;
        //            NpgsqlDataReader reader = command.ExecuteReader();
        //            tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
        //            tempRelationships = GetReadableSubset(tempRelationships, authParams);
        //            relationships.AddRange(tempRelationships);
        //        }
        //    }
        //    return relationships;
        //}

        //public List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    foreach (var currentObjectID in objectIDs)
        //    {
        //        List<DBRelationship> tempRelationships = new List<DBRelationship>();
        //        string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1}"
        //        , RelationshipTable.source, currentObjectID);

        //        using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //        {
        //            conn.Open();
        //            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //            command.CommandTimeout = RequestsTimeout;
        //            command.AllResultTypesAreUnknown = true;
        //            NpgsqlDataReader reader = command.ExecuteReader();
        //            tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
        //            relationships.AddRange(tempRelationships);
        //        }
        //    }
        //    return relationships;
        //}

        //public List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    foreach (var currentObjectID in objectIDs)
        //    {
        //        List<DBRelationship> tempRelationships = new List<DBRelationship>();
        //        string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1} or {2}={3} "
        //        , RelationshipTable.source, currentObjectID
        //        , RelationshipTable.target, currentObjectID);

        //        using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //        {
        //            conn.Open();
        //            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //            command.CommandTimeout = RequestsTimeout;
        //            command.AllResultTypesAreUnknown = true;
        //            NpgsqlDataReader reader = command.ExecuteReader();
        //            tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
        //            relationships.AddRange(tempRelationships);
        //        }
        //    }
        //    return relationships;
        //}
        //public List<DBRelationship> RetrieveRelationshipsBySourceOrTargetObject(List<long> objectIDs)
        //{
        //    List<DBRelationship> relationships = new List<DBRelationship>();
        //    foreach (var currentObjectID in objectIDs)
        //    {
        //        List<DBRelationship> tempRelationships = new List<DBRelationship>();
        //        string queryString = string.Format("SELECT * FROM DBRelationship WHERE {0}={1} or {2}={3} "
        //        , RelationshipTable.source, currentObjectID
        //        , RelationshipTable.target, currentObjectID);

        //        using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //        {
        //            conn.Open();
        //            NpgsqlCommand command = new NpgsqlCommand(queryString, conn);
        //            command.AllResultTypesAreUnknown = true;
        //            NpgsqlDataReader reader = command.ExecuteReader();
        //            tempRelationships = NpgsqlDataReaderToDBRelationships(reader);
        //            relationships.AddRange(tempRelationships);
        //        }
        //    }
        //    return relationships;
        //}
        //private List<DBRelationship> GetReadableSubset(List<DBRelationship> dbRelationship, AuthorizationParametters authParams)
        //{
        //    long[] dataSourceIDs = dbRelationship.Select(p => p.DataSourceID).ToArray();
        //    var uacMan = new UserAccountControlManager();
        //    HashSet<long> readableDataSourceIDs = uacMan.GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
        //    return dbRelationship.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        //}


    }
}
