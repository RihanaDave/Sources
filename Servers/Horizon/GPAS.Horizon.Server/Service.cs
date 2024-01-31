using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Logic;
using GPAS.Horizon.Logic.Synchronization;
using GPAS.Logger;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GPAS.Horizon.Entities.IndexChecking;

namespace GPAS.Horizon.Server
{
    public class Service : IService
    {
        public Service()
        { }
        private void WriteErrorLog(Exception ex, MemoryStream logStream = null, string logStreamTitle = "")
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex, logStream, logStreamTitle);
        }

#if DEBUG
        public void ResetIndexes(bool DeleteExistingIndexes)
        {
            try
            {
                var indexProvider = new IndexingProvider
                {
                    DeleteExistingIndexes = DeleteExistingIndexes
                };
                indexProvider.ResetAllIndexes().Wait();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
#endif

        public SynchronizationResult SyncPublishChanges(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, 
            long dataSourceID = -1, bool isContinousPublish = false)
        {
            MemoryStream consoleOutputStream = null;
            ProcessLogger logger = null;
            try
            {
                consoleOutputStream = new MemoryStream();
                logger = new ProcessLogger();
                logger.Initialization(consoleOutputStream);
                IndexingProvider indexingProvider = new IndexingProvider();
                Task<bool> syncTask = indexingProvider.SynchronizePublishChanges(addedConcepts, modifiedConcepts, dataSourceID, isContinousPublish, logger);

                Task.WaitAll(syncTask);

                var utility = new StreamUtility();
                var result = new SynchronizationResult()
                {
                    IsCompletelySynchronized = syncTask.Result,
                    SyncronizationLog = utility.GetStringFromStream(consoleOutputStream)
                };
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, consoleOutputStream, "Console Output");
                throw;
            }
            finally
            {
                if (logger != null)
                    logger.Finalization();
                if (consoleOutputStream != null)
                    consoleOutputStream.Close();
            }
        }
               
        public void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName)
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
                graphRepositoryProvider.AddNewGroupPropertiesToEdgeClass(newGroupsName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEntities(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAroundProvider provider = new SearchAroundProvider();
                return provider.FindRelatedEntities(searchedObjects, resultLimit, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedDocuments(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAroundProvider provider = new SearchAroundProvider();
                return provider.FindRelatedDocuments(searchedObjects, resultLimit, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEvents(Dictionary<string, long[]> searchedObjects, long resultLimit, 
            AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAroundProvider provider = new SearchAroundProvider();
                return provider.FindRelatedEvents(searchedObjects, resultLimit, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<EventBasedResultsPerSearchedObjects> FindRelatedEntitiesAppearedInEvents(Dictionary<string, long[]> searchedObjects, 
            long resultLimit, AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAroundProvider provider = new SearchAroundProvider();
                return provider.FindRelatedEntitiesAppearedInEvents(searchedObjects, resultLimit, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public CustomSearchAroundResultIDs[] PerformCustomSearchAround(Dictionary<string, long[]> searchedObjects, 
            byte[] serializedCustomSearchAroundCriteria, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAroundProvider searchAroundProvider = new SearchAroundProvider();
                return searchAroundProvider.PerformCustomSearchAround(searchedObjects, serializedCustomSearchAroundCriteria, resultLimit,
                    authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool IsDataIndicesStable()
        {
            try
            {
                IndexingProvider resetDataSyncronizer = new IndexingProvider();
                return resetDataSyncronizer.GetStatusOfStablity();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RemoveHorizonIndexes()
        {
            try
            {
                IndexingProvider indexProvider = new IndexingProvider();
                indexProvider.DeleteExistIndexes();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region Indexed Concept

        public HorizonIndexCheckingResult HorizonIndexChecking(HorizonIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            IndexCheckingProvider indexCheckingProvider = new IndexCheckingProvider();
            return indexCheckingProvider.StartHorizonIndexChecking(input, authorizationParameters);
        }

        #endregion

        public void IsAvailable()
        {
            // Method intentionally left empty.
        }

        public List<IndexModel> GetAllIndexes()
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
               return graphRepositoryProvider.GetAllIndexes();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void CreateIndex(IndexModel index)
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
                graphRepositoryProvider.CreateIndex(index);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void EditIndex(IndexModel oldIndex, IndexModel newIndex)
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
                graphRepositoryProvider.EditIndex(oldIndex, newIndex);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void DeleteIndex(IndexModel index)
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
                graphRepositoryProvider.DeleteIndex(index);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void DeleteAllIndexes()
        {
            try
            {
                GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
                graphRepositoryProvider.Init();
                graphRepositoryProvider.DeleteAllIndexes();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }        
    }
}