using GPAS.RepositoryServer.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using GPAS.RepositoryServer.Data.DatabaseTables;
using NpgsqlTypes;
using GPAS.Utility;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace GPAS.RepositoryServer.Logic
{
    /// <summary>
    /// این کلاس مدیریت ایجاد شی در پایگاه داده را انجام می دهد.
    /// </summary>
    public class ObjectManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public ObjectManager()
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

        public List<DBObject> GetObjects(IEnumerable<long> dbObjectIDs)
        {
            return StorageClient.GetObjects(dbObjectIDs);
        }

        public List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        {
            return StorageClient.RetrieveObjectsSequentialByIDRange(firstID, lastID);
        }


        //---------------------------------------------------------------------------------------------------------------
        //private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //private static readonly int RequestsTimeout = 10800;

        ///// <remarks>
        ///// Parameterized SQL query execution codes templated from:
        ///// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        ///// </remarks>
        //internal void AddNewObjects(List<DBObject> objectsToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (objectsToAdd.Count == 0)
        //        return;

        //    for (int batchIndex = 0; batchIndex <= ((objectsToAdd.Count - 1) / 1000); batchIndex++)
        //    {
        //        int startIndex = batchIndex * 1000;
        //        int lastIndex = Math.Min(startIndex + 1000, objectsToAdd.Count) - 1;

        //        string query = string.Format("INSERT INTO DBObject ({0},{1},{2},{3},{4}) VALUES "
        //            , ObjectTable.id, ObjectTable.labelPropertyID
        //            , ObjectTable.typeuri, ObjectTable.isgroup, ObjectTable.resolvedto
        //            );
        //        var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
        //        for (int i = startIndex; i <= lastIndex; i++)
        //        {
        //            DBObject obj = objectsToAdd[i];

        //            string labelPropertyIdParameterName = "d" + i.ToString();
        //            query += string.Format("({0},:{1},'{2}','{3}',{4}){5}"
        //                , obj.Id, labelPropertyIdParameterName, obj.TypeUri, obj.IsGroup
        //                , ((obj.ResolvedTo.HasValue) ? obj.ResolvedTo.Value.ToString() : "NULL")
        //                , ((i != lastIndex) ? ',' : ';'));
        //            parameters[i - startIndex] = new NpgsqlParameter(labelPropertyIdParameterName, obj.LabelPropertyID);

        //            // parameters.Add(new NpgsqlParameter(displayNameParameterName, obj.DisplayName));
        //        }
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //        command.Parameters.AddRange(parameters.ToArray());
        //        command.ExecuteNonQuery();
        //    }
        //}

        //public List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        //{
        //    List<DBObject> dbObjects = new List<DBObject>();
        //    string query = string.Format("SELECT * FROM  DBObject WHERE({0} is null) and ({1} BETWEEN {2} AND {3})"
        //        , ObjectTable.resolvedto, ObjectTable.id, firstID, lastID);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbObjects = NpgsqlDataReaderToDBObject(reader);
        //    }
        //    return dbObjects;
        //}

        //internal void SetResolveMasterFor(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (resolvedObjIDs.Count == 0)
        //        return;

        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("UPDATE DBObject SET {0} = {1} WHERE {2} IN ({3});"
        //        , ObjectTable.resolvedto, resolutionMasterObjectID
        //        , ObjectTable.id, stringUtil.SeperateIDsByComma(resolvedObjIDs));
        //    NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //    command.ExecuteNonQuery();
        //}

        ///// <summary>
        ///// این تابع اشیا در پایگاه داده که با لیست ورودی برابر است را بر می گرداند.
        ///// </summary>
        ///// <param name="dbObjectIDs">   لیستی از آیدی اشیا.   </param>
        ///// <returns>    لیستس از نوع DBObject را برمی گرداند.    </returns>
        //public List<DBObject> GetObjects(IEnumerable<long> dbObjectIDs)
        //{
        //    if (!dbObjectIDs.Any())
        //        return new List<DBObject>();
        //    List<DBObject> dbObjects = new List<DBObject>();
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM DBObject WHERE {0} in ({1})"
        //        , ObjectTable.id, stringUtil.SeperateIDsByComma(dbObjectIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dbObjects = NpgsqlDataReaderToDBObject(reader);
        //    }
        //    return dbObjects;
        //}

        //private List<DBObject> NpgsqlDataReaderToDBObject(NpgsqlDataReader reader)
        //{
        //    List<DBObject> dbObjects = new List<DBObject>();
        //    while (reader.Read())
        //    {
        //        long id = long.Parse(reader[ObjectTable.id].ToString());
        //        long labelPropertyID = long.Parse(reader[ObjectTable.labelPropertyID].ToString());
        //        string typeUri = reader[ObjectTable.typeuri].ToString();
        //        bool isGroup = false;
        //        if (reader[ObjectTable.isgroup].ToString() == "t")
        //        {
        //            isGroup = true;
        //        }
        //        long? resolvedTo = (string.IsNullOrEmpty(reader[ObjectTable.resolvedto].ToString())) ? null : new long?(long.Parse(reader[ObjectTable.resolvedto].ToString()));
        //        dbObjects.Add(
        //            new DBObject()
        //            {
        //                LabelPropertyID = labelPropertyID,
        //                Id = id,
        //                IsGroup = isGroup,
        //                TypeUri = typeUri,
        //                ResolvedTo = resolvedTo
        //            }
        //            );
        //    }
        //    return dbObjects;
        //}

        //public void EditObject(long objId, long newLabelPropertyID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (objId <= 0)
        //        throw new ArgumentException("Object ID is invalid.");

        //    string parameterName = "newLabelPropertyID";
        //    string query = string.Format("UPDATE DBObject SET {0} = :{1} WHERE {2} = {3};"
        //        , ObjectTable.labelPropertyID, parameterName, ObjectTable.id, objId);
        //    NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //    command.Parameters.AddWithValue(parameterName, newLabelPropertyID);
        //    command.ExecuteNonQuery();
        //}
    }
}