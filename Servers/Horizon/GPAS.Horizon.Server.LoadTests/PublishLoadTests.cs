using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Server.LoadTests.Properties;
using GPAS.LoadTest.Core;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace GPAS.Horizon.Server.LoadTests
{
    [TestClass]
    [Export(typeof(ILoadTestCategory))]
    public class PublishLoadTests : BaseLoadTest, ILoadTestCategory
    {
        public ServerType ServerType { get; set; } = ServerType.HorizonServer;
        public TestClassType TestClassType { get; set; } = TestClassType.PublishLinks;
        public string Title { get; set; } = "Publish Links ";
        
        private const int MaxPropertyValuesLength = 100;
        
        private Stopwatch timer;

        public void ClearAllData()
        {
            OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), BatchCount.ToString()));

            MaxObjectId = 1;
            MaxPropertyId = 1;
            MaxRelationId = 1;
            ClearAllDataTime = "";
            PublishDataTime = "";
            TestTime = "";

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

                timer = new Stopwatch();

                try
                {
                    timer.Start();
                    HorizonService.RemoveHorizonIndexes();
                    timer.Stop();

                    ClearAllDataTime = timer.Elapsed.ToString(@"hh\:mm\:ss\:fff");

                    string message = "Clear all data in " + ClearAllDataTime;

                    WriteLog("Clear all data in " + ClearAllDataTime);
                }
                catch (Exception ex)
                {                    
                    WriteLog(ex.Message);
                }                
            }
        }

        public AddedConcepts GenerateObjectsWithPropertyForRelationships(bool multiProperties)
        {
            List<KObject> generatedObjects = new List<KObject>();

            for (var id = MaxObjectId; id < MaxObjectId + BatchItems; id++)
            {
                generatedObjects.Add(new KObject
                {
                    Id = id,
                    IsGroup = false,
                    TypeUri = "شخص",
                    LabelPropertyID = id
                });
            }

            List<KProperty> generatedproperties = new List<KProperty>();

            int objectListIndex = 0;

            for (int id = MaxPropertyId; id < MaxPropertyId + BatchItems; id++)
            {
                generatedproperties.Add(new KProperty
                {
                    Id = id,
                    TypeUri = "label",
                    Value = RandomString(),
                    DataSourceID = DataSourceId,
                    Owner = generatedObjects[objectListIndex]
                });

                objectListIndex++;
            }

            int propertyId = BatchItems;
            if (multiProperties)
            {
                objectListIndex = 0;
                propertyId = MaxPropertyId + BatchItems;

                while (generatedObjects.Count > objectListIndex)
                {
                    int propertiesCount = 1;

                    while (propertiesCount < PropertiesCount)
                    {
                        generatedproperties.Add(new KProperty
                        {
                            Id = propertyId,
                            TypeUri = "نام",
                            Value = RandomString(),
                            DataSourceID = DataSourceId,
                            Owner = generatedObjects[objectListIndex]
                        });

                        propertyId++;
                        propertiesCount++;
                    }

                    objectListIndex++;
                }
            }

            MaxObjectId += BatchItems;
            MaxPropertyId += propertyId;

            AddedConcepts dbAddedConcepts = new AddedConcepts
            {
                AddedObjects = generatedObjects,
                AddedProperties = generatedproperties,
                AddedMedias = new List<KMedia>(),
                AddedRelationships = new List<RelationshipBaseKlink>()
            };

            return dbAddedConcepts;
        }

        public AddedConcepts GenerateObjectsWithPropertyForEvents(bool multiProperties)
        {
            List<KObject> objectsList = new List<KObject>();

            for (var id = MaxObjectId; id < MaxObjectId + BatchItems - 2; id = id + 3)
            {
                objectsList.Add(new KObject
                {
                    Id = id,
                    IsGroup = false,
                    TypeUri = "شخص",
                    LabelPropertyID = id
                });
                objectsList.Add(new KObject
                {
                    Id = id + 1,
                    IsGroup = false,
                    TypeUri = "مکالمه_تلفنی",
                    LabelPropertyID = id + 1
                });
                objectsList.Add(new KObject
                {
                    Id = id + 2,
                    IsGroup = false,
                    TypeUri = "شخص",
                    LabelPropertyID = id + 2
                });
            }

            objectsList.Add(new KObject
            {
                Id = MaxObjectId + BatchItems - 1,
                IsGroup = false,
                TypeUri = "شخص",
                LabelPropertyID = MaxObjectId + BatchItems - 1
            });

            List<KProperty> propertiesList = new List<KProperty>();

            int objectListIndex = 0;

            for (int id = MaxPropertyId; id < MaxPropertyId + BatchItems; id++)
            {
                propertiesList.Add(new KProperty
                {
                    Id = id,
                    TypeUri = "label",
                    Value = RandomString(),
                    DataSourceID = DataSourceId,
                    Owner = objectsList[objectListIndex]
                });
                objectListIndex++;
            }

            int propertyId = BatchItems;
            if (multiProperties)
            {
                objectListIndex = 0;
                propertyId = MaxPropertyId + BatchItems;

                while (objectsList.Count > objectListIndex)
                {
                    int propertiesCount = 1;

                    while (propertiesCount < PropertiesCount)
                    {
                        propertiesList.Add(new KProperty
                        {
                            Id = propertyId,
                            TypeUri = "نام",
                            Value = RandomString(),
                            DataSourceID = DataSourceId,
                            Owner = objectsList[objectListIndex]
                        });

                        propertyId++;
                        propertiesCount++;
                    }

                    objectListIndex++;
                }
            }

            MaxObjectId += BatchItems;
            MaxPropertyId += propertyId;

            AddedConcepts dbAddedConcepts = new AddedConcepts
            {
                AddedObjects = objectsList,
                AddedProperties = propertiesList,
                AddedMedias = new List<KMedia>(),
                AddedRelationships = new List<RelationshipBaseKlink>()
            };

            return dbAddedConcepts;
        }

        public AddedConcepts GenerateRelationshipsWithOneConnection(bool multiProperties)
        {
            var objects = GenerateObjectsWithPropertyForRelationships(multiProperties);

            List<RelationshipBaseKlink> relationshipsList = new List<RelationshipBaseKlink>();

            for (int id = MaxRelationId; id < MaxRelationId + BatchItems; id++)
            {
                int indexOfCurrentObject = id % BatchItems;

                if (indexOfCurrentObject != 0 && indexOfCurrentObject < objects.AddedObjects.Count)
                {
                    relationshipsList.Add(new RelationshipBaseKlink
                    {
                        Relationship = new KRelationship
                        {
                            Id = id,
                            DataSourceID = DataSourceId,
                            Description = RandomString(),
                            Direction = LinkDirection.SourceToTarget
                        },
                        TypeURI = "حضور_در",
                        Source = objects.AddedObjects[indexOfCurrentObject - 1],
                        Target = objects.AddedObjects[indexOfCurrentObject],
                    });
                }                            
            }

            MaxRelationId += BatchItems;

            objects.AddedRelationships = relationshipsList;

            return objects;
        }

        public AddedConcepts GenerateObjectsWithPropertyForRelatedDocuments(bool multiProperties)
        {
            List<KObject> generatedObjects = new List<KObject>();

            for (var id = MaxObjectId; id < MaxObjectId + BatchItems - 1; id = id + 2)
            {
                generatedObjects.Add(new KObject
                {
                    Id = id,
                    IsGroup = false,
                    TypeUri = "شخص",
                    LabelPropertyID = id
                });

                generatedObjects.Add(new KObject
                {
                    Id = id + 1,
                    IsGroup = false,
                    TypeUri = "سند",
                    LabelPropertyID = id + 1
                });
            }

            List<KProperty> generatedproperties = new List<KProperty>();

            int objectListIndex = 0;

            for (int id = MaxPropertyId; id < MaxPropertyId + BatchItems; id++)
            {
                generatedproperties.Add(new KProperty
                {
                    Id = id,
                    TypeUri = "label",
                    Value = RandomString(),
                    DataSourceID = DataSourceId,
                    Owner = generatedObjects[objectListIndex]
                });

                objectListIndex++;
            }

            int propertyId = BatchItems;
            if (multiProperties)
            {
                objectListIndex = 0;
                propertyId = MaxPropertyId + BatchItems;

                while (generatedObjects.Count > objectListIndex)
                {
                    int propertiesCount = 1;

                    while (propertiesCount < PropertiesCount)
                    {
                        generatedproperties.Add(new KProperty
                        {
                            Id = propertyId,
                            TypeUri = "نام",
                            Value = RandomString(),
                            DataSourceID = DataSourceId,
                            Owner = generatedObjects[objectListIndex]
                        });

                        propertyId++;
                        propertiesCount++;
                    }

                    objectListIndex++;
                }
            }

            MaxObjectId += BatchItems;
            MaxPropertyId += propertyId;

            AddedConcepts dbAddedConcepts = new AddedConcepts
            {
                AddedObjects = generatedObjects,
                AddedProperties = generatedproperties,
                AddedMedias = new List<KMedia>(),
                AddedRelationships = new List<RelationshipBaseKlink>()
            };

            return dbAddedConcepts;
        }

        // در این تابع هر شیء به تعدادی از اشیاء با شناسه ی بالاتر از خودش متصل شده است.
        public AddedConcepts GenerateRelationshipsWithMultipleConnections(bool multiProperties, int batchNumber)
        {
            var objects = GenerateObjectsWithPropertyForRelationships(multiProperties);

            List<RelationshipBaseKlink> relationshipsList = new List<RelationshipBaseKlink>();

            for (int id = MaxRelationId; id < MaxRelationId + BatchItems; id++)
            {
                for (int i = 1; i < RelationCounts + 1; i++)
                {
                    if (((id - ((batchNumber - 1) * BatchItems)) + i - 1) < objects.AddedObjects.Count())
                    {
                        relationshipsList.Add(new RelationshipBaseKlink
                        {
                            Relationship = new KRelationship
                            {
                                Id = id,
                                DataSourceID = DataSourceId,
                                Description = RandomString(),
                                Direction = LinkDirection.Bidirectional
                            },
                            TypeURI = "حضور_در",
                            Source = objects.AddedObjects[((id - ((batchNumber - 1) * BatchItems)) - 1)],
                            Target = objects.AddedObjects[((id - ((batchNumber - 1) * BatchItems)) + i - 1)],
                        });
                    }
                }
            }

            MaxRelationId += BatchItems;

            objects.AddedRelationships = relationshipsList;

            return objects;
        }

        private int GetNumberOf(List<RelationshipBaseKlink> relationshipsList, long v)
        {
            int a = 0;

            foreach (var currentRelation in relationshipsList)
            {
                if (currentRelation.Source.Id == v || currentRelation.Target.Id == v)
                {
                    a++;
                }
            }

            return a;
        }

        private AddedConcepts GenerateRelatedDocumentsWithSingleConnections(bool multiProperties, int batchNumber)
        {
            var objects = GenerateObjectsWithPropertyForRelatedDocuments(multiProperties);

            List<RelationshipBaseKlink> relationshipsList = new List<RelationshipBaseKlink>();

            for (int id = MaxRelationId; id < MaxRelationId + BatchItems - 1; id = id + 2)
            {                
                relationshipsList.Add(new RelationshipBaseKlink
                {
                    Relationship = new KRelationship
                    {
                        Id = id,
                        DataSourceID = DataSourceId,
                        Description = RandomString(),
                        Direction = LinkDirection.Bidirectional
                    },
                    TypeURI = "حضور_در",
                    Source = objects.AddedObjects[((id - ((batchNumber - 1) * BatchItems)) - 1)],
                    Target = objects.AddedObjects[(id - ((batchNumber - 1) * BatchItems))],
                });
            }

            MaxRelationId += BatchItems;

            objects.AddedRelationships = relationshipsList;

            return objects;
        }

        public AddedConcepts GenerateEvents(bool multiProperties, int batchNumber)
        {
            var objects = GenerateObjectsWithPropertyForEvents(multiProperties);

            List<RelationshipBaseKlink> relationshipsList = new List<RelationshipBaseKlink>();

            for (int id = MaxRelationId; id < MaxRelationId + BatchItems - 1; id = id + 3)
            {
                relationshipsList.Add(new RelationshipBaseKlink
                {
                    Relationship = new KRelationship
                    {
                        Id = id,
                        DataSourceID = DataSourceId,
                        Description = RandomString(),
                        Direction = LinkDirection.Bidirectional
                    },
                    TypeURI = "حضور_در",
                    Source = objects.AddedObjects[((id - ((batchNumber - 1) * BatchItems)) - 1)],
                    Target = objects.AddedObjects[(id - ((batchNumber - 1) * BatchItems))],
                });

                relationshipsList.Add(new RelationshipBaseKlink
                {
                    Relationship = new KRelationship
                    {
                        Id = id + 1,
                        DataSourceID = DataSourceId,
                        Description = RandomString(),
                        Direction = LinkDirection.Bidirectional
                    },
                    TypeURI = "حضور_در",
                    Source = objects.AddedObjects[(id - ((batchNumber - 1) * BatchItems))],
                    Target = objects.AddedObjects[((id - ((batchNumber - 1) * BatchItems)) + 1)],
                });
            }

            MaxRelationId += BatchItems;

            objects.AddedRelationships = relationshipsList;

            return objects;
        }

#pragma warning disable CS0108 // 'PublishLoadTests.RandomString()' hides inherited member 'BaseLoadTest.RandomString()'. Use the new keyword if hiding was intended.
        private string RandomString()
#pragma warning restore CS0108 // 'PublishLoadTests.RandomString()' hides inherited member 'BaseLoadTest.RandomString()'. Use the new keyword if hiding was intended.
        {
            const string charsWithNumbers = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(charsWithNumbers, Random.Next(0, MaxPropertyValuesLength)).Select(s => s[Random.Next(s.Length)]).ToArray());
        }


        [ClassInitialize]
        public static void SetFileNameToSaveLogFile(TestContext testContext)
        {
            PublishLoadTests publishLoadTests = new PublishLoadTests();
            publishLoadTests.Publish();
        }

        private void Publish()
        {
            ExtensionFileToSave = DateTime.Now.ToString("HH-mm-ss") + ".txt";
            WriteLog(" ******************  Publish links (entities, events, documents) load tests  ****************** ");
        }

        [TestMethod]
        public void PublishRelationshipsWithOneConnection()
        {
            #region Assign
            timer = new Stopwatch();

            var modified = new ModifiedConcepts
            {
                DeletedMedias = new List<KMedia>(),
                ModifiedProperties = new List<ModifiedProperty>()
            };
            #endregion

            #region Act
            ClearAllData();

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

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonObjectsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonRelationshipsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.DeleteHorizonServerUnsyncConceptsAsync
                    = async (client) =>
                    {
                        await Task.Delay(0);
                    };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
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

                for (int batch = 1; batch <= BatchCount; batch++)
                {                    
                    var data = GenerateRelationshipsWithOneConnection(true);                    

                    try
                    {
                        timer.Start();                        
                        HorizonService.SyncPublishChanges(data, modified, new ResolvedObject[] { }, DataSourceId, false);                        
                        timer.Stop();
                        WriteLog($"Relationships with one connection for batch = {batch} and BatchCount = {BatchCount} was published.\n");
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format(ex.Message + "(BatchCount = {0}/{1}, BatchItems = {2})", batch, BatchCount, BatchItems);
                        WriteLog(logMessage);                        
                    }
                    
                }

                var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
                PublishDataTime = averageTime.ToString(OutPutTimeFormat);

                WriteLog(" ************************************************************************************************************ ");
                string message = "Publish " + BatchCount + " batch of single connected relationships in average " +
                                 averageTime.ToString(OutPutTimeFormat);

                WriteLog(message);
                WriteLog(" ************************************************************************************************************ ");
            }

            #endregion

            #region Assert

            #endregion
        }

        [TestMethod]
        public void PublishRelationshipsWithMultipleConnections()
        {            
            #region Assign
            timer = new Stopwatch();

            var modified = new ModifiedConcepts
            {
                DeletedMedias = new List<KMedia>(),
                ModifiedProperties = new List<ModifiedProperty>()
            };
            #endregion

            #region Act
            ClearAllData();

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

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonObjectsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonRelationshipsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.DeleteHorizonServerUnsyncConceptsAsync
                    = async (client) =>
                    {
                        await Task.Delay(0);
                    };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
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

                for (int batch = 1; batch <= BatchCount; batch++)
                {
                    var data = GenerateRelationshipsWithMultipleConnections(true, batch);
                    try
                    {
                        timer.Start();
                        HorizonService.SyncPublishChanges(data, modified, new ResolvedObject[] { }, DataSourceId, false);
                        timer.Stop();
                        WriteLog($"Relationships with multiple connections for batch = {batch} and BatchCount = {BatchCount} was published.\n");
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format(ex.Message + "(BatchCount = {0}/{1}, BatchItems = {2})", batch, BatchCount, BatchItems);
                        WriteLog(logMessage);
                    }                    
                }

                var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
                PublishDataTime = averageTime.ToString(OutPutTimeFormat);

                WriteLog(" ************************************************************************************************************ ");
                string message = "Publish " + BatchCount + " batch of multiple connected relationships in average " +
                                 averageTime.ToString(OutPutTimeFormat);                

                WriteLog(message);
                WriteLog(" ************************************************************************************************************ ");
            }
            #endregion            
        }

        [TestMethod]
        public void PublishRelatedDocumentsWithSingleConnections()
        {            
            #region Assign
            timer = new Stopwatch();

            var modified = new ModifiedConcepts
            {
                DeletedMedias = new List<KMedia>(),
                ModifiedProperties = new List<ModifiedProperty>()
            };
            #endregion

            #region Act
            ClearAllData();

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

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonObjectsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonRelationshipsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.DeleteHorizonServerUnsyncConceptsAsync
                    = async (client) =>
                    {
                        await Task.Delay(0);
                    };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
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

                for (int batch = 1; batch <= BatchCount; batch++)
                {
                    var data = GenerateRelatedDocumentsWithSingleConnections(false, batch);

                    try
                    {
                        timer.Start();
                        HorizonService.SyncPublishChanges(data, modified, new ResolvedObject[] { }, DataSourceId, false);
                        timer.Stop();
                        WriteLog($"Related documents with single connections for batch = {batch} and BatchCount = {BatchCount} was published.\n");
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format(ex.Message + "(BatchCount = {0}/{1}, BatchItems = {2})", batch, BatchCount, BatchItems);
                        WriteLog(logMessage);
                    }                    
                }

                var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
                PublishDataTime = averageTime.ToString(OutPutTimeFormat);

                WriteLog(" ************************************************************************************************************ ");
                string message = "Publish " + BatchCount + " batch of single connected document in average " +
                                 averageTime.ToString(OutPutTimeFormat);

                WriteLog(message);
                WriteLog(" ************************************************************************************************************ ");
            }
            #endregion            
        }

        [TestMethod]
        public void PublishEvents()
        {            
            #region Assign
            timer = new Stopwatch();

            var modified = new ModifiedConcepts
            {
                DeletedMedias = new List<KMedia>(),
                ModifiedProperties = new List<ModifiedProperty>()
            };
            #endregion

            #region Act
            ClearAllData();
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

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonObjectsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.ApplyHorizonRelationshipsSynchronizationResultAsyncSynchronizationChanges
                    = async (client, changes) =>
                    {
                        await Task.Delay(0);
                    };

                Horizon.Access.DispatchService.Fakes.ShimInfrastructureServiceClient.AllInstances.DeleteHorizonServerUnsyncConceptsAsync
                    = async (client) =>
                    {
                        await Task.Delay(0);
                    };

                Ontology.Fakes.ShimOntology.AllInstances.GetBaseDataTypeOfPropertyString = (o, propertyTypeUri) =>
                {
                    if (allProperties.ContainsKey(propertyTypeUri))
                        return allProperties[propertyTypeUri];

                    return Ontology.BaseDataTypes.String;
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

                for (int batch = 1; batch <= BatchCount; batch++)
                {
                    var data = GenerateEvents(false, batch);

                    try
                    {
                        timer.Start();
                        HorizonService.SyncPublishChanges(data, modified, new ResolvedObject[] { }, DataSourceId, false);
                        timer.Stop();
                        WriteLog($"Events for batch = {batch} and BatchCount = {BatchCount} was published.\n");
                        OnTestCaseProgressChanged(new TestCaseProgressChangedEventArgs(BatchCount.ToString(), batch.ToString()));
                    }
                    catch (Exception ex)
                    {
                        string logMessage = string.Format(ex.Message + "(BatchCount = {0}/{1}, BatchItems = {2})", batch, BatchCount, BatchItems);
                        WriteLog(logMessage);
                    }
                }

                var averageTime = TimeSpan.FromSeconds(timer.Elapsed.TotalSeconds / BatchCount);
                PublishDataTime = averageTime.ToString(OutPutTimeFormat);

                WriteLog(" ************************************************************************************************************ ");
                string message = "Publish " + BatchCount + " batch of events in average " +
                                 averageTime.ToString(OutPutTimeFormat);

                WriteLog(message);
                WriteLog(" ************************************************************************************************************ ");
            }
            #endregion            
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
            int testsMethodCount = 4;

            //محاسبه تمام مراحل تست
            string totalStepsCount = (Math.Log10(endStore) * testsMethodCount + testsMethodCount).ToString(CultureInfo.InvariantCulture);

            int currentStepNumber = 0;

            var testResults = new List<LoadTestResult>
            {
                new LoadTestResult
                {
                    Title = "Publish single connected relationships",
                    Description = "Publish single connected relationships",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish multiple connected relationships",
                    Description = "Publish multi connected relationships",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish events",
                    Description = "Publish events",
                    Statistics = new DataTable()
                },
                new LoadTestResult
                {
                    Title = "Publish documents",
                    Description = "Publish documents",
                    Statistics = new DataTable()
                },                
            };

            //ایجاد ستون‌های جداول نتیجه تست‌ها
            foreach (var testResult in testResults)
            {
                testResult.Statistics.Columns.Add("Batch count");
                testResult.Statistics.Columns.Add("Publish");
                testResult.Statistics.Columns.Add("Clear all data");
            }

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

                for (int i = 0; i < testsMethodCount; i++)
                {
                    token.ThrowIfCancellationRequested();
                    currentStepNumber++;
                    OnStepsProgressChanged(new StepsProgressChangedEventArgs(totalStepsCount, currentStepNumber.ToString(),
                        testResults[i].Description, testResults));

                    try
                    {
                        switch (i)
                        {
                            case 0:
                                await Task.Run(() => PublishRelationshipsWithOneConnection());
                                break;
                            case 1:
                                await Task.Run(() => PublishRelationshipsWithMultipleConnections());
                                break;
                            case 2:
                                await Task.Run(() => PublishEvents());
                                break;
                            case 3:
                                await Task.Run(() => PublishRelatedDocumentsWithSingleConnections());
                                break;
                        }

                        rows[i]["Publish"] = PublishDataTime;
                    }
                    catch (Exception exception)
                    {
                        WriteLog(exception.Message);
                        rows[i]["Publish"] = exception.Message;
                    }
                    finally
                    {
                        await Task.Run(() => ClearAllData());
                        rows[i]["Clear all data"] = ClearAllDataTime;

                        testResults[i].Statistics.Rows.Add(rows[i]);
                    }
                }
            }

            return testResults;           
        }

        public event EventHandler<StepsProgressChangedEventArgs> StepsProgressChanged;

        public event EventHandler<TestCaseProgressChangedEventArgs> TestCaseProgressChanged;

        protected virtual void OnTestCaseProgressChanged(TestCaseProgressChangedEventArgs e)
        {
            TestCaseProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnStepsProgressChanged(StepsProgressChangedEventArgs e)
        {
            StepsProgressChanged?.Invoke(this, e);
        }
    }
}
