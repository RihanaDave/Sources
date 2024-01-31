using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.GlobalResolve;
using GPAS.LoadTest.Core;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace GPAS.Search.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class GetTypeBasedResolutionCandidatesLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.SearchServer;
        public TestClassType TestClassType { get; set; } = TestClassType.GetTypeBasedResolutionCandidates;
        public string Title { get; set; } = "GetTypeBasedResolutionCandidates";

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
        long MaxObjectId = 1;

        [TestInitialize]
        public void Init()
        {
            GPAS.SearchServer.Logic.SearchEngineProvider.Init();
            GPAS.SearchServer.Logic.GlobalResolutionCandidatesProvider.Init();

            SetOntology(allProperties.Keys.ToList());
        }

        bool callRunStatisticalQueryFromTestMethod = false;
        [TestMethod]
        public void GetTypeBasedResolutionCandidates()
        {
            long resolutionCandidateCount = 0;

            try
            {
                if (!callRunStatisticalQueryFromTestMethod)
                {
                    SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();
                    syncPublishChangesLoadTests.SyncPublishChangesRelationsWithMultiProperties();
                }

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

                    #region Assign
                    List<string> properties = allProperties.Keys.ToList();
                    Stopwatch Watch = new Stopwatch();
                    totalTime = new TimeSpan();
                    averageTime = new TimeSpan();
                    List<GlobalResolutionCandidates> result = new List<GlobalResolutionCandidates>();
                    #endregion

                    #region Act
                    for (int i = 0; i < QueryCount; i++)
                    {
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(QueryCount.ToString(), i.ToString()));
                        List<string> propNames = GetRandomElements(properties, Random.Next(1, properties.Count - 1));

                        LinkingProperty[] linkingProperties = GenerateLinkingProperties(propNames);
                        ImportingObject[] importingObjects = GenerateImportingObjects(allObjects, propNames);

                        result = new List<GlobalResolutionCandidates>();
                        Watch.Start();
                        try
                        {
                            result = serviceClient.GetTypeBasedResolutionCandidates(linkingProperties, importingObjects);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException is WebException)
                            {
                                WebException webEx = ex.InnerException as WebException;
                                if ((webEx.Response as HttpWebResponse).StatusCode != HttpStatusCode.BadRequest)
                                {
                                    throw ex;
                                }
                            }
                            else
                            {
                                throw ex;
                            }
                        }

                        Watch.Stop();
                        totalTime += Watch.Elapsed;
                        Watch.Reset();

                        if (result?.Count > 0)
                        {
                            foreach (var grc in result)
                            {
                                if (grc.ResolutionCandidates?.Length > 0)
                                {
                                    resolutionCandidateCount += grc.ResolutionCandidates.Length;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Assert

                    #endregion

                    #region Report
                    averageTime = new TimeSpan(totalTime.Ticks / QueryCount);
                    AverageRetriveTimeString = averageTime.ToString(AverageTimeFormat);
                    TotalRetriveTimeString = totalTime.ToString(TotalTimeFormat);

                    WriteLog("Found " + resolutionCandidateCount + " resolution candidate objecs in " + QueryCount + " request with different query.\n" +
                        "Average Time = " + AverageRetriveTimeString + "\n" +
                        "Total Time = " + TotalRetriveTimeString + "\n\n");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                AverageRetriveTimeString = ex.Message;
                TotalRetriveTimeString = ex.Message;

                WriteLog("Found " + resolutionCandidateCount + " resolution candidate objecs in " + QueryCount + " request with different query.\n" +
                        "Average Time = " + AverageRetriveTimeString + "\n" +
                        "Total Time = " + TotalRetriveTimeString + "\n\n");

                throw ex;
            }
        }

        private ImportingObject[] GenerateImportingObjects(List<string> objNames = null, List<string> propNames = null)
        {
            if (objNames == null || objNames.Count == 0)
            {
                objNames = new List<string>() { "object" };
            }

            if (propNames == null || propNames.Count == 0)
            {
                propNames = new List<string>() { "label" };
            }

            List<ImportingObject> importingObjects = new List<ImportingObject>();
            for (long id = MaxObjectId; id < MaxObjectId + BatchItems; id++)
            {
                ImportingObject obj = new ImportingObject()
                {
                    TypeUri = objNames[Random.Next(0, objNames.Count - 1)],
                };

                List<ImportingProperty> properties = new List<ImportingProperty>();
                string pName = propNames[Random.Next(0, propNames.Count - 1)];

                ImportingProperty property = new ImportingProperty()
                {
                    TypeURI = pName,
                    Value = GenerateValueProperty(pName, true, id).ToString(),
                };

                properties.Add(property);
                obj.Properties = properties;
                importingObjects.Add(obj);
            }
            MaxObjectId += BatchItems;

            return importingObjects.ToArray();
        }

        private LinkingProperty[] GenerateLinkingProperties(List<string> propNames = null)
        {
            if (propNames == null || propNames.Count == 0)
            {
                propNames = new List<string>() { "label" };
            }

            List<LinkingProperty> linkingProperties = new List<LinkingProperty>();

            foreach (var pName in propNames)
            {
                LinkingProperty linkingProperty = new LinkingProperty()
                {
                    resolutionOption = Random.NextDouble() > .5 ? ResolutionOption.NoConflict : ResolutionOption.ExactMatch,
                    typeURI = pName,
                };

                linkingProperties.Add(linkingProperty);
            }

            return linkingProperties.ToArray();
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
                    Title = "Get Type Based Resolution Candidates",
                    Description = "Get Type Based Resolution Candidates with different ogects and properties.",
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
                                await Task.Run(() => GetTypeBasedResolutionCandidates());
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
