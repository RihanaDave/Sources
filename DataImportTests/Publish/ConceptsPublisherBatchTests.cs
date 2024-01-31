using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.DataImport.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.DataImport.ConceptsToGenerate;
using Microsoft.QualityTools.Testing.Fakes;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.AccessControl;

namespace GPAS.DataImport.Publish.Tests
{
    [TestClass()]
    public class ConceptsPublisherBatchTests
    {
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConceptsInBatchMode_CallWithSingleImportingObj_TotallyOneAddedObjMayPublish()
        {
            // Assign
            List<KObject> totalAddedObjects = new List<KObject>(1);
            List<KProperty> totalAddedProperties = new List<KProperty>(1);
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    totalAddedObjects.AddRange(a.AddedObjects);
                    totalAddedProperties.AddRange(a.AddedProperties);
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
            publisher.PublishConceptsInBatchMode();
            // Assert
            Assert.AreEqual(1, totalAddedObjects.Count);
        }

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        public void PublishConceptsInBatchMode_CallWithSingleImportingObj_TypeAndDisplayNameMayPublishCorrectly()
        {
            // Assign
            List<KObject> totalAddedObjects = new List<KObject>(1);
            List<KProperty> totalAddedProperties = new List<KProperty>(1);
            Fakes.StubPublishAdaptor fakeAdaptor = new Fakes.StubPublishAdaptor()
            {
                PublshConceptsAddedConceptsModifiedConceptsResolvedObjectArrayInt64Boolean = (a, m, r, ds, c) =>
                {
                    totalAddedObjects.AddRange(a.AddedObjects);
                    totalAddedProperties.AddRange(a.AddedProperties);
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
            publisher.PublishConceptsInBatchMode();
            // Assert
            Assert.AreEqual(imObj.TypeUri, totalAddedObjects[0].TypeUri);
            Assert.AreEqual(imLabelProperty.TypeURI, totalAddedProperties[0].TypeUri);
            Assert.AreEqual(imLabelProperty.Value, totalAddedProperties[0].Value);
        }
    }
}