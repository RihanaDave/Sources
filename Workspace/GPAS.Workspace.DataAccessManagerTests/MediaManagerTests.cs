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
    public class MediaManagerTests : DamTests
    {
        private void ShimCreateNewMediaPreparation()
        {
            ShimCreateNewMediaPreparation(new long[] { });
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewMediaId = (wsc) => { return GenerateSupposalStoredMediaId(); };
        }
        private void ShimCreateNewMediaPreparation(long newID)
        {
            ShimCreateNewMediaPreparation(new long[] { newID });
        }
        private void ShimCreateNewMediaPreparation(long[] newIdRange)
        {
            int idRangeIndex = 0;
            ShimRemoteServiceClientCreateAndClose();
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewObjectId = (wsc) => { return GenerateSupposalStoredObjectId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return GenerateSupposalStoredPropertyId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewMediaId = (wsc) => { return newIdRange[idRangeIndex++]; };
            Fakes.ShimSystem.GetOntology = () => { return new Ontology.Ontology(); };
            Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (onto) => { return "زمان_و_موقعیت_جغرافیایی"; };
        }

        [TestInitialize]
        public void Init()
        {
            ObjectManager.Initialization("نوع برچسب آزمایشی", "نوع رابطه عضویت در گروه آزمایشی");
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
            MediaManager.DiscardChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
            MediaManager.DiscardChanges();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUnpublishedMedia_ArgNullInput_ThrowsException()
        {
            // Act
            MediaManager.IsUnpublishedMedia(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg1NullInput_ThrowsException()
        {
            // Act
            MediaManager.CommitUnpublishedChanges(null, new long[] { }, 100);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg2NullInput_ThrowsException()
        {
            // Act
            MediaManager.CommitUnpublishedChanges(new long[] { }, null, 100);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewMediaForObject_Arg1NullInput_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject fakeObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                MediaManager.CreateNewMediaForObject(null, "۱۲۳", fakeObject);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewMediaForObject_Arg2NullInput_ThrowsException()
        {
            // Arrange
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                MediaManager.CreateNewMediaForObject(new MediaPathContent(), null, testObject);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewMediaForObject_Arg3NullInput_ThrowsException()
        {
            // Act
            MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteMedia_ArgNullInput_ThrowsException()
        {
            // Act
            MediaManager.DeleteMedia(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetMediaForObject_ArgNullInput_ThrowsException()
        {
            // Act
            await MediaManager.GetMediaForObjectAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        public void CreateNewMedia_AssignsGeneratedId()
        {
            // Arrange
            KWMedia newMedia;
            long supposalId = GenerateSupposalStoredMediaId();
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation(supposalId);
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                newMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
            }
            // Assert
            Assert.AreEqual(supposalId, newMedia.ID);
        }

        [TestMethod()]
        public void CreateNewMedia_KnowMediaAsUnpublished()
        {
            // Arrange
            KWMedia newMedia;
            bool isUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                newMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                isUnpublished = MediaManager.IsUnpublishedMedia(newMedia);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public async Task CreatedMedia_MayBeAccessableForItsOwnerObject()
        {
            // Arrange
            KWMedia testMedia;
            IEnumerable<KWMedia> mediasForObject;
            // Act
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
            }
            // Assert
            Assert.IsTrue(mediasForObject.Contains(testMedia));
            Assert.AreEqual(1, mediasForObject.Count());
        }

        [TestMethod()]
        public void CreatedMedia_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            KWMedia testMedia;
            UnpublishedMediaChanges changes;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                changes = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedMedias.Contains(testMedia));
            Assert.IsFalse(changes.DeletedMedias.Contains(testMedia));
        }

        [TestMethod()]
        public void CreatedMedia_AfterCommitChangesWithoutMappingFakeIdBefore_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            KWMedia testMedia;
            var fakeMediaIds = new List<long>();
            long[] deletedMediaIDs = new long[] { };
            UnpublishedMediaChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
            }
            // نگاشت شناسه‌های نامعتبر به معتبر جهت انجام تست افزوده نشده
            MediaManager.CommitUnpublishedChanges(fakeMediaIds, deletedMediaIDs, 100);
            unpublishedChangesAfterCommit = MediaManager.GetUnpublishedChanges();
            // Assert
            Assert.IsTrue(unpublishedChangesAfterCommit.AddedMedias.Contains(testMedia));
        }

        [TestMethod()]
        public void CreatedMedia_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            KWMedia testMedia;
            var fakeMediaIds = new List<long>();
            long[] deletedMediaIDs = new long[] { };
            UnpublishedMediaChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                fakeMediaIds.Add(testMedia.ID);
                MediaManager.CommitUnpublishedChanges(fakeMediaIds, deletedMediaIDs, 100);
                unpublishedChangesAfterCommit = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedMedias.Contains(testMedia));
            Assert.IsFalse(unpublishedChangesAfterCommit.DeletedMedias.Contains(testMedia));
        }

        [TestMethod()]
        public void CreatedMedia_AfterCommitChanges_MayNotKnownAsUnpublishedMedia()
        {
            // Arrange
            KWMedia testMedia;
            var fakeMediaIds = new List<long>();
            long[] deletedMediaIDs = new long[] { };
            bool isTestMediaUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                fakeMediaIds.Add(testMedia.ID);
                MediaManager.CommitUnpublishedChanges(fakeMediaIds, deletedMediaIDs, 100);
                isTestMediaUnpublished = MediaManager.IsUnpublishedMedia(testMedia);
            }
            // Assert
            Assert.IsFalse(isTestMediaUnpublished);
        }

        [TestMethod()]
        public void CreatedMedia_AfterDelete_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            KWMedia testMedia;
            Dictionary<long, long> fakeMediaIdsToValidIdsMapping = new Dictionary<long, long>();
            UnpublishedMediaChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                MediaManager.DeleteMedia(testMedia);
                unpublishedChangesAfterCommit = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedMedias.Contains(testMedia));
            Assert.IsFalse(unpublishedChangesAfterCommit.DeletedMedias.Contains(testMedia));
        }

        [TestMethod()]
        public async Task CreatedMedia_AfterDelete_MayNotBeAccessableForItsOwnerObject()
        {
            // Arrange
            KWMedia testMedia;
            IEnumerable<KWMedia> mediasForObject;
            using (ShimsContext.Create())
            {
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testMedia = MediaManager.CreateNewMediaForObject(new MediaPathContent(), "۱۲۳", testObject);
                MediaManager.DeleteMedia(testMedia);
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
            }
            // Assert
            Assert.IsFalse(mediasForObject.Contains(testMedia));
            Assert.AreEqual(0, mediasForObject.Count());
        }

        [TestMethod()]
        public async Task StoredMedia_MayBeAccessableForItsOwnerObject()
        {
            // Arrange
            long mediaIdForSupposalMediaOfTestObject = GenerateSupposalStoredMediaId();
            IEnumerable<KWMedia> mediasForObject;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetMediaUrisForObjectAsyncInt64 = async (sc, objid) =>
                    {
                        await Task.Delay(0);
                        var mediaForTestObject = new KMedia()
                        {
                            Id = mediaIdForSupposalMediaOfTestObject,
                            OwnerObjectId = testObject.ID,
                            Description = "۲۳۴",
                            URI = "ftp://testdomain.test/مسیر آزمایشی.پسوندـفایل"
                        };
                        return new KMedia[] { mediaForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
            }
            // Assert
            mediasForObject
                .Single(m => m.ID.Equals(mediaIdForSupposalMediaOfTestObject));
        }

        [TestMethod()]
        public async Task StoredMedia_MayNotKnownAsUnpublishedMedia()
        {
            // Arrange
            long mediaIdForSupposalMediaOfTestObject = GenerateSupposalStoredMediaId();
            IEnumerable<KWMedia> mediasForObject;
            KWMedia retrievedMediaForObject;
            bool isRetrievedMediaKnownAsUnpublished;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetMediaUrisForObjectAsyncInt64 = async (sc, objid) =>
                    {
                        await Task.Delay(0);
                        var mediaForTestObject = new KMedia()
                        {
                            Id = mediaIdForSupposalMediaOfTestObject,
                            OwnerObjectId = testObject.ID,
                            Description = "۲۳۴",
                            URI = "ftp://testdomain.test/مسیر آزمایشی.پسوندـفایل"
                        };
                        return new KMedia[] { mediaForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
                retrievedMediaForObject = mediasForObject
                    .Single(m => m.ID.Equals(mediaIdForSupposalMediaOfTestObject));
                isRetrievedMediaKnownAsUnpublished = MediaManager.IsUnpublishedMedia(retrievedMediaForObject);
            }
            // Assert
            Assert.IsFalse(isRetrievedMediaKnownAsUnpublished);
        }

        [TestMethod()]
        public async Task StoredMedia_AfterDelete_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long mediaIdForSupposalMediaOfTestObject = GenerateSupposalStoredMediaId();
            IEnumerable<KWMedia> mediasForObject;
            KWMedia retrievedMediaForObject;
            UnpublishedMediaChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetMediaUrisForObjectAsyncInt64 = async (sc, objid) =>
                    {
                        await Task.Delay(0);
                        var mediaForTestObject = new KMedia()
                        {
                            Id = mediaIdForSupposalMediaOfTestObject,
                            OwnerObjectId = testObject.ID,
                            Description = "۲۳۴",
                            URI = "ftp://testdomain.test/مسیر آزمایشی.پسوندـفایل"
                        };
                        return new KMedia[] { mediaForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
                retrievedMediaForObject = mediasForObject
                    .Single(m => m.ID.Equals(mediaIdForSupposalMediaOfTestObject));
                MediaManager.DeleteMedia(retrievedMediaForObject);
                changes = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedMedias.Contains(retrievedMediaForObject));
            Assert.IsTrue(changes.DeletedMedias.Contains(retrievedMediaForObject));
        }

        [TestMethod()]
        public async Task StoredMedia_AfterDeleteAndCommitTheDeleting_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long mediaIdForSupposalMediaOfTestObject = GenerateSupposalStoredMediaId();
            IEnumerable<KWMedia> mediasForObject;
            KWMedia retrievedMediaForObject;
            var emptyFakeMediaIds = new List<long>();
            long[] deletedMediaIDs;
            UnpublishedMediaChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetMediaUrisForObjectAsyncInt64 = async (sc, objid) =>
                    {
                        await Task.Delay(0);
                        var mediaForTestObject = new KMedia()
                        {
                            Id = mediaIdForSupposalMediaOfTestObject,
                            OwnerObjectId = testObject.ID,
                            Description = "۲۳۴",
                            URI = "ftp://testdomain.test/مسیر آزمایشی.پسوندـفایل"
                        };
                        return new KMedia[] { mediaForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
                retrievedMediaForObject = mediasForObject
                    .Single(m => m.ID.Equals(mediaIdForSupposalMediaOfTestObject));
                MediaManager.DeleteMedia(retrievedMediaForObject);
                deletedMediaIDs = new long[] { retrievedMediaForObject.ID };
                MediaManager.CommitUnpublishedChanges(emptyFakeMediaIds, deletedMediaIDs, 100);
                changes = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedMedias.Contains(retrievedMediaForObject));
            Assert.IsFalse(changes.DeletedMedias.Contains(retrievedMediaForObject));
        }

        [TestMethod()]
        public async Task StoredMedia_AfterDeleteWithoutCommitTheDeleting_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long mediaIdForSupposalMediaOfTestObject = GenerateSupposalStoredMediaId();
            IEnumerable<KWMedia> mediasForObject;
            KWMedia retrievedMediaForObject;
            var emptyFakeMediaIds = new List<long>();
            long[] deletedMediaIDs = new long[] { };
            UnpublishedMediaChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewMediaPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetMediaUrisForObjectAsyncInt64 = async (sc, objid) =>
                    {
                        await Task.Delay(0);
                        var mediaForTestObject = new KMedia()
                        {
                            Id = mediaIdForSupposalMediaOfTestObject,
                            OwnerObjectId = testObject.ID,
                            Description = "۲۳۴",
                            URI = "ftp://testdomain.test/مسیر آزمایشی.پسوندـفایل"
                        };
                        return new KMedia[] { mediaForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                mediasForObject = await MediaManager.GetMediaForObjectAsync(testObject);
                retrievedMediaForObject = mediasForObject
                    .Single(m => m.ID.Equals(mediaIdForSupposalMediaOfTestObject));
                MediaManager.DeleteMedia(retrievedMediaForObject);
                MediaManager.CommitUnpublishedChanges(emptyFakeMediaIds, deletedMediaIDs, 100);
                changes = MediaManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedMedias.Contains(retrievedMediaForObject));
            Assert.IsTrue(changes.DeletedMedias.Contains(retrievedMediaForObject));
        }
    }
}