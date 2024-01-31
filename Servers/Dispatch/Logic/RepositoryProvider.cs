using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس دسترسی به Repository Server جهت ذخیره و بازیابی شی و لینک و... را فراهم می کند
    /// </summary>
    public class RepositoryProvider
    {
        private string CallerUserName = "";
        public RepositoryProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }


        /// <summary>
        /// این تابع اشیا در پایگاه داده که با لیست ورودی برابر است را بر می گرداند.
        /// </summary>
        /// <param name="dbObjectIDs">   لیستی از آیدی اشیا.   </param>
        /// <returns>    لیستس از نوع KObject را برمی گرداند.    </returns>
        public List<KObject> GetObjects(long[] dbObjectIDs)
        {
            List<KObject> kObjects = new List<KObject>();

            //DBObject[] retrievedObjects;
            //ServiceClient proxy = null;

            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;

            try
            {
                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                ServiceAccess.SearchService.SearchObject[] searchObjects = serviceClient.GetObjectByIDs(dbObjectIDs);
                EntityConvertor entityConvertor = new EntityConvertor();

                foreach (var item in searchObjects)
                {
                    KObject kObject = entityConvertor.ConvertSearchObjectToKObject(item, CallerUserName);
                    kObjects.Add(kObject);
                }

                //proxy = new ServiceClient();
                //retrievedObjects = proxy.GetObjects(dbObjectIDs);
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }

            return kObjects;

            //EntityConvertor entityConvertor = new EntityConvertor();
            //List<KObject> kObjectsList = new List<KObject>(retrievedObjects.Length);
            //foreach (var dbObject in retrievedObjects)
            //{
            //    KObject kObject = entityConvertor.ConvertDBObjectToKObject(dbObject, CallerUserName);
            //    kObjectsList.Add(kObject);
            //}
            //return kObjectsList;
        }

        /// <summary>
        /// این تابع لیستی از لینک ها را که آیدی آنان در لیست ورودی موجود است را بر می گرداند. 
        /// </summary>
        /// <param name="dbRelationshipIDs">   لیستی از آیدی لینک ها می باشد.     </param>
        /// <returns>  را بر میگرداند. RelationshipBaseKlink لیستی از از نوع    </returns>
        public List<RelationshipBaseKlink> GetRelationships(List<long> dbRelationshipIDs)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                List<RelationshipBaseKlink> kRelationshipsList = new List<RelationshipBaseKlink>();

                //proxy = new ServiceClient();
                serviceClient = new ServiceAccess.SearchService.ServiceClient();

                EntityConvertor entityConvertor = new EntityConvertor();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                //List<DBRelationship> dbObjectsList = proxy.GetRelationships(dbRelationshipIDs.ToArray(), authorizationParametter).ToList();
                GPAS.Dispatch.ServiceAccess.SearchService.SearchRelationship[] searchRelationships = serviceClient.GetRelationships(dbRelationshipIDs.ToArray(), authorizationParametter);

                //foreach (var item in dbObjectsList)
                //{
                //RelationshipBaseKlink relationshipBaseKlink = entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(item, CallerUserName);
                //kRelationshipsList.Add(relationshipBaseKlink);
                //}
                //return kRelationshipsList;

                foreach (var item in searchRelationships)
                {
                    //kRelationshipsList.Add(new RelationshipBaseKlink()
                    //{
                    //    Relationship = new KRelationship() { Id = item.Id, DataSourceID = item.DataSourceID, Description = "", Direction = (LinkDirection)item.Direction },
                    //    TypeURI = item.TypeUri,
                    //    Source = new KObject() { Id = item.SourceObjectId, TypeUri = item.SourceObjectTypeUri, IsGroup = false, ResolvedTo = null },
                    //    Target = new KObject() { Id = item.TargetObjectId, TypeUri = item.TargetObjectTypeUri, IsGroup = false, ResolvedTo = null }
                    //});
                    RelationshipBaseKlink relationshipBaseKlink = entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(item, CallerUserName);
                    kRelationshipsList.Add(relationshipBaseKlink);
                }
                return kRelationshipsList;
            }
            finally
            {
                //if (proxy != null)
                //proxy.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        public List<KProperty> GetPropertyForObjects(long[] dbObjIDs)
        {
            //ServiceClient proxy = null;
            //if (dbObjIDs.Count() == 0)
            //{
            //    return new List<KProperty>();
            //}

            //try
            //{
            //    List<KProperty> kProperties = new List<KProperty>();
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    List<DBProperty> dbProperties = proxy.GetPropertiesOfObjects(dbObjIDs, authorizationParametter).ToList();

            //    foreach (var item in dbProperties)
            //    {
            //        KProperty kProperty = entityConvertor.ConvertDBPropertyToKProperty(item, CallerUserName);
            //        kProperties.Add(kProperty);
            //    }
            //    return kProperties;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            //----------------------------
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                GPAS.Dispatch.ServiceAccess.SearchService.SearchProperty[] searchProperties = serviceClient.GetDBPropertyByObjectIds(dbObjIDs, authorizationParametter);

                List<KProperty> kProperties = new List<KProperty>();
                foreach (var item in kProperties)
                {
                    KProperty kProperty = new KProperty()
                    {
                        Id = item.Id,
                        TypeUri = item.TypeUri,
                        Value = item.Value,
                        DataSourceID = item.DataSourceID,
                        Owner = new KObject()
                        {
                            Id = item.Owner.Id,
                            TypeUri = item.TypeUri,
                            LabelPropertyID = item.Owner.LabelPropertyID
                        }
                    };

                    kProperties.Add(kProperty);
                }

                return kProperties;
            }
            catch (Exception)
            {

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
            //ServiceClient proxy = null;
            //try
            //{
            //    List<RelationshipBaseKlink> kRelationshipsList = new List<RelationshipBaseKlink>();
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    List<DBRelationship> dbRelationships = proxy.GetRelationshipsBySourceObject(objectID, typeURI, authorizationParametter).ToList();

            //    foreach (var dbRelationship in dbRelationships)
            //    {
            //        RelationshipBaseKlink relationshipBaseKlink = entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(dbRelationship, CallerUserName);
            //        kRelationshipsList.Add(relationshipBaseKlink);
            //    }
            //    return kRelationshipsList;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            //----------------------------
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                List<ServiceAccess.SearchService.SearchRelationship> dbRelationships = serviceClient.GetRelationshipsBySourceObject(objectID, typeURI, authorizationParametter).ToList();

                List<RelationshipBaseKlink> relationshipBaseKlinks = new List<RelationshipBaseKlink>();

                foreach (var item in dbRelationships)
                {
                    RelationshipBaseKlink relationship = new RelationshipBaseKlink()
                    {
                        TypeURI = item.TypeUri,
                        Source = new KObject() { Id = item.SourceObjectId, TypeUri = item.SourceObjectTypeUri },
                        Target = new KObject() { Id = item.TargetObjectId, TypeUri = item.TargetObjectTypeUri},
                        Relationship = new KRelationship() { Id = item.Id, DataSourceID = item.DataSourceID, Description = "", Direction = (LinkDirection)item.Direction, TimeBegin = DateTime.MinValue, TimeEnd = DateTime.MinValue }
                    };

                    relationshipBaseKlinks.Add(relationship);
                }

                return relationshipBaseKlinks;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RelationshipBaseKlink> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    List<RelationshipBaseKlink> kRelationshipsList = new List<RelationshipBaseKlink>();
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    List<DBRelationship> dbRelationships = proxy.GetRelationshipsBySourceOrTargetObject(objectIDs.ToArray(), authorizationParametter).ToList();

            //    foreach (var dbRelationship in dbRelationships)
            //    {
            //        RelationshipBaseKlink relationshipBaseKlink = entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(dbRelationship, CallerUserName);
            //        kRelationshipsList.Add(relationshipBaseKlink);
            //    }
            //    return kRelationshipsList;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            //-----------------
            ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                ServiceAccess.SearchService.SearchRelationship[] dbRelationships = serviceClient.GetRelationshipsBySourceOrTargetObject(objectIDs.ToArray(), authorizationParametter);

                List<RelationshipBaseKlink> relationshipBaseKlinks = new List<RelationshipBaseKlink>();
                foreach (var item in dbRelationships)
                {
                    RelationshipBaseKlink relationship = new RelationshipBaseKlink()
                    {
                        TypeURI = item.TypeUri,
                        Source = new KObject() { Id = item.SourceObjectId, TypeUri = item.SourceObjectTypeUri },
                        Target = new KObject() { Id = item.TargetObjectId, TypeUri = item.TargetObjectTypeUri},
                        Relationship = new KRelationship() { Id = item.Id, DataSourceID = item.DataSourceID, Description = "", Direction = (LinkDirection)item.Direction, TimeBegin = DateTime.MinValue, TimeEnd = DateTime.MinValue }
                    };

                    relationshipBaseKlinks.Add(relationship);
                }

                return relationshipBaseKlinks;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static object retrieveResolveMasterForDBObjLockObject = new object();
        //internal KObject GetResolveMasterKObjectForDBObject(DBObject dbObjToGetItsResolveMaster)
        //{
        //    if (dbObjToGetItsResolveMaster.ResolvedTo == null)
        //        return null;
        //    else
        //        lock (retrieveResolveMasterForDBObjLockObject)
        //        {
        //            return GetObjects(new long[] { dbObjToGetItsResolveMaster.ResolvedTo.Value }).First();
        //        }
        //}

        /// <summary>
        /// لیستی از ویژگی های مربوط به شی مورد نظر را برمی گرداند.
        /// </summary>
        /// <param name="dbObject">     یک شی از نوع KObject دریافت می کند.   </param>
        /// <returns>  KProperty را برمی گرداند لیستی از    </returns>
        public List<KProperty> GetPtoperty(KObject kObject)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                ServiceAccess.SearchService.SearchProperty[] searchProperties = serviceClient.GetDBPropertyByObjectId(kObject.Id, authorizationParametter);



                List<KProperty> resultProperty = new List<KProperty>();
                //proxy = new ServiceClient();
                EntityConvertor entityConvertor = new EntityConvertor();

                //DBObject dbObject = entityConvertor.ConvertKObjectToDBObject(kObject);

                //DBProperty[] resultDBProperty = proxy.GetPropertiesOfObject(dbObject, authorizationParametter);

                //foreach (var property in resultDBProperty)
                //{
                //    KProperty kProperty = entityConvertor.ConvertDBPropertyToKProperty(property, CallerUserName);
                //    resultProperty.Add(kProperty);
                //}

                foreach (ServiceAccess.SearchService.SearchProperty item in searchProperties)
                {
                    KProperty kProperty = new KProperty()
                    {
                        Id = item.Id,
                        DataSourceID = item.DataSourceID,
                        TypeUri = item.TypeUri,
                        Value = item.Value,
                        Owner = new KObject()
                        {
                            Id = item.OwnerObject.Id,
                            TypeUri = item.OwnerObject.TypeUri,
                            LabelPropertyID = item.OwnerObject.LabelPropertyID
                        }
                    };

                    resultProperty.Add(kProperty);
                }

                return resultProperty;
            }
            finally
            {
                //if (proxy != null)
                //proxy.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        public List<KProperty> GetPropertiesById(long[] dbOPropertyIDs)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            EntityConvertor entityConvertor = new EntityConvertor();
            try
            {
                List<KProperty> resultProperty = new List<KProperty>();
                //proxy = new ServiceClient();
                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);


                //DBProperty[] resultDBProperty = proxy.GetPropertiesByID(dbOPropertyIDs, authorizationParametter);
                ServiceAccess.SearchService.SearchProperty[] resultDBProperty = serviceClient.GetDBPropertyByObjectIds(dbOPropertyIDs, authorizationParametter);

                foreach (var dbProperty in resultDBProperty)
                {
                    //KProperty kProperty = entityConvertor.ConvertDBPropertyToKProperty(dbProperty, CallerUserName);

                    KProperty searchProperty = new KProperty()
                    {
                        Id = dbProperty.Id,
                        TypeUri = dbProperty.TypeUri,
                        Value = dbProperty.Value,
                        DataSourceID = dbProperty.DataSourceID,
                        Owner = new KObject()
                        {
                            Id = dbProperty.OwnerObject.Id,
                            TypeUri = dbProperty.OwnerObject.TypeUri,
                            LabelPropertyID = dbProperty.OwnerObject.LabelPropertyID
                        }
                    };

                    resultProperty.Add(searchProperty);
                }
                return resultProperty;
            }
            finally
            {
                //if (proxy != null)
                //    proxy.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }
        }

        public RelationshipBaseKlink GetExistingRelationship(string typeURI, long source, long target, LinkDirection direction)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    var dbRelationship = proxy.GetExistingRelationship(typeURI, source, target, (RepositoryLinkDirection)Enum.Parse(typeof(RepositoryLinkDirection), direction.ToString(), true), authorizationParametter);
            //    return entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(dbRelationship, CallerUserName);
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            //---------------------------------
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                ServiceAccess.SearchService.SearchRelationship searchRelationship = serviceClient.GetExistingRelationship(typeURI, source, target, (int)direction, authorizationParametter);

                return new RelationshipBaseKlink()
                {
                    TypeURI = searchRelationship.TypeUri,
                    Source = new KObject() { Id = searchRelationship.SourceObjectId, TypeUri = searchRelationship.SourceObjectTypeUri },
                    Target = new KObject() { Id = searchRelationship.TargetObjectId, TypeUri = searchRelationship.TargetObjectTypeUri },
                    Relationship = new KRelationship() { Id = searchRelationship.Id, DataSourceID = searchRelationship.DataSourceID, Description = "", Direction = (LinkDirection)searchRelationship.Direction, TimeBegin = DateTime.MinValue, TimeEnd = DateTime.MinValue }
                };

            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<RelationshipBaseKlink> GetSourceLink(KObject kObject, string relationshipTypeURI)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    List<DBRelationship> resultDBRelationship = new List<DBRelationship>();
            //    List<RelationshipBaseKlink> resultRelationshipBaseKlink = new List<RelationshipBaseKlink>();
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    DBObject dbObject = entityConvertor.ConvertKObjectToDBObject(kObject);
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    resultDBRelationship = proxy.GetSourceLink(dbObject, relationshipTypeURI, authorizationParametter).ToList();

            //    foreach (var dbRelationship in resultDBRelationship)
            //    {
            //        RelationshipBaseKlink tempRelationship = entityConvertor.ConvertDBRelationshipToRelationshipBaseKlink(dbRelationship, CallerUserName);
            //        resultRelationshipBaseKlink.Add(tempRelationship);
            //    }
            //    return resultRelationshipBaseKlink;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            //-----------------------------------

            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                List<ServiceAccess.SearchService.SearchRelationship> relationships = serviceClient.GetSourceLink(kObject.Id, relationshipTypeURI, authorizationParametter).ToList();

                List<RelationshipBaseKlink> relationshipBaseKlinks = new List<RelationshipBaseKlink>();

                foreach (var searchRelationship in relationships)
                {
                    RelationshipBaseKlink relationshipBaseKlink = new RelationshipBaseKlink()
                    {
                        TypeURI = searchRelationship.TypeUri,
                        Source = new KObject() { Id = searchRelationship.SourceObjectId, TypeUri = searchRelationship.SourceObjectTypeUri},
                        Target = new KObject() { Id = searchRelationship.TargetObjectId, TypeUri = searchRelationship.TargetObjectTypeUri},
                        Relationship = new KRelationship() { Id = searchRelationship.Id, DataSourceID = searchRelationship.DataSourceID, Description = "", Direction = (LinkDirection)searchRelationship.Direction, TimeBegin = DateTime.MinValue, TimeEnd = DateTime.MinValue }
                    };

                    relationshipBaseKlinks.Add(relationshipBaseKlink);
                }
                return relationshipBaseKlinks;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// این تابع یک آیدی ابجت را ار ورودی دریافت می کند و آدرس فاسل های جند راسانه ای منتب با این آیدی را به عنوان خروجی نمایش می دهد
        /// </summary>
        /// <param name="objectID">این پارامتر یک آیدی شی را به عنوان ورودی دریافت میکند</param>
        /// <returns>این تابع لیستی از KMedia را به عنوان خروجی ارسال می کند</returns>
        public List<KMedia> GetMediaForObject(long objectID)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    List<KMedia> uriKMediaResult = new List<KMedia>();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    List<DBMedia> uriDBMediaResult = proxy.GetMediaForObject(objectID, authorizationParametter).ToList();

            //    foreach (var item in uriDBMediaResult)
            //    {
            //        uriKMediaResult.Add(new EntityConvertor().ConvertDBMediaToKMedia(item));
            //    }

            //    return uriKMediaResult;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            return new List<KMedia>();
        }

        public List<KProperty> GetPropertiesByOwnersAndTypes(long[] objectIDs, string[] propertyTypes)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //List<KProperty> kProperties;

            List<KProperty> kProperties = new List<KProperty>();
            try
            {
                //proxy = new ServiceClient();
                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                EntityConvertor entityConvertor = new EntityConvertor();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                //DBProperty[] dbPropertyResult = proxy.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, propertyTypes, authorizationParametter);
                ServiceAccess.SearchService.SearchProperty[] searchProperties = serviceClient.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, propertyTypes, authorizationParametter);

                foreach (var item in searchProperties)
                {
                    KProperty kProperty = new KProperty()
                    {
                        Id = item.Id,
                        DataSourceID = item.DataSourceID,
                        TypeUri = item.TypeUri,
                        Value = item.Value,
                        Owner = new KObject()
                        {
                            Id = item.OwnerObject.Id,
                            TypeUri = item.OwnerObject.TypeUri,
                            LabelPropertyID = item.OwnerObject.LabelPropertyID
                        }
                    };

                    kProperties.Add(kProperty);
                }


                //kProperties = new List<KProperty>(dbPropertyResult.Length);
                //foreach (var dbProperty in dbPropertyResult)
                //{
                //KProperty kProperty = entityConvertor.ConvertDBPropertyToKProperty(dbProperty, CallerUserName);
                //kProperties.Add(kProperty);
                //}
            }
            finally
            {
                //proxy.Close();
                if (serviceClient != null)
                    serviceClient.Close();

            }
            return kProperties;
        }

        public List<KProperty> GetPropertiesByOwnersAndTypeAndValue(long[] objectIDs, string propertyType, string propertyValue)
        {
            //ServiceClient proxy = null;
            //List<KProperty> kProperties;
            //try
            //{
            //    proxy = new ServiceClient();
            //    EntityConvertor entityConvertor = new EntityConvertor();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    DBProperty[] dbPropertyResult = proxy.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyType, propertyValue, authorizationParametter);
            //    kProperties = new List<KProperty>(dbPropertyResult.Length);
            //    foreach (var dbProperty in dbPropertyResult)
            //    {
            //        KProperty kProperty = entityConvertor.ConvertDBPropertyToKProperty(dbProperty, CallerUserName);
            //        kProperties.Add(kProperty);
            //    }
            //}
            //finally
            //{
            //    proxy.Close();
            //}
            //return kProperties;
            //---------------------------------
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                ServiceAccess.SearchService.SearchProperty[] dbPropertyResult = serviceClient.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyType, propertyValue, authorizationParametter);

                List<KProperty> kProperties = new List<KProperty>();

                foreach (var item in dbPropertyResult)
                {
                    KProperty property = new KProperty()
                    {
                        Id = item.Id,
                        TypeUri = item.TypeUri,
                        Value = item.Value,
                        DataSourceID = item.DataSourceID,
                        Owner = new KObject()
                        {
                            Id = item.OwnerObject.Id,
                            TypeUri = item.OwnerObject.TypeUri,
                            LabelPropertyID = item.OwnerObject.LabelPropertyID
                        }
                    };

                    kProperties.Add(property);
                }

                return kProperties;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public long GetLastAsignedObjectId()
        {
            ////ServiceClient proxy = null;
            //GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //try
            //{
            //    //proxy = new ServiceClient();
            //    //return proxy.GetLastAsignedObjectId();

            //    serviceClient = new ServiceAccess.SearchService.ServiceClient();
            //    return serviceClient.GetLastAsignedObjectId();
            //}
            //finally
            //{
            //    //if (proxy != null)
            //    //    proxy.Close();
            //    if (serviceClient != null)
            //        serviceClient.Close();
            //}

            return 1;
        }
        internal long GetLastAsignedPropertyId()
        {
            ////ServiceClient proxy = null;
            //GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //try
            //{
            //    //proxy = new ServiceClient();
            //    //return proxy.GetLastAsignedPropertyId();

            //    serviceClient = new ServiceAccess.SearchService.ServiceClient();
            //    return serviceClient.GetLastAsignedPropertyId();
            //}
            //finally
            //{
            //    //if (proxy != null)
            //    //    proxy.Close();
            //    if (serviceClient != null)
            //        serviceClient.Close();
            //}

            return 1;
        }

        internal long GetLastAsignedRelationId()
        {
            ////ServiceClient proxy = null;
            ////try
            ////{
            ////    proxy = new ServiceClient();
            ////    return proxy.GetLastAsignedRelationshipId();
            ////}
            ////finally
            ////{
            ////    if (proxy != null)
            ////        proxy.Close();
            ////}

            ////ServiceClient proxy = null;
            //GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //try
            //{
            //    //proxy = new ServiceClient();
            //    //return proxy.GetLastAsignedPropertyId();

            //    serviceClient = new ServiceAccess.SearchService.ServiceClient();
            //    return serviceClient.GetLastAsignedRelationId();
            //}
            //finally
            //{
            //    //if (proxy != null)
            //    //    proxy.Close();
            //    if (serviceClient != null)
            //        serviceClient.Close();
            //}

            return 1;
        }

        internal long GetLastAsignedMediaId()
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    return proxy.GetLastAsignedMediaId();
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}
            return 1000;
        }

        internal long GetLastAsignedGraphId()
        {
            ////ServiceClient proxy = null;
            //GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //try
            //{
            //    //proxy = new ServiceClient();
            //    //return proxy.GetLastAsignedObjectId();

            //    serviceClient = new ServiceAccess.SearchService.ServiceClient();
            //    return serviceClient.GetLastAssignedGraphaId();
            //}
            //finally
            //{
            //    //if (proxy != null)
            //    //    proxy.Close();
            //    if (serviceClient != null)
            //        serviceClient.Close();
            //}

            return 1;
        }

        internal long GetLastAsignedDataSourceId()
        {
            ////ServiceClient proxy = null;
            ////try
            ////{
            ////    proxy = new ServiceClient();
            ////    return proxy.GetLastAsignedDataSourceId();
            ////}
            ////finally
            ////{
            ////    if (proxy != null)
            ////        proxy.Close();
            ////}
            ////ServiceClient proxy = null;
            //GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            //try
            //{
            //    //proxy = new ServiceClient();
            //    //return proxy.GetLastAsignedPropertyId();

            //    serviceClient = new ServiceAccess.SearchService.ServiceClient();
            //    return serviceClient.GetLastAsignedDataSourceId();
            //}
            //finally
            //{
            //    //if (proxy != null)
            //    //    proxy.Close();
            //    if (serviceClient != null)
            //        serviceClient.Close();
            //}

            return 1;
        }

    }
}
