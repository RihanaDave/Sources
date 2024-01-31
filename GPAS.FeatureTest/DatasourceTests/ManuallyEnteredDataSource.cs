using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.FeatureTest.DispatchTestServiceAccess.TestService;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.FeatureTest.DatasourceTests
{
    [TestClass]
    public class ManuallyEnteredDataSource
    {
        List<string> classifications = new List<string>();
        private int numberOfDataSourcesPerType = 10;

        [TestInitialize]
        public void Init()
        {
            foreach (var classification in Classification.EntriesTree)
            {
                classifications.Add(classification.IdentifierString);
            }
        }

        [TestMethod]
        public async Task CreateManuallyEnteredDataSourceForAdmin()
        {
            //Assign
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync("admin", "admin");
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");

            unpublishedObjects.Add(newUnpublishPerson1);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                        new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            }
            //Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetAllDataSourcesAsync(string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ManuallyEntered, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual("admin", allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);

        }

        [TestMethod]
        public async Task CreateManuallyEnteredDataSourceForUserWithOwnerPermission()
        {
            //Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription = "g1";
            string groupCreatedBy = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            await proxy.CreateNewGroupAsync(groupName, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);

            await proxy.CreateNewMembershipAsync(groupName, userName);

            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, userNamePass);
            await Workspace.Logic.System.InitializationAsync();

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[0],
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

            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");

            unpublishedObjects.Add(newUnpublishPerson1);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                        new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetAllDataSourcesAsync(string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ManuallyEntered, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateManuallyEnteredDataSourceForUserWithWritePermission()
        {
            //Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription = "g1";
            string groupCreatedBy = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            await proxy.CreateNewGroupAsync(groupName, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);

            await proxy.CreateNewMembershipAsync(groupName, userName);

            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, userNamePass);
            await Workspace.Logic.System.InitializationAsync();

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[0],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName,
                        AccessLevel = Permission.Write
                    },
                    new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };

            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");

            unpublishedObjects.Add(newUnpublishPerson1);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                        new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetAllDataSourcesAsync(string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ManuallyEntered, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateManuallyEnteredDataSourceForUserWithReadPermission()
        {
            //Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription = "g1";
            string groupCreatedBy = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            await proxy.CreateNewGroupAsync(groupName, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);

            await proxy.CreateNewMembershipAsync(groupName, userName);

            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, userNamePass);
            await Workspace.Logic.System.InitializationAsync();

            UserAccountControlProvider.ManuallyEnteredDataACL = new AccessControl.ACL()
            {
                Classification = classifications[0],
                Permissions = new List<ACI>()
                {
                    new ACI()
                    {
                        GroupName = groupName,
                        AccessLevel = Permission.Read
                    },
                    new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };

            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");

            unpublishedObjects.Add(newUnpublishPerson1);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                        new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetAllDataSourcesAsync(string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ManuallyEntered, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateManuallyEnteredDataSourceForUserWithoutPermission()
        {
            //Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription = "g1";
            string groupCreatedBy = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            await proxy.CreateNewGroupAsync(groupName, groupDescription, groupCreatedBy, true);

            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);

            await proxy.CreateNewMembershipAsync(groupName, userName);

            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(userName, userNamePass);
            await Workspace.Logic.System.InitializationAsync();

            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");

            unpublishedObjects.Add(newUnpublishPerson1);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                        new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetAllDataSourcesAsync(string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ManuallyEntered, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(0, allDataSources.Where(d => d.Description == constDescription).Count());
        }
    }
}
