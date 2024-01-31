using GPAS.AccessControl;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ObjectSet;
using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.SetAlgebra;
using GPAS.Utility;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using System.ComponentModel.Composition;
using GPAS.LoadTest.Core;
using System.Threading;
using System.Globalization;
using System.Data;

namespace GPAS.Search.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class RunStatisticalQueryLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.SearchServer;
        public TestClassType TestClassType { get; set; } = TestClassType.RunStatisticalQuery;
        public string Title { get; set; } = "RunStatisticalQuery";

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
        public void RunStatisticalQuery()
        {
            try
            {
                #region Assign
                if (!callRunStatisticalQueryFromTestMethod)
                {
                    SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();
                    syncPublishChangesLoadTests.SyncPublishChangesRelationsWithMultiProperties();
                }

                List<Query> queryList = new List<Query>();
                for (int i = 0; i < QueryCount; i++)
                {
                    Query query = new Query()
                    {
                        SourceSet = new StartingObjectSet(),
                        FormulaSequence = new List<FormulaStep>()
                    };

                    int repeat = Random.Next(1, 2);
                    for (int j = 0; j < repeat; j++)
                    {
                        FormulaStep formulaStep = null; //FormulaStep Class has 5 child
                        double x = RandomDouble(0, 4);
                        if (x < 1)
                        {
                            formulaStep = GenerateTypeBasedDrillDown();
                        }
                        else if (x < 2)
                        {
                            formulaStep = GeneratePropertyValueRangeDrillDown();
                        }
                        else if (x < 3)
                        {
                            formulaStep = GenerateLinkBasedDrillDown();
                        }
                        else if (x < 4)
                        {
                            formulaStep = GeneratePropertyValueBasedDrillDown();
                        }
                        else
                        {
                            //PerformSetOperation
                        }

                        query.FormulaSequence.Add(formulaStep);
                    }

                    queryList.Add(query);
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

                QueryResult qr = null;
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

                    GPAS.Ontology.Fakes.ShimOntology.AllInstances.GetAllOntologyRelationships = (o) =>
                    {
                        return new List<string>();
                    };

                    GPAS.Ontology.Fakes.ShimOntology.AllInstances.GetAllParentsString = (o, child) =>
                    {
                        return new System.Collections.ArrayList();
                    };

                    GPAS.Ontology.Fakes.ShimOntology.AllInstances.GetAllChildsString = (o, parent) =>
                    {
                        return new List<string>() { parent };
                    };

                    Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                    {
                        if (allProperties.ContainsKey(propertyTypeUri))
                            return allProperties[propertyTypeUri];

                        return Ontology.BaseDataTypes.String;
                    };

                    //SearchServer.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplySearchObjectsSynchronizationResultAsyncSynchronizationChanges
                    //    = async (client, changes) =>
                    //    {
                    //        await Task.Delay(0);
                    //    };

                    Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (o) =>
                    {
                        KeyValuePair<string, Ontology.BaseDataTypes>? GeoTimeProp = allProperties.Where(ap => ap.Value == Ontology.BaseDataTypes.GeoTime).FirstOrDefault();

                        if (GeoTimeProp == null)
                            return "زمان_و_موقعیت_جغرافیایی";
                        else
                            return GeoTimeProp.Value.Key;
                    };

                    for (int i = 0; i < QueryCount; i++)
                    {
                        string queryString = "Start query " + i.ToString() + ": " + QueryToString(queryList[i]) + "\n";
                        WriteLog(queryString);
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(QueryCount.ToString(), i.ToString()));

                        QuerySerializer serializer = new QuerySerializer();
                        MemoryStream stream = new MemoryStream();
                        serializer.Serialize(queryList[i], stream);
                        StreamUtility streamUtil = new StreamUtility();
                        byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(stream);
                        stream.Dispose();
                        stream.Flush();
                        stream.Close();

                        Watch.Start();
                        qr = serviceClient.RunStatisticalQuery(queryByteArray, authorizationParametters);
                        Watch.Stop();
                        totalTime += Watch.Elapsed;
                        Watch.Reset();
                    }
                }
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalTime.Ticks / QueryCount);
                AverageRetriveTimeString = averageTime.ToString(AverageTimeFormat);
                TotalRetriveTimeString = totalTime.ToString(TotalTimeFormat);

                WriteLog("RunStatisticalQuery " + QueryCount + " request with different query.\n" +
                    "Average Time = " + AverageRetriveTimeString + "\n" +
                    "Total Time = " + TotalRetriveTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AverageRetriveTimeString = ex.Message;
                TotalRetriveTimeString = ex.Message;

                WriteLog("RunStatisticalQuery " + QueryCount + " request with different query.\n" +
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
                    Title = "RunStatisticalQuery",
                    Description = "RunStatisticalQuery with different query.",
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
                                await Task.Run(() => RunStatisticalQuery());
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
