using System;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.FilterSearch;
using System.Linq;
using GPAS.Workspace.Logic.Publish;

namespace GPAS.FeatureTest.QuickSearchTests
{
    [TestClass]
    public class NonWildCardSearch
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

        [TestMethod]
        public async Task RetrieveUnpublishedObjectByExactLabelValue()
        {
            // Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person1";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            // Act
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(newPersonLabel, 10)).ToList();
            // Assert
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newUnpublishPerson1, searchResult[0]);
        }
        [TestMethod]
        public async Task RetrivePublishedObjectByExactValue()
        {
            //Assign
            string newPersonLabel = $"{Guid.NewGuid().ToString()}Person1";
            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newPublishPerson1 }, new List<KWProperty>() { newPublishPerson1.DisplayName}, new List<KWMedia>() , new List<KWRelationship>() , new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            if (ObjectManager.CanDeleteObject(newPublishPerson1))
            {
                await ObjectManager.DeleteObject(newPublishPerson1);
            }
            //Act
            List<KWObject> searchResult
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(newPersonLabel, 10)).ToList();
            //Assert
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(1, searchResult.Count);
            Assert.AreEqual(newPublishPerson1.ID, searchResult.FirstOrDefault().ID);
        }
    }
}
