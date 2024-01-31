using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.Workspace.Entities.SearchAroundResult;
using System.Linq;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.SearchAroundTests
{
    [TestClass]
    public class RelatedEvents
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

        // تستهای جستجوی رخدادها
        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task GetUnpublishedEventWithUnpublishedSides()
        {
            // Assign
            string eventType = "ایمیل";
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.SourceToTarget, null, null, string.Empty);
            // Act

            EventBasedResult searchResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson1 });
            EventBasedResult searchResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson2 });

            // Assert
            Assert.AreEqual(1, searchResultForPerson1.Results.Count);
            Assert.IsTrue(searchResultForPerson1.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResultForPerson1.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson2, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);

            Assert.AreEqual(1, searchResultForPerson2.Results.Count);
            Assert.IsTrue(searchResultForPerson2.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson2));
            Assert.AreEqual(1, searchResultForPerson2.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson1, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
        }

        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task GetUnpublishedEventWithPublishedSides()
        {
            // Assign
            string eventType = "ایمیل";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.SourceToTarget, null, null, string.Empty);
            // Act
            EventBasedResult searchResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson1 });
            EventBasedResult searchResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.AreEqual(1, searchResultForPerson1.Results.Count);
            Assert.IsTrue(searchResultForPerson1.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResultForPerson1.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson2, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);

            Assert.AreEqual(1, searchResultForPerson2.Results.Count);
            Assert.IsTrue(searchResultForPerson2.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson2));
            Assert.AreEqual(1, searchResultForPerson2.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson1, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
        }

        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task GetPublishedEventWithPublishedSides()
        {
            // Assign
            string eventType = "ایمیل";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWRelationship> unpublishedRelationships = new List<KWRelationship>();

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelationships.Add(newEvent.FirstRelationship);
            unpublishedRelationships.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelationships, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            EventBasedResult searchResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson1 });
            EventBasedResult searchResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.AreEqual(1, searchResultForPerson1.Results.Count);
            Assert.IsTrue(searchResultForPerson1.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResultForPerson1.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson2, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);

            Assert.AreEqual(1, searchResultForPerson2.Results.Count);
            Assert.IsTrue(searchResultForPerson2.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson2));
            Assert.AreEqual(1, searchResultForPerson2.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson1, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);

        }

        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task GetPublishedEventWithOnePublishedSide()
        {
            // Assign
            string eventType = "ایمیل";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            unpublishedObjects.Add(newUnpublishPerson1);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);

            // Act
            EventBasedResult searchResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson1 });

            EventBasedResult searchResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.AreEqual(1, searchResultForPerson1.Results.Count);
            Assert.IsTrue(searchResultForPerson1.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.AreEqual(1, searchResultForPerson1.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson2, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson1.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);

            Assert.AreEqual(1, searchResultForPerson2.Results.Count);
            Assert.IsTrue(searchResultForPerson2.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson2));
            Assert.AreEqual(1, searchResultForPerson2.Results.Select(o => o.SearchedObject).Count());
            Assert.AreEqual(newUnpublishPerson1, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject);
            Assert.AreEqual(newEvent.FirstRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().SecondRelationshipID);
            Assert.AreEqual(newEvent.SecondRelationship.ID, searchResultForPerson2.Results.Where(o => o.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.FirstOrDefault().FirstRelationshipID);
        }

        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task CheckEventsNumberBetweenTwoObject()
        {
            // Assign
            string eventType = "ایمیل";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();
            for (int i = 0; i < Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize; i++)
            {
                EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);

                unpublishedRelations.Add(newEvent.FirstRelationship);
                unpublishedRelations.Add(newEvent.SecondRelationship);
                unpublishedObjects.Add(newEvent.IntermediaryEvent);
            }

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            EventBasedResult searchResult
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson1 });

            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.IsTrue(searchResult.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson1));
            Assert.IsFalse(searchResult.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(
                Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize,
                searchResult.Results.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count
                );


        }

        [TestCategory("جستجوی رخداد")]
        [TestMethod]
        public async Task CheckRelatedEventNumber()
        {
            // Assign
            string eventType = "ایمیل";
            KWObject newUnpublishPerson0 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 0");

            List<KWObject> unpublishedObjects = new List<KWObject>();

            for (int i = 1; i < 101; i++)
            {
                KWObject newUnpublishPerson = await ObjectManager.CreateNewObject("شخص",
                    string.Format("{0}Person {1}", Guid.NewGuid().ToString(), i));
                unpublishedObjects.Add(newUnpublishPerson);
            }

            foreach (var currentObject in unpublishedObjects)
            {
                EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson0, currentObject, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            }

            // Act
            EventBasedResult searchResult
                = await Workspace.Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(new KWObject[] { newUnpublishPerson0 });
            // Assert
            Assert.AreEqual(1, searchResult.Results.Count);
            Assert.IsTrue(searchResult.Results.Select(o => o.SearchedObject).Contains(newUnpublishPerson0));
            Assert.IsFalse(searchResult.IsResultsCountMoreThanThreshold);

            Assert.AreEqual(
                Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize,
                searchResult.Results.FirstOrDefault().LoadedResults.Count
                );

            Assert.AreEqual(
                100 - Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize,
                searchResult.Results.FirstOrDefault().NotLoadedResults.Count
                );
        }
    }
}
