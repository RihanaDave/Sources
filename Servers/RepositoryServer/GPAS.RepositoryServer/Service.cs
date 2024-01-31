using GPAS.AccessControl;
using GPAS.Logger;
using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using GPAS.RepositoryServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.RepositoryServer
{
    public class Service : IService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        #region Objects Retrievation
        /// <summary>
        /// این تابع اشیا در پایگاه داده که با لیست ورودی برابر است را بر می گرداند.
        /// </summary>
        /// <param name="dbObjectIDs">   لیستی از آیدی اشیا.   </param>
        /// <returns>    لیستس از نوع DBObject را برمی گرداند.    </returns>
        public List<DBObject> GetObjects(List<long> dbObjectIDs)
        {
            try
            {
                ObjectManager objectManager = new ObjectManager();
                return objectManager.GetObjects(dbObjectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        {
            try
            {
                ObjectManager objectManager = new ObjectManager();
                return objectManager.RetrieveObjectsSequentialByIDRange(firstID, lastID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Properties Retrievation
        /// <summary>
        /// لیستی از ویژگی های مربوط به شی مورد نظر را از پایگاه داده برمی گرداند.
        /// </summary>
        /// <param name="dbObject">     یک شی از نوع DBObject دریافت می کند.   </param>
        /// <returns>  DBProperty را برمی گرداند لیستی از    </returns>
        public List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetPropertiesOfObject(dbObject, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetPropertiesOfObjectsWithoutAuthorization(objectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetPropertiesOfObjects(objectIDs, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authParams)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, specifiedPropertyTypeUris, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyTypeUri, propertyValue, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams)
        {
            try
            {
                ProperyManager propertyManeger = new ProperyManager();
                return propertyManeger.GetPropertiesByID(dbPropertyIDs, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Relationships Retrievation
        /// <summary>
        /// این تابع لیستی از لینک ها را که آیدی آنان در لیست ورودی موجود است را بر می گرداند. 
        /// </summary>
        /// <param name="dbRelationshipIDs">   لیستی از آیدی لینک ها می باشد.     </param>
        /// <returns>  را بر میگرداند. DBRelationship لیستی از از نوع    </returns>
        public List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetRelationships(dbRelationshipIDs, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.RetrieveRelationships(dbRelationshipIDs);
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
        /// <returns>      لینک از نوع DBRelationship را برمی گرداند.     </returns>
        public List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetRelationshipsBySourceObject(objectID, typeURI, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetRelationshipsBySourceOrTargetObject(objectIDs, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetRelationshipsBySourceObjectWithoutAuthParams(objectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(objectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbObject"></param>
        /// <param name="typeURI"></param>
        /// <returns></returns>
        public List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.GetSourceLink(dbObject, typeURI, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// لینک موجود با شرایط دریافتی را در صورت موجود برمیگرداند.
        /// </summary>
        /// <param name="type"> نوع رابطه را مشخص میکند. </param>
        /// <param name="source"> شی مبدا را تعیین میکند. </param>
        /// <param name="target">  شی مقصد را تعیین میکند. </param>
        /// <param name="direction">  جهت رابطه را تعیین میکند.  </param>
        /// <returns>   در صورت وجود رابطه همان لینک را برمی گرداند در غیر ین صورت null برمی گرداند.    </returns>
        public DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams)
        {
            try
            {
                RelationshipManager relationshipManager = new RelationshipManager();
                return relationshipManager.GetExistingRelationship(typeURI, source, target, direction,null);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID)
        {
            try
            {
                RelationshipManager relationshipManeger = new RelationshipManager();
                return relationshipManeger.RetrieveRelationshipsSequentialByIDRange(firstID, lastID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Medias Retrievation
        /// <summary>
        /// این تابع لیستی از فایل ها ی جند رسانه ای منتسب به یک شی را بر می گرداند
        /// </summary>
        /// <param name="objectID">این پارامتر آیدی شی را از کاربر دریافت می کند</param>
        /// <returns>این تابع لیستی از ساختمان داده DBMEDIA را بر می گرداند</returns>
        public List<DBMedia> GetMediaForObject(long objectID, AuthorizationParametters authParams)
        {
            try
            {
                MediaManager mediaManager = new MediaManager();
                return mediaManager.GetMedia(objectID, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBMedia> GetMediasForObjectsWithoutAuthorization(long[] objectIDs)
        {
            try
            {
                MediaManager mediaManager = new MediaManager();
                return mediaManager.GetMediasOfObjects(objectIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public List<DBMedia> GetMediasForObjects(long[] objectIDs, AuthorizationParametters authParams)
        {
            try
            {
                MediaManager mediaManager = new MediaManager();
                return mediaManager.GetMediasOfObjects(objectIDs, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Publish
        public void Publish(DBAddedConcepts addedConcept, DBModifiedConcepts modifiedConcept, DBResolvedObject[] resolvedObjects, long dataSourceID)
        {
            try
            {
                PublishManager publishManager = new PublishManager();
                publishManager.Publish(addedConcept, modifiedConcept, resolvedObjects, dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Graph Store and Retrieve
        /// <summary>
        /// این تابع چینش گراف ها را دریاقت کرده و در پایگاه داده ذخیره می کند.
        /// </summary>
        /// <param name="dbGraphArrangement">   یک شی از DBGraphArrangement را دریافت می کند.    </param>
        /// <returns>   یک شی از DBGraphArrangement را بر می گرداند.    </returns>
        public DBGraphArrangement CreateNewGraphArrangment(DBGraphArrangement dbGraphArrangement)
        {
            try
            {
                GraphManager graphManager = new GraphManager();
                return graphManager.SaveNew(dbGraphArrangement);
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
        /// <returns>   لیستی از DBGraphArrangement را بر می گرداند.    </returns>
        public List<DBGraphArrangement> GetGraphArrangements(AuthorizationParametters authParams)
        {
            try
            {
                GraphManager graphManager = new GraphManager();
                return graphManager.GetGraphArrangements(authParams);
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
        public byte[] GetGraphImage(int dbGraphArrangementID, AuthorizationParametters authParams)
        {
            try
            {
                GraphManager grapgManager = new GraphManager();
                return grapgManager.GetGraphImage(dbGraphArrangementID, authParams);
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
        public byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authParams)
        {
            try
            {
                GraphManager grapgManager = new GraphManager();
                return grapgManager.GetGraphArrangementXML(dbGraphArrangementID, authParams);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        /// <summary>
        /// یک گراف را از پایگاه داده حذف می کند.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteGraph(int id)
        {
            try
            {
                GraphManager graphManager = new GraphManager();
                return graphManager.DeleteGraph(id);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region ID Assignment
        public long GetLastAsignedObjectId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedObjectId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetLastAsignedGraphId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedGraphId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetLastAsignedPropertyId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedPropertyId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetLastAsignedRelationshipId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedRelationshipId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetLastAsignedMediaId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedMediaId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long GetLastAsignedDataSourceId()
        {
            try
            {
                IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
                return idRetrievationManager.GetLastAsignedDataSourceId();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        #region Repository Server Management
        public void Optimize()
        {
            try
            {
                var optimizer = new DatabaseOptimizer();
                optimizer.Optimize();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void TruncateDatabase()
        {
            try
            {
                var cleaner = new TruncateDatabase();
                cleaner.Truncate();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region ACL
        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        {
            try
            {
                UserAccountControlManager userAccountControlManager = new UserAccountControlManager();
                userAccountControlManager.RegisterNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            try
            {
                UserAccountControlManager userAccountControlManager = new UserAccountControlManager();
                return userAccountControlManager.GetSubsetOfConceptsByPermission(conceptType, IDs, groupNames, minimumPermission);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DBDataSourceACL> RetrieveDataSourceACLs(long[] DataSourceIDs)
        {
            try
            {
                DataSourceManager dataSourceManager = new DataSourceManager();
                return dataSourceManager.RetrieveDataSourceACLs(DataSourceIDs);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            try
            {
                DataSourceManager dataSourceManager = new DataSourceManager();
                return dataSourceManager.RetrieveTopNDataSourceACLs(topN);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID)
        {
            try
            {
                DataSourceManager dataSourceManager = new DataSourceManager();
                return dataSourceManager.RetriveDataSourcesSequentialIDByIDRange(firstID,lastID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids)
        {
            try
            {
                DataSourceManager dataSourceManager = new DataSourceManager();
                return dataSourceManager.GetDataSourcesByIDs(ids);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion

        public void IsAvailable()
        {
        }
    }
}
