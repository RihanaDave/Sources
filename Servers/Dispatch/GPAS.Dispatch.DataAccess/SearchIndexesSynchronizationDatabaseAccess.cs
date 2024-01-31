using GPAS.Dispatch.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.DataAccess
{
    public class SearchIndexesSynchronizationDatabaseAccess
    {
        private static string UserAccountDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static string UserAccountDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];

        private static string SearchIndexesSynchronizationDataBaseName = "SearchIndexesSynchronizationDB";

        public void CreateSearchServerUnsyncDataSourceTable()
        {
            ExecuteQuery(CreationSearchServerUnsyncDataSourceTableQuery);
        }

        private static string HorizonServerUnsyncObjectsTableName = "HorizonServerUnsyncObjects";

        private static string TriggerAfterInserOnTableStatistics = "trgAfterInserOnTableStatistics";
        private static string TriggerAfterDeleteOnTableStatistics = "trgAfterDeleteOnTableStatistics";

        
        private static string HorizonServerUnsyncRelationshipsTableName = "HorizonServerUnsyncRelationships";

        
        private static string StatisticsTableName = "TableStatistics";



        private static string SearchServerUnsyncObjectsTableName = "SearchServerUnsyncObjects";

       

        private static string SearchServerUnsyncDataSourceTableName = "SearchServerUnsyncDataSources";


        public void CreateTriggers()
        {
            //insert trigger
            ExecuteQuery(GetCreationAfterInsertTriggerQuery(TriggerAfterInserOnTableStatistics + "Into" + SearchServerUnsyncObjectsTableName, SearchServerUnsyncObjectsTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterInsertTriggerQuery(TriggerAfterInserOnTableStatistics + "Into" + SearchServerUnsyncDataSourceTableName, SearchServerUnsyncDataSourceTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterInsertTriggerQuery(TriggerAfterInserOnTableStatistics + "Into" + HorizonServerUnsyncObjectsTableName, HorizonServerUnsyncObjectsTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterInsertTriggerQuery(TriggerAfterInserOnTableStatistics + "Into" + HorizonServerUnsyncRelationshipsTableName, HorizonServerUnsyncRelationshipsTableName, StatisticsTableName));

            //delete trigger
            ExecuteQuery(GetCreationAfterDeleteTriggerQuery(TriggerAfterDeleteOnTableStatistics + "Into" + SearchServerUnsyncObjectsTableName, SearchServerUnsyncObjectsTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterDeleteTriggerQuery(TriggerAfterDeleteOnTableStatistics + "Into" + SearchServerUnsyncDataSourceTableName, SearchServerUnsyncDataSourceTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterDeleteTriggerQuery(TriggerAfterDeleteOnTableStatistics + "Into" + HorizonServerUnsyncObjectsTableName, HorizonServerUnsyncObjectsTableName, StatisticsTableName));
            ExecuteQuery(GetCreationAfterDeleteTriggerQuery(TriggerAfterDeleteOnTableStatistics + "Into" + HorizonServerUnsyncRelationshipsTableName, HorizonServerUnsyncRelationshipsTableName, StatisticsTableName));
        }

        
        private string GetCreationAfterDeleteTriggerQuery(string triggerName, string triggerOnTableName, string triggerIntoTableName)
        {
            return string.Format(" if not exists(SELECT * from sys.objects WHERE [type] = 'TR' AND [name] ='{0}' )  EXEC ( 'CREATE TRIGGER {0} ON {1} FOR DELETE AS declare @count int; set @count= (select count(*) from deleted); update {2} set NumberOfRow=NumberOfRow-@count where TableName=''{1}'' ');"
                 , triggerName, triggerOnTableName, triggerIntoTableName
                 );
        }

        private string GetCreationAfterInsertTriggerQuery(string triggerName, string triggerOnTableName, string triggerIntoTableName)
        {
            return string.Format(" if not exists(SELECT * from sys.objects WHERE [type] = 'TR' AND [name] ='{0}' )  EXEC ( 'CREATE TRIGGER {0} ON {1} FOR INSERT AS declare @count int; set @count= (select count(*) from inserted); update {2} set NumberOfRow=NumberOfRow+@count where TableName=''{1}'' ');"
                 , triggerName, triggerOnTableName, triggerIntoTableName
                 );
        }



        private string CreationDatabaseQuery
        {
            get
            {
                return string.Format(" if not exists(select * from sys.databases where name = \'{0}\') create database {0}", SearchIndexesSynchronizationDataBaseName);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private string GetConnectionString()
        {
            return string.Format("Data Source=localhost\\SqlExpress; Initial catalog={0};User id={1}; Password={2};"
                                     , SearchIndexesSynchronizationDataBaseName, UserAccountDatabaseUserName, UserAccountDatabasePassword);
        }

        private string CreationHorizonServerUnsyncObjectsTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (ID int PRIMARY KEY,LastTryTime datetime);", HorizonServerUnsyncObjectsTableName);
            }
        }

        private string CreationHorizonServerUnsyncRelationshipsTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (ID int PRIMARY KEY,LastTryTime datetime);", HorizonServerUnsyncRelationshipsTableName);
            }
        }

        private string CreationSearchServerUnsyncObjectsTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (ID int PRIMARY KEY,LastTryTime datetime);", SearchServerUnsyncObjectsTableName);
            }
        }
        private string CreationSearchServerUnsyncDataSourceTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (ID int PRIMARY KEY,LastTryTime datetime);", SearchServerUnsyncDataSourceTableName);
            }
        }
        private string CreationStatisticsTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (TableName  NVARCHAR(250) PRIMARY KEY,NumberOfRow int);"
                    , StatisticsTableName);
            }
        }

        private string InsertionTableStatisticsQuery(string tableName, int numberOfRow)
        {
            return string.Format("If Not exists(SELECT * from {0} where TableName='{1}') Begin insert into {0} (TableName, NumberOfRow) values ('{1}', {2} ) End"
                , StatisticsTableName, tableName, numberOfRow);
        }

        public void DeleteTableQuery(string tableName)
        {
            string query = $"DELETE FROM  {tableName}  WHERE ID>0";
            ExecuteQuery(query);
        }

        private void DeleteAllSearchServerUnsyncObjects()
        {
            DeleteTableQuery(SearchServerUnsyncObjectsTableName);
        }

        private void DeleteAllSearchServerUnsyncDataSource()
        {
            DeleteTableQuery(SearchServerUnsyncDataSourceTableName);
        }

        private void DeleteAllHorizonServerUnsyncRelationships()
        {
            DeleteTableQuery(HorizonServerUnsyncObjectsTableName);
        }

        private void DeleteAllHorizonServerUnsyncObjects()
        {
            DeleteTableQuery(HorizonServerUnsyncRelationshipsTableName);
        }

        public void DeleteHorizonServerUnsyncConcepts()
        {
            DeleteAllHorizonServerUnsyncObjects();
            DeleteAllHorizonServerUnsyncRelationships();
        }

        public void DeleteSearchServerUnsyncConcepts()
        {
            DeleteAllSearchServerUnsyncObjects();
            DeleteAllSearchServerUnsyncDataSource();
        }

        public void CreateDataBase()
        {
            using (SqlConnection connection = new SqlConnection(
               string.Format("server=(local)\\SQLEXPRESS;Initial Catalog= master;User id={0}; Password={1};",
               UserAccountDatabaseUserName, UserAccountDatabasePassword))
               )
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(CreationDatabaseQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CreateStatisticTable()
        {
            ExecuteQuery(CreationStatisticsTableQuery);
        }

        public void CreateHorizonServerUnsyncObjectsTable()
        {
            ExecuteQuery(CreationHorizonServerUnsyncObjectsTableQuery);
        }
        public void InitStatisticTable()
        {
            ExecuteQuery(InsertionTableStatisticsQuery(SearchServerUnsyncObjectsTableName, 0));
            ExecuteQuery(InsertionTableStatisticsQuery(SearchServerUnsyncDataSourceTableName, 0));
            ExecuteQuery(InsertionTableStatisticsQuery(HorizonServerUnsyncObjectsTableName, 0));
            ExecuteQuery(InsertionTableStatisticsQuery(HorizonServerUnsyncRelationshipsTableName, 0));
        }

        public void CreateHorizonServerUnsyncRelationshipsTable()
        {
            ExecuteQuery(CreationHorizonServerUnsyncRelationshipsTableQuery);
        }

        public void CreateSearchServerUnsyncObjectsTable()
        {
            ExecuteQuery(CreationSearchServerUnsyncObjectsTableQuery);
        }

        public List<long> GetOldestHorizonUnsyncObjects(int count)
        {
            List<long> result = new List<long>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateOldestHorizonUnsyncObjectsQuery(count), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(long.Parse(reader["ID"].ToString()));
                        }
                    }
                }
            }
            return result;
        }

        public List<long> GetOldestSearchUnsyncDatasources(int count)
        {
            List<long> result = new List<long>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateOldestSearchUnsyncDataSourcesQuery(count), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(long.Parse(reader["ID"].ToString()));
                        }
                    }
                }
            }
            return result;
        }


        public List<long> GetOldestSearchUnsyncObjects(int count)
        {
            List<long> result = new List<long>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateOldestSearchUnsyncObjectsQuery(count), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(long.Parse(reader["ID"].ToString()));
                        }
                    }
                }
            }
            return result;
        }
        public List<long> GetOldestHorizonUnsyncRelationships(int count)
        {
            List<long> result = new List<long>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateOldestHorizonUnsyncRelationshipsQuery(count), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(long.Parse(reader["ID"].ToString()));
                        }
                    }
                }
            }
            return result;
        }
        private string GenerateUnsyncConceptCountQuery(string tableName)
        {
            return string.Format("SELECT [NumberOfRow] FROM [{0}].[dbo].[{1}] where [TableName] = '{2}'", SearchIndexesSynchronizationDataBaseName, StatisticsTableName, tableName);
        }
        private string GenerateOldestSearchUnsyncObjectsQuery(int count)
        {
            return string.Format("SELECT TOP({0}) * FROM[{1}].[dbo].[{2}] ;", count, SearchIndexesSynchronizationDataBaseName, SearchServerUnsyncObjectsTableName);
        }
        private string GenerateOldestHorizonUnsyncObjectsQuery(int count)
        {
            return string.Format("SELECT TOP ({0}) * FROM[{1}].[dbo].[{2}]",
                count, SearchIndexesSynchronizationDataBaseName, HorizonServerUnsyncObjectsTableName);
        }
        private string GenerateOldestSearchUnsyncDataSourcesQuery(int count)
        {
            return string.Format("SELECT TOP({0}) * FROM[{1}].[dbo].[{2}] ;", count, SearchIndexesSynchronizationDataBaseName, SearchServerUnsyncDataSourceTableName);
        }
        private string GenerateOldestHorizonUnsyncRelationshipsQuery(int count)
        {
            return string.Format("SELECT TOP ({0}) * FROM[{1}].[dbo].[{2}]", count, SearchIndexesSynchronizationDataBaseName, HorizonServerUnsyncRelationshipsTableName);
        }

        public void ApplySearchObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            if (synchronizationResult.StayUnsynchronizeConceptsIDs.Length > 0)
            {
                GenerateAddUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.SearchServerUnsyncObjects);
            }

            if (synchronizationResult.SynchronizedConceptsIDs.Length > 0)
            {
                string removeUnSyncResultQuery = GenerateRemoveUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.SearchServerUnsyncObjects);
                ExecuteQuery(removeUnSyncResultQuery); 
            }
        }

        public void ApplySearchDataSourcesSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            if (synchronizationResult.StayUnsynchronizeConceptsIDs.Length > 0)
            {
                GenerateAddUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.SearchServerUnsyncDataSources);
            }

            if (synchronizationResult.SynchronizedConceptsIDs.Length > 0)
            {
                string removeUnSyncResultQuery = GenerateRemoveUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.SearchServerUnsyncDataSources);
                ExecuteQuery(removeUnSyncResultQuery);
            }
        }

        public void ApplyHorizonObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            if (synchronizationResult.StayUnsynchronizeConceptsIDs.Length > 0)
            {
                GenerateAddUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.HorizonServerUnsyncObjects);
            }

            if (synchronizationResult.SynchronizedConceptsIDs.Length > 0)
            {
                string removeUnSyncResultQuery = GenerateRemoveUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.HorizonServerUnsyncObjects);
                ExecuteQuery(removeUnSyncResultQuery);
            }
        }

        public void ApplyHorizonRelationshipsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            if (synchronizationResult.StayUnsynchronizeConceptsIDs.Length > 0)
            {
                GenerateAddUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.HorizonServerUnsyncRelationships);
            }

            if (synchronizationResult.SynchronizedConceptsIDs.Length > 0)
            {
                string removeUnSyncResultQuery = GenerateRemoveUnSyncResultQuery(synchronizationResult, SearchIndecesSynchronizationTables.HorizonServerUnsyncRelationships);
                ExecuteQuery(removeUnSyncResultQuery);
            }
        }

        private void GenerateAddUnSyncResultQuery(SynchronizationChanges synchronizationResult, SearchIndecesSynchronizationTables searchIndexTable)
        {
            RegisterUnpublishedConcepts(new List<long>(synchronizationResult.StayUnsynchronizeConceptsIDs), searchIndexTable);
        }

        private string GenerateRemoveUnSyncResultQuery(SynchronizationChanges synchronizationResult, SearchIndecesSynchronizationTables searchIndexTable)
        {
            string ids = "( ";
            foreach (var currentResult in synchronizationResult.SynchronizedConceptsIDs)
            {
                if (currentResult.Equals(synchronizationResult.SynchronizedConceptsIDs.Last()))
                {
                    ids += currentResult.ToString() + " )";
                }
                else
                {
                    ids += currentResult.ToString() + ", ";
                }

            }

            return string.Format("DELETE FROM {0} WHERE ID IN {1} ", searchIndexTable, ids);
        }

        private void ExecuteQuery(string query)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public void RegisterUnpublishedConcepts(List<long> unSyncIds, SearchIndecesSynchronizationTables searchIndexTable)
        {
            RegisterIdsTransaction(unSyncIds, searchIndexTable);
        }

        private void RegisterIdsTransaction(List<long> unsyncIds, SearchIndecesSynchronizationTables searchIndexTable)
        {
            if (unsyncIds.Any())
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    SqlTransaction transaction;
                    transaction = connection.BeginTransaction("Add Unsync Ids Transaction");
                    command.Connection = connection;
                    command.Transaction = transaction;
                    string deleteQuery = $"DELETE FROM {GetTableName(searchIndexTable)} WHERE ID IN ({getCommaSeprateIds(unsyncIds)})";
                    command.CommandText = deleteQuery;
                    command.ExecuteNonQuery();

                    DataTable dataTable = new DataTable();
                    DataColumn dc1 = new DataColumn("ID", typeof(string));
                    DataColumn dc2 = new DataColumn("LastTryTime", typeof(DateTime));
                    dataTable.Columns.Add(dc1);
                    dataTable.Columns.Add(dc2);
                    foreach (var unsyncId in unsyncIds)
                    {
                        DataRow dataRow = dataTable.NewRow();
                        dataRow[0] = unsyncId;
                        dataRow[1] = DateTime.Now;
                        dataTable.Rows.Add(dataRow);
                    }

                    using (SqlBulkCopy bulkCopy =
                        new SqlBulkCopy
                        (
                        connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers,
                        transaction
                        ))
                    {
                        bulkCopy.DestinationTableName = GetTableName(searchIndexTable);
                        try
                        {
                            bulkCopy.WriteToServer(dataTable);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                        }
                        finally
                        {
                            connection.Close();
                        }

                    }

                }

            }
        }

        private string getCommaSeprateIds(List<long> collection)
        {
            string commaSeprateString = string.Empty;
            foreach (long item in collection)
            {
                commaSeprateString += $"{item} ,";
            }
            return commaSeprateString.Remove(commaSeprateString.Length - 1);
        }

        private string GetTableName(SearchIndecesSynchronizationTables searchIndexTable)
        {
            switch (searchIndexTable)
            {
                case SearchIndecesSynchronizationTables.HorizonServerUnsyncObjects:
                    return HorizonServerUnsyncObjectsTableName;
                case SearchIndecesSynchronizationTables.HorizonServerUnsyncRelationships:
                    return HorizonServerUnsyncRelationshipsTableName;
                case SearchIndecesSynchronizationTables.SearchServerUnsyncObjects:
                    return SearchServerUnsyncObjectsTableName;
                case SearchIndecesSynchronizationTables.SearchServerUnsyncDataSources:
                    return SearchServerUnsyncDataSourceTableName;
                default:
                    throw new NotSupportedException();
            }
        }

        public int GetHorizonUnsyncObjectsCount()
        {
            int result = 0;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateUnsyncConceptCountQuery(HorizonServerUnsyncObjectsTableName), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = (int.Parse(reader["NumberOfRow"].ToString()));
                        }
                    }
                }
            }
            return result;
        }

        public int GetHorizonUnsyncRelationshipsCount()
        {
            int result = 0;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateUnsyncConceptCountQuery(HorizonServerUnsyncRelationshipsTableName), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = (int.Parse(reader["NumberOfRow"].ToString()));
                        }
                    }
                }
            }
            return result;
        }
        public int GetSearchUnsyncObjectsCount()
        {
            int result = 0;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GenerateUnsyncConceptCountQuery(SearchServerUnsyncObjectsTableName), connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = (int.Parse(reader["NumberOfRow"].ToString()));
                        }
                    }
                }
            }
            return result;
        }
    }
}
