using GPAS.Dispatch.Entities;
using System.Configuration;
using System.IO;

namespace GPAS.Dispatch.Logic
{
    public static class IdGenerators
    {
        public static IdGenerator ObjectIdGenerator = null;
        public static IdGenerator PropertyIdGenerator = null;
        public static IdGenerator RelationIdGenerator = null;
        public static IdGenerator MediaIdGenerator = null;
        public static IdGenerator GraphIdGenerator = null;
        public static IdGenerator DataSourceIdGenerator = null;
        public static IdGenerator DataInvestigationGenerator = null;

        private static readonly string LastAssignedObjectIdStorePath = ConfigurationManager.AppSettings["LastAssignedObjectIdStorePath"];
        private static readonly string LastAssignedPropertyIdStorePath = ConfigurationManager.AppSettings["LastAssignedPropertyIdStorePath"];
        private static readonly string LastAssignedRelationshipIdStorePath = ConfigurationManager.AppSettings["LastAssignedRelationshipIdStorePath"];
        private static readonly string LastAssignedMediaIdStorePath = ConfigurationManager.AppSettings["LastAssignedMediaIdStorePath"];
        private static readonly string LastAssignedGraphIdStorePath = ConfigurationManager.AppSettings["LastAssignedGraphIdStorePath"];
        private static readonly string LastAssignedDataSourceIdStorePath = ConfigurationManager.AppSettings["LastAssignedDataSourceIdStorePath"];
        private static readonly string LastAssignedInvestigationStorePath = ConfigurationManager.AppSettings["LastAssignedInvestigationIdStorePath"];
        private static bool IsDiskOnlyIdCachingModeEnabled = false;
        private static RepositoryProvider rp = null;
        private static RepositoryProvider GetRepositoryProvider
        {
            get
            {
                if (rp == null)
                    rp = new RepositoryProvider(AccessControl.Users.NativeUser.Admin.ToString());
                return rp;
            }
        }

        public static void InitializeIdGenerators()
        {
            ValidateStorePathes();

            string DiskOnlyIdCachingModeAppSettingValue = ConfigurationManager.AppSettings["EnableDiskOnlyIdCachingMode"];
            // در صورتی که این تنظیم وجود نداشته باشد این ویژگی غیرفعال خواهد ماند، در نتیجه
            // می‌توان این تنظیم را در نسخه‌ی مشتری قرار نداد تا باعث کاهش کارایی نشود
            if (!string.IsNullOrWhiteSpace(DiskOnlyIdCachingModeAppSettingValue))
            {
                bool.TryParse(DiskOnlyIdCachingModeAppSettingValue, out IsDiskOnlyIdCachingModeEnabled);
            }

            InitIdGenerator(ref ObjectIdGenerator, GetLastAssignedObjectId(), LastAssignedObjectIdStorePath,IdGeneratorItems.Object);
            InitIdGenerator(ref PropertyIdGenerator, GetLastAssignedPropertyId(), LastAssignedPropertyIdStorePath, IdGeneratorItems.Property);
            InitIdGenerator(ref RelationIdGenerator, GetLastAssignedRelationId(), LastAssignedRelationshipIdStorePath, IdGeneratorItems.Relation);
            InitIdGenerator(ref MediaIdGenerator, GetLastAssignedMediaId(), LastAssignedMediaIdStorePath, IdGeneratorItems.Media);
            InitIdGenerator(ref GraphIdGenerator, GetLastAssignedGraphaId(), LastAssignedGraphIdStorePath, IdGeneratorItems.Graph);
            InitIdGenerator(ref DataSourceIdGenerator, GetLastAsignedDataSourceId(), LastAssignedDataSourceIdStorePath, IdGeneratorItems.DataSourse);
            InitIdGenerator(ref DataInvestigationGenerator, GetLastAsignedInvestigationId(), LastAssignedInvestigationStorePath, IdGeneratorItems.Investigation);
        }

        private static void ValidateStorePathes()
        {
            ValidateStorePath(LastAssignedObjectIdStorePath);
            ValidateStorePath(LastAssignedPropertyIdStorePath);
            ValidateStorePath(LastAssignedRelationshipIdStorePath);
            ValidateStorePath(LastAssignedMediaIdStorePath);
            ValidateStorePath(LastAssignedGraphIdStorePath);
            ValidateStorePath(LastAssignedDataSourceIdStorePath);
            ValidateStorePath(LastAssignedInvestigationStorePath);
        }

        private static void ValidateStorePath(string storePath)
        {
            if (storePath == null)
                throw new ConfigurationErrorsException("Unable to read one or more 'Last Assigned ID Store Path' app settings form config file");
            var fi = new FileInfo(storePath);
            if (!fi.Exists)
            {
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                using (var sw = new StreamWriter(fi.OpenWrite()))
                {
                    sw.Write("-1");
                    sw.Close();
                }
            }
        }

        private static long GetLastAsignedInvestigationId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedInvestigationStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return 1;
            }
        }
        private static long GetLastAsignedDataSourceId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedDataSourceIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedDataSourceId();
            }
        }
        private static long GetLastAssignedGraphaId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedGraphIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedGraphId();
            }
        }
        private static long GetLastAssignedMediaId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedMediaIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedMediaId();
            }
        }
        private static long GetLastAssignedRelationId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedRelationshipIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedRelationId();
            }
        }
        private static long GetLastAssignedPropertyId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedPropertyIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedPropertyId();
            }
        }
        private static long GetLastAssignedObjectId()
        {
            long locallyStoredID = 0;
            if (TryReadStoredIdFromPath(LastAssignedObjectIdStorePath, out locallyStoredID)
                && locallyStoredID > 0)
            {
                return locallyStoredID;
            }
            else
            {
                return GetRepositoryProvider.GetLastAsignedObjectId();
            }
        }

        private static bool TryReadStoredIdFromPath(string idStorePath, out long storedID)
        {
            string IdFileContent = File.ReadAllText(idStorePath);
            return long.TryParse(IdFileContent, out storedID);
        }

        private static object initIdGeneratorLockObject = new object();
        private static void InitIdGenerator(ref IdGenerator generator, long lastAssignedId, long[] excludedIds, string writeIdPath, IdGeneratorItems item)
        {
            lock (initIdGeneratorLockObject)
            {
                generator = new IdGenerator(item, writeIdPath, IsDiskOnlyIdCachingModeEnabled);
                if (IdGenerator.IsValidId(lastAssignedId))
                    generator.Init(lastAssignedId, excludedIds);
                else
                    generator.Init(excludedIds);
            }
        }
        private static void InitIdGenerator(ref IdGenerator generator, long lastAssignedId, string writeIdPath, IdGeneratorItems item)
        {
            InitIdGenerator(ref generator, lastAssignedId, new long[] { }, writeIdPath, item);
        }
    }
}
