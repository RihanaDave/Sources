using GPAS.LoadTest.Core;
using GPAS.LoadTest.Presenter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace GPAS.LoadTest.Presenter.ViewModel
{
    public class MainViewModel
    {
        [ImportMany]
#pragma warning disable CS0649 // Field 'MainViewModel.loadTests' is never assigned to, and will always have its default value null
        private IEnumerable<ILoadTestCategory> loadTests;
#pragma warning restore CS0649 // Field 'MainViewModel.loadTests' is never assigned to, and will always have its default value null

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public MainViewModel()
        {
            //مقدار‌دهی اولیه متغییر‌ها
            ProgressBarInfo = new ProgressBarModel();
            TestsInfo = new RunTestsInfoModel();
            TestsResultToShow = new ObservableCollection<LoadTestResultModel>();
            SidebarItems = new ObservableCollection<SidebarMenuModel>();
            AvailableLoadTests = new ObservableCollection<AvailableLoadTestsModel>();

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Adds all the parts found in the same assembly as the Program class
            //   catalog.Catalogs.Add(new AssemblyCatalog(typeof(App).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Repository.Server.LoadTests.BaseLoadTest).Assembly));
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(FileRepository.Tests.ServiceTests).Assembly));
            //  catalog.Catalogs.Add(new AssemblyCatalog(typeof(Horizon.Server.LoadTests.BaseLoadTest).Assembly));
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(Search.Server.LoadTests.BaseLoadTest).Assembly));

            //Create the CompositionContainer with the parts in the catalog
            var container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }

            //مدیریت رویداد‌های صادر شده از سمت تست‌ها
            foreach (ILoadTestCategory test in loadTests)
            {
                test.StepsProgressChanged += ChangedStepsProgress;
                test.TestCaseProgressChanged += ChangedTestCaseProgress;
            }

            //پر کردن لیست سرور‌ها در سمت چپ پنجره
            LoadSidebarItems();
        }

        private string startRunTestsTime;
        public CancellationTokenSource TokenSource;
        public ProgressBarModel ProgressBarInfo { get; set; }
        public RunTestsInfoModel TestsInfo { get; set; }
        public ObservableCollection<LoadTestResultModel> TestsResultToShow { get; set; }
        public ObservableCollection<SidebarMenuModel> SidebarItems { get; set; }
        public ObservableCollection<AvailableLoadTestsModel> AvailableLoadTests { get; set; }

        private void ChangedTestCaseProgress(object sender, TestCaseProgressChangedEventArgs e)
        {
            ProgressBarInfo.CurrentStepNumber = e.CurrentBatch + " from " + e.BatchCount;
            ProgressBarInfo.CurrentStepMaximum = double.Parse(e.BatchCount);
            ProgressBarInfo.CurrentStepValue = double.Parse(e.CurrentBatch);
        }

        private void ChangedStepsProgress(object sender, StepsProgressChangedEventArgs e)
        {
            ProgressBarInfo.StepsProgressTitle = e.StepName;
            ProgressBarInfo.StepsProgressNumber = e.CurrentStepNumber + " from " + e.TotalStepsCount;
            ProgressBarInfo.StepsProgressMaximum = double.Parse(e.TotalStepsCount);
            ProgressBarInfo.StepsProgressValue = double.Parse(e.CurrentStepNumber);

            if (TestsResultToShow.Count != 0)
                TestsResultToShow.Clear();

            foreach (var result in e.StepResult)
            {
                TestsResultToShow.Add(new LoadTestResultModel
                {
                    TestTitle = result.Title,
                    ResultTest = result.Statistics.AsDataView()
                });
            }

            WriteLog(e.StepResult);
        }

        /// <summary>
        /// افزودن سرور‌های شناخته شده به لیست سرور‌ها در سمت چپ پنجره
        /// </summary>
        private void LoadSidebarItems()
        {
            var testsType = loadTests.GroupBy(i => new { i.ServerType }).Select(x => x.First()).ToList();

            foreach (var test in testsType)
            {
                switch (test.ServerType)
                {
                    case ServerType.DataRepository:
                        SidebarItems.Add(new SidebarMenuModel
                        {
                            Title = "Data Repository",
                            Icon = PackIconKind.Database,
                            Tag = ServerType.DataRepository
                        });
                        break;
                    case ServerType.HorizonServer:
                        SidebarItems.Add(new SidebarMenuModel
                        {
                            Title = "Horizon Server",
                            Icon = PackIconKind.Graphql,
                            Tag = ServerType.HorizonServer
                        });
                        break;
                    case ServerType.SearchServer:
                        SidebarItems.Add(new SidebarMenuModel
                        {
                            Title = "Search Server",
                            Icon = PackIconKind.DatabaseSearch,
                            Tag = ServerType.SearchServer
                        });
                        break;
                    case ServerType.FileRepository:
                        SidebarItems.Add(new SidebarMenuModel
                        {
                            Title = "File Repository",
                            Icon = PackIconKind.FileSearch,
                            Tag = ServerType.FileRepository
                        });
                        break;
                }
            }
        }

        /// <summary>
        /// پر کردن لیست آبشاری تست‌ها بر اساس سرور انتخاب شده
        /// </summary>
        /// <param name="selectedServer">سرور انتخاب شده</param>
        public void FillAvailableLoadTests(object selectedServer)
        {
            if (AvailableLoadTests.Count != 0)
                AvailableLoadTests.Clear();

            foreach (var test in loadTests)
            {
                if (test.ServerType.Equals(selectedServer))
                {
                    AvailableLoadTests.Add(new AvailableLoadTestsModel
                    {
                        LoadTestName = test.Title,
                        Tag = test.TestClassType
                    });
                }
            }
        }

        /// <summary>
        /// فراخوانی عملیات اجرای تست‌ها
        /// </summary>
        /// <param name="selectedTest">کلاس تست انتخاب شده</param>
        /// <returns></returns>
        public async Task RunTests(object selectedTest)
        {
            if (TokenSource == null)
            {
                //برای متوقف کردن اجرا استفاده می‌شود
                TokenSource = new CancellationTokenSource();

                var results = new List<LoadTestResult>();
                try
                {
                    startRunTestsTime = DateTime.Now.ToString("yyyyMMddHH-mm-ss") + ".txt";

                    foreach (ILoadTestCategory test in loadTests)
                    {
                        if (test.TestClassType.Equals(((AvailableLoadTestsModel)selectedTest).Tag))
                        {
                            results = await test.RunTests(TestsInfo.PublishStart, TestsInfo.PublishEnd, TestsInfo.RetrieveStart,
                                TestsInfo.RetrieveEnd, TokenSource.Token);

                            break;
                        }
                    }

                    if (TestsResultToShow.Count != 0)
                        TestsResultToShow.Clear();

                    foreach (var result in results)
                    {
                        TestsResultToShow.Add(new LoadTestResultModel
                        {
                            TestTitle = result.Title,
                            ResultTest = result.Statistics.AsDataView()
                        });
                    }

                    WriteLog(results);
                }
                catch (OperationCanceledException)
                {
                    WriteLog(results);
                }
                finally
                {
                    TokenSource = null;
                }
            }
            else
            {
                TokenSource.Cancel();
                TokenSource = null;
            }

        }

        /// <summary>
        /// ساخت فایل و نوشتن نتایج تست‌ها در قالب
        /// csv
        /// </summary>
        /// <param name="results">نتایحج تست‌ها</param>
        private void WriteLog(List<LoadTestResult> results)
        {
            try
            {
                string resultFolderPath = Properties.Settings.Default.Setting_LogFolder;

                if (!Directory.Exists(resultFolderPath))
                {
                    Directory.CreateDirectory(resultFolderPath);
                }

                foreach (var result in results)
                {
                    using (StreamWriter streamWriter = File.CreateText(Path.Combine(resultFolderPath, result.Title + startRunTestsTime)))
                    {
                        streamWriter.WriteLine(" ******** Test name :" + result.Title + " ******** ");
                        streamWriter.WriteLine(" Description :" + result.Description);
                        streamWriter.WriteLine("");

                        StringBuilder stringBuilder = new StringBuilder();

                        IEnumerable<string> columnNames = result.Statistics.Columns.Cast<DataColumn>().
                            Select(column => column.ColumnName);
                        stringBuilder.AppendLine(string.Join(",", columnNames));

                        foreach (DataRow row in result.Statistics.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                            stringBuilder.AppendLine(string.Join(",", fields));
                        }

                        streamWriter.WriteLine(stringBuilder.ToString());
                        streamWriter.WriteLine("");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
