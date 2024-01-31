using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;
using static GPAS.Workspace.DataAccessManager.LinkManager;
using LinkDirection = GPAS.Workspace.Entities.KWLinks.LinkDirection;

namespace GPAS.Workspace.DataAccessManager
{
    /// <summary>
    /// مدیریت داده‌ای اشیاء سمت محیط کاربری
    /// </summary>
    public class ObjectManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        static ObjectManager()
        {
            GroupMembershipRelationshipsTypeUri = string.Empty;
        }

        #region آماده‌سازی اولیه | Initialization
        private static bool IsInitialized()
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(ObjectLabelPropertyTypeUri)
                || string.IsNullOrWhiteSpace(GroupMembershipRelationshipsTypeUri))
            {
                result = false;
            }
            return result;
        }
        public static void Initialization(string defaultObjectLabelPropertyTypeUri, string defaultGroupInnerRelationshipsTypeUri)
        {
            if (defaultGroupInnerRelationshipsTypeUri == null)
                throw new ArgumentNullException(nameof(defaultGroupInnerRelationshipsTypeUri));
            if (defaultObjectLabelPropertyTypeUri == null)
                throw new ArgumentNullException(nameof(defaultObjectLabelPropertyTypeUri));

            if (!IsInitialized())
            {
                ObjectLabelPropertyTypeUri = defaultObjectLabelPropertyTypeUri;
                GroupMembershipRelationshipsTypeUri = defaultGroupInnerRelationshipsTypeUri;
            }
        }

        internal static string GroupMembershipRelationshipsTypeUri
        {
            get;
            private set;
        }
        internal static string ObjectLabelPropertyTypeUri
        {
            get;
            private set;
        }
        #endregion

        #region  مدیریت میانگیری و رفع تصادم‌ها | Caching & Conflicts resolution
        internal class CachedObjectMetadata
        {
            internal KWObject CachedObject;
            internal bool IsLocallyResolved;
            internal bool IsPublished;
            internal bool IsNotUploadedSourceDocument;
            /// <summary>
            /// آرایه‌ی اشیائی که به صورت محلی با این شئ ادغام شده‌اند (این شئ میزبان ادغام
            /// آن‌هاست) و تغییرات این ادغام هنوز منتشر نشده است.
            /// 
            /// کاربرد این فیلد برای «توابع بازیابی» اجزای اشیاء است تا در صورت نیاز اجزای
            /// اشیاء محلی ادغام شده نیز بازیابی شوند
            /// </summary>
            internal KWObject[] ObjectsWhereLocallyResolvedTo;
        }

        /// <summary>
        /// لیست مرتب شده اشیا ایجاد شده در سمت محیط کاربری که براساس شناسه شان مرتب شده اند؛
        /// این لیست تضمین می کند که به ازای هر شئ یکتا در سمت محیط کاربری، بیش از یک نمونه ساخته نشود.
        /// این لیست منحصرا توسط تابع تبدیل نمونه شی سمت سرویس دهنده راه دور به نمونه شی سمت محیط کاربری «تغییر» می کند.
        /// </summary>
        private static Dictionary<long, CachedObjectMetadata> cachedObjectsMetadataIdentifiedByID
            = new Dictionary<long, CachedObjectMetadata>();

        private static void AddObjectToCache(KWObject objectToCache, bool isPublished, bool isNotUploadedSourceDocument = false)
        {
            var newMetaData = new CachedObjectMetadata()
            {
                CachedObject = objectToCache,
                IsPublished = isPublished,
                IsNotUploadedSourceDocument = isNotUploadedSourceDocument,
                ObjectsWhereLocallyResolvedTo = new KWObject[] { }
            };
            cachedObjectsMetadataIdentifiedByID.Add(objectToCache.ID, newMetaData);
        }

        private static void SetCachedObjectAsLocallyResolved(KWObject resolvedObject)
        {
            var objectMetadata = GetCachedMetadataById(resolvedObject.ID);
            objectMetadata.IsLocallyResolved = true;
        }

        private static void SetObjectAsResolveMasterOfObjects(KWObject resolveMaster, List<KWObject> resolvedObjects)
        {
            var resolveMasterObhectMetadata = GetCachedMetadataById(resolveMaster.ID);
            resolveMasterObhectMetadata.ObjectsWhereLocallyResolvedTo = resolvedObjects.ToArray();
        }

        private static List<KWObject> GetUnpublishedObjectsById(List<long> unpublishedObjectIds)
        {
            var result = new List<KWObject>(unpublishedObjectIds.Count);
            foreach (var id in unpublishedObjectIds)
            {
                var unpublishedObject = cachedObjectsMetadataIdentifiedByID[id].CachedObject;
                result.Add(unpublishedObject);
            }
            return result;
        }

        private static void UpdateCacheByRetrievedObject(KObject retrievedObject)
        {
            // راهکار سابق برای رفع تصادم بین داده‌های محلی و راه‌دور
            //if (IsRetrievedObjectConflictsWithCache(retrievedObject))
            //{
            //    ResolveRetrievedAndCachedObjectConflicts(retrievedObject);
            //}
        }
        private static bool IsObjectCached(long objectID)
        {
            return cachedObjectsMetadataIdentifiedByID.ContainsKey(objectID);
        }
        internal static KWObject GetCachedObjectById(long objectId)
        {
            return GetCachedMetadataById(objectId).CachedObject;
        }
        private static CachedObjectMetadata GetCachedMetadataById(long objectId)
        {
            return cachedObjectsMetadataIdentifiedByID[objectId];
        }
        private static bool IsRetrievedObjectCachedBefore(KObject retrievedObject)
        {
            return IsObjectCached(retrievedObject.Id);
        }
        private static bool IsRetrievedObjectConflictsWithCache(KObject retrievedObject)
        {
            var cachedObjectMetadata = GetCachedMetadataById(retrievedObject.Id);
            var cachedObject = cachedObjectMetadata.CachedObject;

            // TODO: آینده - درصورتی که نام نمایشی شئ، پس از یک بار میانگیری، در
            // مخزن داده‌ها به‌روز شود، این حالت (در حال حاضر) تصادم تلقی می‌شود؛
            // می‌توان برای این اتفاق راهکار بهتری داشت.
            if (cachedObject.DisplayName.ID.Equals(retrievedObject.LabelPropertyID)
                /*&& cachedObject.DisplayName.Value.Equals() در صورت نیاز می‌توان در آینده مقدار نام نمایشی را هم به صورت مقتضی بررسی کرد*/)
                return false;
            else
                return true;
        }
        private static void DiscardUnpublishedChangesForObject(KObject retrievedObject)
        {
            var cachedObjectMetadata = GetCachedMetadataById(retrievedObject.Id);
            // TODO: منطق این تابع در صورت نیاز به استفاده از آن می‌بایست با سازگار نام نمایشی مبتنی بر ویزگی سازگار شود
            //cachedObjectMetadata.CachedObject.DisplayName.ID = retrievedObject.LabelPropertyID.GetValueOrDefault();
            throw new NotImplementedException();
        }

        internal static IEnumerable<KWObject> GetLocallyChangedObjects()
        {
            return cachedObjectsMetadataIdentifiedByID
                .Values
                .Where(metadata => (IsUnpublishedObject(metadata.CachedObject)) && !metadata.IsNotUploadedSourceDocument)
                .Select(metadata => metadata.CachedObject);
        }

        internal static IEnumerable<CachedObjectMetadata> GetUnpublishedCachedMetadata()
        {
            return cachedObjectsMetadataIdentifiedByID
                .Values
                .Where(metadata => IsUnpublishedObject(metadata.CachedObject) && !metadata.IsNotUploadedSourceDocument).ToList();
        }

        internal static void AddSavedMetadataToCache(List<CachedObjectMetadata> objectsMetadata)
        {
            foreach (var currentMetadata in objectsMetadata)
            {
                if (!cachedObjectsMetadataIdentifiedByID.ContainsKey(currentMetadata.CachedObject.ID))
                {
                    cachedObjectsMetadataIdentifiedByID.Add(currentMetadata.CachedObject.ID, currentMetadata);
                }
                else
                {
                    UpdateCacheBySavedMetadata(currentMetadata);
                }
            }
        }

        private static void UpdateCacheBySavedMetadata(CachedObjectMetadata objectMetadata)
        {
            if (cachedObjectsMetadataIdentifiedByID.ContainsKey(objectMetadata.CachedObject.ID))
            {
                CachedObjectMetadata existanceMetadataInCache = cachedObjectsMetadataIdentifiedByID[objectMetadata.CachedObject.ID];
                existanceMetadataInCache.IsLocallyResolved = objectMetadata.IsLocallyResolved;
                existanceMetadataInCache.IsNotUploadedSourceDocument = objectMetadata.IsNotUploadedSourceDocument;
                existanceMetadataInCache.IsPublished = objectMetadata.IsPublished;
                existanceMetadataInCache.CachedObject = objectMetadata.CachedObject;
                existanceMetadataInCache.ObjectsWhereLocallyResolvedTo = objectMetadata.ObjectsWhereLocallyResolvedTo;
            }
        }

        internal static void ClearCache()
        {
            cachedObjectsMetadataIdentifiedByID.Clear();
        }

        //private static IEnumerable<CachedObjectMetadata> GetResolvedCachedMetadata()
        //{
        //    Dictionary<long, ObjectResolutionMap> masterToObjectResolutionMapDictionary = new Dictionary<long, ObjectResolutionMap>();
        //    foreach (var currentCachedObjectMetadata in cachedObjectsMetadataIdentifiedByID.Values)
        //    {
        //        if (currentCachedObjectMetadata.IsLocallyResolved)
        //        {
        //            if (masterToObjectResolutionMapDictionary.Keys.ToList().Contains(currentCachedObjectMetadata.CachedObject.ResolvedTo.ID))
        //            {
        //                ObjectResolutionMap relatedValue = null;
        //                masterToObjectResolutionMapDictionary.TryGetValue(currentCachedObjectMetadata.CachedObject.ResolvedTo.ID, out relatedValue);
        //                relatedValue.ResolvedObjects.Add(currentCachedObjectMetadata.CachedObject);
        //            }
        //            else
        //            {
        //                long key = currentCachedObjectMetadata.CachedObject.ResolvedTo.ID;
        //                List<KWObject> ResolvedObjectsTemp = new List<KWObject>();
        //                ResolvedObjectsTemp.Add(currentCachedObjectMetadata.CachedObject);
        //                ObjectResolutionMap value = new ObjectResolutionMap()
        //                {
        //                    ResolveMaster = currentCachedObjectMetadata.CachedObject.ResolvedTo,
        //                    ResolvedObjects = ResolvedObjectsTemp
        //                };
        //                masterToObjectResolutionMapDictionary.Add(key, value);
        //            }
        //        }
        //    }
        //    return masterToObjectResolutionMapDictionary.Values;
        //}

        internal static IEnumerable<KWObject> GetUnpublishedObjects()
        {
            return cachedObjectsMetadataIdentifiedByID
                .Values
                .Where(metadata => IsUnpublishedObject(metadata.CachedObject) && !metadata.IsNotUploadedSourceDocument)
                .Select(metadata => metadata.CachedObject);
        }

        internal static IEnumerable<KWObject> GetCacheObjects()
        {
            return cachedObjectsMetadataIdentifiedByID
                .Values
                .Select(objectMetadata => objectMetadata.CachedObject);
        }
        private static IEnumerable<KWObject> GetCachedPublishedObjectsModifiedLocally()
        {
            return cachedObjectsMetadataIdentifiedByID
                .Values
                .Where(objectMetadata
                    => (!objectMetadata.IsLocallyResolved)
                    && !IsUnpublishedObject(objectMetadata.CachedObject))
                .Select(metadata => metadata.CachedObject);
        }

        public static bool IsUnpublishedObject(KWObject objectToCheck)
        {
            if (objectToCheck == null)
                throw new ArgumentNullException(nameof(objectToCheck));

            return IsUnpublishedObject(objectToCheck.ID);
        }
        public static bool IsUnpublishedObject(long objectId)
        {
            if (IsObjectCached(objectId))
            {
                var metadata = GetCachedMetadataById(objectId);
                return !metadata.IsPublished;
            }
            else
            {
                return false;
            }
        }

        public static bool ContainsUnpublishedObject(IEnumerable<KWObject> objectsToResolve)
        {
            bool result = false;
            foreach (var currentObject in objectsToResolve)
            {
                if (IsUnpublishedObject(currentObject))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        #endregion

        #region خدمات انتشار اشیاء | Publish services
        public static UnpublishedObjectChanges GetUnpublishedChanges()
        {
            var changes = new UnpublishedObjectChanges();
            changes.AddedObjects = GetUnpublishedObjects();
            return changes;
        }

        internal static List<CachedObjectMetadata> GetCachedObjectMetadatas()
        {
            List<CachedObjectMetadata> cachedObjectMetadatas = new List<CachedObjectMetadata>(); ;
            List<CachedObjectMetadata> addedObjects = GetUnpublishedCachedMetadata().ToList();
            //List<CachedObjectMetadata> resolvedObjects = GetResolvedCachedMetadata();

            cachedObjectMetadatas.AddRange(addedObjects);
            //cachedObjectMetadatas.AddRange(resolvedObjects);
            return cachedObjectMetadatas;
        }

        public static void CommitUnpublishedChanges(IEnumerable<long> addedObjectIDs)
        {
            if (addedObjectIDs == null)
                throw new ArgumentNullException(nameof(addedObjectIDs));

            ApplyObjectsAdditionToCache(addedObjectIDs);
        }

        /// <summary>
        /// تغییرات محلی و همچنین میانگیری‌های قبلی را از بین می‌برد
        /// </summary>
        public static void DiscardChanges()
        {
            cachedObjectsMetadataIdentifiedByID.Clear();
        }

        private static bool IsAnyUnpublishedChangeRemains()
        {
            bool result = false;
            foreach (var metadata in cachedObjectsMetadataIdentifiedByID.Values)
                if (IsUnpublishedObject(metadata.CachedObject))
                {
                    result = true;
                    break;
                }
            return result;
        }
        private static void ApplyObjectsAdditionToCache(IEnumerable<long> addedObjectIDs)
        {
            var IdList = addedObjectIDs.ToList();
            while (IdList.Count > 0)
            {
                var metadata = cachedObjectsMetadataIdentifiedByID[IdList[0]];
                metadata.IsPublished = true;
                // از آنجایی که دیکشنری میانگیری، حاوی آخرین تغییرات محلی برای
                // اشیاء می‌باشد، بجز برداشتن تگ، نیاز به کار دیگری نیست
                IdList.RemoveAt(0);
            }
        }

        public static void SetDocumentSourceAsUploaded(KWObject obj)
        {
            CachedObjectMetadata metadata = GetCachedMetadataById(obj.ID);
            metadata.IsNotUploadedSourceDocument = false;
        }
        public static bool IsNotUploadedSourceDocument(KWObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("objectToCheck");

            return cachedObjectsMetadataIdentifiedByID[obj.ID].IsNotUploadedSourceDocument;
        }
        #endregion

        #region تعامل با سرویس‌دهنده راه‌دور
        private static long GetNewObjectID()
        {
            long objectId = -1;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                objectId = sc.GetNewObjectId();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            if (objectId == -1)
                throw new NullReferenceException("Invalid server response");

            return objectId;
        }

        private async static Task<KObject[]> RetriveKObjectsByIdAsync(IEnumerable<long> publishedObjectIds)
        {
            KObject[] retrivedKObjects = null;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                retrivedKObjects = await sc.GetObjectListByIdAsync(publishedObjectIds.ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            if (retrivedKObjects == null)
                throw new NullReferenceException("Invalid server response");
            // تبدیل به انواع شناخته شده برای محیط کاربری

            return retrivedKObjects;
        }

        /// <summary>
        /// بازیابی اشیاء از طریق شناسه‌ی آن‌ها؛ این بازیابی «بدون» اعمال ادغام اشیا انجام می‌گیرد
        /// </summary>
        private async static Task<List<KWObject>> RetriveObjectsByIdAsync(List<long> publishedObjectIds)
        {
            KObject[] retrivedKObjects = await RetriveKObjectsByIdAsync(publishedObjectIds);
            // تبدیل به انواع شناخته شده برای محیط کاربری
            return await GetObjectsFromRetrievedDataAsync(retrivedKObjects, false);
        }

        private async static Task<List<KWObject>> RetriveObjectsByIdAsync(List<KObject> publishedObjectIds)
        {
            return await GetObjectsFromRetrievedDataAsync(publishedObjectIds.ToArray(), false);
        }

        public async static Task<KWObject> GetObjectFromRetrievedDataAsync(KObject retrievedObject, bool applyResolutionTree = true)
        {
            if (retrievedObject == null)
                throw new ArgumentNullException(nameof(retrievedObject));
            return (await GetObjectsFromRetrievedDataAsync(new KObject[] { retrievedObject }, applyResolutionTree)).FirstOrDefault();
        }

        public static async Task<List<KWObject>> GetObjectsFromRetrievedDataAsync(KObject[] retrievedObjects, bool applyResolutionTree = true)
        {
            if (retrievedObjects == null)
                throw new ArgumentNullException(nameof(retrievedObjects));
            List<KObject> notCachedRetrievedObjects = new List<KObject>(retrievedObjects.Length);
            List<KWObject> result = new List<KWObject>(retrievedObjects.Length);
            foreach (KObject retObj in retrievedObjects)
            {
                if (retObj.TypeUri == null)
                {

                }
                if (IsRetrievedObjectCachedBefore(retObj))
                {
                    UpdateCacheByRetrievedObject(retObj);
                    KWObject cachedObject = GetCachedObjectById(retObj.Id);
                    if (applyResolutionTree)
                    {
                        result.Add(GetResolveLeaf(cachedObject));
                    }
                    else
                    {
                        result.Add(cachedObject);
                    }
                }
                else
                {
                    KObject finalObj = retObj;
                    if (applyResolutionTree)
                    {
                        //finalObj = GetResolveLeaf(finalObj);
                    }

                    //if (finalObj.IsGroup)
                    //{
                    //    var groupInnerRelationships = await GetRelationshipsByTargetObjectIdAsync(finalObj.Id, GroupMembershipRelationshipsTypeUri);
                    //    result.Add(GenerateRetrivedGroupMasterObject(finalObj, groupInnerRelationships, GroupMembershipRelationshipsTypeUri));
                    //}
                    //else
                    //{
                        notCachedRetrievedObjects.Add(finalObj);
                    //}
                }
            }
            if (notCachedRetrievedObjects.Count > 0)
            {
                result.AddRange(GenerateReterievedObjects(notCachedRetrievedObjects));
            }
            return result;
        }

        /// <summary>
        /// درصورتی که یک شئ منتشر شده، خارج از محیط کاربری ادغام شده باشد، شئ جایگزین آن
        /// را برمی‌گرداند، در غیراینصورت «نال» برمی‌گرداند
        /// </summary>
        public async static Task<KObject> GetResolutionMasterForPublishedObjectIfRemotelyResolved(long objectId)
        {
            if (IsUnpublishedObject(objectId))
                throw new ArgumentException("Remote resolution master can not retrieved for an not-published object");

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            KObject objectLastRemoteVersion;
            try
            {
                objectLastRemoteVersion = (await sc.GetObjectListByIdAsync(new long[] { objectId }))[0];
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }

            // تبدیل به خاطر میانگیری شئ معادل و نیز اعمال درخت ادغام انجام می‌شود
            KWObject objectLastVersion = await GetObjectFromRetrievedDataAsync(objectLastRemoteVersion);

            return GetKObjectFromKWObject(objectLastVersion);
        }
        #endregion

        #region سازنده‌های اشیاء | Singleton Constructions - CRITICAL SECTION
        private static object generateUnpublishedObjectLockObject = new object();
        private static KWObject GenerateNewUnpublishedObject(string typeUri, string displayName, bool isNotUploadedSourceDocument = false)
        {
            KWObject newObject;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک شئ، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی شئ، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده

            lock (generateUnpublishedObjectLockObject)
            {
                long newObjectId = GetNewObjectID();
                newObject = new KWObject()
                {
                    ID = newObjectId,
                    TypeURI = typeUri,
                    ResolvedTo = null,
                    MasterId = newObjectId,
                };
                AddObjectToCache(newObject, false, isNotUploadedSourceDocument);
                KWProperty labelProperty = PropertyManager.CreateNewProperty(ObjectLabelPropertyTypeUri, displayName, newObject);
                newObject.DisplayName = labelProperty;
            }
            return newObject;
        }

        private static object generateUnpublishedGroupMasterObjectLockObject = new object();
        private static GroupMasterKWObject GenerateNewUnpublishedGroupMasterObject(string typeURI, string displayName)
        {
            GroupMasterKWObject newGroupMasterObject;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک شئ، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی شئ، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateUnpublishedGroupMasterObjectLockObject)
            {
                long newGroupMasterObjectId = GetNewObjectID();
                var newGroupInnerRelationships = new KeyValuePair<KWRelationship, KWObject>[] { };
                newGroupMasterObject = new GroupMasterKWObject(newGroupInnerRelationships, GroupMembershipRelationshipsTypeUri)
                {
                    ID = newGroupMasterObjectId,
                    TypeURI = typeURI,
                    ResolvedTo = null
                };
                AddObjectToCache(newGroupMasterObject, false);
                KWProperty labelProperty = PropertyManager.CreateNewProperty(ObjectLabelPropertyTypeUri, displayName, newGroupMasterObject);
                newGroupMasterObject.DisplayName = labelProperty;
            }
            return newGroupMasterObject;
        }

        private static object createNewResolveAndGroupMasterObjectLockObject = new object();
        private static GroupMasterKWObject GenerateNewUnpublishedResolveAndGroupMasterObject(string typeURI, string displayName, IEnumerable<KeyValuePair<KWRelationship, KWObject>> subGroupsGroupLinks)
        {
            GroupMasterKWObject newGroupMasterObject;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک شئ، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی شئ، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (createNewResolveAndGroupMasterObjectLockObject)
            {
                long newGroupMasterObjectId = GetNewObjectID();
                newGroupMasterObject = new GroupMasterKWObject(subGroupsGroupLinks, GroupMembershipRelationshipsTypeUri)
                {
                    ID = newGroupMasterObjectId,
                    TypeURI = typeURI,
                    ResolvedTo = null
                };
                AddObjectToCache(newGroupMasterObject, false);
                KWProperty labelProperty = PropertyManager.CreateNewProperty(ObjectLabelPropertyTypeUri, displayName, newGroupMasterObject);
                newGroupMasterObject.DisplayName = labelProperty;
            }
            return newGroupMasterObject;
        }

        private static KWObject GenerateReterievedObject(KObject retrievedObject)
        {
            return GenerateReterievedObjects(new List<KObject> { retrievedObject }).FirstOrDefault();
        }

        private static object generateRetrivedObjectsLockObject = new object();
        private static List<KWObject> GenerateReterievedObjects(List<KObject> retrievedObjects)
        {
            List<KWObject> newObjects = new List<KWObject>(retrievedObjects.Count);
            List<KObject> notCachedRetrievedObjects = new List<KObject>(retrievedObjects.Count);

            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک شئ، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی شئ، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده

            lock (generateRetrivedObjectsLockObject)
            {
                foreach (var kObj in retrievedObjects)
                {
                    if (IsRetrievedObjectCachedBefore(kObj))
                        newObjects.Add(GetCachedObjectById(kObj.Id));
                    else
                    {
                        KWObject newObject = new KWObject()
                        {
                            ID = kObj.Id,
                            TypeURI = kObj.TypeUri,
                            IsMaster = kObj.IsMaster == 1,
                            MasterId = kObj.Id,
                        };

                        if (kObj.KObjectMaster != null)
                        {
                            newObject.KWObjectMaster = new KWObjectMaster()
                            {
                                Id = kObj.KObjectMaster.Id,
                                MasterId = kObj.KObjectMaster.MasterId,
                                ResolveTo = kObj.KObjectMaster.ResolveTo,
                            };
                            newObject.IsSlave = kObj.IsMaster == 0;
                            newObject.MasterId = kObj.KObjectMaster.MasterId == -1? kObj.Id : kObj.KObjectMaster.MasterId;
                        }

                        AddObjectToCache(newObject, true);
                        notCachedRetrievedObjects.Add(kObj);
                        newObjects.Add(newObject);
                    }
                }
            }

            if (notCachedRetrievedObjects.Count > 0)
            {
                List<KWProperty> newlyAddedObjectLabels = GetLabelPropertyForRetreivedObjects(notCachedRetrievedObjects);
                foreach (KWProperty labelProp in newlyAddedObjectLabels)
                {
                    labelProp.Owner.DisplayName = labelProp;
                    cachedObjectsMetadataIdentifiedByID[labelProp.Owner.ID].CachedObject = labelProp.Owner;
                }
            }
            //TODO : discover Conflict betwwen owner of Label propery and object IDs
            return newObjects;
        }

        private static List<KWProperty> GetLabelPropertyForRetreivedObjects(IEnumerable<KObject> retrievedObjetcs)
        {
            var tsk = Task.Run(() => PropertyManager.GetPropertyListByIdAsync
                (retrievedObjetcs.Where(o => o.LabelPropertyID.HasValue)
                    .Select(o => o.LabelPropertyID.Value)));
            tsk.Wait();
            return tsk.Result;
        }

        private static object generateRetrivedGroupMasterObjectLockObject = new object();

        #endregion

        #region توابع ایجادی/ویرایشی | Need Initialization
        public static KWObject CreateNewObject(string typeURI, string displayName, bool isNotUploadedSourceDocument = false)
        {
            if (typeURI == null)
                throw new ArgumentNullException(nameof(typeURI));
            if (displayName == null)
                throw new ArgumentNullException(nameof(displayName));

            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            return GenerateNewUnpublishedObject(typeURI, displayName, isNotUploadedSourceDocument);
        }
        public static GroupMasterKWObject CreateNewGroupMasterObject(string typeURI, string displayName)
        {
            if (typeURI == null)
                throw new ArgumentNullException(nameof(typeURI));
            if (displayName == null)
                throw new ArgumentNullException(nameof(displayName));

            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            return GenerateNewUnpublishedGroupMasterObject(typeURI, displayName);
        }
        /// <summary>
        /// نمونه شئ میزبان گروه سمت محیط کاربری متناسب را براساس رابطه های عضویت در گروه سمت سرویس دهنده راه دور برمی گرداند
        /// </summary>
        /// <param name="groupMembershipRelationships">نمونه رابطه های عضویت در گروه سمت سرویس دهنده راه دور برای تبدیل</param>
        /// <remarks>
        /// این عملکرد ابتدا لیست اشیائی که قبلا در سمت محیط کاربری ایجاد کرده را بررسی می کند
        /// و در صورت وجود شئی با شناسه خواسته شده (که به معنی ایجاد این شی در گذشته است
        /// در سمت محیط کاربریست) نمونه شی سابق را برای بازگشت در نظر می گیرد و در غیراینصورت
        /// یک شئ میزبان گروه سمت محیط کاربری جدید براساس داده های داده شده ایجاد می کند؛
        /// لینک های نشاندهنده زیرگروه ها در هر دو صورت ایجاد نمونه جدید یا استفاده از نمونه
        /// میانگیری شده، داده های گروه ها براساس ورودی این عملکرد افزوده/به روز می شوند
        /// </remarks>
        /// <returns>شئ سمت محیط کاربری متناظر را برمی گرداند</returns>
        public static GroupMasterKWObject CreateNewGroupOfObjects
            (IEnumerable<KWObject> objectsToGroup
            , string groupMasterObjectTypeURI
            , string groupMasterObjectDisplayName
            , string groupRelationshipsDisplayText
            , DateTime? groupRelationshipsTimeBegin
            , DateTime? groupRelationshipsTimeEnd)
        {
            if (objectsToGroup == null)
                throw new ArgumentNullException(nameof(objectsToGroup));
            if (groupMasterObjectTypeURI == null)
                throw new ArgumentNullException(nameof(groupMasterObjectTypeURI));
            if (groupMasterObjectDisplayName == null)
                throw new ArgumentNullException(nameof(groupMasterObjectDisplayName));
            if (groupRelationshipsDisplayText == null)
                throw new ArgumentNullException(nameof(groupRelationshipsDisplayText));

            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            GroupMasterKWObject newGroupMasterObject =
               GenerateNewUnpublishedGroupMasterObject
                    (groupMasterObjectTypeURI, groupMasterObjectDisplayName);
            var membershipLinks = new List<RelationshipBasedKWLink>();
            foreach (var member in objectsToGroup)
            {
                var membershipLink = LinkManager.CreateNewRelationshipBaseLink
                    (member
                    , newGroupMasterObject
                    , GroupMembershipRelationshipsTypeUri
                    , groupRelationshipsDisplayText
                    , LinkDirection.TargetToSource
                    , groupRelationshipsTimeBegin
                    , groupRelationshipsTimeEnd);
                membershipLinks.Add(membershipLink);
            }
            newGroupMasterObject.AddSubGroupLinksRange(membershipLinks);
            return newGroupMasterObject;

            //RelationshipBaseKlink[] groupMembershipRelationships
            //    = null;

            ////if (groupRelationshipsBaseKLinks == null)
            ////    throw new NullReferenceException();
            ////if (groupMembershipRelationships == null)
            ////    throw new ArgumentNullException("groupMembershipRelationships");

            //GroupMasterKWObject result = null;
            //CachedObjectMetadata chachedGroupMaster;
            //// استخراج شی میزبان گروه براساس نتیجه برگشتی از سرویس دهنده راه دور
            //KObject groupMasterObject = groupMembershipRelationships.First().Target;
            //if (!groupMasterObject.IsGroup)
            //    throw new Exception("Invalid server response; Group-master Object is not defined as a Group-master");
            //// استخراج زیرگروه ها و روابط عضویت آن ها با شی میزبان گره
            //List<KeyValuePair<KWRelationship, KWObject>> groupMembersRelationshipWithMaster = new List<KeyValuePair<KWRelationship, KWObject>>();
            //foreach (var item in groupMembershipRelationships)
            //{
            //    if (item.TypeURI != groupRelationshipsTypeURI)
            //        throw new Exception("Invalid server response; At least one of the returned Group internal Relationships (Group Member Of) is not valid.\r\nServer completes group creation (storing data) but Workspace is unable to use retrived data");
            //    if (item.Target.Id != groupMasterObject.Id)
            //        throw new Exception("Invalid server response; At least one of the returned members is not defined as member of this group\r\nServer completes group creation (storing data) but workspace is unable to use retrived data");
            //    groupMembersRelationshipWithMaster.Add(
            //        new KeyValuePair<KWRelationship, KWObject>
            //            (LinkManager.GetKWRelationshipFromKRelationship(item.Relationship, item.TypeURI)
            //            , GetKWObjectFromKObjectAsync(item.Source)));
            //}
            //// تلاش برای یافتن شناسه شی متناظر در سمت محیط کاربری بین اشیائی که قبلا میانگیری شده اند
            //if (cachedObjectsMetadataSortedByID.TryGetValue(groupMasterObject.Id, out chachedGroupMaster))
            //{
            //    // در صورت میانگیری قبلی، نمونه شی میانگیری شده نتیجه مورد نظر خواهد بود
            //    result = chachedGroupMaster.CachedObject as GroupMasterKWObject;
            //    // به روز رسانی زیرمجموعه های گروه براساس آخرین مقادیر داده شده
            //    List<RelationshipBasedKWLink> updatedMembershipLinks = new List<RelationshipBasedKWLink>();
            //    foreach (var item in groupMembershipRelationships)
            //        updatedMembershipLinks.Add(await LinkManager.GetRelationshipBaseKWLinkFromRelationshipBaseKLinkAsync(item));
            //    result.UpdateSubGroupLinks(updatedMembershipLinks);
            //}
            //else
            //{
            //    // در صورت عدم میانگیری قبلی، ایجاد شی جدید براساس خصوصیت های دریافتی از سرویس دهنده راه دور
            //    result =
            //        new GroupMasterKWObject(groupMembersRelationshipWithMaster, groupRelationshipsTypeURI) { ID = groupMasterObject.Id, TypeURI = groupMasterObject.TypeUri, DisplayName = groupMasterObject.DisplayName };
            //    // افزودن شی به تازگی ایجاد شده به لیست مرتب شده اشیا میانگیری شده سمت محیط کاربری
            //    cachedObjectsMetadataSortedByID.Add(groupMasterObject.Id, new CachedObjectMetadata()
            //    {
            //        CachedObject = result,
            //        IsModified = false
            //    });
            //}
            //if (result == null)
            //    throw new NullReferenceException("Unable to retrive/construct 'Group-master object' in workspace side.");
            //// بازگرداندن شی ایجاد/بازیابی شده
            //return result;
        }
        public static void ChangeDisplayNameOfObject(KWObject editingObject, string newDisplayName)
        {
            if (editingObject == null)
                throw new ArgumentNullException(nameof(editingObject));
            if (newDisplayName == null)
                throw new ArgumentNullException(nameof(newDisplayName));

            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            var objectMetadata = GetCachedMetadataById(editingObject.ID);
            PropertyManager.UpdatePropertyValue(objectMetadata.CachedObject.DisplayName, newDisplayName);
        }

        /// <summary>
        /// حذف یک شئ (به همراه ویژگی‌ها، رابطه‌ها و مدیاهای آن)؛
        /// این تابع شئ داده شده را به صورت نرم، حذف می‌کند؛
        /// حذف نرم به معنای عدم دستکاری در مخزن داده‌ها بوده و
        /// فقط برای شئ‌های منتشر نشده قابل انجام است
        /// </summary>
        public static async Task DeleteObjectAsync(KWObject objectToDelete)
        {
            if (objectToDelete == null)
                throw new ArgumentNullException(nameof(objectToDelete));

            if (IsUnpublishedObject(objectToDelete))
            {
                var properties = (await PropertyManager.GetPropertiesForObjectAsync(objectToDelete)).ToList();
                foreach (var item in properties)
                {
                    PropertyManager.DeleteProperty(item);
                }

                var relationshipsMetadata = LinkManager.GetUnpublishedRelationships()
                    .Where(r => r.RelationshipSourceId == objectToDelete.ID
                        || r.RelationshipTargetId == objectToDelete.ID)
                    .ToList();
                foreach (var item in relationshipsMetadata)
                {
                    LinkManager.DeleteRelationship(item.CachedRelationship);
                }

                var medias = (await MediaManager.GetMediaForObjectAsync(objectToDelete)).ToList();
                foreach (var item in medias)
                {
                    MediaManager.DeleteMedia(item);
                }

                cachedObjectsMetadataIdentifiedByID.Remove(objectToDelete.ID);
            }
            else
                throw new InvalidOperationException("Unable to delete published object");
        }
        #endregion

        #region توابع بازیابی | Need Initialization | گلوگاه اعمال ادغام
        public async static Task<KWObject> GetObjectByIdAsync(long objectId, bool applyResolutionTree = true)
        {
            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            var singleIdArray = new long[] { objectId };
            KWObject resultObj = (await GetObjectsListByIdAsync(singleIdArray, applyResolutionTree))[0];
            return resultObj;
        }

        public async static Task<List<KWObject>> GetObjectsListByIdAsync(IEnumerable<long> objectIdsToRetrieve, bool applyResolutionTree = true)
        {
            if (objectIdsToRetrieve == null)
                throw new ArgumentNullException(nameof(objectIdsToRetrieve));
            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            int countOfObjectIdsToRetrieve = objectIdsToRetrieve.Count();

            List<long> unpublishedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            List<long> publishedCachedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            List<long> publishedNotCachedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            foreach (long objID in objectIdsToRetrieve)
            {
                if (IsUnpublishedObject(objID))
                {
                    unpublishedObjectIds.Add(objID);
                }
                else
                {
                    if (IsObjectCached(objID))
                    {
                        publishedCachedObjectIds.Add(objID);
                    }
                    else
                    {
                        publishedNotCachedObjectIds.Add(objID);
                    }
                }
            }

            HashSet<KWObject> result = new HashSet<KWObject>();

            if (unpublishedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(GetUnpublishedObjectsById(unpublishedObjectIds), ref result, applyResolutionTree);
            }
            if (publishedCachedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(publishedCachedObjectIds.Select(id => GetCachedObjectById(id)), ref result, applyResolutionTree);
            }
            if (publishedNotCachedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(await RetriveObjectsByIdAsync(objectIdsToRetrieve.ToList()), ref result, applyResolutionTree);
            }

            // TODO: آینده - در اینجا می‌توان ویژگی‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد؛ همچنین
            // درصورتی که شناسایی تفاوت بین داده‌های میانگیری شده با داده‌های
            // مخزن، در اولین فرصت ممکن، مهم باشد می‌بایست به جای خواندن از
            // میانگیر، حتما از مخزن خواند و تفاوت‌ها را استخراج کرد

            return result.ToList();
        }

        public async static Task<List<KWObject>> GetObjectsListByIdAsync(IEnumerable<KObject> objectIdsToRetrieve, bool applyResolutionTree = true)
        {
            if (objectIdsToRetrieve == null)
                throw new ArgumentNullException(nameof(objectIdsToRetrieve));
            if (!IsInitialized())
                throw new InvalidOperationException("Manager is not initialized");

            int countOfObjectIdsToRetrieve = objectIdsToRetrieve.Count();

            List<long> unpublishedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            List<long> publishedCachedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            List<long> publishedNotCachedObjectIds = new List<long>(countOfObjectIdsToRetrieve);
            foreach (KObject objID in objectIdsToRetrieve)
            {
                if (IsUnpublishedObject(objID.Id))
                {
                    unpublishedObjectIds.Add(objID.Id);
                }
                else
                {
                    if (IsObjectCached(objID.Id))
                    {
                        publishedCachedObjectIds.Add(objID.Id);
                    }
                    else
                    {
                        publishedNotCachedObjectIds.Add(objID.Id);
                    }
                }
            }

            HashSet<KWObject> result = new HashSet<KWObject>();

            if (unpublishedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(GetUnpublishedObjectsById(unpublishedObjectIds), ref result, applyResolutionTree);
            }
            if (publishedCachedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(publishedCachedObjectIds.Select(id => GetCachedObjectById(id)), ref result, applyResolutionTree);
            }
            if (publishedNotCachedObjectIds.Count > 0)
            {
                AddObjectsToHashSet(await RetriveObjectsByIdAsync(objectIdsToRetrieve.ToList()), ref result, applyResolutionTree);
            }

            // TODO: آینده - در اینجا می‌توان ویژگی‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد؛ همچنین
            // درصورتی که شناسایی تفاوت بین داده‌های میانگیری شده با داده‌های
            // مخزن، در اولین فرصت ممکن، مهم باشد می‌بایست به جای خواندن از
            // میانگیر، حتما از مخزن خواند و تفاوت‌ها را استخراج کرد

            return result.ToList();
        }

        private static void AddObjectsToHashSet(IEnumerable<KWObject> objectsToAdd, ref HashSet<KWObject> destinationSet, bool applyResolutionTree)
        {
            if (applyResolutionTree == true)
            {
                foreach (KWObject objToAdd in objectsToAdd.Select(o => GetResolveLeaf(o)))
                {
                    AddObjectToHashSet(objToAdd, ref destinationSet);
                }
            }
            else
            {
                foreach (KWObject objToAdd in objectsToAdd)
                {
                    AddObjectToHashSet(objToAdd, ref destinationSet);
                }
            }
        }

        private static void AddObjectToHashSet(KWObject objToAdd, ref HashSet<KWObject> destinationSet)
        {
            if (!destinationSet.Contains(objToAdd))
            {
                destinationSet.Add(objToAdd);
            }
        }
        #endregion

        #region توابع مدیریت ادغام اشیاء
        public static KWObject ResolveObjects(List<KWObject> objectsToResolve, string resolveMasterDisplayName)
        {
            KWObject resolveMasterObject;
            if (objectsToResolve.Any(o => o is GroupMasterKWObject))
            {
                IEnumerable<KeyValuePair<KWRelationship, KWObject>> resolvewdObjectsGroupLinks =
                    objectsToResolve
                        .Where(o => o is GroupMasterKWObject)
                        .Select(o => o as GroupMasterKWObject)
                        .SelectMany(g => g.GroupLinks)
                        .Select(gl => new KeyValuePair<KWRelationship, KWObject>(gl.Relationship, gl.Source));
                resolveMasterObject = GenerateNewUnpublishedResolveAndGroupMasterObject(objectsToResolve.FirstOrDefault().TypeURI, resolveMasterDisplayName, resolvewdObjectsGroupLinks);
            }
            else
            {
                resolveMasterObject = CreateNewObject(objectsToResolve.FirstOrDefault().TypeURI, resolveMasterDisplayName);
            }

            foreach (var currentResolvedObject in objectsToResolve)
            {
                SetCachedObjectAsLocallyResolved(currentResolvedObject);
                PropertyManager.ChangeOwnerOfCachedPropertiesForObject(currentResolvedObject, resolveMasterObject);
                MediaManager.ChangeOwnerOfCachedMediasForObject(currentResolvedObject, resolveMasterObject);
                LinkManager.ChangeOwnerOfCachedRelationshipsForObject(currentResolvedObject, resolveMasterObject);
                currentResolvedObject.ResolvedTo = resolveMasterObject;
            }
            SetObjectAsResolveMasterOfObjects(resolveMasterObject, objectsToResolve);
            return resolveMasterObject;
        }

        public static KWObject GetResolveLeaf(KWObject obj)
        {
            if (obj.ResolvedTo == null)
            {
                return obj;
            }
            else
            {
                return GetResolveLeaf(obj.ResolvedTo);
            }
        }

        public static KWObject[] GetObjectWhereLocallyResolvedToObject(long objID)
        {
            if (IsObjectCached(objID))
            {
                CachedObjectMetadata metadata = GetCachedMetadataById(objID);
                return metadata.ObjectsWhereLocallyResolvedTo;
            }
            else
            {
                return new KWObject[] { };
            }
        }
        #endregion


        #region Deprecated Methods
        public static List<KObject> ConvertKWObjectsToKObjectList(IEnumerable<KWObject> kwObjectList)
        {
            List<KObject> kobjectList = new List<KObject>();
            foreach (var item in kwObjectList)
            {
                KObject kobject = GetKObjectFromKWObject(item);
                kobjectList.Add(kobject);
            }
            return kobjectList;
        }

        /// <summary>
        /// نمونه شئ سمت سرویس دهنده راه دور متناسب را براساس مقادیر سمت  محیط کاربری برمی گرداند
        /// </summary>
        /// <param name="objectToConvert">نمونه شئ سمت محیط کاربری برای تبدیل</param>
        /// <remarks>
        /// این عملکرد براساس سازوکار موجود که صرفا ساختاری برای پاس دادن اشیا است، نمونه ای از کلاس مورد نظر
        /// را براساس خصوصیات شی مبدا (سمت محیط کاربری) ایجاد می کند و برمی گرداند
        /// </remarks>
        /// <returns>یک نمونه شئ سمت سرویس دهنده راه دور متناسب ایجاد می کند و برمی گرداند</returns>
        public static KObject GetKObjectFromKWObject(KWObject objectToConvert)
        {
            if (objectToConvert == null)
                throw new ArgumentNullException(nameof(objectToConvert));

            // بازگرداندن شی معادل سمت سرور
            return CreateNewKObject
                (objectToConvert.ID
                , (objectToConvert.DisplayName == null) ? null : new long?(objectToConvert.DisplayName.ID)
                , objectToConvert.TypeURI
                , objectToConvert is GroupMasterKWObject
                , (objectToConvert.ResolvedTo == null || IsUnpublishedObject(objectToConvert.ResolvedTo))
                    ? null
                    : GetKObjectFromKWObject(objectToConvert.ResolvedTo));
        }

        public static List<KObject> GetKObjectsFromKWObjects(List<KWObject> objectsToConvert)
        {
            List<KObject> result = new List<KObject>();

            if (objectsToConvert == null)
                throw new ArgumentNullException(nameof(objectsToConvert));

            foreach (var currentObject in objectsToConvert)
            {
                result.Add(GetKObjectFromKWObject(currentObject));
            }

            return result;
        }

        public static KObject CreateNewKObject(long id, long? labelPropertyID, string typeUri, bool isGroup, KObject resolvedTo)
        {
            return new KObject()
            {
                Id = id,
                LabelPropertyID = labelPropertyID,
                TypeUri = typeUri
            };
        }

        public static KObject GenerateEmptyKObjectByID(long id)
        {
            // بازگرداندن شی معادل سمت سرور
            return CreateNewKObject(id, null, "", false, null);
        }

        #endregion
    }
}