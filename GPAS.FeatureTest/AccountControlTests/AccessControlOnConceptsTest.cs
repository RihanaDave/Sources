using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.FeatureTest.DispatchTestServiceAccess.TestService;
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

namespace GPAS.FeatureTest.AccountControlTests
{
    [TestClass]
    public class AccessControlOnConceptsTest
    {
        private bool isInitialized = false;
        List<string> classifications = new List<string>();

        [TestInitialize]
        public void Init()
        {
            if (!isInitialized)
            {
                foreach (var classification in Classification.EntriesTree)
                {
                    classifications.Add(classification.IdentifierString);
                }

                isInitialized = true;
            }
        }

        [TestMethod]
        public async Task GetObjectsBySameGoupWithOneUserPublishedAndAnotherRetrieved()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName, userName);
            await proxy.CreateNewMembershipAsync(groupName, userName2);


            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

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

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[3],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };
            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);

            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetObjectsByTwoDiferentGroupAndUsers()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription = "g";
            string groupCreatedBy = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName1, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);


            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

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

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[3],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };

            // Act
           await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.AreEqual(0, unpublishedSearchResult.Count);

            Assert.AreEqual(0, searchResult.Count);
        }

        [TestMethod]
        public async Task GetObjectsByTwoDiferentGroupAndUsersWithWritePermision()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription = "g";
            string groupCreatedBy = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName1, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);


            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

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

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[3],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = groupName2,
                        AccessLevel = Permission.Write
                    }
                }
            };
            
            // Act
          await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

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
        public async Task GetObjectsByTwoDiferentGroupAndUsersWithReadPermision()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription = "g";
            string groupCreatedBy = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName1, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);


            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string propertyValue = $"{Guid.NewGuid().ToString()} irani";
            string PropertyTypeUri = "ملیت";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);
            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);

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

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[2],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = groupName2,
                        AccessLevel = Permission.Read
                    }
                }
            };

            // Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery, 10.ToString())).ToList();
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, unpublishedSearchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult[0].ID);

            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult[0].ID);
        }

        [TestMethod]
        public async Task GetObjectsByTwoDiferentGroupAndUsersWithWritePermistionOnPropery()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription = "g";
            string groupCreatedBy = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName1, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);
            
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string propertyValue = $"{Guid.NewGuid().ToString()} irani";
            string PropertyTypeUri = "ملیت";
            string namePropertyTypeUri = "نام";
            string namePropertyValue = $"{Guid.NewGuid().ToString()} ali";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, namePropertyTypeUri, namePropertyValue);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);
            List<KWProperty> properties1 = new List<KWProperty>();
            List<KWProperty> properties2 = new List<KWProperty>();
            properties1.Add(kWProperty2);
            properties2.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);

            Query filterSearchQuery1 = new Query()
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

            Query filterSearchQuery2 = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =namePropertyTypeUri,
                            OperatorValuePair = new      StringPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = namePropertyValue
                            }
                        }
                    }
                }
            };
            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[2],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = groupName2,
                        AccessLevel = Permission.Read
                    }
                }
            };
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties1
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );


            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[2],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(new List<KWObject>(),
                   properties2
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            // Act
            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult1 = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery1, 10.ToString())).ToList();
            List<KWObject> unpublishedSearchResult2 = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery2, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult1
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery1, 10.ToString())).ToList();
            List<KWObject> searchResult2
             = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery2, 10.ToString())).ToList();
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, unpublishedSearchResult1.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult1[0].ID);

            Assert.AreEqual(0, unpublishedSearchResult2.Count);
            Assert.AreEqual(0, searchResult2.Count);

        }

        [TestMethod]
        public async Task GetObjectsByTwoDiferentGroupAndUsersWithReadPermistionOnOneTwoPropery()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupName3 = $"g{Guid.NewGuid().ToString()}g3".Replace("-", "");
            string groupDescription = "g";
            string groupCreatedBy = "test feature";
            string userName = $"{Guid.NewGuid().ToString()}u1";
            string userName2 = $"{Guid.NewGuid().ToString()}u2";

            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            await proxy.CreateNewGroupAsync(groupName1, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription, groupCreatedBy, true);
            await proxy.CreateNewGroupAsync(groupName3, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);
            await proxy.CreateNewMembershipAsync(groupName3, userName2);

            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, "1");
            await Workspace.Logic.System.InitializationAsync();

            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person 1";
            string propertyValue = $"{Guid.NewGuid().ToString()}irani";
            string PropertyTypeUri = "ملیت";
            string namePropertyTypeUri = "نام";
            string namePropertyValue = $"{Guid.NewGuid().ToString()}ali";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, namePropertyTypeUri, namePropertyValue);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, PropertyTypeUri, propertyValue);
            List<KWProperty> properties1 = new List<KWProperty>();
            //List<KWProperty> properties2 = new List<KWProperty>();
            properties1.Add(kWProperty2);
            properties1.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newUnpublishPerson1);

            Query filterSearchQuery1 = new Query()
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

            Query filterSearchQuery2 = new Query()
            {
                CriteriasSet = new CriteriaSet()
                {
                    SetOperator = BooleanOperator.All,
                    Criterias = new ObservableCollection<CriteriaBase>()
                    {
                        new PropertyValueCriteria()
                        {
                            PropertyTypeUri =namePropertyTypeUri,
                            OperatorValuePair = new      StringPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = namePropertyValue
                            }
                        }
                    }
                }
            };
            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[2],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = groupName3,
                        AccessLevel = Permission.Read
                    }
                }
            };
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(kwObjects,
                   properties1
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );


            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[2],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName1,
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    },
                      new ACI()
                    {
                        GroupName = groupName2,
                        AccessLevel = Permission.Read
                    }
                }
            };
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(new List<KWObject>(),
                   properties1
                    , new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            // Act
            result = await authentication.AuthenticateAsync(userName2, "2");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedSearchResult1 = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery1, 10.ToString())).ToList();
            List<KWObject> unpublishedSearchResult2 = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery2, 10.ToString())).ToList();

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();

            List<KWObject> searchResult1
                = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery1, 10.ToString())).ToList();
            List<KWObject> searchResult2
             = (await Workspace.Logic.Search.FilterSearch.PerformFilterSearchAsync(filterSearchQuery2, 10.ToString())).ToList();
            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, unpublishedSearchResult1.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, unpublishedSearchResult1[0].ID);

            Assert.AreEqual(1, searchResult1.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult1[0].ID);

            Assert.AreEqual(1, unpublishedSearchResult2.Count);

            Assert.AreEqual(1, searchResult2.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, searchResult2[0].ID);
        }
    }
}
