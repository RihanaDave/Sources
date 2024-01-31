using GPAS.AccessControl;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using GPAS.FilterSearch;
using GPAS.Utility;
using System.IO;
using System.Diagnostics;
using System;
using System.ComponentModel.Composition;
using GPAS.LoadTest.Core;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Collections.ObjectModel;

namespace GPAS.Search.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class PerformFilterSearchLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.SearchServer;
        public TestClassType TestClassType { get; set; } = TestClassType.PerformFilterSearch;
        public string Title { get; set; } = "PerformFilterSearch";

        public event EventHandler<StepsProgressChangedEventArgs> StepsProgressChanged;
        protected virtual void OnStepsProgressChanged(StepsProgressChangedEventArgs e)
        {
            StepsProgressChanged?.Invoke(this, e);
        }

        public event EventHandler<TestCaseProgressChangedEventArgs> TestCaseProgressChanged;
        protected virtual void OnTestCaseProgressChanged(TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        TimeSpan averageTime = new TimeSpan();
        TimeSpan totalTime = new TimeSpan();

        [TestInitialize]
        public void Init()
        {
            GPAS.SearchServer.Logic.SearchEngineProvider.Init();
            GPAS.SearchServer.Logic.GlobalResolutionCandidatesProvider.Init();

            SetOntology(allProperties.Keys.ToList());
        }

        bool callRunStatisticalQueryFromTestMethod = false;
        [TestMethod]
        public void PerformFilterSearch()
        {
            long retrivedItems = 0;

            try
            {
                #region Assign
                if (!callRunStatisticalQueryFromTestMethod)
                {
                    SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();
                    syncPublishChangesLoadTests.SyncPublishChangesRelationsWithMultiProperties();
                }

                AuthorizationParametters authorizationParametters = new AuthorizationParametters()
                {
                    permittedGroupNames = new List<string>()
                    {
                        currentACL.Permissions[0].GroupName,
                    },
                        readableClassifications = new List<string>()
                    {
                        currentACL.Classification,
                    },
                };

                totalTime = new TimeSpan();
                averageTime = new TimeSpan();
                Stopwatch Watch = new Stopwatch();
                #endregion

                #region Act
                using (ShimsContext.Create())
                {
                    SearchServer.Fakes.ShimOntologyProvider.GetOntology = () =>
                    {
                        return ontology;
                    };

                    for (int i = 0; i < QueryCount; i++)
                    {
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(QueryCount.ToString(), i.ToString()));

                        Query filterSearchQuery = new Query()
                        {
                            CriteriasSet = new CriteriaSet()
                            {
                                Criterias = new ObservableCollection<CriteriaBase>(),
                            },
                        };

                        filterSearchQuery.CriteriasSet.SetOperator = (Random.NextDouble() > .5) ? BooleanOperator.All : BooleanOperator.Any;

                        int repeat = Random.Next(1, 4);
                        for (int j = 0; j < repeat; j++)
                        {
                            CriteriaBase criteriaBase = null;//CriteriaBase Class has 5 child
                            double x = RandomDouble(0, 3);
                            if (x < 1)
                            {
                                criteriaBase = GenerateObjectTypeCriteria();
                            }
                            else if (x < 2)
                            {
                                criteriaBase = GenerateKeywordCriteria();
                            }
                            else if (x < 3)
                            {
                                criteriaBase = GenerateDateRangeCriteria();
                            }
                            else if (x < 4)
                            {
                                //PropertyValueCriteria
                            }
                            else
                            {
                                //NastedCriteria
                            }

                            filterSearchQuery.CriteriasSet.Criterias.Add(criteriaBase);
                        }

                        QuerySerializer serializer = new QuerySerializer();
                        MemoryStream streamWriter = new MemoryStream();
                        serializer.Serialize(streamWriter, filterSearchQuery);
                        StreamUtility streamUtil = new StreamUtility();
                        byte[] filterSearchQueryByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);

                        Watch.Start();
                        List<SearchServer.Entities.SearchObject> result = serviceClient.PerformFilterSearch(filterSearchQueryByteArray, (int)BatchItems, authorizationParametters);
                        Watch.Stop();
                        totalTime += Watch.Elapsed;
                        Watch.Reset();

                        if (result != null)
                            retrivedItems += result.Count;
                    }
                }
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalTime.Ticks / QueryCount);
                AverageRetriveTimeString = averageTime.ToString(AverageTimeFormat);
                TotalRetriveTimeString = totalTime.ToString(TotalTimeFormat);

                WriteLog("Search result " + retrivedItems + " Items in " + QueryCount + " request with different query.\n" +
                    "Average Time = " + AverageRetriveTimeString + "\n" +
                    "Total Time = " + TotalRetriveTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AverageRetriveTimeString = ex.Message;
                TotalRetriveTimeString = ex.Message;

                WriteLog("Search result " + retrivedItems + " Items in " + QueryCount + " request with different query.\n" +
                    "Average Time = " + AverageRetriveTimeString + "\n" +
                    "Total Time = " + TotalRetriveTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void Test()
        {
            DateTime startRunTestsTime = DateTime.Now;
            var res = RunTests(startStore, endStore, startRetrieve, endRetrieve, new CancellationToken()).GetAwaiter().GetResult();
            ExportTestCSV(res, startRunTestsTime);
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve, CancellationToken tokenSource)
        {
            callRunStatisticalQueryFromTestMethod = true;
            BatchCount = endRetrieve;
            SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();

            var queryResult = syncPublishChangesLoadTests.GetStatistical();
            long queryObjectCount = 0;
            foreach (var item in queryResult.ObjectTypePreview)
            {
                queryObjectCount += item.Frequency;
            }

            if (queryObjectCount < endRetrieve)
                syncPublishChangesLoadTests.SyncPublishChangesRelationsWithMultiProperties();

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Perform Filter Search",
                    Description = "Perform Filter Search with different query.",
                    AveragePublishTimeString = AveragePublishTimeString,
                    TotalPublishTimeString = TotalPublishTimeString,
                    Statistics = new DataTable(),
                },
            };

            //ایجاد ستون‌های جداول نتیجه تست‌ها
            foreach (var testResult in testResults)
            {
                testResult.Statistics.Columns.Add("Query count");
                testResult.Statistics.Columns.Add("Average Retrieve Time");
                testResult.Statistics.Columns.Add("Total Retrieve Time");
            }

            for (long retrieveIndex = startRetrieve; retrieveIndex <= endRetrieve; retrieveIndex *= 10)
            {
                var rows = new List<DataRow>();

                for (int i = 0; i < testResults.Count; i++)
                {
                    rows.Add(testResults[i].Statistics.NewRow());
                }

                QueryCount = retrieveIndex;

                foreach (var row in rows)
                {
                    row["Query count"] = retrieveIndex.ToString();
                }

                for (int i = 0; i < testResults.Count; i++)
                {
                    //بررسی اینکه آیا درخواست لغو شده است یا نه
                    tokenSource.ThrowIfCancellationRequested();

                    var tr = testResults[i];
                    OnStepsProgressChanged(new StepsProgressChangedEventArgs(testResults.Count.ToString(), (i + 1).ToString(), tr.Title, testResults));

                    try
                    {
                        switch (i)
                        {
                            case 0:
                                await Task.Run(() => PerformFilterSearch());
                                break;
                            default:
                                break;
                        }

                        rows[i]["Average Retrieve Time"] = AverageRetriveTimeString;
                        rows[i]["Total Retrieve Time"] = TotalRetriveTimeString;
                    }
                    catch (Exception ex)
                    {
                        rows[i]["Average Retrieve Time"] = ex.Message;
                        rows[i]["Total Retrieve Time"] = ex.Message;
                    }

                    testResults[i].Statistics.Rows.Add(rows[i]);
                }
            }

            //await Task.Run(() => ClearAllPublishedData());

            foreach (var testResult in testResults)
            {
                testResult.ClearAllDataTimeString = ClearAllDataTimeString;
            }

            callRunStatisticalQueryFromTestMethod = false;

            return testResults;
        }
    }
}
