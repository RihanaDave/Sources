using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.GlobalResolve;
using GPAS.DataImport.GlobalResolve.Suite;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImport.Publish.Tests
{
    [TestClass()]
    public class ConceptsPublisherTests
    {
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PublishConcepts_CallNoImportingConcept_MayThrowInvalidOperationException()
        {
            // Assign
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor() { };

            var importingObjects = new List<ImportingObject>();
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            // Assert => ExpectedException
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_MayCallAdaptorPublishMethodOnce()
        {
            // Assign
            byte publishCallTimes = 0;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    publishCallTimes++;
                    return new PublishResult();
                }
            };

            var imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.AreEqual(1, publishCallTimes);
        }

        #region Simple Added Object
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_OneAddedObjMayPublish()
        {
            // Assign
            KObject[] addedObjects = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(1, addedObjects.Length);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_TypeAndDisplayNameMayPublishCorrectly()
        {
            // Assign
            KObject[] addedObjects = null;
            KProperty[] addedProperties = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    addedProperties = a.AddedProperties.ToArray();
                    return new PublishResult();
                }
            };

            var imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.AreEqual(imObj.TypeUri, addedObjects[0].TypeUri);
            Assert.AreEqual(imObj.LabelProperty.TypeURI, addedProperties[0].TypeUri);
            Assert.AreEqual(imObj.LabelProperty.Value, addedProperties[0].Value);
            Assert.IsFalse(addedObjects[0].IsGroup);
            Assert.IsNull(addedObjects[0].ResolvedTo);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_NoConceptOtherThanObjectMayAddedForPublish()
        {
            // Assign
            KProperty[] addedProperties = null;
            RelationshipBaseKlink[] addedRelationships = null;
            KMedia[] addedMedias = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedProperties = a.AddedProperties.ToArray();
                    addedRelationships = a.AddedRelationships.ToArray();
                    addedMedias = a.AddedMedias.ToArray();
                    return new PublishResult();
                }
            };

            ImportingProperty imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedProperties);
            Assert.IsNotNull(addedRelationships);
            Assert.IsNotNull(addedMedias);
            Assert.AreEqual(1, addedProperties.Length);
            Assert.AreEqual(0, addedRelationships.Length);
            Assert.AreEqual(0, addedMedias.Length);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_NoModifiedConceptMayPassForPublish()
        {
            // Assign
            ModifiedProperty[] modifiedProperties = null;
            long[] deletedMediaIDs = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    modifiedProperties = m.ModifiedProperties.ToArray();
                    deletedMediaIDs = m.DeletedMedias.Select(media => media.Id).ToArray();
                    return new PublishResult();
                }
            };

            ImportingProperty imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(modifiedProperties);
            Assert.IsNotNull(deletedMediaIDs);
            Assert.AreEqual(0, modifiedProperties.Length);
            Assert.AreEqual(0, deletedMediaIDs.Length);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingObj_NoResolvedMayPassForPublish()
        {
            // Assign
            ResolvedObject[] resolvedObjects = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    resolvedObjects = r;
                    return new PublishResult();
                }
            };

            ImportingProperty imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(resolvedObjects);
            Assert.AreEqual(0, resolvedObjects.Length);
        }
        #endregion

        // TODO: Check ID assignment by Adaptor method call

        #region Simple Added Relatinoship
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingRelations_MayAddOneRelatinshipToPublish()
        {
            // Assign
            RelationshipBaseKlink[] addedRelationships = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedRelationships = a.AddedRelationships.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty1 = new ImportingProperty("temp_label_type", "TestName1");
            var imObj1 = new ImportingObject("TestType1", imLabelProperty1);
            var imLabelProperty2 = new ImportingProperty("temp_label_type", "TestName1");
            var imObj2 = new ImportingObject("TestType2", imLabelProperty2);
            var imRel = new ImportingRelationship(imObj1, imObj2, "relType11", InternalResolve.IRRelationshipDirection.TargetToSource, DateTime.MinValue, DateTime.MaxValue, "desc111");
            var importingObjects = new List<ImportingObject>() { imObj1, imObj2 };
            var importingRelationships = new List<ImportingRelationship>() { imRel };
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedRelationships);
            Assert.AreEqual(1, addedRelationships.Length);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSingleImportingRelations_DataMayCorrectlyPassToPublish()
        {
            // Assign
            RelationshipBaseKlink[] addedRelationships = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedRelationships = a.AddedRelationships.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty1 = new ImportingProperty("temp_label_type", "TestName1");
            var imObj1 = new ImportingObject("TestType1", imLabelProperty1);
            var imLabelProperty2 = new ImportingProperty("temp_label_type", "TestName1");
            var imObj2 = new ImportingObject("TestType2", imLabelProperty2);
            var imRel = new ImportingRelationship(imObj1, imObj2, "relType11", InternalResolve.IRRelationshipDirection.TargetToSource, DateTime.MinValue, DateTime.MaxValue, "desc111");
            var importingObjects = new List<ImportingObject>() { imObj1, imObj2 };
            var importingRelationships = new List<ImportingRelationship>() { imRel };
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.AreEqual(imRel.TypeURI, addedRelationships[0].TypeURI);
            Assert.AreEqual(imRel.TimeBegin, addedRelationships[0].Relationship.TimeBegin);
            Assert.AreEqual(imRel.TimeEnd, addedRelationships[0].Relationship.TimeEnd);
            Assert.AreEqual(imRel.Description, addedRelationships[0].Relationship.Description);
            Assert.AreEqual(ConceptsPublisher.ConvertImportingDirectionToDispatchDirection(imRel.Direction), addedRelationships[0].Relationship.Direction);
        }
        #endregion

        #region Simple Added Properties
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSomeImportingProperties_PropertiesMayAddedToPublish()
        {
            // Assign
            KProperty[] addedProperties = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedProperties = a.AddedProperties.ToArray();
                    return new PublishResult();
                }
            };
            ImportingProperty imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            ImportingObject imObj = new ImportingObject("TestType1", imLabelProperty);
            var imProp1 = new ImportingProperty("PropType11", "PropValue11");
            imObj.AddPropertyForObject(imProp1);
            var imProp2 = new ImportingProperty("PropType12", "PropValue12");
            imObj.AddPropertyForObject(imProp2);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedProperties);
            Assert.AreEqual(3, addedProperties.Length);
        }
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConcepts_CallWithSomeImportingProperties_PropertiesFieldsMayCorrectlyPassToPublish()
        {
            // Assign
            KProperty[] addedProperties = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedProperties = a.AddedProperties.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "TestName1");
            var imObj = new ImportingObject("TestType1", imLabelProperty);
            var imProp1 = new ImportingProperty("PropType11", "PropValue11");
            imObj.AddPropertyForObject(imProp1);
            var imProp2 = new ImportingProperty("PropType12", "PropValue12");
            imObj.AddPropertyForObject(imProp2);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            var publisher = new ConceptsPublisher();
            // Act
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl);
            publisher.PublishConcepts();
            // Assert
            Assert.IsTrue(addedProperties.Any(p => p.TypeUri.Equals(imProp1.TypeURI)));
            Assert.IsTrue(addedProperties.Any(p => p.TypeUri.Equals(imProp2.TypeURI)));
            Assert.IsTrue(addedProperties.Any(p => p.Value.Equals(imProp1.Value)));
            Assert.IsTrue(addedProperties.Any(p => p.Value.Equals(imProp2.Value)));
        }
        #endregion

        #region Global Resolution
        private GlobalResolutionSuite GetSampleSuite()
        {
            return new GlobalResolutionSuite()
            {
                Passes = new List<Pass>()
                {
                    new Pass()
                    {
                        unresolvedObjectsSelectionFilter = new UnresolveObjectSelectionFilter()
                        {
                            ObjectTypeFilters = new List<string>(),
                            PropertyTypeValueFilters = new List<PropertyTypeValuePair>()
                        },
                        matchingCriteria = new CandidatesMatchingCriteria()
                        {
                            TargetingObjectTypeAndLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties[]
                            {
                                new TargetingObjTypeWithRelatedLinkingProperties()
                                {
                                    TargetingObjectType = new TargetingObject() {typrUri = "شخص"},
                                    LinkingProperties = new LinkingProperty[]
                                    {
                                        new LinkingProperty() { typeURI = "شناسه", resolutionOption = ResolutionOption.ExactMatch }
                                    }
                                }
                            }
                        },
                        matchedObjectsSelection = new SuccessfulMatchedObjectsResolution[]
                        {
                            new SuccessfulMatchedObjectsResolution()
                            {
                                ObjectType = new TargetingObject()
                                {
                                    typrUri = "شخص"
                                },
                                ResolutionMethod = MatchedObjectsResolutionMethod.ResolveAll
                            }
                        }
                    }
                }
            };
        }

        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectNotMatchedToTargetingObjectType_PublishTheObjAsAdded()
        {
            // Assign
            KObject[] addedObjects = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "بیمارستان ۱");
            ImportingObject imObj = new ImportingObject("سازمان", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(1, addedObjects.Length);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectNotMatchedToTargetingObjectType_DoNotGetCandidate()
        {
            // Assign
            bool getCandidateMethodCalled = false;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    getCandidateMethodCalled = true;
                    return new GlobalResolutionCandidates[] { };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                { return new PublishResult(); }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "بیمارستان ۱");
            ImportingObject imObj = new ImportingObject("سازمان", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsFalse(getCandidateMethodCalled);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndWithoutLinkingProperties_DoNotTryGetCandidates()
        {
            // Assign
            bool getCandidateMethodCalled = false;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    getCandidateMethodCalled = true;
                    return new GlobalResolutionCandidates[] { };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                { return new PublishResult(); }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsFalse(getCandidateMethodCalled);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndLinkingProperty_TryGetCandidates()
        {
            // Assign
            bool getCandidateMethodCalled = false;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    getCandidateMethodCalled = true;
                    return new GlobalResolutionCandidates[] { };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                { return new PublishResult(); }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            imObj.AddPropertyForObject(new ImportingProperty("شناسه", "3001"));
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsTrue(getCandidateMethodCalled);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndNoCandidate_DoNotRetrieveAnyObject()
        {
            // Assign
            bool RetrieveObjectsMethodCalled = false;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                RetrieveStoredObjectsByIDIEnumerableOfInt64 = (IDs) =>
                {
                    RetrieveObjectsMethodCalled = true;
                    return new KObject[] { };
                },
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                { return new GlobalResolutionCandidates[] { }; },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                { return new PublishResult(); }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsFalse(RetrieveObjectsMethodCalled);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndWithoutLinkingPropertyAndWithNoCandidate_GetCandidateOnlyForOneObject()
        {
            // Assign
            int passedObjectsToGetCandidates = 0;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    passedObjectsToGetCandidates = o.Length;
                    return new GlobalResolutionCandidates[] { };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.AreEqual(0, passedObjectsToGetCandidates);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndLinkingPropertyAndNoCandidate_GetCandidateOnlyForOneObject()
        {
            // Assign
            int passedObjectsToGetCandidates = 0;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    passedObjectsToGetCandidates = o.Length;
                    return new GlobalResolutionCandidates[] { };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            imObj.AddPropertyForObject(new ImportingProperty("شناسه", "3001"));
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.AreEqual(1, passedObjectsToGetCandidates);
        }
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithTargetingTypeAndNoCandidate_PublishAsAddedConcept()
        {
            // Assign
            KObject[] addedObjects = null;
            KProperty[] addedProperties = null;
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    ImportingProperty Label = new ImportingProperty(o[0].LabelProperty.TypeURI, o[0].LabelProperty.Value);
                    return new GlobalResolutionCandidates[] { new GlobalResolutionCandidates()
                    {
                        // جهت شبیه سازی رفتار واقعی تابع، نمونه‌ی‌ جدید ایجاد شده
                        Master = new ImportingObject(o[0].TypeUri, Label),
                        LinkingProperties = lp,
                        ResolutionCandidates = new CandidateMetadata[] { }
                    }};
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    addedProperties = a.AddedProperties.ToArray();
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(1, addedObjects.Length);
            Assert.IsNotNull(addedProperties);
            Assert.AreEqual(1, addedProperties.Length);
            Assert.AreEqual(imObj.TypeUri, addedObjects[0].TypeUri);
            Assert.AreEqual(imObj.LabelProperty.TypeURI, addedProperties[0].TypeUri);
            Assert.AreEqual(imObj.LabelProperty.Value, addedProperties[0].Value);
        }

        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithOneResolutionCandidate_ExistingButNotALinkingPropertiesMayNotAddAgain()
        {
            // Assign
            KObject[] addedObjects = null;
            KProperty[] addedProperties = null;
            long supposalObjectID = 1001;

            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    ImportingProperty Label = new ImportingProperty(o[0].LabelProperty.TypeURI, o[0].LabelProperty.Value);
                    return new GlobalResolutionCandidates[] { new GlobalResolutionCandidates()
                    {
                        // جهت شبیه سازی رفتار واقعی تابع، نمونه‌ی‌ جدید ایجاد شده
                        Master = new ImportingObject(o[0].TypeUri, Label),
                        LinkingProperties = lp,
                        ResolutionCandidates = new CandidateMetadata[]
                        {
                            new CandidateMetadata()
                            {
                                CandidateID = supposalObjectID,
                                DistinctProperties = new CandidateProperty[]
                                {
                                    new CandidateProperty() { TypeUri = "شناسه", Value = "2001" },
                                    new CandidateProperty() { TypeUri = "شماره تلفن", Value = "912" }
                                }
                            }
                        }
                    }};
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    addedProperties = a.AddedProperties.ToArray();
                    return new PublishResult();
                },
                RetrieveStoredObjectsByIDIEnumerableOfInt64 = (IDs) =>
                {
                    if (IDs.Count() != 1)
                        Assert.Fail();
                    if (!IDs.First().Equals(supposalObjectID))
                        Assert.Fail();
                    return new KObject[]
                    {
                        new KObject() { Id = supposalObjectID, TypeUri = "شخص", LabelPropertyID = 2001, IsGroup  = false, ResolvedTo = null }
                    };
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            imObj.Properties.Add(new ImportingProperty("شناسه", "2001"));
            imObj.Properties.Add(new ImportingProperty("شماره تلفن", "912"));
            imObj.Properties.Add(new ImportingProperty("رنگ مو", "مشکی"));
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(0, addedObjects.Length);
            Assert.IsNotNull(addedProperties);
            Assert.AreEqual(2, addedProperties.Length);
            Assert.IsNotNull(addedProperties.SingleOrDefault(p => p.TypeUri.Equals(imObj.LabelProperty.TypeURI) && p.Value.Equals(imObj.LabelProperty.Value)));
            Assert.IsNotNull(addedProperties.SingleOrDefault(p => p.TypeUri.Equals(imObj.Properties[3].TypeURI) && p.Value.Equals(imObj.Properties[3].Value)));
            Assert.AreEqual(supposalObjectID, addedProperties[0].Owner.Id);
            Assert.AreEqual(supposalObjectID, addedProperties[1].Owner.Id);
        }
        
        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithMoreThanThreshouldResolutionCandidate_PreventResolution()
        {
            // Assign
            KObject[] addedObjects = null;
            KProperty[] addedProperties = null;
            ResolvedObject[] resolvedObjects = null;
            int maxResolutionCandidates = 10;
            int supposalCandidatesCount = 11;

            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    ImportingProperty Label = new ImportingProperty(o[0].LabelProperty.TypeURI, o[0].LabelProperty.Value);
                    GlobalResolutionCandidates candidates = new GlobalResolutionCandidates()
                    {
                        // جهت شبیه سازی رفتار واقعی تابع، نمونه‌ی‌ جدید ایجاد شده
                        Master = new ImportingObject(o[0].TypeUri, Label),
                        LinkingProperties = lp,
                        ResolutionCandidates = new CandidateMetadata[supposalCandidatesCount]
                    };
                    for (int i = 0; i < candidates.ResolutionCandidates.Length; i++)
                    {
                        candidates.ResolutionCandidates[i] = new CandidateMetadata()
                        {
                            CandidateID = 10001 + i,
                            DistinctProperties = new CandidateProperty[] { }
                        };
                    }
                    return new GlobalResolutionCandidates[] { candidates };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    addedProperties = a.AddedProperties.ToArray();
                    resolvedObjects = r;
                    return new PublishResult();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            imObj.Properties.Add(new ImportingProperty("شناسه", "2001"));
            imObj.Properties.Add(new ImportingProperty("شماره تلفن", "912"));
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.MaximumNumberOfGlobalResolutionCandidates = maxResolutionCandidates;
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(1, addedObjects.Length);
            Assert.IsNotNull(addedProperties);
            Assert.AreEqual(3, addedProperties.Length);
            Assert.IsNotNull(resolvedObjects);
            Assert.AreEqual(0, resolvedObjects.Length);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[0].Owner.Id);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[1].Owner.Id);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[2].Owner.Id);
        }

        [TestMethod]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("ادغام سراسری")]
        public void PublishConcepts_ForAnObjectWithLessThanThreshouldResolutionCandidate_DoNotPreventResolution()
        {
            // Assign
            KObject[] addedObjects = null;
            KProperty[] addedProperties = null;
            ResolvedObject[] resolvedObjects = null;
            int maxResolutionCandidates = 10;
            int supposalCandidatesCount = 9;

            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                GetSameTypeResolutionCandidatesForImportingObjectArrayLinkingPropertyArray = (o, lp) =>
                {
                    ImportingProperty Label = new ImportingProperty(o[0].LabelProperty.TypeURI, o[0].LabelProperty.Value);
                    GlobalResolutionCandidates candidates = new GlobalResolutionCandidates()
                    {
                        // جهت شبیه سازی رفتار واقعی تابع، نمونه‌ی‌ جدید ایجاد شده
                        Master = new ImportingObject(o[0].TypeUri, Label),
                        LinkingProperties = lp,
                        ResolutionCandidates = new CandidateMetadata[supposalCandidatesCount]
                    };
                    for (int i = 0; i < candidates.ResolutionCandidates.Length; i++)
                    {
                        candidates.ResolutionCandidates[i] = new CandidateMetadata()
                        {
                            CandidateID = 10001 + i,
                            DistinctProperties = new CandidateProperty[] { }
                        };
                    }
                    return new GlobalResolutionCandidates[] { candidates };
                },
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    addedObjects = a.AddedObjects.ToArray();
                    addedProperties = a.AddedProperties.ToArray();
                    resolvedObjects = r;
                    return new PublishResult();
                },
                RetrieveStoredObjectsByIDIEnumerableOfInt64 = (IDs) =>
                {
                    return IDs.Select(id => new KObject() { Id = id, TypeUri = "شخص", LabelPropertyID = 2001, IsGroup = false, ResolvedTo = null }).ToArray();
                }
            };
            var imLabelProperty = new ImportingProperty("temp_label_type", "شخص ۱");
            ImportingObject imObj = new ImportingObject("شخص", imLabelProperty);
            imObj.Properties.Add(new ImportingProperty("شناسه", "2001"));
            imObj.Properties.Add(new ImportingProperty("شماره تلفن", "912"));
            var importingObjects = new List<ImportingObject>() { imObj };
            var importingRelationships = new List<ImportingRelationship>();
            GlobalResolutionSuite suite = GetSampleSuite();
            var publisher = new ConceptsPublisher();
            var acl = new ACL() { Permissions = new List<ACI>(), Classification = "" };
            // Act
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, fakeAdaptor, 15, acl, suite);
            publisher.MaximumNumberOfGlobalResolutionCandidates = maxResolutionCandidates;
            publisher.PublishConcepts();
            // Assert
            Assert.IsNotNull(addedObjects);
            Assert.AreEqual(1, addedObjects.Length);
            Assert.IsNotNull(addedProperties);
            Assert.AreEqual(3, addedProperties.Length);
            Assert.IsNotNull(resolvedObjects);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[0].Owner.Id);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[1].Owner.Id);
            Assert.AreEqual(addedObjects[0].Id, addedProperties[2].Owner.Id);
            Assert.AreEqual(1, resolvedObjects.Length);
            Assert.AreEqual(supposalCandidatesCount, resolvedObjects[0].ResolutionCondidateObjectIDs.Length);
        }
        #endregion
    }
}