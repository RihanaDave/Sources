using GPAS.DataImport.Material.SemiStructured;
using GPAS.JobServer.Logic.Entities.ConfigElements;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using GPAS.JobServer.Data.Context;
using System.Linq;

namespace GPAS.JobServer.Logic.SemiStructuredDataImport
{
    public class DatabaseImporter
    {
        private static List<DatabaseServer> DefinedServers = null;
        private readonly int GetDatabaseSpecQueryTimeout = 120; // Seconds
        private readonly int DataRetrievalQueryTimeout = 10800;

        public static void Init()
        {
            //Create Job Table
            try
            {
                using (var jobContextObject = new JobsDBEntities())
                {
                    jobContextObject.Database.CreateIfNotExists();
                }
            }
            catch (Exception ex)
            {
                var logger = new ExceptionHandler();
                logger.WriteErrorLog(ex);
            }

            DefinedServers = new List<DatabaseServer>();
            JobServerDatabases configDatabases = (JobServerDatabases)ConfigurationManager.GetSection("JobServerDatabases");
            if (configDatabases == null)
            {
                throw new ConfigurationErrorsException("Unable to read \"JobServerDatabases\" configuration section");
            }
            foreach (DatabaseServer server in configDatabases.Servers)
            {
                if (server == null)
                {
                    throw new ConfigurationErrorsException("Unable to read from \"Servers\" section as child of \"JobServerDatabases\" configuration section");
                }
                DefinedServers.Add(server);
            }
        }

        public string[] GetUriOfAliveDatabasesForImport()
        {
            List<string> result = new List<string>();
            foreach (DatabaseServer server in DefinedServers)
            {
                string connectionString = string.Format("Data Source={0};User ID={1};Password={2};Connection Timeout={3};"
                    , server.HostAddress, server.UserName, server.Password, GetDatabaseSpecQueryTimeout.ToString());
                SqlConnection connection = new SqlConnection(connectionString);
                SqlCommand command = new SqlCommand
                    ("SELECT name FROM master.dbo.sysdatabases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');"
                    , connection); // Query source: http://stackoverflow.com/questions/147659/get-list-of-databases-from-sql-server
                command.CommandTimeout = GetDatabaseSpecQueryTimeout;
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string DatabaseName = reader.GetString(0);
                        if (DatabaseName.StartsWith("system_"))
                            continue;
                        result.Add(AttachedDatabaseTableMaterial.GetDatabaseUri(server.Key, DatabaseName));
                    }
                }
                catch(Exception ex)
                {
                    var logger = new ExceptionHandler();
                    logger.WriteErrorLog(ex);
                }
                finally
                {
                    if (connection != null)
                        connection.Close();
                }
            }
            return result.ToArray();
        }

        public DataSet GetTablesAndViewsPreviewForDatabase(string dbUri)
        {
            DataSet result = new DataSet();

            string serverKey = AttachedDatabaseTableMaterial.GetServerKeyFromDatabaseUri(dbUri);
            string databaseName = AttachedDatabaseTableMaterial.GetDatabaseNameFromDatabaseUri(dbUri);
            DatabaseServer server = DefinedServers.Single(s => s.Key.Equals(serverKey));
            string connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};Connection Timeout={4};"
                , server.HostAddress, databaseName, server.UserName, server.Password, DataRetrievalQueryTimeout.ToString());
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(string.Format
                    ("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = '{0}'", databaseName)
                    , connection); // Query source: http://stackoverflow.com/questions/3913620/get-all-table-names-of-a-particular-database-by-sql-query
                command.CommandTimeout = DataRetrievalQueryTimeout;
                SqlDataReader reader = command.ExecuteReader();
                List<string> tables = new List<string>();
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    tables.Add(tableName);
                }
                reader.Close();

                command = new SqlCommand(string.Format
                    ("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_CATALOG = '{0}'", databaseName), connection);
                command.CommandTimeout = DataRetrievalQueryTimeout;
                reader = command.ExecuteReader();
                List<string> views = new List<string>();
                while (reader.Read())
                {
                    string viewName = reader.GetString(0);
                    views.Add(viewName);
                }
                reader.Close();

                List<string> tablesAndViews = tables.Concat(views).ToList();
                if (tablesAndViews.Count == 0)
                    return result;

                foreach (string item in tables.Concat(views))
                {
                    command = new SqlCommand(string.Format("SELECT TOP 10 * FROM [{0}]", item), connection);
                    command.CommandTimeout = DataRetrievalQueryTimeout;
                    reader = command.ExecuteReader();
                    result.Load(reader, LoadOption.PreserveChanges, new string[] { item });
                    reader.Close();
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return result;
        }
    }
}
