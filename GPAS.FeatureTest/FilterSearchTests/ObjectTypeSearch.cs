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
    public class ObjectTypeSearch
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
        public async Task GetPublishedObjectByObjectTypeWithoutChildSearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType = "شخص";
            string PropertyTypeUri = "label";
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            objectsTypeUris.Add(objectType);
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType, newPersonLabel2);

            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, newPersonLabel1);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, PropertyTypeUri, newPersonLabel2);

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriterias =
                new ContainerCriteria()
                {
                    CriteriaSet = new CriteriaSet()
                    {
                        Criterias = new ObservableCollection<CriteriaBase>()
                        {
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = PropertyTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel1
                                            }
                                        }, new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = PropertyTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel2
                                            }
                                        }
                        },
                        SetOperator = BooleanOperator.Any
                    }
                };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                     new ObjectTypeCriteria()
                                        {
                                            ObjectsTypeUri = objectsTypeUris
                                        },
                     nastedCriterias
                },
                SetOperator = BooleanOperator.All
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);

            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishPerson2);
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
            Assert.AreEqual(2, UnpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, UnpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, UnpublishedSearchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);

            Assert.AreEqual(2, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, searchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);
        }

        [TestMethod]
        public async Task GetPublishedDifferentObjectByObjectTypeSearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newOrganization1 = $"{Guid.NewGuid().ToString()}Organization 1";
            string objectType1 = "شخص";
            string objectType2 = "سازمان";
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            objectsTypeUris.Add(objectType1);
            objectsTypeUris.Add(objectType2);

            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(objectType2, newOrganization1);
            KWProperty kWPropertyForPerson = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1.ToString());
            KWProperty kWPropertyForOrganization = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganization1.ToString());

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriterias3 =
                new ContainerCriteria()
                {
                    CriteriaSet = new CriteriaSet()
                    {
                        Criterias = new ObservableCollection<CriteriaBase>()
                        {
                            new ObjectTypeCriteria()
                                        {
                                            ObjectsTypeUri = objectsTypeUris
                                        }
                        },
                        SetOperator = BooleanOperator.Any
                    }
                };

            ContainerCriteria nastedCriterias1 =
                new ContainerCriteria()
                {
                    CriteriaSet = new CriteriaSet()
                    {
                        Criterias = new ObservableCollection<CriteriaBase>()
                        {
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel1
                                            }
                                        },
                            nastedCriterias3
                        },
                        SetOperator = BooleanOperator.All
                    }
                };

            ContainerCriteria nastedCriterias2 =
               new ContainerCriteria()
               {
                   CriteriaSet = new CriteriaSet()
                   {
                       Criterias = new ObservableCollection<CriteriaBase>()
                       {
                            new PropertyValueCriteria()
                                        {
                                          PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newOrganization1
                                            }
                                        },
                            nastedCriterias3
                       },
                       SetOperator = BooleanOperator.All
                   }
               };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                     nastedCriterias2,
                     nastedCriterias1
                },
                SetOperator = BooleanOperator.Any
            };

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWPropertyForPerson);
            properties.Add(kWPropertyForOrganization);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishOrganization1);
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
            Assert.AreEqual(2, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);

            Assert.AreEqual(2, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, searchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);
        }


        [TestMethod]
        public async Task GetPublishedObejectsBetweenManyObjectTypesByObjectTypeSearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person1";
            string newOrganization1 = $"{Guid.NewGuid().ToString()}Organization1";
            string objectType1 = "شخص";
            string objectType2 = "سازمان";
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            objectsTypeUris.Add(objectType1);
            objectsTypeUris.Add(objectType2);

            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(objectType2, newOrganization1);
            KWProperty kWPropertyLabelForPerson = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1);
            KWProperty kWPropertyLabelForOrganization = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganization1);

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriterias =
                new ContainerCriteria()
                {
                    CriteriaSet = new CriteriaSet()
                    {
                        Criterias = new ObservableCollection<CriteriaBase>()
                        {
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel1
                                            }
                                        }, new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = labelTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newOrganization1
                                            }
                                        }
                        },
                        SetOperator = BooleanOperator.Any
                    }
                };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                     new ObjectTypeCriteria()
                                        {
                                            ObjectsTypeUri = objectsTypeUris
                                        },
                     nastedCriterias
                },
                SetOperator = BooleanOperator.All
            };

            List<KWProperty> properties = new List<KWProperty>();
            //properties.Add(kWPropertyForPerson);
            //properties.Add(kWPropertyForOrganization);
            properties.Add(kWPropertyLabelForPerson);
            properties.Add(kWPropertyLabelForOrganization);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishOrganization1);
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
            Assert.AreEqual(2, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);

            Assert.AreEqual(2, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, searchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectByObjectTypeWithChildSearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType = "شخص";
            string PropertyTypeUri = "label";
            var ontology = OntologyProvider.GetOntology();
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            List<string> childs = ontology.GetAllChilds("موجودیت");
            foreach (var item in childs)
            {
                objectsTypeUris.Add(item);
            }
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType, newPersonLabel2);
            KWProperty kWPropertyForPerson1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, newPersonLabel1.ToString());
            KWProperty kWPropertyForPerson2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, PropertyTypeUri, newPersonLabel2.ToString());

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriterias =
                new ContainerCriteria()
                {
                    CriteriaSet = new CriteriaSet()
                    {
                        Criterias = new ObservableCollection<CriteriaBase>()
                        {
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = PropertyTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel1
                                            }
                                        }, new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = PropertyTypeUri,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = newPersonLabel2
                                            }
                                        }
                        },
                        SetOperator = BooleanOperator.Any
                    }
                };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                     new ObjectTypeCriteria()
                                        {
                                            ObjectsTypeUri = objectsTypeUris
                                        },
                     nastedCriterias
                },
                SetOperator = BooleanOperator.All
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWPropertyForPerson1);
            properties.Add(kWPropertyForPerson2);

            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishPerson2);
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
            Assert.AreEqual(2, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);

            Assert.AreEqual(2, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, searchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);
        }

    }
}
