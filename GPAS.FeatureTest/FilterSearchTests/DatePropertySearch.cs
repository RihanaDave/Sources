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
    public class DatePropertySearch
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
        public async Task GetPublishedObjectDatePropertyByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string dateString = "7/10/1974 7:10:24 AM";
            string labelTypeUri = "label";
            DateTime propertyValue =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
            string propertyTypeUri = "تاریخ_ایجاد_کاربر_شبکه_اجتماعی";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, propertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newPersonLabel
                }
            });

            criteriaSet.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = propertyTypeUri,
                OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = DateTime.Parse(propertyValue.ToString())
                }
            });
            criteriaSet.SetOperator = BooleanOperator.All;

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = criteriaSet
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            properties.Add(kWPropertyLabel);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);
            // Act
          var temp =  await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
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
        public async Task GetPublishedObjectDateRangePropertyWithMonthCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string dateString = "7/10/1990";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            DateTime propertyValue =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            DateTime startTime = DateTime.Parse("6/10/1990", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("8/10/1990", System.Globalization.CultureInfo.InvariantCulture);

            
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()
                
            };

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(dateRangeCriteria);
            criteriaSet.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newPersonLabel
                }
            });

            criteriaSet.SetOperator = BooleanOperator.All;

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = criteriaSet
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);

        }

        [TestMethod]
        public async Task GetPublishedObjectDateRangePropertyWithYearCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            string propertyValue = "7/9/2001";

            DateTime startTime = DateTime.Parse("7/9/2000 7:10:24 AM", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("7/9/2002 7:10:24 AM", System.Globalization.CultureInfo.InvariantCulture);


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(dateRangeCriteria);
            criteriaSet.Criterias.Add(new PropertyValueCriteria()
            {
                PropertyTypeUri = labelTypeUri,
                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                {

                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = newPersonLabel
                }
            });

            criteriaSet.SetOperator = BooleanOperator.All;

            Query filterSearchQuery = new Query()
            {
                CriteriasSet = criteriaSet
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectDateRangePropertyWithDayCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            string propertyValue = "7/20/2000";

            DateTime startTime = DateTime.Parse("7/19/2000", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("7/21/2000", System.Globalization.CultureInfo.InvariantCulture);


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        dateRangeCriteria,
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =labelTypeUri,
                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                            {

                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = newPersonLabel
                            }
                        }
                    }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectDateRangePropertyWithHourCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            string propertyValue = "7/20/2000  7:10:24 AM";

            DateTime startTime = DateTime.Parse("7/20/2000  6:10:24 AM", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("7/20/2000  8:10:24 AM", System.Globalization.CultureInfo.InvariantCulture);


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        dateRangeCriteria,
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =labelTypeUri,
                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                            {

                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = newPersonLabel
                            }
                        }
                    }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectDateRangePropertyWithSecondCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            string propertyValue = "7/20/2000  7:10:24 AM";

            DateTime startTime = DateTime.Parse("7/20/2000  7:10:23 AM", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("7/20/2000  7:10:25 AM", System.Globalization.CultureInfo.InvariantCulture);


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        dateRangeCriteria,
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =labelTypeUri,
                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                            {

                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = newPersonLabel
                            }
                        }
                    }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetPublishedObjectDateRangePropertyWithMinuteCriteriaByPropertySearch()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string personTypeUri = "شخص";
            string PropertyTypeUri = "تاریخ_تولد";
            string labelTypeUri = "label";

            string propertyValue = "7/20/2000  7:10:24 AM";

            DateTime startTime = DateTime.Parse("7/20/2000  7:9:24 AM", System.Globalization.CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.Parse("7/20/2000  7:11:24 AM", System.Globalization.CultureInfo.InvariantCulture);


            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue.ToString());
            KWProperty kWPropertyLabel = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, labelTypeUri, newPersonLabel);

            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria()
            {
                StartTime = startTime.ToString(),
                EndTime = endTime.ToString()

            };
            Query filterSearchQuery = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        dateRangeCriteria,
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =labelTypeUri,
                            OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                            {

                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = newPersonLabel
                            }
                        }
                    }
                }
            };
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
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
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }


    }
}
