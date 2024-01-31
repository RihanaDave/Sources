using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.ViewModel.DataImport;
using System.IO;
using GPAS.DataImport.DataMapping.Unstructured;
using System.Windows.Media.Imaging;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.SearchAroundTests
{
    [TestClass]
    public class RelatedDocument
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

        // تست‌های جستجوی رابطه‌
        [TestCategory("جستجوی روابط مبتنی بر سند")]
        [TestMethod]
        public async Task GetRelatedDocumentForUnpublishedObject()
        {
            // Assign
            string PDFFileTest = $"{documentsPath}PDFFileTest.pdf";

            FileInfo fi = new FileInfo(PDFFileTest);
            string fileExtension = PDFFileTest.Substring(PDFFileTest.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = PDFFileTest,
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

            var ImportResult = await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            List<KWObject> generatedDocuments = ImportResult.Item1;

            Assert.AreNotEqual(0, generatedDocuments.Count);

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, generatedDocuments.ElementAt(0), "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);

            // Act
            RelationshipBasedResult searchResult = await Workspace.Logic.Search.SearchAround.GetRelatedDocuments(new List<KWObject> { newUnpublishPerson1 });

            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.IsTrue(searchResult.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResult.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(generatedDocuments.ElementAt(0), searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().SearchedObject.TypeURI, "شخص");
        }

        // تست‌های جستجوی رابطه‌
        [TestCategory("جستجوی روابط مبتنی بر سند")]
        [TestMethod]
        public async Task GetRelatedDocumentForPublishedObject()
        {
            // Assign
            string PDFFileTest = $"{documentsPath}PDFFileTest.pdf";

            FileInfo fi = new FileInfo(PDFFileTest);
            string fileExtension = PDFFileTest.Substring(PDFFileTest.LastIndexOf('.') + 1).ToUpper();

            ItemToImportVM itemToImport = new ItemToImportVM()
            {
                IsUnstructured = true,
                DocumentTypeUri = fileExtension,
                Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(fileExtension)),
                IsSelected = true,
                ItemName = fi.Name,
                ItemPath = PDFFileTest,
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

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            var ImportResult = await ImportProvider.WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials);
            List<KWObject> generatedDocuments = ImportResult.Item1;



            Assert.AreNotEqual(0, generatedDocuments.Count);

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            unpublishedObjects.Add(newUnpublishPerson1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, generatedDocuments.ElementAt(0), "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            // Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            RelationshipBasedResult searchResult = await Workspace.Logic.Search.SearchAround.GetRelatedDocuments(new List<KWObject> { newUnpublishPerson1 });

            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.IsTrue(searchResult.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResult.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(generatedDocuments.ElementAt(0), searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().SearchedObject.TypeURI, "شخص");

        }
    }
}
