﻿using GPAS.AccessControl;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.Formula;
using GPAS.Utility;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using GPAS.LoadTest.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

namespace GPAS.Search.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class RetrieveObjectIDsByStatisticalQueryLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.SearchServer;
        public TestClassType TestClassType { get; set; } = TestClassType.RetrieveObjectIDsByStatistical;
        public string Title { get; set; } = "RetrieveObjectIDsByStatistical";

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
        public void RetrieveObjectIDsByStatisticalQuery()
        {
            long retrivedObjectsCount = 0;

            try
            {
                #region Assign
                if (!callRunStatisticalQueryFromTestMethod)
                {
                    SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();
                    syncPublishChangesLoadTests.SyncPublishChangesObjectsWithMultiProperties();
                }

                //List<Query> queryList = new List<Query>();
                //for (int i = 0; i < QueryCount; i++)
                //{
                //    Query query = new Query()
                //    {
                //        SourceSet = new StatisticalQuery.ObjectSet.StartingObjectSet(),
                //        FormulaSequence = new List<FormulaStep>(),
                //    };

                //    int repeat = Random.Next(1, 4);
                //    for (int j = 0; j < repeat; j++)
                //    {
                //        FormulaStep formulaStep = null;

                //        double x = RandomDouble(0, 3);
                //        if (x < 1)
                //        {
                //            formulaStep = GeneratePropertyValueRangeDrillDown();
                //        }
                //        else if (x < 2)
                //        {
                //            formulaStep = GenerateLinkBasedDrillDown();
                //        }
                //        else if (x < 3)
                //        {
                //            formulaStep = GenerateTypeBasedDrillDown();
                //        }
                //        else
                //        {
                //            //PerformSetOperation
                //        }

                //        query.FormulaSequence.Add(formulaStep);
                //    }

                //    queryList.Add(query);
                //}

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

                    Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                    {
                        if (allProperties.ContainsKey(propertyTypeUri))
                            return allProperties[propertyTypeUri];

                        return Ontology.BaseDataTypes.String;
                    };

                    GPAS.Ontology.Fakes.ShimOntology.AllInstances.GetAllChildsString = (o, parent) =>
                    {
                        return new List<string>() { parent };
                    };

                    for (int i = 0; i < QueryCount; i++)
                    {
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(QueryCount.ToString(), i.ToString()));

                        Query query = new Query()
                        {
                            SourceSet = new StatisticalQuery.ObjectSet.StartingObjectSet(),
                            FormulaSequence = new List<FormulaStep>(),
                        };

                        int repeat = Random.Next(1, 4);
                        for (int j = 0; j < repeat; j++)
                        {
                            FormulaStep formulaStep = null;

                            double x = RandomDouble(0, 3);
                            if (x < 1)
                            {
                                formulaStep = GeneratePropertyValueRangeDrillDown();
                            }
                            else if (x < 2)
                            {
                                formulaStep = GenerateLinkBasedDrillDown();
                            }
                            else if (x < 3)
                            {
                                formulaStep = GenerateTypeBasedDrillDown();
                            }
                            else
                            {
                                //PerformSetOperation
                            }

                            query.FormulaSequence.Add(formulaStep);
                        }
                        //string queryString = "Start query " + i.ToString() + ": " + QueryToString(query) + "\n";
                        //WriteLog(queryString);

                        QuerySerializer serializer = new QuerySerializer();
                        MemoryStream stream = new MemoryStream();
                        serializer.Serialize(query, stream);
                        StreamUtility streamUtil = new StreamUtility();
                        byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(stream);

                        Watch.Start();
                        var result = serviceClient.RetrieveObjectIDsByStatisticalQuery(queryByteArray, (int)BatchItems, authorizationParametters);
                        Watch.Stop();
                        totalTime += Watch.Elapsed;
                        Watch.Reset();

                        if (result != null)
                            retrivedObjectsCount += result.Length;
                    }
                }
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalTime.Ticks / QueryCount);
                AverageRetriveTimeString = averageTime.ToString(AverageTimeFormat);
                TotalRetriveTimeString = totalTime.ToString(TotalTimeFormat);

                WriteLog("Retrived " + retrivedObjectsCount + " objects in " + QueryCount + " requests with different query.\n" +
                    "Average Time = " + AverageRetriveTimeString + "\n" +
                    "Total Time = " + TotalRetriveTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AverageRetriveTimeString = ex.Message;
                TotalRetriveTimeString = ex.Message;

                WriteLog("Retrived " + retrivedObjectsCount + " objects in " + QueryCount + " requests with different query.\n" +
                   "Average Time = " + AverageRetriveTimeString + "\n" +
                   "Total Time = " + TotalRetriveTimeString + "\n\n");

                throw;
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
                    Title = "Retrieve ObjectIDs By Statistical Query",
                    Description = "Retrieve object IDs By statistical query with different query.",
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
                                await Task.Run(() => RetrieveObjectIDsByStatisticalQuery());
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