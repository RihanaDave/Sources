using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.IndexChecking;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.SearchServer.Entities.Sync;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GPAS.SearchServer
{
    [ServiceContract]
    interface IService
    {
        #region Quick-Search
        [OperationContract]
        SearchObject[] QuickSearch(string keyword, AuthorizationParametters authorizationParametters);

        #endregion
        //#region Search
        //[OperationContract]
        //List<SearchResultModel> Search(SearchModel searchModel, AuthorizationParametters authorizationParametters);
        //[OperationContract]
        //long GetTotalTextDocResults(SearchModel searchModel, AuthorizationParametters authorizationParametters);
        //#endregion
        #region Filter Search
        [OperationContract]
        List<SearchObject> PerformFilterSearch(byte[] stream, int? count, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<long> PerformSelectMatching(byte[] stream, List<long> ObjectIDs, AuthorizationParametters authorizationParametters);
        #endregion

        #region Search-Around
        [OperationContract]
        List<PropertiesMatchingResults> FindPropertiesSameWith(KProperty[] properties, int totalResultsThreshold, AuthorizationParametters authorizationParametters);
        #endregion

        #region Data Synchronization
        [OperationContract]
        Dispatch.Entities.Publish.SynchronizationResult SyncPublishChanges
            (AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts
            , long dataSourceID, bool isContinousPublish = false);
        [OperationContract]
        void FinalizeContinousPublish();
#if DEBUG
        [OperationContract]
        void ResetIndexes(bool DeleteExistingIndexes);
#endif
        [OperationContract]
        bool IsDataIndicesStable();
        [OperationContract]
        void RemoveSearchIndexes();
        #endregion

        #region Search-Server Management
        [OperationContract]
        void Optimize();
        #endregion

        #region Geo Search
        [OperationContract]
        List<SearchObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters);
        [OperationContract]
        List<SearchObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<SearchObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters);
        [OperationContract]
        List<SearchObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters);

        #endregion

        [OperationContract]
        string GetDocumentPossibleExtractedContent(long docID);

        [OperationContract]
        void AddNewGroupFieldsToSearchServer(List<string> newGroupsName);

        #region dataSource
        [OperationContract]
        Dispatch.Entities.Publish.SynchronizationResult SynchronizeDataSource(DataSourceInfo dataSourceInfo);

        [OperationContract]
        List<DataSourceInfo> GetDataSources(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<DataSourceInfo> GetAllDataSources(int count, string filter, AuthorizationParametters authorizationParametters);
        #endregion

        #region ImageAnalytics
        [OperationContract]
        List<BoundingBox> FaceDetection(byte[] imageFile, string extention, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<RetrievedFace> FaceRecognition(byte[] imageFile, string extention, List<BoundingBox> boundingBoxs, int count, AuthorizationParametters authorizationParametters);

        [OperationContract]
        bool IsMachneVisonServiceInstalled();
        #endregion

        #region Object Explorer
        [OperationContract]
        QueryResult RunStatisticalQuery(byte[] queryByteArray, AuthorizationParametters authParams);
        [OperationContract]
        long[] RetrieveObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit, AuthorizationParametters authParams);
        [OperationContract]
        PropertyValueStatistics RetrievePropertyValueStatistics
            (byte[] queryByteArray, string exploredPropertyTypeUri, int startOffset, int resultsLimit
            , long minimumCount, AuthorizationParametters authParams);

        [OperationContract]
        LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray, AuthorizationParametters authParams);

        [OperationContract]
        long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit, AuthorizationParametters authorizationParametters);

        [OperationContract]
        PropertyBarValues RetrievePropertyBarValuesStatistics(byte[] queryByteArray, string numericPropertyTypeUri, long bucketCount, double minValue, double maxValue, AuthorizationParametters authorizationParametters);

        #endregion

        #region Indexed Concept

        [OperationContract]
        SearchIndexCheckingResult IndexChecking(SearchIndexCheckingInput input, AuthorizationParametters authorizationParameters);

        #endregion

        [OperationContract]
        void IsAvailable();

        #region Timeline

        [OperationContract]
        long GetTimeLineMaxFrequecyCount(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters);

        [OperationContract]
        DateTime GetTimeLineMaxDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters);

        [OperationContract]
        DateTime GetTimeLineMinDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters);

        #endregion

        #region TextualSearch

        [OperationContract]
        List<TextualSearch.BaseSearchResult> PerformTextualSearch(byte[] stream, AuthorizationParametters authorizationParametters);

        #endregion

        [OperationContract]
        List<SearchProperty> GetDBPropertyByObjectId(long objectId, AuthorizationParametters authorizationParametters);

        [OperationContract]
        SearchObject GetObject(long objectId);

        [OperationContract]
        List<SearchProperty> GetDBPropertyByObjectIds(long[] propertyIds, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authorizationParametters);

        [OperationContract]
        void RegisterNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime);

        [OperationContract]
        List<SearchDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs);

        [OperationContract]
        List<SearchObject> GetObjectByIDs(long[] objectIDs);

        [OperationContract]
        long GetLastAsignedDataSourceId();

        [OperationContract]
        long GetLastAsignedObjectId();

        [OperationContract]
        long GetLastAsignedPropertyId();

        [OperationContract]
        long GetLastAsignedRelationId();

        [OperationContract]
        long GetLastAssignedGraphaId();

        [OperationContract]
        List<SearchProperty> GetDBPropertyByObjectIdsWithoutAuthorization(long[] objectIDs);

        [OperationContract]
        List<SearchRelationship> RetrieveRelationships(long[] relationshipIDs);

        [OperationContract]
        List<SearchRelationship> GetRelationships(List<long> relationshipIDs, AuthorizationParametters authorizationParametters);

        [OperationContract]
        SearchGraphArrangement SaveNew(SearchGraphArrangement dbGraphArrangement);

        [OperationContract]
        List<SearchGraphArrangement> GetGraphArrangements(AuthorizationParametters authorizationParametters);

        [OperationContract]
        byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authorizationParametters);

        [OperationContract]
        byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authorizationParametters);

        [OperationContract]
        bool DeleteGraph(int id);

        [OperationContract]
        List<SearchRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs);

        [OperationContract]
        long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission);

        [OperationContract]
        List<SearchDataSourceACL> RetrieveTopNDataSourceACLs(long topN);

        [OperationContract]
        List<SearchRelationship> GetRelationshipsBySourceObject(long objectID, string typeUri, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<SearchRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authorizationParametters);

        [OperationContract]
        SearchRelationship GetExistingRelationship(string typeUri, long source, long target, int direction, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<SearchRelationship> GetSourceLink(long objectId, string typeUri, AuthorizationParametters authorizationParametters);

        [OperationContract]
        List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authorizationParametters);
    }
}