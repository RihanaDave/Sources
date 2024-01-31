using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkDirection = GPAS.Workspace.Entities.KWLinks.LinkDirection;

namespace GPAS.Workspace.DataAccessManager
{
    /// <summary>
    /// مدیریت داده‌ای لینک‌ها سمت محیط کاربری
    /// </summary>
    public class LinkManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private LinkManager()
        { }

        #region  مدیریت میانگیری و رفع تصادم‌ها | Caching & Conflicts resolution
        public class CachedRelationshipMetadata
        {
            public KWRelationship CachedRelationship;
            public long RelationshipSourceId;
            public long RelationshipTargetId;
            public bool IsPublished;
        }
        /// <summary>
        /// لیست مرتب شده رابطه های ایجاد شده در سمت محیط کاربری که براساس شناسه شان مرتب شده اند؛
        /// این لیست تضمین می کند که به ازای هر رابطه (و نه لینک) یکتا در سمت محیط کاربری، بیش از یک نمونه ساخته نشود.
        /// این لیست منحصرا توسط تابع تبدیل نمونه شی سمت سرویس دهنده راه دور به نمونه شی سمت محیط کاربری «تغییر» می کند.
        /// </summary>
        /// <remarks>
        ///  لیست میانگیری شده برای رابطه ها استفاده می شود که اشیا زیرساختی برای انواع لینک ها می باشند
        ///  و نه برای هر یک از لینک ها، چرا که لینک ها احتمال تغییر در سمت محیط کاربری دارند ولی تغییرات
        ///  آن ها به خاطر نقش عنصری رابطه ها، روی آن ها تاثیر ندارد
        /// </remarks>
        public static Dictionary<long, CachedRelationshipMetadata> cachedRelationshipsMetadataIdentifiedByID
            = new Dictionary<long, CachedRelationshipMetadata>();

        private static void AddRelationshpToCache(KWRelationship relationship, long sourceObjectId, long targetObjectId, bool isPublished)
        {
            var newRelationshipMetadata = new CachedRelationshipMetadata()
            {
                CachedRelationship = relationship,
                RelationshipSourceId = sourceObjectId,
                RelationshipTargetId = targetObjectId,
                IsPublished = isPublished
            };
            cachedRelationshipsMetadataIdentifiedByID.Add(relationship.ID, newRelationshipMetadata);
        }

        private static List<KWRelationship> GetUnpublishedRelationshipsById(List<long> unpublishedRelationshipIds)
        {
            var result = new List<KWRelationship>(unpublishedRelationshipIds.Count);
            foreach (var id in unpublishedRelationshipIds)
            {
                var relationship = cachedRelationshipsMetadataIdentifiedByID[id].CachedRelationship;
                result.Add(relationship);
            }
            return result;
        }

        internal static IEnumerable<CachedRelationshipMetadata> GetUnpublishedRelationships()
        {
            IEnumerable<CachedRelationshipMetadata> result = cachedRelationshipsMetadataIdentifiedByID
                .Values
                .Where(relation => IsUnpublishedRelationship(relation.CachedRelationship));
            return result;
        }

        internal static IEnumerable<CachedRelationshipMetadata> GetCachedRelationships()
        {
            IEnumerable<CachedRelationshipMetadata> result = cachedRelationshipsMetadataIdentifiedByID
                .Values;
            return result;
        }

        private static async Task<List<RelationshipBasedKWLink>> GetUnpublishedRelationshipBasedLinksByIdAsync(List<long> unpublishedRelationshipIds)
        {
            var result = new List<RelationshipBasedKWLink>(unpublishedRelationshipIds.Count);
            foreach (var id in unpublishedRelationshipIds)
            {
                CachedRelationshipMetadata relationshipMetadata = cachedRelationshipsMetadataIdentifiedByID[id];
                RelationshipBasedKWLink link = await GenerateRelationshipBasedLinkAsync(relationshipMetadata);
                result.Add(link);
            }
            return result;
        }
        private static async Task<List<RelationshipBasedKWLink>> GetUnpublishedRelationshipBasedLinksBySourceObjectAsync(KWObject objectToGetRelationships, string relationshipsTypeUri)
        {
            var result = new List<RelationshipBasedKWLink>();
            foreach (var cachedMetadata in cachedRelationshipsMetadataIdentifiedByID.Values)
            {
                if (IsUnpublishedRelationship(cachedMetadata.CachedRelationship)
                    && cachedMetadata.RelationshipSourceId.Equals(objectToGetRelationships.ID)
                    && cachedMetadata.CachedRelationship.TypeURI.Equals(relationshipsTypeUri))
                {
                    var link = await GenerateRelationshipBasedLinkAsync(cachedMetadata);
                    result.Add(link);
                }
            }
            return result;
        }
        private static async Task<Dictionary<KWRelationship, KWObject>> GetUnpublishedRelationshipsPerSourceByTargetObjectIdAsync(long objectIdToGetRelationships, string relationshipsTypeUri)
        {
            var result = new Dictionary<KWRelationship, KWObject>();
            foreach (var cachedMetadata in cachedRelationshipsMetadataIdentifiedByID.Values)
            {
                if (IsUnpublishedRelationship(cachedMetadata.CachedRelationship)
                    && cachedMetadata.RelationshipTargetId.Equals(objectIdToGetRelationships)
                    && cachedMetadata.CachedRelationship.TypeURI.Equals(relationshipsTypeUri))
                {
                    var sourceObject = await ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipSourceId);
                    result.Add(cachedMetadata.CachedRelationship, sourceObject);
                }
            }
            return result;
        }

        private static KWRelationship GetCachedRelationshipById(long relationshipId)
        {
            return GetCachedMetadataById(relationshipId).CachedRelationship;
        }
        public static CachedRelationshipMetadata GetCachedMetadataById(long unpublishedId)
        {
            return cachedRelationshipsMetadataIdentifiedByID[unpublishedId];
        }
        private static bool IsRetrievedRelationshipCachedBefore(KRelationship relationship)
        {
            return cachedRelationshipsMetadataIdentifiedByID.ContainsKey(relationship.Id);
        }

        private static async Task<List<UnpublishedRelationshipChanges.RelationshipEntity>> GetUnpublishedRelationshipsAsync()
        {
            // TODO: Clean!
            var r = cachedRelationshipsMetadataIdentifiedByID
                .Values
                .Where(relationshipMetadata => IsUnpublishedRelationship(relationshipMetadata.CachedRelationship));
            var result = new List<UnpublishedRelationshipChanges.RelationshipEntity>();
            foreach (var item in r)
            {
                var m = new UnpublishedRelationshipChanges.RelationshipEntity()
                {
                    Relationship = item.CachedRelationship,
                    Source = await ObjectManager.GetObjectByIdAsync(item.RelationshipSourceId),
                    Target = await ObjectManager.GetObjectByIdAsync(item.RelationshipTargetId)
                };
                result.Add(m);
            }
            return result;
        }

        //private static async Task<List<UnpublishedRelationshipChanges.RelationshipEntity>> GetUnpublishedRelationshipsAsync()
        //{
        //    // TODO: Clean!
        //    var r = cachedRelationshipsMetadataIdentifiedByID
        //        .Values
        //        .Where(relationshipMetadata => IsUnpublishedRelationship(relationshipMetadata.CachedRelationship));
        //    var result = new List<UnpublishedRelationshipChanges.RelationshipEntity>();
        //    foreach (var item in r)
        //    {
        //        var m = new UnpublishedRelationshipChanges.RelationshipEntity()
        //        {
        //            Relationship = item.CachedRelationship,
        //            Source = await ObjectManager.GetObjectByIdAsync(item.RelationshipSourceId),
        //            Target = await ObjectManager.GetObjectByIdAsync(item.RelationshipTargetId)
        //        };
        //        result.Add(m);
        //    }
        //    return result;
        //}

        private static IEnumerable<CachedRelationshipMetadata> GetCachedRelationshipsRelatedToObject(KWObject obj)
        {
            return cachedRelationshipsMetadataIdentifiedByID
                .Values
                .Where(relatinshipMetadata
                    => relatinshipMetadata.RelationshipSourceId.Equals(obj.ID)
                    || relatinshipMetadata.RelationshipTargetId.Equals(obj.ID));
        }

        internal static void ClearCache()
        {
            cachedRelationshipsMetadataIdentifiedByID.Clear();
        }

        public static bool IsUnpublishedRelationship(KWRelationship relationshipToCheck)
        {
            if (relationshipToCheck == null)
                throw new ArgumentNullException(nameof(relationshipToCheck));

            return IsUnpublishedRelationship(relationshipToCheck.ID);
        }

        internal static void AddSavedMetadataToCache(List<CachedRelationshipMetadata> relationsMetadata)
        {
            foreach (var currentMetadata in relationsMetadata)
            {
                if (!cachedRelationshipsMetadataIdentifiedByID.ContainsKey(currentMetadata.CachedRelationship.ID))
                {
                    cachedRelationshipsMetadataIdentifiedByID.Add(currentMetadata.CachedRelationship.ID, currentMetadata);
                }
                else
                {
                    UpdateCacheBySavedMetadata(currentMetadata);
                }
            }
        }

        private static void UpdateCacheBySavedMetadata(CachedRelationshipMetadata relationshipMetadata)
        {
            if (cachedRelationshipsMetadataIdentifiedByID.ContainsKey(relationshipMetadata.CachedRelationship.ID))
            {
                CachedRelationshipMetadata existanceMetadataInCache = cachedRelationshipsMetadataIdentifiedByID[relationshipMetadata.CachedRelationship.ID];
                existanceMetadataInCache.CachedRelationship = relationshipMetadata.CachedRelationship;
                existanceMetadataInCache.IsPublished = relationshipMetadata.IsPublished;
                existanceMetadataInCache.RelationshipSourceId = relationshipMetadata.RelationshipSourceId;
                existanceMetadataInCache.RelationshipTargetId = relationshipMetadata.RelationshipTargetId;
            }
        }

        public static bool IsUnpublishedRelationship(long relationshipId)
        {
            if (IsRelationshipCached(relationshipId))
            {
                var metadata = GetCachedMetadataById(relationshipId);
                return !metadata.IsPublished;
            }
            else
            {
                return false;
            }
        }

        private static bool IsRelationshipCached(long relationshipId)
        {
            return cachedRelationshipsMetadataIdentifiedByID.ContainsKey(relationshipId);
        }
        #endregion

        #region خدمات انتشار رابطه ها | Publish services
        public static async Task<UnpublishedRelationshipChanges> GetUnpublishedChangesAsync()
        {
            var changes = new UnpublishedRelationshipChanges();
            changes.AddedRelationships = await GetUnpublishedRelationshipsAsync();
            //changes.ModifiedRelationships = GetCachedPublishedRelationshipsModifiedLocally();
            return changes;
        }

        internal static List<CachedRelationshipMetadata> GetCachedRelationshipMetadatas()
        {
            List<CachedRelationshipMetadata> changes = new List<CachedRelationshipMetadata>();
            changes = cachedRelationshipsMetadataIdentifiedByID
                .Values
                .Where(relationshipMetadata => IsUnpublishedRelationship(relationshipMetadata.CachedRelationship)).ToList();
            return changes;
        }
        public static void CommitUnpublishedChanges(IEnumerable<long> addedRelationshipIDs, long dataSourceID)
        {
            if (addedRelationshipIDs == null)
                throw new ArgumentNullException(nameof(addedRelationshipIDs));

            ApplyRelationshipsAdditionToCache(addedRelationshipIDs);
            // No modification Tag declared to be applied here
            SetDataSourceIdForAddedRelationships(addedRelationshipIDs, dataSourceID);
        }

        private static void SetDataSourceIdForAddedRelationships(IEnumerable<long> addedRelationshipIDs, long dataSourceID)
        {
            foreach (long relID in addedRelationshipIDs)
            {
                KWRelationship relationship = GetCachedRelationshipById(relID);
                relationship.DataSourceId = dataSourceID;
            }
        }

        private static void ApplyRelationshipsAdditionToCache(IEnumerable<long> addedRelationshipIDs)
        {
            var IdList = addedRelationshipIDs.ToList();
            while (IdList.Count > 0)
            {
                var metadata = cachedRelationshipsMetadataIdentifiedByID[IdList[0]];
                metadata.IsPublished = true;
                // از آنجایی که دیکشنری میانگیری، حاوی آخرین تغییرات محلی برای
                // اشیاء می‌باشد، بجز برداشتن تگ، نیاز به کار دیگری نیست
                IdList.RemoveAt(0);
            }
        }

        /// <summary>
        /// تغییرات محلی و همچنین میانگیری‌های قبلی را از بین می‌برد
        /// </summary>
        public static void DiscardChanges()
        {
            cachedRelationshipsMetadataIdentifiedByID.Clear();
        }

        private static bool IsAnyUnpublishedChangeRemains()
        {
            bool result = false;
            foreach (var metadata in cachedRelationshipsMetadataIdentifiedByID.Values)
                if (IsUnpublishedRelationship(metadata.CachedRelationship)
                    /*|| metadata.IsModified*/)
                {
                    result = true;
                    break;
                }
            return result;
        }
        #endregion

        #region تعامل با سرویس‌دهنده راه‌دور
        private async static Task<List<RelationshipBasedKWLink>> RetriveRelationshipBasedLinksByIdAsync(long[] publishedRelationshipIds)
        {
            if (publishedRelationshipIds == null)
                throw new ArgumentNullException(nameof(publishedRelationshipIds));

            List<RelationshipBaseKlink> retrievedRelationshipBaseKLinks = null;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                retrievedRelationshipBaseKLinks = (await sc.GetRelationshipListByIdAsync(publishedRelationshipIds)).ToList();
            }
            finally
            {
                sc?.Close();
            }
            if (retrievedRelationshipBaseKLinks == null)
                throw new NullReferenceException("Invalid server response");
            // تبدیل به انواع شناخته شده برای محیط کاربری
            List<RelationshipBasedKWLink> retrievedRelationships = new List<RelationshipBasedKWLink>();
            foreach (RelationshipBaseKlink item in retrievedRelationshipBaseKLinks)
            {
                RelationshipBasedKWLink itemLink = await GetRelationshipBaseLinkFromRetrievedDataAsync(item);
                retrievedRelationships.Add(itemLink);
            }
            return retrievedRelationships;
        }
        private async static Task<List<RelationshipBasedKWLink>> RetriveRelationshipBasedLinksBySourceObjectAsync(KWObject objectToGetRelationships, string relationshipsTypeUri)
        {
            List<RelationshipBaseKlink> retrievedRelationshipBasedLinks;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                retrievedRelationshipBasedLinks = (await sc.GetRelationshipsBySourceObjectAsync(objectToGetRelationships.ID, relationshipsTypeUri)).ToList();
            }
            finally
            {
                sc?.Close();
            }
            if (retrievedRelationshipBasedLinks == null)
                throw new NullReferenceException("Invalid server response");
            // تبدیل به انواع شناخته شده برای محیط کاربری
            List<RelationshipBasedKWLink> retrievedRelationships = new List<RelationshipBasedKWLink>();
            foreach (var item in retrievedRelationshipBasedLinks)
            {
                RelationshipBasedKWLink itemLink = await GetRelationshipBaseLinkFromRetrievedDataAsync(item);
                retrievedRelationships.Add(itemLink);
            }
            return retrievedRelationships;
        }
        private async static Task<Dictionary<KWRelationship, KWObject>> RetriveRelationshipsPerSourceObjectByRelationshipTargetAsync(long targetObjectId, string relationshipsTypeUri)
        {
            List<RelationshipBaseKlink> retrievedRelationshipBasedLinks;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                retrievedRelationshipBasedLinks = (await sc.GetLinksSourcedByObjectAsync(ObjectManager.GenerateEmptyKObjectByID(targetObjectId), relationshipsTypeUri)).ToList();
            }
            finally
            {
                sc?.Close();
            }
            if (retrievedRelationshipBasedLinks == null)
                throw new NullReferenceException("Invalid server response");
            // تبدیل به انواع شناخته شده برای محیط کاربری
            var retrievedRelationshipsPreSourceObjects = new Dictionary<KWRelationship, KWObject>();
            foreach (var item in retrievedRelationshipBasedLinks)
            {
                //var itemTarget = await ObjectManager.GetObjectFromRetrievedDataAsync(item.Target);
                var itemRelationship = GetRelationshipFromReterievedData(item.Relationship, item.TypeURI, item.Source.Id, item.Target.Id);
                //var itemLink = GenerateRelationshipBasedLink(itemRelationship);

                var itemSource = await ObjectManager.GetObjectFromRetrievedDataAsync(item.Source);
                retrievedRelationshipsPreSourceObjects.Add(itemRelationship, itemSource);
            }
            return retrievedRelationshipsPreSourceObjects;
        }

        public static KWRelationship GetRelationshipFromReterievedData(KRelationship relationship, string relationshipTypeURI, long relationshipSourceObjectID, long relationshipTargetObjectID)
        {
            if (relationship == null)
                throw new ArgumentNullException(nameof(relationship));
            if (relationshipTypeURI == null)
                throw new ArgumentNullException(nameof(relationshipTypeURI));

            KWRelationship result = null;
            if (IsRetrievedRelationshipCachedBefore(relationship))
            {
                // TODO: آینده - در اینجا می‌توان داده‌های بازیابی شده را با داده‌های میانگیری شده تطابق داد؛
                // مثلا مبدا و مقصد ورودی این تابع می‌بایست با آنچه در میانگیری ذخیره شده یکسان باشند.

                // درحال حاضر رابطه‌ها چیزی برای به‌روز رسانی ندارند
                //UpdateCacheByRetrievedRelationship(retrievedLink.Relationship);
                result = GetCachedRelationshipById(relationship.Id);
            }
            else
            {
                result = GenerateReterievedRelationship(relationship, relationshipTypeURI, relationshipSourceObjectID, relationshipTargetObjectID);
            }
            return result;
        }

        public async static Task<List<RelationshipBasedKWLink>> GetRelationshipBasedKWLinkRelatedToObject(KWObject kwObject)
        {
            if (kwObject == null)
                throw new ArgumentNullException(nameof(kwObject));

            return await GetUnpublishRelationshipForObject(kwObject);
        }
        private async static Task<List<RelationshipBasedKWLink>> GetUnpublishRelationshipForObject(KWObject kwObject)
        {
            List<RelationshipBasedKWLink> relatedRelationships = new List<RelationshipBasedKWLink>();
            foreach (CachedRelationshipMetadata item in GetUnpublishedRelationships())
            {
                if ((kwObject.ID == item.RelationshipSourceId) || (kwObject.ID == item.RelationshipTargetId))
                {
                    relatedRelationships.Add(await GenerateRelationshipBasedLinkAsync(item));
                }
            }
            return relatedRelationships;
        }

        public async static Task<RelationshipBasedKWLink> GetRelationshipBaseLinkFromRetrievedDataAsync(RelationshipBaseKlink retrievedLink)
        {
            if (retrievedLink == null)
                throw new ArgumentNullException(nameof(retrievedLink));
            KWObject linkSource = await ObjectManager.GetObjectFromRetrievedDataAsync(retrievedLink.Source);
            KWObject linkTarget = await ObjectManager.GetObjectFromRetrievedDataAsync(retrievedLink.Target);
            KWRelationship linkRelationship = GetRelationshipFromReterievedData
                (retrievedLink.Relationship, retrievedLink.TypeURI, retrievedLink.Source.Id, retrievedLink.Target.Id);

            return GenerateRelationshipBasedLink(linkRelationship, linkSource, linkTarget);
        }

        public async static Task<List<RelationshipBasedKWLink>> GetRelationshipBaseLinksFromRetrievedDataAsync(IEnumerable<RelationshipBaseKlink> retrievedLinks)
        {
            if (retrievedLinks == null)
                throw new ArgumentNullException(nameof(retrievedLinks));

            List<RelationshipBasedKWLink> result = new List<RelationshipBasedKWLink>();
            List<KObject> kObjects = new List<KObject>();

            foreach (var currentLink in retrievedLinks)
            {
                kObjects.Add(currentLink.Source);
                kObjects.Add(currentLink.Target);
            }

            List<KWObject> kwObjects = await ObjectManager.GetObjectsFromRetrievedDataAsync(kObjects.ToArray());

            Dictionary<long, KWObject> innerObjects = new Dictionary<long, KWObject>();
            foreach (KWObject kwObj in kwObjects)
            {
                if (!innerObjects.ContainsKey(kwObj.ID))
                {
                    innerObjects.Add(kwObj.ID, kwObj);
                }
            }

            foreach (var currentLink in retrievedLinks)
            {
                KWRelationship linkRelationship = GetRelationshipFromReterievedData
                (currentLink.Relationship, currentLink.TypeURI, currentLink.Source.Id, currentLink.Target.Id);
                result.Add(GenerateRelationshipBasedLink(linkRelationship, innerObjects));
            }

            return result;
        }

        public async static Task<EventBasedKWLink> ConvertEventBaseKlinkToEventBasedKWLink(EventBaseKlink linkToExchange)
        {
            if (linkToExchange == null)
                throw new ArgumentNullException(nameof(linkToExchange));

            KObject[] innerKObjects = new KObject[]
            {
                linkToExchange.SourceRelationship.Source,
                linkToExchange.SourceRelationship.Target,
                linkToExchange.TargetRelationship.Source,
                linkToExchange.TargetRelationship.Target
            };

            Dictionary<long, KWObject> innerObjects = new Dictionary<long, KWObject>(4);
            foreach (KObject kObj in innerKObjects)
            {
                if (!innerObjects.ContainsKey(kObj.Id))
                {
                    KWObject obj = await ObjectManager.GetObjectFromRetrievedDataAsync(kObj);
                    innerObjects.Add(kObj.Id, obj);
                }
            }

            KWRelationship firstRelationship = GetRelationshipFromReterievedData
                    (linkToExchange.SourceRelationship.Relationship, linkToExchange.SourceRelationship.TypeURI
                    , linkToExchange.SourceRelationship.Source.Id, linkToExchange.SourceRelationship.Target.Id);
            KWRelationship secondRelationship = GetRelationshipFromReterievedData
                    (linkToExchange.TargetRelationship.Relationship, linkToExchange.TargetRelationship.TypeURI
                    , linkToExchange.TargetRelationship.Source.Id, linkToExchange.TargetRelationship.Target.Id);

            return GenerateEventBasedLinkByInnerRelationshipsAndObjects(firstRelationship, secondRelationship, innerObjects);
        }

        public async static Task<List<EventBasedKWLink>> ConvertEventBaseKlinksToEventBasedKWLinks(List<EventBaseKlink> linksToExchange)
        {

            List<EventBasedKWLink> result = new List<EventBasedKWLink>();
            if (linksToExchange == null)
                throw new ArgumentNullException(nameof(linksToExchange));

            Dictionary<long, KObject> idToKObjectMapping = new Dictionary<long, KObject>();
            List<KObject> innerKObjects = new List<KObject>();
            foreach (var currentEventBaseKlink in linksToExchange)
            {
                if (!idToKObjectMapping.ContainsKey(currentEventBaseKlink.SourceRelationship.Source.Id))
                {
                    idToKObjectMapping.Add(currentEventBaseKlink.SourceRelationship.Source.Id, currentEventBaseKlink.SourceRelationship.Source);
                }

                if (!idToKObjectMapping.ContainsKey(currentEventBaseKlink.SourceRelationship.Target.Id))
                {
                    idToKObjectMapping.Add(currentEventBaseKlink.SourceRelationship.Target.Id, currentEventBaseKlink.SourceRelationship.Target);
                }

                if (!idToKObjectMapping.ContainsKey(currentEventBaseKlink.TargetRelationship.Source.Id))
                {
                    idToKObjectMapping.Add(currentEventBaseKlink.TargetRelationship.Source.Id, currentEventBaseKlink.TargetRelationship.Source);
                }

                if (!idToKObjectMapping.ContainsKey(currentEventBaseKlink.TargetRelationship.Target.Id))
                {
                    idToKObjectMapping.Add(currentEventBaseKlink.TargetRelationship.Target.Id, currentEventBaseKlink.TargetRelationship.Target);
                }
            }
            innerKObjects = idToKObjectMapping.Values.ToList();
            List<KWObject> kwObjects = await ObjectManager.GetObjectsFromRetrievedDataAsync(innerKObjects.ToArray());
            Dictionary<long, KWObject> innerObjects = new Dictionary<long, KWObject>();
            foreach (KWObject kwObj in kwObjects)
            {
                if (!innerObjects.ContainsKey(kwObj.ID))
                {
                    innerObjects.Add(kwObj.ID, kwObj);
                }
            }

            foreach (var linkToExchange in linksToExchange)
            {
                KWRelationship firstRelationship = GetRelationshipFromReterievedData
                    (linkToExchange.SourceRelationship.Relationship, linkToExchange.SourceRelationship.TypeURI
                    , linkToExchange.SourceRelationship.Source.Id, linkToExchange.SourceRelationship.Target.Id);
                KWRelationship secondRelationship = GetRelationshipFromReterievedData
                        (linkToExchange.TargetRelationship.Relationship, linkToExchange.TargetRelationship.TypeURI
                        , linkToExchange.TargetRelationship.Source.Id, linkToExchange.TargetRelationship.Target.Id);
                result.Add(GenerateEventBasedLinkByInnerRelationshipsAndObjects(firstRelationship, secondRelationship, innerObjects));
            }

            return result;
        }

        /// <summary>
        /// یک جهت وابستگی (ریلیشنشیپ) از نوع راه دور (ارائه دهنده سرویس به محیط کاربری) را به جهت از نوع قابل استفاده سمت محیط کاربری تبدیل می کند
        /// </summary>
        private static LinkDirection GetLocalEntitiesDirectionFromRemoteServiceDirection(Dispatch.Entities.Concepts.LinkDirection directionToExchange)
        {
            switch (directionToExchange)
            {
                case Dispatch.Entities.Concepts.LinkDirection.SourceToTarget:
                    return LinkDirection.SourceToTarget;
                case Dispatch.Entities.Concepts.LinkDirection.TargetToSource:
                    return LinkDirection.TargetToSource;
                case Dispatch.Entities.Concepts.LinkDirection.Bidirectional:
                    return LinkDirection.Bidirectional;
                default:
                    return LinkDirection.Bidirectional;
            }
        }

        /// <summary>
        /// یک جهت وابستگی (ریلیشنشیپ) از نوع قابل استفاده سمت محیط کاربری را به جهت از نوع راه دور (ارائه دهنده سرویس به محیط کاربری) تبدیل می کند
        /// </summary>
        private static Dispatch.Entities.Concepts.LinkDirection GetRemoteServiceDirectionFromLocalEntitiesDirection(LinkDirection linkDirectionToExchange)
        {
            switch (linkDirectionToExchange)
            {
                case LinkDirection.SourceToTarget:
                    return Dispatch.Entities.Concepts.LinkDirection.SourceToTarget;
                case LinkDirection.TargetToSource:
                    return Dispatch.Entities.Concepts.LinkDirection.TargetToSource;
                case LinkDirection.Bidirectional:
                    return Dispatch.Entities.Concepts.LinkDirection.Bidirectional;
                default:
                    return Dispatch.Entities.Concepts.LinkDirection.Bidirectional;
            }
        }

        private static long GetNewRelationshipID()
        {
            long relationshipId = -1;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                relationshipId = sc.GetNewRelationId();
            }
            finally
            {
                sc?.Close();
            }
            if (relationshipId == -1)
                throw new NullReferenceException("Invalid server response");

            return relationshipId;
        }
        #endregion

        #region سازنده‌های رابطه‌ها | Singleton Constructions - CRITICAL SECTION
        private static object generateNewUnpublishedRelationshipLockObject = new object();

        private static KWRelationship GenerateNewUnpublishedRelationship
            (KWObject source, KWObject target, string typeUri, LinkDirection linkDirection, DateTime? timeBegin, DateTime? timeEnd, string description)
        {
            KWRelationship newRelationship;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک رابطه، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی رابطه، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateNewUnpublishedRelationshipLockObject)
            {
                long newID = GetNewRelationshipID();
                newRelationship = new KWRelationship()
                {
                    ID = newID,
                    TypeURI = typeUri,
                    LinkDirection = linkDirection,
                    TimeBegin = timeBegin,
                    TimeEnd = timeEnd,
                    Description = description,
                    DataSourceId = null
                };
                AddRelationshpToCache(newRelationship, source.ID, target.ID, false);
            }
            return newRelationship;
        }

        private static object generateReterievedRelationshipLockObject = new object();

        private static KWRelationship GenerateReterievedRelationship(KRelationship retrievedRelationship, string relationshipTypeUri, long sourceObjectId, long targetObjectId)
        {
            KWRelationship newRelationship;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک رابطه، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی رابطه، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateReterievedRelationshipLockObject)
            {
                if (IsRelationshipCached(retrievedRelationship.Id))
                    newRelationship = GetCachedRelationshipById(retrievedRelationship.Id);
                else
                {
                    newRelationship = new KWRelationship()
                    {
                        ID = retrievedRelationship.Id,
                        TypeURI = relationshipTypeUri,
                        LinkDirection = GetLocalEntitiesDirectionFromRemoteServiceDirection(retrievedRelationship.Direction),
                        TimeBegin = retrievedRelationship.TimeBegin,
                        TimeEnd = retrievedRelationship.TimeEnd,
                        Description = retrievedRelationship.Description,
                        DataSourceId = retrievedRelationship.DataSourceID
                    };
                    AddRelationshpToCache(newRelationship, sourceObjectId, targetObjectId, true);
                }
            }
            return newRelationship;
        }
        #endregion

        #region سازنده‌های لینک‌ها | Based on Relationship Singleton Constructors (not Singleton)
        internal static async Task<RelationshipBasedKWLink> GenerateRelationshipBasedLinkAsync(CachedRelationshipMetadata metadata)
        {
            // اشیاء از قبل میانگیری شده‌اند و این بازیابی‌ها منجر به فراخوانی‌ سرویس راه‌دور نمی‌شوند
            KWObject source = await ObjectManager.GetObjectByIdAsync(metadata.RelationshipSourceId);
            KWObject target = await ObjectManager.GetObjectByIdAsync(metadata.RelationshipTargetId);
            return GenerateRelationshipBasedLink(metadata.CachedRelationship, source, target);
        }
        internal static RelationshipBasedKWLink GenerateRelationshipBasedLink(KWRelationship innerRelationship, KWObject source, KWObject target)
        {
            CachedRelationshipMetadata innerRelationshipCachedMetadata = GetCachedMetadataById(innerRelationship.ID);
            var newLink = new RelationshipBasedKWLink()
            {
                Source = source,
                Target = target,
                Relationship = innerRelationshipCachedMetadata.CachedRelationship
            };
            return newLink;
        }
        public static RelationshipBasedKWLink GetRelationshipBasedKWLink(KWRelationship baseRelationship)
        {
            return GetRelationshipBasedKWLink(baseRelationship.ID);
        }
        public static RelationshipBasedKWLink GetRelationshipBasedKWLink(long cachedRelationshipID)
        {
            CachedRelationshipMetadata innerRelationshipCachedMetadata = GetCachedMetadataById(cachedRelationshipID);
            var newLink = new RelationshipBasedKWLink()
            {
                Source = ObjectManager.GetCachedObjectById(innerRelationshipCachedMetadata.RelationshipSourceId),
                Target = ObjectManager.GetCachedObjectById(innerRelationshipCachedMetadata.RelationshipTargetId),
                Relationship = innerRelationshipCachedMetadata.CachedRelationship
            };
            return newLink;
        }
        internal static RelationshipBasedKWLink GenerateRelationshipBasedLink(KWRelationship innerRelationship, Dictionary<long, KWObject> innerObjects)
        {
            CachedRelationshipMetadata innerRelationshipCachedMetadata = GetCachedMetadataById(innerRelationship.ID);
            var newLink = new RelationshipBasedKWLink()
            {
                Source = innerObjects[innerRelationshipCachedMetadata.RelationshipSourceId],
                Target = innerObjects[innerRelationshipCachedMetadata.RelationshipTargetId],
                Relationship = innerRelationshipCachedMetadata.CachedRelationship
            };
            return newLink;
        }

        public static async Task<EventBasedKWLink> GenerateEventBasedLinkByInnerRelationshipsAsync(KWRelationship firstRelationship, KWRelationship secondRelationship)
        {
            CachedRelationshipMetadata firstRelationshipCachedMetadata = GetCachedMetadataById(firstRelationship.ID);
            CachedRelationshipMetadata secondRelationshipCachedMetadata = GetCachedMetadataById(secondRelationship.ID);

            long[] innerObjectIDs = new long[]
            {
                firstRelationshipCachedMetadata.RelationshipSourceId,
                firstRelationshipCachedMetadata.RelationshipTargetId,
                secondRelationshipCachedMetadata.RelationshipSourceId,
                secondRelationshipCachedMetadata.RelationshipTargetId
            };

            Dictionary<long, KWObject> innerObjects = new Dictionary<long, KWObject>(4);

            foreach (long objID in innerObjectIDs)
            {
                if (!innerObjects.ContainsKey(objID))
                {
                    var obj = await ObjectManager.GetObjectByIdAsync(objID);
                    innerObjects.Add(objID, obj);
                }
            }

            return GenerateEventBasedLinkByInnerRelationshipsAndObjects(firstRelationship, secondRelationship, innerObjects);
        }
        public static EventBasedKWLink GenerateEventBasedLinkByInnerRelationshipsAndObjects(KWRelationship firstRelationship, KWRelationship secondRelationship, Dictionary<long, KWObject> innerObjects)
        {
            CachedRelationshipMetadata firstRelMetadata = GetCachedMetadataById(firstRelationship.ID);
            CachedRelationshipMetadata secondRelMetadata = GetCachedMetadataById(secondRelationship.ID);
            EventBasedKWLink newLink = new EventBasedKWLink()
            {
                FirstRelationship = firstRelMetadata.CachedRelationship,
                SecondRelationship = secondRelMetadata.CachedRelationship
            };
            newLink.SetDirection(LinkDirection.Bidirectional);

            if (firstRelMetadata.RelationshipTargetId.Equals(secondRelMetadata.RelationshipSourceId))
            {
                newLink.Source = innerObjects[firstRelMetadata.RelationshipSourceId];
                newLink.IntermediaryEvent = innerObjects[firstRelMetadata.RelationshipTargetId];
                newLink.Target = innerObjects[secondRelMetadata.RelationshipTargetId];
                if (firstRelationship.LinkDirection == LinkDirection.SourceToTarget && secondRelationship.LinkDirection == LinkDirection.SourceToTarget)
                {
                    newLink.SetDirection(LinkDirection.SourceToTarget);
                }
                else if (firstRelationship.LinkDirection == LinkDirection.TargetToSource && secondRelationship.LinkDirection == LinkDirection.TargetToSource)
                {
                    newLink.SetDirection(LinkDirection.TargetToSource);
                }
            }
            else if (firstRelMetadata.RelationshipSourceId.Equals(secondRelMetadata.RelationshipTargetId))
            {
                newLink.Source = innerObjects[firstRelMetadata.RelationshipTargetId];
                newLink.IntermediaryEvent = innerObjects[secondRelMetadata.RelationshipTargetId];
                newLink.Target = innerObjects[secondRelMetadata.RelationshipSourceId];
                if (firstRelationship.LinkDirection == LinkDirection.TargetToSource && secondRelationship.LinkDirection == LinkDirection.TargetToSource)
                {
                    newLink.SetDirection(LinkDirection.SourceToTarget);
                }
                else if (firstRelationship.LinkDirection == LinkDirection.SourceToTarget && secondRelationship.LinkDirection == LinkDirection.SourceToTarget)
                {
                    newLink.SetDirection(LinkDirection.TargetToSource);
                }
            }
            else if (firstRelMetadata.RelationshipTargetId.Equals(secondRelMetadata.RelationshipTargetId))
            {
                newLink.Source = innerObjects[firstRelMetadata.RelationshipSourceId];
                newLink.IntermediaryEvent = innerObjects[firstRelMetadata.RelationshipTargetId];
                newLink.Target = innerObjects[secondRelMetadata.RelationshipSourceId];
                if (firstRelationship.LinkDirection == LinkDirection.SourceToTarget && secondRelationship.LinkDirection == LinkDirection.TargetToSource)
                {
                    newLink.SetDirection(LinkDirection.SourceToTarget);
                }
                else if (firstRelationship.LinkDirection == LinkDirection.TargetToSource && secondRelationship.LinkDirection == LinkDirection.SourceToTarget)
                {
                    newLink.SetDirection(LinkDirection.TargetToSource);
                }
            }
            else if (firstRelMetadata.RelationshipSourceId.Equals(secondRelMetadata.RelationshipSourceId))
            {
                newLink.Source = innerObjects[firstRelMetadata.RelationshipTargetId];
                newLink.IntermediaryEvent = innerObjects[secondRelMetadata.RelationshipSourceId];
                newLink.Target = innerObjects[secondRelMetadata.RelationshipTargetId];
                if (firstRelationship.LinkDirection == LinkDirection.TargetToSource && secondRelationship.LinkDirection == LinkDirection.SourceToTarget)
                {
                    newLink.SetDirection(LinkDirection.SourceToTarget);
                }
                else if (firstRelationship.LinkDirection == LinkDirection.SourceToTarget && secondRelationship.LinkDirection == LinkDirection.TargetToSource)
                {
                    newLink.SetDirection(LinkDirection.TargetToSource);
                }
            }
            else
            {
                throw new InvalidOperationException("Relationships' source and target are not match to be in a Event-based Link");
            }
            return newLink;
        }
        #endregion

        #region توابع ایجادی/ویرایشی
        public static RelationshipBasedKWLink CreateNewRelationshipBaseLink
            (KWObject source, KWObject target, string relationshipTypeUri, string description
            , LinkDirection linkDirection, DateTime? timeBegin, DateTime? timeEnd)
        {

            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (relationshipTypeUri == null)
                throw new ArgumentNullException(nameof(relationshipTypeUri));
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            KWRelationship linkInnerRelationship = GenerateNewUnpublishedRelationship
                (source, target, relationshipTypeUri, linkDirection, timeBegin, timeEnd, description);
            return GenerateRelationshipBasedLink(linkInnerRelationship, source, target);
        }

        public static EventBasedKWLink CreateNewEventBaseLink
            (KWObject source, KWObject target, string intermediaryEventTypeUri, string description
            , LinkDirection linkDirection, DateTime? timeBegin, DateTime? timeEnd
            , string sourceToIntermediaryEventRelationshipType, string intermediaryEventToTargetRelationshipType)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (intermediaryEventTypeUri == null)
                throw new ArgumentNullException(nameof(intermediaryEventTypeUri));
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (sourceToIntermediaryEventRelationshipType == null)
                throw new ArgumentNullException(nameof(sourceToIntermediaryEventRelationshipType));
            if (intermediaryEventToTargetRelationshipType == null)
                throw new ArgumentNullException(nameof(intermediaryEventToTargetRelationshipType));

            KWObject intermediaryEvent = ObjectManager.CreateNewObject(intermediaryEventTypeUri, description);

            var innerObjects = new Dictionary<long, KWObject>(3);
            innerObjects.Add(source.ID, source);
            if (!innerObjects.ContainsKey(target.ID))
            {
                innerObjects.Add(target.ID, target);
            }
            innerObjects.Add(intermediaryEvent.ID, intermediaryEvent);

            KWRelationship firstRelationship = GenerateNewUnpublishedRelationship
                (source, intermediaryEvent, sourceToIntermediaryEventRelationshipType, linkDirection, timeBegin, timeEnd, description);
            KWRelationship secondRelationship = GenerateNewUnpublishedRelationship
                (intermediaryEvent, target, intermediaryEventToTargetRelationshipType, linkDirection, timeBegin, timeEnd, description);
            return GenerateEventBasedLinkByInnerRelationshipsAndObjects
                (firstRelationship, secondRelationship, innerObjects);
        }

        /// <summary>
        /// حذف یک رابطه؛
        /// این تابع رابطه‌ی داده شده را به صورت نرم، حذف می‌کند؛
        /// حذف نرم به معنای عدم دستکاری در مخزن داده‌ها بوده و
        /// فقط برای رابطه‌های منتشر نشده قابل انجام است
        /// </summary>
        public static void DeleteRelationship(KWRelationship relationship)
        {
            if (relationship == null)
                throw new ArgumentNullException(nameof(relationship));

            if (IsUnpublishedRelationship(relationship))
            {
                cachedRelationshipsMetadataIdentifiedByID.Remove(relationship.ID);
                relationship.OnDeleted();
            }
            else
                throw new InvalidOperationException("Unable to delete published relationship");
        }
        #endregion

        #region توابع بازیابی
        public async static Task<List<RelationshipBasedKWLink>> GetRelationshipRangeByIdAsync(List<long> relationshipIds)
        {
            try
            {
                if (relationshipIds == null)
                    throw new ArgumentNullException(nameof(relationshipIds));

                int countOfRelationshipIds = relationshipIds.Count;
                List<RelationshipBasedKWLink> result = new List<RelationshipBasedKWLink>(countOfRelationshipIds);

                List<long> unpublishedRelationshipIds = new List<long>(countOfRelationshipIds);
                List<long> publishedRelationshipIds = new List<long>(countOfRelationshipIds);
                foreach (long id in relationshipIds)
                {
                    if (IsUnpublishedRelationship(id))
                    {
                        unpublishedRelationshipIds.Add(id);
                    }
                    else
                    {
                        publishedRelationshipIds.Add(id);
                    }
                }

                if (unpublishedRelationshipIds.Count > 0)
                    result.AddRange(await GetUnpublishedRelationshipBasedLinksByIdAsync(unpublishedRelationshipIds));

                if (publishedRelationshipIds.Count > 0)
                    result.AddRange(await RetriveRelationshipBasedLinksByIdAsync(publishedRelationshipIds.ToArray()));

                // TODO: آینده - در اینجا می‌توان رابطه‌هایی که قبلا وجود داشته‌اند ولی
                // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد
                // (شناسه‌هایی که قبلا معتبر بوده‌اند، ولی دیگر قابل بازیابی نیستند)

                // TODO: آینده - امکان بهبود کارایی این تابع مانند تابع معادل برای «شئ» وجود دارد

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async static Task<List<RelationshipBasedKWLink>> GetRelationshipsBySourceObjectAsync(KWObject objectToGetRelationships, string relationshipsTypeUri)
        {
            if (objectToGetRelationships == null)
                throw new ArgumentNullException(nameof(objectToGetRelationships));
            if (relationshipsTypeUri == null)
                throw new ArgumentNullException(nameof(relationshipsTypeUri));

            List<RelationshipBasedKWLink> result;

            result =
                await GetUnpublishedRelationshipBasedLinksBySourceObjectAsync(objectToGetRelationships, relationshipsTypeUri);

            if (!ObjectManager.IsUnpublishedObject(objectToGetRelationships))
                result.AddRange
                    (await RetriveRelationshipBasedLinksBySourceObjectAsync(objectToGetRelationships, relationshipsTypeUri));

            foreach (KWObject locallyResolvedObj in ObjectManager.GetObjectWhereLocallyResolvedToObject(objectToGetRelationships.ID))
            {
                result.AddRange
                    (await GetRelationshipsBySourceObjectAsync(locallyResolvedObj, relationshipsTypeUri));
            }

            // TODO: آینده - در اینجا می‌توان رابطه‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد
            // (رابطه‌هایی که قبلا با مبدا خاصی بازیابی شده‌اند و اکنون برای همان شئ بازیابی نمی‌شوند)

            return result;
        }

        public async static Task<Dictionary<KWRelationship, KWObject>> GetRelationshipsByTargetObjectIdAsync(long objectIdToGetRelationships, string relationshipsTypeUri)
        {
            if (relationshipsTypeUri == null)
                throw new ArgumentNullException(nameof(relationshipsTypeUri));

            Dictionary<KWRelationship, KWObject> result;

            result =
                await GetUnpublishedRelationshipsPerSourceByTargetObjectIdAsync(objectIdToGetRelationships, relationshipsTypeUri);

            if (!ObjectManager.IsUnpublishedObject(objectIdToGetRelationships))
            {
                var retrievedRelationshipsPerSources
                    = await RetriveRelationshipsPerSourceObjectByRelationshipTargetAsync
                        (objectIdToGetRelationships, relationshipsTypeUri);
                foreach (var item in retrievedRelationshipsPerSources)
                    result.Add(item.Key, item.Value);
            }

            foreach (KWObject locallyResolvedObj in ObjectManager.GetObjectWhereLocallyResolvedToObject(objectIdToGetRelationships))
            {
                foreach (KeyValuePair<KWRelationship, KWObject> RelationshipAndTargetObject
                    in await GetRelationshipsByTargetObjectIdAsync(locallyResolvedObj.ID, relationshipsTypeUri))
                {
                    result.Add(RelationshipAndTargetObject.Key, RelationshipAndTargetObject.Value);
                }
            }

            // TODO: آینده - در اینجا می‌توان رابطه‌هایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد
            // (رابطه‌هایی که قبلا با مقصد خاصی بازیابی شده‌اند و اکنون برای همان شئ بازیابی نمی‌شوند)

            return result;
        }
        #endregion

        #region توابع مدیریت ادغام اشیاء
        internal static void ChangeOwnerOfCachedRelationshipsForObject(KWObject objectToResolve, KWObject resolveMaster)
        {
            foreach (CachedRelationshipMetadata relMetadata in GetCachedRelationshipsRelatedToObject(objectToResolve))
            {
                if (relMetadata.RelationshipSourceId == objectToResolve.ID)
                {
                    relMetadata.RelationshipSourceId = resolveMaster.ID;
                }
                // !نمی‌خواهد else -- امکان اینکه دو سر رابطه یک شئ باشند که ادغام هم شده وجود دارد 
                if (relMetadata.RelationshipTargetId == objectToResolve.ID)
                {
                    relMetadata.RelationshipTargetId = resolveMaster.ID;
                }
            }
        }
        #endregion

        public async static Task<List<RelationshipBaseKlink>> ConvertKRelationshipListToKWRelationshipList
            (IEnumerable<UnpublishedRelationshipChanges.RelationshipEntity> kwRelationshipList)
        {
            List<RelationshipBaseKlink> relationshipBaseKlinkList = new List<RelationshipBaseKlink>();
            foreach (var link in kwRelationshipList)
            {
                RelationshipBaseKlink relationshipBaseKlink = new RelationshipBaseKlink();
                relationshipBaseKlink.Relationship = GetKRelationshipFromKWRelationship(link.Relationship);
                relationshipBaseKlink.TypeURI = link.Relationship.TypeURI;

                KObject remoteSource;
                if (ObjectManager.IsUnpublishedObject(link.Source))
                {
                    remoteSource = ObjectManager.GetKObjectFromKWObject(link.Source);
                }
                else
                {
                    KObject relationshipRemoteSourceResolutionMaster
                            = await ObjectManager.GetResolutionMasterForPublishedObjectIfRemotelyResolved(link.Source.ID);
                    if (relationshipRemoteSourceResolutionMaster != null)
                    {
                        remoteSource = relationshipRemoteSourceResolutionMaster;
                    }
                    else
                    {
                        remoteSource = ObjectManager.GetKObjectFromKWObject(link.Source);
                    }
                }
                relationshipBaseKlink.Source = remoteSource;

                KObject remoteTarget;
                if (ObjectManager.IsUnpublishedObject(link.Target))
                {
                    remoteTarget = ObjectManager.GetKObjectFromKWObject(link.Target);
                }
                else
                {
                    KObject relationshipRemoteTargetResolutionMaster
                            = await ObjectManager.GetResolutionMasterForPublishedObjectIfRemotelyResolved(link.Target.ID);
                    if (relationshipRemoteTargetResolutionMaster != null)
                    {
                        remoteTarget = relationshipRemoteTargetResolutionMaster;
                    }
                    else
                    {
                        remoteTarget = ObjectManager.GetKObjectFromKWObject(link.Target);
                    }
                }
                relationshipBaseKlink.Target = remoteTarget;

                relationshipBaseKlinkList.Add(relationshipBaseKlink);
            }
            return relationshipBaseKlinkList;
        }
        public static KRelationship GetKRelationshipFromKWRelationship(KWRelationship objectToConvert)
        {
            if (objectToConvert == null)
                throw new ArgumentNullException(nameof(objectToConvert));

            KRelationship result = new KRelationship()
            {
                Id = objectToConvert.ID,
                Direction = GetRemoteServiceDirectionFromLocalEntitiesDirection(objectToConvert.LinkDirection),
                Description = objectToConvert.Description,
                TimeBegin = objectToConvert.TimeBegin,
                TimeEnd = objectToConvert.TimeEnd
            };

            // بازگرداندن شی معادل سمت سرور
            return result;
        }

        public static async Task<List<RelationshipBaseKlink>> GetRelationshipBaseKlinksFromRelationships(IEnumerable<KWRelationship> kwRelationships)
        {
            List<RelationshipBaseKlink> result = new List<RelationshipBaseKlink>();
            foreach (KWRelationship kwRel in kwRelationships)
            {
                CachedRelationshipMetadata metadata = GetCachedMetadataById(kwRel.ID);
                KWObject source = await ObjectManager.GetObjectByIdAsync(metadata.RelationshipSourceId);
                KWObject target = await ObjectManager.GetObjectByIdAsync(metadata.RelationshipTargetId);
                RelationshipBaseKlink newLink = new RelationshipBaseKlink()
                {
                    Relationship = GetKRelationshipFromKWRelationship(kwRel),
                    Source = ObjectManager.GetKObjectFromKWObject(source),
                    Target = ObjectManager.GetKObjectFromKWObject(target),
                    TypeURI = kwRel.TypeURI
                };
                result.Add(newLink);
            }
            return result;
        }
    }
}
