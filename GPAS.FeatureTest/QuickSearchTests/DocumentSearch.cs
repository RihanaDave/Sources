using GPAS.DataImport.DataMapping.Unstructured;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.ViewModel.DataImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.FeatureTest.QuickSearchTests
{
    [TestClass]
    public class DocumentSearch
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
        [TestMethod]
        public async Task SearchMetaDataInDocuments()
        {
            // Assign
            string testDocumentPath = documentsPath + "Quick Search Test.mp3";
            TagLib.File testDocument = TagLib.File.Create(testDocumentPath);
            string title = Guid.NewGuid().ToString();
            testDocument.Tag.Title = title;
            string album = Guid.NewGuid().ToString();
            testDocument.Tag.Album = album;
            testDocument.Save();

            FileInfo fi = new FileInfo(testDocumentPath);
            string fileExtension = testDocumentPath.Substring(testDocumentPath.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = testDocumentPath,
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


            //Act
            var ImportResult = await 
                ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);

            List<KWObject> generatedDocuments = ImportResult.Item1;
            Assert.AreNotEqual(0, generatedDocuments.Count);

            List<KWObject> searchTitleResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(title, 10)).ToList();
            List<KWObject> searchAlbumResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(album, 10)).ToList();

            Assert.AreEqual(generatedDocuments[0], searchTitleResult.FirstOrDefault());
            Assert.AreEqual(generatedDocuments[0], searchAlbumResult.FirstOrDefault());
        }
        [TestMethod]
        public async Task SearchContentInDocuments_English_nonWildCard()
        {
            //Assign
            List<KWObject> ImportResult = await PublishSampleTextualDocuments();
            //Act
            List<KWObject> generatedDocuments = ImportResult;
            Assert.AreNotEqual(0, generatedDocuments.Count);
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync("quick search test operation", 1000)).ToList();
            //Assert
            foreach (var generatedDocument in generatedDocuments)
            {
                Assert.IsTrue(searchResult.Contains(generatedDocument));
            }
        }
        [TestMethod]
        public async Task SearchContentInDocuments_Persian_nonWildCard()
        {
            //Assign
            List<KWObject> ImportResult = await PublishSampleTextualDocuments();
            //Act
            List<KWObject> generatedDocuments = ImportResult;
            Assert.AreNotEqual(0, generatedDocuments.Count);
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync("جست و جوی سریع", 1000)).ToList();
            //Assert
            foreach (var generatedDocument in generatedDocuments)
            {
                Assert.IsTrue(searchResult.Contains(generatedDocument));
            }
        }
        [TestMethod]
        public async Task SearchContentInDocuments_Persian_WildCard()
        {
            //Assign
            List<KWObject> ImportResult = await PublishSampleTextualDocuments();
            //Act
            List<KWObject> generatedDocuments = ImportResult;
            Assert.AreNotEqual(0, generatedDocuments.Count);
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync("*جس؟؟*جوی سر?ع*", 1000)).ToList();
            //Assert
            foreach (var generatedDocument in generatedDocuments)
            {
                Assert.IsTrue(searchResult.Contains(generatedDocument));
            }
        }
        [TestMethod]
        public async Task SearchContentInDocuments_English_WildCard()
        { 
            //Assign
            List<KWObject> ImportResult = await PublishSampleTextualDocuments();
            //Act
            List<KWObject> generatedDocuments = ImportResult;
            Assert.AreNotEqual(0, generatedDocuments.Count);
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync("q??ck s*rch*", 1000)).ToList();
            //Assert
            foreach (var generatedDocument in generatedDocuments)
            {
                Assert.IsTrue(searchResult.Contains(generatedDocument));
            }
        }
        private async Task<List<KWObject>> PublishSampleTextualDocuments()
        {
            var files = System.IO.Directory.GetFiles(documentsPath);

            List<System.IO.FileInfo> filesInfos = new List<System.IO.FileInfo>();
            List<string> filesPath = new List<string>();
            foreach (string filePath in files)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                if ((fileInfo.Extension).ToLowerInvariant() != ".mp3")
                {
                    filesInfos.Add(fileInfo);
                    filesPath.Add(filePath);
                }
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



            return (await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials)).Item1;
        }
    }
}
