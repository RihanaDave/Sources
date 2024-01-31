using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.PublishTests
{
    [TestClass]
    public class PublishRelationshipAndEvent
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
        public async Task CreateNewRelationship()
        {
            //Assign
            string guid1 = Guid.NewGuid().ToString();
            string obj1Label = $"{guid1}obj1";
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);

            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);

            RelationshipBasedKWLink relationship1 = await LinkManager.CreateRelationshipBaseLinkAsync(newObj1, newObj2, "شبیه", LinkDirection.Bidirectional, null, null, "myDiscription");
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newObj1, newObj2 }, new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName }, new List<KWMedia>(), new List<KWRelationship>() { relationship1.Relationship }, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            RelationshipBasedKWLink retrivedRelationship = (await LinkManager.RetrieveRelationshipBaseLinksAsync(new List<long>() { relationship1.Relationship.ID })).FirstOrDefault();
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            Assert.AreEqual(retrivedRelationship.Source.ID, newObj1.ID);
            Assert.AreEqual(retrivedRelationship.Target.ID, newObj2.ID);
            Assert.AreEqual(retrivedRelationship.Relationship.ID, relationship1.Relationship.ID);


        }
        [TestMethod]
        public async Task CreateNewEvent()
        {
            //Assign
            string guid1 = Guid.NewGuid().ToString();
            string obj1Label = $"{guid1}obj1";
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "ایمیل", LinkDirection.Bidirectional, null, null, "myDiscription");
            //Act
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs, objs.Select(o => o.DisplayName).ToList(), new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship }, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<RelationshipBasedKWLink> retrivedRelationshipOfEvent = (await LinkManager.RetrieveRelationshipBaseLinksAsync(new List<long>() { event1.FirstRelationship.ID, event1.SecondRelationship.ID }));
            EventBasedKWLink retrivedEvent = LinkManager.GetEventBaseKWLinkFromLinkInnerRelationships(retrivedRelationshipOfEvent[0], retrivedRelationshipOfEvent[1]);
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(retrivedEvent.Source.ID, event1.Source.ID);
            Assert.AreEqual(retrivedEvent.Target.ID, event1.Target.ID);

        }

    }
}
