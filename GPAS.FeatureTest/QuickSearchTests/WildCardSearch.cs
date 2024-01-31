using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.QuickSearchTests
{
    [TestClass]
    public class WildCardSearch
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
        public async Task RetrivePublishedObjectByWildCard()

        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            guid = guid.Replace("-", "ـ");
            string newPersonLabel = $"Person1{guid}Per son1";
            string astrixWildCard = $"*{guid}*";
            string questionMarkWildCard = $"??rson?{guid}P?r ?o??";
            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newPublishPerson1 }, new List<KWProperty>() { newPublishPerson1.DisplayName }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            if (ObjectManager.CanDeleteObject(newPublishPerson1))
            {
                await ObjectManager.DeleteObject(newPublishPerson1);
            }
            //Act
            List<KWObject> searchResult1
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(astrixWildCard, 10)).ToList();

            List<KWObject> searchResult2
                = (Workspace.Logic.Search.SearchProvider.QuickSearchAsync(questionMarkWildCard, 10)).ToList();
            //Assert
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            Assert.AreEqual(searchResult1.Count, 1);
            Assert.AreEqual(searchResult1.FirstOrDefault().ID, newPublishPerson1.ID);


            Assert.AreEqual(searchResult2.Count, 1);
            Assert.AreEqual(searchResult2.FirstOrDefault().ID, newPublishPerson1.ID);
        }
    }
}
