using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.ImageProcessing;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Dispatch.Entities.DatalakeEntities;
using GPAS.Dispatch.Entities.Jobs;
using GPAS.Dispatch.Entities.NLP;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Dispatch.Entities.Search;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.SearchAround;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.ServiceModel;

namespace GPAS.Dispatch
{

    [ServiceContract]
    interface IWorkspaceService
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
        #region Property Retrieval
        [OperationContract]
        List<KProperty> GetPropertyForObject(KObject kObject);
        [OperationContract]
        List<KProperty> GetPropertyForObjects(long[] dbObjIDs);
        [OperationContract]
        List<KProperty> GetPropertyListById(long[] dbOPropertyIDs);
        [OperationContract]
        List<KProperty> GetSpecifiedPropertiesOfObjectsByTypes(long[] objectsId, string[] propertiesType);
        [OperationContract]
        List<KProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(long[] objectIDs, string propertiesTypeUri, string propertiesValue);
        #endregion
        #region Relationship Retrieval
        [OperationContract]
        List<RelationshipBaseKlink> GetLinksSourcedByObject(KObject kObject, string relationshipTypeURI);
        [OperationContract]
        List<RelationshipBaseKlink> GetRelationshipListById(List<long> dbRelationshipIDs);
        [OperationContract]
        List<RelationshipBaseKlink> GetRelationshipsBySourceObject(long objectID, string typeURI);
        [OperationContract]
        RelationshipBaseKlink GetExistingRelationship(string typeURI, long source, long target, LinkDirection direction);
        #endregion
        #region Media Retrieval
        [OperationContract]
        List<KMedia> GetMediaUrisForObject(long objectID);
        #endregion
        #region Graph Retrieval
        [OperationContract]
        List<KGraphArrangement> GetPublishedGraphs();
        [OperationContract]
        byte[] GetPublishedGraphImage(int kGraphArrangementID);
        [OperationContract]
        byte[] GetPublishedGraph(int kGraphArrangementID);
        #endregion

        #region Quick Search
        [OperationContract]
        List<KObject> QuickSearch(string keyword);
        #endregion
        //#region Search
        //[OperationContract]
        //List<SearchResultModel> Search(SearchModel searchModel);
        //[OperationContract]
        //long GetTotalTextDocResults(SearchModel searchModel);
        //#endregion
        #region Filter Search
        [OperationContract]
        List<KObject> PerformFilterSearch(byte[] stream, int? count);

        [OperationContract]
        List<long> PerformSelectMatching(byte[] stream, List<long> ObjectIDs);
        #endregion
        #region Search-Around
        [OperationContract]
        RelationshipBasedResult FindRelatedEntities(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold);

        [OperationContract]
        RelationshipBasedResult FindRelatedDocuments(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold);

        [OperationContract]
        RelationshipBasedResult FindRelatedEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold);

        [OperationContract]
        EventBasedResult FindRelatedEntitiesAppearedInEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold);

        [OperationContract]
        PropertyBasedResult FindPropertiesSameWith(KProperty[] properties, int loadNResults, int totalResultsThreshold);

        [OperationContract]
        CustomSearchAroundResult PerformCustomSearchAround(Dictionary<string, long[]> searchedVertices, byte[] serializedCustomSearchAroundCriteria, int totalResultsThreshold);
        #endregion

        #region Geo Search
        [OperationContract]
        List<KObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult);
        [OperationContract]
        List<KObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult);

        [OperationContract]
        List<KObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult);
        [OperationContract]
        List<KObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult);
        #endregion

        #endregion

        #region Publish / Data Manipulation
        #region Concepts
        [OperationContract]
        PublishResult Publish(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID, bool isContinousPublish = false);
        [OperationContract]
        void FinalizeContinousPublish();
        [OperationContract]
        bool CanPerformNewPublish();
        #endregion
        #region Data Source

        [OperationContract]
        void RegisterNewDataSourceToRepositoryServer(long dsId, string name, DataSourceType type, ACL acl, string description);

        [OperationContract]
        void SynchronizeNewDataSourceInSearchServer(long dsId, string name, DataSourceType type, ACL acl, string description);
        #endregion
        #region Graph
        [OperationContract]
        KGraphArrangement PublishGraph(long id, string title,
         string description, byte[] GraphImage,
         byte[] GraphArrangement, int nodesCount,
         string timeCreated, long dataSourceID);

        [OperationContract]
        bool DeletePublishedGraph(int graphID);
        #endregion
        #endregion

        #region ID Assignment
        [OperationContract]
        long GetNewObjectId();
        [OperationContract]
        long GetNewObjectIdRange(long count);

        [OperationContract]
        long GetNewPropertyId();
        [OperationContract]
        long GetNewPropertyIdRange(long count);

        [OperationContract]
        long GetNewRelationId();
        [OperationContract]
        long GetNewRelationIdRange(long count);

        [OperationContract]
        long GetNewMediaId();
        [OperationContract]
        long GetNewMediaIdRange(long count);

        [OperationContract]
        long GetNewGraphId();

        [OperationContract]
        long GetNewInvestigationId();

        [OperationContract]
        long GetNewDataSourceId();
        #endregion

        #region Data Import
        #region Server-side Import
        [OperationContract]
        void RegisterNewImportRequests(SemiStructuredDataImportRequestMetadata[] requestsData);
        [OperationContract]
        JobRequest[] GetJobRequests();
        #endregion

        #region Import from attached Databases
        [OperationContract]
        string[] GetUriOfDatabasesForImport();
        [OperationContract]
        DataSet GetTablesAndViewsOfDatabaseForImport(string dbForImportURI);
        #endregion
        #region Import from Data-Lake
        [OperationContract]
        void StartStreamingIngestion(StreamingIngestion streamingIngestion);
        [OperationContract]
        void StopStreamingIngestion(StreamingIngestion streamingIngestion);
        [OperationContract]
        List<string> GetJobsStatus();
        [OperationContract]
        List<string> GetStreamJobsStatus();
        [OperationContract]
        List<string> GetPreviewDataFromDatalake(string category, string dateTime);
        [OperationContract]
        string[] GetDatalakeSlice(string category, string dateTime, List<SearchCriteria> searchCriterias);
        [OperationContract]
        List<string> GetDatalakeCategories(string path);
        [OperationContract]
        List<string> GetDatalakeSliceHeaders(string category, string dateTime);
        [OperationContract]
        void InsertFileIngestionJobStatus(IngestionFile ingestionFile);
        [OperationContract]
        void InsertStreamIngestionStartStatus(StreamingIngestion streamingIngestion);
        [OperationContract]
        void InsertStreamIngestionStopStatus(StreamingIngestion streamingIngestion);
        [OperationContract]
        bool IsListenProcessorExist(StreamingIngestion streamingIngestion);
        #endregion
        #endregion

        #region Account / Access Control
        [OperationContract]
        bool Authenticate(string userName, string password);

        [OperationContract]
        string GetDispatchCurrentDateTime();

        [OperationContract]
        List<GroupInfo> GetGroups();

        [OperationContract]
        List<GroupClassificationBasedPermission> GetClassificationBasedPermissionForGroups(string[] groupNames);
        [OperationContract]
        string[] GetGroupsOfUser(string username);
        [OperationContract]
        List<DataSourceACL> GetDataSourceACL(long[] dataSourceIDs);
        [OperationContract]
        Tuple<long[], long[]> GetReadableSubsetOfConcepts(long[] objIDs, long[] relationshipIDs, string[] groupNames);
        #endregion

        #region File Repository
        #region Medias
        [OperationContract]
        List<DirectoryContent> GetMediaPathContent(string path);
        [OperationContract]
        bool DeleteMediaDirectory(string path);
        [OperationContract]
        bool CreateMediaDirectory(string path);
        [OperationContract]
        bool RenameMediaDirectory(string sourcePath, string targetPath);
        [OperationContract]
        byte[] DownloadMediaFile(string filePath);
        [OperationContract]
        bool UploadMediaFile(byte[] fileToUpload, string fileName, string targetPath);
        #endregion

        #region Data-Sources & Documents

        [OperationContract]
        void UploadDataSourceFileByName(string docName, byte[] docContent);

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
        [OperationContract]
        byte[] DownloadDocumentFile(long docID);
        [OperationContract]
        byte[] DownloadDataSourceFile(long dataSourceID);

        [OperationContract]
        byte[] DownloadDataSourceFileByName(string dataSourceName);
        #endregion
        #endregion

        #region Ontology
        [OperationContract]
        Stream GetOntology();
        [OperationContract]
        Stream GetIcon();
        [OperationContract]
        void UpdateOntologyFile(Stream reader);
        #endregion

        #region Map and Geo.
        [OperationContract]
        byte[] GetMapTileImage(string tileSource, int zoomLevel, long x, long y);

        [OperationContract]
        GeographicalLocationModel GetGeoLocationBaseOnIP(string ip);

        [OperationContract]
        bool InsertGeoSpecialInformationBasedOnIP(string ip, double latitude, double longitude);

        [OperationContract]
        string[] GetMapTileSources();
        #endregion

        #region NLP
        [OperationContract]
        string GetDocumentPlaneText(long docID);
        [OperationContract]
        List<DetectedLanguage> DetectLanguage(string content);
        [OperationContract]
        TagCloudKeyPhrase[] GetTagCloud(string content);
        [OperationContract]
        TagCloudKeyPhrase[] GetLanguageTagCloud(string content, Language language);
        [OperationContract]
        List<string> GetSummarize(SummarizationRequest summarizationRequest);
        [OperationContract]
        List<string> GetLanguageSummarize(SummarizationRequest summarizationRequest, Language lang);

        [OperationContract]
        bool IsNLPServiceInstalled();
        #endregion

        #region dataSource
        [OperationContract]
        List<DataSourceInfo> GetDataSources(long dataSourceType, int star, string filter);

        [OperationContract]
        List<DataSourceInfo> GetAllDataSources(string filter);
        #endregion

        #region Image Analytics
        [OperationContract]
        List<Entities.Concepts.ImageProcessing.BoundingBox> FaceDetection(byte[] imageFile, string extention);

        [OperationContract]
        List<Entities.Concepts.ImageProcessing.RetrievedFaceKObject> FaceRecognition(byte[] imageFile, string extention, List<BoundingBox> boundingBoxs, int count);

        [OperationContract]
        bool IsMachneVisonServiceInstalled();
        #endregion

        #region Object Explorer
        [OperationContract]
        QueryResult RunStatisticalQuery(byte[] queryByteArray);

        [OperationContract]
        long[] RetrieveObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit);

        [OperationContract]
        PropertyValueStatistics RetrievePropertyValueStatistics(byte[] statQueryByteArray, string exploredPropertyTypeUri,
            int startOffset, int resultsLimit, long minimumCount);

        [OperationContract]
        PropertyBarValues RetrievePropertyBarValuesStatistics(byte[] queryByteArray, string exploredPropertyTypeUri, long bucketCount, double minValue, double maxValue);

        [OperationContract]
        LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray);

        [OperationContract]
        long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit);

        #endregion

        #region Save Investigation

        [OperationContract]
        void SaveInvestigation(KInvestigation kInvestigation);

        [OperationContract]
        List<InvestigationInfo> GetSavedInvestigations();

        [OperationContract]
        byte[] GetSavedInvestigationImage(long id);

        [OperationContract]
        byte[] GetSavedInvestigationStatus(long id);

        #endregion

        #region Timeline

        [OperationContract]
        long GetTimeLineMaxFrequecyCount(string[] propertiesTypeUri, string binLevel);

        [OperationContract]
        DateTime GetTimeLineMaxDate(string[] propertiesTypeUri, string binLevel);

        [OperationContract]
        DateTime GetTimeLineMinDate(string[] propertiesTypeUri, string binLevel);

        #endregion
        
        #region TextualSearch

        [OperationContract]
        List<TextualSearch.BaseSearchResult> PerformTextualSearch(byte[] stream);

        #endregion
    }
}