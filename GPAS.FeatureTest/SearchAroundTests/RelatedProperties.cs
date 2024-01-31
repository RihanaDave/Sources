using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.Workspace.Entities.SearchAroundResult;
using System.Linq;

namespace GPAS.FeatureTest.SearchAroundTests
{
    [TestClass]
    public class RelatedProperties
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

        // تستهای جستجوی روابط مبتنی بر ویژگی
        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetUnpublishedSamePropertyForUnpublishedSides()
        {
            // Assign
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", "name");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            KWProperty newUnpublishProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, "نام", "name");
            // Act
            PropertyBasedResult searchResult =
                await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Target, newUnpublishPerson2);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
        }

        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetUnpublishedSamePropertyForPublishedSides()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            string propId = Guid.NewGuid().ToString();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", $"{propId}name");
            KWProperty newUnpublishProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, "نام", $"{propId}name");

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Target, newUnpublishPerson2);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
        }

        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetUnpublishedSamePropertyWithOnePublishedSide()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            unpublishedObjects.Add(newUnpublishPerson1);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            string propId = Guid.NewGuid().ToString();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", $"{propId}name");
            KWProperty newUnpublishProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, "نام", $"{propId}name");

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Target, newUnpublishPerson2);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");

        }

        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetSamePropertyWithPublishedSides()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            string propId = Guid.NewGuid().ToString();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", $"{propId}name");
            KWProperty newUnpublishProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, "نام", $"{propId}name");
            unpublishedProperties.Add(newUnpublishProperty1);
            unpublishedProperties.Add(newUnpublishProperty2);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Target, newUnpublishPerson2);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
        }


        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetSamePropertyForPublishedSidesAndOnePublishedProperty()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            string propId = Guid.NewGuid().ToString();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", $"{propId}name");
            KWProperty newUnpublishProperty2 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, "نام", $"{propId}name");
            unpublishedProperties.Add(newUnpublishProperty1);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Target, newUnpublishPerson2);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
        }

        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task CheckSamePropertiesNumber()
        {
            // Assign
            string propId = Guid.NewGuid().ToString();
            KWObject newUnpublishPerson0 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 0");
            KWProperty newUnpublishProperty0 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson0, "نام", $"{propId}name");

            List<KWObject> unpublishObjects = new List<KWObject>();

            for (int i = 0; i < Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize; i++)
            {
                KWObject newUnpublishPerson = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person {i}");
                KWProperty newUnpublishProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson, "نام", $"{propId}name");
            }

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> { newUnpublishProperty0
               }); 
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().Source, newUnpublishPerson0);
            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
            Assert.AreEqual(
                searchResult.Results.FirstOrDefault().LoadedResults.Count,
                Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize
                );
        }

        [TestCategory("جستجوی ویژگی")]
        [TestMethod]
        public async Task GetSamePropertyBetweenMultipleObjects()
        {
            // Assign
            string propId1 = Guid.NewGuid().ToString();
            string propId2 = Guid.NewGuid().ToString();
            KWObject newUnpublishPerson0 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 0");
            KWProperty newUnpublishProperty0 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson0, "نام", $"{propId1}name");

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson1, "نام", $"{propId2}name");

            List<KWObject> unpublishObjects = new List<KWObject>();

            for (int i = 2; i < Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize +2 ; i++)
            {
                KWObject newUnpublishPerson = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person {i}");
                KWProperty newUnpublishProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson, "نام", $"{propId1}name");
                KWProperty newUnpublishProperty_ = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson, "نام", $"{propId2}name");
            }

            // Act
            PropertyBasedResult searchResult =
               await Workspace.Logic.Search.SearchAround.GetObjectsWithSameProperty(new List<KWProperty> {newUnpublishProperty0, newUnpublishProperty1 });
            // Assert
            Assert.AreEqual(2, searchResult.Results.Count);
            Assert.AreEqual(searchResult.Results[0].LoadedResults.FirstOrDefault().Source, newUnpublishPerson0);
            Assert.AreEqual(searchResult.Results[1].LoadedResults.FirstOrDefault().Source, newUnpublishPerson1);

            Assert.AreEqual(searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().SamePropertyTypeUri, "نام");
            Assert.AreEqual(
                searchResult.Results[0].LoadedResults.Count,
                Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize
                );
            Assert.AreEqual(
                searchResult.Results[1].LoadedResults.Count,
                Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize
                );
            Assert.AreEqual(searchResult.Results[0].LoadedResults.FirstOrDefault().SamePropertyValue, $"{propId1}name");
            Assert.AreEqual(searchResult.Results[1].LoadedResults.FirstOrDefault().SamePropertyValue, $"{propId2}name");


        }
    }
}
