using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Entities;
using System.Collections.Generic;
using System.ServiceModel;
using GPAS.Horizon.Entities.IndexChecking;

namespace GPAS.Horizon.Server
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IService
    {
#if DEBUG
        [OperationContract]
        void ResetIndexes(bool DeleteExistingIndexes);
#endif

        [OperationContract]
        Dispatch.Entities.Publish.SynchronizationResult SyncPublishChanges
            (AddedConcepts addedConcept, ModifiedConcepts modifiedConcept
            , long dataSourceID = -1, bool isContinousPublish = false);

        [OperationContract]
        void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName);

        #region Search-Around
        [OperationContract]
        List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEntities(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<RelationshipBasedResultsPerSearchedObjects> FindRelatedDocuments(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEvents(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<EventBasedResultsPerSearchedObjects> FindRelatedEntitiesAppearedInEvents(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters);

        [OperationContract]
        CustomSearchAroundResultIDs[] PerformCustomSearchAround(Dictionary<string, long[]> searchedObjects, byte[] serializedCustomSearchAroundCriteria, long resultLimit,
            AuthorizationParametters authorizationParametters);
        #endregion

        [OperationContract]
        bool IsDataIndicesStable();
        [OperationContract]
        void RemoveHorizonIndexes();

        #region Indexed Concept

        [OperationContract]
        HorizonIndexCheckingResult HorizonIndexChecking(HorizonIndexCheckingInput input, AuthorizationParametters authorizationParameters);

        #endregion

        #region Horizon Index 

        [OperationContract]
        List<IndexModel> GetAllIndexes();

        [OperationContract]
        void CreateIndex(IndexModel index);

        [OperationContract]
        void EditIndex(IndexModel oldIndex, IndexModel newIndex);

        [OperationContract]
        void DeleteIndex(IndexModel index);

        [OperationContract]
        void DeleteAllIndexes();

        #endregion

        [OperationContract]
        void IsAvailable();
    }
}
