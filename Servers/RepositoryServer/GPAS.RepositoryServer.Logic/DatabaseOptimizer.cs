using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.SqlClient;

namespace GPAS.RepositoryServer.Logic
{
    public class DatabaseOptimizer
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public DatabaseOptimizer()
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

        public void Optimize()
        {
            StorageClient.Optimize();
        }

        //-------------------------------------------------------------------------------
        //SqlConnection connection = null;
        //string connctionString = ConnectionStringManager.GetRepositoryDBConnectionString();

        //public void Optimize()
        //{
        //    connection = new SqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        ReindexTable(ObjectTable.tableName);
        //        ReindexTable(PropertyTable.tableName);
        //        ReindexTable(RelationshipTable.tableName);
        //        ReindexTable(GraphTable.tableName);
        //        ReindexTable(MediaTable.tableName);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //private void ReindexTable(string tableName)
        //{
        //    string query = string.Format("DBCC DBREINDEX ({0})", tableName);
        //    SqlCommand command = new SqlCommand(query, connection);
        //    command.ExecuteNonQuery();
        //}
    }
}
