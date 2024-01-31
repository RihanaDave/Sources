using GPAS.RepositoryServer.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class IdRetrievationManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public IdRetrievationManager()
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

        public long GetLastAsignedObjectId()
        {
            return StorageClient.GetLastAsignedObjectId();
        }

        public long GetLastAsignedGraphId()
        {
            return StorageClient.GetLastAsignedGraphId();
        }
        public long GetLastAsignedPropertyId()
        {
            return StorageClient.GetLastAsignedPropertyId();
        }

        public long GetLastAsignedRelationshipId()
        {
            return StorageClient.GetLastAsignedRelationshipId();
        }

        public long GetLastAsignedMediaId()
        {
            return StorageClient.GetLastAsignedMediaId();
        }

        public long GetLastAsignedDataSourceId()
        {
            return StorageClient.GetLastAsignedDataSourceId();
        }


        //------------------------------------------------------------------------------------------------
        //private static string connctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //private static readonly int RequestsTimeout = 10800;

        //public static long GetLastAsignedObjectId()
        //{

        //    string query = "select max(ID) from DBObject;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public static long GetLastAsignedPropertyId()
        //{

        //    string query = "select max(ID) from DBProperty;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public static long GetLastAsignedRelationshipId()
        //{
        //    string query = "select max(ID) from DBRelationship;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public static long GetLastAsignedMediaId()
        //{
        //    string query = "select max(ID) from DBMedia;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public static long GetLastAsignedGraphId()
        //{
        //    string query = "select max(ID) from DBGraph;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public static long GetNewDataSourceId()
        //{
        //    string query = "select max(id) from dbdatasource;";
        //    NpgsqlConnection connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        string id = command.ExecuteScalar().ToString();
        //        if (string.IsNullOrEmpty(id))
        //        {
        //            return 0;
        //        }
        //        return long.Parse(id);
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}
    }
}