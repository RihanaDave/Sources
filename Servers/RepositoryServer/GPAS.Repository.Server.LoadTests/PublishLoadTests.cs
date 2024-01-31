using System;
using GPAS.AccessControl;
using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GPAS.LoadTest.Core;

namespace GPAS.Repository.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class PublishLoadTests : BaseLoadTest, ILoadTestCategory
    {
        private const int BatchItems = 1000;
        private const int DataSourceId = 300;
        private const int MaxPropertyValuesLength = 100;
        private Stopwatch timer;

        public ServerType ServerType { get; set; } = ServerType.DataRepository;
        public TestClassType TestClassType { get; set; } = TestClassType.Publish;
        public string Title { get; set; } = "Publish Objects";

        public event EventHandler<StepsProgressChangedEventArgs> StepsProgressChanged;

        public event EventHandler<TestCaseProgressChangedEventArgs> TestCaseProgressChanged;

        protected virtual void OnTestCaseProgressChanged(TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnStepsProgressChanged(StepsProgressChangedEventArgs e)
        {
            StepsProgressChanged?.Invoke(this, e);
        }

        [ClassInitialize]
        public static void SetFileNameToSaveLogFile(TestContext testContext)
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.Publish();
        }

        private void Publish()
        {
            ExtensionFileToSave = DateTime.Now.ToString("HH-mm-ss") + ".txt";
            WriteLog(" ******************  Publish objects load tests  ****************** ");
        }

        public DBAddedConcepts GenerateObjectsWithProperty(bool multiProperties)
        {
            List<DBObject> objectsList = new List<DBObject>();

            for (var id = MaxObjectId; id < MaxObjectId + BatchItems; id++)
            {
                objectsList.Add(new DBObject
                {
                    Id = id,
                    IsGroup = false,
                    TypeUri = "شخص",
                    LabelPropertyID = id
                });
            }

            List<DBProperty> propertiesList = new List<DBProperty>();

            int objectListIndex = 0;

            for (long id = MaxPropertyId; id < MaxPropertyId + BatchItems; id++)
            {
                propertiesList.Add(new DBProperty
                {
                    Id = id,
                    TypeUri = "label",
                    Value = RandomString(),
                    DataSourceID = DataSourceId,
                    Owner = objectsList[objectListIndex]
                });

                objectListIndex++;
            }

            long propertyId = MaxPropertyId + BatchItems;

            if (multiProperties)
            {
                objectListIndex = 0;

                while (objectsList.Count > objectListIndex)
                {
                    int propertiesCount = 1;

                    while (propertiesCount < PropertiesCount)
                    {
                        propertiesList.Add(new DBProperty
                        {
                            Id = propertyId,
                            TypeUri = "نام",
                            Value = RandomString(),
                            DataSourceID = DataSourceId,
                            Owner = objectsList[objectListIndex]
                        });

                        propertyId++;
                        propertiesCount++;
                    }

                    objectListIndex++;
                }
            }

            MaxObjectId += BatchItems;
            MaxPropertyId = propertyId;

            DBAddedConcepts dbAddedConcepts = new DBAddedConcepts
            {
                AddedObjectList = objectsList,
                AddedPropertyList = propertiesList,
                AddedMediaList = new List<DBMedia>(),
                AddedRelationshipList = new List<DBRelationship>()
            };

            return dbAddedConcepts;
        }

        public DBAddedConcepts GenerateRelationships(bool multiProperties)
        {
            var objects = GenerateObjectsWithProperty(multiProperties);

            List<DBRelationship> relationshipsList = new List<DBRelationship>();

            for (int id = MaxRelationId; id < MaxRelationId + BatchItems; id++)
            {
                relationshipsList.Add(new DBRelationship
                {
                    Id = id,
                    DataSourceID = DataSourceId,
                    Description = RandomString(),
                    TypeURI = "حضور_در",
                    Source = objects.AddedObjectList[Random.Next(0, objects.AddedObjectList.Count)],
                    Target = objects.AddedObjectList[Random.Next(0, objects.AddedObjectList.Count)],
                    Direction = RepositoryLinkDirection.SourceToTarget
                });
            }

            MaxRelationId += BatchItems;

            objects.AddedRelationshipList = relationshipsList;

            return objects;
        }

        public void ClearAllData()
        {
            OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), BatchCount.ToString()));

            MaxObjectId = 0;
            MaxPropertyId = 0;
            MaxRelationId = 0;

            timer = new Stopwatch();
            timer.Start();
            RepositoryService.TruncateDatabase();
            timer.Stop();

            ClearAllDataTime = timer.Elapsed.ToString(OutPutTimeFormat);

            WriteLog("Clear all data in " + ClearAllDataTime);
        }

        private string RandomString()
        {
            const string charsWithNumbers = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(charsWithNumbers, Random.Next(0, MaxPropertyValuesLength)).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public void RegisterNewDataSourceAcl()
        {
            var acl = new ACL
            {
                Classification = Classification.EntriesTree.First().IdentifierString,
                Permissions = new List<ACI>
                {
                    new ACI
                    {
                        AccessLevel = Permission.Owner,
                        GroupName = "G1"
                    },
                }
            };

            RepositoryService.RegisterNewDataSource(DataSourceId, "Test", DataSourceType.ManuallyEntered,
                acl, "description", "admin", "2019-05-02 14:12:22");
        }

        [TestMethod]
        public void PublishObjectsWithOneProperty()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateObjectsWithProperty(false);
                timer.Start();
                RepositoryService.Publish(data, modified, new DBResolvedObject[] { }, DataSourceId);
                timer.Stop();

            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects with one property in average" +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        [TestMethod]
        public void PublishObjectsWithMultiProperties()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateObjectsWithProperty(true);
                timer.Start();
                RepositoryService.Publish(data, modified, new DBResolvedObject[] { }, DataSourceId);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects with " + PropertiesCount + " properties in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        [TestMethod]
        public void PublishRelations()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateRelationships(false);
                timer.Start();
                RepositoryService.Publish(data, modified, new DBResolvedObject[] { }, DataSourceId);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects and relations with one property in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        [TestMethod]
        public void PublishRelationsWithMultiProperties()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateRelationships(true);
                timer.Start();
                RepositoryService.Publish(data, modified, new DBResolvedObject[] { }, DataSourceId);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects and relations with multi properties in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        [TestMethod]
        public void PublishObjectsWithOnePropertyAndResolve()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateObjectsWithProperty(false);

                long masterObjectId = Random.Next(1, MaxObjectId);

                var resolveObject = new List<DBResolvedObject>
                {
                    new DBResolvedObject
                    {
                        ResolutionMasterObjectID = masterObjectId,
                        ResolvedObjectIDs = Enumerable.Range(1, MaxObjectId).Where(i => masterObjectId != i ).OrderBy(x => Random.Next())
                            .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToArray(),
                        MatchedProperties = new DBMatchedProperty[]{}
                    }
                };

                timer.Start();
                RepositoryService.Publish(data, modified, resolveObject.ToArray(), DataSourceId);
                timer.Stop();

            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects with one property and resolve " + RetrieveItemsCount +
                     " objects in average" + averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        [TestMethod]
        public void PublishObjectsWithMultiPropertiesAndResolve()
        {
            ClearAllData();
            RegisterNewDataSourceAcl();
            timer = new Stopwatch();

            var modified = new DBModifiedConcepts
            {
                DeletedMediaIDList = new List<long>(),
                ModifiedPropertyList = new List<DBModifiedProperty>()
            };

            for (int batch = 1; batch <= BatchCount; batch++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                var data = GenerateObjectsWithProperty(true);

                long masterObjectId = Random.Next(1, MaxObjectId);

                var resolveObject = new List<DBResolvedObject>
                {
                    new DBResolvedObject
                    {
                        ResolutionMasterObjectID = masterObjectId,
                        ResolvedObjectIDs = Enumerable.Range(1, MaxObjectId).Where(i => masterObjectId != i ).OrderBy(x => Random.Next())
                            .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToArray(),
                        MatchedProperties = new DBMatchedProperty[]{}
                    }
                };

                timer.Start();
                RepositoryService.Publish(data, modified, resolveObject.ToArray(), DataSourceId);
                timer.Stop();

            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            PublishDataTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Publish " + BatchCount + " batch of objects with multi properties and resolve " + RetrieveItemsCount +
                     " objects in average" + averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + BatchItems + " objects");
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve,
            CancellationToken token)
        {
            //تعداد متد‌‌های تست در این کلاس
            int testsMethodCount = 6;

            //محاسبه تمام مراحل تست
            string totalStepsCount = (Math.Log10(endStore) * testsMethodCount + testsMethodCount).ToString(CultureInfo.InvariantCulture);

            int currentStepNumber = 0;

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Publish objects",
                    Description = "Publish objects with one property",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish objects",
                    Description = "Publish objects with multi properties",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish objects and relations",
                    Description = "Publish objects and relations with one property",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish objects and relations",
                    Description = "Publish objects and relations with multi properties",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish objects and resolve",
                    Description = "Publish objects with one property and resolve",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish objects and resolve",
                    Description = "Publish objects with multi properties and resolve",
                    Statistics = new DataTable()
                }

            };

            //ایجاد ستون‌های جداول نتیجه تست‌ها
            foreach (var testResult in testResults)
            {
                testResult.Statistics.Columns.Add("Batch count");
                testResult.Statistics.Columns.Add("Publish");
                testResult.Statistics.Columns.Add("Clear all data");
            }

            for (long storeIndex = startStore; storeIndex <= endStore; storeIndex *= 10)
            {
                var rows = new List<DataRow>();

                for (int i = 0; i < testsMethodCount; i++)
                {
                    rows.Add(testResults[i].Statistics.NewRow());
                }

                BatchCount = storeIndex;

                foreach (var row in rows)
                {
                    row["Batch count"] = storeIndex.ToString();
                }

                for (int i = 0; i < testsMethodCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    currentStepNumber++;
                    OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                        testResults[i].Description, testResults));

                    try
                    {
                        switch (i)
                        {
                            case 0:
                                await Task.Run(() => PublishObjectsWithOneProperty());
                                break;
                            case 1:
                                await Task.Run(() => PublishObjectsWithMultiProperties());
                                break;
                            case 2:
                                await Task.Run(() => PublishRelations());
                                break;
                            case 3:
                                await Task.Run(() => PublishRelationsWithMultiProperties());
                                break;
                            case 4:
                                await Task.Run(() => PublishObjectsWithOnePropertyAndResolve());
                                break;
                            case 5:
                                await Task.Run(() => PublishObjectsWithMultiPropertiesAndResolve());
                                break;
                        }

                        rows[i]["Publish"] = PublishDataTime;
                    }
                    catch (Exception exception)
                    {
                        rows[i]["Publish"] = exception.Message;
                    }
                    finally
                    {
                        await Task.Run(() => ClearAllData());
                        rows[i]["Clear all data"] = ClearAllDataTime;

                        testResults[i].Statistics.Rows.Add(rows[i]);
                    }
                }
            }

            return testResults;
        }
    }
}
