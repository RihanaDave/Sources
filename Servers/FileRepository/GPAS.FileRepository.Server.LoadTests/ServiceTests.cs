using GPAS.LoadTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPAS.FileRepository.Tests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class ServiceTests : ILoadTestCategory
    {
        private readonly Random random = new Random();
        private Stopwatch timer;
        string CreatedString = string.Empty;
        public long BatchCount = 100;
        private long MaxFileId = 0;
        byte[] DownloadDocument;
        Service service = new Service();
        public string totalTimeUpload = "";
        public string totalTimeDownload = "";
        public string totalTimeClearAll = "";

        public ServerType ServerType { get; set; } = ServerType.FileRepository;
        public TestClassType TestClassType { get; set; } = TestClassType.UploadAndDownload;
        public string Title { get; set; } = "Upload and download file";

        [ClassInitialize]
        public static void CallUploadFileTest(TestContext testContext)
        {
            ServiceTests serviceTests = new ServiceTests();
            serviceTests.DeleteDocumentFileTest();
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

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve,
            long endRetrieve, CancellationToken token)
        {
            //محاسبه تمام مراحل تست
            string totalStepsCount = (Math.Log10(endStore) * Math.Log10(endRetrieve) + Math.Log10(endStore) +
                                      Math.Log10(endStore)).ToString(CultureInfo.InvariantCulture);
            //شماره مرحله جاری
            int currentStepNumber = 0;

            //به تعداد متد‌های تست در کلاس جاری باید از 
            //نوع LoadTestResult ایجاد شود
            var t1 = new LoadTestResult
            {
                Title = "Retrieve list of FileDirectory",
                Description = "Retrieve list of FileDirectory by random ids",
                Statistics = new DataTable()
            };

            //ایجاد ستون برای جدول‌های نتایج
            t1.Statistics.Columns.Add("Batch count");
            t1.Statistics.Columns.Add("Upload");

            for (long i = startRetrieve; i <= endRetrieve; i *= 10)
            {
                t1.Statistics.Columns.Add(i.ToString());
            }

            t1.Statistics.Columns.Add("Remove all Files");

            await Task.Run(() => DeleteDocumentFileTest());

            //حلقه مرحله اول برای ساخت و انتشار مفاهیم می‌باشد
            //حلقه مرحله دوم برای بازیابی و اجرای تک تک تست ها می‌باشد

            int rowCount = 0;

            for (long storeIndex = startStore; storeIndex <= endStore; storeIndex *= 10)
            {
                var row1 = t1.Statistics.NewRow();

                BatchCount = storeIndex;

                row1["Batch count"] = storeIndex.ToString();

                token.ThrowIfCancellationRequested();
                currentStepNumber++;
                OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                    "Upload file ", new List<LoadTestResult> { t1 }));
                await Task.Run(() => UploadDocumentFileTest());
                row1["Upload"] = totalTimeUpload;

                t1.Statistics.Rows.Add(row1);

                for (long retrieveIndex = startRetrieve; retrieveIndex <= endRetrieve; retrieveIndex *= 10)
                {
                    BatchCount = retrieveIndex;

                    token.ThrowIfCancellationRequested();
                    currentStepNumber++;
                    OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                        "Download files", new List<LoadTestResult> { t1 }));
                    await Task.Run(() => DownloadDocumentFileTest());
                    t1.Statistics.Rows[rowCount][retrieveIndex.ToString()] = totalTimeDownload;

                }

                currentStepNumber++;
                OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                    "Remove all Files", new List<LoadTestResult> { t1 }));
                await Task.Run(() => DeleteDocumentFileTest());
                t1.Statistics.Rows[rowCount]["Remove all Files"] = totalTimeClearAll;
                rowCount++;
            }
            return new List<LoadTestResult> { t1 };
        }

        [TestMethod]
        public void UploadDocumentFileTest()
        {
            try
            {
                timer = new Stopwatch();

                for (long j = 0; j <= BatchCount; j++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), j.ToString()));

                    byte[] array = Encoding.ASCII.GetBytes(CreateRandomString());

                    timer.Start();
                    service.UploadDocumentFile(j, array);
                    timer.Stop();

                    MaxFileId++;
                }

                totalTimeUpload = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount).ToString(@"hh\:mm\:ss\.ffffff");
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        public string CreateRandomString()
        {
            try
            {
                string charsWithNumbers = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
                CreatedString = new string(Enumerable.Repeat(charsWithNumbers, random.Next(0, 10000)).Select(s => s[random.Next(s.Length)]).ToArray());
                return CreatedString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [TestMethod]
        public void DeleteDocumentFileTest()
        {
            timer = new Stopwatch();

            timer.Start();
            service.RemoveAllFiles();
            timer.Stop();

            totalTimeClearAll = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds).ToString(@"hh\:mm\:ss\.ffffff");
        }

        [TestMethod]
        public void DownloadDocumentFileTest()
        {
            try
            {
                timer = new Stopwatch();
                for (int j = 0; j <= BatchCount; j++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), j.ToString()));

                    timer.Start();
                    DownloadDocument = service.DownloadDocumentFile(random.Next(0, int.Parse(MaxFileId.ToString())));
                    timer.Stop();
                }

                totalTimeDownload = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount).ToString(@"hh\:mm\:ss\.ffffff");
            }
            catch (Exception)
            {
                // Do nothing
            }
        }
    }
}
