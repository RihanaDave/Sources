using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.GlobalResolve;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.Sync;
using GPAS.SearchServer.Logic.Synchronization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Tests
{
    [TestClass()]
    public class DataSynchronizerTests
    {
        public static bool IsInit = false;

        [TestInitialize]
        public void Init()
        {
            if (!IsInit)
            {
                InitProvider.Init();
                IsInit = true;
            }
        }

        // به خاطر حذف داده‌ها پس از اتمام تست، غیرفعال شد
        //[TestCleanup]
        public void Cleanup()
        {
            // Delete all document from solr after test...
            SearchEngineProvider.GetNewDefaultSearchEngineClient().DeleteAllDocuments();
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت و حذف داده‌ها پس از اتمام تست، غیرفعال شد
        //[TestMethod()]
        public async Task SynchronizeTest()
        {
            RetrieveDataClient retrieveDataClient = new RetrieveDataClient();
            List<SearchObject> searchObjects = await retrieveDataClient.RetrieveObjectsSequentialByIDRangeAsync(1, 5);
            if (searchObjects.Count == 0)
                return;

            //arrange
            List<ModifiedProperty> ModifiedProperties = new List<ModifiedProperty>();
            var t = new List<long>();
            t.Add(searchObjects.ElementAt(3).Id);
            List<AccessControled<SearchProperty>> SearchProperty1 = retrieveDataClient.RetrievePropertiesOfObjects(t);
            foreach (var item in SearchProperty1.Select(acsp => acsp.ConceptInstance))
            {
                ModifiedProperties.Add(
                new ModifiedProperty()
                {
                    ID = item.Id,
                    TypeUri = item.TypeUri,
                    newValue = $"changed {item.Value}"
                });
            }

            List<long> t1 = new List<long>();
            t1.Add(searchObjects.ElementAt(4).Id);
            List<AccessControled<SearchProperty>> SearchProperty2 = retrieveDataClient.RetrievePropertiesOfObjects(t1);
            foreach (var item in SearchProperty2.Select(acsp => acsp.ConceptInstance))
            {
                ModifiedProperties.Add(
                new ModifiedProperty()
                {
                    ID = item.Id,
                    TypeUri = item.TypeUri,
                    newValue = $"changed {item.Value}"
                });
            }

            var addedConcepts = new AddedConcepts()
            {
                AddedObjects = new SearchObject[] { },
                AddedProperties = new SearchProperty[] { },
                AddedRelationships = new SearchRelationship[] { },
                AddedMedias = new SearchMedia[] { }
            };
            var modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = ModifiedProperties.ToArray(),
                DeletedMedias = new List<SearchMedia>()
            };

            //act
            IndexingProvider indexingProvider = new IndexingProvider();
            await indexingProvider.SynchronizePublishChanges(addedConcepts, modifiedConcepts, new List<ResolveMasterObject>(), 1200);

            // assert
            Assert.IsTrue(modifiedConcepts.ModifiedProperties.Length == (SearchProperty1.Count + SearchProperty2.Count));
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت و حذف داده‌ها پس از اتمام تست، غیرفعال شد
        //[TestMethod()]
        public async Task GetResolutionCandidatesTest()
        {
            // arrange 
            List<SearchObject> AddedObjectsList = new List<SearchObject>();
            List<SearchProperty> AddedPropertiesList = new List<SearchProperty>();

            SearchObject searchDBObject1 = new SearchObject()
            {
                LabelPropertyID = 1,
                Id = -1,
                TypeUri = "شخص"
            };

            SearchObject searchDBObject2 = new SearchObject()
            {
                LabelPropertyID = 2,
                Id = -2,
                TypeUri = "شخص"
            };

            SearchObject searchDBObject3 = new SearchObject()
            {
                LabelPropertyID = 3,
                Id = -3,
                TypeUri = "شخص"
            };

            SearchObject searchDBObject4 = new SearchObject()
            {
                LabelPropertyID = 4,
                Id = -4,
                TypeUri = "شخص"
            };
            SearchObject searchDBObject5 = new SearchObject()
            {
                LabelPropertyID = 5,
                Id = -5,
                TypeUri = "سازمان"
            };
            AddedObjectsList.Add(searchDBObject1);
            AddedObjectsList.Add(searchDBObject2);
            AddedObjectsList.Add(searchDBObject3);
            AddedObjectsList.Add(searchDBObject4);
            AddedObjectsList.Add(searchDBObject5);

            SearchProperty searchDBProperty1 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "123456789",
                OwnerObject = searchDBObject1
            };

            SearchProperty searchDBProperty2 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "987654321",
                OwnerObject = searchDBObject2
            };

            SearchProperty searchDBProperty3 = new SearchProperty()
            {
                TypeUri = "سن",
                Value = "77",
                OwnerObject = searchDBObject2
            };

            SearchProperty searchDBProperty4 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "123456789",
                OwnerObject = searchDBObject3
            };
            SearchProperty searchDBProperty5 = new SearchProperty()
            {
                TypeUri = "نام_خانوادگی",
                Value = "jafari",
                OwnerObject = searchDBObject3
            };
            SearchProperty searchDBProperty6 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "53719",
                OwnerObject = searchDBObject4
            };

            SearchProperty searchDBProperty7 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "123456789",
                OwnerObject = searchDBObject5
            };

            ACL acl = new ACL()
            {
                Classification = "R",
                Permissions = new List<ACI>()
                {
                    new ACI() { GroupName = AccessControl.Groups.NativeGroup.Administrators.ToString(), AccessLevel = Permission.Owner },
                    new ACI() { GroupName = AccessControl.Groups.NativeGroup.EveryOne.ToString(), AccessLevel = Permission.Read }
                }
            };

            AddedPropertiesList.Add(searchDBProperty1);
            AddedPropertiesList.Add(searchDBProperty2);
            AddedPropertiesList.Add(searchDBProperty3);
            AddedPropertiesList.Add(searchDBProperty4);
            AddedPropertiesList.Add(searchDBProperty5);
            AddedPropertiesList.Add(searchDBProperty6);
            AddedPropertiesList.Add(searchDBProperty7);
            
            var addedConcepts = new AddedConcepts()
            {
                AddedObjects = AddedObjectsList.ToArray(),
                AddedProperties = AddedPropertiesList.ToArray(),
                AddedRelationships = new SearchRelationship[] { },
                AddedMedias = new SearchMedia[] { }
            };
            var modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = new ModifiedProperty[] { },
                DeletedMedias = new List<SearchMedia>()
            };

            // Add New Objects For Test.
            IndexingProvider indexingProvider = new IndexingProvider();
            await indexingProvider.SynchronizePublishChanges(addedConcepts, modifiedConcepts, new List<ResolveMasterObject>(), 1200);

            List<LinkingProperty> linkingProperties = new List<LinkingProperty>();
            LinkingProperty linkingProperty1 = new LinkingProperty()
            {
                resolutionOption = ResolutionOption.ExactMatch,
                typeURI = "شماره_ملی"
            };
            LinkingProperty linkingProperty2 = new LinkingProperty()
            {
                resolutionOption = ResolutionOption.NoConflict,
                typeURI = "نام_خانوادگی"
            };
            linkingProperties.Add(linkingProperty1);
            linkingProperties.Add(linkingProperty2);

            List<ImportingObject> importingObjects = new List<ImportingObject>();

            // First importing object.
            List<ImportingProperty> properties = new List<ImportingProperty>();
            ImportingProperty importingProperty1 = new ImportingProperty()
            {
                TypeURI = "شماره_ملی",
                Value = "123456789"
            };
            ImportingProperty importingProperty2 = new ImportingProperty()
            {
                TypeURI = "نام_خانوادگی",
                Value = "jafari"
            };

            properties.Add(importingProperty1);
            properties.Add(importingProperty2);
            ImportingObject importingObject1 = new ImportingObject()
            {
                LabelProperty = new ImportingProperty("Label", "testimport1"),
                TypeUri = "شخص",
                Properties = properties
            };

            // Second importing object.
            List<ImportingProperty> properties1 = new List<ImportingProperty>();
            ImportingProperty importingProperty3 = new ImportingProperty()
            {
                TypeURI = "شماره_ملی",
                Value = "987654321"
            };

            properties1.Add(importingProperty3);
            ImportingObject importingObject2 = new ImportingObject()
            {
                LabelProperty = new ImportingProperty("Label", "testimport2"),
                TypeUri = "شخص",
                Properties = properties1
            };
            importingObjects.Add(importingObject1);
            importingObjects.Add(importingObject2);

            //act  
            GlobalResolutionCandidatesProvider globalResolutionCandidatesProvider = new GlobalResolutionCandidatesProvider();
            var result = globalResolutionCandidatesProvider.GetTypeBasedResolutionCandidates(linkingProperties.ToArray(), importingObjects.ToArray());

            // assert  
            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual(result[0].Master.LabelProperty.Value, importingObject1.LabelProperty.Value);
            Assert.IsTrue(result[0].ResolutionCandidates.Count() == 2);
            Assert.IsTrue(result[0].ResolutionCandidates.Select(c => c.CandidateID).Contains(-1));
            Assert.IsTrue(result[0].ResolutionCandidates.Select(c => c.CandidateID).Contains(-3));
            Assert.IsTrue(result[1].ResolutionCandidates.Select(c => c.CandidateID).Contains(-2));
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت و حذف داده‌ها پس از اتمام تست، غیرفعال شد
        //[TestMethod()]
        public async Task GetResolutionCandidatesNotConflictFunctionalityNullTest()
        {
            // arrange 
            List<SearchObject> AddedObjectsList = new List<SearchObject>();
            List<SearchProperty> AddedPropertiesList = new List<SearchProperty>();

            SearchObject searchDBObject1 = new SearchObject()
            {
                LabelPropertyID = 6,
                Id = -6,
                TypeUri = "شخص"
            };
            AddedObjectsList.Add(searchDBObject1);
            SearchProperty searchDBProperty1 = new SearchProperty()
            {
                TypeUri = "شماره_ملی",
                Value = "147258",
                OwnerObject = searchDBObject1
            };
            SearchProperty searchDBProperty2 = new SearchProperty()
            {
                TypeUri = "نام_خانوادگی",
                Value = "Carry",
                OwnerObject = searchDBObject1
            };

            ACL acl = new ACL()
            {
                Classification = "R",
                Permissions = new List<ACI>()
                {
                    new ACI() { GroupName = AccessControl.Groups.NativeGroup.Administrators.ToString(), AccessLevel = Permission.Owner },
                    new ACI() { GroupName = AccessControl.Groups.NativeGroup.EveryOne.ToString(), AccessLevel = Permission.Read }
                }
            };

            AddedPropertiesList.Add(searchDBProperty1);
            AddedPropertiesList.Add(searchDBProperty2);
            
            var addedConcepts = new AddedConcepts()
            {
                AddedObjects = AddedObjectsList.ToArray(),
                AddedProperties = AddedPropertiesList.ToArray(),
                AddedRelationships = new SearchRelationship[] { },
                AddedMedias = new SearchMedia[] { }
            };
            var modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = new ModifiedProperty[] { },
                DeletedMedias = new List<SearchMedia>()
            };

            // Add New Objects For Test.
            IndexingProvider indexingProvider = new IndexingProvider();
            await indexingProvider.SynchronizePublishChanges(addedConcepts, modifiedConcepts, new List<ResolveMasterObject>(), 1200);

            List<LinkingProperty> linkingProperties = new List<LinkingProperty>();
            LinkingProperty linkingProperty1 = new LinkingProperty()
            {
                resolutionOption = ResolutionOption.ExactMatch,
                typeURI = "شماره_ملی"
            };
            LinkingProperty linkingProperty2 = new LinkingProperty()
            {
                resolutionOption = ResolutionOption.NoConflict,
                typeURI = "نام_خانوادگی"
            };
            linkingProperties.Add(linkingProperty1);
            linkingProperties.Add(linkingProperty2);

            List<ImportingObject> importingObjects = new List<ImportingObject>();

            // First importing object.
            List<ImportingProperty> properties = new List<ImportingProperty>();
            ImportingProperty importingProperty1 = new ImportingProperty()
            {
                TypeURI = "شماره_ملی",
                Value = "147258"
            };
            ImportingProperty importingProperty2 = new ImportingProperty()
            {
                TypeURI = "نام_خانوادگی",
                Value = "Terry"
            };

            properties.Add(importingProperty1);
            properties.Add(importingProperty2);
            ImportingObject importingObject1 = new ImportingObject()
            {
                LabelProperty = new ImportingProperty("Label", "testimport3"),
                TypeUri = "شخص",
                Properties = properties
            };
            importingObjects.Add(importingObject1);

            //act  
            GlobalResolutionCandidatesProvider globalResolutionCandidatesProvider = new GlobalResolutionCandidatesProvider();
            var result = globalResolutionCandidatesProvider.GetTypeBasedResolutionCandidates(linkingProperties.ToArray(), importingObjects.ToArray());

            // assert  
            Assert.IsTrue(result[0].ResolutionCandidates.Length == 0);
        }
    }
}