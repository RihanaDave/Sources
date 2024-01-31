using GPAS.AccessControl;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.Sync;
using GPAS.StatisticalQuery;
using GPAS.Utility;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using GPAS.LoadTest.Core;
using System.Data;
using System.Globalization;
using System.Threading;

namespace GPAS.Search.Server.LoadTests
{
    [TestClass()]
    [Export(typeof(ILoadTestCategory))]
    public class SyncPublishChangesLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.SearchServer;
        public TestClassType TestClassType { get; set; } = TestClassType.SyncPublishChanges;
        public string Title { get; set; } = "SyncPublishChanges";

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

        long MaxObjectId = 1;
        long MaxPropertyId = 1;
        long MaxRelationId = 1;
        TimeSpan totalPublishTime = new TimeSpan();
        TimeSpan averageTime = new TimeSpan();

        [TestInitialize]
        public void Init()
        {
            GPAS.SearchServer.Logic.SearchEngineProvider.Init();
            GPAS.SearchServer.Logic.GlobalResolutionCandidatesProvider.Init();

            SetOntology(allProperties.Keys.ToList());
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
            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "SyncPublishChanges objects",
                    Description = "SyncPublishChanges objects with one property",
                    Statistics = new DataTable(),
                },
                new LoadTestResult
                {
                    Title = "SyncPublishChanges objects with properties",
                    Description = "SyncPublishChanges objects with multi properties",
                    Statistics = new DataTable(),
                },
                new LoadTestResult
                {
                    Title = "SyncPublishChanges objects and relations",
                    Description = "SyncPublishChanges objects with one property and relations",
                    Statistics = new DataTable(),
                },
                new LoadTestResult
                {
                    Title = "SyncPublishChanges objects with properties and relations",
                    Description = "Publish objects with multi properties and relations",
                    Statistics = new DataTable(),
                },
                //new LoadTestResult
                //{
                //    Title = "SyncPublishChanges objects with resolves",
                //    Description = "SyncPublishChanges objects with resolves",
                //    Statistics = new DataTable(),
                //},
                //new LoadTestResult
                //{
                //    Title = "SyncPublishChanges objects with nested resolves",
                //    Description = "SyncPublishChanges objects with nested resolves (2 step)",
                //    Statistics = new DataTable(),
                //},
            };

            //ایجاد ستون‌های جداول نتیجه تست‌ها
            foreach (var testResult in testResults)
            {
                testResult.Statistics.Columns.Add("Batch count");
                testResult.Statistics.Columns.Add("Batch items");
                testResult.Statistics.Columns.Add("Average Publish Time");
                testResult.Statistics.Columns.Add("Total Publish Time");
                testResult.Statistics.Columns.Add("Clear all data");
            }

            for (long storeIndex = startStore; storeIndex <= endStore; storeIndex *= 10)
            {
                var rows = new List<DataRow>();

                for (int i = 0; i < testResults.Count; i++)
                {
                    rows.Add(testResults[i].Statistics.NewRow());
                }

                BatchCount = storeIndex;

                foreach (var row in rows)
                {
                    row["Batch count"] = storeIndex.ToString();
                    row["Batch items"] = BatchItems.ToString();
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
                                await Task.Run(() => SyncPublishChangesObjectsWithOneProperty());
                                break;
                            case 1:
                                await Task.Run(() => SyncPublishChangesObjectsWithMultiProperties());
                                break;
                            case 2:
                                await Task.Run(() => SyncPublishChangesRelationsWithOneProperty());
                                break;
                            case 3:
                                await Task.Run(() => SyncPublishChangesRelationsWithMultiProperties());
                                break;
                            case 4:
                                await Task.Run(() => SyncPublishChangesWithResolves());
                                break;
                            case 5:
                                await Task.Run(() => SyncPublishChangesWithNestedResolves());
                                break;
                            default:
                                break;
                        }

                        rows[i]["Average Publish Time"] = AveragePublishTimeString;
                        rows[i]["Total Publish Time"] = TotalPublishTimeString;
                    }
                    catch(Exception ex)
                    {
                        rows[i]["Average Publish Time"] = ex.Message;
                        rows[i]["Total Publish Time"] = ex.Message;
                    }

                    try
                    {
                        await Task.Run(() => ClearAllData());
                        rows[i]["Clear all data"] = ClearAllDataTimeString;
                    }
                    catch(Exception ex)
                    {
                        rows[i]["Clear all data"] = ex.Message;
                    }

                    testResults[i].Statistics.Rows.Add(rows[i]);
                }
            }

            return testResults;
        }

        [TestMethod]
        public void SyncPublishChangesObjectsWithOneProperty()
        {
            try
            {
                #region Assign
                ClearAllData();
                totalPublishTime = new TimeSpan();
                averageTime = new TimeSpan();
                #endregion

                #region Act
                for (int batch = 0; batch < BatchCount; batch++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    AddedConcepts addedConcepts = GenerateAddedConcepts(allObjects);
                    ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
                    List<ResolveMasterObject> resolveMasterObjects = new List<ResolveMasterObject>();
                    bool isContinousPublish = true;
                    SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, DataSourceId, isContinousPublish);
                }
                FinalizeContinousPublish();
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalPublishTime.Ticks / BatchCount);
                AveragePublishTimeString = averageTime.ToString(AverageTimeFormat);
                TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects with one property.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Average Time = " + AveragePublishTimeString + "\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch(Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects with one property.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Average Time = " + AveragePublishTimeString + "\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void SyncPublishChangesObjectsWithMultiProperties()
        {
            try
            {
                #region Assign
                ClearAllData();
                totalPublishTime = new TimeSpan();
                averageTime = new TimeSpan();
                List<string> properties = allProperties.Keys.ToList();
                #endregion

                #region Act
                for (int batch = 0; batch < BatchCount; batch++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    AddedConcepts addedConcepts = GenerateAddedConcepts(allObjects, properties);
                    ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
                    List<ResolveMasterObject> resolveMasterObjects = new List<ResolveMasterObject>();
                    bool isContinousPublish = true;
                    SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, DataSourceId, isContinousPublish);
                }
                FinalizeContinousPublish();
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalPublishTime.Ticks / BatchCount);
                AveragePublishTimeString = averageTime.ToString(AverageTimeFormat);
                TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects with " + properties.Count + " property per each object.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Average Time = " + AveragePublishTimeString + "\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch(Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects with " + allProperties.Values.Count + " property per each object.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Average Time = " + AveragePublishTimeString + "\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void SyncPublishChangesRelationsWithOneProperty()
        {
            try
            {
                #region Assign
                ClearAllData();
                totalPublishTime = new TimeSpan();
                averageTime = new TimeSpan();
                #endregion

                #region Act
                for (int batch = 0; batch < BatchCount; batch++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    AddedConcepts addedConcepts = GenerateAddedConcepts(allObjects, null, allRelations);
                    ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
                    List<ResolveMasterObject> resolveMasterObjects = new List<ResolveMasterObject>();
                    bool isContinousPublish = true;
                    SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, DataSourceId, isContinousPublish);
                }
                FinalizeContinousPublish();
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalPublishTime.Ticks / BatchCount);
                AveragePublishTimeString = averageTime.ToString(AverageTimeFormat);
                TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects and relations with one property.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Average Time = " + AveragePublishTimeString + "\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects and relations with one property.\n" +
                     "Each Batch " + BatchItems + " items\n" +
                     "Average Time = " + AveragePublishTimeString + "\n" +
                     "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void SyncPublishChangesRelationsWithMultiProperties()
        {
            try
            {
                #region Assign
                ClearAllData();
                totalPublishTime = new TimeSpan();
                averageTime = new TimeSpan();
                List<string> properties = allProperties.Keys.ToList();
                #endregion

                #region Act
                for (int batch = 0; batch < BatchCount; batch++)
                {
                    OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    AddedConcepts addedConcepts = GenerateAddedConcepts(allObjects, properties, allRelations);
                    ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
                    List<ResolveMasterObject> resolveMasterObjects = new List<ResolveMasterObject>();
                    bool isContinousPublish = true;
                    SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, DataSourceId, isContinousPublish);
                }
                FinalizeContinousPublish();
                #endregion

                #region Assert

                #endregion

                #region Report
                averageTime = new TimeSpan(totalPublishTime.Ticks / BatchCount);
                AveragePublishTimeString = averageTime.ToString(AverageTimeFormat);
                TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects and relations with " + properties.Count + " property per each object.\n" +
                        "Each Batch " + BatchItems + " items\n" +
                        "Average Time = " + AveragePublishTimeString + "\n" +
                        "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("SyncPublishChanges " + BatchCount + " Batch of objects and relations with " + allProperties.Values.Count + " property per each object.\n" +
                        "Each Batch " + BatchItems + " items\n" +
                        "Average Time = " + AveragePublishTimeString + "\n" +
                        "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void SyncPublishChangesWithResolves()
        {
            long numberObjesctsInRepository = 0;
            long numberObjectsAfterResolved = 0;

            try
            {
                #region Assign
                SyncPublishChangesObjectsWithOneProperty();
                totalPublishTime = new TimeSpan();

                AddedConcepts addedConcepts = new AddedConcepts()
                {
                    AddedMedias = new List<SearchMedia>().ToArray(),
                    AddedObjects = new List<SearchObject>().ToArray(),
                    AddedProperties = new List<SearchProperty>().ToArray(),
                    AddedRelationships = new List<SearchRelationship>().ToArray(),
                };

                OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs("1", "1"));
                ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
                int step = 5;
                numberObjesctsInRepository = MaxObjectId - 1;
                List<ResolveMasterObject> resolveMasterObjects = GenerateResolveMasterObjects(numberObjesctsInRepository, step);
                numberObjectsAfterResolved = resolveMasterObjects.Count;
                bool isContinousPublish = false;
                #endregion

                #region Act
                SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, DataSourceId, isContinousPublish);

                FinalizeContinousPublish();
                #endregion

                #region Assert

                #endregion

                #region Report
                TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

                WriteLog("Resolved " + (numberObjesctsInRepository - numberObjectsAfterResolved) + " oject to " + numberObjectsAfterResolved + " object.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("Resolved " + (numberObjesctsInRepository - numberObjectsAfterResolved) + " oject to " + numberObjectsAfterResolved + " object.\n" +
                    "Each Batch " + BatchItems + " items\n" +
                    "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        [TestMethod]
        public void SyncPublishChangesWithNestedResolves()
        {
            long numberObjesctsInRepositoryInFirstResolve = 0;
            long numberObjectsAfterFirstResolved = 0;
            long numberObjectsAfterSecondResolved = 0;

            try { 
            #region Assign First Resolve
            SyncPublishChangesObjectsWithOneProperty();
            totalPublishTime = new TimeSpan();
            averageTime = new TimeSpan();

            AddedConcepts addedConcepts = new AddedConcepts()
            {
                AddedMedias = new List<SearchMedia>().ToArray(),
                AddedObjects = new List<SearchObject>().ToArray(),
                AddedProperties = new List<SearchProperty>().ToArray(),
                AddedRelationships = new List<SearchRelationship>().ToArray(),
            };

            OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs("2", "1"));
            ModifiedConcepts modifiedConcepts = GenerateModifiedConcepts();
            int step = 5;
            numberObjesctsInRepositoryInFirstResolve = MaxObjectId - 1;
            List<ResolveMasterObject> firstResolvedList = GenerateResolveMasterObjects(numberObjesctsInRepositoryInFirstResolve, step);
            numberObjectsAfterFirstResolved = firstResolvedList.Count;
            bool isContinousPublish = false;
            #endregion

            #region Act First Resolve
            SyncPublishChanges(addedConcepts, modifiedConcepts, firstResolvedList, DataSourceId, isContinousPublish);
            FinalizeContinousPublish();
            #endregion

            #region Assign Second Resolve
            OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs("2", "2"));

            List<ResolveMasterObject> secondResolvedList = new List<ResolveMasterObject>();
            for (long i = 1; i <= numberObjesctsInRepositoryInFirstResolve; i += (step * step))
            {
                ResolveMasterObject resolveMasterObject = new ResolveMasterObject();
                long id = i;
                resolveMasterObject.ResolutionMasterObjectID = id;
                id += step;
                resolveMasterObject.ResolutionCondidateObjectIDs = new long[step - 1];
                for (int j = 0; j < resolveMasterObject.ResolutionCondidateObjectIDs.Length; j++)
                {
                    resolveMasterObject.ResolutionCondidateObjectIDs[j] = id;
                    id += step;
                }

                secondResolvedList.Add(resolveMasterObject);
            }
            numberObjectsAfterSecondResolved = secondResolvedList.Count;
            #endregion

            #region Act Second Resolve
            SyncPublishChanges(addedConcepts, modifiedConcepts, secondResolvedList, DataSourceId, isContinousPublish);
            FinalizeContinousPublish();
            #endregion

            #region Assert

            #endregion

            #region Report
            averageTime = new TimeSpan(totalPublishTime.Ticks / 2);
            AveragePublishTimeString = averageTime.ToString(AverageTimeFormat);
            TotalPublishTimeString = totalPublishTime.ToString(TotalTimeFormat);

            WriteLog("Nested Resolved in 2 step\n" +
                        "First Resolve: " + (numberObjesctsInRepositoryInFirstResolve - numberObjectsAfterFirstResolved) + " oject to " + numberObjectsAfterFirstResolved + " object\n" +
                        "Second Resolve: " + (numberObjectsAfterFirstResolved - numberObjectsAfterSecondResolved) + " oject to " + numberObjectsAfterSecondResolved + " object\n" +
                        "Each Batch " + BatchItems + " items\n" +
                        "Average Time = " + AveragePublishTimeString + "\n" +
                        "Total Time = " + TotalPublishTimeString + "\n\n");
                #endregion
            }
            catch (Exception ex)
            {
                AveragePublishTimeString = ex.Message;
                TotalPublishTimeString = ex.Message;

                WriteLog("Nested Resolved in 2 step\n" +
                       "First Resolve: " + (numberObjesctsInRepositoryInFirstResolve - numberObjectsAfterFirstResolved) + " oject to " + numberObjectsAfterFirstResolved + " object\n" +
                       "Second Resolve: " + (numberObjectsAfterFirstResolved - numberObjectsAfterSecondResolved) + " oject to " + numberObjectsAfterSecondResolved + " object\n" +
                       "Each Batch " + BatchItems + " items\n" +
                       "Average Time = " + AveragePublishTimeString + "\n" +
                       "Total Time = " + TotalPublishTimeString + "\n\n");

                throw ex;
            }
        }

        private List<ResolveMasterObject> GenerateResolveMasterObjects(long numberObjesctsInRepository, int step)
        {
            List<ResolveMasterObject> Resolves = new List<ResolveMasterObject>();
            for (long i = 1; i <= numberObjesctsInRepository; i += step)
            {
                ResolveMasterObject resolveMasterObject = new ResolveMasterObject();
                long id = i;
                resolveMasterObject.ResolutionMasterObjectID = id++;
                resolveMasterObject.ResolutionCondidateObjectIDs = new long[step - 1];
                for (int j = 0; j < resolveMasterObject.ResolutionCondidateObjectIDs.Length; j++)
                {
                    resolveMasterObject.ResolutionCondidateObjectIDs[j] = id++;
                }

                Resolves.Add(resolveMasterObject);
            }

            return Resolves;
        }

        private void FinalizeContinousPublish()
        {
            using (ShimsContext.Create())
            {
                SearchServer.Fakes.ShimOntologyProvider.GetOntology = () =>
                {
                    return ontology;
                };

                serviceClient.FinalizeContinousPublish();
            }
        }

        private LinkTypeStatistics RetrieveLinkTypeStatistics()
        {
            Query query = new Query()
            {
                SourceSet = new StatisticalQuery.ObjectSet.StartingObjectSet(),
            };

            StatisticalQuery.QuerySerializer serializer = new QuerySerializer();
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(query, stream);
            StreamUtility streamUtil = new StreamUtility();
            byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(stream);

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

            using (ShimsContext.Create())
            {
                SearchServer.Fakes.ShimOntologyProvider.GetOntology = () =>
                {
                    return ontology;
                };

                return serviceClient.RetrieveLinkTypeStatistics(queryByteArray, authorizationParametters);
            }
        }

        public QueryResult GetStatistical()
        {
            Query query = new Query()
            {
                SourceSet = new StatisticalQuery.ObjectSet.StartingObjectSet(),
            };

            StatisticalQuery.QuerySerializer serializer = new QuerySerializer();
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(query, stream);
            StreamUtility streamUtil = new StreamUtility();
            byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(stream);

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

                return serviceClient.RunStatisticalQuery(queryByteArray, authorizationParametters);
            }
        }

        private Dispatch.Entities.Publish.SynchronizationResult SyncPublishChanges(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts,
                                                        List<ResolveMasterObject> resolveMasterObjects, long dataSourceID, bool isContinousPublish = true)
        {
            using (ShimsContext.Create())
            {
                var dic = new Dictionary<long, AccessControl.ACL>();
                foreach (var id in new List<long>() { dataSourceID })
                {
                    dic[id] = currentACL;
                }

                SearchServer.Access.DataClient.Fakes.ShimRetrieveDataClient.AllInstances.GetDataSourceACLsMappingListOfInt64 = (rdc, ids) =>
                {
                    return dic;
                };

                //SearchServer.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplySearchObjectsSynchronizationResultAsyncSynchronizationChanges
                //    = async (client, changes) =>
                //    {
                //        await Task.Delay(0);
                //    };

                SearchServer.Fakes.ShimOntologyProvider.GetOntology = () => {
                    return ontology;
                };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
                };

                Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (o) =>
                {
                    KeyValuePair<string, Ontology.BaseDataTypes>? GeoTimeProp = allProperties.Where(ap => ap.Value == Ontology.BaseDataTypes.GeoTime).FirstOrDefault();

                    if (GeoTimeProp == null)
                        return "زمان_و_موقعیت_جغرافیایی";
                    else
                        return GeoTimeProp.Value.Key;
                };

                Stopwatch Watch = new Stopwatch();
                Watch.Start();
                var result = serviceClient.SyncPublishChanges(addedConcepts, modifiedConcepts, resolveMasterObjects, dataSourceID, isContinousPublish);
                Watch.Stop();
                totalPublishTime += Watch.Elapsed;
                Watch.Reset();
                return result;
            }
        }

        private ModifiedConcepts GenerateModifiedConcepts(bool hasProperty = false, bool hasMedia = false)
        {
            return new ModifiedConcepts()
            {
                DeletedMedias = new List<SearchMedia>(),
                ModifiedProperties = new List<ModifiedProperty>().ToArray(),
            };
        }

        private AddedConcepts GenerateAddedConcepts(List<string> objNames = null, List<string> propNames = null, List<string> relNames = null)
        {
            if (objNames == null || objNames.Count == 0)
            {
                objNames = new List<string>() { "object" };
            }

            if (propNames == null || propNames.Count == 0)
            {
                propNames = new List<string>() { "label" };
            }

            if (!propNames.Contains("label"))
            {
                propNames.Insert(0, "label");
            }

            List<SearchObject> addedObjects = new List<SearchObject>();
            List<SearchProperty> addedProperties = new List<SearchProperty>();

            for (long id = MaxObjectId; id < MaxObjectId + BatchItems; id++)
            {
                SearchObject searchObject = new SearchObject
                {
                    Id = id,
                    TypeUri = objNames[Random.Next(0, objNames.Count - 1)],
                };

                for (int i = 0; i < propNames.Count; i++)
                {
                    string pName = propNames[i];
                    string value = GenerateValueProperty(pName, true, id).ToString();

                    SearchProperty searchProperty = new SearchProperty()
                    {
                        DataSourceID = DataSourceId,
                        Id = MaxPropertyId,
                        OwnerObject = searchObject,
                        TypeUri = pName,
                        Value = value,
                    };

                    if (pName.Equals("label"))
                    {
                        searchObject.LabelPropertyID = searchProperty.Id;
                    }

                    addedProperties.Add(searchProperty);
                    MaxPropertyId++;
                }

                addedObjects.Add(searchObject);
            }

            MaxObjectId += BatchItems;

            List<SearchRelationship> addedRelationships = new List<SearchRelationship>();
            if (relNames?.Count > 0)
            {
                for (long id = MaxRelationId; id < MaxRelationId + BatchItems; id++)
                {
                    int sourceObjectIndex = Random.Next(0, addedObjects.Count - 1);
                    int targetObjectIndex = Random.Next(0, addedObjects.Count - 1);

                    SearchRelationship searchRelationship = new SearchRelationship()
                    {
                        Id = id,
                        DataSourceID = DataSourceId,
                        TypeUri = relNames[Random.Next(0, relNames.Count - 1)],
                        SourceObjectId = addedObjects[sourceObjectIndex].Id,
                        SourceObjectTypeUri = addedObjects[sourceObjectIndex].TypeUri,
                        TargetObjectId = addedObjects[targetObjectIndex].Id,
                        TargetObjectTypeUri = addedObjects[targetObjectIndex].TypeUri,
                    };

                    addedRelationships.Add(searchRelationship);
                }
            }
            MaxRelationId += BatchItems;

            return new AddedConcepts()
            {
                AddedMedias = new List<SearchMedia>().ToArray(),
                AddedObjects = addedObjects.ToArray(),
                AddedProperties = addedProperties.ToArray(),
                AddedRelationships = addedRelationships.ToArray(),
            };
        }

        public void ClearAllData()
        {
            try
            {
                MaxObjectId = 1;
                MaxPropertyId = 1;
                MaxRelationId = 1;

                using (ShimsContext.Create())
                {
                    SearchServer.Fakes.ShimOntologyProvider.GetOntology = () =>
                    {
                        return ontology;
                    };

                    Stopwatch Watch = new Stopwatch();
                    Watch.Start();
                    serviceClient.RemoveSearchIndexes();
                    Watch.Stop();
                    ClearAllDataTimeString = Watch.Elapsed.ToString(AverageTimeFormat);
                    WriteLog("Clear all data. Time = " + ClearAllDataTimeString);
                }
            }
            catch (Exception ex)
            {
                ClearAllDataTimeString = ex.Message;
                throw ex;
            }
        }
    }
}