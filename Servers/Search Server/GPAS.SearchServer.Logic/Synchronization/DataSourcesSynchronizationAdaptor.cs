using GPAS.AccessControl;
using GPAS.DataSynchronization;
using GPAS.SearchServer.Access.DataClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Synchronization
{
    public class DataSourcesSynchronizationAdaptor : ISynchronizationAdaptor
    {
        public string ConceptsTypeTitle { get => "Data-Source"; }

        public async Task<List<long>> GetOldestUnsyncConceptIDs(int stepSize)
        {
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();
                return (await client.GetOldestSearchUnsyncDataSourcesAsync(stepSize)).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        public async Task<long> RetrieveLastAssignedID()
        {
            Access.DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new Access.DispatchService.InfrastructureServiceClient();
                return await sc.GetLastAssignedDataSourceIDAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public async Task<CachedConcepts> RetrieveConceptsByIDs(IEnumerable<long> IDsToSync)
        {
            var retrieveDataClient = new RetrieveDataClient();
            List<DataSourceInfo> retrievedDataSources = (await retrieveDataClient.GetDataSourcesByIDsAsync(IDsToSync.Distinct().ToList())).ToList();
            var cachedConcepts = new DataSourceSynchronizationCachedConcepts(retrievedDataSources);
            return cachedConcepts;
        }

        public void SynchronizeAllCachedConcepts(CachedConcepts cachedConcepts)
        {
            DataSourceSynchronizationCachedConcepts cachedDataSources = ValidateFilledCachedConcepts(cachedConcepts);
            var changeProvider = new DataChangeProvider();
            changeProvider.Synchronize(cachedDataSources.CachedDataSources);
        }

        private DataSourceSynchronizationCachedConcepts ValidateFilledCachedConcepts(CachedConcepts cachedConcepts)
        {
            if (!(cachedConcepts is DataSourceSynchronizationCachedConcepts))
            {
                throw new NotSupportedException("Cached-Concepts instance not supported");
            }
            DataSourceSynchronizationCachedConcepts cachedDataSources = (cachedConcepts as DataSourceSynchronizationCachedConcepts);
            if (cachedDataSources.CachedDataSources == null)
            {
                throw new InvalidOperationException("Data-Sources are not cached!");
            }
            return cachedDataSources;
        }

        public void SynchronizeSpecificCachedConcept(long dataSourceID, CachedConcepts cachedConcepts)
        {
            DataSourceSynchronizationCachedConcepts cachedDataSources = ValidateFilledCachedConcepts(cachedConcepts);
            DataSourceInfo dataSource = cachedDataSources.CachedDataSources.FirstOrDefault(ds => ds.Id.Equals(dataSourceID));
            if (dataSource != null)
            {
                var dataChangeProvider = new DataChangeProvider();
                dataChangeProvider.Synchronize(new List<DataSourceInfo>() { dataSource });
            }
            else
            {
                throw new InvalidOperationException($"Data-Source '{dataSourceID}' not found in cached Data-Sources!");
            }
        }

        public async Task FinalizeSynchronization(CachedConcepts cachedConcepts)
        {
            DataSourceSynchronizationCachedConcepts cachedDataSources = ValidateFilledCachedConcepts(cachedConcepts);
            if (cachedDataSources.SynchronizedIDs.Count == 0 && cachedDataSources.NotSynchronizeIDs.Count == 0)
                return;
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();
                //var syncResult = new Access.DispatchService.SynchronizationChanges()
                var syncResult = new Dispatch.Entities.SynchronizationChanges()
                {
                    SynchronizedConceptsIDs = cachedDataSources.SynchronizedIDs.ToArray(),
                    StayUnsynchronizeConceptsIDs = cachedDataSources.NotSynchronizeIDs.ToArray()
                };
                await client.ApplySearchDataSourcesSynchronizationResultAsync(syncResult);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        public void FinalizeContinousSynchronization()
        {
            IndexingProvider indexingProvider = new IndexingProvider();
            indexingProvider.FinalizeContinousIndexing();
        }
    }
}
