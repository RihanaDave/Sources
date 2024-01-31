using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.FeatureTest.DispatchTestServiceAccess.TestService;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.DataImport;
using GPAS.Workspace.ViewModel.DataImport;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.DatasourceTests
{
    [TestClass]
    public class CsvFileDataSource
    {
        List<string> classifications = new List<string>();
        private string datasourceDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\";
        private int numberOfDataSourcesPerType = 10;
        TypeMapping loadedMapping = null;
        string csvDataSourceFilePath = string.Empty;
        string csvDataSourceMapFilePath = string.Empty;

        [TestInitialize]
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task Init()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            csvDataSourceFilePath = $"{datasourceDirectoryPath}csvDataSource.CSV";
            csvDataSourceMapFilePath = $"{datasourceDirectoryPath}csvDataSourceMap.imm";
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            loadedMapping = serializer.DeserializeFromFile(csvDataSourceMapFilePath);


            foreach (var classification in Classification.EntriesTree)
            {
                classifications.Add(classification.IdentifierString);
            }
        }

        [TestMethod]
        public async Task CreateCsvFileDataSourceForAdmin()
        {
            //Assign
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync("admin", "admin");
            await Workspace.Logic.System.InitializationAsync();

            FileInfo fi = new FileInfo(csvDataSourceFilePath);
            string fileExtension = csvDataSourceFilePath.Substring(csvDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Csv;
            DataTable sheetPreview = new DataTable();
            CsvFileMaterial csvFileMaterial = new CsvFileMaterial()
            {
                FileJobSharePath = string.Empty,
                Separator = ','
            };

            CsvFileMaterialVM csvFileMaterialVM = new CsvFileMaterialVM(true, fi.Name, csvDataSourceFilePath, sheetPreview)
            {
                relatedMaterialBase = csvFileMaterial,
                relatedItemToImport = itemToImport,
                
            };

            TransformationResult transformation = ImportProvider.PerformTransformation(csvFileMaterialVM, loadedMapping);

            DataSourceProvider dsProvider = new DataSourceProvider();
                       
            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(csvFileMaterialVM);
                WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            }
            
            //Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual("admin", allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateCsvFileDataSourceForUserWithOwnerPermission()
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

            UserAccountControlProvider.ImportACL= new AccessControl.ACL()
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

            FileInfo fi = new FileInfo(csvDataSourceFilePath);
            string fileExtension = csvDataSourceFilePath.Substring(csvDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Csv;
            DataTable sheetPreview = new DataTable();
            CsvFileMaterial csvFileMaterial = new CsvFileMaterial()
            {
                FileJobSharePath = string.Empty,
                Separator = ','
            };

            CsvFileMaterialVM csvFileMaterialVM = new CsvFileMaterialVM(true, fi.Name, csvDataSourceFilePath, sheetPreview)
            {
                relatedMaterialBase = csvFileMaterial,
                relatedItemToImport = itemToImport,
            };

            TransformationResult transformation = ImportProvider.PerformTransformation(csvFileMaterialVM, loadedMapping);

            DataSourceProvider dsProvider = new DataSourceProvider();

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(csvFileMaterialVM);
                WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateCsvFileDataSourceForUserWithWritePermission()
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

            UserAccountControlProvider.ImportACL = new AccessControl.ACL()
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

            FileInfo fi = new FileInfo(csvDataSourceFilePath);
            string fileExtension = csvDataSourceFilePath.Substring(csvDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Csv;
            DataTable sheetPreview = new DataTable();
            CsvFileMaterial csvFileMaterial = new CsvFileMaterial()
            {
                FileJobSharePath = string.Empty,
                Separator = ','
            };
            CsvFileMaterialVM csvFileMaterialVM = new CsvFileMaterialVM(true, fi.Name, csvDataSourceFilePath, sheetPreview)
            {
                relatedMaterialBase = csvFileMaterial,
                relatedItemToImport = itemToImport,

            };

            TransformationResult transformation = ImportProvider.PerformTransformation(csvFileMaterialVM, loadedMapping);

            DataSourceProvider dsProvider = new DataSourceProvider();

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(csvFileMaterialVM);
                WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateCsvFileDataSourceForUserWithReadPermission()
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

            UserAccountControlProvider.ImportACL = new AccessControl.ACL()
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

            FileInfo fi = new FileInfo(csvDataSourceFilePath);
            string fileExtension = csvDataSourceFilePath.Substring(csvDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Csv;
            DataTable sheetPreview = new DataTable();
            CsvFileMaterial csvFileMaterial = new CsvFileMaterial()
            {
                FileJobSharePath = string.Empty,
                Separator = ','
            };

            CsvFileMaterialVM csvFileMaterialVM = new CsvFileMaterialVM(true, fi.Name, csvDataSourceFilePath, sheetPreview)
            {
                relatedMaterialBase = csvFileMaterial,
                relatedItemToImport = itemToImport,

            };

            TransformationResult transformation = ImportProvider.PerformTransformation(csvFileMaterialVM, loadedMapping);

            DataSourceProvider dsProvider = new DataSourceProvider();

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(csvFileMaterialVM);
                WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateCsvFileDataSourceForUserWithoutPermission()
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

            FileInfo fi = new FileInfo(csvDataSourceFilePath);
            string fileExtension = csvDataSourceFilePath.Substring(csvDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Csv;
            DataTable sheetPreview = new DataTable();
            CsvFileMaterial csvFileMaterial = new CsvFileMaterial()
            {
                FileJobSharePath = string.Empty,
                Separator = ','
            };

            CsvFileMaterialVM csvFileMaterialVM = new CsvFileMaterialVM(true, fi.Name, csvDataSourceFilePath, sheetPreview)
            {
                relatedMaterialBase = csvFileMaterial,
                relatedItemToImport = itemToImport,

            };

            TransformationResult transformation = ImportProvider.PerformTransformation(csvFileMaterialVM, loadedMapping);

            DataSourceProvider dsProvider = new DataSourceProvider();

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(csvFileMaterialVM);
                WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.CsvFile, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(0, allDataSources.Where(d => d.Description == constDescription).Count());
        }
    }
}
