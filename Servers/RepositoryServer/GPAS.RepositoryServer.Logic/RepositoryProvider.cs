using GPAS.Logger;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class RepositoryProvider
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public RepositoryProvider()
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

        public void Init()
        {
            StorageClient.Init();
        }

        //--------------------------------------------------------------------------
        //private string GetDatabaseQuery()
        //{
        //    return string.Format("CREATE DATABASE IF NOT EXISTS {0};", Database.databaseName);
        //}

        //private string GetObjectTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} int NOT NULL, {4} STRING(420) NOT NULL, {5} BOOLEAN NOT NULL, {6} int NULL);"
        //        , Database.databaseName, ObjectTable.tableName, ObjectTable.id, ObjectTable.labelPropertyID, ObjectTable.typeuri, ObjectTable.isgroup, ObjectTable.resolvedto);
        //}

        //private string GetGraphTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420) NULL, {5} STRING(420) NULL, {6} STRING NOT NULL, {7} STRING NOT NULL, {8} int NOT NULL, {9} int NOT NULL REFERENCES {0}.{10}({11}));"
        //        , Database.databaseName, GraphTable.tableName, GraphTable.id, GraphTable.title, GraphTable.description, GraphTable.timecreated, GraphTable.graphimage, GraphTable.grapharrangement, GraphTable.nodescount, GraphTable.dsid , DataSource.tableName,DataSource.id);
        //}

        //private string GetMediaTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420) NOT NULL, {5} int NOT NULL REFERENCES {0}.{6}({7}));"
        //        , Database.databaseName, MediaTable.tableName, MediaTable.id, MediaTable.description, MediaTable.uri, MediaTable.objectid, ObjectTable.tableName, ObjectTable.id);
        //}

        //private string GetDataSourceTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NULL, {4} STRING(420), {5} STRING(420) NOT NULL, {6} INT2 NOT NULL , {7} STRING(420), {8} STRING(420));"
        //        , Database.databaseName, DataSource.tableName, DataSource.id, DataSource.dsname, DataSource.description, DataSource.classification, DataSource.sourceType, DataSource.createdBy, DataSource.createdTime);
        //}

        //private string GetDataSourceACITableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL REFERENCES {0}.{3}({4}), {5} STRING(420) NULL, {6} INT2 NOT NULL, PRIMARY KEY({2},{5}));"
        //        , Database.databaseName, DataSourceACI.tableName, DataSourceACI.dsid, DataSource.tableName, DataSource.id, DataSourceACI.groupname, DataSourceACI.permission);
        //}

        //private string GetPropertyTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} STRING(420) NOT NULL, {4} STRING NOT NULL, {5} int NOT NULL REFERENCES {0}.{6}({7}), {8} int NOT NULL REFERENCES {0}.{9}({10}));"
        //        , Database.databaseName, PropertyTable.tableName, PropertyTable.id, PropertyTable.typeuri, PropertyTable.value , PropertyTable.objectid, ObjectTable.tableName, ObjectTable.id, PropertyTable.dsid, DataSource.tableName, DataSource.id);
        //}

        //private string GetRelationshipTableQuery()
        //{
        //    return string.Format("CREATE TABLE IF NOT EXISTS {0}.{1}({2} int NOT NULL PRIMARY KEY, {3} int NOT NULL REFERENCES {0}.{4}({5}), {6} int NOT NULL REFERENCES {0}.{4}({5}), {7} DATE NULL, {8} DATE NULL, {9} STRING(420) NOT NULL, {10} STRING(420) NULL, {11} int NOT NULL, {12} int NOT NULL REFERENCES {0}.{13}({14}));"
        //        , Database.databaseName, RelationshipTable.tableName, RelationshipTable.id, RelationshipTable.source,ObjectTable.tableName,ObjectTable.id, RelationshipTable.target, RelationshipTable.timebegin, RelationshipTable.timeend, RelationshipTable.typeuri, RelationshipTable.description, RelationshipTable.direction, RelationshipTable.dsid, DataSource.tableName, DataSource.id);
        //}

        //private string GetMediaTableIndexedQuery()
        //{
        //    return string.Format("CREATE INDEX IF NOT EXISTS index_{1}_{2} ON {0}.{1}({2});"
        //        , Database.databaseName, MediaTable.tableName, MediaTable.objectid);
        //}

        //private string GetPropertyTableIndexedQuery()
        //{
        //    return string.Format("CREATE INDEX IF NOT EXISTS index_{1}_{2} ON {0}.{1}({2});"
        //        , Database.databaseName, PropertyTable.tableName, PropertyTable.objectid);
        //}

        //private void ExecuteQuery(string query, NpgsqlConnection connection)
        //{
        //    NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //    command.ExecuteNonQuery();
        //}

        //public void Init()
        //{
        //    string repositorydbConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //    string systemDBConnctionString = ConnectionStringManager.GetSystemDBConnectionString();

        //    NpgsqlConnection repositorydbConnection = new NpgsqlConnection(repositorydbConnctionString);
        //    NpgsqlConnection systemDBConnection = new NpgsqlConnection(systemDBConnctionString);

        //    try
        //    {
        //        //create Database
        //        systemDBConnection.Open();
        //        ExecuteQuery(GetDatabaseQuery(), systemDBConnection);
        //        //create Tables
        //        repositorydbConnection.Open();
        //        ExecuteQuery(GetObjectTableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetDataSourceTableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetDataSourceACITableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetPropertyTableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetRelationshipTableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetMediaTableQuery(), repositorydbConnection);
        //        ExecuteQuery(GetGraphTableQuery(), repositorydbConnection);

        //        //Create Index
        //        ExecuteQuery(GetMediaTableIndexedQuery(), repositorydbConnection);
        //        ExecuteQuery(GetPropertyTableIndexedQuery(), repositorydbConnection);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionHandler exceptionHandler = new ExceptionHandler();
        //        exceptionHandler.WriteErrorLog(ex);
        //        throw;
        //    }
        //    finally
        //    {
        //        if (repositorydbConnection != null)
        //            repositorydbConnection.Close();
        //        if (systemDBConnection != null)
        //            systemDBConnection.Close();
        //    }
        //}
    }
}
