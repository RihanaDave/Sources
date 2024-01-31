using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv;
using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.DataImport;
using GPAS.Workspace.ViewModel.DataImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.ImportTests
{
    [TestClass]
    public class EmlFileDataImportTests
    {
        private bool isInitialized = false;
        private string EmlPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Eml\\";
        public static readonly string emlDirectoryPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\Temp\\";
        private readonly string[] SupportedStructuredFileExtensions = new string[] { "CSV", "XLSX", "EML" };

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
            //SaveInternalResolveTestFile();
        }

        [TestCleanup()]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void EmlFileImportTest()
        {
            // Assign
            string EmlMapping = $"{EmlPath}\\EmlMap\\EmlMapping.imm";
            string EmlDir = $"{EmlPath}EmlDir\\";
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            TypeMapping loadedMapping = serializer.DeserializeFromFile(EmlMapping);

            ItemToImportVM itemToImport = new ItemToImportVM();
            itemToImport.ItemType = MaterialType.Eml;
            DataTable sheetPreview = new DataTable();
            EmlDirectory emlDirectory = new EmlDirectory()
            {
                DirectoryJobSharePath = string.Empty,
            };

            EmlFileMaterialVM emlFileMaterialVM = new EmlFileMaterialVM(true, "EML Files Material", sheetPreview)
            {
                relatedMaterialBase = emlDirectory,
                relatedItemToImport = itemToImport,
                Icon = null,
            };

            List<FileInfo> emlFileInfos = new List<FileInfo>();
            if (Directory.Exists(EmlDir))
            {
                string[] files = GetSupportedFilePathesFromFolderContent(EmlDir).ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    string filePath = files[i];
                    if (!File.Exists(files[i]))
                        throw new FileNotFoundException("File not found .. ");

                    emlFileInfos.Add(new FileInfo(filePath));

                }
            }

            emlFileMaterialVM.AddEmlFilesIfNotExist(emlFileInfos);

            itemToImport.ItemMaterials.Add(emlFileMaterialVM);

            Convertor convertor = new Convertor();
            Directory.CreateDirectory(emlDirectoryPath);
            convertor.TargetDirectoryPath = emlDirectoryPath;
            convertor.SourceFiles = emlFileMaterialVM.GetEmlFiles();
            convertor.AttachmentsPathPrefix = emlDirectoryPath;
            convertor.SpliteCsvFiles = false;

            var utility = new GPAS.DataImport.Transformation.Utility();
            string[] csvPathes = convertor.PerformConversionToCsvFiles();

            TransformationResult transformation;
            FileStream csvStream = new FileStream(csvPathes.FirstOrDefault(), FileMode.Open, FileAccess.Read);
            try
            {
                transformation = ImportProvider.PerformTransformation(csvStream, ',', loadedMapping);
                emlFileMaterialVM.workspceSideCsvFilePath = csvPathes.FirstOrDefault();
            }
            finally
            {
                if (csvStream != null)
                    csvStream.Close();
            }

            string fileName = Path.GetFileName(emlFileMaterialVM.workspceSideCsvFilePath);
            string dsName = Path.GetFileNameWithoutExtension(emlFileMaterialVM.workspceSideCsvFilePath);
            if (string.IsNullOrEmpty(fileName))
                fileName = "CSV";

            emlFileMaterialVM.Preview = utility.GenerateDataTableFromStringArray(convertor.PerformConversionToInMemoryMatrix(10), fileName, dsName);
            DataSourceProvider dsProvider = new DataSourceProvider();

            DataSourceMetadata dataSource = dsProvider.GenerateImportDataSourceMetadata(emlFileMaterialVM);
            // Act
            WorkSpaceSidePublishResult result = ImportProvider.WorkspaceSideSemiStructuredDataImport
                           (transformation.GeneratingObjects, transformation.GeneratingRelationships
                           , dataSource);
            // Assert
            Assert.IsFalse(result.IsAnySearchIndexSynchronizationFailureOccured);
            Assert.AreEqual(transformation.GeneratingObjects.Count, result.AddedOrResolvedObjectIDs.Count);
        }

        private List<string> GetSupportedFilePathesFromFolderContent(string folderPath)
        {
            var containingSupportedFiles = new List<string>();
            foreach (var file in Directory.GetFiles(folderPath))
            {
                if (IsFileSupported(file))
                {
                    containingSupportedFiles.Add(file);
                }
            }
            foreach (var subFolder in Directory.GetDirectories(folderPath))
            {
                containingSupportedFiles.AddRange(GetSupportedFilePathesFromFolderContent(subFolder));
            }
            return containingSupportedFiles;
        }

        private bool IsFileSupported(string filePath)
        {
            int extensionStartIndex = filePath.LastIndexOf('.') + 1;
            if (filePath.Length <= extensionStartIndex)
                return false;
            string extension = filePath.Substring(extensionStartIndex).ToUpper();
            if (SupportedStructuredFileExtensions.Contains(extension))
                return true;
            return false;
        }
    }


}
