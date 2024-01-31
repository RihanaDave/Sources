using GPAS.DataImport.DataMapping.Unstructured;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.ViewModel.DataImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.FeatureTest.PublishTests
{
    [TestClass]
    public class PubllishDocument
    {
        private bool isInitialized = false;
        private string documentsPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\Resources\\";

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
        [TestCategory("انتشار سند")]
        [TestMethod]
        public async Task CreateNewDocument()
        {
            // Assign
            var files = System.IO.Directory.GetFiles(documentsPath);

            List<System.IO.FileInfo> filesInfos = new List<System.IO.FileInfo>();
            foreach (string filePath in files)
            {
                filesInfos.Add(new System.IO.FileInfo(filePath));
            }

            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials = new Dictionary<MaterialBaseVM, DataImport.DataMapping.Unstructured.TypeMapping>();

            foreach (var currentFileInfo in filesInfos)
            {
                string fileExtension = currentFileInfo.FullName.Substring(currentFileInfo.FullName.LastIndexOf('.') + 1).ToUpper();
               
                ItemToImportVM itemToImport = new ItemToImportVM()
                {
                    IsUnstructured = true,
                    DocumentTypeUri = fileExtension,
                    Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                    IsSelected = true,
                    ItemName = currentFileInfo.Name,
                    ItemPath = currentFileInfo.FullName,
                };

                UnstructuredMaterialVM unstructuredMaterialVM = new UnstructuredMaterialVM(true, itemToImport.ItemName)
                {
                    Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                    relatedMaterialBase = null,
                    relatedItemToImport = itemToImport
                };

                itemToImport.ItemMaterials.Add(unstructuredMaterialVM);

                ObjectMapping objectToAdd = new ObjectMapping
                        (currentFileInfo.Extension.TrimStart(new char[] { '.' }).ToUpper(), currentFileInfo.Name);
                TypeMapping typeMapping = new TypeMapping();
                typeMapping.AddObjectMapping(objectToAdd);                

                unstructuredMaterials.Add(unstructuredMaterialVM, typeMapping);
            }
            //Act
            var ImportResult = await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            List<KWObject> generatedDocuments = ImportResult.Item1;
            Assert.AreNotEqual(0, generatedDocuments.Count);
            List<KWObject> retriveDocumentObjects = (await ObjectManager.RetriveObjectsAsync(generatedDocuments.Select(o => o.ID))).ToList();
            List<string> retrivedDataSourceFilePath = new List<string>();
            List<string> retrivedDocumentFilePath = new List<string>();
            for (int i = 0; i < retriveDocumentObjects.Count; i++)
            {
                KWObject retrivedDocumentobject = retriveDocumentObjects.ElementAt(i);
                string localPath = $"{retrivedDocumentobject.DisplayName.DataSourceId.Value}{filesInfos[i].Extension}";
                await DataSourceProvider.DownloadDataSourceAsync
                    (retrivedDocumentobject.DisplayName.DataSourceId.Value, localPath);
                retrivedDataSourceFilePath.Add(localPath);
                string documentPath = await DataSourceProvider.DownloadDocumentAsync(retrivedDocumentobject);
                retrivedDocumentFilePath.Add(documentPath);
            }
            //Assert
            FileUtility fileUtil = new FileUtility();
            for (int i = 0; i < files.Length; i++)
            {
                byte[] md5HashOfOrginalFile = fileUtil.ComputeFileBytesFromFileFilePath(files[i]);
                byte[] md5HashOfDataSourceFile = fileUtil.ComputeFileBytesFromFileFilePath(retrivedDataSourceFilePath[i]);
                byte[] md5HashOfDocumentFile = fileUtil.ComputeFileBytesFromFileFilePath(retrivedDocumentFilePath[i]);
                
                Assert.AreEqual(md5HashOfDocumentFile.Length, md5HashOfOrginalFile.Length);
                Assert.AreEqual(md5HashOfDataSourceFile.Length, md5HashOfOrginalFile.Length);
                for (int j = 0; j < md5HashOfOrginalFile.Length; j++)
                {
                    Assert.AreEqual(md5HashOfOrginalFile[j], md5HashOfDocumentFile[j]);
                    Assert.AreEqual(md5HashOfOrginalFile[j], md5HashOfDataSourceFile[j]);
                }
            }
        }
    }
}
