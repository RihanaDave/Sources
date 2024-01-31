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
    public class NastedSearch
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
        public async Task GetPublishedObjectByOneLevelNestedSearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string propertyValueOfPerson = "irani";
            string propertyTypeUriOfPerson = "ملیت";
            string labelTypeUri = "label";

            string newOrganizationLabel = $"{Guid.NewGuid().ToString()}Organization 1";
            string organizationTypeUri = "سازمان";
            string propertyValueOfOrganization = "ir";
            string PropertyTypeUriOfOrganization = "نام";

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, propertyTypeUriOfPerson, propertyValueOfPerson);
            KWProperty kWPropertyLable1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(organizationTypeUri, newOrganizationLabel);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, PropertyTypeUriOfOrganization, propertyValueOfOrganization);
            KWProperty kWPropertyLable2 = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganizationLabel);



            Query filterSearchQuery = new Query();

            ObjectTypeCriteria objectTypeCriteria = new ObjectTypeCriteria();
            objectTypeCriteria.ObjectsTypeUri.Add(personTypeUri);

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
                                                CriteriaValue = newPersonLabel
                                            }
                                        }, new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = propertyTypeUriOfPerson,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = propertyValueOfPerson
                                            }
                                        },
                             objectTypeCriteria
                        },
                        SetOperator = BooleanOperator.All
                    }
                };

            ObjectTypeCriteria objectTypeCriteria1 = new ObjectTypeCriteria();
            objectTypeCriteria1.ObjectsTypeUri.Add(organizationTypeUri);

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
                                                CriteriaValue = newOrganizationLabel
                                            }
                                        }, new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = PropertyTypeUriOfOrganization,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = propertyValueOfOrganization
                                            }
                                        },
                             objectTypeCriteria1
                       },
                       SetOperator = BooleanOperator.All
                   }
               };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                      nastedCriterias1,
                      nastedCriterias2
                },
                SetOperator = BooleanOperator.Any
            };

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWPropertyLable1);
            properties.Add(kWPropertyLable2);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishOrganization1);

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
            Assert.AreEqual(2, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);

            Assert.AreEqual(2, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishOrganization1.ID, searchResult.Where(o => o.ID == newUnpublishOrganization1.ID).FirstOrDefault().ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectByANDOpretionNestedSearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string labelTypeUri = "label";
            string property1TypeUriOfPerson = "ملیت";
            string property1ValueOfPerson = "irani";

            string property2TypeUriOfPerson = "نام";
            string property2ValueOfPerson = "ali";

            string property3TypeUriOfPerson = "قد";
            float property3ValueOfPerson = 170;


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, property1TypeUriOfPerson, property1ValueOfPerson);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, property2TypeUriOfPerson, property2ValueOfPerson);
            KWProperty kWProperty3 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, property3TypeUriOfPerson, property3ValueOfPerson.ToString());

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriteria1 =
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
                                                CriteriaValue = newPersonLabel
                                            }
                                        }
                       },
                       SetOperator = BooleanOperator.All
                   }
               };



            ContainerCriteria nastedCriteria2 = new ContainerCriteria()
            {
                CriteriaSet = new CriteriaSet()
                {
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        nastedCriteria1 ,
                        new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = property1TypeUriOfPerson,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = property1ValueOfPerson
                                            }
                                        }
                    },
                    SetOperator = BooleanOperator.All
                }
            };

            ContainerCriteria nastedCriteria3 = new ContainerCriteria()
            {
                CriteriaSet = new CriteriaSet()
                {
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        nastedCriteria2 ,
                        new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = property2TypeUriOfPerson,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = property2ValueOfPerson
                                            }
                                        }
                    },
                    SetOperator = BooleanOperator.All
                }
            };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                        nastedCriteria3,

                        new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = property3TypeUriOfPerson,
                                            OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = property3ValueOfPerson
                                            }
                                        }

                },
                SetOperator = BooleanOperator.All
            };

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWProperty3);
            properties.Add(kWPropertyLabel);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);

            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
        }


        [TestMethod]
        public async Task GetPublishedObjectByANYOpretionNestedSearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string labelTypeUri = "label";
            string propertyTypeUriOfPerson1 = "ملیت";
            string propertyValueOfPerson1 = "irani";

            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person2";
            string propertyTypeUriOfPerson2 = "نام";
            string propertyValueOfPerson2 = "ali";

            string newPersonLabel3 = $"{Guid.NewGuid().ToString()}Person 3";
            string propertyTypeUriOfPerson3 = "قد";
            float propertyValueOfPerson3 = 170;


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel2);
            KWObject newUnpublishPerson3 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel3);
            KWProperty kWPropertyLebel1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1);
            KWProperty kWPropertyLebel2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, labelTypeUri, newPersonLabel2);
            KWProperty kWPropertyLebel3 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson3, labelTypeUri, newPersonLabel3);

            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, propertyTypeUriOfPerson1, propertyValueOfPerson1);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, propertyTypeUriOfPerson2, propertyValueOfPerson2);
            KWProperty kWProperty3 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson3, propertyTypeUriOfPerson3, propertyValueOfPerson3.ToString());

            Query filterSearchQuery = new Query();

            ContainerCriteria nastedCriteria1 =
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
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = propertyTypeUriOfPerson1,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = propertyValueOfPerson1
                                            }
                                        }
                       },
                       SetOperator = BooleanOperator.All
                   }
               };

            ContainerCriteria nastedCriteria2 =
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
                                                CriteriaValue = newPersonLabel2
                                            }
                                        },
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = propertyTypeUriOfPerson2,
                                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = propertyValueOfPerson2
                                            }
                                        }
                       },
                       SetOperator = BooleanOperator.All
                   }
               };

            ContainerCriteria nastedCriteria3 =
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
                                                CriteriaValue = newPersonLabel3
                                            }
                                        },
                             new PropertyValueCriteria()
                                        {
                                            PropertyTypeUri = propertyTypeUriOfPerson3,
                                            OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair
                                            {
                                                CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                                CriteriaValue = propertyValueOfPerson3
                                            }
                                        }
                      },
                      SetOperator = BooleanOperator.All
                  }
              };

            filterSearchQuery.CriteriasSet = new CriteriaSet()
            {
                Criterias = new ObservableCollection<CriteriaBase>()
                {
                    nastedCriteria1,
                    nastedCriteria2,
                    nastedCriteria3
                },
                SetOperator = BooleanOperator.Any
            };

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWProperty3);

            properties.Add(kWPropertyLebel1);
            properties.Add(kWPropertyLebel2);
            properties.Add(kWPropertyLebel3);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            kwObjects.Add(newUnpublishPerson2);
            kwObjects.Add(newUnpublishPerson3);

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
            Assert.AreEqual(3, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson3.ID, unpublishedSearchResult.Where(o => o.ID == newUnpublishPerson3.ID).FirstOrDefault().ID);

            Assert.AreEqual(3, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult.Where(o => o.ID == newUnpublishPerson1.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson2.ID, searchResult.Where(o => o.ID == newUnpublishPerson2.ID).FirstOrDefault().ID);
            Assert.AreEqual(newUnpublishPerson3.ID, searchResult.Where(o => o.ID == newUnpublishPerson3.ID).FirstOrDefault().ID);
        }

    }
}
