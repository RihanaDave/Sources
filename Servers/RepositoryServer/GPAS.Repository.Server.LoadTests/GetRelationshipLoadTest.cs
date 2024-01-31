using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.LoadTest.Core;
using GPAS.RepositoryServer.Entities;

namespace GPAS.Repository.Server.LoadTests
{
    /// <summary>
    /// Summary description for GetRelationshipLoadTest
    /// </summary>
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class GetRelationshipLoadTest : BaseLoadTest, ILoadTestCategory
    {
        private Stopwatch timer;

        public ServerType ServerType { get; set; } = ServerType.DataRepository;
        public TestClassType TestClassType { get; set; } = TestClassType.RelationsRetrieve;
        public string Title { get; set; } = "Retrieve Relationship";

        [ClassInitialize]
        public static void PublishRelationsBeforeTests(TestContext testContext)
        {
            GetRelationshipLoadTest relationshipLoadTest = new GetRelationshipLoadTest();
            relationshipLoadTest.Publish();
        }

        private void Publish()
        {
            ExtensionFileToSave = DateTime.Now.ToString("yyyyMMdd-HH-mm-ss") + ".txt";
            WriteLog(" ******************  Retrieve relationship load tests  ****************** ");

            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.TestCaseProgressChanged += PublishLoadTestsOnTestCaseProgressChanged;
            publishLoadTests.PublishRelations();
        }

        private void PublishLoadTestsOnTestCaseProgressChanged(object sender, TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        public event EventHandler<StepsProgressChangedEventArgs> StepsProgressChanged;
        public event EventHandler<TestCaseProgressChangedEventArgs> TestCaseProgressChanged;

        protected virtual void OnStepsProgressChanged(StepsProgressChangedEventArgs e)
        {
            StepsProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnTestCaseProgressChanged(TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        [ClassCleanup]
        public static void ClearAllDataAfterRunAllTests()
        {
            GetRelationshipLoadTest relationshipLoadTest = new GetRelationshipLoadTest();
            relationshipLoadTest.ClearAllData();
        }

        private void ClearAllData()
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.ClearAllData();
        }

        [TestMethod]
        public void GetRelationshipsByIdWithoutAuthParams()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfRelationsIds = Enumerable.Range(1, MaxRelationId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.RetrieveRelationships(listOfRelationsIds);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by relation id in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        [TestMethod]
        public void GetRelationshipsById()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            var authorization = new AuthorizationParametters
            {
                permittedGroupNames = new List<string> { "G1" },
                readableClassifications = new List<string> { Classification.EntriesTree.First().IdentifierString }
            };

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfRelationsIds = Enumerable.Range(1, MaxRelationId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetRelationships(listOfRelationsIds, authorization);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by relation id and authorization in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        [TestMethod]
        public void GetRelationshipsBySourceObjectWithoutAuthParams()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfObjectsIds = Enumerable.Range(1, MaxObjectId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetRelationshipsBySourceObjectWithoutAuthParams(listOfObjectsIds);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by source object in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        [TestMethod]
        public void GetRelationshipsBySourceObject()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            var authorization = new AuthorizationParametters
            {
                permittedGroupNames = new List<string> { "G1" },
                readableClassifications = new List<string> { Classification.EntriesTree.First().IdentifierString }
            };

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                var objectId = Random.Next(0, MaxObjectId);

                timer.Start();
                result = RepositoryService.GetRelationshipsBySourceObject(objectId, "حضور_در", authorization);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by source object and authorization in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        [TestMethod]
        public void GetRelationshipsBySourceOrTargetObjectWithoutAuthParams()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfObjectsIds = Enumerable.Range(1, MaxObjectId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(listOfObjectsIds);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by source or target object in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        [TestMethod]
        public void GetRelationshipsBySourceOrTargetObject()
        {
            timer = new Stopwatch();
            var result = new List<DBRelationship>();

            var authorization = new AuthorizationParametters
            {
                permittedGroupNames = new List<string> { "G1" },
                readableClassifications = new List<string> { Classification.EntriesTree.First().IdentifierString }
            };

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfObjectsIds = Enumerable.Range(1, MaxObjectId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetRelationshipsBySourceOrTargetObject(listOfObjectsIds, authorization);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of relations by source or target objects and authorization in average " +
                     averageTime.ToString(OutPutTimeFormat) + " that each batch contain " + RetrieveItemsCount + " relations");
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve,
            CancellationToken token)
        {
            //تعداد متد‌‌های تست در این کلاس
            int testsMethodCount = 6;

            //محاسبه تمام مراحل تست
            //عدد 6 تعداد متد‌‌های تست می‌باشد
            string totalStepsCount = (Math.Log10(endStore) * Math.Log10(endRetrieve) * testsMethodCount + Math.Log10(endStore) +
                                      Math.Log10(endStore)).ToString(CultureInfo.InvariantCulture);

            //شماره مرحله جاری
            int currentStepNumber = 0;

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Retrieve relationships",
                    Description = "Retrieve list of relationships by random relations id",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve relationships with authorization",
                    Description = "Retrieve list of relationships by random relation id with authorization",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve relationships by source id",
                    Description = "Retrieve list of relationships by random source id",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve relationships by source id",
                    Description = "Retrieve list of relationships by random source id with authorization ",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve Relationships by source or target id",
                    Description = "Retrieve list of relationships by random source or target id",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve Relationships by source or target id with authorization",
                    Description = "Retrieve List of relationships by random source or target id with authorization ",
                    Statistics = new DataTable()
                }
            };

            //ایجاد ستون برای جدول‌های نتایج
            foreach (var testResult in testResults)
            {
                testResult.Statistics.Columns.Add("Batch count");
                testResult.Statistics.Columns.Add("Publish");

                for (long i = startRetrieve; i <= endRetrieve; i *= 10)
                {
                    testResult.Statistics.Columns.Add(i.ToString());
                }

                testResult.Statistics.Columns.Add("Clear all data");
            }

            int rowCount = 0;

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

                token.ThrowIfCancellationRequested();
                currentStepNumber++;
                OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                    "Publish objects with one property", testResults));

                try
                {
                    await Task.Run(() => Publish());

                    foreach (var row in rows)
                    {
                        row["Publish"] = PublishDataTime;
                    }
                }
                catch (Exception exception)
                {
                    foreach (var row in rows)
                    {
                        row["Publish"] = exception.Message;
                    }
                }

                //افزودن سطر‌ها به جدول‌ها
                for (int i = 0; i <= 5; i++)
                {
                    testResults[i].Statistics.Rows.Add(rows[i]);
                }

                for (long retrieveIndex = startRetrieve; retrieveIndex <= endRetrieve; retrieveIndex *= 10)
                {
                    BatchCount = retrieveIndex;

                    //اجرای تک تک تست ها به ترتیب
                    for (int i = 0; i < testsMethodCount; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        currentStepNumber++;
                        OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                             testResults[i].Title, testResults));

                        try
                        {
                            switch (i)
                            {
                                case 0:
                                    await Task.Run(() => GetRelationshipsByIdWithoutAuthParams());
                                    break;
                                case 1:
                                    await Task.Run(() => GetRelationshipsById());
                                    break;
                                case 2:
                                    await Task.Run(() => GetRelationshipsBySourceObjectWithoutAuthParams());
                                    break;
                                case 3:
                                    await Task.Run(() => GetRelationshipsBySourceObject());
                                    break;
                                case 4:
                                    await Task.Run(() => GetRelationshipsBySourceOrTargetObjectWithoutAuthParams());
                                    break;
                                case 5:
                                    await Task.Run(() => GetRelationshipsBySourceOrTargetObject());
                                    break;
                            }

                            testResults[i].Statistics.Rows[rowCount][retrieveIndex.ToString()] = TestTime;
                        }
                        catch (Exception exception)
                        {
                            testResults[i].Statistics.Rows[rowCount][retrieveIndex.ToString()] = exception.Message;
                        }
                    }
                }

                currentStepNumber++;
                OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                    "Clear all data", testResults));

                try
                {
                    await Task.Run(() => ClearAllData());

                    foreach (var testResult in testResults)
                    {
                        testResult.Statistics.Rows[rowCount]["Clear all data"] = ClearAllDataTime;
                    }
                }
                catch (Exception exception)
                {
                    foreach (var testResult in testResults)
                    {
                        testResult.Statistics.Rows[rowCount]["Clear all data"] = exception.Message;
                    }
                }

                rowCount++;
            }

            return testResults;
        }
    }
}
