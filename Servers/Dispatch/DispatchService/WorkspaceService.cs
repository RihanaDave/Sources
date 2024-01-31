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
using GPAS.Dispatch.Logic;
using GPAS.Dispatch.Logic.Datalake;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.Logger;
using GPAS.SearchAround;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GPAS.Dispatch
{
    /// <summary>
    /// سرویسی که توسط Dispatch Server به فضای کاری ارائه می شود.
    /// همه درخواست های کاربر توسط این سرور توزیع می شود.
    /// </summary>
    class WorkspaceService : IWorkspaceService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        /// <summary>
        /// آیکون های موجود را برای فضای کاری ارسال می کند.
        /// این تابع آیکون ها را به صورت فایل زیپ پاس می دهد.
        /// </summary>
        /// <returns> آیکون ها به صورت فایل زیپ </returns>
        public Stream GetIcon()
        {
            try
            {
                DispatchFileProvider fileProvider = new DispatchFileProvider();
                return fileProvider.GetIconPack();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// آنتولوژی که یک فایل ایکس ام ال است را برای فضای کاری ارسال می کند.
        /// </summary>
        /// <returns> آنتولوژی </returns>
        public Stream GetOntology()
        {
            try
            {
                DispatchFileProvider fileProvider = new DispatchFileProvider();
                return fileProvider.GetOntology();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// لیستی از ویژگی های مربوط به شی مورد نظر را برمی گرداند.
        /// </summary>
        /// <param name="dbObject">     یک شی از نوع KObject دریافت می کند.   </param>
        /// <returns>  KProperty را برمی گرداند لیستی از    </returns>
        public List<KProperty> GetPropertyForObject(KObject kObject)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetPtoperty(kObject);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KProperty> GetPropertyForObjects(long[] dbObjIDs)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetPropertyForObjects(dbObjIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KProperty> GetPropertyListById(long[] dbOPropertyIDs)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetPropertiesById(dbOPropertyIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RelationshipBaseKlink> GetLinksSourcedByObject(KObject kObject, string relationshipTypeURI)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetSourceLink(kObject, relationshipTypeURI);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع اشیا در پایگاه داده که با لیست ورودی برابر است را بر می گرداند.
        /// </summary>
        /// <param name="dbObjectIDs">   لیستی از آیدی اشیا.   </param>
        /// <returns>    لیستس از نوع KObject را برمی گرداند.    </returns>
        public List<KObject> QuickSearch(string keyword)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchProvider searchProvider = new SearchProvider(callerUserName);
                return searchProvider.QuickSearch(keyword);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        //public List<SearchResultModel> Search(SearchModel searchModel)
        //{
        //    try
        //    {
        //        string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
        //        SearchProvider searchProvider = new SearchProvider(callerUserName);
        //        return searchProvider.Search(searchModel);

        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorLog(ex);
        //        throw;
        //    }
        //}

        //public long GetTotalTextDocResults(SearchModel searchModel)
        //{
        //    try
        //    {
        //        string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
        //        SearchProvider searchProvider = new SearchProvider(callerUserName);
        //        return searchProvider.GetTotalTextDocResults(searchModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteErrorLog(ex);
        //        throw;
        //    }
        //}

        #region File Repository
        #region Medias
        /// <summary>
        /// این تابع لیست پوشه ها و فایل ها را در مسیر مورد نظر  بر می گرداند.
        /// </summary>
        /// <param name="path"> رشته مسیر را به عنوان ورودی دریافت می کند.</param>
        /// <returns> لیستی از DirectoryContent را بر می گرداند.</returns>
        public List<Entities.DirectoryContent> GetMediaPathContent(string path)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.GetMediaPathContent(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool DeleteMediaDirectory(string path)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.DeleteMediaDirectory(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع پوشه مورد نظر را به مکان با اسم مورد انتقال می دهد.
        /// </summary>
        /// <param name="sourcePath">این پارامتر پوشه مورد نظر برای تغییر را دریافت می کند.</param>
        /// <param name="targetPath">این پارامتر پوشه مقصد را برای انتقال و تغییر نام دریافت می کند.</param>
        /// <returns></returns>
        public bool RenameMediaDirectory(string sourcePath, string targetPath)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.RenameMediaDirectory(sourcePath, targetPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// در مسیر مورد نظر یک پوشه ایجاد می کند.
        /// </summary>
        /// <param name="path"> رشته آدرس را از کاربر دریافت میکند</param>
        /// <returns> در صورتی که قبلا در مسیر مورد نظر پوشه وجود داشت مقدار False و در غیر این صورت پوشه را ایجاد و مقدار True را ارسال می کند.</returns>
        public bool CreateMediaDirectory(string path)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.CreateMediaDirectory(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        ///ین تابع مسیری فایلی را از ورودی دریافت کرده و آن را از سرور فایل دانلود کرده و فایل را به صورت رشته ای از بایت ها برای کاربر بر می گرداند. 
        /// </summary>
        /// <param name="filePath">این پارامتر مسیر فایلی را که قصد دانلود آن را داریم ار ورودی دریافت می کند</param>
        /// <returns> رشته از بایت های فایل درخواست شده کاربر را برای کاربر ارسال می کند./</returns>
        public byte[] DownloadMediaFile(string filePath)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.DownloadMediaFile(filePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع فایل را دریافت کرده و در مکان تعیین شده آپلود می کند.
        /// </summary>
        /// <param name="fileToUpload"> این پارامتر رشته ای ز بایت های تابع را از کاربر دریافت می کتند</param>
        /// <param name="fileName">این پارامتر نام فایل و پسنود ان را از کاربر دریافت  می کند </param>
        /// <param name="targetPath">این پارامتر مسیری را که فایل در آنجا آپلوذ می شود را تعیین می کند </param>
        /// <returns> خروجی این فایل در صورتی که آپلود با موفقیت انجام بپذیرد مقدار TRUE را بر می گرداند و در صورتی فایل به دلیلی آپلود نشود مثدار False را بر می گرداند.</returns>
        public bool UploadMediaFile(byte[] fileToUpload, string fileName, string targetPath)
        {
            try
            {
                MediaProvider provider = new MediaProvider();
                return provider.UploadMediaFile(fileToUpload, fileName, targetPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Data-Sources & Documents

        
        public void UploadDataSourceFileByName(string docName, byte[] docContent)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDataSourceFileByName(docName, docContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void UploadDocumentFile(long docID, byte[] docContent)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDocumentFile(docID, docContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDataSourceFile(dataSourceID, dataSourceContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadFileAsDocumentAndDataSource(byte[] fileContent, long docID, long dataSourceID)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadFileAsDocumentAndDataSource(fileContent, docID, dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDocumentFromJobShare(long docID, string docJobSharePath)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDocumentFromJobShare(docID, docJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                provider.UploadDataSourceFromJobShare(dataSourceID, dataSourceJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public byte[] DownloadDocumentFile(long docID)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                return provider.DownloadDocumentFile(docID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public byte[] DownloadDataSourceFile(long dataSourceID)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                return provider.DownloadDataSourceFile(dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public byte[] DownloadDataSourceFileByName(string dataSourceName)
        {
            try
            {
                DataSourceProvider provider = new DataSourceProvider();
                return provider.DownloadDataSourceFileByName(dataSourceName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion
        #endregion

        /// <summary>
        /// این تابع چینش گراف ها را دریاقت کرده و در پایگاه داده ذخیره می کند.
        /// </summary>
        /// <param name="title">    .عنوان چینش گراف است    </param>
        /// <param name="description">   توضیح در  رابطه با چینش گراف است.   </param>
        /// <param name="GraphImage">   تصویر چینش گراف است.    </param>
        /// <param name="GraphArrangement">   این پارامتر فایل ایکس ام ال چینش گراف است.  </param>
        /// <param name="nodesCount">   این پارامتر تعداد نود ها ی موجود در گراف است.   </param>
        /// <param name="timeCreated">   این پارامتر زمان ایجاد گراف است   </param>
        /// <returns> یک شی از KGraphArrangement را بر می گرداند. </returns>
        public KGraphArrangement PublishGraph(long id, string title, string description, byte[] GraphImage, byte[] GraphArrangement,
            int nodesCount, string timeCreated, long dataSourceID)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GraphProvider graphProvider = new GraphProvider(callerUserName);
                return graphProvider.CreateGraphArrangement(id, title, description, GraphImage, GraphArrangement, nodesCount, timeCreated, dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع اشیا در پایگاه داده که با لیست ورودی برابر است را بر می گرداند.
        /// </summary>
        /// <param name="dbObjectIDs">   لیستی از آیدی اشیا.   </param>
        /// <returns>    لیستس از نوع KObject را برمی گرداند.    </returns>
        public List<KObject> GetObjectListById(long[] dbObjectIDs)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetObjects(dbObjectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع لیستی از لینک ها را که آیدی آنان در لیست ورودی موجود است را بر می گرداند. 
        /// </summary>
        /// <param name="dbRelationshipIDs">   لیستی از آیدی لینک ها می باشد.     </param>
        /// <returns>  را بر میگرداند. RelationshipBaseKlink لیستی از از نوع    </returns>
        public List<RelationshipBaseKlink> GetRelationshipListById(List<long> dbRelationshipIDs)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetRelationships(dbRelationshipIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع لینک هایی که مبدا آن با شی مورد نظر برابر است را برمی گرداند
        /// </summary>
        /// <param name="objectID">    آی دی شی را تعیین می کند.     </param>
        /// <param name="typeURI">      نوع لینک را مشخص می کند.    </param>
        /// <returns>      لینک از نوع RelationshipBaseKlink را برمی گرداند.     </returns>
        public List<RelationshipBaseKlink> GetRelationshipsBySourceObject(long objectID, string typeURI)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetRelationshipsBySourceObject(objectID, typeURI);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        /// این تابع همه چینش گراف های موجود در پایگاه داده را بر می گرداند.    
        /// </summary>
        /// <returns>   لیستی از KGraphArrangement را بر می گرداند.    </returns>
        public List<KGraphArrangement> GetPublishedGraphs()
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GraphProvider graphProvider = new GraphProvider(callerUserName);
                return graphProvider.GetGraphArrangement();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع تصویر متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    تصویر را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetPublishedGraphImage(int kGraphArrangementID)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GraphProvider graphProvider = new GraphProvider(callerUserName);
                return graphProvider.GetGraphImage(kGraphArrangementID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع چینش XML متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    چینش XML را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetPublishedGraph(int kGraphArrangementID)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GraphProvider graphProvider = new GraphProvider(callerUserName);
                return graphProvider.GetGraphArrangementXML(kGraphArrangementID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع آنتولوژی به روز رسانی شده را از کاربر دریافت می کند و جایگزین انتولوژی قبلی می کند.
        /// </summary>
        /// <param name="reader">این پارامتر رشته ای از فایل انتولوزی را دریافت می کند.</param>
        public void UpdateOntologyFile(Stream reader)
        {
            try
            {
                DispatchFileProvider fileProvider = new DispatchFileProvider();
                fileProvider.UpdateOntology(reader);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// این تابع یک آیدی ابجت را ار ورودی دریافت می کند و آدرس فاسل های جند راسانه ای منتب با این آیدی را به عنوان خروجی نمایش می دهد
        /// </summary>
        /// <param name="objectID">این پارامتر یک آیدی شی را به عنوان ورودی دریافت میکند</param>
        /// <returns>این تابع لیستی از KMedia را به عنوان خروجی ارسال می کند</returns>

        public List<KMedia> GetMediaUrisForObject(long objectID)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetMediaForObject(objectID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// یک گراف را از پایگاه داده حذف می کند و در صورت موفقیت آمیز بودن true برمی گرداند.
        /// </summary>
        /// <param name="graphID"></param>
        /// <returns></returns>
        public bool DeletePublishedGraph(int graphID)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GraphProvider gProvider = new GraphProvider(callerUserName);
                return gProvider.DeleteGraph(graphID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public JobRequest[] GetJobRequests()
        {
            try
            {
                var jobProvider = new JobsProvider();
                return jobProvider.GetJobRequests();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public RelationshipBasedResult FindRelatedEntities(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAreound = new SearchAroundProvider(callerUserName);
                return searchAreound.FindRelatedEntities(searchedVertices, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public RelationshipBasedResult FindRelatedDocuments(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAreound = new SearchAroundProvider(callerUserName);
                return searchAreound.FindRelatedDocuments(searchedVertices, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public RelationshipBasedResult FindRelatedEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAreound = new SearchAroundProvider(callerUserName);
                return searchAreound.FindRelatedEvents(searchedVertices, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public EventBasedResult FindRelatedEntitiesAppearedInEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAreound = new SearchAroundProvider(callerUserName);
                return searchAreound.FindRelatedEntitiesApearedInEvents(searchedVertices, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KProperty> GetSpecifiedPropertiesOfObjectsByTypes(long[] objectsId, string[] propertiesType)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetPropertiesByOwnersAndTypes(objectsId, propertiesType);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(long[] objectIDs, string propertiesTypeUri, string propertiesValue)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetPropertiesByOwnersAndTypeAndValue(objectIDs, propertiesTypeUri, propertiesValue);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KObject> PerformFilterSearch(byte[] stream, int? count)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                FilterSearchProvider filterSearchProvider = new FilterSearchProvider(callerUserName);
                return filterSearchProvider.PerformFilterSearch(stream, count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<long> PerformSelectMatching(byte[] stream, List<long> ObjectIDs)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                FilterSearchProvider filterSearchProvider = new FilterSearchProvider(callerUserName);
                return filterSearchProvider.PerformSelectMatching(stream, ObjectIDs.ToArray());
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public PublishResult Publish(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept,
            long dataSourceID, bool isContinousPublish = false)
        {
            try
            {
                PublishProvider publishProvider = new PublishProvider();
                return publishProvider.Publish(addedConcept, modifiedConcept, dataSourceID, isContinousPublish);
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
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.FinalizeContinousPublish();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool CanPerformNewPublish()
        {
            try
            {
                PublishProvider publishProvider = new PublishProvider();
                return publishProvider.CanPerformNewPublish();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        /// این تابع بر اساس نوع ویژگی و مقدار جست و جو انجام می دهد.
        /// </summary>
        /// <param name="type"> نوع ویژگی را مشخص می کند. </param>
        /// <param name="value">  مقدار ویژگی را تعیین می کند. </param>
        /// <returns>    لیستی از KProperty به عنوان خروجی برمی گرداند.   </returns>
        public PropertyBasedResult FindPropertiesSameWith(KProperty[] properties, int loadNResults, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAroundProvider = new SearchAroundProvider(callerUserName);
                return searchAroundProvider.FindPropertiesSameWith(properties, loadNResults, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RegisterNewImportRequests(SemiStructuredDataImportRequestMetadata[] requestsData)
        {
            try
            {
                ImportProvider importProvider = new ImportProvider();
                importProvider.RegisterNewImportRequests(requestsData);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public string[] GetUriOfDatabasesForImport()
        {
            try
            {
                ImportProvider importRequestManager = new ImportProvider();
                return importRequestManager.GetUriOfDatabasesForImport();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public DataSet GetTablesAndViewsOfDatabaseForImport(string dbForImportURI)
        {
            try
            {
                ImportProvider importRequestManager = new ImportProvider();
                return importRequestManager.GetTablesAndViewsOfDatabaseForImport(dbForImportURI);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public RelationshipBaseKlink GetExistingRelationship(string typeURI, long source, long target, LinkDirection direction)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                RepositoryProvider repositoryProvider = new RepositoryProvider(callerUserName);
                return repositoryProvider.GetExistingRelationship(typeURI, source, target, direction);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public byte[] GetMapTileImage(string tileSource, int zoomLevel, long x, long y)
        {
            try
            {
                var mapProvider = new MapProvider();
                return mapProvider.GetMapTileImage(tileSource, zoomLevel, x, y);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public string[] GetMapTileSources()
        {
            try
            {
                var mapProvider = new MapProvider();
                return mapProvider.GetMapTileSources();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public GeographicalLocationModel GetGeoLocationBaseOnIP(string ip)
        {
            try
            {
                return new Logic.GeographicalStaticLocationProvider().GetGeoLocationBaseOnIP(ip);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool InsertGeoSpecialInformationBasedOnIP(string ip, double latitude, double longitude)
        {
            try
            {
                return new Logic.GeographicalStaticLocationProvider().InsertGeoSpecialInformationBasedOnIP(ip, latitude, longitude);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region IdGenerators

        #region انتساب شناسه تکی
        /// <summary></summary>
        /// <returns>شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewObjectId()
        {
            try
            {
                return IdGenerators.ObjectIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewPropertyId()
        {
            try
            {
                return IdGenerators.PropertyIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewRelationId()
        {
            try
            {
                return IdGenerators.RelationIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewMediaId()
        {
            try
            {
                return IdGenerators.MediaIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetNewInvestigationId()
        {
            try
            {
                return IdGenerators.DataInvestigationGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long GetNewGraphId()
        {
            try
            {
                return IdGenerators.GraphIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetNewDataSourceId()
        {
            try
            {
                return IdGenerators.DataSourceIdGenerator.GenerateNewID();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        #region انتساب شناسه دسته ای
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewObjectIdRange(long count)
        {
            try
            {
                return IdGenerators.ObjectIdGenerator.GenerateIDRange(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewPropertyIdRange(long count)
        {
            try
            {
                return IdGenerators.PropertyIdGenerator.GenerateIDRange(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewRelationIdRange(long count)
        {
            try
            {
                return IdGenerators.RelationIdGenerator.GenerateIDRange(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary></summary>
        /// <returns>مقدار آخرین شناسه انتساب داده شده را برمی‌گرداند</returns>
        public long GetNewMediaIdRange(long count)
        {
            try
            {
                return IdGenerators.MediaIdGenerator.GenerateIDRange(count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion
        #endregion

        public bool Authenticate(string userName, string password)
        {
            UserAccountManagement userAccountControl = new UserAccountManagement();
            return userAccountControl.Authenticate(userName, password);
        }

        public string GetDispatchCurrentDateTime()
        {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public string GetDocumentPlaneText(long docID)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.GetDocumentPlaneText(docID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DetectedLanguage> DetectLanguage(string content)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.DetectLanguage(content);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public TagCloudKeyPhrase[] GetTagCloud(string content)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.GetTagCloud(content);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public TagCloudKeyPhrase[] GetLanguageTagCloud(string content, Language language)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.GetLanguageTagCluod(content, language);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<string> GetSummarize(SummarizationRequest summarizationRequest)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.GetSummarize(summarizationRequest);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<string> GetLanguageSummarize(SummarizationRequest summarizationRequest, Language lang)
        {
            try
            {
                NLPProvider nlpProvider = new NLPProvider();
                return nlpProvider.GetLanguageSummarize(summarizationRequest, lang);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool IsNLPServiceInstalled()
        {
            bool result = bool.Parse(ConfigurationManager.AppSettings["NLPServiceInstalled"]);
            return result;
        }

        #region سرویس‌های مربوط به دریاچه داده
        public void StartStreamingIngestion(StreamingIngestion streamingIngestion)
        {
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();
                dataLakeProvider.StartListeningToSpecificPortNumber(streamingIngestion);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void StopStreamingIngestion(StreamingIngestion streamingIngestion)
        {
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();
                dataLakeProvider.StopListeningToSpecificPortNumber(streamingIngestion);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<string> GetJobsStatus()
        {
            List<string> result = null;
            try
            {
                JobProvider jobProvider = new JobProvider();
                result = jobProvider.GetJobsStatus().ToList();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            return result;
        }

        public List<string> GetStreamJobsStatus()
        {
            List<string> result = null;
            try
            {
                JobProvider jobProvider = new JobProvider();
                result = jobProvider.GetStreamJobsStatus().ToList();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            return result;
        }

        public void InsertFileIngestionJobStatus(IngestionFile ingestingFile)
        {
            try
            {
                JobProvider jobProvider = new JobProvider();
                jobProvider.InsertFileIngestionJobStatus(ingestingFile);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void InsertStreamIngestionStartStatus(StreamingIngestion streamingIngestion)
        {
            try
            {
                JobProvider jobProvider = new JobProvider();
                jobProvider.InsertStreamIngestionStartStatus(streamingIngestion);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void InsertStreamIngestionStopStatus(StreamingIngestion streamingIngestion)
        {
            try
            {
                JobProvider jobProvider = new JobProvider();
                jobProvider.InsertStreamIngestionStopStatus(streamingIngestion);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool IsListenProcessorExist(StreamingIngestion streamingIngestion)
        {
            bool result = false;
            try
            {
                JobProvider jobProvider = new JobProvider();
                result = jobProvider.IsListenProcessorExist(streamingIngestion);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            return result;
        }

        public List<string> GetPreviewDataFromDatalake(string category, string dateTime)
        {
            List<string> result = null;
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();
                result = dataLakeProvider.GetPreviewDataRelatedToSelectedCategoryAndDateTime(category, dateTime).ToList();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            return result;
        }

        public string[] GetDatalakeSlice(string category, string dateTime, List<SearchCriteria> searchCriterias)
        {
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();
                return dataLakeProvider.DownloadFileFromDataLake(category, dateTime, searchCriterias);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<string> GetDatalakeCategories(string path)
        {
            try
            {
                HadoopManager hadoopManager = new HadoopManager();
                return hadoopManager.GetListDirectory(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<string> GetDatalakeSliceHeaders(string category, string dateTime)
        {
            List<string> result = new List<string>();
            try
            {
                DataLakeProvider dataLakeProvider = new DataLakeProvider();

                return dataLakeProvider.GetHeaders(category, dateTime).ToList().Distinct().ToList();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        public List<GroupInfo> GetGroups()
        {
            try
            {
                GroupManagement groupManagement = new GroupManagement();
                return groupManagement.GetGroups();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RegisterNewDataSourceToRepositoryServer(long dsId, string name, DataSourceType type, ACL acl, string description)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                userAccountControlProvider.RegisterNewDataSourceToRepositoryServer(dsId, name, type, acl, description, callerUserName,
                    DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void SynchronizeNewDataSourceInSearchServer(long dsId, string name, DataSourceType type, ACL acl, string description)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                userAccountControlProvider.SynchronizeNewDataSourceInSearchServer(dsId, name, type, acl, description, callerUserName,
                    DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<GroupClassificationBasedPermission> GetClassificationBasedPermissionForGroups(string[] groupNames)
        {
            try
            {
                GroupManagement groupManagement = new GroupManagement();
                return groupManagement.GetClassificationBasedPremissionForGroups(groupNames);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceACL> GetDataSourceACL(long[] dataSourceIDs)
        {
            try
            {
                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                return userAccountControlProvider.RetriveDataSourceACLs(dataSourceIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public string[] GetGroupsOfUser(string username)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                return groupMembershipManagement.GetGroupsOfUser(username);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public Tuple<long[], long[]> GetReadableSubsetOfConcepts(long[] objIDs, long[] relationshipIDs, string[] groupNames)
        {
            try
            {
                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                return userAccountControlProvider.GetReadableSubsetOfConcepts(objIDs, relationshipIDs, groupNames);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region Geo Search

        public List<KObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GeoSearchProvider searchAreound = new GeoSearchProvider(callerUserName);
                return searchAreound.PerformGeoCircleSearch(circleSearchCriteria, maxResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<KObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GeoSearchProvider searchAreound = new GeoSearchProvider(callerUserName);
                return searchAreound.PerformGeoPolygonSearch(polygonSearchCriteria, maxResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GeoSearchProvider searchAreound = new GeoSearchProvider(callerUserName);
                return searchAreound.PerformGeoCircleFilterSearch(circleSearchCriteria, filterSearchCriteria, maxResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<KObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                GeoSearchProvider searchAreound = new GeoSearchProvider(callerUserName);
                return searchAreound.PerformGeoPolygonFilterSearch(polygonSearchCriteria, filterSearchCriteria, maxResult);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        public CustomSearchAroundResult PerformCustomSearchAround(Dictionary<string, long[]> searchedVertices, 
            byte[] serializedCustomSearchAroundCriteria, int totalResultsThreshold)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                SearchAroundProvider searchAroundProvider = new SearchAroundProvider(callerUserName);
                return searchAroundProvider.PerformCustomSearchAround(searchedVertices, serializedCustomSearchAroundCriteria, totalResultsThreshold);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceInfo> GetDataSources(long dataSourceType, int star, string filter)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                DataSourceProvider dataSourceProvider = new DataSourceProvider(callerUserName);
                return dataSourceProvider.GetDataSources(dataSourceType, star, filter);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceInfo> GetAllDataSources(string filter)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                DataSourceProvider dataSourceProvider = new DataSourceProvider(callerUserName);
                return dataSourceProvider.GetAllDataSources(filter);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<BoundingBox> FaceDetection(byte[] imageFile, string extention)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                ImageAnalyticsProvider imageAnalyticsProvider = new ImageAnalyticsProvider(callerUserName);
                return imageAnalyticsProvider.FaceDetection(imageFile, extention);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<RetrievedFaceKObject> FaceRecognition(byte[] imageFile, string extention, List<BoundingBox> boundingBoxs, int count)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                ImageAnalyticsProvider imageAnalyticsProvider = new ImageAnalyticsProvider(callerUserName);
                return imageAnalyticsProvider.FaceRecognition(imageFile, extention, boundingBoxs, count);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool IsMachneVisonServiceInstalled()
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                ImageAnalyticsProvider imageAnalyticsProvider = new ImageAnalyticsProvider(callerUserName);
                return imageAnalyticsProvider.IsMachneVisonServiceInstalled();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }


        public QueryResult RunStatisticalQuery(byte[] queryByteArrayy)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RunQuery(queryByteArrayy);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long[] RetrieveObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RetrieveObjectIDsByQuery(queryByteArray, PassObjectsCountLimit);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public PropertyValueStatistics RetrievePropertyValueStatistics(byte[] statQueryByteArray, string exploredPropertyTypeUri,
            int startOffset, int resultsLimit, long minimumCount)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RetrievePropertyValueStatistics(statQueryByteArray, exploredPropertyTypeUri,
                    startOffset, resultsLimit, minimumCount);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public PropertyBarValues RetrievePropertyBarValuesStatistics
(byte[] queryByteArray, string exploredPropertyTypeUri, long bucketCount, double minValue, double maxValue)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RetrievePropertyBarValuesStatistics(queryByteArray, exploredPropertyTypeUri,
                    bucketCount, minValue, maxValue);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }




        public LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RetrieveLinkTypeStatistics(queryByteArray);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider(callerUserName);
                return statisticalQueryProvider.RetrieveLinkedObjectIDsByStatisticalQuery(queryByteArray, PassObjectsCountLimit);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void SaveInvestigation(KInvestigation kInvestigation)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                InvestigationManagement investigationManagement = new InvestigationManagement(callerUserName);
                 investigationManagement.SaveInvestigation(kInvestigation);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<InvestigationInfo> GetSavedInvestigations()
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                InvestigationManagement investigationManagement = new InvestigationManagement(callerUserName);
               return  investigationManagement.GetSavedInvestigations();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public byte[] GetSavedInvestigationImage(long id)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                InvestigationManagement investigationManagement = new InvestigationManagement(callerUserName);
                return investigationManagement.GetSavedInvestigationImage(id);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public byte[] GetSavedInvestigationStatus(long id)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                InvestigationManagement investigationManagement = new InvestigationManagement(callerUserName);
                return investigationManagement.GetSavedInvestigationStatus(id);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #region Timeline

        public long GetTimeLineMaxFrequecyCount(string[] propertiesTypeUri, string binLevel)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                TimelineProvider timelineProvider = new TimelineProvider(callerUserName);
                return timelineProvider.GetTimeLineMaxFrequecyCount(propertiesTypeUri, binLevel);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public DateTime GetTimeLineMaxDate(string[] propertiesTypeUri, string binLevel)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                TimelineProvider timelineProvider = new TimelineProvider(callerUserName);
                return timelineProvider.GetTimeLineMaxDate(propertiesTypeUri, binLevel);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public DateTime GetTimeLineMinDate(string[] propertiesTypeUri, string binLevel)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;
                TimelineProvider timelineProvider = new TimelineProvider(callerUserName);
                return timelineProvider.GetTimeLineMinDate(propertiesTypeUri, binLevel);
             }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        #region TextualSearch

        public List<TextualSearch.BaseSearchResult> PerformTextualSearch(byte[] stream)
        {
            try
            {
                string callerUserName = System.ServiceModel.ServiceSecurityContext.Current.PrimaryIdentity.Name;

                TextualSearchProvider textualSearchProvider = new TextualSearchProvider(callerUserName);
                return textualSearchProvider.PerformTextualSearch(stream);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion
    }
}