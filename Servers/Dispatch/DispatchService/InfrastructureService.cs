using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.DatalakeEntities;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Dispatch.Logic;
using GPAS.Dispatch.Logic.Datalake;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPAS.Dispatch
{
    /// <summary>
    /// سرویسی که توسط Dispatch Server به فضای کاری ارائه می شود.
    /// همه درخواست های کاربر توسط این سرور توزیع می شود.
    /// </summary>
    class InfrastructureService : IInfrastructureService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        public List<KObject> GetObjectListById(long[] dbObjectIDs)
        {
            try
            {
                // با توجه به بازیابی نتایج برای زیرساخت، این کار با دسترسی ادمین انجام می‌شود
                RepositoryProvider repositoryProvider = new RepositoryProvider(AccessControl.Users.NativeUser.Admin.ToString());
                return repositoryProvider.GetObjects(dbObjectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary></summary>
        /// <param name="importingObjects">Same Type importing objects</param>

        public PublishResult Publish(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID, bool isContinousPublish = false)
        {
            try
            {
                PublishProvider publishProvider = new PublishProvider();
                return publishProvider.Publish(addedConcept, modifiedConcept, dataSourceID, isContinousPublish);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void FinalizeContinousPublish()
        {
            try
            {
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.FinalizeContinousPublish();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RegisterNewDataSourceToRepositoryServer(long dsId, string name, DataSourceType type, ACL acl, string description)
        {
            try
            {
                string createdBy = string.Empty;
                string createdTime = DateTime.Now.ToString();

                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                userAccountControlProvider.RegisterNewDataSourceToRepositoryServer(dsId, name, type, acl, description, createdBy, createdTime);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void SynchronizeNewDataSourceInSearchServer(long dsId, string name, DataSourceType type, ACL acl, string description)
        {
            try
            {
                string createdBy = string.Empty;
                string createdTime = DateTime.Now.ToString();

                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                userAccountControlProvider.SynchronizeNewDataSourceInSearchServer(dsId, name, type, acl, description, createdBy, createdTime);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region ID Assignment
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewObjectIdRange(long count)
        {
            try
            {
                return GetNewIdRange(count, IdGenerators.ObjectIdGenerator, "Object");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long GetLastAssignedObjectID()
        {
            try
            {
                return GetLastAssignedID(IdGenerators.ObjectIdGenerator, "Object");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long GetLastAssignedDataSourceID()
        {
            try
            {
                return GetLastAssignedID(IdGenerators.DataSourceIdGenerator, "DataSource");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewPropertyIdRange(long count)
        {
            try
            {
                return GetNewIdRange(count, IdGenerators.PropertyIdGenerator, "Property");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewRelationIdRange(long count)
        {
            try
            {
                return GetNewIdRange(count, IdGenerators.RelationIdGenerator, "Relationship");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long GetLastAssignedRelatioshshipID()
        {
            try
            {
                return GetLastAssignedID(IdGenerators.RelationIdGenerator, "Relationship");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetNewDataSourceId()
        {
            try
            {
                return GetNewID(IdGenerators.DataSourceIdGenerator, "Data Source");
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        private static long GetLastAssignedID(Entities.IdGenerator idGenerator, string generatorTitle)
        {
            if (!idGenerator.IsInitialized())
            {
                throw new InvalidOperationException($"{generatorTitle} ID generator is not initialized");
            }
            return idGenerator.LastAssignedId;
        }
        private static long GetNewID(Entities.IdGenerator idGenerator, string generatorTitle)
        {
            if (!idGenerator.IsInitialized())
            {
                throw new InvalidOperationException($"{generatorTitle} ID generator is not initialized");
            }
            return idGenerator.GenerateNewID();
        }
        private static long GetNewIdRange(long idsCount, Entities.IdGenerator idGenerator, string generatorTitle)
        {
            if (!idGenerator.IsInitialized())
            {
                throw new InvalidOperationException($"{generatorTitle} ID generator is not initialized");
            }
            return idGenerator.GenerateIDRange(idsCount);
        }
        #endregion

        #region Import from Data-Lake
        public string[] GetDatalakeSlice(string category, string dateTime, List<SearchCriteria> searchCriterias)
        {
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();
                return dataLakeProvider.DownloadFileFromDataLake(category, dateTime, searchCriterias);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        public List<GroupInfo> GetGroups()
        {
            try
            {
                GroupManagement groupManagement = new GroupManagement();
                return groupManagement.GetGroups();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region Data-Sources & Documents
        public void UploadDocumentFile(long docID, byte[] docContent)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDocumentFile(docID, docContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDataSourceFile(dataSourceID, dataSourceContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadFileAsDocumentAndDataSource(byte[] fileContent, long docID, long dataSourceID)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadFileAsDocumentAndDataSource(fileContent, docID, dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDocumentFromJobShare(long docID, string docJobSharePath)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDocumentFromJobShare(docID, docJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDataSourceFromJobShare(dataSourceID, dataSourceJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        public Stream GetOntology()
        {
            try
            {
                DispatchFileProvider fileProvider = new DispatchFileProvider();
                return fileProvider.GetOntology();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void OptimizeDeployment()
        {
            try
            {
                var optimizer = new OptimizationProvider();
                optimizer.PerformOptimization();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }


        public List<long> GetOldestSearchUnsyncObjects(int count)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetOldestSearchUnsyncObjects(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<long> GetOldestSearchUnsyncDataSources(int count)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetOldestSearchUnsyncDataSources(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<long> GetOldestHorizonUnsyncObjects(int count)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetOldestHorizonUnsyncObjects(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<long> GetOldestHorizonUnsyncRelatioinships(int count)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetOldestHorizonUnsyncRelationships(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void ApplySearchObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                searchIndexesSynchronization.ApplySearchObjectsSynchronizationResult(synchronizationResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void ApplySearchDataSourcesSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                searchIndexesSynchronization.ApplySearchDataSourcesSynchronizationResult(synchronizationResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void ApplyHorizonObjectsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                searchIndexesSynchronization.ApplyHorizonObjectsSynchronizationResult(synchronizationResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void ApplyHorizonRelationshipsSynchronizationResult(SynchronizationChanges synchronizationResult)
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                searchIndexesSynchronization.ApplyHorizonRelationshipsSynchronizationResult(synchronizationResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }


        public int GetHorizonUnsyncObjectsCount()
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetHorizonUnsyncObjectsCount();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public int GetHorizonUnsyncRelationshipsCount()
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetHorizonUnsyncRelationshipsCount();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public int GetSearchUnsyncObjectsCount()
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                return searchIndexesSynchronization.GetSearchUnsyncObjectsCount();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void DeleteHorizonServerUnsyncConcepts()
        {
            SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
            searchIndexesSynchronization.DeleteHorizonServerUnsyncConcepts();
        }

        public void DeleteSearchServerUnsyncConcepts()
        {
            SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
            searchIndexesSynchronization.DeleteSearchServerUnsyncConcepts();
        }

        public void IsAvailable()
        {
        }
    }
}