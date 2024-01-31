using System;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.FilterSearch;
using System.Linq;
using System.Collections.ObjectModel;

namespace GPAS.FeatureTest.FilterSearchTests
{
    [TestClass]
    public class KeywordSearch
    {
        private bool isInitialized = false;

        [TestInitialize]
        public async Task Init()
        {
            if (!isInitialized)
            {
                var authentication = new UserAccountControlProvider();
                bool result = await authentication.AuthenticateAsync("admin", "admin");
                await Workspace.Logic.System.InitializationAsync();
                isInitialized = true;
            }
        }

        [TestMethod]
        public async Task GetUnpublishedObjectByKeyword()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new KeywordCriteria()
                        {
                            Keyword = newPersonLabel
                        }
                    }
                }
            };
            // Act
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1, searchResult[0]);
        }

        [TestMethod]
        public async Task GetUnpublishedObjectWithLikeOperationByKeyword()
        {
            // Assign
            string guid = $"{Guid.NewGuid().ToString()}";
            string newPersonLabel = $"{guid} Person 1";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new KeywordCriteria()
                        {
                            Keyword = guid
                        }
                    }
                }
            };
            // Act
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1, searchResult[0]);
        }

        [TestMethod]
        public async Task GetPublishedObjectByKeyword()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person1";
            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel.ToString());
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new KeywordCriteria()
                        {
                            Keyword = newPersonLabel
                        }
                    }
                }
            };

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);

            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects, properties
                 , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                 );
            List<KWObject> unpublishedSearchResult
                         = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);

            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectStringPropertyWithLikeOperationByKeywordSearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string guid =  $"{ Guid.NewGuid().ToString() }";
            string propertyValue = $"{guid} name";
            string PropertyTypeUri = "نام";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                       new KeywordCriteria()
                        {
                            Keyword = guid
                        }
                }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                properties
                 , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                 );
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1, searchResult[0]);
        }

        [TestMethod]
        public async Task GetPublishedObjectStringPropertyWithEqaulOperationByKeyword()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string propertyValue = $"{ Guid.NewGuid().ToString() } name";
            string PropertyTypeUri = "نام";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                       new KeywordCriteria()
                        {
                            Keyword = propertyValue
                        }
                }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                properties
                 , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                 );
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1, searchResult[0]);
        }

    }
}
