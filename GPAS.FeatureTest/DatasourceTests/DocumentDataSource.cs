using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.DataImport.DataMapping.Unstructured;
using GPAS.FeatureTest.DispatchTestServiceAccess.TestService;
using GPAS.Workspace.Logic;
using GPAS.Workspace.ViewModel.DataImport;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.FeatureTest.DatasourceTests
{
    [TestClass]
    public class DocumentDataSource
    {
        List<string> classifications = new List<string>();
        private string datasourceDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\";
        private int numberOfDataSourcesPerType = 10;
        string documentDataSourceFilePath = string.Empty;

        [TestInitialize]
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task Init()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            documentDataSourceFilePath = $"{datasourceDirectoryPath}documentDataSource.txt";

            foreach (var classification in Classification.EntriesTree)
            {
                classifications.Add(classification.IdentifierString);
            }
        }

        [TestMethod]
        public async Task CreateDocumentDataSourceForAdmin()
        {
            //Assign
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync("admin", "admin");
            await Workspace.Logic.System.InitializationAsync();

            FileInfo fi = new FileInfo(documentDataSourceFilePath);
            string fileExtension = documentDataSourceFilePath.Substring(documentDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = documentDataSourceFilePath,
            };

            UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
            {
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                relatedMaterialBase = null,
                relatedItemToImport = itemToImport
            };

            itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

            ObjectMapping objectToAdd = new ObjectMapping
                    (fi.Extension.TrimStart(new char[] { '.' }).ToUpper(), fi.Name);
            TypeMapping typeMapping = new TypeMapping();
            typeMapping.AddObjectMapping(objectToAdd);


            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);


            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };

                await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            }

            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual("admin", allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateDocumentDataSourceForUserWithOwnerPermission()
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
                        AccessLevel = Permission.Owner
                    },
                    new ACI()
                    {
                        GroupName = NativeGroup.Administrators.ToString(),
                        AccessLevel = Permission.Owner
                    }
                }
            };

            FileInfo fi = new FileInfo(documentDataSourceFilePath);
            string fileExtension = documentDataSourceFilePath.Substring(documentDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = documentDataSourceFilePath,
            };

            UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
            {
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                relatedMaterialBase = null,
                relatedItemToImport = itemToImport
            };

            itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

            ObjectMapping objectToAdd = new ObjectMapping
                    (fi.Extension.TrimStart(new char[] { '.' }).ToUpper(), fi.Name);
            TypeMapping typeMapping = new TypeMapping();
            typeMapping.AddObjectMapping(objectToAdd);


            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            }

            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
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

            FileInfo fi = new FileInfo(documentDataSourceFilePath);
            string fileExtension = documentDataSourceFilePath.Substring(documentDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = documentDataSourceFilePath,
            };

            UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
            {
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                relatedMaterialBase = null,
                relatedItemToImport = itemToImport
            };

            itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

            ObjectMapping objectToAdd = new ObjectMapping
                    (fi.Extension.TrimStart(new char[] { '.' }).ToUpper(), fi.Name);
            TypeMapping typeMapping = new TypeMapping();
            typeMapping.AddObjectMapping(objectToAdd);


            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            }

            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(1, allDataSources.Where(d => d.Description == constDescription).Count());
            Assert.AreEqual(userName, allDataSources.Where(d => d.Description == constDescription).First().CreatedBy);
        }

        [TestMethod]
        public async Task CreateDocumentDataSourceForUserWithReadPermission()
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

            FileInfo fi = new FileInfo(documentDataSourceFilePath);
            string fileExtension = documentDataSourceFilePath.Substring(documentDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = documentDataSourceFilePath,
            };

            UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
            {
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                relatedMaterialBase = null,
                relatedItemToImport = itemToImport
            };

            itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

            ObjectMapping objectToAdd = new ObjectMapping
                    (fi.Extension.TrimStart(new char[] { '.' }).ToUpper(), fi.Name);
            TypeMapping typeMapping = new TypeMapping();
            typeMapping.AddObjectMapping(objectToAdd);


            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);

            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            }

            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
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

            FileInfo fi = new FileInfo(documentDataSourceFilePath);
            string fileExtension = documentDataSourceFilePath.Substring(documentDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = documentDataSourceFilePath,
            };

            UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
            {
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                relatedMaterialBase = null,
                relatedItemToImport = itemToImport
            };

            itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

            ObjectMapping objectToAdd = new ObjectMapping
                    (fi.Extension.TrimStart(new char[] { '.' }).ToUpper(), fi.Name);
            TypeMapping typeMapping = new TypeMapping();
            typeMapping.AddObjectMapping(objectToAdd);


            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);


            string constDescription = Guid.NewGuid().ToString();
            using (ShimsContext.Create())
            {
                DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                {
                    return constDescription;
                };
                await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            }

            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.Document, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            Assert.AreEqual(0, allDataSources.Where(d => d.Description == constDescription).Count());
        }
    }
}
