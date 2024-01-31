using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GPAS.IdGenrator.Services
{
    public class RepositoryProvider
    {       
        private readonly List<ItemModel> AllItems;

        public RepositoryProvider()
        {
            string mainFolder = ConfigurationManager.AppSettings["MainFolder"];
            AllItems = new List<ItemModel>
            {
                new ItemModel
                {
                    Type = Items.Object,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedObjectIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.Property,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedPropertyIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.Relation,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedRelationshipIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.Graph,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedGraphIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.Media,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedMediaIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.DataSourse,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedDataSourceIdStorePath"])
                },
                new ItemModel
                {
                    Type = Items.Investigation,
                    FilePath = Path.Combine(mainFolder, ConfigurationManager.AppSettings["LastAssignedInvestigationIdStorePath"])
                }
            };

            if (!Directory.Exists(mainFolder))
            {
                Directory.CreateDirectory(mainFolder);
                foreach (var item in AllItems)
                {
                    File.WriteAllLines(item.FilePath, new string[] { "0" });
                }
            }
        }

        public long GetNewObjectId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Object).FilePath);
        }

        public long GetNewObjectIdRange(long range)
        {
            long lastAssignableComponentId = GenerateIDRange(range, AllItems.FirstOrDefault(i => i.Type == Items.Object).FilePath);
            return lastAssignableComponentId - range + 1;
        }

        public long GetNewPropertyId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Property).FilePath);
        }

        public long GetNewPropertyIdRange(long range)
        {
            long lastAssignableComponentId = GenerateIDRange(range, AllItems.FirstOrDefault(i => i.Type == Items.Property).FilePath);
            return lastAssignableComponentId - range + 1;
        }

        public long GetNewRelationId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Relation).FilePath);
        }

        public long GetNewRelationIdRange(long range)
        {
            long lastAssignableComponentId = GenerateIDRange(range, AllItems.FirstOrDefault(i => i.Type == Items.Relation).FilePath);
            return lastAssignableComponentId - range + 1;
        }

        public long GetNewMediaId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Media).FilePath);
        }

        public long GetNewMediaIdRange(long range)
        {
            long lastAssignableComponentId = GenerateIDRange(range, AllItems.FirstOrDefault(i => i.Type == Items.Media).FilePath);
            return lastAssignableComponentId - range + 1;
        }

        public long GetNewDataSourceId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.DataSourse).FilePath);
        }

        public long GetNewGraphId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Graph).FilePath);
        }

        public long GetNewInvestigationId()
        {
            return GenerateNewID(AllItems.FirstOrDefault(i => i.Type == Items.Investigation).FilePath);
        }


        private long GenerateNewID(string path)
        {
            return GenerateIDRange(1, path);
        }

        private long GenerateIDRange(long count, string path)
        {
            lock (this)
            {
                long newLastId = LoadIdFromFrom(path);
                newLastId += count;
                SaveIdToFile(newLastId, path);
                return newLastId;
            }
        }

        private readonly object saveIdToDiskLockObject = new object();

        private void SaveIdToFile(long id, string path)
        {
            lock (saveIdToDiskLockObject)
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(id.ToString());
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private readonly object loadIdToDiskLockObject = new object();

        private long LoadIdFromFrom(string path)
        {
            lock (loadIdToDiskLockObject)
            {
                return long.Parse(File.ReadAllText(path));
            }
        }
    }
}