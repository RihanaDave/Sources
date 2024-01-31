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
    /// Summary description for GetPropertiesLoadTest
    /// </summary>
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class GetPropertiesLoadTest : BaseLoadTest, ILoadTestCategory
    {
        private Stopwatch timer;

        public ServerType ServerType { get; set; } = ServerType.DataRepository;
        public TestClassType TestClassType { get; set; } = TestClassType.PropertiesRetrieve;
        public string Title { get; set; } = "Retrieve Properties";

        [ClassInitialize]
        public static void PublishObjectsWithMultiPropertiesBeforeTests(TestContext testContext)
        {
            GetPropertiesLoadTest getPropertiesLoadTest = new GetPropertiesLoadTest();
            getPropertiesLoadTest.Publish();
        }

        private void Publish()
        {
            ExtensionFileToSave = DateTime.Now.ToString("HH-mm-ss") + ".txt";
            WriteLog(" ******************  Retrieve properties load tests  ****************** ");

            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.TestCaseProgressChanged += PublishLoadTestsOnTestCaseProgressChanged;
            publishLoadTests.PublishObjectsWithMultiProperties();
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
            GetPropertiesLoadTest getPropertiesLoadTest = new GetPropertiesLoadTest();
            getPropertiesLoadTest.ClearAllData();
        }

        private void ClearAllData()
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.ClearAllData();
        }

        [TestMethod]
        public void GetPropertiesOfObjectsWithoutAuthorization()
        {
            timer = new Stopwatch();
            var result = new List<DBProperty>();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfObjectsIds = Enumerable.Range(1, MaxObjectId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetPropertiesOfObjectsWithoutAuthorization(listOfObjectsIds.ToArray());
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of properties by objects id in average " +
                     averageTime.ToString(OutPutTimeFormat));

        }

        [TestMethod]
        public void GetPropertiesOfObjects()
        {
            timer = new Stopwatch();
            var result = new List<DBProperty>();

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
                result = RepositoryService.GetPropertiesOfObjects(listOfObjectsIds.ToArray(), authorization);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of properties by objects id and authorization in average " +
                     averageTime.ToString(OutPutTimeFormat));
        }

        [TestMethod]
        public void GetPropertiesById()
        {
            timer = new Stopwatch();
            var result = new List<DBProperty>();

            var authorization = new AuthorizationParametters
            {
                permittedGroupNames = new List<string> { "G1" },
                readableClassifications = new List<string> { Classification.EntriesTree.First().IdentifierString }
            };

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfPropertiesIds = Enumerable.Range(1, (int)MaxPropertyId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                result = RepositoryService.GetPropertiesByID(listOfPropertiesIds, authorization);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + BatchCount + " batch of properties by objects id and authorization in average " +
                     averageTime.ToString(OutPutTimeFormat));
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve,
            CancellationToken token)
        {
            //تعداد متد‌‌های تست در این کلاس
            int testsMethodCount = 3;

            //محاسبه تمام مراحل تست
            string totalStepsCount = (Math.Log10(endStore) * Math.Log10(endRetrieve) * testsMethodCount + Math.Log10(endStore) +
                                      Math.Log10(endStore)).ToString(CultureInfo.InvariantCulture);

            //شماره مرحله جاری
            int currentStepNumber = 0;

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Retrieve properties with object id",
                    Description = "Retrieve properties of objects by random ids",
                    Statistics = new DataTable()
                },new LoadTestResult
                {
                    Title = "Retrieve properties with authorization",
                    Description = "Retrieve properties of objects by random ids and authorization",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve properties by id",
                    Description = "Retrieve properties by random ids",
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

            //حلقه مرحله اول برای ساخت و انتشار مفاهیم می‌باشد
            //حلقه مرحله دوم برای بازیابی و اجرای تک تک تست ها می‌باشد

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
                    "Publish objects with multi properties", testResults));

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
                for (int i = 0; i < testsMethodCount; i++)
                {
                    testResults[i].Statistics.Rows.Add(rows[i]);
                }

                for (long retrieveIndex = startRetrieve; retrieveIndex <= endRetrieve; retrieveIndex *= 10)
                {
                    BatchCount = retrieveIndex;

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
                                    await Task.Run(() => GetPropertiesOfObjectsWithoutAuthorization());
                                    break;
                                case 1:
                                    await Task.Run(() => GetPropertiesOfObjects());
                                    break;
                                case 2:
                                    await Task.Run(() => GetPropertiesById());
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
