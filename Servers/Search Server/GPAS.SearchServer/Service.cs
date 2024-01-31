using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.Logger;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.IndexChecking;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.SearchServer.Entities.Sync;
using GPAS.SearchServer.Logic;
using GPAS.SearchServer.Logic.Synchronization;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GPAS.SearchServer
{
    public class Service : IService
    {
        private void WriteErrorLog(Exception ex, MemoryStream logStream = null, string logStreamTitle = "")
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex, logStream, logStreamTitle);
        }

        #region Quick-Search
        private const int QuickSearchResultsTreshould = 200;
        public SearchObject[] QuickSearch(string keyword, AuthorizationParametters authorizationParametters)
        {
            try
            {
                QuickSearch quickSearch = new QuickSearch();
                return quickSearch.Search(keyword, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        //#region Search
        //public List<SearchResultModel> Search(SearchModel searchModel, AuthorizationParametters authorizationParametters)
        //{
        //    try
        //    {
        //        QuickSearch quickSearch = new QuickSearch();
        //        var result = quickSearch.Search(searchModel, authorizationParametters);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorLog(ex);
        //        throw;
        //    }
        //}


        //public long GetTotalTextDocResults(SearchModel searchModel, AuthorizationParametters authorizationParametters)
        //{
        //    try
        //    {
        //        QuickSearch quickSearch = new QuickSearch();
        //        var outPutServerSEarch = quickSearch.GetTotalTextDocResults(searchModel, authorizationParametters);
        //        return outPutServerSEarch;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorLog(ex);
        //        throw;
        //    }
        //}
        //#endregion

        #region Filter Search
        public List<SearchObject> PerformFilterSearch(byte[] stream, int? count, AuthorizationParametters authorizationParametters)
        {
            try
            {
                var items = Logic.FilterSearch.Instance.PerformFilterSearch(stream, count, authorizationParametters);
                return items;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<long> PerformSelectMatching(byte[] stream, List<long> ObjectIDs, AuthorizationParametters authorizationParametters)
        {
            try
            {
                var items = Logic.FilterSearch.Instance.PerformSelectMatching(stream, ObjectIDs, authorizationParametters);
                return items;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Search-Around
        /// <summary>
        /// این تابع بر اساس نوع ویژگی و مقدار جست و جو انجام می دهد.
        /// </summary>
        /// <param name="type"> نوع ویژگی را مشخص می کند. </param>
        /// <param name="value">  مقدار ویژگی را تعیین می کند. </param>
        /// <returns>    لیستی از SearchDBProperty به عنوان خروجی برمی گرداند.   </returns>
        public List<PropertiesMatchingResults> FindPropertiesSameWith(KProperty[] properties, int totalResultsThreshold, AuthorizationParametters authorizationParametters)
        {
            try
            {
                SearchAround findProperties = new SearchAround();
                return findProperties.FindPropertiesSameWith(properties, totalResultsThreshold, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Data Synchronization
        public Dispatch.Entities.Publish.SynchronizationResult SyncPublishChanges(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, long dataSourceID, bool isContinousPublish = false)
        {
            MemoryStream consoleOutputStream = null;
            ProcessLogger logger = null;
            try
            {
                consoleOutputStream = new MemoryStream();
                logger = new ProcessLogger();
                logger.Initialization(consoleOutputStream);
                IndexingProvider indexingProvider = new IndexingProvider();
                Task<bool> syncTask = indexingProvider.SynchronizePublishChanges(addedConcepts, modifiedConcepts, dataSourceID, isContinousPublish, logger);

                Task.WaitAll(syncTask);

                var utility = new StreamUtility();
                var result = new Dispatch.Entities.Publish.SynchronizationResult()
                {
                    IsCompletelySynchronized = syncTask.Result,
                    SyncronizationLog = utility.GetStringFromStream(consoleOutputStream)
                };
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, consoleOutputStream, "Console Output");
                throw;
            }
            finally
            {
                if (logger != null)
                    logger.Finalization();
                if (consoleOutputStream != null)
                    consoleOutputStream.Close();
            }
        }
        public Dispatch.Entities.Publish.SynchronizationResult SynchronizeDataSource(DataSourceInfo dataSourceInfo)
        {
            MemoryStream consoleOutputStream = null;
            ProcessLogger logger = null;
            try
            {
                consoleOutputStream = new MemoryStream();
                logger = new ProcessLogger();
                logger.Initialization(consoleOutputStream);
                IndexingProvider indexingProvider = new IndexingProvider();
                Task<bool> syncTask = indexingProvider.SynchronizeDataSource(dataSourceInfo, logger);

                Task.WaitAll(syncTask);

                var utility = new StreamUtility();
                var result = new Dispatch.Entities.Publish.SynchronizationResult()
                {
                    IsCompletelySynchronized = syncTask.Result,
                    SyncronizationLog = utility.GetStringFromStream(consoleOutputStream)
                };
                return result;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex, consoleOutputStream, "Console Output");
                throw;
            }
            finally
            {
                if (logger != null)
                    logger.Finalization();
                if (consoleOutputStream != null)
                    consoleOutputStream.Close();
            }
        }

        public List<DataSourceInfo> GetDataSources(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters)
        {
            try
            {
                DataSourceProvider dataSourceProvider = new DataSourceProvider();
                return dataSourceProvider.GetDataSources(dataSourceType, star, count, filter, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceInfo> GetAllDataSources(int count, string filter, AuthorizationParametters authorizationParametters)
        {
            try
            {
                DataSourceProvider dataSourceProvider = new DataSourceProvider();
                return dataSourceProvider.GetAllDataSources(count, filter, authorizationParametters);
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
                IndexingProvider indexingProvider = new IndexingProvider();
                indexingProvider.FinalizeContinousIndexing();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
#if DEBUG
        public void ResetIndexes(bool DeleteExistingIndexes)
        {
            try
            {
                var indexProvider = new IndexingProvider();
                indexProvider.DeleteExistingIndexes = DeleteExistingIndexes;
                indexProvider.ResetAllIndexes().Wait();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
#endif

        public bool IsDataIndicesStable()
        {
            IndexingProvider dataSyncronizer = new IndexingProvider();
            return dataSyncronizer.GetStatusOfStablity();
        }
        public void RemoveSearchIndexes()
        {
            try
            {
                var dataSyncronizer = new IndexingProvider();
                dataSyncronizer.DeleteExistIndexes();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Search-Server Management
        public void Optimize()
        {
            try
            {
                var optimizer = new Optimizer();
                optimizer.Optimize();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        public string GetDocumentPossibleExtractedContent(long docID)
        {
            try
            {
                var contentExtractionProvider = new DocumentContentExtractionProvieder();
                return contentExtractionProvider.GetDocumentPossibleExtractedContent(docID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void AddNewGroupFieldsToSearchServer(List<string> newGroupsName)
        {
            try
            {
                SchemaChangeProvider schemaSynchronizer = new SchemaChangeProvider();
                schemaSynchronizer.AddNewGroupFieldsToSearchServer(newGroupsName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #region Geo Search
        public List<SearchObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            Logic.GeoSearch geoSearch = new Logic.GeoSearch();
            return geoSearch.PerformGeoCircleSearch(circleSearchCriteria, maxResult, authorizationParametters);
        }

        public List<SearchObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            Logic.GeoSearch geoSearch = new Logic.GeoSearch();
            return geoSearch.PerformGeoPolygonSearch(polygonSearchCriteria, maxResult, authorizationParametters);
        }

        public List<SearchObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            Logic.GeoSearch geoSearch = new Logic.GeoSearch();
            return geoSearch.PerformGeoCircleFilterSearch(circleSearchCriteria, filterSearchCriteria, maxResult, authorizationParametters);
        }

        public List<SearchObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            Logic.GeoSearch geoSearch = new Logic.GeoSearch();
            return geoSearch.PerformGeoPolygonFilterSearch(polygonSearchCriteria, filterSearchCriteria, maxResult, authorizationParametters);
        }
        #endregion

        #region ImageAnalytics
        public List<BoundingBox> FaceDetection(byte[] imageFile, string extention, AuthorizationParametters authorizationParametters)
        {
            try
            {
                ImageProcessingProvider imageProcessingProvider = new ImageProcessingProvider();
                return imageProcessingProvider.FaceDetection(imageFile, extention, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RetrievedFace> FaceRecognition(byte[] imageFile, string extention, List<BoundingBox> boundingBoxes, int count, AuthorizationParametters authorizationParametters)
        {
            try
            {
                ImageProcessingProvider imageProcessingProvider = new ImageProcessingProvider();
                return imageProcessingProvider.FaceRecoginition(imageFile, extention, boundingBoxes, count, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool IsMachneVisonServiceInstalled()
        {
            ImageProcessingProvider imageProcessingProvider = new ImageProcessingProvider();
            return imageProcessingProvider.IsMachneVisonServiceInstalled();
        }
        #endregion

        #region Object Explorer
        public QueryResult RunStatisticalQuery(byte[] queryByteArray, AuthorizationParametters authParams)
        {
            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RunQuery(queryByteArray, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long[] RetrieveObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit, AuthorizationParametters authParams)
        {
            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RetrieveObjectIDsByQuery(queryByteArray, PassObjectsCountLimit, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public PropertyValueStatistics RetrievePropertyValueStatistics
            (byte[] queryByteArray, string exploredPropertyTypeUri, int startOffset, int resultsLimit
            , long minimumCount, AuthorizationParametters authParams)
        {
            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RetrievePropertyValueStatistics
                    (queryByteArray, exploredPropertyTypeUri, startOffset, resultsLimit, minimumCount, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray, AuthorizationParametters authParams)
        {

            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RetrieveLinkTypeStatistics(queryByteArray, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit, AuthorizationParametters authorizationParametters)
        {
            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RetrieveLinkedObjectIDsByStatisticalQuery(queryByteArray, PassObjectsCountLimit, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public PropertyBarValues RetrievePropertyBarValuesStatistics(byte[] queryByteArray, string numericPropertyTypeUri, long bucketCount, double minValue, double maxValue, AuthorizationParametters authorizationParametters)
        {
            try
            {
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
                return statisticalQueryProvider.RetrievePropertyBarValuesStatistics(queryByteArray, numericPropertyTypeUri, bucketCount, minValue, maxValue, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Indexed Concept

        public SearchIndexCheckingResult IndexChecking(SearchIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            IndexCheckingProvider indexCheckingProvider = new IndexCheckingProvider();
            return indexCheckingProvider.StartSearchIndexChecking(input, authorizationParameters);
        }

        #endregion

        public void IsAvailable()
        {
        }

        #region Timeline

        public long GetTimeLineMaxFrequecyCount(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            try
            {
                TimelineProvider timelineProvider = new TimelineProvider();
                return timelineProvider.GetTimeLineMaxFrequecyCount(propertiesTypeUri, binLevel, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public DateTime GetTimeLineMaxDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            try
            {
                TimelineProvider timelineProvider = new TimelineProvider();
                return timelineProvider.GetTimeLineMaxDate(propertiesTypeUri, binLevel, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public DateTime GetTimeLineMinDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            try
            {
                TimelineProvider timelineProvider = new TimelineProvider();
                return timelineProvider.GetTimeLineMinDate(propertiesTypeUri, binLevel, authorizationParametters);

            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        //private GPAS.SearchServer.Entities.SearchModel ConvertWorkSpaceSearchModelToSearchServerSearchModel(GPAS.Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    Entities.SearchModel SearchServerEntity = new Entities.SearchModel
        //    {
        //        TypeSearch = searchModel.TypeSearch,
        //        ExactKeyWord = searchModel.ExactKeyWord,
        //        KeyWordSearch = searchModel.KeyWordSearch,
        //        AnyWord = searchModel.AnyWord,
        //        NoneWord = searchModel.NoneWord,
        //        ImportDateOf = searchModel.ImportDateOf,
        //        ImportDateUntil = searchModel.ImportDateUntil,
        //        SortOrder = searchModel.SortOrder,
        //        SortOrderType = searchModel.SortOrderType,
        //        Language = searchModel.Language,
        //        FileType = searchModel.FileType,
        //        SearchIn = searchModel.SearchIn,
        //        Topic = searchModel.Topic,
        //        CreationDateOF = searchModel.CreationDateOF,
        //        CreationDateUntil = searchModel.CreationDateUntil,
        //        FileSizeOF = searchModel.FileSizeOF,
        //        FileSizeUntil = searchModel.FileSizeUntil
        //    };
        //    return SearchServerEntity;
        //}

        public int FindCount(string filter)
        {
            throw new NotImplementedException();
        }

        #region TextualSearch

        public List<TextualSearch.BaseSearchResult> PerformTextualSearch(byte[] stream, AuthorizationParametters authorizationParametters)
        {
            try
            {
                Logic.Search.TextualSearchProvider textualSearchProvider = new Logic.Search.TextualSearchProvider();
                return textualSearchProvider.PerformFilterSearch(stream, authorizationParametters);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }



        #endregion


        public List<SearchProperty> GetDBPropertyByObjectId(long objectId, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetDBPropertyByObjectId(objectId, authorizationParametters);
        }

        public SearchObject GetObject(long objectId)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetObject(objectId);
        }

        public List<SearchProperty> GetDBPropertyByObjectIds(long[] propertyIds, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetDBPropertyByObjectIds(propertyIds, authorizationParametters);
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, specifiedPropertyTypeUris, authorizationParametters);
        }

        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            generalRequestSearchProvider.RegisterNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
        }

        public List<SearchDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.RetrieveDataSourceACLs(dataSourceIDs);
        }

        public List<SearchObject> GetObjectByIDs(long[] objectIDs)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetObjectByIDs(objectIDs);
        }

        public long GetLastAsignedDataSourceId()
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetLastAsignedDataSourceId();
        }

        public long GetLastAsignedObjectId()
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetLastAsignedObjectId();
        }

        public long GetLastAsignedPropertyId()
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetLastAsignedPropertyId();
        }

        public long GetLastAsignedRelationId()
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetLastAsignedRelationId();
        }

        public long GetLastAssignedGraphaId()
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetLastAssignedGraphaId();
        }

        public List<SearchProperty> GetDBPropertyByObjectIdsWithoutAuthorization(long[] objectIDs)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetDBPropertyByObjectIdsWithoutAuthorization(objectIDs);
        }

        public List<SearchRelationship> RetrieveRelationships(long[] relationshipIDs)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.RetrieveRelationships(relationshipIDs);
        }

        public List<SearchRelationship> GetRelationships(List<long> relationshipIDs, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetRelationships(relationshipIDs, authorizationParametters);
        }

        public SearchGraphArrangement SaveNew(SearchGraphArrangement dbGraphArrangement)
        {
            GraphManager graphManager = new GraphManager();
            return graphManager.SaveNew(dbGraphArrangement);
        }

        public List<SearchGraphArrangement> GetGraphArrangements(AuthorizationParametters authorizationParametters)
        {
            GraphManager graphManager = new GraphManager();
            return graphManager.GetGraphArrangements(authorizationParametters);
        }

        public byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authorizationParametters)
        {
            GraphManager graphManager = new GraphManager();
            return graphManager.GetGraphImage(dbGrapharagmentID, authorizationParametters);
        }

        public byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authorizationParametters)
        {
            GraphManager graphManager = new GraphManager();
            return graphManager.GetGraphArrangementXML(dbGraphArrangementID, authorizationParametters);
        }

        public bool DeleteGraph(int id)
        {
            GraphManager graphManager = new GraphManager();
            return graphManager.DeleteGraph(id);
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(objectIDs);
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetSubsetOfConceptsByPermission(conceptType, IDs, groupNames, minimumPermission);
        }

        public List<SearchDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.RetrieveTopNDataSourceACLs(topN);
        }

        public List<SearchRelationship> GetRelationshipsBySourceObject(long objectID, string typeUri, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetRelationshipsBySourceObject(objectID, typeUri, authorizationParametters);
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetRelationshipsBySourceOrTargetObject(objectIDs, authorizationParametters);
        }

        public SearchRelationship GetExistingRelationship(string typeUri, long source, long target, int direction, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetExistingRelationship(typeUri, source, target, direction, authorizationParametters);
        }

        public List<SearchRelationship> GetSourceLink(long objectId, string typeUri, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetSourceLink(objectId, typeUri, authorizationParametters);
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authorizationParametters)
        {
            GeneralRequestSearchProvider generalRequestSearchProvider = new GeneralRequestSearchProvider();
            return generalRequestSearchProvider.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyTypeUri, propertyValue,  authorizationParametters);
        }
    }
}
