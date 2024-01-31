using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Horizon.Server.LoadTests.Properties;
using GPAS.LoadTest.Core;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.Horizon.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class GetSingleRelatedEntitiesLoadTest : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.HorizonServer;
        public TestClassType TestClassType { get; set; } = TestClassType.SingleConnectedEntitisRetrival;
        public string Title { get; set; } = "Retrieve single connected entities ";                

        private Stopwatch timer;

        [ClassInitialize]
        public static void PublishObjectsWithOnePropertyBeforeTests(TestContext testContext)
        {
            //GetSingleRelatedEntitiesLoadTest getSingleRelatedEntitiesLoadTest = new GetSingleRelatedEntitiesLoadTest();
            //getSingleRelatedEntitiesLoadTest.Publish();
        }

        [ClassCleanup]
        public static void ClearAllDataAfterRunAllTests()
        {
            GetSingleRelatedEntitiesLoadTest getSingleRelatedEntitiesLoadTest = new GetSingleRelatedEntitiesLoadTest();
            //getSingleRelatedEntitiesLoadTest.ClearAllData();
        }        

        private void ClearAllData()
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.ClearAllData();
        }

        private void Publish()
        {
            
            ExtensionFileToSave = DateTime.Now.ToString("HH-mm-ss") + ".txt";            
            WriteLog(" ******************  Retrieve single connected entities load tests  ****************** ");
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.TestCaseProgressChanged += PublishLoadTests_TestCaseProgressChanged;
            publishLoadTests.PublishRelationshipsWithOneConnection();
        }

        private void PublishLoadTests_TestCaseProgressChanged(object sender, TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        [TestMethod]
        public bool FindRelatedEntities()
        {
            #region Assign     
            bool retrievalSucceeded = true;
            var authorization = new GPAS.AccessControl.AuthorizationParametters
            {
                permittedGroupNames = new List<string> { "Administrators" },
                readableClassifications = new List<string> { Classification.EntriesTree.First().IdentifierString }
            };
            timer = new Stopwatch();
            #endregion

            #region Act
            using (ShimsContext.Create())
            {
                Horizon.Logic.Fakes.ShimOntologyProvider.GetOntology = () =>
                {
                    return ontology;
                };
                Horizon.Logic.Fakes.ShimOntologyMaterial.GetOntologyMaterialOntology = (a) =>
                {
                    return ontologyMaterial;
                };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
                };

                GPAS.Ontology.Fakes.ShimOntology.AllInstances.GetAllChildsString = (o, parent) =>
                {
                    return new List<string>() { "شخص" };
                };

                var dic = new Dictionary<long, AccessControl.ACL>();
                foreach (var id in new List<long>() { DataSourceId })
                {
                    dic[id] = currentACL;
                }

                Horizon.Access.DataClient.Fakes.ShimRetrieveDataClient.AllInstances.GetDataSourceACLsListOfInt64 = (rdc, ids) =>
                {
                    return dic;
                };

                GenerateDefaultOntology(allProperties.Keys.ToList());
                GenerateDefaultOntologyMaterial();

                for (int batch = 0; batch < BatchCount; batch++)
                {
                    long[] listOfObjectsIds = Enumerable.Range(1, RetrieveItemsCount).OrderBy(x =>
                    Random.Next()).Take(RetrieveItemsCount).Select(Convert.ToInt64).ToArray();
                    var listOfObjects = new Dictionary<string, long[]>
                    {
                        { "شخص", listOfObjectsIds }
                    };

                    try
                    {
                        timer.Start();
                        List<RelationshipBasedResultsPerSearchedObjects> result =
                            HorizonService.FindRelatedEntities(listOfObjects, resultLimit, authorization);
                        WriteLog($"{result.Count} related entitie(s) for batch = {batch} and BatchCount = {BatchCount} was retrieved.\n");
                        timer.Stop();

                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));

                        #region Assert

                        retrievalSucceeded = SingleRelatedEntitiesDataValidation(result);
                        if (!retrievalSucceeded)
                        {                            
                            break;
                        }
                        #endregion                        
                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format(ex.Message + "(BatchCount = {0}/{1}, BatchItems = {2})", batch, BatchCount, BatchItems);
                        WriteLog(logMessage);
                        break;
                    }
                }

                if (retrievalSucceeded)
                {
                    var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);

                    TestTime = averageTime.ToString(OutPutTimeFormat);

                    WriteLog(" ************************************************************************************************************ ");
                    WriteLog("Retrieve single connected entities " + BatchCount + " times in average " +
                         averageTime.ToString(OutPutTimeFormat));
                    WriteLog(" ************************************************************************************************************ ");
                }
            }
            #endregion

            return retrievalSucceeded;
        }

        private bool SingleRelatedEntitiesDataValidation(List<RelationshipBasedResultsPerSearchedObjects> retrievedData)
        {
            bool validation = true;

            foreach (var currentRelation in retrievedData)
            {
                long remainingOfSearchedObjIdDevidedByBatchItem = (currentRelation.SearchedObjectID % BatchItems);

                if (remainingOfSearchedObjIdDevidedByBatchItem == 0 ||
                    remainingOfSearchedObjIdDevidedByBatchItem == 1)
                {
                    if (currentRelation.NotLoadedResults.Count() != 1)
                    {
                        validation = false;
                        string failedMassage = string.Format("Single related entity retrieval failed for id = {0}", currentRelation.SearchedObjectID);
                        WriteLog(failedMassage);
                    }
                    else
                    {
                        if (Math.Abs(currentRelation.NotLoadedResults[0].TargetObjectID - currentRelation.SearchedObjectID) != 1)
                        {
                            validation = false;
                            string failedMassage = string.Format("Single related entity retrieval failed for id = {0}", currentRelation.SearchedObjectID);
                            WriteLog(failedMassage);
                        }
                    }
                }
                else
                {
                    if (currentRelation.NotLoadedResults.Count() != 2)
                    {
                        validation = false;
                        string failedMassage = string.Format("Single related entity retrieval failed for id = {0}", currentRelation.SearchedObjectID);
                        WriteLog(failedMassage);
                    }
                    else
                    {
                        if (Math.Abs(currentRelation.NotLoadedResults[0].TargetObjectID - currentRelation.SearchedObjectID) != 1 ||
                            Math.Abs(currentRelation.NotLoadedResults[1].TargetObjectID - currentRelation.SearchedObjectID) != 1)
                        {
                            validation = false;
                            string failedMassage = string.Format("Single related entity retrieval failed for id = {0}", currentRelation.SearchedObjectID);
                            WriteLog(failedMassage);
                        }
                    }
                }
            }

            return validation;
        }

        [TestMethod]
        public async Task test()
        {
            try
            {
                startRunTestsTime = DateTime.Now.ToString("HH-mm-ss") + ".txt";
                List<LoadTestResult> result = await RunTests(10, 100, 10, 100, new CancellationToken(false));
                WriteLog(result);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                Assert.Fail();
            }
            
        }

        private void WriteLog(List<LoadTestResult> results)
        {
            try
            {
                string resultFolderPath = Resources.LogFolder;

                if (!Directory.Exists(resultFolderPath))
                {
                    Directory.CreateDirectory(resultFolderPath);
                }

                foreach (var result in results)
                {
                    using (StreamWriter streamWriter = File.CreateText(Path.Combine(resultFolderPath, result.Title + "_" + startRunTestsTime)))
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
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                //MessageBox.Show(e.Message);
            }
        }

        public async Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve,
            CancellationToken token)
        {
            //تعداد متد‌‌های تست در این کلاس
            int testsMethodCount = 1;

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
                    Title = "Retrieve related entities",
                    Description = "Retrieve related entities of random selected objects with one connection",
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
                    "Publish single connected relationships", testResults));

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
                    WriteLog(exception.Message);
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
                                    await Task.Run(() => FindRelatedEntities());
                                    break;
                            }

                            testResults[i].Statistics.Rows[rowCount][retrieveIndex.ToString()] = TestTime;
                        }
                        catch (Exception exception)
                        {
                            WriteLog(exception.Message);
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
                    WriteLog(exception.Message);
                    foreach (var testResult in testResults)
                    {
                        testResult.Statistics.Rows[rowCount]["Clear all data"] = exception.Message;
                    }
                }

                rowCount++;
            }

            return testResults;
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
    }
}
