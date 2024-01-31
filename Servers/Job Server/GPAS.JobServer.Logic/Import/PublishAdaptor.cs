using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using System.Collections.Generic;
using System.Linq;
using DispatchService = GPAS.JobServer.ServiceAccess.DispatchService;

namespace GPAS.JobServer.Logic.SemiStructuredDataImport
{
    public class PublishAdaptor : DataImport.Publish.PublishAdaptor
    {
        public long GetFirstIdOfReservedObjectIdRange(long rangeLength)
        {
            long lastAssignableComponentId;
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                sc.UploadFileAsDocumentAndDataSource(documentContent, documentId, dataSourceID);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public void UploadSemiStructuredDataSouurceToFileRepository(byte[] dataSourceContent, long dataSourceID)
        {
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                sc.UploadDataSourceFile(dataSourceID, dataSourceContent);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void UploadSharedSemiStructuredDataSouurceToFileRepository(string sharedCsvDataSourcePath, long dataSourceID)
        {
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                sc.UploadDataSourceFromJobShare(dataSourceID, sharedCsvDataSourcePath);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void RegisterNewDataSourceToRepositoryServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                sc.RegisterNewDataSourceToRepositoryServer(dataSourceID, name, sourceType, dataSourceACL, description);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void SynchronizeNewDataSourceInSearchServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "")
        {
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                sc.SynchronizeNewDataSourceInSearchServer(dataSourceID, name, sourceType, dataSourceACL, description);
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
                result = sc.Publish(addedConcepts, modifiedConcepts, dataSourceID, isContinous);
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
            DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new DispatchService.InfrastructureServiceClient();
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