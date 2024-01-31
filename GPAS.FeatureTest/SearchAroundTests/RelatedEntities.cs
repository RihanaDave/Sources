using System;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.Entities.SearchAroundResult;
using System.Linq;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.SearchAroundTests
{
    [TestClass]
    public class RelatedEntities
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


        // تست‌های جستجوی رابطه‌
        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task GetUnpublishedRelationshipWithUnpublishedSides()
        {
            // Assign
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");
            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            // Act
            RelationshipBasedResult relationshipBasedResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson1 });

            RelationshipBasedResult relationshipBasedResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson2 });

            // Assert
            Assert.IsFalse(relationshipBasedResultForPerson1.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Select(r=>r.SearchedObject.ID).Contains(newUnpublishPerson1.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson2.ID, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));

            Assert.IsFalse(relationshipBasedResultForPerson2.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson2.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
        }

        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task GetUnpublishedRelationshipWithPublishedSides()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects, 
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            // Act
            RelationshipBasedResult relationshipBasedResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson1 });

            RelationshipBasedResult relationshipBasedResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.IsFalse(relationshipBasedResultForPerson1.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson1.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson2.ID, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));

            Assert.IsFalse(relationshipBasedResultForPerson2.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson2.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
        }

        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task GetPublishedRelationshipWithPublishedSides()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List < KWRelationship > unpublishedRelations = new List<KWRelationship>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            RelationshipBasedResult relationshipBasedResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson1 });

            RelationshipBasedResult relationshipBasedResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.IsFalse(relationshipBasedResultForPerson1.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson1.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson2.ID, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));

            Assert.IsFalse(relationshipBasedResultForPerson2.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson2.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
        }

        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task GetPublishedRelationshipWithPublishedOneSide()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();           
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());


            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);

            // Act
            RelationshipBasedResult relationshipBasedResultForPerson1
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson1 });

            RelationshipBasedResult relationshipBasedResultForPerson2
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson2 });
            // Assert
            Assert.IsFalse(relationshipBasedResultForPerson1.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson1.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson2.ID, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson1.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));

            Assert.IsFalse(relationshipBasedResultForPerson2.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson2.ID));
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(1, relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(relationshipBasedResultForPerson2.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
        }

        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task CheckRelatedEntitiesNumber()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject("شخص", $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();
            for (int i = 0; i < 1000; i++)
            {
                RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1,
                    newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
                unpublishedRelations.Add(newRelation.Relationship);
            }

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            // Act
            RelationshipBasedResult relationshipBasedResult
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson1 });

            // Assert
            Assert.IsFalse(relationshipBasedResult.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResult.Results.Count);
            Assert.IsTrue(relationshipBasedResult.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson1.ID));
            Assert.AreEqual(1, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(0, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().NotLoadedResults.Count);
            Assert.AreEqual(1000, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.AreEqual(newUnpublishPerson2.ID, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
        }

        [TestCategory("جستجوی رابطه")]
        [TestMethod]
        public async Task CheckRelationshipNumber()
        {
            // Assign

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
                RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson0,
                    currentObject, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            }

            // Act
            RelationshipBasedResult relationshipBasedResult
                = await Workspace.Logic.Search.SearchAround.GetRelatedEntities(new KWObject[] { newUnpublishPerson0 });
            // Assert
            Assert.IsFalse(relationshipBasedResult.IsResultsCountMoreThanThreshold);
            Assert.AreEqual(1, relationshipBasedResult.Results.Count);
            Assert.IsTrue(relationshipBasedResult.Results.Select(r => r.SearchedObject.ID).Contains(newUnpublishPerson0.ID));
            Assert.AreEqual(Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson0.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(100 - Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson0.ID).FirstOrDefault().NotLoadedResults.Count);
            Assert.AreEqual(1, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson0.ID).FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.AreEqual(1, relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson0.ID).FirstOrDefault().NotLoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(unpublishedObjects.Select(o=>o.ID).Contains(relationshipBasedResult.Results.Where(r => r.SearchedObject.ID == newUnpublishPerson0.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID));
        }        
    }
}
