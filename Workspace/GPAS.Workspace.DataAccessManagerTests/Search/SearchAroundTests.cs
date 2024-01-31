using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.DataAccessManager.Tests;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess.RemoteService;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.Search.Tests
{
    [TestClass()]
    public class SearchAroundTests : DamTests
    {
        private void ShimConceptsCreationPreparation()
        {
            ShimRemoteServiceClientCreateAndClose();
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewObjectId = (wsc) => { return GenerateSupposalStoredObjectId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return GenerateSupposalStoredPropertyId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewRelationId = (wsc) => { return GenerateSupposalStoredRelationshipId(); };
            DataAccessManager.Fakes.ShimSystem.GetOntology = () => { return new Ontology.Ontology(); };
            Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (onto) => { return "زمان_و_موقعیت_جغرافیایی"; };
        }

        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedEntitiesOfObjectAsyncTest_ForUnpublishedObjectWithoutRelatedEntity()
        {
            // Arrange
            KWObject objectToTest = new KWObject();
            KWObject[] testObjectsArray = new KWObject[] { objectToTest };
            IEnumerable<RelationshipBasedKWLink> result = new RelationshipBasedKWLink[] { };
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEntitiesAsyncInt64Array
                = async (sc, id) =>
                    {
                        await Task.Delay(0);
                        return new RelationshipBaseKlink[] { };
                    };
                global::System.ServiceModel.Fakes.ShimClientBase<IWorkspaceService>.AllInstances.Close = (sc) => { };

                //DataAccessManager.Fakes.ShimLinkManager.GetRelationshipBaseLinkListFromRetrievedDataIEnumerableOfRelationshipBaseKlink
                //    =async (obj) =>
                //{
                //    await Task.Delay(0);
                //    return new RelationshipBasedKWLink[] { };
                //};
                // Act
                result = await SearchAroundManager.GetRelatedEntitiesOfObjectAsync(testObjectsArray);
            }

            // Assert
            Assert.IsTrue(!result.Any());
        }
        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedEntitiesOfObjectAsyncTest_ForUnpublishedObjects()
        {
            // Arrange
            KWObject objectToTest;
            IEnumerable<RelationshipBasedKWLink> result = new RelationshipBasedKWLink[] { };
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimConceptsCreationPreparation();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEntitiesAsyncInt64Array
                = async (sc, id) =>
                {
                    await Task.Delay(0);
                    return new RelationshipBaseKlink[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.IsEntityString =
                    (onto, str) =>
                {
                    return true;
                };

                objectToTest = ObjectManager.CreateNewObject("بیمارستان", "نور");
                KWObject object1 = ObjectManager.CreateNewObject("شخص", "نام آزمایشی1");
                KWObject object2 = ObjectManager.CreateNewObject("شخص", "نام آزمایشی2");
                KWObject object3 = ObjectManager.CreateNewObject("شخص", "نام آزمایشی3");

                LinkManager.CreateNewRelationshipBaseLink(object1, objectToTest, "عضو", "1لینک آزمایشی",
                    Entities.LinkDirection.Direction.SourceToTarget, DateTime.Now, DateTime.Now);
                LinkManager.CreateNewRelationshipBaseLink(object2, objectToTest, "عضو", "2لینک آزمایشی",
                    Entities.LinkDirection.Direction.Bidirectional, DateTime.Now, DateTime.Now);
                LinkManager.CreateNewRelationshipBaseLink(objectToTest, object3, "عضو", "3لینک آزمایشی",
                    Entities.LinkDirection.Direction.TargetToSource, DateTime.Now, DateTime.Now);

                KWObject[] testObjectsArray = new KWObject[] { objectToTest };
                // Act
                result = await SearchAroundManager.GetRelatedEntitiesOfObjectAsync(testObjectsArray);
            }
            // Assert
            Assert.IsTrue(result.Count() == 3);
            Assert.IsTrue(result.ElementAt(0).TypeURI.Equals("عضو"));
            Assert.IsTrue(result.ElementAt(2).Source.ID == objectToTest.ID ||
                result.ElementAt(2).Target.ID == objectToTest.ID);
        }

        //[TestMethod()]
        //[TestCategory("جستجوی پیرامونی")]
        //public async Task GetRelatedEntitiesOfObjectAsyncTest_ForPublishedObjects()
        //{
        //    // Arrange       
        //    KWObject objectToTest = new KWObject();

        //    IEnumerable<RelationshipBasedKWLink> result = new RelationshipBasedKWLink[] { };
        //    // Fake Arrange!
        //    using (ShimsContext.Create())
        //    {
        //        ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.Constructor = (sc) => { };
        //        ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEntitiesAsyncInt64Array
        //        = async (sc, id) =>
        //        {
        //            await Task.Delay(0);
        //            KObject object1 = new KObject()
        //            {
        //                DisplayName = "نام آزمایشی1",
        //                Id = 10001,
        //                IsGroup = false,
        //                TypeUri = "شخص"
        //            };
        //            KObject object2 = new KObject()
        //            {
        //                DisplayName = "نام آزمایشی2",
        //                Id = 10002,
        //                IsGroup = false,
        //                TypeUri = "شخص"
        //            };
        //            KObject object3 = new KObject()
        //            {
        //                DisplayName = "نام آزمایشی3",
        //                Id = 10003,
        //                IsGroup = false,
        //                TypeUri = "شخص"
        //            };

        //            RelationshipBaseKlink r1 = new RelationshipBaseKlink()
        //            {
        //                Relationship = new KRelationship()
        //                {
        //                    Direction = ServiceAccess.RemoteService.LinkDirection.SourceToTarget,
        //                    Description = "1لینک آزمایشی",
        //                    Id = 10001,
        //                    TimeBegin = DateTime.Now,
        //                    TimeEnd = DateTime.Now
        //                },
        //                Source = object1,
        //                Target = ObjectManager.GetKObjectFromKWObject(objectToTest),
        //                TypeURI = "عضو"
        //            };

        //            RelationshipBaseKlink r2 = new RelationshipBaseKlink()
        //            {
        //                Relationship = new KRelationship()
        //                {
        //                    Direction = ServiceAccess.RemoteService.LinkDirection.TargetToSource,
        //                    Description = "2لینک آزمایشی",
        //                    Id = 10002,
        //                    TimeBegin = DateTime.Now,
        //                    TimeEnd = DateTime.Now
        //                },
        //                Source = object2,
        //                Target = ObjectManager.GetKObjectFromKWObject(objectToTest),
        //                TypeURI = "عضو"
        //            };
        //            RelationshipBaseKlink r3 = new RelationshipBaseKlink()
        //            {
        //                Relationship = new KRelationship()
        //                {
        //                    Direction = ServiceAccess.RemoteService.LinkDirection.SourceToTarget,
        //                    Description = "3لینک آزمایشی",
        //                    Id = 10003,
        //                    TimeBegin = DateTime.Now,
        //                    TimeEnd = DateTime.Now
        //                },
        //                Source = ObjectManager.GetKObjectFromKWObject(objectToTest),
        //                Target = object3,
        //                TypeURI = "عضو"
        //            };


        //            List<RelationshipBaseKlink> relationshipBaseKlinkList = new List<RelationshipBaseKlink>();

        //            relationshipBaseKlinkList.Add(r1);
        //            relationshipBaseKlinkList.Add(r2);
        //            relationshipBaseKlinkList.Add(r3);
        //            return relationshipBaseKlinkList.ToArray();
        //        };
        //        global::System.ServiceModel.Fakes.ShimClientBase<IWorkspaceService>.AllInstances.Close = (sc) => { };
        //        DataAccessManager.Fakes.ShimSystem.GetOntology = () =>
        //        { return new Ontology.Ontology(); };
        //        Ontology.Fakes.ShimOntology.AllInstances.IsEntityString = (onto, str) =>
        //        { return true; };
        //        // Act
        //        result = await SearchAround.GetRelatedEntitiesOfObjectAsync(objectToTest);
        //    }
        //    // Assert
        //    Assert.IsTrue(result.Count() == 3);
        //    Assert.IsTrue(result.ElementAt(0).TypeURI.Equals("عضو"));
        //    Assert.IsTrue(result.ElementAt(2).Source.ID == objectToTest.ID ||
        //        result.ElementAt(2).Target.ID == objectToTest.ID);
        //}
        [TestInitialize]
        public void Init()
        {
            ObjectManager.Initialization("نوع برچسب آزمایشی", "نوع رابطه عضویت در گروه آزمایشی");
        }

        [TestCleanup]
        public void Cleanup()
        {
            ObjectManager.DiscardChanges();
        }

        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedEventsTest_ForUnpublishedObjectWithoutRelatedEvent()
        {
            // Arrange
            KWObject objectToTest = new KWObject();
            KWObject[] testObjectsArray = new KWObject[] { objectToTest };
            EventBasedKWLink[] result = new EventBasedKWLink[] { };
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEventsAsyncInt64Array
                = async (sc, id) =>
                {
                    await Task.Delay(0);
                    return new RelationshipBaseKlink[] { };
                };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEntitiesAppearedInEventsAsyncInt64Array
                = async (wsc, o) =>
                {
                    await Task.Delay(0);
                    return new EventBaseKlink[] { };
                };
                // Act
                result = await SearchAroundManager.GetRelatedEntitiesByMediatorEventsAsync(testObjectsArray);
            }
            // Assert
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedEventsOfObjectAsyncTest_ForUnpublishedObjects()
        {
            // Arrange       
            KWObject objectToTest;
            KWObject[] supposalObjects;
            EventBasedKWLink[] result = null;
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimConceptsCreationPreparation();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEventsAsyncInt64Array
                = async (sc, id) =>
                {
                    await Task.Delay(0);
                    return new RelationshipBaseKlink[] { };
                };
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedEntitiesAppearedInEventsAsyncInt64Array
                = async (wsc, o) =>
                {
                    await Task.Delay(0);
                    return new EventBaseKlink[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.IsEventString = (onto, str) => { return true; };
                objectToTest = ObjectManager.CreateNewObject("بیمارستان", "نور");
                supposalObjects = new KWObject[]
                {
                    ObjectManager.CreateNewObject("شخص", "نام آزمایشی1"),
                    ObjectManager.CreateNewObject("شخص", "نام آزمایشی2"),
                    ObjectManager.CreateNewObject("شخص", "نام آزمایشی3"),
                };

                LinkManager.CreateNewEventBaseLink(supposalObjects[0], objectToTest, "تماس_تلفنی", "رخداد آزمایشی",
                    Entities.LinkDirection.Direction.SourceToTarget, DateTime.Now, DateTime.Now, "حضور_در", "حضور_در");
                LinkManager.CreateNewEventBaseLink(supposalObjects[1], objectToTest, "تماس_تلفنی", "رخداد آزمایشی",
                    Entities.LinkDirection.Direction.SourceToTarget, DateTime.Now, DateTime.Now, "حضور_در", "حضور_در");
                LinkManager.CreateNewEventBaseLink(objectToTest, supposalObjects[2], "تماس_تلفنی", "رخداد آزمایشی",
                    Entities.LinkDirection.Direction.Bidirectional, DateTime.Now, DateTime.Now, "حضور_در", "حضور_در");

                // Act
                result = await SearchAroundManager.GetRelatedEntitiesByMediatorEventsAsync(new KWObject[] { objectToTest });
            }
            // Assert
            Assert.IsTrue(result.Length == 3);
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue
                    ((result[i].Source.TypeURI.Equals("شخص") && result[i].Target.TypeURI.Equals("بیمارستان"))
                    || (result[i].Source.TypeURI.Equals("بیمارستان") && result[i].Target.TypeURI.Equals("شخص")));
                Assert.IsTrue
                    ((result[i].Source.Equals(objectToTest) && result[i].Target.Equals(supposalObjects[i]))
                    || (result[i].Source.Equals(supposalObjects[i]) && result[i].Target.Equals(objectToTest)));
                Assert.IsTrue(result[i].IntermediaryEvent.TypeURI.Equals("تماس_تلفنی"));
            }
        }

        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedDocumentsTest_ForUnpublishedObjectWithoutRelatedDocument()
        {
            // Arrange
            KWObject objectToTest = new KWObject();
            KWObject[] testObjectsArray = new KWObject[] { objectToTest };
            RelationshipBasedKWLink[] result = new RelationshipBasedKWLink[] { };
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimRemoteServiceClientCreateAndClose();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedDocumentsAsyncInt64Array
                = async (sc, id) =>
                {
                    await Task.Delay(0);
                    return new RelationshipBaseKlink[] { };
                };
                // Act
                result = await SearchAroundManager.GetRelatedDocuments(testObjectsArray);
            }
            // Assert
            Assert.IsTrue(result.Length == 0);
        }


        [TestMethod()]
        [TestCategory("جستجوی پیرامونی")]
        public async Task GetRelatedDocumentsTest_ForUnpublishedObjects()
        {
            // Arrange       
            KWObject objectToTest;

            RelationshipBasedKWLink[] result = new RelationshipBasedKWLink[] { };
            // Fake Arrange!
            using (ShimsContext.Create())
            {
                ShimConceptsCreationPreparation();
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.FindRelatedDocumentsAsyncInt64Array
                = async (sc, id) =>
                {
                    await Task.Delay(0);
                    return new RelationshipBaseKlink[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.IsDocumentString =
                    (onto, str) =>
                    {
                        return true;
                    };

                objectToTest = ObjectManager.CreateNewObject("شخص", "مهران");
                KWObject object1 = ObjectManager.CreateNewObject("سند", "سند1");
                KWObject object2 = ObjectManager.CreateNewObject("سند", "سند2");
                KWObject object3 = ObjectManager.CreateNewObject("سند", "سند3");

                LinkManager.CreateNewRelationshipBaseLink(object1, objectToTest, "شبیه", "1لینک آزمایشی",
                    Entities.LinkDirection.Direction.SourceToTarget, DateTime.Now, DateTime.Now);
                LinkManager.CreateNewRelationshipBaseLink(object2, objectToTest, "شبیه", "2لینک آزمایشی",
                    Entities.LinkDirection.Direction.Bidirectional, DateTime.Now, DateTime.Now);
                LinkManager.CreateNewRelationshipBaseLink(objectToTest, object3, "شبیه", "3لینک آزمایشی",
                    Entities.LinkDirection.Direction.TargetToSource, DateTime.Now, DateTime.Now);

                KWObject[] testObjectsArray = new KWObject[] { objectToTest };
                // Act
                result = await SearchAroundManager.GetRelatedDocuments(testObjectsArray);
            }
            // Assert
            Assert.IsTrue(result.Length == 3);
            Assert.IsTrue(result[0].TypeURI.Equals("شبیه"));
            Assert.IsTrue(result[2].Source.ID == objectToTest.ID ||
                result[2].Target.ID == objectToTest.ID);
        }
    }
}