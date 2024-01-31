using GPAS.FilterSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.FilterSearchTests
{
    [TestClass]
    public class PropertySearch
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
        public async Task GetPublishedObjectStringPropertyWithEqualsOperationByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string propertyValue = $"{Guid.NewGuid().ToString()} irani";
            string PropertyTypeUri = "ملیت";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =PropertyTypeUri,
                            OperatorValuePair = new      StringPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = propertyValue
                            }
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
            List<KWObject> UnpublishedSearchResult
           = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, UnpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, UnpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectStringPropertyWithLikeOperationByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string guid = $"{Guid.NewGuid().ToString()}";
            string propertyValue = $"{guid} irani";
            string PropertyTypeUri = "ملیت";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =PropertyTypeUri,
                            OperatorValuePair = new      StringPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like,
                                CriteriaValue = guid
                            }
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
            List<KWObject> UnpublishedSearchResult
          = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(1, UnpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, UnpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectFloatPropertyByPropertySearch()
        {
            // Assign
            string labelTypeUri = "label";
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string properyTypUri = "قد";
            double propertyValue = 10.5;
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, properyTypUri, propertyValue.ToString());
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel.ToString());

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                       new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel
                                            }
                                        } ,
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri = properyTypUri,
                            OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair()
                            {
                                CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = (float)propertyValue,
                                EqualityPrecision= 0
                            }
                        }
                    }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                properties
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
        public async Task GetPublishedObjectIntPropertyLessThanByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string labelTypeUri = "label";
            string properyTypUri = "سن";
            long propertyValue = 25;
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, properyTypUri, propertyValue.ToString());
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel.ToString());

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                         new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel
                                            }
                                        },
                                        new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = properyTypUri,
                                            OperatorValuePair = new LongPropertyCriteriaOperatorValuePair()
                                            {
                                                CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan,
                                                CriteriaValue = propertyValue + 1
                                            }
                                        }
                    }
                }

            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                properties
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
        public async Task GetPublishedObjectIntPropertyGreaterThanByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string labelTypeUri = "label";
            string properyTypUri = "سن";
            int propertyValue = 25;
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, properyTypUri, propertyValue.ToString());
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel.ToString());

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                         new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel
                                            }
                                        },
                                        new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = properyTypUri,
                                            OperatorValuePair = new LongPropertyCriteriaOperatorValuePair()
                                            {
                                                CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan,
                                                CriteriaValue = propertyValue - 1
                                            }
                                        }
                    }
                }

            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                properties
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

    }
}
