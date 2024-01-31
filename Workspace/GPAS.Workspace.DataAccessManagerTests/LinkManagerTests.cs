using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;
using LinkDirection = GPAS.Workspace.Entities.KWLinks.LinkDirection;

namespace GPAS.Workspace.DataAccessManager.Tests
{
    [TestClass()]
    public class LinkManagerTests : DamTests
    {
        private void ShimCreateNewRelationshipPreparation()
        {
            ShimCreateNewRelationshipPreparation(new long[] { });
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewRelationId = (wsc) => { return GenerateSupposalStoredRelationshipId(); };
        }
        private void ShimCreateNewRelationshipPreparation(long newID)
        {
            ShimCreateNewRelationshipPreparation(new long[] { newID });
        }
        private void ShimCreateNewRelationshipPreparation(long[] newIdRange)
        {
            int idRangeIndex = 0;
            ShimRemoteServiceClientCreateAndClose();
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewObjectId = (wsc) => { return GenerateSupposalStoredObjectId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return GenerateSupposalStoredPropertyId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewRelationId = (wsc) => { return newIdRange[idRangeIndex++]; };
            Fakes.ShimSystem.GetOntology = () => { return new Ontology.Ontology(); };
            Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (onto) => { return "زمان_و_موقعیت_جغرافیایی"; };
        }

        [TestInitialize]
        public void Init()
        {
            ObjectManager.Initialization("نوع برچسب آزمایشی", "نوع رابطه عضویت در گروه آزمایشی");
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
            LinkManager.DiscardChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
            LinkManager.DiscardChanges();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUnpublishedRelationship_ArgNullInput_ThrowsException()
        {
            // Act
            LinkManager.IsUnpublishedRelationship(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_ArgNullInput_ThrowsException()
        {
            // Act
            LinkManager.CommitUnpublishedChanges(null, 0);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRelationshipFromReterievedData_Arg1NullInput_ThrowsException()
        {
            // Act
            LinkManager.GetRelationshipFromReterievedData(null, "۱۲۳", 110, 120);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetRelationshipFromReterievedData_Arg2NullInput_ThrowsException()
        {
            // Act
            LinkManager.GetRelationshipFromReterievedData(new KRelationship(), null, 110, 120);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRelationshipBaseLinkFromRetrievedDataAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await LinkManager.GetRelationshipBaseLinkFromRetrievedDataAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewRelationshipBaseLink_Arg1NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewRelationshipBaseLink(null, new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewRelationshipBaseLink_Arg2NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewRelationshipBaseLink(new KWObject(), null, "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewRelationshipBaseLink_Arg3NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), null, "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewRelationshipBaseLink_Arg4NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۲۳۴", null, LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg1NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (null, new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg2NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (new KWObject(), null, "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg3NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (new KWObject(), new KWObject(), null, "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg4NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (new KWObject(), new KWObject(), "۲۳۴", null, LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg8NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, null, "۴۵۶");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewEventBaseLink_Arg9NullInput_ThrowsException()
        {
            // Act
            LinkManager.CreateNewEventBaseLink
                (new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRelationshipRangeByIdAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await LinkManager.GetRelationshipRangeByIdAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRelationshipsBySourceObjectAsync_Arg1NullInput_ThrowsException()
        {
            // Act
            await LinkManager.GetRelationshipsBySourceObjectAsync(null, "۱۲۳");
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRelationshipsBySourceObjectAsync_Arg2NullInput_ThrowsException()
        {
            // Act
            await LinkManager.GetRelationshipsBySourceObjectAsync(new KWObject(), null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetRelationshipsByTargetObjectAsync_Arg2NullInput_ThrowsException()
        {
            // Act
            await LinkManager.GetRelationshipsByTargetObjectIdAsync(123, null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        public void CreateNewRelationshipBaseLink_AssignsGeneratedIdToInnerRelationship()
        {
            // Arrange
            RelationshipBasedKWLink newLink;
            long supposalId = GenerateSupposalStoredRelationshipId();
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation(supposalId);
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                newLink = LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
            }
            // Assert
            Assert.AreEqual(supposalId, newLink.Relationship.ID);
        }
        [TestMethod()]
        public void CreateNewEventBaseLink_AssignsGeneratedIdsToInnerRelationships()
        {
            // Arrange
            EventBasedKWLink newLink;
            long[] supposalIds = GenerateSupposalStoredRelationshipId(2);
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation(supposalIds);
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                newLink = LinkManager.CreateNewEventBaseLink
                    (new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            }
            // Assert
            Assert.AreEqual(supposalIds[0], newLink.FirstRelationship.ID);
            Assert.AreEqual(supposalIds[1], newLink.SecondRelationship.ID);
        }

        [TestMethod()]
        public void CreateNewRelationship_InnerRelationshipKnowRelationshipAsUnpublished()
        {
            // Arrange
            RelationshipBasedKWLink newLink;
            bool isUnpublished;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                newLink = LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
                isUnpublished = LinkManager.IsUnpublishedRelationship(newLink.Relationship);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public void CreateNewEventBaseLink_MakeIntermediaryEvent()
        {
            // Arrange
            EventBasedKWLink newLink;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                newLink = LinkManager.CreateNewEventBaseLink
                    (new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
            }
            // Assert
            Assert.IsNotNull(newLink.IntermediaryEvent);
        }

        [TestMethod()]
        public async Task CreatedRelationshipBasedLink_BasedOnInnerRelationshipMayBeAccessableById()
        {
            // Arrange
            RelationshipBasedKWLink testLink;
            IEnumerable<RelationshipBasedKWLink> givenLinksForId;
            RelationshipBasedKWLink givenLink;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);

                givenLinksForId = await LinkManager.GetRelationshipRangeByIdAsync(new List<long> { testLink.Relationship.ID });
                givenLink = givenLinksForId.Single(l => l.Relationship.ID.Equals(testLink.Relationship.ID));
            }
            // Assert
            Assert.AreEqual(testLink.Relationship, givenLink.Relationship);
        }
        [TestMethod()]
        public async Task CreatedRelationshipBasedLink_BasedOnInnerRelationshipMayBeAccessableBySourceObject()
        {
            // Arrange
            string testRelationshipTypeUri = "نوع رابطه آزمایشی";
            RelationshipBasedKWLink testLink;
            IEnumerable<RelationshipBasedKWLink> givenLinksForSourceObject;
            RelationshipBasedKWLink givenLink;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                KWObject sourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject targetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(sourceObject, targetObject, testRelationshipTypeUri, "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);

                givenLinksForSourceObject = await LinkManager.GetRelationshipsBySourceObjectAsync(sourceObject, testRelationshipTypeUri);
                givenLink = givenLinksForSourceObject.Single(l => l.Relationship.ID.Equals(testLink.Relationship.ID));
            }
            // Assert
            Assert.AreEqual(testLink.Relationship, givenLink.Relationship);
        }
        [TestMethod()]
        public async Task CreatedRelationshipBasedLink_BasedOnInnerRelationshipMayBeAccessableByTargetObjectId()
        {
            // Arrange
            string testRelationshipTypeUri = "نوع رابطه آزمایشی";
            RelationshipBasedKWLink testLink;
            IDictionary<KWRelationship, KWObject> givenRelationshipsForTargetObject;
            KWRelationship givenRelationship;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                KWObject sourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject targetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(sourceObject, targetObject, testRelationshipTypeUri, "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);

                givenRelationshipsForTargetObject = await LinkManager.GetRelationshipsByTargetObjectIdAsync(targetObject.ID, testRelationshipTypeUri);
                givenRelationship = givenRelationshipsForTargetObject.Single(l => l.Key.ID.Equals(testLink.Relationship.ID)).Key;
            }
            // Assert
            Assert.AreEqual(testLink.Relationship, givenRelationship);
        }

        [TestMethod()]
        public async Task CreatedRelationshipBasedLink_InnerRelationshipMayAppearedInUnpublishedChanges()
        {
            // Arrange
            KWObject sourceObject, targetObject;
            RelationshipBasedKWLink testLink;
            UnpublishedRelationshipChanges changes;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                sourceObject = new KWObject() { ID = GenerateSupposalStoredObjectId() };
                targetObject = new KWObject() { ID = GenerateSupposalStoredObjectId() };
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (id, applyResolutionTree) =>
                {
                    await Task.Delay(0);
                    if (id == sourceObject.ID)
                        return sourceObject;
                    else if (id == targetObject.ID)
                        return targetObject;
                    else
                        return new KWObject();
                };

                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(sourceObject, targetObject, "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
                changes = await LinkManager.GetUnpublishedChangesAsync();
            }
            // Assert
            var equivalentRelationship = changes.AddedRelationships.Single(r => r.Relationship.Equals(testLink.Relationship));
            Assert.AreEqual(sourceObject, equivalentRelationship.Source);
            Assert.AreEqual(targetObject, equivalentRelationship.Target);
        }
        [TestMethod()]
        public async Task CreatedEventBasedLink_InnerRelationshipsMayAppearedInUnpublishedChanges()
        {
            // Arrange
            KWObject sourceObject, targetObject;
            EventBasedKWLink testLink;
            UnpublishedRelationshipChanges changes;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                sourceObject = new KWObject() { ID = GenerateSupposalStoredObjectId() };
                targetObject = new KWObject() { ID = GenerateSupposalStoredObjectId() };
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (id, applyResolutionTree) =>
                {
                    await Task.Delay(0);
                    if (id == sourceObject.ID)
                        return sourceObject;
                    else if (id == targetObject.ID)
                        return targetObject;
                    else
                        return new KWObject();
                };

                // Act
                testLink = LinkManager.CreateNewEventBaseLink
                (sourceObject, targetObject, "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now, "۳۴۵", "۴۵۶");
                changes = await LinkManager.GetUnpublishedChangesAsync();
            }
            // Assert
            var firstEquivalentRelation = changes.AddedRelationships.Single(r => r.Relationship.Equals(testLink.FirstRelationship));
            Assert.AreEqual(sourceObject, firstEquivalentRelation.Source);
            var secondEquivalentRelation = changes.AddedRelationships.Single(r => r.Relationship.Equals(testLink.SecondRelationship));
            Assert.AreEqual(targetObject, secondEquivalentRelation.Target);
        }

        [TestMethod()]
        public async Task CreatedRelationship_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            RelationshipBasedKWLink testLink;
            long relationshipIdAfterSupposalPublish = GenerateSupposalStoredRelationshipId();
            UnpublishedRelationshipChanges unpublishedChangesAfterCommit;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (id, applyResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
                LinkManager.CommitUnpublishedChanges(new long[] { testLink.Relationship.ID }, 100);
                unpublishedChangesAfterCommit = await LinkManager.GetUnpublishedChangesAsync();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedRelationships
                .Where(r => r.Relationship.Equals(testLink.Relationship))
                .Count() >= 1);
        }

        [TestMethod()]
        public void CreatedRelationship_AfterCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            RelationshipBasedKWLink testLink;
            long relationshipIdAfterSupposalPublish = GenerateSupposalStoredRelationshipId();
            bool isTestRelationshipUnpublished;
            // Fake Arranges
            using (ShimsContext.Create())
            {
                ShimCreateNewRelationshipPreparation();
                Fakes.ShimObjectManager.GetObjectByIdAsyncInt64Boolean = async (a, appluResolutionTree) =>
                {
                    await Task.Delay(0);
                    return new KWObject();
                };

                // Act
                testLink = LinkManager.CreateNewRelationshipBaseLink(new KWObject(), new KWObject(), "۱۲۳", "۲۳۴", LinkDirection.TargetToSource, DateTime.Now, DateTime.Now);
                LinkManager.CommitUnpublishedChanges(new long[] { testLink.Relationship.ID }, 100);
                isTestRelationshipUnpublished = LinkManager.IsUnpublishedRelationship(testLink.Relationship);
            }
            // Assert
            Assert.IsFalse(isTestRelationshipUnpublished);
        }

        [TestMethod()]
        public async Task StoredRelationship_MayBeAccessableByItsId()
        {
            // Arrange
            long supposalStoredRelationshipId = GenerateSupposalStoredRelationshipId();
            IEnumerable<RelationshipBasedKWLink> givenRelationshipsForId;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewRelationshipPreparation();
                KWObject supposalSourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject supposalTargetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetRelationshipListByIdAsyncInt64Array = async (sc, id) =>
                    {
                        if (id[0] != supposalStoredRelationshipId)
                            throw new InvalidOperationException("Supposal input not taken to fake remote service");

                        await Task.Delay(0);
                        var source = new KObject()
                        {
                            Id = supposalSourceObject.ID,
                            TypeUri = supposalSourceObject.TypeURI,
                            IsGroup = false
                        };
                        var target = new KObject()
                        {
                            Id = supposalTargetObject.ID,
                            TypeUri = supposalTargetObject.TypeURI,
                            IsGroup = false
                        };
                        var relationship = new KRelationship()
                        {
                            Id = supposalStoredRelationshipId,
                            Direction = Dispatch.Entities.Concepts.LinkDirection.TargetToSource,
                            Description = "توضیحات فرضی",
                            TimeBegin = DateTime.MinValue,
                            TimeEnd = DateTime.MaxValue
                        };
                        var link = new RelationshipBaseKlink()
                        {
                            Source = source,
                            Target = target,
                            TypeURI = "نوع رابطه آزمایشی",
                            Relationship = relationship
                        };
                        return new RelationshipBaseKlink[] { link };
                    };

                // Act
                givenRelationshipsForId = await LinkManager.GetRelationshipRangeByIdAsync(new List<long> { supposalStoredRelationshipId });
            }
            // Assert
            givenRelationshipsForId.Single(l => l.Relationship.ID.Equals(supposalStoredRelationshipId));
            Assert.AreEqual(1, givenRelationshipsForId.Count());
        }
        [TestMethod()]
        public async Task StoredRelationship_MayBeAccessableByItsSourceObject()
        {
            // Arrange
            long supposalStoredRelationshipId = GenerateSupposalStoredRelationshipId();
            string supposalStoredRelationshipTypeUri = "نوع رابطه آزمایشی";
            // ثبت فرضی اشیا مبدا و مقصد رابطه
            IEnumerable<RelationshipBasedKWLink> givenRelationshipsForSourceObject;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewRelationshipPreparation();
                KWObject supposalSourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject supposalTargetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetRelationshipsBySourceObjectAsyncInt64String = async (sc, objId, relationshipType) =>
                    {
                        if (objId != supposalSourceObject.ID || relationshipType != supposalStoredRelationshipTypeUri)
                            throw new InvalidOperationException("Supposal input not taken to fake remote service");

                        await Task.Delay(0);
                        var source = new KObject()
                        {
                            Id = supposalSourceObject.ID,
                            TypeUri = supposalSourceObject.TypeURI,
                            IsGroup = false
                        };
                        var target = new KObject()
                        {
                            Id = supposalTargetObject.ID,
                            TypeUri = supposalTargetObject.TypeURI,
                            IsGroup = false
                        };
                        var relationship = new KRelationship()
                        {
                            Id = supposalStoredRelationshipId,
                            Direction = Dispatch.Entities.Concepts.LinkDirection.TargetToSource,
                            Description = "توضیحات فرضی",
                            TimeBegin = DateTime.MinValue,
                            TimeEnd = DateTime.MaxValue
                        };
                        var link = new RelationshipBaseKlink()
                        {
                            Source = source,
                            Target = target,
                            TypeURI = supposalStoredRelationshipTypeUri,
                            Relationship = relationship
                        };
                        return new RelationshipBaseKlink[] { link };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(supposalSourceObject) || obj.Equals(supposalTargetObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                givenRelationshipsForSourceObject = await LinkManager.GetRelationshipsBySourceObjectAsync(supposalSourceObject, supposalStoredRelationshipTypeUri);
            }
            // Assert
            givenRelationshipsForSourceObject.Single(l => l.Relationship.ID.Equals(supposalStoredRelationshipId));
            Assert.AreEqual(1, givenRelationshipsForSourceObject.Count());
        }
        [TestMethod()]
        public async Task StoredRelationship_MayBeAccessableByItsTargetObject()
        {
            // Arrange
            long supposalStoredRelationshipId = GenerateSupposalStoredRelationshipId();
            string supposalStoredRelationshipTypeUri = "نوع رابطه آزمایشی";
            // ثبت فرضی اشیا مبدا و مقصد رابطه
            IDictionary<KWRelationship, KWObject> givenRelationshipsForTargetObject;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewRelationshipPreparation();
                KWObject supposalSourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject supposalTargetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetLinksSourcedByObjectAsyncKObjectString = async (sc, targetObj, relationshipType) =>
                    {
                        if (targetObj.Id != supposalTargetObject.ID || relationshipType != supposalStoredRelationshipTypeUri)
                            throw new InvalidOperationException("Supposal input not taken to fake remote service");

                        await Task.Delay(0);
                        var source = new KObject()
                        {
                            Id = supposalSourceObject.ID,
                            TypeUri = supposalSourceObject.TypeURI,
                            IsGroup = false
                        };
                        var target = new KObject()
                        {
                            Id = supposalTargetObject.ID,
                            TypeUri = supposalTargetObject.TypeURI,
                            IsGroup = false
                        };
                        var relationship = new KRelationship()
                        {
                            Id = supposalStoredRelationshipId,
                            Direction = Dispatch.Entities.Concepts.LinkDirection.TargetToSource,
                            Description = "توضیحات فرضی",
                            TimeBegin = DateTime.MinValue,
                            TimeEnd = DateTime.MaxValue
                        };
                        var link = new RelationshipBaseKlink()
                        {
                            Source = source,
                            Target = target,
                            TypeURI = supposalStoredRelationshipTypeUri,
                            Relationship = relationship
                        };
                        return new RelationshipBaseKlink[] { link };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectInt64 = (id) =>
                {
                    if (id.Equals(supposalSourceObject.ID) || id.Equals(supposalTargetObject.ID))
                        return false;
                    else
                        throw new InvalidOperationException();
                };

                // Act
                givenRelationshipsForTargetObject = await LinkManager.GetRelationshipsByTargetObjectIdAsync(supposalTargetObject.ID, supposalStoredRelationshipTypeUri);
            }
            // Assert
            givenRelationshipsForTargetObject.Single(l => l.Key.ID.Equals(supposalStoredRelationshipId));
            Assert.AreEqual(1, givenRelationshipsForTargetObject.Count());
        }

        //// Multiple results for 'retrieval' methods can be test too!

        [TestMethod()]
        public async Task StoredRelationship_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalStoredRelationshipId = GenerateSupposalStoredRelationshipId();
            IEnumerable<RelationshipBasedKWLink> givenRelationshipsForId;
            RelationshipBasedKWLink givenRelationship;
            bool isRetrievedRelationshipKnownAsUnpublished;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewRelationshipPreparation();
                KWObject supposalSourceObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نمونه شئ آزمایشی ۱");
                KWObject supposalTargetObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نمونه شئ آزمایشی ۲");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetRelationshipListByIdAsyncInt64Array = async (sc, id) =>
                    {
                        if (id[0] != supposalStoredRelationshipId)
                            throw new InvalidOperationException("Supposal input not taken to fake remote service");

                        await Task.Delay(0);
                        var source = new KObject()
                        {
                            Id = supposalSourceObject.ID,
                            TypeUri = supposalSourceObject.TypeURI,
                            IsGroup = false
                        };
                        var target = new KObject()
                        {
                            Id = supposalTargetObject.ID,
                            TypeUri = supposalTargetObject.TypeURI,
                            IsGroup = false
                        };
                        var relationship = new KRelationship()
                        {
                            Id = supposalStoredRelationshipId,
                            Direction = Dispatch.Entities.Concepts.LinkDirection.TargetToSource,
                            Description = "توضیحات فرضی",
                            TimeBegin = DateTime.MinValue,
                            TimeEnd = DateTime.MaxValue
                        };
                        var link = new RelationshipBaseKlink()
                        {
                            Source = source,
                            Target = target,
                            TypeURI = "نوع رابطه آزمایشی",
                            Relationship = relationship
                        };
                        return new RelationshipBaseKlink[] { link };
                    };

                // Act
                givenRelationshipsForId = await LinkManager.GetRelationshipRangeByIdAsync(new List<long> { supposalStoredRelationshipId });
                givenRelationship = givenRelationshipsForId.Single(l => l.Relationship.ID.Equals(supposalStoredRelationshipId));
                isRetrievedRelationshipKnownAsUnpublished = LinkManager.IsUnpublishedRelationship(givenRelationship.Relationship);
            }
            // Assert
            Assert.IsFalse(isRetrievedRelationshipKnownAsUnpublished);
        }
    }
}