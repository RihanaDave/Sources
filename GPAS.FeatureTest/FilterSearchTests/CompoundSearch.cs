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
    public class CompoundSearch
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
        public async Task GetPublishedObjectByCompoundObjectTypeAndPropertySearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newOrganization1 = $"{Guid.NewGuid().ToString()}Organization 1";
            string objectType1 = "شخص";
            string personPropertyName1 = "ملیت";
            string personPropertyValue1 = "us";

            string objectType2 = "سازمان";
            string organizationPropertyName1 = "نام";
            string organizationPropertyValue1 = "apple";
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            objectsTypeUris.Add(objectType1);
            objectsTypeUris.Add(objectType2);

            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(objectType2, newOrganization1);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, personPropertyName1, personPropertyValue1.ToString());
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, organizationPropertyName1, organizationPropertyValue1.ToString());
            KWProperty kWPropertyLabelForPerson = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1);
            KWProperty kWPropertyLabelForOrganization = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganization1);

            Query filterSearchQuery = new Query();

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(new ObjectTypeCriteria()
            {
                ObjectsTypeUri = objectsTypeUris
            });
            criteriaSet.SetOperator = BooleanOperator.Any;

            ContainerCriteria nastedCriteriaObjectType = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet
            };

            CriteriaSet criteriaSet1 = new CriteriaSet();
            criteriaSet1.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newPersonLabel1
                }
            });

            criteriaSet1.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = personPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue1
                }
            });

            criteriaSet1.Criterias.Add(nastedCriteriaObjectType);

            criteriaSet1.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria1 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet1
            };

            CriteriaSet criteriaSet2 = new CriteriaSet();
            criteriaSet2.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newOrganization1
                }
            });

            criteriaSet2.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = organizationPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = organizationPropertyValue1
                }
            });

            criteriaSet2.Criterias.Add(nastedCriteriaObjectType);
            criteriaSet2.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria2 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet2
            };

            CriteriaSet criteriaSet3 = new CriteriaSet();
            criteriaSet3.Criterias.Add(nastedCriteria1);
            criteriaSet3.Criterias.Add(nastedCriteria2);
            criteriaSet3.SetOperator = BooleanOperator.Any;

            filterSearchQuery.CriteriasSet = criteriaSet3;

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWPropertyLabelForPerson);
            properties.Add(kWPropertyLabelForOrganization);

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
        public async Task GetPublishedObjectByCompoundObjectTypeAndDatePropertySearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newOrganization1 = $"{Guid.NewGuid().ToString()}Organization 1";
            string objectType1 = "شخص";
            string personPropertyName1 = "ملیت";
            string personPropertyValue1 = "us";

            string dateString = "7/10/1990";
            string personPropertyName2 = "تاریخ_تولد";
            DateTime personPropertyValue2 =
               DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            DateTime startTime = DateTime.Parse("6/10/1990", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("8/11/1990", System.Globalization.CultureInfo.InvariantCulture);

            string objectType2 = "سازمان";
            string organizationPropertyName1 = "نام";
            string organizationPropertyValue1 = "apple";
            ObservableCollection<string> objectsTypeUris = new ObservableCollection<string>();
            objectsTypeUris.Add(objectType1);
            objectsTypeUris.Add(objectType2);

            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(objectType2, newOrganization1);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, personPropertyName1, personPropertyValue1.ToString());
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, personPropertyName2, personPropertyValue2.ToString());
            KWProperty kWProperty3 = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, organizationPropertyName1, organizationPropertyValue1.ToString());
            KWProperty kWPropertyLabelForPerson = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1);
            KWProperty kWPropertyLabelForOrganization = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganization1);

            Query filterSearchQuery = new Query();
            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(new ObjectTypeCriteria()
            {
                ObjectsTypeUri = objectsTypeUris
            });
            criteriaSet.SetOperator = BooleanOperator.Any;

            ContainerCriteria nastedCriteriaObjectType = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet
            };

            CriteriaSet criteriaSet1 = new CriteriaSet();
            criteriaSet1.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newPersonLabel1
                }
            });

            criteriaSet1.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = personPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue1
                }
            });

            criteriaSet1.Criterias.Add(nastedCriteriaObjectType);
            criteriaSet1.Criterias.Add(dateRangeCriteria);
            criteriaSet1.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria1 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet1
            };

            CriteriaSet criteriaSet2 = new CriteriaSet();
            criteriaSet2.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newOrganization1
                }
            });

            criteriaSet2.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = organizationPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = organizationPropertyValue1
                }
            });

            criteriaSet2.Criterias.Add(nastedCriteriaObjectType);
            criteriaSet2.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria2 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet2
            };

            CriteriaSet criteriaSet3 = new CriteriaSet();
            criteriaSet3.Criterias.Add(nastedCriteria1);
            criteriaSet3.Criterias.Add(nastedCriteria2);
            criteriaSet3.SetOperator = BooleanOperator.Any;

            filterSearchQuery.CriteriasSet = criteriaSet3;

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWProperty3);
            properties.Add(kWPropertyLabelForPerson);
            properties.Add(kWPropertyLabelForOrganization);

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
        public async Task GetPublishedObjectByKywordAndPropertySearch()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person1";
            string newOrganization1 = $"{Guid.NewGuid().ToString()}Organization1";
            string objectType1 = "شخص";
            string personPropertyName1 = "ملیت";
            string personPropertyValue1 = "us";

            string objectType2 = "سازمان";
            string organizationPropertyName1 = "نام";
            string organizationPropertyValue1 = "apple";
            List<string> objectsTypeUris = new List<string>();
            objectsTypeUris.Add(objectType1);
            objectsTypeUris.Add(objectType2);

            string labelTypeUri = "label";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(objectType2, newOrganization1);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, personPropertyName1, personPropertyValue1.ToString());
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, organizationPropertyName1, organizationPropertyValue1.ToString());
            KWProperty kWPropertyLabelForPerson = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel1);
            KWProperty kWPropertyLabelForOrganization = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, labelTypeUri, newOrganization1);

            Query filterSearchQuery = new Query();

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(new KeywordCriteria()
            {
                Keyword = newPersonLabel1
            });

            criteriaSet.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = personPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue1
                }
            });
            criteriaSet.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria1 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet
            };

            CriteriaSet criteriaSet1 = new CriteriaSet();
            criteriaSet1.Criterias.Add(new KeywordCriteria()
            {
                Keyword = newOrganization1
            });

            criteriaSet1.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = organizationPropertyName1,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = organizationPropertyValue1
                }
            });
            criteriaSet1.SetOperator = BooleanOperator.All;

            ContainerCriteria nastedCriteria2 = new ContainerCriteria()
            {
                CriteriaSet = criteriaSet1
            };

            CriteriaSet criteriaSet2 = new CriteriaSet();
            criteriaSet2.Criterias.Add(nastedCriteria1);
            criteriaSet2.Criterias.Add(nastedCriteria2);
            criteriaSet2.SetOperator = BooleanOperator.Any;

            filterSearchQuery.CriteriasSet = criteriaSet2;

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWPropertyLabelForPerson);
            properties.Add(kWPropertyLabelForOrganization);
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

    }
}
