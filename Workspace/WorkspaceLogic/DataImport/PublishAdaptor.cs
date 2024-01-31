using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.DataImport
{
    public class PublishAdaptor : GPAS.DataImport.Publish.PublishAdaptor
    {
        private const int MaximumAcceptableGlobalResolutionCandidates = 200;
        private int maximumNumberOfGlobalResolutionCandidates = 50;
        public int MaximumNumberOfGlobalResolutionCandidates
        {
            get { return maximumNumberOfGlobalResolutionCandidates; }
            set
            {
                if (value > MaximumAcceptableGlobalResolutionCandidates)
                    throw new ArgumentOutOfRangeException(nameof(MaximumNumberOfGlobalResolutionCandidates));
                else
                    maximumNumberOfGlobalResolutionCandidates = value;
            }
        }

        public long GetFirstIdOfReservedObjectIdRange(long rangeLength)
        {
            long lastAssignableComponentId;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                // سرویس موجود آخرین شناسه‌ی قابل انتساب به اجزای جدید را برمی‌گرداند
                lastAssignableComponentId = sc.GetNewObjectIdRange(rangeLength);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return lastAssignableComponentId - rangeLength + 1;
        }
        public long GetFirstIdOfReservedPropertyIdRange(long rangeLength)
        {
            long lastAssignableComponentId;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                // سرویس موجود آخرین شناسه‌ی قابل انتساب به اجزای جدید را برمی‌گرداند
                lastAssignableComponentId = sc.GetNewPropertyIdRange(rangeLength);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return lastAssignableComponentId - rangeLength + 1;
        }
        public long GetFirstIdOfReservedRelationshipIdRange(long rangeLength)
        {
            long lastAssignableComponentId;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                // سرویس موجود آخرین شناسه‌ی قابل انتساب به اجزای جدید را برمی‌گرداند
                lastAssignableComponentId = sc.GetNewRelationIdRange(rangeLength);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return lastAssignableComponentId - rangeLength + 1;
        }

        public KObject[] RetrieveStoredObjectsByID(IEnumerable<long> objIDs)
        {
            KObject[] result;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                result = sc.GetObjectListById(objIDs.ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }
        
        public long GetNewDataSourceID()
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                return sc.GetNewDataSourceId();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public void UploadDocumentToFileRepository(byte[] documentContent, long documentId, long dataSourceID)
        {
            DataSourceProvider DataSourceProvider = new DataSourceProvider();
            DataSourceProvider.UploadDocument(documentContent, documentId, dataSourceID);
        }
        public void UploadSemiStructuredDataSouurceToFileRepository(byte[] dataSourceContent, long dataSourceID)
        {
            DataSourceProvider DataSourceProvider = new DataSourceProvider();
            DataSourceProvider.UploadDataSource(dataSourceID, dataSourceContent);
        }

        public void UploadSemiStructuredDataSouurceToFileRepositoryByName(byte[] dataSourceContent, string dataSourceName)
        {
            DataSourceProvider DataSourceProvider = new DataSourceProvider();
            DataSourceProvider.UploadDataSourceByName(dataSourceName, dataSourceContent);
        }

        public void UploadSharedSemiStructuredDataSouurceToFileRepository(string sharedCsvDataSourcePath, long dataSourceID)
        {
            throw new NotSupportedException();
        }

        public void RegisterNewDataSourceToRepositoryServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.RegisterNewDataSourceToRepositoryServer(dataSourceID, name, sourceType, dataSourceACL, description);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public async Task RegisterNewDataSourceToRepositoryServerAsync(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                await sc.RegisterNewDataSourceToRepositoryServerAsync(dataSourceID, name, sourceType, dataSourceACL, description);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void SynchronizeNewDataSourceInSearchServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.SynchronizeNewDataSourceInSearchServer(dataSourceID, name, sourceType, dataSourceACL, description);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public async Task SynchronizeNewDataSourceInSearchServerAsync(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                await sc.SynchronizeNewDataSourceInSearchServerAsync(dataSourceID, name, sourceType, dataSourceACL, description);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public PublishResult PublshConcepts(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, long dataSourceID, bool isContinous = false)
        {
            PublishResult result;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                result = sc.Publish(addedConcepts, modifiedConcepts, dataSourceID, isContinous);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }
        public async Task<PublishResult> PublshConceptsAsync(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, long dataSourceID, bool isContinous = false)
        {
            PublishResult result;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                result = await sc.PublishAsync(addedConcepts, modifiedConcepts, dataSourceID, isContinous);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }

        public async Task<long> GetNewObjectIdAsync()
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                return await sc.GetNewObjectIdAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public async Task<long> GetNewPropertyIdAsync()
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                return await sc.GetNewPropertyIdAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public async Task<long> GetNewRelationshipIdAsync()
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                return await sc.GetNewRelationIdAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public async Task<IEnumerable<KProperty>> RetrieveStoredPropertiesForObjectAsync(KObject obj)
        {
            KProperty[] result;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                result = await sc.GetPropertyForObjectAsync(obj);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }

        public async Task<IEnumerable<KProperty>> RetrieveStoredPropertiesForObjectsAsync(IEnumerable<KObject> objs)
        {
            KProperty[] result;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                result = await sc.GetPropertyForObjectsAsync(objs.Select(o=>o.Id).ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }

        public void FinalizeContinousPublish()
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.FinalizeContinousPublish();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
    }
}