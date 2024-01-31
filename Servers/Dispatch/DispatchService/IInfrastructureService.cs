using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.DatalakeEntities;
using GPAS.Dispatch.Entities.Publish;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace GPAS.Dispatch
{
    [ServiceContract]
    interface IInfrastructureService
    {
#if debugMode
        [OperationContract]
        string test();
#endif
        #region Data Retrieval
        #region Object Retrieval
        [OperationContract]
        List<KObject> GetObjectListById(long[] dbObjectIDs);
        #endregion

        #endregion

        #region Publish / Data Manipulation
        #region Concepts
        [OperationContract]
        PublishResult Publish(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID, bool isContinousPublish = false);
        [OperationContract]
        void FinalizeContinousPublish();
        #endregion
        #region Data Source

        [OperationContract]
        void RegisterNewDataSourceToRepositoryServer(long dsId, string name, DataSourceType type, ACL acl, string description);

        [OperationContract]
        void SynchronizeNewDataSourceInSearchServer(long dsId, string name, DataSourceType type, ACL acl, string description);
        #endregion
        #endregion

        #region ID Assignment
        [OperationContract]
        long GetNewObjectIdRange(long count);
        [OperationContract]
        long GetLastAssignedObjectID();
        [OperationContract]
        long GetLastAssignedDataSourceID();
        [OperationContract]
        long GetNewPropertyIdRange(long count);
        [OperationContract]
        long GetNewRelationIdRange(long count);
        [OperationContract]
        long GetLastAssignedRelatioshshipID();
        [OperationContract]
        long GetNewDataSourceId();
        #endregion

        #region Data Import
        #region Import from Data-Lake
        [OperationContract]
        string[] GetDatalakeSlice(string category, string dateTime, List<SearchCriteria> searchCriterias);
        #endregion
        #endregion

        #region Account / Access Control
        [OperationContract]
        List<GroupInfo> GetGroups();
        #endregion

        #region Data-Sources & Documents
        [OperationContract]
        void UploadDocumentFile(long docID, byte[] docContent);
        [OperationContract]
        void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent);
        [OperationContract]
        void UploadFileAsDocumentAndDataSource(byte[] fileContent, long docID, long dataSourceID);
        [OperationContract]
        void UploadDocumentFromJobShare(long docID, string docJobSharePath);
        [OperationContract]
        void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath);
        #endregion

        #region Ontology
        [OperationContract]
        Stream GetOntology();
        #endregion
        
        #region Deployment Management
        [OperationContract]
        void OptimizeDeployment();
        #endregion

        [OperationContract]
        List<long> GetOldestSearchUnsyncObjects(int count);
        [OperationContract]
        List<long> GetOldestSearchUnsyncDataSources(int count);
        [OperationContract]
        List<long> GetOldestHorizonUnsyncObjects(int count);
        [OperationContract]
        List<long> GetOldestHorizonUnsyncRelatioinships(int count);

        [OperationContract]
        void ApplySearchObjectsSynchronizationResult(SynchronizationChanges synchronizationResult);
        [OperationContract]
        void ApplySearchDataSourcesSynchronizationResult(SynchronizationChanges synchronizationResult);
        [OperationContract]
        void ApplyHorizonObjectsSynchronizationResult(SynchronizationChanges synchronizationResult);
        [OperationContract]
        void ApplyHorizonRelationshipsSynchronizationResult(SynchronizationChanges synchronizationResult);

        [OperationContract]
        int GetHorizonUnsyncObjectsCount();
        [OperationContract]
        int GetHorizonUnsyncRelationshipsCount();
        [OperationContract]
        int GetSearchUnsyncObjectsCount();
        [OperationContract]
        void DeleteHorizonServerUnsyncConcepts();
        [OperationContract]
        void DeleteSearchServerUnsyncConcepts();

        [OperationContract]
        void IsAvailable();
    }
}