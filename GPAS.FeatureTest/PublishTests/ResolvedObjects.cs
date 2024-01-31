using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.PublishTests
{
    [TestClass]
    public class ResolvedObjects
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
        public async Task ResolvePublishObjects()
        {
            //Assign
            //obj1
            Random random = new Random();
            string guid = Guid.NewGuid().ToString();
            string obj1Label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            KWProperty doubleProperty = PropertyManager.CreateNewPropertyForObject(newObj1, "قد", (Math.Abs(random.NextDouble() * (220) - 220)).ToString());

            //obj2
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            KWProperty numericProperty2 = PropertyManager.CreateNewPropertyForObject(newObj2, "سن", random.Next(1, 120).ToString());
            //event1 Email
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "خرید", LinkDirection.Bidirectional, null, null, "myDiscription");
            KWProperty event1LongProperty = PropertyManager.CreateNewPropertyForObject(event1.IntermediaryEvent, "مبلغ", random.Next(1, int.MaxValue).ToString());
            //Add To List
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            List<List<KWProperty>> objectsProperties = new List<List<KWProperty>>();
            objectsProperties.Add(new List<KWProperty>() { newObj1.DisplayName, doubleProperty });
            objectsProperties.Add(new List<KWProperty>() { newObj2.DisplayName, numericProperty2 });
            objectsProperties.Add(new List<KWProperty>() { event1.IntermediaryEvent.DisplayName, event1LongProperty });
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs,
                    new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName, event1.IntermediaryEvent.DisplayName, doubleProperty, numericProperty2, event1LongProperty },
                    new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship },
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            //Act
            GlobalResolutionProvider globalResolutionProvider = new GlobalResolutionProvider();
            List<KWObject> resolvedObjects = new List<KWObject>() { newObj1, newObj2 };
            var resolveResult = globalResolutionProvider.ResolveObjects(resolvedObjects, newObj1.DisplayName.Value);
            List<KWObject> retrivePublishedObjs = (await ObjectManager.RetriveObjectsAsync(resolvedObjects.Select(o => o.ID))).ToList();
            List<KWObject> retriveResolveMaster = (await ObjectManager.RetriveObjectsAsync(new List<long> {resolveResult.Item1.ID})).ToList();
            //Assert;
            Assert.AreEqual(retrivePublishedObjs.FirstOrDefault().ID, resolveResult.Item1.ID);
            Assert.AreEqual(retriveResolveMaster.FirstOrDefault().ID, resolveResult.Item1.ID);
        }

    }
}
