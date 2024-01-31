using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager
{
    /// <summary>
    /// مدیریت داده‌ای ویژگی‌ها سمت محیط کاربری
    /// </summary>
    public class PropertyManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private PropertyManager()
        { }

        #region  مدیریت میانگیری و رفع تصادم‌ها | Caching & Conflicts resolution
        internal class CachedPropertyMetadata
        {
            internal KWProperty CachedProperty;
            internal bool IsPublished;
            internal bool IsModified;
        }

        /// <summary>
        /// لیست مرتب شده ویژگی های ایجاد شده در سمت محیط کاربری که براساس شناسه شان مرتب شده اند؛
        /// این لیست تضمین می کند که به ازای هر ویژگی یکتا در سمت محیط کاربری، بیش از یک نمونه ساخته نشود.
        /// این لیست منحصرا توسط تابع تبدیل نمونه شی سمت سرویس دهنده راه دور به نمونه شی سمت محیط کاربری «تغییر» می کند.
        /// </summary>
        private static Dictionary<long, CachedPropertyMetadata> cachedPropertiesMetadataIdentifiedByID
            = new Dictionary<long, CachedPropertyMetadata>();

        private static void AddPropertyToCache(KWProperty property, bool isPublished)
        {
            var newPropertyMetadata = new CachedPropertyMetadata()
            {
                CachedProperty = property,
                IsPublished = isPublished,
                IsModified = false
            };
            cachedPropertiesMetadataIdentifiedByID.Add(property.ID, newPropertyMetadata);
        }
        private static void SetCachedPropertyAsModified(KWProperty property, string newValue)
        {
            CachedPropertyMetadata propertyMetadata = GetCachedMetadataById(property.ID);
            if (property is GeoTimeKWProperty)
            {
                (property as GeoTimeKWProperty).GeoTimeValue = GeoTime.GeoTimeEntityRawData(newValue);
            }

            if (property is GeoPointKWProperty)
            {
                (property as GeoPointKWProperty).GeoLocationValue = GeoPoint.GeoPointEntityRawData(newValue);
            }

            propertyMetadata.CachedProperty.Value = newValue;
            propertyMetadata.IsModified = true;
        }
        private static List<KWProperty> GetUnpublishedPropertiesById(List<long> unpublishedPropertyIds)
        {
            var result = new List<KWProperty>(unpublishedPropertyIds.Count);
            foreach (var id in unpublishedPropertyIds)
            {
                var property = cachedPropertiesMetadataIdentifiedByID[id].CachedProperty;
                result.Add(property);
            }
            return result;
        }
        internal static IEnumerable<KWObject> GetUnpublishedPropertiesOwner()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Select(propertyMetadata => propertyMetadata.CachedProperty.Owner);
        }
        //internal static IEnumerable<KWProperty> GetUnpublishedProperties()
        //{
        //    return cachedPropertiesMetadataSortedByID
        //        .Values
        //        .Select(propertyMetadata => propertyMetadata.CachedProperty);
        //}

        private static IEnumerable<KWProperty> GetUnpublishedSpecifiedPropertiesForObjects(IEnumerable<string> propertyTypes, IEnumerable<KWObject> objectsToGetTheirProperties)
        {
            var result = cachedPropertiesMetadataIdentifiedByID
                .Values
                .Select(metadata => metadata.CachedProperty)
                .Where(cachedProperty
                   => objectsToGetTheirProperties.Contains(cachedProperty.Owner)
                   && propertyTypes.Contains(cachedProperty.TypeURI)
                   && IsUnpublishedProperty(cachedProperty));
            return result;
        }
        private static void UpdateCacheByRetrievedProperty(KProperty item)
        {
            // راهکار سابق برای رفع تصادم بین داده‌های محلی و راه‌دور
            //if (isretrievedpropertyconflictswithcache(item))
            //{
            //    resolveretrievedandcachedpropertyconflicts(item);
            //}
        }
        private static KWProperty GetCachedPropertyById(long propertyId)
        {
            return GetCachedMetadataById(propertyId).CachedProperty;
        }
        private static CachedPropertyMetadata GetCachedMetadataById(long propertyId)
        {
            return cachedPropertiesMetadataIdentifiedByID[propertyId];
        }
        private static bool IsRetrievedPropertyCachedBefore(KProperty item)
        {
            return cachedPropertiesMetadataIdentifiedByID.ContainsKey(item.Id);
        }
        private static bool IsRetrievedPropertyConflictsWithCache(KProperty retrievedProperty)
        {
            var cachedPropertyMetadata = GetCachedMetadataById(retrievedProperty.Id);
            var cachedProperty = cachedPropertyMetadata.CachedProperty;

            if (cachedPropertyMetadata.IsModified)
            {
                return true;
            }
            else
            {
                // TODO: آینده - درصورتی که مقدار ویژگی، پس از یک بار میانگیری، در
                // مخزن داده‌ها به‌روز شود، این حالت (در حال حاضر) تصادم تلقی می‌شود؛
                // می‌توان برای این اتفاق راهکار بهتری داشت.
                if (cachedProperty.Value.Equals(retrievedProperty.Value))
                    return false;
                else
                    return true;
            }
        }

        private static void DiscardUnpublishedChangesForProperty(KProperty retrievedProperty)
        {
            var cachedPropertyMetadata = GetCachedMetadataById(retrievedProperty.Id);
            cachedPropertyMetadata.CachedProperty.Value = retrievedProperty.Value;
            cachedPropertyMetadata.IsModified = false;
        }
        private static void ChangeCachedPropertyId(long currentId, long newId)
        {
            var mediaMetadata = GetCachedMetadataById(currentId);
            cachedPropertiesMetadataIdentifiedByID.Remove(currentId);
            cachedPropertiesMetadataIdentifiedByID.Add(newId, mediaMetadata);
            mediaMetadata.CachedProperty.ID = newId;
            mediaMetadata.IsModified = false;
        }

        internal static IEnumerable<KWProperty> GetLocallyChangedProperties()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata => IsUnpublishedProperty(propertyMetadata.CachedProperty) || propertyMetadata.IsModified)
                .Select(propertyMetadata => propertyMetadata.CachedProperty);
        }

        internal static IEnumerable<KWProperty> GetUnpublishedProperties()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata => IsUnpublishedProperty(propertyMetadata.CachedProperty))
                .Select(propertyMetadata => propertyMetadata.CachedProperty);
        }
        private static IEnumerable<KWProperty> GetCachedPublishedPropertiesModifiedLocally()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata
                    => propertyMetadata.IsModified
                    && !IsUnpublishedProperty(propertyMetadata.CachedProperty))
                .Select(metadata => metadata.CachedProperty);
        }

        internal static IEnumerable<CachedPropertyMetadata> GetUnpublishedPropertiesCachedMetadatas()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata => IsUnpublishedProperty(propertyMetadata.CachedProperty)).ToList();
        }
        private static IEnumerable<CachedPropertyMetadata> GetCachedMetadataOfPublishedPropertiesModifiedLocally()
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata
                    => propertyMetadata.IsModified
                    && !IsUnpublishedProperty(propertyMetadata.CachedProperty)).ToList();
        }

        internal static void ClearCache()
        {
            cachedPropertiesMetadataIdentifiedByID.Clear();
        }

        internal static void AddSavedMetadataToCache(List<CachedPropertyMetadata> propertiesMetadata)
        {
            foreach (var currentMetadata in propertiesMetadata)
            {
                if (!cachedPropertiesMetadataIdentifiedByID.ContainsKey(currentMetadata.CachedProperty.ID))
                {
                    cachedPropertiesMetadataIdentifiedByID.Add(currentMetadata.CachedProperty.ID, currentMetadata);
                }
                else
                {
                    UpdateCacheBySavedMetadata(currentMetadata);
                }
            }
        }

        private static void UpdateCacheBySavedMetadata(CachedPropertyMetadata propertyMetadata)
        {
            if (cachedPropertiesMetadataIdentifiedByID.ContainsKey(propertyMetadata.CachedProperty.ID))
            {
                CachedPropertyMetadata existanceMetadataInCache = cachedPropertiesMetadataIdentifiedByID[propertyMetadata.CachedProperty.ID];
                existanceMetadataInCache.CachedProperty = propertyMetadata.CachedProperty;
                existanceMetadataInCache.IsModified = propertyMetadata.IsModified;
                existanceMetadataInCache.IsPublished = propertyMetadata.IsPublished;
            }
        }

        private static IEnumerable<KWProperty> GetCachedPropertiesForObject(KWObject obj)
        {
            return cachedPropertiesMetadataIdentifiedByID
                .Values
                .Where(propertyMetadata => propertyMetadata.CachedProperty.Owner.Equals(obj))
                .Select(propertyMetadata => propertyMetadata.CachedProperty);
        }
        internal static bool IsPropertyCached(long propertyId)
        {
            return cachedPropertiesMetadataIdentifiedByID.ContainsKey(propertyId);
        }

        public static bool IsUnpublishedProperty(KWProperty propertyToCheck)
        {
            if (propertyToCheck == null)
                throw new ArgumentNullException("propertyToCheck");

            return IsUnpublishedProperty(propertyToCheck.ID);
        }
        public static bool IsUnpublishedProperty(long propertyId)
        {
            if (IsPropertyCached(propertyId))
            {
                var metadata = GetCachedMetadataById(propertyId);
                return !metadata.IsPublished;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region خدمات انتشار ویژگی‌ها | Publish services
        public static UnpublishedPropertyChanges GetUnpublishedChanges()
        {
            var changes = new UnpublishedPropertyChanges();
            changes.AddedProperties = GetUnpublishedProperties();
            changes.ModifiedProperties = GetCachedPublishedPropertiesModifiedLocally();
            return changes;
        }
        internal static List<CachedPropertyMetadata> GetCachedPropertyMetadatas()
        {
            List<CachedPropertyMetadata> cachedPropertyMetadatas = new List<CachedPropertyMetadata>();
            List<CachedPropertyMetadata> addedProperties = GetUnpublishedPropertiesCachedMetadatas().ToList();
            List<CachedPropertyMetadata> modifiedProperties = GetCachedMetadataOfPublishedPropertiesModifiedLocally().ToList();

            cachedPropertyMetadatas.AddRange(addedProperties);
            cachedPropertyMetadatas.AddRange(modifiedProperties);
            return cachedPropertyMetadatas;
        }

        public static void CommitUnpublishedChanges(IEnumerable<long> addedPropertyIDs, IEnumerable<long> modifiedPropertyIDs, long dataSourceID)
        {
            if (addedPropertyIDs == null)
                throw new ArgumentNullException(nameof(addedPropertyIDs));
            if (modifiedPropertyIDs == null)
                throw new ArgumentNullException(nameof(modifiedPropertyIDs));

            ApplyPropertiesAdditionToCache(addedPropertyIDs);
            ApplyPropertiesModificationToCache(modifiedPropertyIDs);
            SetDataSourceIdForAddedProperties(addedPropertyIDs, dataSourceID);
        }

        private static void SetDataSourceIdForAddedProperties(IEnumerable<long> addedPropertyIDs, long dataSourceID)
        {
            foreach (long propID in addedPropertyIDs)
            {
                KWProperty property = GetCachedPropertyById(propID);
                property.DataSourceId = dataSourceID;
            }
        }

        /// <summary>
        /// تغییرات محلی و همچنین میانگیری‌های قبلی را از بین می‌برد
        /// </summary>
        public static void DiscardChanges()
        {
            cachedPropertiesMetadataIdentifiedByID.Clear();
        }

        private static bool IsAnyUnpublishedChangeRemains()
        {
            bool result = false;
            foreach (var metadata in cachedPropertiesMetadataIdentifiedByID.Values)
                if (IsUnpublishedProperty(metadata.CachedProperty)
                    || metadata.IsModified)
                {
                    result = true;
                    break;
                }
            return result;
        }
        private static void ApplyPropertiesAdditionToCache(IEnumerable<long> addedPropertyIDs)
        {
            var IdList = addedPropertyIDs.ToList();
            while (IdList.Count > 0)
            {
                var metadata = cachedPropertiesMetadataIdentifiedByID[IdList[0]];
                metadata.IsPublished = true;
                metadata.IsModified = false;
                // از آنجایی که دیکشنری میانگیری، حاوی آخرین تغییرات محلی برای
                // ویژگی‌ها می‌باشد، بجز برداشتن تگ، نیاز به کار دیگری نیست
                IdList.RemoveAt(0);
            }
        }
        private static void ApplyPropertiesModificationToCache(IEnumerable<long> modifiedPropertyIDs)
        {
            var IdOfPropertiesToModify = modifiedPropertyIDs.ToList();
            while (IdOfPropertiesToModify.Count > 0)
            {
                var modifiedPropertyMetadata
                    = cachedPropertiesMetadataIdentifiedByID[IdOfPropertiesToModify[0]];
                modifiedPropertyMetadata.IsModified = false;
                // از آنجایی که دیکشنری میانگیری، حاوی آخرین تغییرات محلی برای
                // ویژگی‌ها می‌باشد، بجز برداشتن تگ، نیاز به کار دیگری نیست
                IdOfPropertiesToModify.RemoveAt(0);
            }
        }
        #endregion

        #region تعامل با سرویس‌دهنده راه‌دور
        private async static Task<IEnumerable<KWProperty>> RetrivePropertiesByIdAsync(IEnumerable<long> publishedPropertyIds)
        {
            // ارسال درخواست ایجاد ویژگی برای یک شئ، به سرویس دهنده ی راه دور
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            KProperty[] retrivedKProperties = null;
            try
            {
                retrivedKProperties = await sc.GetPropertyListByIdAsync(publishedPropertyIds.ToArray());
            }
            finally
            {
                sc.Close();
            }
            // بررسی اعتبار مقدار بازگشتی از سرویس دهنده راه دور
            if (retrivedKProperties == null)
                throw new InvalidOperationException("Invalid server respone");
            // ایجاد نمونه‌ی سمت محیط کاربری متناسب با نتیجه برگردانده شده از سرویس دهنده راه دور
            KWProperty[] retrievedProperties = await GetPropertyFromRetrievedDataArrayAsync(retrivedKProperties);
            return retrievedProperties;
        }

        public static bool IsModifiedProperty(KWProperty propertyToCheck)
        {
            if (propertyToCheck == null)
                throw new ArgumentNullException("propertyToCheck");
            return cachedPropertiesMetadataIdentifiedByID[propertyToCheck.ID].IsModified;
        }

        private async static Task<IEnumerable<KWProperty>> RetrivePropertiesForObjectAsync(KWObject objectToGetProperties)
        {
            // ارسال درخواست دریافت ویژگی های یک شئ، به سرویس دهنده ی راه دور
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            KProperty[] retrivedKProperties = null;
            try
            {
                retrivedKProperties = await sc.GetPropertyForObjectAsync(ObjectManager.GetKObjectFromKWObject(objectToGetProperties));
            }
            finally
            {
                sc.Close();
            }
            if (retrivedKProperties == null)
                throw new InvalidOperationException("Invalid server respone");
            // ایجاد نمونه ی سمت محیط کاربری متناسب با نتیجه برگردانده شده از سرویس دهنده راه دور
            KWProperty[] propertiesForObject = await GetPropertyFromRetrievedDataArrayAsync(retrivedKProperties);
            return propertiesForObject;
        }
        private async static Task<IEnumerable<KWProperty>> RetriveSpecifiedPropertiesForObjectsAsync(IEnumerable<string> propertyTypes, IEnumerable<KWObject> objectsToGetTheirProperties)
        {
            // ارسال درخواست دریافت ویژگی های یک شئ، به سرویس دهنده ی راه دور
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            KProperty[] retrivedKProperties = null;
            try
            {
                retrivedKProperties = await sc.GetSpecifiedPropertiesOfObjectsByTypesAsync(objectsToGetTheirProperties.Select(o => o.ID).ToArray(), propertyTypes.ToArray());
            }
            finally
            {
                sc.Close();
            }
            if (retrivedKProperties == null)
                throw new InvalidOperationException("Invalid server respone");
            // ایجاد نمونه ی سمت محیط کاربری متناسب با نتیجه برگردانده شده از سرویس دهنده راه دور
            KWProperty[] propertiesForObject = await GetPropertyFromRetrievedDataArrayAsync(retrivedKProperties);
            return propertiesForObject;
        }
        public async static Task<KWProperty> GetPropertyFromRetrievedDataAsync(KProperty retrievedProperty)
        {
            if (retrievedProperty == null)
                throw new ArgumentNullException("retrievedProperty");

            KWProperty result = null;
            if (IsRetrievedPropertyCachedBefore(retrievedProperty))
            {
                UpdateCacheByRetrievedProperty(retrievedProperty);
                result = GetCachedPropertyById(retrievedProperty.Id);
            }
            else
            {
                result = await GenerateReterievedPropertyAsync(retrievedProperty);
            }
            return result;
        }

        //public async static Task<KWProperty[]> GetPropertyFromSavedDataArrayAsync(KProperty[] savedProperties)
        //{
        //    if (savedProperties == null)
        //        throw new ArgumentNullException(nameof(savedProperties));

        //    KWProperty[] result = GetPropertyFromRetrievedDataArrayAsync(savedProperties);
        //    for (int i = 0; i < result.Length; i++)
        //    {
        //        cachedPropertiesMetadataIdentifiedByID[result[i].ID].IsPublished = false;
        //    }
        //    return result;
        //}
        public async static Task<KWProperty[]> GetPropertyFromRetrievedDataArrayAsync(KProperty[] retrievedProperty)
        {
            if (retrievedProperty == null)
                throw new ArgumentNullException("retrievedProperty");

            KWProperty[] result = new KWProperty[retrievedProperty.Length];
            for (int i = 0; i < retrievedProperty.Length; i++)
            {
                result[i] = await GetPropertyFromRetrievedDataAsync(retrievedProperty[i]);
            }
            return result;
        }

        private static long GetNewPropertyId()
        {
            long propertyId = -1;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                propertyId = sc.GetNewPropertyId();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            if (propertyId == -1)
                throw new NullReferenceException("Invalid server response");

            return propertyId;
        }
        #endregion

        #region سازنده‌های ویژگی‌ها | Singleton Constructions - CRITICAL SECTION
        private static object generateNewUnpublishedPropertyLockObject = new object();
        private static KWProperty GenerateNewUnpublishedProperty(string typeUri, string propertyValue, KWObject owner)
        {
            KWProperty newProperty;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک ویژگی، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی ویژگی، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateNewUnpublishedPropertyLockObject)
            {
                long newPropertyId = GetNewPropertyId();
                if (typeUri.Equals(System.GetOntology().GetDateRangeAndLocationPropertyTypeUri()))
                {
                    newProperty = new GeoTimeKWProperty()
                    {
                        GeoTimeValue = GeoTime.GeoTimeEntityRawData(propertyValue)
                    };
                }
                else if (System.GetOntology().GetBaseDataTypeOfProperty(typeUri) == BaseDataTypes.GeoPoint)
                {
                    newProperty = new GeoPointKWProperty()
                    {
                        GeoLocationValue = GeoPoint.GeoPointEntityRawData(propertyValue)
                    };
                }
                else
                {
                    newProperty = new KWProperty();
                }

                newProperty.ID = newPropertyId;
                newProperty.TypeURI = typeUri;
                newProperty.Value = propertyValue;
                newProperty.Owner = owner;
                newProperty.DataSourceId = null;

                AddPropertyToCache(newProperty, false);
            }
            return newProperty;
        }

        private static object generateReterievedPropertyLockObject = new object();
        private async static Task<KWProperty> GenerateReterievedPropertyAsync(KProperty retrievedProperty)
        {
            KWObject owner = await ObjectManager.GetObjectByIdAsync(retrievedProperty.Owner.Id);

            KWProperty newProperty;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک ویژگی، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی ویژگی، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateReterievedPropertyLockObject)
            {
                if (IsRetrievedPropertyCachedBefore(retrievedProperty))
                    newProperty = GetCachedPropertyById(retrievedProperty.Id);
                else
                {
                    if (retrievedProperty.TypeUri.Equals(System.GetOntology().GetDateRangeAndLocationPropertyTypeUri()))
                    {
                        newProperty = new GeoTimeKWProperty()
                        {
                            GeoTimeValue = GeoTime.GeoTimeEntityRawData(retrievedProperty.Value)
                        };
                    }
                    else if (System.GetOntology().GetBaseDataTypeOfProperty(retrievedProperty.TypeUri) == BaseDataTypes.GeoPoint )
                    {
                        newProperty = new GeoPointKWProperty()
                        {
                            GeoLocationValue = GeoPoint.GeoPointEntityRawData(retrievedProperty.Value)
                        };
                    }
                    else
                    {
                        newProperty = new KWProperty();
                    }
                    newProperty.ID = retrievedProperty.Id;
                    newProperty.TypeURI = retrievedProperty.TypeUri;
                    newProperty.Owner = owner;
                    newProperty.Value = retrievedProperty.Value;
                    newProperty.DataSourceId = retrievedProperty.DataSourceID;

                    AddPropertyToCache(newProperty, true);
                }
            }
            return newProperty;
        }
        #endregion

        #region توابع ایجادی/ویرایشی
        public static KWProperty CreateNewProperty(string typeUri, string propertyValue, KWObject owner)
        {
            if (typeUri == null)
                throw new ArgumentNullException("typeUri");
            if (propertyValue == null)
                throw new ArgumentNullException("propertyValue");
            if (owner == null)
                throw new ArgumentNullException("owner");

            return GenerateNewUnpublishedProperty(typeUri, propertyValue, owner);
        }
        public static KWProperty UpdatePropertyValue(KWProperty propertyToEdit, string newValue)
        {
            if (propertyToEdit == null)
                throw new ArgumentNullException("propertyToEdit");
            if (newValue == null)
                throw new ArgumentNullException("newValue");

            SetCachedPropertyAsModified(propertyToEdit, newValue);
            return propertyToEdit;
        }

        /// <summary>
        /// حذف یک ویژگی؛
        /// این تابع ویژگی داده شده را به صورت نرم، حذف می‌کند؛
        /// حذف نرم به معنای عدم دستکاری در مخزن داده‌ها بوده و
        /// فقط برای ویژگی‌های منتشر نشده قابل انجام است
        /// </summary>
        public static void DeleteProperty(KWProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (IsUnpublishedProperty(property))
            {
                cachedPropertiesMetadataIdentifiedByID.Remove(property.ID);
                property.OnDeleted();
            }
            else
                throw new InvalidOperationException("Unable to delete published property");
        }
        #endregion

        #region توابع بازیابی

        public static IEnumerable<KWProperty> GetUnpublishedPropertiesForObject(KWObject objectToGetProperties)
        {
            return cachedPropertiesMetadataIdentifiedByID.Values.Select(metadata => metadata.CachedProperty)
                .Where(cachedProperty => cachedProperty.Owner.Equals(objectToGetProperties) && IsUnpublishedProperty(cachedProperty));
        }

        public async static Task<List<KWProperty>> GetPropertyListByIdAsync(IEnumerable<long> propertyIdsToRetrive)
        {
            if (propertyIdsToRetrive == null)
                throw new ArgumentNullException("propertyIdsToRetrive");

            int countOfPropertyIdsToRetrive = propertyIdsToRetrive.Count();
            List<KWProperty> result = new List<KWProperty>(countOfPropertyIdsToRetrive);

            List<long> unpublishedPropertyIds = new List<long>(countOfPropertyIdsToRetrive);
            List<long> publishedPropertyIds = new List<long>(countOfPropertyIdsToRetrive);
            List<long> publishedCachedPropertyIds = new List<long>(countOfPropertyIdsToRetrive);


            foreach (long id in propertyIdsToRetrive)
            {
                if (IsUnpublishedProperty(id))
                {
                    unpublishedPropertyIds.Add(id);
                }
                else
                {
                    if (IsPropertyCached(id))
                    {
                        publishedCachedPropertyIds.Add(id);
                    }
                    else
                    {
                        publishedPropertyIds.Add(id);
                    }
                }
            }

            if (unpublishedPropertyIds.Count > 0)
                result.AddRange(GetUnpublishedPropertiesById(unpublishedPropertyIds));

            if (publishedPropertyIds.Count > 0)
                result.AddRange(await RetrivePropertiesByIdAsync(publishedPropertyIds));
            if (publishedCachedPropertyIds.Count > 0)
                result.AddRange(publishedCachedPropertyIds.Select(o => GetCachedPropertyById(o)));

            // TODO: آینده - در اینجا می‌توان ویژگی‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد

            // TODO: آینده - امکان بهبود کارایی این تابع مانند تابع معادل برای «شئ» وجود دارد

            return result;
        }
        public async static Task<List<KWProperty>> GetPropertiesForObjectAsync(KWObject objectToGetProperties)
        {
            if (objectToGetProperties == null)
                throw new ArgumentNullException("objectToGetProperties");

            List<KWProperty> result = new List<KWProperty>(GetUnpublishedPropertiesForObject(objectToGetProperties));
            // از آنجایی که برای اشیا منتشر نشده، هیچ ویژگی در مخزن داده‌ها نیست، درخواست
            // بازیابی ویژگی‌ها فقط در صورتی که شئ منتشر شده باشد صادر می‌شود
            if (!ObjectManager.IsUnpublishedObject(objectToGetProperties))
                result.AddRange
                    (await RetrivePropertiesForObjectAsync(objectToGetProperties));

            foreach (KWObject locallyResolvedObj in ObjectManager.GetObjectWhereLocallyResolvedToObject(objectToGetProperties.ID))
            {
                result.AddRange
                    (await GetPropertiesForObjectAsync(locallyResolvedObj));
            }

            // TODO: آینده - در اینجا می‌توان ویژگی‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد

            return result;
        }
        public async static Task<IEnumerable<KWProperty>> GetSpecifiedPropertiesOfObjectAsync(IEnumerable<KWObject> objectsToGetTheirProperties, IEnumerable<string> propertyTypes)
        {
            if (objectsToGetTheirProperties == null)
                throw new ArgumentNullException("objectsToGetTheirProperties");
            if (propertyTypes == null)
                throw new ArgumentNullException("propertyTypes");

            var result = GetUnpublishedSpecifiedPropertiesForObjects(propertyTypes, objectsToGetTheirProperties);

            IEnumerable<KWObject> objectsWhereResolvedToSpecifiedObjects
                = objectsToGetTheirProperties.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            // از آنجایی که برای اشیا منتشر نشده، هیچ ویژگی در مخزن داده‌ها نیست، درخواست
            // بازیابی ویژگی‌ها فقط برای اشیا منتشر شده صادر می‌شود
            var publishedObjects = objectsToGetTheirProperties
                .Concat(objectsWhereResolvedToSpecifiedObjects)
                .Where(o => !ObjectManager.IsUnpublishedObject(o));
            if (publishedObjects.Any())
                result = result.Concat
                    (await RetriveSpecifiedPropertiesForObjectsAsync(propertyTypes, publishedObjects));

            // TODO: آینده - در اینجا می‌توان ویژگی‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد

            return result;
        }

        internal async static Task<KProperty[]> RetrievedSpecifiedPropertiesOfObjectAsync(KWObject[] objectsToGetTheirProperties, string propertyTypeUri, string propertyValue)
        {
            if (objectsToGetTheirProperties == null)
                throw new ArgumentNullException(nameof(objectsToGetTheirProperties));
            if (propertyTypeUri == null)
                throw new ArgumentNullException(nameof(propertyTypeUri));
            if (propertyValue == null)
                throw new ArgumentNullException(nameof(propertyValue));

            IEnumerable<KWObject> objectsWhereResolvedToSpecifiedObjects
                = objectsToGetTheirProperties.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            // از آنجایی که برای اشیا منتشر نشده، هیچ ویژگی در مخزن داده‌ها نیست، درخواست
            // بازیابی ویژگی‌ها فقط برای اشیا منتشر شده صادر می‌شود
            var publishedObjects = objectsToGetTheirProperties
                .Concat(objectsWhereResolvedToSpecifiedObjects)
                .Where(o => !ObjectManager.IsUnpublishedObject(o));
            if (publishedObjects.Any())
            {
                WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
                KProperty[] retrivedKProperties = null;
                try
                {
                    retrivedKProperties = await sc.GetSpecifiedPropertiesOfObjectsByTypeAndValueAsync(publishedObjects.Select(o => o.ID).ToArray(), propertyTypeUri, propertyValue);
                }
                finally
                {
                    sc.Close();
                }
                if (retrivedKProperties == null)
                    throw new InvalidOperationException("Invalid server respone");
                else
                    return retrivedKProperties;
            }
            else
            {
                return new KProperty[] { };
            }
        }
        public static IEnumerable<KWProperty> GetCachedPropertiesWithSpecifiedTypeAndValue(string propertyTypeURI, string propertyValue)
        {
            if (propertyTypeURI == null)
                throw new ArgumentNullException("propertyTypeURI");
            if (propertyValue == null)
                throw new ArgumentNullException("propertyValue");
            return cachedPropertiesMetadataIdentifiedByID
                .Where(propertyMetadata => ((propertyMetadata.Value.CachedProperty.TypeURI == propertyTypeURI) && (propertyMetadata.Value.CachedProperty.Value == propertyValue)))
                .Select(propertyMetadata => propertyMetadata.Value.CachedProperty);
        }

        #endregion

        #region توابع مدیریت ادغام اشیاء
        internal static void ChangeOwnerOfCachedPropertiesForObject(KWObject objectToResolve, KWObject resolveMaster)
        {
            foreach (var relatedProperty in GetCachedPropertiesForObject(objectToResolve))
            {
                ChangeOwnerOfProperty(relatedProperty.ID, resolveMaster);
            }
        }
        private static void ChangeOwnerOfProperty(long propertyID, KWObject newOwner)
        {
            var propertyMetadata = GetCachedMetadataById(propertyID);
            propertyMetadata.CachedProperty.Owner = newOwner;
        }
        #endregion

        public async static Task<List<KProperty>> GetKPropertiesFromKWProperties(IEnumerable<KWProperty> kwPropertyList)
        {
            List<KProperty> KPropertyList = new List<KProperty>();
            foreach (var item in kwPropertyList)
            {
                KProperty kproperty = new KProperty();
                kproperty = await GetKPropertyFromKWProperty(item);
                KPropertyList.Add(kproperty);
            }
            return KPropertyList;
        }
        public static async Task<KProperty> GetKPropertyFromKWProperty(KWProperty objectToConvert)
        {
            if (objectToConvert == null)
                throw new ArgumentNullException("objectToConvert");

            KProperty result = new KProperty();
            result.Id = objectToConvert.ID;
            result.Value = objectToConvert.Value;
            result.TypeUri = objectToConvert.TypeURI;

            if (ObjectManager.IsUnpublishedObject(objectToConvert.Owner))
            {
                result.Owner = ObjectManager.GetKObjectFromKWObject(objectToConvert.Owner);
            }
            else
            {
                KObject propertyRemoteOwnerResolutionMaster
                    = await ObjectManager.GetResolutionMasterForPublishedObjectIfRemotelyResolved(objectToConvert.Owner.ID);
                if (propertyRemoteOwnerResolutionMaster != null)
                {
                    result.Owner = propertyRemoteOwnerResolutionMaster;
                }
                else
                {
                    result.Owner = ObjectManager.GetKObjectFromKWObject(objectToConvert.Owner);
                }
            }

            // بازگرداندن شی معادل سمت سرور
            return result;
        }
        public async static Task<List<ModifiedProperty>> GetModifiedPropertiesFromKWProperties(IEnumerable<KWProperty> modifiedProperties, bool applyRemoteResolutionForPropertyOwner = true)
        {
            List<ModifiedProperty> KModifiedPropertyList = new List<ModifiedProperty>();
            foreach (var property in modifiedProperties)
            {
                ModifiedProperty ModifiedProperty = new ModifiedProperty();
                ModifiedProperty = await GetModifiedPropertyFromKWProperty(property, applyRemoteResolutionForPropertyOwner);
                KModifiedPropertyList.Add(ModifiedProperty);
            }
            return KModifiedPropertyList;
        }
        private async static Task<ModifiedProperty> GetModifiedPropertyFromKWProperty(KWProperty objectToConvert, bool applyRemoteResolutionForPropertyOwner = true)
        {
            if (objectToConvert == null)
                throw new ArgumentNullException("objectToConvert");

            long ownerID;

            if (applyRemoteResolutionForPropertyOwner
                // مالک ویژگی تغییریافته حتما منتشر شده است
                /*&& !ObjectManager.IsUnpublishedObject(objectToConvert.Owner)*/
                )
            {
                KObject propertyRemoteOwnerResolutionMaster
                        = await ObjectManager.GetResolutionMasterForPublishedObjectIfRemotelyResolved(objectToConvert.Owner.ID);
                if (propertyRemoteOwnerResolutionMaster != null)
                {
                    ownerID = propertyRemoteOwnerResolutionMaster.Id;
                }
                else
                {
                    ownerID = objectToConvert.Owner.ID;
                }
            }
            else
            {
                ownerID = objectToConvert.Owner.ID;
            }

            ModifiedProperty result = new ModifiedProperty()
            {
                Id = objectToConvert.ID,
                NewValue = objectToConvert.Value,
                OwnerObjectID = ownerID,
                TypeUri = objectToConvert.TypeURI
            };

            // بازگرداندن شی معادل سمت سرور
            return result;
        }

        private void ConvertValidationToBoolean()
        {
        }
        public static bool IsPropertyValid(BaseDataTypes propertyBaseType, string propertyValue)
        {
            if (propertyValue == null)
                throw new ArgumentNullException("propertyValue");

            if (ValueBaseValidation.IsValidPropertyValue(propertyBaseType, propertyValue).Status == ValidationStatus.Valid ||
                ValueBaseValidation.IsValidPropertyValue(propertyBaseType, propertyValue).Status == ValidationStatus.Warning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsPropertyValid(KWProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            BaseDataTypes baseDataTypeOfProperty = System.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI);
            return IsPropertyValid(baseDataTypeOfProperty, property.Value);
        }
        public static object ParsePropertyValue(BaseDataTypes propertyBaseType, string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            return ValueBaseValidation.ParsePropertyValue(propertyBaseType, value);
        }

        public static bool TryParsePropertyValue(BaseDataTypes propertyBaseType, string value, out object parsedValue)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (ValueBaseValidation.TryParsePropertyValue(propertyBaseType, value, out parsedValue).Status == ValidationStatus.Valid ||
                ValueBaseValidation.TryParsePropertyValue(propertyBaseType, value, out parsedValue).Status == ValidationStatus.Warning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}