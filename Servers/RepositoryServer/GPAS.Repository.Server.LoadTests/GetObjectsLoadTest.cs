using GPAS.LoadTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPAS.Repository.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class GetObjectsLoadTest : BaseLoadTest, ILoadTestCategory
    {
        private Stopwatch timer;

        public ServerType ServerType { get; set; } = ServerType.DataRepository;
        public TestClassType TestClassType { get; set; } = TestClassType.ObjectsRetrieve;
        public string Title { get; set; } = "Retrieve Objects";

        [ClassInitialize]
        public static void PublishObjectsWithOnePropertyBeforeTests(TestContext testContext)
        {
            GetObjectsLoadTest getObjectsLoadTest = new GetObjectsLoadTest();
            getObjectsLoadTest.Publish();
        }

        private void Publish()
        {
            ExtensionFileToSave = DateTime.Now.ToString("HH-mm-ss") + ".txt";
            WriteLog(" ******************  Retrieve objects load tests  ****************** ");

            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.TestCaseProgressChanged += PublishLoadTestsOnTestCaseProgressChanged;
            publishLoadTests.PublishObjectsWithOneProperty();
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
            GetObjectsLoadTest getObjectsLoadTest = new GetObjectsLoadTest();
            getObjectsLoadTest.ClearAllData();
        }

        private void ClearAllData()
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.ClearAllData();
        }

        [TestMethod]
        public void GetListOfObjects()
        {
            timer = new Stopwatch();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                List<long> listOfObjectsIds = Enumerable.Range(1, MaxObjectId).OrderBy(x => Random.Next())
                    .Take(RetrieveItemsCount).Select(Convert.ToInt64).ToList();

                timer.Start();
                var result = RepositoryService.GetObjects(listOfObjectsIds);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + RetrieveItemsCount + " objects by id " + BatchCount + " times in average " +
                     averageTime.ToString(OutPutTimeFormat));
        }

        [TestMethod]
        public void GetMultiRangesOfRandomListOfObjects()
        {
            timer = new Stopwatch();

            for (int i = 0; i < BatchCount; i++)
            {
                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), i.ToString()));

                long firstId = Random.Next(1, MaxObjectId - RetrieveItemsCount);
                long lastId = firstId + RetrieveItemsCount - 1;

                timer.Start();
                var result = RepositoryService.RetrieveObjectsSequentialByIDRange(firstId, lastId);
                timer.Stop();
            }

            var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

            TestTime = averageTime.ToString(OutPutTimeFormat);

            WriteLog("Retrieve " + RetrieveItemsCount + " range of objects " + BatchCount + " times in average " +
                     averageTime.ToString(OutPutTimeFormat));
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve,
            CancellationToken token)
        {
            //تعداد متد‌‌های تست در این کلاس
            int testsMethodCount = 2;

            //محاسبه تمام مراحل تست
            string totalStepsCount = (Math.Log10(endStore) * Math.Log10(endRetrieve) * testsMethodCount + Math.Log10(endStore) +
                                      Math.Log10(endStore)).ToString(CultureInfo.InvariantCulture);

            //شماره مرحله جاری
            int currentStepNumber = 0;

            //به تعداد متد‌های تست در کلاس جاری باید از 
            //نوع LoadTestResult ایجاد شود

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Retrieve list of objects",
                    Description = "Retrieve list of objects by random ids",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Retrieve range of objects",
                    Description = "Retrieve multi range of objects by random range id",
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

                //بررسی اینکه آیا درخواست لغو شده است یا نه
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
                for (int i = 0; i < testsMethodCount; i++)
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
                        OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount,
                            currentStepNumber.ToString(), testResults[i].Title, testResults));

                        try
                        {
                            switch (i)
                            {
                                case 0:
                                    await Task.Run(() => GetListOfObjects());
                                    break;
                                case 1:
                                    await Task.Run(() => GetMultiRangesOfRandomListOfObjects());
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
