using GPAS.RepositoryServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Npgsql;
using System.Configuration;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.AccessControl;
using GPAS.Utility;

namespace GPAS.RepositoryServer.Logic
{
    public class MediaManager
    {
        private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();

        /// <remarks>
        /// Parameterized SQL query execution codes templated from:
        /// https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.parameters.aspx
        /// </remarks>
        internal void AddNewMedia(List<DBMedia> mediasToAdd, NpgsqlConnection connection, NpgsqlTransaction transaction, long dataSourceID)
        {
            if (mediasToAdd.Count == 0)
                return;

            for (int batchIndex = 0; batchIndex <= ((mediasToAdd.Count - 1) / 1000); batchIndex++)
            {
                int startIndex = batchIndex * 1000;
                int lastIndex = Math.Min(startIndex + 1000, mediasToAdd.Count) - 1;

                string query = string.Format("INSERT INTO DBMedia ({0},{1},{2},{3},{4}) VALUES "
                    , MediaTable.id, MediaTable.uri, MediaTable.description, MediaTable.objectid, MediaTable.dsid);
                var parameters = new NpgsqlParameter[lastIndex - startIndex + 1]; // Parameters are used to avoid strings invalid characters bad effects on query
                for (int i = startIndex; i <= lastIndex; i++)
                {
                    DBMedia media = mediasToAdd[i];
                    string descriptionParameterName = "d" + i.ToString();
                    query += string.Format("({0},'{1}',:{2},{3},{4}){5}"
                        , media.Id, media.URI, descriptionParameterName, media.ObjectId
                        , dataSourceID
                        , ((i != lastIndex) ? ',' : ';'));
                    parameters[i - startIndex] = new NpgsqlParameter(descriptionParameterName, media.Description);
                }
                NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        internal void ChangeMediasOwner(HashSet<long> resolvedObjIDs, long resolutionMasterObjectID, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (resolvedObjIDs.Count == 0)
                return;

            StringUtility stringUtil = new StringUtility();
            string query = string.Format("UPDATE DBMedia SET {0} = {1} WHERE {0} IN ({2});"
                , MediaTable.objectid, resolutionMasterObjectID
                , stringUtil.SeperateIDsByComma(resolvedObjIDs));
            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.ExecuteNonQuery();

        }

        /// <summary>
        /// این تابع لیستی از فایل ها ی جند رسانه ای منتسب به یک شی را بر می گرداند
        /// </summary>
        /// <param name="objectID">این پارامتر آیدی شی را از کاربر دریافت می کند</param>
        /// <returns>این تابع لیستی از ساختمان داده DBMEDIA را بر می گرداند</returns>
        public List<DBMedia> GetMedia(long objectID, AuthorizationParametters authParams)
        {
            if (objectID == 0)
                throw new ArgumentNullException("ObjectId is invalid.");
            List<DBMedia> uriResult = new List<DBMedia>();
            string query = string.Format("SELECT * FROM DBMedia WHERE({0} = {1})"
                , MediaTable.objectid, objectID);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                uriResult = NpgsqlDataReaderToDBMedia(reader);
                uriResult = GetReadableSubset(uriResult, authParams);
            }
            return uriResult;
        }

        private List<DBMedia> NpgsqlDataReaderToDBMedia(NpgsqlDataReader reader)
        {
            List<DBMedia> dbMedias = new List<DBMedia>();
            while (reader.Read())
            {
                long id = long.Parse(reader[MediaTable.id].ToString());
                string description = reader[MediaTable.description].ToString();
                string uri = reader[MediaTable.uri].ToString();
                long objectID = long.Parse(reader[MediaTable.objectid].ToString());
                long dataSourceID = long.Parse(reader[MediaTable.dsid].ToString());
                dbMedias.Add(
                    new DBMedia()
                    {
                        Id = id,
                        Description = description,
                        URI = uri,
                        ObjectId = objectID,
                        DataSourceID = dataSourceID
                    }
                    );
            }
            return dbMedias;
        }

        public void DeleteMedias(IEnumerable<long> mediaIDs, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            if (!mediaIDs.Any())
                return;
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("DELETE FROM DBMedia WHERE {0} IN ({1});"
                , MediaTable.id, stringUtil.SeperateIDsByComma(mediaIDs));
            NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
            command.ExecuteNonQuery();
        }

        public List<DBMedia> GetMediasOfObjects(long[] objectIDs)
        {
            if (!objectIDs.Any())
                return new List<DBMedia>();
            List<DBMedia> uriResult = new List<DBMedia>();
            StringUtility stringUtil = new StringUtility();
            string query = string.Format("SELECT * FROM DBMedia WHERE {0} in ({1})"
                , MediaTable.objectid, stringUtil.SeperateIDsByComma(objectIDs));
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                uriResult = NpgsqlDataReaderToDBMedia(reader);
            }
            return uriResult;
        }

        public List<DBMedia> GetMediasOfObjects(long[] objectIDs, AuthorizationParametters authParams)
        {
            List<DBMedia> uriResult = GetMediasOfObjects(objectIDs);
            return GetReadableSubset(uriResult, authParams);
        }

        private List<DBMedia> GetReadableSubset(List<DBMedia> dbMedia, AuthorizationParametters authParams)
        {
            long[] dataSourceIDs = dbMedia.Select(p => p.DataSourceID).ToArray();
            var uacMan = new UserAccountControlManager();
            HashSet<long> readableDataSourceIDs = uacMan.GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
            return dbMedia.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        }
    }
}
