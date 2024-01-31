using GPAS.DataImport.ConceptsToGenerate;
using GPAS.Dispatch.Entities.Concepts;
using System.Collections.Generic;
using GPAS.Dispatch.Entities.Publish;
using GPAS.AccessControl;

namespace GPAS.DataImport.Publish
{
    public interface PublishAdaptor
    {
        long GetFirstIdOfReservedObjectIdRange(long rangeLength);
        long GetFirstIdOfReservedPropertyIdRange(long rangeLength);
        long GetFirstIdOfReservedRelationshipIdRange(long rangeLength);
        KObject[] RetrieveStoredObjectsByID(IEnumerable<long> objIDs);
        #region مراحل چهارگانه‌ی انتشار
        // 1
        long GetNewDataSourceID();
        // 2
        void UploadSemiStructuredDataSouurceToFileRepository(byte[] dataSourceContent, long dataSourceID);
        void UploadSharedSemiStructuredDataSouurceToFileRepository(string sharedCsvDataSourcePath, long dataSourceID);
        void UploadDocumentToFileRepository(byte[] documentContent, long documentId, long dataSourceID);
        // 3
        void RegisterNewDataSourceToRepositoryServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "");
        void SynchronizeNewDataSourceInSearchServer(long dataSourceID, string name, DataSourceType sourceType, ACL dataSourceACL, string description = "");
        // 4
        PublishResult PublshConcepts(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, long dataSourceID, bool isContinous = false);
        #endregion
        void FinalizeContinousPublish();
    }
}
