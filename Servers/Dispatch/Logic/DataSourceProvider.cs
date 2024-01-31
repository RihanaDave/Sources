using GPAS.AccessControl;
using GPAS.Dispatch.DataAccess;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.ServiceAccess.SearchService;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FileRepository = GPAS.Dispatch.ServiceAccess.FileRepositoryService;

namespace GPAS.Dispatch.Logic
{
    public class DataSourceProvider
    {
        private string CallerUserName = "";
        static readonly int NumberOfDataSourcesPerType = int.Parse(ConfigurationManager.AppSettings["NumberOfDataSourcesPerType"]);
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        public DataSourceProvider()
        {
        }
        public DataSourceProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }
        public void SynchronizeDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createdBy, string createdTime)
        {

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                DataSourceInfo dataSourceInfo = new DataSourceInfo()
                {
                    Id = dsId,
                    Description = description,
                    Name = name,
                    Type = (long)type,
                    CreatedBy = createdBy,
                    CreatedTime = createdTime,
                    Acl = acl
                };
                proxy.SynchronizeDataSource(dataSourceInfo);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                AddUnsyncDataSourceToDataSourceUnsyncTable(dsId);
                throw;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        private void AddUnsyncDataSourceToDataSourceUnsyncTable(long dsId)
        {
            List<long> unSyncIds = new List<long>();
            unSyncIds.Add(dsId);
            SearchIndexesSynchronizationDatabaseAccess searchIndexesSynchronizationDatabaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            searchIndexesSynchronizationDatabaseAccess.RegisterUnpublishedConcepts(unSyncIds, SearchIndecesSynchronizationTables.SearchServerUnsyncDataSources);
        }

        public List<DataSourceInfo> GetDataSources(long dataSourceType, int star, string filter)
        {
            AuthorizationParametters authorizationParametter
             = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetDataSources(dataSourceType, star, NumberOfDataSourcesPerType, filter, authorizationParametter).ToList();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public List<DataSourceInfo> GetAllDataSources(string filter)
        {
            AuthorizationParametters authorizationParametter
            = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetAllDataSources(NumberOfDataSourcesPerType, filter, authorizationParametter).ToList();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        #region Data-Sources & Documents Upload/Download

        
        public void UploadDataSourceFileByName(string docName, byte[] docContent)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadDocumentFileByName(docName, docContent);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public void UploadDocumentFile(long docID, byte[] docContent)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadDocumentFile(docID, docContent);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadDataSourceFile(dataSourceID, dataSourceContent);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public void UploadFileAsDocumentAndDataSource(byte[] fileContent, long docID, long dataSourceID)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadFileAsDocumentAndDataSource(fileContent, docID, dataSourceID);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public void UploadDocumentFromJobShare(long docID, string docJobSharePath)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadDocumentFromJobShare(docID, docJobSharePath);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                proxy.UploadDataSourceFromJobShare(dataSourceID, dataSourceJobSharePath);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public byte[] DownloadDocumentFile(long docID)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                byte[] resultFile = proxy.DownloadDocumentFile(docID);
                return resultFile;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public byte[] DownloadDataSourceFile(long dataSourceID)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                byte[] resultFile = proxy.DownloadDataSourceFile(dataSourceID);
                return resultFile;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public byte[] DownloadDataSourceFileByName(string dataSourceName)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                byte[] resultFile = proxy.DownloadDataSourceFileByName(dataSourceName);
                return resultFile;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        #endregion
    }
}
