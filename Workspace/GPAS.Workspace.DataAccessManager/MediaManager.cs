using GPAS.Dispatch.Entities.Concepts;
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
    /// مدیریت داده‌ای مدیاها سمت محیط کاربری
    /// </summary>
    public class MediaManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private MediaManager()
        { }

        #region مدیریت میانگیری و رفع تصادم‌ها | Caching & Conflicts resolution
        internal class CachedMediaMetadata
        {
            internal KWMedia CachedMedia;
            internal bool IsPublished;
            internal bool IsDeleted;
        }

        private static Dictionary<long, CachedMediaMetadata> cachedMediasIdentifiedByID
            = new Dictionary<long, CachedMediaMetadata>();

        private static void AddMediaToCache(KWMedia media, bool isPublished)
        {
            var newMediaMetadata = new CachedMediaMetadata()
            {
                CachedMedia = media,
                IsPublished = isPublished,
                IsDeleted = false
            };
            cachedMediasIdentifiedByID.Add(media.ID, newMediaMetadata);
        }
        private static void SetCachedMediaAsDeleted(KWMedia media)
        {
            CachedMediaMetadata mediaMetadata = GetCachedMetadataById(media.ID);
            (mediaMetadata as CachedMediaMetadata).IsDeleted = true;
        }
        private static IEnumerable<KWMedia> GetUnpublishedMediaForObject(KWObject objectToGetMedia)
        {
            return cachedMediasIdentifiedByID
                .Values
                .Select(metadata => metadata.CachedMedia)
                .Where(cachedMedia
                   => cachedMedia.Owner.Equals(objectToGetMedia)
                   && IsUnpublishedMedia(cachedMedia));
        }
        private static void UpdateCacheByRetrievedMedia(KMedia item)
        {
            // راهکار سابق برای رفع تصادم بین داده‌های محلی و راه‌دور
            //if (IsRetrievedMediaConflictsWithCache(item))
            //{
            //    //ResolveRetrievedAndCachedMediaConflicts(item);
            //}
        }
        private static KWMedia GetCachedMediaById(long mediaId)
        {
            return GetCachedMetadataById(mediaId).CachedMedia;
        }
        private static CachedMediaMetadata GetCachedMetadataById(long mediaId)
        {
            return cachedMediasIdentifiedByID[mediaId];
        }
        private static bool IsRetrievedMediaCachedBefore(KMedia item)
        {
            return cachedMediasIdentifiedByID.ContainsKey(item.Id);
        }
        private static bool IsRetrievedMediaConflictsWithCache(KMedia item)
        {
            return GetCachedMetadataById(item.Id).IsDeleted;
            // در حال حاظر، به خاطر نبودن امکان تغییر توضیحات مدیا، نیاز بررسی دیگری نیست
        }
        private static void DiscardUnpublishedChangesForMedia(KMedia retrievedMedia)
        {
            var cachedMediaMetadata = GetCachedMetadataById(retrievedMedia.Id);
            cachedMediaMetadata.IsDeleted = false;
        }
        private static void ChangeCachedMediaId(long currentId, long newId)
        {
            var mediaMetadata = GetCachedMetadataById(currentId);
            cachedMediasIdentifiedByID.Remove(currentId);
            cachedMediasIdentifiedByID.Add(newId, mediaMetadata);
            mediaMetadata.CachedMedia.ID = newId;
            //mediaMetadata.IsDeleted = false;  // نیازی به این انتساب نیست
        }

        private static bool IsMediaCached(KWMedia mediaToCheck)
        {
            return cachedMediasIdentifiedByID.ContainsKey(mediaToCheck.ID);
        }

        private static IEnumerable<KWMedia> GetUnpublishedMediasNotDeletedLocally()
        {
            return cachedMediasIdentifiedByID
                .Values
                .Where(mediaMetadata
                    => !mediaMetadata.IsDeleted
                    && IsUnpublishedMedia(mediaMetadata.CachedMedia))
                .Select(metadata => metadata.CachedMedia);
        }
        
        private static IEnumerable<CachedMediaMetadata> GetCachedMediaMetadataOfUnpublishedMediasDeletedLocally()
        {
            return cachedMediasIdentifiedByID
                .Values
                .Where(mediaMetadata
                    => !mediaMetadata.IsDeleted
                    && IsUnpublishedMedia(mediaMetadata.CachedMedia));
        }
        private static IEnumerable<KWMedia> GetCachedPublishedMediasDeletedLocally()
        {
            return cachedMediasIdentifiedByID
                .Values
                .Where(mediaMetadata
                    => mediaMetadata.IsDeleted
                    && !IsUnpublishedMedia(mediaMetadata.CachedMedia))
                .Select(metadata => metadata.CachedMedia);
        }

        private static IEnumerable<CachedMediaMetadata> GetCachedMediaMetadataOfPublishedMediasDeletedLocally()
        {
            return cachedMediasIdentifiedByID
                .Values
                .Where(mediaMetadata
                    => mediaMetadata.IsDeleted
                    && !IsUnpublishedMedia(mediaMetadata.CachedMedia));
        }

        private static IEnumerable<KWMedia> GetCachedMediasForObject(KWObject obj)
        {
            return cachedMediasIdentifiedByID
                .Values
                .Where(mediaMetadata => mediaMetadata.CachedMedia.Owner.Equals(obj))
                .Select(mediaMetadata => mediaMetadata.CachedMedia);
        }

        public static bool IsUnpublishedMedia(KWMedia mediaToCheck)
        {
            if (mediaToCheck == null)
                throw new ArgumentNullException("mediaToCheck");

            if (IsMediaCached(mediaToCheck))
            {
                var metadata = GetCachedMetadataById(mediaToCheck.ID);
                return !metadata.IsPublished;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region خدمات انتشار مدیاها | Publish services
        public static UnpublishedMediaChanges GetUnpublishedChanges()
        {
            var changes = new UnpublishedMediaChanges();
            changes.AddedMedias = GetUnpublishedMediasNotDeletedLocally();
            changes.DeletedMedias = GetCachedPublishedMediasDeletedLocally();
            return changes;
        }

        internal static List<CachedMediaMetadata> GetCachedMediaMetadatas()
        {
            List<CachedMediaMetadata> changes = new List<CachedMediaMetadata>();
            List<CachedMediaMetadata> addedMedias = GetCachedMediaMetadataOfUnpublishedMediasDeletedLocally().ToList();
            List<CachedMediaMetadata> deletedMedias = GetCachedMediaMetadataOfPublishedMediasDeletedLocally().ToList();
            changes.AddRange(addedMedias);
            changes.AddRange(deletedMedias);
            return changes;
        }

        public static void CommitUnpublishedChanges(IEnumerable<long> addedMediaIDs, IEnumerable<long> deletedMediaIDs, long dataSourceID)
        {
            if (addedMediaIDs == null)
                throw new ArgumentNullException(nameof(addedMediaIDs));

            ApplyMediasAdditionToCache(addedMediaIDs);
            ApplyMediasDeletationToCache(deletedMediaIDs);
            SetDataSourceIdForAddedMedias(addedMediaIDs, dataSourceID);
        }
        
        private static void SetDataSourceIdForAddedMedias(IEnumerable<long> addedMediaIDs, long dataSourceID)
        {
            foreach (long mediaID in addedMediaIDs)
            {
                KWMedia media = GetCachedMediaById(mediaID);
                media.DataSourceId = dataSourceID;
            }
        }

        /// <summary>
        /// تغییرات محلی و همچنین میانگیری‌های قبلی را از بین می‌برد
        /// </summary>
        public static void DiscardChanges()
        {
            cachedMediasIdentifiedByID.Clear();
        }

        private static bool IsAnyUnpublishedChangeRemains()
        {
            bool result = false;
            foreach (var metadata in cachedMediasIdentifiedByID.Values)
                if (IsUnpublishedMedia(metadata.CachedMedia)
                    || metadata.IsDeleted)
                {
                    result = true;
                    break;
                }
            return result;
        }
        private static void ApplyMediasAdditionToCache(IEnumerable<long> addedMediaIDs)
        {
            List<long> IdList = addedMediaIDs.ToList();
            while (IdList.Count > 0)
            {
                CachedMediaMetadata metadata = cachedMediasIdentifiedByID[IdList[0]];
                metadata.IsPublished = true;
                IdList.RemoveAt(0);
            }
        }

        internal static void ClearCache()
        {
            cachedMediasIdentifiedByID.Clear();
        }

        internal static void AddSavedMetadataToCache(List<CachedMediaMetadata> mediasMetadata)
        {
            foreach (var currentMetadata in mediasMetadata)
            {
                if (!cachedMediasIdentifiedByID.ContainsKey(currentMetadata.CachedMedia.ID))
                {
                    cachedMediasIdentifiedByID.Add(currentMetadata.CachedMedia.ID, currentMetadata);
                }
                else
                {
                    UpdateCacheBySavedMetadata(currentMetadata);
                }
            }
        }

        private static void UpdateCacheBySavedMetadata(CachedMediaMetadata mediaMetadata)
        {
            if (cachedMediasIdentifiedByID.ContainsKey(mediaMetadata.CachedMedia.ID))
            {
                CachedMediaMetadata existanceMetadataInCache = cachedMediasIdentifiedByID[mediaMetadata.CachedMedia.ID];
                existanceMetadataInCache.CachedMedia = mediaMetadata.CachedMedia;
                existanceMetadataInCache.IsDeleted = mediaMetadata.IsDeleted;
                existanceMetadataInCache.IsPublished = mediaMetadata.IsPublished;
            }
        }

        private static void ApplyMediasDeletationToCache(IEnumerable<long> deletedMediaIDs)
        {
            List<long> IdOfMediasToDelete = deletedMediaIDs.ToList();
            while (IdOfMediasToDelete.Count > 0)
            {
                cachedMediasIdentifiedByID.Remove(IdOfMediasToDelete[0]);
                IdOfMediasToDelete.RemoveAt(0);
            }
        }
        #endregion

        #region تعامل با سرویس‌دهنده راه‌دور
        private static long GetNewMediaId()
        {
            long mediaId = -1;
            // فراخوانی سرویس راه دور
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                mediaId = sc.GetNewMediaId();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            if (mediaId == -1)
                throw new NullReferenceException("Invalid server response");

            return mediaId;
        }
        private async static Task<List<KWMedia>> RetriveMediaForObjectAsync(KWObject objectToGetMedia)
        {
            KMedia[] retrievedKMedias = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                retrievedKMedias = await sc.GetMediaUrisForObjectAsync(objectToGetMedia.ID);
            }
            finally
            {
                sc.Close();
            }
            if (retrievedKMedias == null)
                throw new NullReferenceException("Invalid server Response.");

            List<KWMedia> mediasForObject = new List<KWMedia>();
            foreach (var item in retrievedKMedias)
            {
                KWMedia itemMedia = await GetMediaFromRetrievedDataAsync(item);
                mediasForObject.Add(itemMedia);
            }
            return mediasForObject;
        }
        private async static Task<KWMedia> GetMediaFromRetrievedDataAsync(KMedia item)
        {
            KWMedia result = null;
            if (IsRetrievedMediaCachedBefore(item))
            {
                UpdateCacheByRetrievedMedia(item);
                result = GetCachedMediaById(item.Id);
            }
            else
            {
                result = await GenerateReterievedMediaAsync(item);
            }
            return result;
        }
        #endregion

        #region سازنده‌های مدیاها | Singleton Constructions - CRITICAL SECTION
        private static object generateNewUnpublishedMediaLockObject = new object();
        private static KWMedia GenerateNewUnpublishedMedia(MediaPathContent mediaPath, string description, KWObject owner)
        {
            KWMedia newMedia;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک مدیا، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی مدیا، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateNewUnpublishedMediaLockObject)
            {
                long newMediaId = GetNewMediaId();
                newMedia = new KWMedia()
                {
                    ID = newMediaId,
                    MediaUri = mediaPath,
                    Description = description,
                    Owner = owner,
                    DataSourceId = null
                };
                AddMediaToCache(newMedia, false);
            }
            return newMedia;
        }
        
        private static object generateReterievedMediaLockObject = new object();
        private async static Task<KWMedia> GenerateReterievedMediaAsync(KMedia retrievedMedia)
        {
            string[] uriSegment = retrievedMedia.URI.Split('/');
            string mediaDisplayName = uriSegment[uriSegment.Length - 1];
            MediaPathContent mediaPath = new MediaPathContent()
            {
                DisplayName = mediaDisplayName,
                Type = MediaPathContentType.File,
                UriAddress = retrievedMedia.URI
            };
            KWObject owner = await ObjectManager.GetObjectByIdAsync(retrievedMedia.OwnerObjectId);

            KWMedia newMedia;
            // با توجه به ایستا بودن متد و فراخوانی‌های غیرهمگام در محیط کاربری
            // ایجاد یک مدیا، ناحیه‌ی بحرانی محسوب می‌شود
            //
            // طبق چینش متدهای این کلاس، در واقع ناحیه بحرانی از زمانی شروع می‌شود
            // که پس از بررسی میانگیری قبلی مدیا، تصمیم به ایجاد آن گرفته می‌شود
            // اما از آنجا که امکان قفل کردن کدی که دارای فراخوانی غیرهمگام باشد
            // وجود ندارد، قفل شدن کد به این قسمت از کد منتقل شده
            lock (generateReterievedMediaLockObject)
            {
                if (IsRetrievedMediaCachedBefore(retrievedMedia))
                    newMedia = GetCachedMediaById(retrievedMedia.Id);
                else
                {
                    newMedia = new KWMedia()
                    {
                        ID = retrievedMedia.Id,
                        Description = retrievedMedia.Description,
                        MediaUri = mediaPath,
                        Owner = owner,
                        DataSourceId = retrievedMedia.DataSourceID
                    };
                    AddMediaToCache(newMedia, true);
                }
            }
            return newMedia;
        }
        #endregion

        #region توابع ایجادی/ویرایشی
        public static KWMedia CreateNewMediaForObject(MediaPathContent mediaPath, string description, KWObject objectToAddMedia)
        {
            if (mediaPath == null)
                throw new ArgumentNullException("mediaPath");
            if (description == null)
                throw new ArgumentNullException("description");
            if (objectToAddMedia == null)
                throw new ArgumentNullException("objectToAddMedia");

            return GenerateNewUnpublishedMedia(mediaPath, description, objectToAddMedia);
        }

        public static void DeleteMedia(KWMedia media)
        {
            if (media == null)
                throw new ArgumentNullException("media");

            if (IsUnpublishedMedia(media))
            {
                cachedMediasIdentifiedByID.Remove(media.ID);
                media.OnDeleted();
            }
            else
                SetCachedMediaAsDeleted(media);
        }
        #endregion

        #region توابع بازیابی
        public async static Task<List<KWMedia>> GetMediaForObjectAsync(KWObject objectToGetMedia)
        {
            if (objectToGetMedia == null)
                throw new ArgumentNullException("objectToGetMedia");

            List<KWMedia> result = new List<KWMedia>
                (GetUnpublishedMediaForObject(objectToGetMedia));
            if (!ObjectManager.IsUnpublishedObject(objectToGetMedia))
                result.AddRange
                    (await RetriveMediaForObjectAsync(objectToGetMedia));

            foreach (KWObject locallyResolvedObj in ObjectManager.GetObjectWhereLocallyResolvedToObject(objectToGetMedia.ID))
            {
                result.AddRange
                    (await GetMediaForObjectAsync(locallyResolvedObj));
            }

            // TODO: آینده - در اینجا می‌توان مدیاهایی که قبلا وجود داشته‌اند ولی
            // دیگر بین داده‌ها منتشر شده وجود ندارند را شناسایی کرد
            return result;
        }

        public static bool IsDeletedMedia(KWMedia mediaToCheck)
        {
            if (mediaToCheck == null)
                throw new ArgumentNullException("mediaToCheck");
            return cachedMediasIdentifiedByID[mediaToCheck.ID].IsDeleted;
        }
        #endregion

        #region توابع مدیریت ادغام اشیاء
        internal static void ChangeOwnerOfCachedMediasForObject(KWObject objectToResolve, KWObject resolveMaster)
        {
            foreach (KWMedia relatedMedia in GetCachedMediasForObject(objectToResolve))
            {
                ChangeOwnerOfMedia(relatedMedia.ID, resolveMaster);
            }
        }
        private static void ChangeOwnerOfMedia(long mediaID, KWObject newOwner)
        {
            var mediaMetadata = GetCachedMetadataById(mediaID);
            mediaMetadata.CachedMedia.Owner = newOwner;
        }
        #endregion

        public async static Task<List<KMedia>> GetKMediasFromKWMedias(IEnumerable<KWMedia> KWMediaList, bool applyRemoteResolutionForMediaOwner = true)
        {
            List<KMedia> KMediaList = new List<KMedia>();
            foreach (var item in KWMediaList)
            {
                KMedia kmedia = new KMedia();
                kmedia.Id = item.ID;
                kmedia.URI = item.MediaUri.UriAddress;
                kmedia.Description = item.Description;

                if (applyRemoteResolutionForMediaOwner
                    && ObjectManager.IsUnpublishedObject(item.Owner))
                {
                    KObject mediaRemoteOwnerResolutionMaster
                       = await ObjectManager.GetResolutionMasterForPublishedObjectIfRemotelyResolved(item.Owner.ID);
                    if (mediaRemoteOwnerResolutionMaster != null)
                    {
                        kmedia.OwnerObjectId = mediaRemoteOwnerResolutionMaster.Id;
                    }
                    else
                    {
                        kmedia.OwnerObjectId = item.Owner.ID;
                    }
                }
                else
                {
                    kmedia.OwnerObjectId = item.Owner.ID;
                }

                KMediaList.Add(kmedia);
            }
            return KMediaList;
        }
    }
}