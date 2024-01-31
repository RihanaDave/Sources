using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.Tests
{
    [TestClass()]
    public class ObjectManagerTests : DamTests
    {
        private void ShimCreateNewObjectPreparation(long newObjectID)
        {
            ShimCreateNewObjectPreparation(new long[] { newObjectID });
        }
        private void ShimCreateNewObjectPreparation(long[] newObjectIdRange)
        {
            int objectIdRangeIndex = 0;
            ShimRemoteServiceClientCreateAndClose();
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewObjectId = (wsc) => { return newObjectIdRange[objectIdRangeIndex++]; };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return GenerateSupposalStoredPropertyId(); };
            ShimKWPropertyGenerationRequirements();
        }
        private static void ShimKWPropertyGenerationRequirements()
        {
            Fakes.ShimSystem.GetOntology = () => { return new Ontology.Ontology(); };
            Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (onto) => { return "زمان_و_موقعیت_جغرافیایی"; };
        }

        private void ShimCreateNewObjectPreparationIncludingRelationship(long[] newObjectIdRange)
        {
            ShimCreateNewObjectPreparation(newObjectIdRange);
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewRelationId = (wsc) => { return GenerateSupposalStoredRelationshipId(); };
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
        public void IsUnpublishedObject_ArgNullInput_ThrowsException()
        {
            // Act
            ObjectManager.IsUnpublishedObject(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialization_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.Initialization(null, "نوع رابطه عضویت در گروه آزمایشی");
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialization_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.Initialization("نوع برچسب آزمایشی", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CommitUnpublishedChanges(null, new long[] { });
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CommitUnpublishedChanges(new long[] { }, null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetObjectFromRetrievedDataAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await ObjectManager.GetObjectFromRetrievedDataAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewObject_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewObject(null, "نمونه شئ آزمایشی");
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewObject_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewObject("نوع شئ آزمایشی", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupMasterObject_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupMasterObject(null, "نمونه شئ آزمایشی");
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupMasterObject_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupOfObjects_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupOfObjects
                (null, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupOfObjects_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, null, "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupOfObjects_Arg3NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", null, "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewGroupOfObjects_Arg4NullInput_ThrowsException()
        {
            // Act
            ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", null, DateTime.Now, DateTime.Now);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChangeDisplayNameOfObject_Arg1NullInput_ThrowsException()
        {
            // Act
            ObjectManager.ChangeDisplayNameOfObject(null, "۱۲۳");
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ChangeDisplayNameOfObject_Arg2NullInput_ThrowsException()
        {
            // Act
            ObjectManager.ChangeDisplayNameOfObject(new KWObject(), null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetObjectsListByIdAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await ObjectManager.GetObjectsListByIdAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNewObject_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNewGroupMasterObject_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNewGroupOfObjects_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                ObjectManager.CreateNewGroupOfObjects
                    (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ChangeDisplayNameOfObject_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                ObjectManager.ChangeDisplayNameOfObject(new KWObject(), "۱۲۳");
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetObjectByIdAsync_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                await ObjectManager.GetObjectByIdAsync(123);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetObjectsListByIdAsync_WithoutInitialization_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {// Fake Arranges!
                Fakes.ShimObjectManager.IsInitialized = () =>
                { return false; };

                // Act
                await ObjectManager.GetObjectsListByIdAsync(new long[] { 123 });
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        public void CreateNewObject_AssignsGeneratedObjectId()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject newObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
            }
            // Assert
            Assert.AreEqual(supposalID, newObject.ID);
        }

        [TestMethod()]
        public void CreateNewGroupMasterObject_AssignsGeneratedObjectId()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject newGroupMasterObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
            }
            // Assert
            Assert.AreEqual(supposalID, newGroupMasterObject.ID);
        }

        [TestMethod()]
        public void CreateNewGroupOfObjects_AssignsGeneratedObjectId()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject newGroupMasterObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                    (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            }
            // Assert
            Assert.AreEqual(supposalID, newGroupMasterObject.ID);
        }

        [TestMethod()]
        public void CreateNewObject_KnowObjectAsUnpublished()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject newObject;
            bool isUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                isUnpublished = ObjectManager.IsUnpublishedObject(newObject);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public void CreateNewGroupMasterObject_KnowObjectAsUnpublished()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject newGroupMasterObject;
            bool isUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                isUnpublished = ObjectManager.IsUnpublishedObject(newGroupMasterObject);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public void CreateNewGroupMasterObject_ReturnsInstanceWithNoSubGroup()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject newGroupMasterObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
            }
            // Assert
            Assert.AreEqual(0, (newGroupMasterObject as GroupMasterKWObject).GetSubGroupObjectsCount);
        }

        [TestMethod()]
        public void CreateNewGroupOfObjects_KnowObjectAsUnpublished()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject newGroupMasterObject;
            bool isUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                newGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                isUnpublished = ObjectManager.IsUnpublishedObject(newGroupMasterObject);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public void CreateNewGroupOfObjects_ReturnsInstanceWithInputSubGroups()
        {
            // Arrange
            long[] supposalIDs = GenerateSupposalStoredObjectIdRange(3);
            GroupMasterKWObject newGroupMasterObject;
            KWObject subgroup1, subgroup2;
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparationIncludingRelationship(supposalIDs);
                subgroup1 = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۱", "نام نمایشی آزمایشی ۱");
                subgroup2 = ObjectManager.CreateNewObject("نوع شئ آزمایشی ۲", "نام نمایشی آزمایشی ۲");
                List<KWObject> subgroups = new List<KWObject>
                {
                    subgroup1,
                    subgroup2
                };

                // Act
                newGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                    (subgroups, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
            }
            // Assert
            Assert.AreEqual(2, (newGroupMasterObject as GroupMasterKWObject).GetSubGroupObjectsCount);
            Assert.IsTrue((newGroupMasterObject as GroupMasterKWObject).GetSubGroupObjects().Contains(subgroup1));
            Assert.IsTrue((newGroupMasterObject as GroupMasterKWObject).GetSubGroupObjects().Contains(subgroup2));
        }

        [TestMethod()]
        public async Task CreatedObject_MayBeAccessableUsingGetObjectByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            KWObject givenObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                givenObject = await ObjectManager.GetObjectByIdAsync(testObject.ID);
            }
            // Assert
            Assert.AreEqual(testObject, givenObject);
        }

        [TestMethod()]
        public async Task CreatedObject_MayBeAccessableUsingGetObjectsListByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            IEnumerable<KWObject> givenObjects;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                givenObjects = await ObjectManager.GetObjectsListByIdAsync(new long[] { testObject.ID });
            }
            // Assert
            Assert.IsTrue(givenObjects.Contains(testObject));
            Assert.AreEqual(1, givenObjects.Count());
        }

        [TestMethod()]
        public async Task CreatedGroupMasterObject_MayBeAccessableUsingGetObjectByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            KWObject givenObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                givenObject = await ObjectManager.GetObjectByIdAsync(testGroupMasterObject.ID);
            }
            // Assert
            Assert.AreEqual(testGroupMasterObject, givenObject);
        }

        [TestMethod()]
        public async Task CreatedGroupMasterObject_MayBeAccessableUsingGetObjectsListByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            IEnumerable<KWObject> givenObjects;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                givenObjects = await ObjectManager.GetObjectsListByIdAsync(new long[] { testGroupMasterObject.ID });
            }
            // Assert
            Assert.IsTrue(givenObjects.Contains(testGroupMasterObject));
            Assert.AreEqual(1, givenObjects.Count());
        }

        [TestMethod()]
        public async Task CreatedGroupOfObjects_MayBeAccessableUsingGetObjectByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            KWObject givenObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                givenObject = await ObjectManager.GetObjectByIdAsync(testGroupMasterObject.ID);
            }
            // Assert
            Assert.AreEqual(testGroupMasterObject, givenObject);
        }

        [TestMethod()]
        public async Task CreatedGroupOfObjects_MayBeAccessableUsingGetObjectsListByIdAsync()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            IEnumerable<KWObject> givenObjects;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                givenObjects = await ObjectManager.GetObjectsListByIdAsync(new long[] { testGroupMasterObject.ID });
            }
            // Assert
            Assert.IsTrue(givenObjects.Contains(testGroupMasterObject));
            Assert.AreEqual(1, givenObjects.Count());
        }

        [TestMethod()]
        public void CreatedObject_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }

        [TestMethod()]
        public void CreatedObject_AfterModifyDisplayName_MayAppearedInAddedUnpublishedObjects()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testObject, "نام نمایشی جدید آزمایشی");
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterModifyDisplayName_MayAppearedInAddedUnpublishedObjects()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید آزمایشی");
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterModifyDisplayName_MayAppearedInAddedUnpublishedObjects()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            UnpublishedObjectChanges changes;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید آزمایشی");
                changes = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(changes.ResolvedObjects.Any());
        }

        [TestMethod()]
        public void CreatedObject_AfterCommitChangesWithoutMappingFakeIdBefore_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            var fakeObjectIds = new List<long>();
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                // نگاشت شناسه‌های نامعتبر به معتبر جهت انجام تست افزوده نشده
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(unpublishedChangesAfterCommit.AddedObjects.Contains(testObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterCommitChangesWithoutMappingFakeIdBefore_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>();
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                // نگاشت شناسه‌های نامعتبر به معتبر جهت انجام تست افزوده نشده
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterCommitChangesWithoutMappingFakeIdBefore_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>();
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                // نگاشت شناسه‌های نامعتبر به معتبر جهت انجام تست افزوده نشده
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
        }

        [TestMethod()]
        public void CreatedObject_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                fakeObjectIds.Add(testObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }

        [TestMethod()]
        public void CreatedObject_AfterCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                fakeObjectIds.Add(testObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
            Assert.IsFalse(ObjectManager.GetUnpublishedChanges().ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
            Assert.IsFalse(ObjectManager.GetUnpublishedChanges().ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
            Assert.IsFalse(ObjectManager.GetUnpublishedChanges().ResolvedObjects.Any());
        }

        [TestMethod()]
        public void CreatedObject_AfterModifyDisplayNameAndCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterModifyDisplayNameAndCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterModifyDisplayNameAndCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            UnpublishedObjectChanges unpublishedChangesAfterCommit;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                unpublishedChangesAfterCommit = ObjectManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedObjects.Contains(testGroupMasterObject));
            Assert.IsFalse(unpublishedChangesAfterCommit.ResolvedObjects.Any());
        }

        [TestMethod()]
        public void CreatedObject_AfterModifyDisplayNameAndCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterModifyDisplayNameAndCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterModifyDisplayNameAndCommitChanges_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testGroupMasterObject;
            var fakeObjectIds = new List<long>(1);
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, "نام نمایشی جدید شئ آزمایشی");
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.IsFalse(isTestObjectUnpublished);
        }

        [TestMethod()]
        public void CreatedObject_AfterModifyDisplayNameAndCommitChanges_MayHaveNewValue()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testObject;
            string objectNewDisplayName = "نام نمایشی جدید شئ آزمایشی";
            var fakeObjectIds = new List<long>();
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testObject, objectNewDisplayName);
                fakeObjectIds.Add(testObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testObject);
            }
            // Assert
            Assert.AreEqual(objectNewDisplayName, testObject.DisplayName.Value);
        }
        [TestMethod()]
        public void CreatedGroupMasterObject_AfterModifyDisplayNameAndCommitChanges_MayHaveNewValue()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            GroupMasterKWObject testGroupMasterObject;
            string objectNewDisplayName = "نام نمایشی جدید شئ آزمایشی";
            var fakeObjectIds = new List<long>();
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupMasterObject("نوع شئ آزمایشی", "نام نمایشی آزمایشی");
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, objectNewDisplayName);
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.AreEqual(objectNewDisplayName, testGroupMasterObject.DisplayName.Value);
        }
        [TestMethod()]
        public void CreatedGroupOfObjects_AfterModifyDisplayNameAndCommitChanges_MayHaveNewValue()
        {
            // Arrange
            long supposalID = GenerateSupposalStoredObjectId();
            KWObject testGroupMasterObject;
            string objectNewDisplayName = "نام نمایشی جدید شئ آزمایشی";
            var fakeObjectIds = new List<long>();
            bool isTestObjectUnpublished;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewObjectPreparation(supposalID);
                testGroupMasterObject = ObjectManager.CreateNewGroupOfObjects
                (new KWObject[] { }, "نوع شئ آزمایشی", "نام نمایشی آزمایشی", "عنوان رابطه آزمایشی", DateTime.Now, DateTime.Now);
                ObjectManager.ChangeDisplayNameOfObject(testGroupMasterObject, objectNewDisplayName);
                fakeObjectIds.Add(testGroupMasterObject.ID);
                ObjectManager.CommitUnpublishedChanges(fakeObjectIds, new long[] { });
                isTestObjectUnpublished = ObjectManager.IsUnpublishedObject(testGroupMasterObject);
            }
            // Assert
            Assert.AreEqual(objectNewDisplayName, testGroupMasterObject.DisplayName.Value);
        }

        [TestMethod()]
        public async Task StoredObject_MayBeAccessableUsingGetObjectByIdMethod()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            KWObject givenObjectForId;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var obj = new KObject()
                        {
                            Id = objectIdForSupposalObject,
                            TypeUri = "نوع شئ آزمایشی"
                        };
                        return new KObject[] { obj };
                    };

                // Act
                givenObjectForId = await ObjectManager.GetObjectByIdAsync
                    (objectIdForSupposalObject);
            }
            // Assert
            Assert.AreEqual(objectIdForSupposalObject, givenObjectForId.ID);
        }
        [TestMethod()]
        public async Task StoredGroupMasterObject_MayBeAccessableUsingGetObjectByIdMethod()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            GroupMasterKWObject givenObjectForId;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var obj = new KObject()
                        {
                            Id = objectIdForSupposalObject,
                            TypeUri = "نوع شئ آزمایشی"
                        };
                        return new KObject[] { obj };
                    };
                Fakes.ShimLinkManager.GetRelationshipsByTargetObjectIdAsyncInt64String = async (i, t) =>
                {
                    await Task.Delay(0);
                    return new Dictionary<KWRelationship, KWObject> { };
                };

                // Act
                givenObjectForId = (await ObjectManager.GetObjectByIdAsync
                    (objectIdForSupposalObject)) as GroupMasterKWObject;
            }
            // Assert
            Assert.AreEqual(objectIdForSupposalObject, givenObjectForId.ID);
        }

        [TestMethod()]
        public async Task StoredObject_MayBeAccessableUsingGetObjectsListByIdMethod()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            long propertyLabelIdForSupposalObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWObject> givenObjectsForId;
            KObject supposalretrivalObject = new KObject()
            {
                Id = objectIdForSupposalObject,
                TypeUri = "نوع شئ آزمایشی",
                LabelPropertyID = propertyLabelIdForSupposalObject
            };
            KProperty supposalRetrievalProperty = new KProperty()
            {
                Id = propertyLabelIdForSupposalObject,
                TypeUri = "برچسب",
                Owner = supposalretrivalObject,
                Value = "نام نمایشی اولیه"
            };
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ShimKWPropertyGenerationRequirements();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KObject[] { supposalretrivalObject };
                    };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KProperty[] { supposalRetrievalProperty };
                    };
                // Act
                givenObjectsForId = await ObjectManager.GetObjectsListByIdAsync
                    (new long[] { objectIdForSupposalObject });
            }
            // Assert
            givenObjectsForId
                .Single(m => m.ID.Equals(objectIdForSupposalObject)); // درصورت عدم وجود یک شئ به ازای شناسه، استثناء صادر می‌شود
        }
        [TestMethod()]
        public async Task StoredGroupMasterObject_MayBeAccessableUsingGetObjectsListByIdMethod()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            long propertyLabelIdForSupposalObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWObject> givenObjectsForId;
            KObject supposalretrivalObject = new KObject()
            {
                Id = objectIdForSupposalObject,
                TypeUri = "نوع شئ آزمایشی",
                LabelPropertyID = propertyLabelIdForSupposalObject
            };
            KProperty supposalRetrievalProperty = new KProperty()
            {
                Id = propertyLabelIdForSupposalObject,
                TypeUri = "برچسب",
                Owner = supposalretrivalObject,
                Value = "نام نمایشی اولیه"
            };
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ShimKWPropertyGenerationRequirements();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KObject[] { supposalretrivalObject };
                    };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KProperty[] { supposalRetrievalProperty };
                    };
                Fakes.ShimLinkManager.GetRelationshipsByTargetObjectIdAsyncInt64String = async (i, t) =>
                {
                    await Task.Delay(0);
                    return new Dictionary<KWRelationship, KWObject> { };
                };

                // Act
                givenObjectsForId = await ObjectManager.GetObjectsListByIdAsync
                    (new long[] { objectIdForSupposalObject });
            }
            // Assert
            givenObjectsForId
                .Single(m => m.ID.Equals(objectIdForSupposalObject)); // درصورت عدم وجود یک شئ به ازای شناسه، استثناء صادر می‌شود
        }

        // Multiple results for "GetObjectsListById" method can be test too!

        [TestMethod()]
        public async Task StoredObject_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            KWObject givenObjectForId;
            bool isRetrievedObjectKnownAsUnpublished;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var obj = new KObject()
                        {
                            Id = objectIdForSupposalObject,
                            TypeUri = "نوع شئ آزمایشی"
                        };
                        return new KObject[] { obj };
                    };

                // Act
                givenObjectForId = await ObjectManager.GetObjectByIdAsync
                    (objectIdForSupposalObject);
                isRetrievedObjectKnownAsUnpublished = ObjectManager.IsUnpublishedObject(givenObjectForId);
            }
            // Assert
            Assert.IsFalse(isRetrievedObjectKnownAsUnpublished);
        }
        [TestMethod()]
        public async Task StoredGroupMasterObject_MayNotKnownAsUnpublishedObject()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            GroupMasterKWObject givenObjectForId;
            bool isRetrievedObjectKnownAsUnpublished;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var obj = new KObject()
                        {
                            Id = objectIdForSupposalObject,
                            TypeUri = "نوع شئ آزمایشی"
                        };
                        return new KObject[] { obj };
                    };
                Fakes.ShimLinkManager.GetRelationshipsByTargetObjectIdAsyncInt64String = async (i, t) =>
                {
                    await Task.Delay(0);
                    return new Dictionary<KWRelationship, KWObject> { };
                };

                // Act
                givenObjectForId = (await ObjectManager.GetObjectByIdAsync
                    (objectIdForSupposalObject)) as GroupMasterKWObject;
                isRetrievedObjectKnownAsUnpublished = ObjectManager.IsUnpublishedObject(givenObjectForId);
            }
            // Assert
            Assert.IsFalse(isRetrievedObjectKnownAsUnpublished);
        }
        
        [TestMethod()]
        public async Task StoredObject_AfterModifyDisplayNameAndCommitTheModification_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            long propertyLabelIdForSupposalObject = GenerateSupposalStoredPropertyId();
            KWObject retrievedObject;
            UnpublishedObjectChanges objectChanges;
            UnpublishedPropertyChanges propertyChanges;
            KObject supposalretrivalObject = new KObject()
            {
                Id = objectIdForSupposalObject,
                TypeUri = "نوع شئ آزمایشی",
                LabelPropertyID = propertyLabelIdForSupposalObject
            };
            KProperty supposalRetrievalProperty = new KProperty()
            {
                Id = propertyLabelIdForSupposalObject,
                TypeUri = "برچسب",
                Owner = supposalretrivalObject,
                Value = "نام نمایشی اولیه"
            };
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ShimKWPropertyGenerationRequirements();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KObject[] { supposalretrivalObject };
                    };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KProperty[] { supposalRetrievalProperty };
                    };
                // Act
                retrievedObject = await ObjectManager.GetObjectByIdAsync(objectIdForSupposalObject);
                ObjectManager.ChangeDisplayNameOfObject(retrievedObject, "نام نمایشی جدید شئ آزمایشی");
                PropertyManager.CommitUnpublishedChanges(new long[] { }, new long[] { propertyLabelIdForSupposalObject }, -1);
                objectChanges = ObjectManager.GetUnpublishedChanges();
                propertyChanges = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(objectChanges.AddedObjects.Any());
            Assert.IsFalse(objectChanges.ResolvedObjects.Any());
            Assert.IsFalse(propertyChanges.AddedProperties.Any());
            Assert.IsFalse(propertyChanges.ModifiedProperties.Any());
        }
        [TestMethod()]
        public async Task StoredObject_AfterModifyDisplayNameWithoutCommitTheModification_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long objectIdForSupposalObject = GenerateSupposalStoredObjectId();
            long propertyLabelIdForSupposalObject = GenerateSupposalStoredPropertyId();
            KWObject retrievedObject;
            var emptyFakeObjectIds = new List<long>();
            UnpublishedObjectChanges objectChanges;
            UnpublishedPropertyChanges propertyChanges;
            KObject supposalretrivalObject = new KObject()
            {
                Id = objectIdForSupposalObject,
                TypeUri = "نوع شئ آزمایشی",
                LabelPropertyID = propertyLabelIdForSupposalObject
            };
            KProperty supposalRetrievalProperty = new KProperty()
            {
                Id = propertyLabelIdForSupposalObject,
                TypeUri = "برچسب",
                Owner = supposalretrivalObject,
                Value = "نام نمایشی اولیه"
            };
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimRemoteServiceClientCreateAndClose();
                ShimKWPropertyGenerationRequirements();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetObjectListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KObject[] { supposalretrivalObject };
                    };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        return new KProperty[] { supposalRetrievalProperty };
                    };
                // Act
                retrievedObject = await ObjectManager.GetObjectByIdAsync(objectIdForSupposalObject);
                ObjectManager.ChangeDisplayNameOfObject(retrievedObject, "نام نمایشی جدید شئ آزمایشی");
                objectChanges = ObjectManager.GetUnpublishedChanges();
                propertyChanges = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(objectChanges.AddedObjects.Any());
            Assert.IsFalse(objectChanges.ResolvedObjects.Any());
            Assert.IsFalse(propertyChanges.AddedProperties.Any());
            Assert.IsTrue(propertyChanges.ModifiedProperties.Contains(retrievedObject.DisplayName));
            Assert.AreEqual(1, propertyChanges.ModifiedProperties.Count());
        }
    }
}