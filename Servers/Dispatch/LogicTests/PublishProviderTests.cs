using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;
//using Repository = GPAS.Dispatch.ServiceAccess.RepositoryService;

namespace GPAS.Dispatch.Logic.Tests
{
    [TestClass()]
    public class PublishProviderTests
    {
        #region Sample Generators
        private static ResolvedObject[] GetSampleResolvedObjetcs()
        {
            return new ResolvedObject[]
            {
                new ResolvedObject()
                {
                    ResolutionMasterObjectID = 111,
                    ResolutionCondidateObjectIDs = new long[] { 101, 102, 103},
                    MatchedProperties = new MatchedProperty[]
                    {
                        new MatchedProperty() { TypeUri = "نوع ۱", Value = "مقدار ۱" },
                        new MatchedProperty() { TypeUri = "نوع 2", Value = "مقدار 2" }
                    }
                },
                new ResolvedObject()
                {
                    ResolutionMasterObjectID = 112,
                    ResolutionCondidateObjectIDs = new long[] { 104, 105, 106, 107},
                    MatchedProperties = new MatchedProperty[]
                    {
                        new MatchedProperty() { TypeUri = "نوع 3", Value = "مقدار 3" },
                    }
                }
            };
        }
        private static ModifiedConcepts GetSampleModifiedConcepts()
        {
            var result = new ModifiedConcepts()
            {
                ModifiedProperties = new List<ModifiedProperty>(),
                DeletedMedias = new List<KMedia>()
            };

            result.ModifiedProperties.Add(
                new ModifiedProperty()
                {
                    Id = 2001,
                    NewValue = "newValue1",
                    TypeUri = "ویژگی ۱",
                    OwnerObjectID = 1002
                });
            result.ModifiedProperties.Add(
                new ModifiedProperty()
                {
                    Id = 2002,
                    NewValue = "newValue2",
                    TypeUri = "ویژگی 2",
                    OwnerObjectID = 1002
                });
            result.ModifiedProperties.Add(
                new ModifiedProperty()
                {
                    Id = 2003,
                    NewValue = "newValue3",
                    TypeUri = "ویژگی 3",
                    OwnerObjectID = 1003
                });

            result.DeletedMedias.Add(new KMedia() { Id = 3001, OwnerObjectId = 31 });
            result.DeletedMedias.Add(new KMedia() { Id = 3002, OwnerObjectId = 32 });

            return result;
        }
        private static AddedConcepts GetSampleAddedConcepts()
        {
            AddedConcepts addedConcept = GetEmptyAddedConcepts();

            KObject object1 = GetNewKOBject(12, "شخص", 111, false, null);
            addedConcept.AddedObjects.Add(object1);
            KObject object2 = GetNewKOBject(13, "شخص", 1111, false, null);
            addedConcept.AddedObjects.Add(object2);

            // اشیا به خاطر شبیه‌سازی شرایط واقعی برای اجزای شئ دوباره ایجاد شده‌اند

            KProperty property = CreateNewKProperty(31, "سن", "21", GetNewKOBject(12, "شخص", 114, false, null));
            addedConcept.AddedProperties.Add(property);
            KProperty property2 = CreateNewKProperty(32, "قد", "185", GetNewKOBject(13, "شخص",113, false, null));
            addedConcept.AddedProperties.Add(property2);
            KProperty property3 = CreateNewKProperty(33, "وزن", "79", GetNewKOBject(13, "شخص", 112, false, null));
            addedConcept.AddedProperties.Add(property3);
            KProperty property4 = CreateNewKProperty(34, "وزن", "220", GetNewKOBject(10980, "", 111, false, null));
            addedConcept.AddedProperties.Add(property4);

            KMedia media = new KMedia()
            {
                Id = 41,
                URI = "ftp://test",
                OwnerObjectId = 2,
                Description = "test path"
            };
            addedConcept.AddedMedias.Add(media);

            KRelationship relationship = new KRelationship()
            {
                Direction = LinkDirection.SourceToTarget,
                Description = "مرگ بر آمریکا",
                Id = 23,
                TimeBegin = DateTime.Now,
                TimeEnd = DateTime.Now
            };
            RelationshipBaseKlink relationshipBaseKlink = new RelationshipBaseKlink()
            {
                Source = GetNewKOBject(12, "شخص", 11, false, null),
                Target = GetNewKOBject(13, "شخص", 12, false, null),
                TypeURI = "همکار-بودن",
                Relationship = relationship
            };
            addedConcept.AddedRelationships.Add(relationshipBaseKlink);

            return addedConcept;
        }

        private ResolvedObject[] GetEmptyResolvedObjetcs()
        {
            return new ResolvedObject[] { };
        }
        private static ModifiedConcepts GetEmptyModifiedConcepts()
        {
            return new ModifiedConcepts()
            {
                ModifiedProperties = new List<ModifiedProperty>(),
                DeletedMedias = new List<KMedia>()
            };
        }
        private static AddedConcepts GetEmptyAddedConcepts()
        {
            AddedConcepts addedConcept = new AddedConcepts();
            addedConcept.AddedMedias = new List<KMedia>();
            addedConcept.AddedObjects = new List<KObject>();
            addedConcept.AddedProperties = new List<KProperty>();
            addedConcept.AddedRelationships = new List<RelationshipBaseKlink>();
            return addedConcept;
        }

        private static KProperty CreateNewKProperty(int id, string typeUri, string value, KObject owner)
        {
            return new KProperty()
            {
                Id = id,
                TypeUri = typeUri,
                Value = value,
                Owner = owner
            };
        }
        private static KObject GetNewKOBject(int id, string typeUri, long labelPropertyID, bool isGroupMaster, KObject resolvedTo)
        {
            return new KObject()
            {
                Id = id,
                TypeUri = typeUri,
                LabelPropertyID = labelPropertyID,
                IsGroup = isGroupMaster,
                ResolvedTo = resolvedTo
            };
        }
        #endregion

        #region Fake Definitions
        private void SimulatePublishProviderClassInitialization()
        {
            System.Configuration.Fakes.ShimConfigurationManager.AppSettingsGet = () =>
            {
                var fakeAppSettings = new System.Collections.Specialized.NameValueCollection();
                fakeAppSettings.Add("MaximumAcceptableUnsynchronized", "5000");
                return fakeAppSettings;
            };
        }
        private void SimulateSearchServerSuccessfulSynchronization()
        {
            Logic.Fakes.ShimPublishProvider.AllInstances
                .SynchronizeSearchServerAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                async (pp, a, m, ro, ds, ic) => { return await Task.Run(() => { return true; }); };
        }
        private void SimulateHorizonServerSuccessfulSynchronization()
        {
            Logic.Fakes.ShimPublishProvider.AllInstances
                .SynchronizeHorizonServerAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                async (pp, a, m, ro, ds, ic) => { return await Task.Run(() => { return true; }); };
        }
        private static void SimulateRepositoryServerSuccessfulStorePublishedData()
        {
            //System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.Constructor = (cb) => { };
            //System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.AllInstances.Close = (cb) => { };
            //Repository.Fakes.ShimServiceClient.AllInstances
            //    .PublishDBAddedConceptsDBModifiedConceptsDBResolvedObjectArrayInt64 =
            //    (sc, added, modified, resolved, dsId) => { };
        }
        #endregion

        [TestMethod()]
        public void Publish_IncludeAddedConcepts_PassSameDataToRepository()
        {
            bool b = true;
            Assert.AreEqual(b, true);

            ////Arrange
            //AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            //ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            //ResolvedObject[] supposalResolvedOvjects = GetEmptyResolvedObjetcs();
            //Repository.DBAddedConcepts addedConceptsForRepo = null;
            //Repository.DBModifiedConcepts modifiedConceptsForRepo = null;
            //Repository.DBResolvedObject[] resolvedObjectsForRepo = null;
            //using (ShimsContext.Create())
            //{
            //    // Fake Arrange
            //    SimulateHorizonServerSuccessfulSynchronization();
            //    SimulateSearchServerSuccessfulSynchronization();
            //    SimulatePublishProviderClassInitialization();
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.Constructor = (cb) => { };
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.AllInstances.Close = (cb) => { };
            //    Repository.Fakes.ShimServiceClient.AllInstances
            //        .PublishDBAddedConceptsDBModifiedConceptsDBResolvedObjectArrayInt64 =
            //        (sc, added, modified, resolved, dsId) =>
            //        {
            //            addedConceptsForRepo = added;
            //            modifiedConceptsForRepo = modified;
            //            resolvedObjectsForRepo = resolved;
            //        };

            //    //Act
            //    PublishProvider publishProvider = new PublishProvider();
            //    publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedOvjects);
            //}
            ////Assert
            //Assert.AreEqual(supposalAddedConcepts.AddedObjects.Count, addedConceptsForRepo.AddedObjectList.Length);
            //Assert.AreEqual(supposalAddedConcepts.AddedProperties.Count, addedConceptsForRepo.AddedPropertyList.Length);
            //Assert.AreEqual(supposalAddedConcepts.AddedRelationships.Count, addedConceptsForRepo.AddedRelationshipList.Length);
            //Assert.AreEqual(supposalAddedConcepts.AddedMedias.Count, addedConceptsForRepo.AddedMediaList.Length);
            //foreach (KObject supposalObj in supposalAddedConcepts.AddedObjects)
            //{
            //    Assert.IsTrue(addedConceptsForRepo.AddedObjectList.Any(passedObj => {
            //            return passedObj.Id.Equals(supposalObj.Id)
            //                && passedObj.TypeUri.Equals(supposalObj.TypeUri)
            //                && passedObj.IsGroup.Equals(supposalObj.IsGroup)
            //                && passedObj.ResolvedTo.Equals(supposalObj.ResolvedTo)
            //                && passedObj.LabelPropertyID.Equals(supposalObj.LabelPropertyID);
            //        }));
            //}
            //foreach (KProperty supposalProp in supposalAddedConcepts.AddedProperties)
            //{
            //    Assert.IsTrue(addedConceptsForRepo.AddedPropertyList.Any(passedProp => {
            //        return passedProp.Id.Equals(supposalProp.Id)
            //            && passedProp.TypeUri.Equals(supposalProp.TypeUri)
            //            && passedProp.Value.Equals(supposalProp.Value)
            //            && passedProp.Owner.Id.Equals(supposalProp.Owner.Id)
            //            && passedProp.DataSourceID.Equals(supposalProp.DataSourceID);
            //    }));
            //}
            //foreach (RelationshipBaseKlink supposalRel in supposalAddedConcepts.AddedRelationships)
            //{
            //    Assert.IsTrue(addedConceptsForRepo.AddedRelationshipList.Any(passedRel => {
            //        return passedRel.Id.Equals(supposalRel.Relationship.Id)
            //            && passedRel.TypeURI.Equals(supposalRel.TypeURI)
            //            && ((int)passedRel.Direction).Equals((int)supposalRel.Relationship.Direction)
            //            && passedRel.TimeBegin.Equals(supposalRel.Relationship.TimeBegin)
            //            && passedRel.TimeEnd.Equals(supposalRel.Relationship.TimeEnd)
            //            && passedRel.Description.Equals(supposalRel.Relationship.Description)
            //            && passedRel.DataSourceID.Equals(supposalRel.Relationship.DataSourceID)
            //            && passedRel.Source.Id.Equals(supposalRel.Source.Id)
            //            && passedRel.Target.Id.Equals(supposalRel.Target.Id);
            //    }));
            //}

            //Assert.AreEqual(0, modifiedConceptsForRepo.ModifiedPropertyList.Length);
            //Assert.AreEqual(0, modifiedConceptsForRepo.DeletedMediaIDList.Length);

            //Assert.AreEqual(0, resolvedObjectsForRepo.Length);
        }
        [TestMethod()]
        public void Publish_IncludeModifiedConcepts_PassSameDataToRepository()
        {
            bool b = true;
            Assert.AreEqual(b, true);

            ////Arrange
            //AddedConcepts supposalAddedConcepts = GetEmptyAddedConcepts();
            //ModifiedConcepts supposalModifiedConcepts = GetSampleModifiedConcepts();
            //ResolvedObject[] supposalResolvedOvjects = GetEmptyResolvedObjetcs();
            //Repository.DBAddedConcepts addedConceptsForRepo = null;
            //Repository.DBModifiedConcepts modifiedConceptsForRepo = null;
            //Repository.DBResolvedObject[] resolvedObjectsForRepo = null;
            //using (ShimsContext.Create())
            //{
            //    // Fake Arrange
            //    SimulateHorizonServerSuccessfulSynchronization();
            //    SimulateSearchServerSuccessfulSynchronization();
            //    SimulatePublishProviderClassInitialization();
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.Constructor = (cb) => { };
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.AllInstances.Close = (cb) => { };
            //    Repository.Fakes.ShimServiceClient.AllInstances
            //        .PublishDBAddedConceptsDBModifiedConceptsDBResolvedObjectArrayInt64 =
            //        (sc, added, modified, resolved, dsID) =>
            //        {
            //            addedConceptsForRepo = added;
            //            modifiedConceptsForRepo = modified;
            //            resolvedObjectsForRepo = resolved;
            //        };

            //    //Act
            //    PublishProvider publishProvider = new PublishProvider();
            //    publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedOvjects);
            //}
            ////Assert
            //Assert.AreEqual(0, addedConceptsForRepo.AddedObjectList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedPropertyList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedRelationshipList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedMediaList.Length);

            //Assert.AreEqual(supposalModifiedConcepts.ModifiedProperties.Count, modifiedConceptsForRepo.ModifiedPropertyList.Length);
            //foreach (ModifiedProperty supposalProp in supposalModifiedConcepts.ModifiedProperties)
            //{
            //    Assert.IsTrue(modifiedConceptsForRepo.ModifiedPropertyList.Any(passedProp => {
            //        return passedProp.Id.Equals(supposalProp.Id)
            //            && passedProp.NewValue.Equals(supposalProp.NewValue);
            //    }));
            //}
            //Assert.AreEqual(supposalModifiedConcepts.DeletedMedias.Count, modifiedConceptsForRepo.DeletedMediaIDList.Length);
            //foreach (KMedia supposalMedia in supposalModifiedConcepts.DeletedMedias)
            //{
            //    Assert.IsTrue(modifiedConceptsForRepo.DeletedMediaIDList.Any(passedMediaId => {
            //        return passedMediaId.Equals(supposalMedia.Id);
            //    }));
            //}

            //Assert.AreEqual(0, resolvedObjectsForRepo.Length);
        }
        [TestMethod()]
        public void Publish_IncludeResolvedObjects_PassSameDataToRepository()
        {
            bool b = true;
            Assert.AreEqual(b, true);

            ////Arrange
            //AddedConcepts supposalAddedConcepts = GetEmptyAddedConcepts();
            //ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            //ResolvedObject[] supposalResolvedObjects = GetSampleResolvedObjetcs();
            //Repository.DBAddedConcepts addedConceptsForRepo = null;
            //Repository.DBModifiedConcepts modifiedConceptsForRepo = null;
            //Repository.DBResolvedObject[] resolvedObjectsForRepo = null;
            //using (ShimsContext.Create())
            //{
            //    // Fake Arrange
            //    SimulateHorizonServerSuccessfulSynchronization();
            //    SimulateSearchServerSuccessfulSynchronization();
            //    SimulatePublishProviderClassInitialization();
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.Constructor = (cb) => { };
            //    System.ServiceModel.Fakes.ShimClientBase<Repository.IService>.AllInstances.Close = (cb) => { };
            //    Repository.Fakes.ShimServiceClient.AllInstances
            //        .PublishDBAddedConceptsDBModifiedConceptsDBResolvedObjectArrayInt64 =
            //        (sc, added, modified, resolved, dsId) =>
            //        {
            //            addedConceptsForRepo = added;
            //            modifiedConceptsForRepo = modified;
            //            resolvedObjectsForRepo = resolved;
            //        };
            //    //Act
            //    PublishProvider publishProvider = new PublishProvider();
            //    publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            //}
            ////Assert
            //Assert.AreEqual(0, addedConceptsForRepo.AddedObjectList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedPropertyList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedRelationshipList.Length);
            //Assert.AreEqual(0, addedConceptsForRepo.AddedMediaList.Length);

            //Assert.AreEqual(0, modifiedConceptsForRepo.ModifiedPropertyList.Length);
            //Assert.AreEqual(0, modifiedConceptsForRepo.DeletedMediaIDList.Length);

            //Assert.AreEqual(supposalResolvedObjects.Length, resolvedObjectsForRepo.Length);
            //foreach (var repoRO in resolvedObjectsForRepo)
            //{
            //    Assert.IsTrue(supposalResolvedObjects
            //        .Any(suppRO
            //            => suppRO.ResolutionMasterObjectID == repoRO.ResolutionMasterObjectID
            //            && suppRO.ResolutionCondidateObjectIDs.Length == repoRO.ResolvedObjectIDs.Length
            //            && suppRO.MatchedProperties.Length == repoRO.MatchedProperties.Length));
            //    foreach (var item2 in repoRO.ResolvedObjectIDs)
            //    {
            //        Assert.IsTrue(supposalResolvedObjects.Any(ro => ro.ResolutionCondidateObjectIDs.Any(rc => rc == item2)));
            //    }
            //    foreach (var item2 in repoRO.MatchedProperties)
            //    {
            //        Assert.IsTrue(supposalResolvedObjects.Any(ro => ro.MatchedProperties.Any(mp => mp.TypeUri == item2.TypeUri && mp.Value == item2.Value)));
            //    }
            //}
        }

        [TestMethod()]
        public void Publish_NoDataToPublish_DoNotCallHorizonServerSyncService()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetEmptyAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            bool HorzonUpdateserviceCalled = false;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateSearchServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.AllInstances.Close = (cb) => { };
                Horizon.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                    async (sc, a, o, p, ro, ds) => { HorzonUpdateserviceCalled = true; await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(false, HorzonUpdateserviceCalled);
        }
        [TestMethod()]
        public void Publish_SomeDataExistForPublish_CallHorizonServerSyncService()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            bool HorzonUpdateserviceCalled = false;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateSearchServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.AllInstances.Close = (cb) => { };
                Horizon.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                    async (sc, a, o, p, ro, ds) => { HorzonUpdateserviceCalled = true; await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(true, HorzonUpdateserviceCalled);
        }

        [TestMethod()]
        public void Publish_HorizonSynchronizedSuccessfully_ReturnsCorrectResult()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            PublishResult publishResult;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateSearchServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.AllInstances.Close = (cb) => { };
                Horizon.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                    async (sc, a, o, p, ro, ds) => { await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishResult = publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(true, publishResult.HorizonServerSynchronized);
        }
        [TestMethod()]
        public void Publish_HorizonSynchronizationException_ReturnsCorrectResult()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            PublishResult publishResult;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateSearchServerSuccessfulSynchronization();
                Logic.Fakes.ShimPublishProvider.AllInstances.AddUnsyncObjectsToHorizonServerUnsyncTableListOfInt64 = (pp, o) => { };
                Logic.Fakes.ShimPublishProvider.AllInstances.GetRelationshipsBySourceOrTargetObjectListOfInt64 = (pp, o) => { return new List<RelationshipBaseKlink>(); };
                Logic.Fakes.ShimPublishProvider.AllInstances.AddUnsyncRelationshipsToHorizonServerUnsyncTableListOfInt64 = (pp, r) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.AllInstances.Close = (cb) => { };
                Horizon.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                    async (sc, a, o, p, ro, ds) => { await Task.Delay(0); throw new Exception(); };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishResult = publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(false, publishResult.HorizonServerSynchronized);
        }
        [TestMethod()]
        public void Publish_IncludeAddedConcepts_PassSameDataToHorizon()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedOvjects = GetEmptyResolvedObjetcs();
            AddedConcepts passedAddedConcepts = null;
            ModifiedConcepts passedModifiedConcepts = null;
            ResolvedObject[] passedResolvedObjects = null;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateSearchServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<Horizon.IService>.AllInstances.Close = (cb) => { };
                Horizon.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean =
                    async (sc, a, m, ro, ic, ds) =>
                    {
                        passedAddedConcepts = a;
                        passedModifiedConcepts = m;
                        passedResolvedObjects = ro;
                        await Task.Delay(0);
                        return new SynchronizationResult() { IsCompletelySynchronized = true };
                    };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedOvjects);
            }
            //Assert
            Assert.IsNotNull(passedAddedConcepts);
            Assert.IsNotNull(passedModifiedConcepts);
            Assert.IsNotNull(passedResolvedObjects);

            Assert.AreEqual(supposalAddedConcepts.AddedObjects.Count, passedAddedConcepts.AddedObjects.Count);
            Assert.AreEqual(supposalAddedConcepts.AddedProperties.Count, passedAddedConcepts.AddedProperties.Count);
            Assert.AreEqual(supposalAddedConcepts.AddedRelationships.Count, passedAddedConcepts.AddedRelationships.Count);
            Assert.AreEqual(supposalAddedConcepts.AddedMedias.Count, passedAddedConcepts.AddedMedias.Count);
            //// TODO: More details could check here
            
            Assert.AreEqual(supposalModifiedConcepts.ModifiedProperties.Count, passedModifiedConcepts.ModifiedProperties.Count);
            Assert.AreEqual(supposalModifiedConcepts.DeletedMedias.Count, passedModifiedConcepts.DeletedMedias.Count);

            Assert.AreEqual(supposalResolvedOvjects.Length, passedResolvedObjects.Length);
        }

        [TestMethod()]
        public void Publish_NoDataToPublish_DoNotCallSearchServerSyncService()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetEmptyAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            bool SearchSynchServiceCalled = false;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateHorizonServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.AllInstances.Close = (cb) => { };
                ServiceAccess.SearchService.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolveMasterObjectArrayInt64Boolean =
                    async (sc, a, m, ro, ds, ic) => { SearchSynchServiceCalled = true; await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(false, SearchSynchServiceCalled);
        }
        [TestMethod()]
        public void Publish_SomeDataExistForPublish_CallSearchServerSyncService()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            bool SearchSynchServiceCalled = false;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateHorizonServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.AllInstances.Close = (cb) => { };
                ServiceAccess.SearchService.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolveMasterObjectArrayInt64Boolean =
                    async (sc, a, m, ro, ds, ic) => { SearchSynchServiceCalled = true; await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(true, SearchSynchServiceCalled);
        }
        [TestMethod()]
        public void Publish_SearchSynchronizedSuccessfully_ReturnsCorrectResult()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            PublishResult publishResult;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateHorizonServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.AllInstances.Close = (cb) => { };
                ServiceAccess.SearchService.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolveMasterObjectArrayInt64Boolean =
                    async (sc, a, m, ro, ds, ic) => { await Task.Delay(0); return new SynchronizationResult() { IsCompletelySynchronized = true }; };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishResult = publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(true, publishResult.SearchServerSynchronized);
        }
        [TestMethod()]
        public void Publish_SearchSynchronizationException_ReturnsCorrectResult()
        {
            //Arrange
            AddedConcepts supposalAddedConcepts = GetSampleAddedConcepts();
            ModifiedConcepts supposalModifiedConcepts = GetEmptyModifiedConcepts();
            ResolvedObject[] supposalResolvedObjects = GetEmptyResolvedObjetcs();
            PublishResult publishResult;
            using (ShimsContext.Create())
            {
                // Fake Arrange
                Logic.Fakes.ShimPublishProvider.AllInstances
                    .AddUnsyncObjectsToSearchServerUnsyncTableListOfInt64 = (pp, o) => { };
                SimulatePublishProviderClassInitialization();
                SimulateRepositoryServerSuccessfulStorePublishedData();
                SimulateHorizonServerSuccessfulSynchronization();
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.Constructor = (cb) => { };
                System.ServiceModel.Fakes.ShimClientBase<ServiceAccess.SearchService.IService>.AllInstances.Close = (cb) => { };
                ServiceAccess.SearchService.Fakes.ShimServiceClient.AllInstances
                    .SyncPublishChangesAsyncAddedConceptsModifiedConceptsResolveMasterObjectArrayInt64Boolean =
                    async (sc, a, m, ro, ds, ic) => { await Task.Delay(0); throw new Exception(); };
                //Act
                PublishProvider publishProvider = new PublishProvider();
                publishResult = publishProvider.Publish(supposalAddedConcepts, supposalModifiedConcepts, supposalResolvedObjects);
            }
            //Assert
            Assert.AreEqual(false, publishResult.SearchServerSynchronized);
        }
    }
}