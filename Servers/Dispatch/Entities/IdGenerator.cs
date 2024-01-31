using System;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GPAS.Dispatch.Entities
{
    public class IdGenerator
    {
        private static readonly string mainUrl = ConfigurationManager.AppSettings["IdGeneratorApiUrl"];
        private static readonly long InitialInvalidId = 0;
        public long[] ExcludedIds { get; private set; }
        private long lastAssignedId;
        public long LastAssignedId
        {
            get
            {
                if (!IsInDiskOnlyIdCachingMode)
                { return lastAssignedId; }
                else
                { return LoadIdFromDisk(); }
            }
            private set
            {
                if (!IsInDiskOnlyIdCachingMode)
                { lastAssignedId = value; }
                else
                { SaveIdToDisk(value); }
            }
        }
        public string SaveLastAssignedIdPath { get; private set; }
        private bool SaveLastAssignedID = false;
        private bool isInitialized = false;
        private IdGeneratorItems currentItem;
        private IdGeneratorProvider idGeneratorProvider = new IdGeneratorProvider();

        public bool IsInDiskOnlyIdCachingMode { get; private set; }

        public static bool IsValidId(long lastAssignedId)
        {
            return lastAssignedId > InitialInvalidId;
        }

        public IdGenerator(IdGeneratorItems item, string saveLastAssignedIdPath = "", bool enableDiskOnlyIdCachingMode = false)
        {
            SaveLastAssignedIdPath = saveLastAssignedIdPath;
            IsInDiskOnlyIdCachingMode = enableDiskOnlyIdCachingMode;
            LastAssignedId = InitialInvalidId;
            currentItem = item;
        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        /// <summary>
        /// آماده‌سازی شناسه‌ساز؛
        /// این تابع در مواقعی که قبلا شناسه‌ای انتساب داده نشده است استفاده می‌شود
        /// </summary>
        public void Init()
        {
            Init(InitialInvalidId + 1);
        }
        /// <summary>
        /// آماده‌سازی شناسه‌ساز
        /// </summary>
        /// <param name="lastAssignedId">آخرین شناسه‌ای که به درستی اختصاص یافته</param>
        public void Init(long lastAssignedId)
        {
            Init(lastAssignedId, new long[] { });
        }

        /// <summary>
        /// آماده‌سازی شناسه‌ساز
        /// </summary>
        /// <param name="lastAssignedId">آخرین شناسه‌ای که به درستی اختصاص یافته</param>
        /// <param name="excludedIds">شناسه‌هایی که نباید انتساب داده شوند</param>
        public void Init(long lastAssignedId, long[] excludedIds)
        {
            lock (this)
            {
                if (IsInitialized())
                    throw new InvalidOperationException("ID generator is currently initialized");
                if (!IsValidId(lastAssignedId))
                    throw new ArgumentException("Invalid ID for initialization", "lastAssignedId");

                ExcludedIds = excludedIds;
                LastAssignedId = lastAssignedId;
                SaveLastAssignedID = IsPathValidForSave(SaveLastAssignedIdPath);
                isInitialized = true;
            }
        }
        public void Init(long[] excludedIds)
        {
            Init(InitialInvalidId + 1, excludedIds);
        }

        private bool IsPathValidForSave(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            if (!File.Exists(path))
                return false;
            try
            {
                string existContent = File.ReadAllText(path);
                File.WriteAllText(path, existContent);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public long GenerateNewID()
        {
            return idGeneratorProvider.GetNewId(currentItem);
           // return GenerateIDRange(1);
        }

        public long GenerateIDRange(long count)
        {
            return idGeneratorProvider.GetNewRangeId(currentItem,count);
            //lock (this)
            //{
            //    if (!IsInitialized())
            //        throw new InvalidOperationException("ID generator is not initialized");
            //    long newLastId = LastAssignedId;
            //    do
            //    {
            //        newLastId = newLastId + count;
            //    } while (ExcludedIds.Contains(newLastId)); // TODO: برای انتساب دسته‌ای بازبینی شود

            //    LastAssignedId = newLastId;

            //    if (SaveLastAssignedID && !IsInDiskOnlyIdCachingMode)
            //    {
            //        SaveIdToDisk(newLastId);
            //    }

            //    return newLastId;
            //}
        }

        private object saveIdToDiskLockObject = new object();
        private void SaveIdToDisk(long id)
        {
            lock (saveIdToDiskLockObject)
            {
                using (StreamWriter sw = new StreamWriter(SaveLastAssignedIdPath))
                {
                    sw.Write(id.ToString());
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private object loadIdToDiskLockObject = new object();
        private long LoadIdFromDisk()
        {
            lock (loadIdToDiskLockObject)
            {
                return long.Parse(File.ReadAllText(SaveLastAssignedIdPath));
            }
        }
    }
}
