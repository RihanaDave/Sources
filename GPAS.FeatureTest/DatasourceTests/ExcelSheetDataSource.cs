using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

namespace GPAS.FeatureTest.DatasourceTests
{
    [TestClass]
    public class ExcelSheetDataSource
    {
        List<string> classifications = new List<string>();
        private string datasourceDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\";
        private int ImportPreview_MaximumSampleRows = 10;
        private int numberOfDataSourcesPerType = 10;
        TypeMapping loadedMapping = null;
        string excelSheetDataSourceFilePath = string.Empty;
        string excelSheetDataSourceMapFilePath = string.Empty;

        [TestInitialize]
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task Init()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            excelSheetDataSourceFilePath = $"{datasourceDirectoryPath}excelDataSource.xlsx";
            excelSheetDataSourceMapFilePath = $"{datasourceDirectoryPath}excelDataSourceMap.imm";
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            loadedMapping = serializer.DeserializeFromFile(excelSheetDataSourceMapFilePath);

            foreach (var classification in Classification.EntriesTree)
            {
                classifications.Add(classification.IdentifierString);
            }
        }

        [TestMethod]
        public async Task CreateExcelFileDataSourceForAdmin()
        {
            //Assign
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync("admin", "admin");
            await Workspace.Logic.System.InitializationAsync();

            FileInfo fi = new FileInfo(excelSheetDataSourceFilePath);
            string fileExtension = excelSheetDataSourceFilePath.Substring(excelSheetDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();


            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Excel;
            itemToImport.ItemPath = excelSheetDataSourceFilePath;
            itemToImport.ItemName = fi.Name;

            var util = new GPAS.DataImport.Transformation.Utility();

            Dictionary<string, DataTable> excelSheetToDataTableMapping = null;
            await Task.Factory.StartNew(() =>
            {
                excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(itemToImport.ItemPath, ImportPreview_MaximumSampleRows, true);
            });

            string[] excelFileSheetsName = util.GetExcelFileSheets(itemToImport.ItemPath);
            foreach (var sheetName in excelFileSheetsName)
            {
                ExcelSheet sheetMaterial = new ExcelSheet()
                {
                    FileJobSharePath = string.Empty,
                    SheetName = sheetName
                };

                DataTable sheetPreview = null;
                if (excelSheetToDataTableMapping.ContainsKey(sheetName))
                {
                    sheetPreview = excelSheetToDataTableMapping[sheetName];
                }

                ExcelSheetMaterialVM sheetMaterialVM = new ExcelSheetMaterialVM(true, itemToImport.ItemName, itemToImport.ItemPath, sheetName, sheetPreview)
                {
                    relatedMaterialBase = sheetMaterial,
                    relatedItemToImport = itemToImport,
                    Icon = null
                };
                itemToImport.ItemMaterials.Add(sheetMaterialVM);
            }

            List<string> assignedDescriptions = new List<string>();
            using (ShimsContext.Create())
            {
                foreach (var currentMaterialBaseVM in itemToImport.ItemMaterials)
                {
                    SemiStructuredMaterialVM semiStructuredMaterialVM = currentMaterialBaseVM as SemiStructuredMaterialVM;
                    string constDescription = Guid.NewGuid().ToString();

                    DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                    {
                        return constDescription;
                    };

                    assignedDescriptions.Add(constDescription);

                    DataSourceProvider dsProvider = new DataSourceProvider();
                    DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(semiStructuredMaterialVM);
                    TransformationResult transformation = ImportProvider.PerformTransformation(semiStructuredMaterialVM.Preview, loadedMapping);
                    WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                               (transformation.GeneratingObjects, transformation.GeneratingRelationships
                               , dataSource);
                }
            }


            //Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            foreach (var currentDescription in assignedDescriptions)
            {
                Assert.AreEqual(1, allDataSources.Where(d => assignedDescriptions.Contains(d.Description)).Count());
                Assert.AreEqual("admin", allDataSources.Where(d => assignedDescriptions.Contains(d.Description)).First().CreatedBy);
            }
        }


        [TestMethod]
        public async Task CreateExcelFileDataSourceForUserWithOwnerPermission()
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

            FileInfo fi = new FileInfo(excelSheetDataSourceFilePath);
            string fileExtension = excelSheetDataSourceFilePath.Substring(excelSheetDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();
            
            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Excel;
            itemToImport.ItemPath = excelSheetDataSourceFilePath;
            itemToImport.ItemName = fi.Name;

            var util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> excelSheetToDataTableMapping = null;
            await Task.Factory.StartNew(() =>
            {
                excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(itemToImport.ItemPath, ImportPreview_MaximumSampleRows, true);
            });
            foreach (var sheetName in excelSheetToDataTableMapping.Keys)
            {
                ExcelSheet sheetMaterial = new ExcelSheet()
                {
                    FileJobSharePath = string.Empty,
                    SheetName = sheetName
                };
                DataTable sheetPreview = null;
                if (true)
                {
                    sheetPreview = excelSheetToDataTableMapping[sheetName];
                }
                
                ExcelSheetMaterialVM sheetMaterialVM = new ExcelSheetMaterialVM(true, itemToImport.ItemName, itemToImport.ItemPath, sheetName, sheetPreview)
                {
                    relatedMaterialBase = sheetMaterial,
                    relatedItemToImport = itemToImport,
                    Icon = null
                };
                itemToImport.ItemMaterials.Add(sheetMaterialVM);
            }

            List<string> assignedDescriptions = new List<string>();
            using (ShimsContext.Create())
            {
                foreach (var currentMaterialBaseVM in itemToImport.ItemMaterials)
                {
                    SemiStructuredMaterialVM semiStructuredMaterialVM = currentMaterialBaseVM as SemiStructuredMaterialVM;
                    string constDescription = Guid.NewGuid().ToString();

                    DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                    {
                        return constDescription;
                    };

                    assignedDescriptions.Add(constDescription);

                    DataSourceProvider dsProvider = new DataSourceProvider();
                    DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(semiStructuredMaterialVM);
                    TransformationResult transformation = ImportProvider.PerformTransformation(semiStructuredMaterialVM.Preview, loadedMapping);
                    WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                               (transformation.GeneratingObjects, transformation.GeneratingRelationships
                               , dataSource);
                }
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            foreach (var currentDescription in assignedDescriptions)
            {
                Assert.AreEqual(1, allDataSources.Where(d => d.Description == currentDescription).Count());
                Assert.AreEqual(userName, allDataSources.Where(d => d.Description == currentDescription).First().CreatedBy);
            }
        }

        [TestMethod]
        public async Task CreateExcelFileDataSourceForUserWithWritePermission()
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

            FileInfo fi = new FileInfo(excelSheetDataSourceFilePath);
            string fileExtension = excelSheetDataSourceFilePath.Substring(excelSheetDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();
            
            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Excel;
            itemToImport.ItemPath = excelSheetDataSourceFilePath;
            itemToImport.ItemName = fi.Name;

            var util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> excelSheetToDataTableMapping = null;
            await Task.Factory.StartNew(() =>
            {
                excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(itemToImport.ItemPath, ImportPreview_MaximumSampleRows, true);
            });
            foreach (var sheetName in excelSheetToDataTableMapping.Keys)
            {
                ExcelSheet sheetMaterial = new ExcelSheet()
                {
                    FileJobSharePath = string.Empty,
                    SheetName = sheetName
                };
                DataTable sheetPreview = null;

                if (excelSheetToDataTableMapping.ContainsKey(sheetName))
                {
                    sheetPreview = excelSheetToDataTableMapping[sheetName];
                }
                ExcelSheetMaterialVM sheetMaterialVM = new ExcelSheetMaterialVM(true, itemToImport.ItemName, itemToImport.ItemPath, sheetName, sheetPreview)
                {
                    relatedMaterialBase = sheetMaterial,
                    relatedItemToImport = itemToImport,
                    Icon = null
                };
                itemToImport.ItemMaterials.Add(sheetMaterialVM);
            }

            List<string> assignedDescriptions = new List<string>();

            using (ShimsContext.Create())
            {
                foreach (var currentMaterialBaseVM in itemToImport.ItemMaterials)
                {
                    SemiStructuredMaterialVM semiStructuredMaterialVM = currentMaterialBaseVM as SemiStructuredMaterialVM;
                    string constDescription = Guid.NewGuid().ToString();

                    DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                    {
                        return constDescription;
                    };

                    assignedDescriptions.Add(constDescription);

                    DataSourceProvider dsProvider = new DataSourceProvider();
                    DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(semiStructuredMaterialVM);
                    TransformationResult transformation = ImportProvider.PerformTransformation(semiStructuredMaterialVM.Preview, loadedMapping);
                    WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                               (transformation.GeneratingObjects, transformation.GeneratingRelationships
                               , dataSource);
                }
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            foreach (var currentDescription in assignedDescriptions)
            {
                Assert.AreEqual(1, allDataSources.Where(d => d.Description == currentDescription).Count());
                Assert.AreEqual(userName, allDataSources.Where(d => d.Description == currentDescription).First().CreatedBy);
            }
        }


        [TestMethod]
        public async Task CreateExcelFileDataSourceForUserWithReadPermission()
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

            FileInfo fi = new FileInfo(excelSheetDataSourceFilePath);
            string fileExtension = excelSheetDataSourceFilePath.Substring(excelSheetDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();
            
            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Excel;
            itemToImport.ItemPath = excelSheetDataSourceFilePath;
            itemToImport.ItemName = fi.Name;

            var util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> excelSheetToDataTableMapping = null;
            await Task.Factory.StartNew(() =>
            {
                excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(itemToImport.ItemPath, ImportPreview_MaximumSampleRows, true);
            });
            foreach (var sheetName in excelSheetToDataTableMapping.Keys)
            {
                ExcelSheet sheetMaterial = new ExcelSheet()
                {
                    FileJobSharePath = string.Empty,
                    SheetName = sheetName
                };
                DataTable sheetPreview = null;
                if (excelSheetToDataTableMapping.ContainsKey(sheetName))
                {
                    sheetPreview = excelSheetToDataTableMapping[sheetName];
                }                
                ExcelSheetMaterialVM sheetMaterialVM = new ExcelSheetMaterialVM(true, itemToImport.ItemName, itemToImport.ItemPath, sheetName, sheetPreview)
                {
                    relatedMaterialBase = sheetMaterial,
                    relatedItemToImport = itemToImport,
                    Icon = null
                };
                itemToImport.ItemMaterials.Add(sheetMaterialVM);
            }

            List<string> assignedDescriptions = new List<string>();

            using (ShimsContext.Create())
            {
                foreach (var currentMaterialBaseVM in itemToImport.ItemMaterials)
                {
                    SemiStructuredMaterialVM semiStructuredMaterialVM = currentMaterialBaseVM as SemiStructuredMaterialVM;
                    string constDescription = Guid.NewGuid().ToString();

                    DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                    {
                        return constDescription;
                    };

                    assignedDescriptions.Add(constDescription);

                    DataSourceProvider dsProvider = new DataSourceProvider();
                    DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(semiStructuredMaterialVM);
                    TransformationResult transformation = ImportProvider.PerformTransformation(semiStructuredMaterialVM.Preview, loadedMapping);
                    WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                               (transformation.GeneratingObjects, transformation.GeneratingRelationships
                               , dataSource);
                }
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            foreach (var currentDescription in assignedDescriptions)
            {
                Assert.AreEqual(1, allDataSources.Where(d => d.Description == currentDescription).Count());
                Assert.AreEqual(userName, allDataSources.Where(d => d.Description == currentDescription).First().CreatedBy);
            }
        }

        [TestMethod]
        public async Task CreateExcelFileDataSourceForUserWithoutPermission()
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

            FileInfo fi = new FileInfo(excelSheetDataSourceFilePath);
            string fileExtension = excelSheetDataSourceFilePath.Substring(excelSheetDataSourceFilePath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Excel;
            itemToImport.ItemPath = excelSheetDataSourceFilePath;
            itemToImport.ItemName = fi.Name;

            var util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> excelSheetToDataTableMapping = null;
            await Task.Factory.StartNew(() =>
            {
                excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(itemToImport.ItemPath, ImportPreview_MaximumSampleRows, true);
            });
            foreach (var sheetName in excelSheetToDataTableMapping.Keys)
            {
                ExcelSheet sheetMaterial = new ExcelSheet()
                {
                    FileJobSharePath = string.Empty,
                    SheetName = sheetName
                };
                DataTable sheetPreview = null;
                if (excelSheetToDataTableMapping.ContainsKey(sheetName))
                {
                    sheetPreview = excelSheetToDataTableMapping[sheetName];
                }
                
                ExcelSheetMaterialVM sheetMaterialVM = new ExcelSheetMaterialVM(true, itemToImport.ItemName, itemToImport.ItemPath, sheetName, sheetPreview)
                {
                    relatedMaterialBase = sheetMaterial,
                    relatedItemToImport = itemToImport,
                    Icon = null
                };
                itemToImport.ItemMaterials.Add(sheetMaterialVM);
            }

            List<string> assignedDescriptions = new List<string>();

            using (ShimsContext.Create())
            {
                foreach (var currentMaterialBaseVM in itemToImport.ItemMaterials)
                {
                    SemiStructuredMaterialVM semiStructuredMaterialVM = currentMaterialBaseVM as SemiStructuredMaterialVM;
                    string constDescription = Guid.NewGuid().ToString();

                    DataImport.Publish.Fakes.ShimDataSourceMetadata.AllInstances.DescriptionGet = (metadata) =>
                    {
                        return constDescription;
                    };

                    assignedDescriptions.Add(constDescription);

                    DataSourceProvider dsProvider = new DataSourceProvider();
                    DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(semiStructuredMaterialVM);
                    TransformationResult transformation = ImportProvider.PerformTransformation(semiStructuredMaterialVM.Preview, loadedMapping);
                    WorkSpaceSidePublishResult workSpaceSidePublishResult = ImportProvider.WorkspaceSideSemiStructuredDataImport
                               (transformation.GeneratingObjects, transformation.GeneratingRelationships
                               , dataSource);
                }
            }

            // Act
            List<DataSourceInfo> allDataSources = new List<DataSourceInfo>();
            List<DataSourceInfo> retrievedDataSources = new List<DataSourceInfo>();
            DataSourceProvider dataSourceProvider = new DataSourceProvider();
            int moreCount = 0;
            retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
            allDataSources.AddRange(retrievedDataSources);
            while (retrievedDataSources.Count > numberOfDataSourcesPerType)
            {
                moreCount++;
                retrievedDataSources.Clear();
                retrievedDataSources = (await dataSourceProvider.GetDataSourcesAsync(DataSourceType.ExcelSheet, moreCount, string.Empty)).ToList();
                allDataSources.AddRange(retrievedDataSources);
            }

            //Assert 
            foreach (var currentDescription in assignedDescriptions)
            {
                Assert.AreEqual(0, allDataSources.Where(d => d.Description == currentDescription).Count());
            }
        }
    }
}
