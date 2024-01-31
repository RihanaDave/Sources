using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class TruncateDatabase
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public TruncateDatabase()
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

        public void Truncate()
        {
            StorageClient.TruncateDatabase();
        }


        //---------------------------------------------------------------------
        //NpgsqlConnection connection = null;
        //private static readonly string connectionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //private static readonly int RequestsTimeout = 10800;
        //public void TruncateTables()
        //{
        //    connection = new NpgsqlConnection(connectionString);
        //    try
        //    {
        //        connection.Open();
        //        TruncateTable(ObjectTable.tableName);
        //        TruncateTable(PropertyTable.tableName);
        //        TruncateTable(RelationshipTable.tableName);
        //        TruncateTable(GraphTable.tableName);
        //        TruncateTable(MediaTable.tableName);
        //        TruncateTable(DataSource.tableName);
        //        TruncateTable(DataSourceACI.tableName);


        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}
        //public void TruncateTable(string tableName)
        //{
        //    string query = $"TRUNCATE TABLE {tableName} CASCADE ;";
        //    NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //    command.CommandTimeout = RequestsTimeout;
        //    command.ExecuteNonQuery();
        //}
    }
}
