using GPAS.Dispatch.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.ServiceAccess;

namespace GPAS.Dispatch.Logic
{
    public class SearchIndexesSynchronization
    {
        public void Init()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.CreateDataBase();
            databaseAccess.CreateSearchServerUnsyncObjectsTable();
            databaseAccess.CreateSearchServerUnsyncDataSourceTable();
            databaseAccess.CreateHorizonServerUnsyncObjectsTable();
            databaseAccess.CreateHorizonServerUnsyncRelationshipsTable();
            databaseAccess.CreateStatisticTable();
            databaseAccess.InitStatisticTable();
            databaseAccess.CreateTriggers();
        }
        
        public void DeleteHorizonServerUnsyncConcepts()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.DeleteHorizonServerUnsyncConcepts();
        }

        public void DeleteSearchServerUnsyncConcepts()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.DeleteSearchServerUnsyncConcepts();
        }

        public List<long> GetOldestSearchUnsyncObjects(int count)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetOldestSearchUnsyncObjects(count);
        }

        public List<long> GetOldestSearchUnsyncDataSources(int count)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetOldestSearchUnsyncDatasources(count);
        }
        public List<long> GetOldestHorizonUnsyncObjects(int count)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetOldestHorizonUnsyncObjects(count);
        }

        public List<long> GetOldestHorizonUnsyncRelationships(int count)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetOldestHorizonUnsyncRelationships(count);
        }

        public void ApplySearchObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.ApplySearchObjectsSynchronizationResult(synchronizationResult);
        }


        public void ApplySearchDataSourcesSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.ApplySearchDataSourcesSynchronizationResult(synchronizationResult);
        }

        public void ApplyHorizonObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.ApplyHorizonObjectsSynchronizationResult(synchronizationResult);
        }

        public void ApplyHorizonRelationshipsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            databaseAccess.ApplyHorizonRelationshipsSynchronizationResult(synchronizationResult);
        }

        public int GetUnsyncConceptsCount()
        {
            return (GetHorizonUnsyncObjectsCount() + GetHorizonUnsyncRelationshipsCount() + GetSearchUnsyncObjectsCount());
        }

        public int GetHorizonUnsyncObjectsCount()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetHorizonUnsyncObjectsCount();
        }

        public int GetHorizonUnsyncRelationshipsCount()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetHorizonUnsyncRelationshipsCount();
        }

        public int GetSearchUnsyncObjectsCount()
        {
            SearchIndexesSynchronizationDatabaseAccess databaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            return databaseAccess.GetSearchUnsyncObjectsCount();
        }

        public async Task<bool> IsHorizonDataIndicesStable()
        {
            bool result = false; 
            try
            {
                SearchSynchronizationServiceClient searchSynchronizationServiceClient = new SearchSynchronizationServiceClient();
                result = await searchSynchronizationServiceClient.IsHorizonDataIndicesStable();
            }
            catch (Exception)
            {
                result = false;
            }            
            return result; 
        }
        public async Task<bool> IsSearchDataIndicesStable()
        {
            bool result = false;
            try
            {
                SearchSynchronizationServiceClient searchSynchronizationServiceClient = new SearchSynchronizationServiceClient();
                result = await searchSynchronizationServiceClient.IsSearchDataIndicesStable();
            }
            catch (Exception)
            {
                result = false;                
            }
            return result;
        }

    }
}
