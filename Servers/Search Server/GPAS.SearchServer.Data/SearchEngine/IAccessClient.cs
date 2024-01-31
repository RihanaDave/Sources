using GPAS.AccessControl;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GPAS.SearchServer.Access.SearchEngine
{
    public interface IAccessClient
    {
        bool AutoCommitEnabled { get; set; }
        long AutoCommitMaximumPendingDocumentsCount { get; set; }

        #region Manager Functions
        void Init(string[] groupNames);

        void AddFieldToSchema(string fieldName, DataType fieldType, bool isMultiValue = false);

        void Optimize();
        #endregion

        #region Retrieve Functions
        //long GetTotalTextDocResults(SearchModel searchModel, AuthorizationParametters authorizationParametters);
        //List<SearchObject> GetResultsForDocText(SearchModel searchModel, AuthorizationParametters authorizationParametters, Ontology.Ontology ontology);
        List<SearchObject> GetEntityDocumentIDsForMatchedKeyword(
            string keyword, AuthorizationParametters authorizationParametters
            , long resultLimit, Ontology.Ontology ontology
            );

        List<SearchObject> GetEventDocumentIDsForMatchedKeyword(
            string keyword, AuthorizationParametters authorizationParametters
            , long resultLimit, Ontology.Ontology ontology
            );

        List<SearchObject> GetDocumentDocumentIDsForMatchedKeyword(
            string keyword, AuthorizationParametters authorizationParametters
            , long resultLimit, Ontology.Ontology ontology
            );
        
        List<long> GetFileDocumentOwnerObjectIDs(
            string keyword, AuthorizationParametters authorizationParametters
            , long resultLimit);

        List<SearchObject> GetObjectDocumentIDByFilterCriteriaSet(
            CriteriaSet criteria, AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology, long resultLimit
            );
        List<long> GetPropertyDocumentIDByFilterCriteriaSet(
            CriteriaSet criteria, AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology, long resultLimit
            );

        List<long> GetObjectDocumentIDByFilterCriteriaSet(List<long> objectIDs,
          CriteriaSet criteria, AuthorizationParametters authorizationParametters
          , Ontology.Ontology ontology, long resultLimit);

        List<Entities.SearchEngine.Documents.DataSourceDocument> GetDataSources(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters);

        List<Entities.SearchEngine.Documents.DataSourceDocument> GetAllDataSources(int count, string filter, AuthorizationParametters authorizationParametters);

        bool IsFileDocumentExistWithID(string fileDocID);

        string GetFileDocumentPossibleExtractedContent(string fileDocID);

        List<RetrievedFace> RetrieveImageDocument(KeyValuePair<BoundingBox, List<double>> imageEmbedding, int treshould, AuthorizationParametters authorizationParametters);
        #endregion

        #region Store Functions
        void AddObjectDocument(ObjectDocument objDoc);

        void AddPropertyDocument(SearchObject objDoc, List<Property> properties);

        void AddRelationshipDocument(List<Relationship> value);

        void AddFileDocument(File fileDoc);

        void AddImageDocument(ImageDocument imageDoc);

        void AddDataSourceDocument(DataSourceDocument dataSource);

        void UpdateObjectDocumentField(string objDocID, string fieldName, string newValue);

        void UpdatePropertyDocumentField(string propertyDocID, string ownerObjDocID, string fieldName, string newvalue);

        void MovePropertyDocuments(List<string> currentOwnerObjDocIDs, string newOwnerObjID);

        void MoveRelationshipDocuments(List<string> currentSourceObjDocIDs, string newSourceObjDocID);

        void ChangeValueFromFileDocumentMultiField(string multiValueFieldName, string currentValue, string newValue);

        void AddValueToFileDocumentMultiValue(string fileDocID, string multiValueFieldName, List<string> valueToAdd);

        void RemoveValueFromFileDocumentMultiField(
            string fileDocID, string multivalueFieldName, string valueToRemove
            , bool removedocumentIfNoMoreValueRemainsForField);

        void ApplyChanges(string collectionName, bool alsoCommitChanges = true);

        void DeleteObjectDocument(List<string> objDocIDList);

        void DeleteFileDocuments(List<string> fileDocIDs);

        void DeleteAllDocuments();
        void Commit();
        #endregion

        #region Geo Search
        List<SearchObject> GetObjectDocumentIDByGeoCircleSearch(CircleSearchCriteria circleSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters
           );

        List<SearchObject> GetObjectDocumentIDByGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters
           );


        List<SearchObject> GetObjectDocumentIDByGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria
            , CriteriaSet filterSearchCriteria
            , int maxResultAuthorizationParametters
            , AuthorizationParametters authorizationParametters, Ontology.Ontology ontology);

        List<SearchObject> GetObjectDocumentIDByGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria
            , CriteriaSet filterSearchCriteria
            , int maxResultAuthorizationParametters
            , AuthorizationParametters authorizationParametters, Ontology.Ontology ontology);
        #endregion

        #region Object Explorer
        QueryResult RunStatisticalQuery(StatisticalQuery.Query query, Ontology.Ontology ontology, AuthorizationParametters authParams);
        long[] RetrieveObjectIDsByStatisticalQuery(StatisticalQuery.Query query, int PassObjectsCountLimit, Ontology.Ontology ontology, AuthorizationParametters authParams);
        PropertyValueStatistics RetrievePropertyValueStatistics
            (StatisticalQuery.Query query, string exploredPropertyTypeUri, int startOffset, int resultsLimit
            , long minimumCount, Ontology.Ontology ontology, AuthorizationParametters authParams);

        LinkTypeStatistics RetrieveLinkTypeStatistics(StatisticalQuery.Query query, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        long[] RetrieveLinkedObjectIDsByStatisticalQuery(StatisticalQuery.Query query, int PassObjectsCountLimit, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        PropertyBarValues RetrievePropertyBarValuesStatistics(StatisticalQuery.Query query, string numericPropertyTypeUri, long bucketCount, double minValue, double maxValue, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        #endregion

        #region Indexed Concept

        JToken RetrieveEntireDocumentObjectFromSolr(string objDocId);

        JToken RetrieveImageDocumentFromSolr(string docId);

        #endregion

        #region Timeline

        long GetTimelineMaxFrequecyCount(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        DateTime GetTimelineMaxDate(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        DateTime GetTimelineMinDate(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters);

        #endregion
        
        #region TextualSearch

        List<TextualSearch.BaseSearchResult> GetResultsForTextualSearch(TextualSearch.TextualSearchQuery query, AuthorizationParametters authorizationParametters);

        #endregion

        List<SearchProperty> GetDBPropertyByObjectId(long objectId, AuthorizationParametters authorizationParametters);

        SearchObject GetObject(long objectId);

        List<SearchProperty> GetDBPropertyByObjectIds(long[] propertyIds, AuthorizationParametters authorizationParametters);

        List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authorizationParametters);

        void RegisterNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime);

        List<SearchDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs);

        List<SearchObject> GetObjectByIDs(long[] objectIDs);

        long GetLastAsignedDataSourceId();

        long GetLastAsignedObjectId();

        long GetLastAsignedPropertyId();

        long GetLastAsignedRelationId();

        long GetLastAssignedGraphaId();

        List<SearchProperty> GetDBPropertyByObjectIdsWithoutAuthorization(long[] objectIDs);

        List<SearchRelationship> RetrieveRelationships(long[] relationshipIDs);

        List<SearchRelationship> GetRelationships(List<long> relationshipIDs, AuthorizationParametters authorizationParametters);

        SearchGraphArrangement AddSearchGraph(SearchGraphArrangement searchGraphArrangement);

        List<SearchGraphArrangement> GetGraphArrangements(AuthorizationParametters authorizationParametters);

        byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authorizationParametters);

        byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authorizationParametters);

        bool DeleteGraph(int id);

        List<SearchRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs);

        long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission);

        List<SearchDataSourceACL> RetrieveTopNDataSourceACLs(long topN);

        List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids);

        List<SearchRelationship> GetRelationshipsBySourceObject(long objectID, string typeUri, AuthorizationParametters authorizationParametters);

        List<SearchRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authorizationParametters);

        SearchRelationship GetExistingRelationship(string typeUri, long source, long target, int direction, AuthorizationParametters authorizationParametters);

        List<SearchRelationship> GetSourceLink(long objectId, string typeURI, AuthorizationParametters authorizationParametters);

        List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authorizationParametters);
    }
}
