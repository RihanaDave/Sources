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
    public class PropertyManagerTests : DamTests
    {
        private void ShimCreateNewPropertyPreparation()
        {
            ShimCreateNewPropertyPreparation(new long[] { });
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return GenerateSupposalStoredPropertyId(); };
        }
        private void ShimCreateNewPropertyPreparation(long newPropertyID)
        {
            ShimCreateNewPropertyPreparation(new long[] { newPropertyID });
        }
        private void ShimCreateNewPropertyPreparation(long[] newPropertyIdRange)
        {
            int idRangeIndex = 0;
            ShimRemoteServiceClientCreateAndClose();
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewObjectId = (wsc) => { return GenerateSupposalStoredObjectId(); };
            ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                .GetNewPropertyId = (wsc) => { return newPropertyIdRange[idRangeIndex++]; };
            Fakes.ShimSystem.GetOntology = () => { return new Ontology.Ontology(); };
            Ontology.Fakes.ShimOntology.AllInstances.GetDateRangeAndLocationPropertyTypeUri = (onto) => { return "زمان_و_موقعیت_جغرافیایی"; };
        }

        [TestInitialize]
        public void Init()
        {
            ObjectManager.Initialization("نوع برچسب آزمایشی", "نوع رابطه عضویت در گروه آزمایشی");
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ObjectManager.DiscardChanges();
            PropertyManager.DiscardChanges();
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUnpublishedProperty_ArgNullInput_ThrowsException()
        {
            // Act
            PropertyManager.IsUnpublishedProperty(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg1NullInput_ThrowsException()
        {
            // Act
            PropertyManager.CommitUnpublishedChanges(null, new long[] { }, 1);
            // Assert
            // ExpectedException defined.
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommitUnpublishedChanges_Arg2NullInput_ThrowsException()
        {
            // Act
            PropertyManager.CommitUnpublishedChanges(new long[] { }, null, 1);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPropertyFromRetrievedDataAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await PropertyManager.GetPropertyFromRetrievedDataAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewProperty_Arg1NullInput_ThrowsException()
        {
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                // Arrange
                KWObject fakeObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                PropertyManager.CreateNewProperty(null, "۱۲۳", fakeObject);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewProperty_Arg2NullInput_ThrowsException()
        {
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                // Arrange
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                PropertyManager.CreateNewProperty("۱۲۳", null, testObject);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewProperty_Arg3NullInput_ThrowsException()
        {
            // Act
            PropertyManager.CreateNewProperty("۲۳۴", "۱۲۳", null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePropertyValue_Arg1NullInput_ThrowsException()
        {
            // Act
            PropertyManager.UpdatePropertyValue(null, "۱۲۳");
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdatePropertyValue_Arg2NullInput_ThrowsException()
        {
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                // Arrange
                var testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                var testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                // Act
                PropertyManager.UpdatePropertyValue(testProperty, null);
            }
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPropertyListByIdAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await PropertyManager.GetPropertyListByIdAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetPropertiesForObjectAsync_ArgNullInput_ThrowsException()
        {
            // Act
            await PropertyManager.GetPropertiesForObjectAsync(null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetSpecifiedPropertiesOfObjectAsync_Arg1NullInput_ThrowsException()
        {
            // Act
            await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(null, new string[] { });
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetSpecifiedPropertiesOfObjectAsync_Arg2NullInput_ThrowsException()
        {
            // Act
            await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(new KWObject[] { }, null);
            // Assert
            // ExpectedException defined.
        }

        [TestMethod()]
        public void CreateNewProperty_AssignsGeneratedId()
        {
            // Arrange
            long[] supposalIds = GenerateSupposalStoredPropertyIdRange(2);
            KWProperty newProperty;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation(supposalIds);
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                newProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
            }
            // Assert
            Assert.AreEqual(supposalIds[1], newProperty.ID);
        }

        [TestMethod()]
        public void CreateNewProperty_KnowPropertyAsUnpublished()
        {
            // Arrange
            KWProperty newProperty;
            bool isUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                newProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                isUnpublished = PropertyManager.IsUnpublishedProperty(newProperty);
            }
            // Assert
            Assert.IsTrue(isUnpublished);
        }

        [TestMethod()]
        public async Task CreatedProperty_MayBeAccessableByItsId()
        {
            // Arrange
            KWProperty testProperty;
            IEnumerable<KWProperty> givenProperties;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                givenProperties = await PropertyManager.GetPropertyListByIdAsync(new long[] { testProperty.ID });
            }
            // Assert
            Assert.IsTrue(givenProperties.Contains(testProperty));
            Assert.AreEqual(1, givenProperties.Count());
        }

        [TestMethod()]
        public async Task CreatedProperty_MayBeAccessableForItsOwnerObject()
        {
            // Arrange
            KWProperty testProperty;
            IEnumerable<KWProperty> propertiesForObject;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                propertiesForObject = await PropertyManager.GetPropertiesForObjectAsync(testObject);
            }
            // Assert
            Assert.IsTrue(propertiesForObject.Contains(testProperty));
            Assert.AreEqual(2, propertiesForObject.Count());
        }

        [TestMethod()]
        public async Task CreatedProperty_MayBeAccessableByItsOwnerObjectAndType()
        {
            // Arrange
            KWProperty testProperty;
            IEnumerable<KWProperty> specifiedPropertiesForObject;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                string testPropertyTypeUri = "نوع ویژگی آزمایشی";
                // Act
                testProperty = PropertyManager.CreateNewProperty(testPropertyTypeUri, "مقدار ویژگی آزمایشی", testObject);
                specifiedPropertiesForObject = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync
                    (new KWObject[] { testObject }
                    , new string[] { testPropertyTypeUri });
            }
            // Assert
            Assert.IsTrue(specifiedPropertiesForObject.Contains(testProperty));
            Assert.AreEqual(1, specifiedPropertiesForObject.Count());
        }

        [TestMethod()]
        public void CreatedProperty_MayAppearedInUnpublishedChanges()
        {
            KWProperty testProperty;
            UnpublishedPropertyChanges changes;
            // Arrange
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                changes = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedProperties.Contains(testProperty));
            Assert.IsFalse(changes.ModifiedProperties.Contains(testProperty));
        }

        [TestMethod()]
        public void CreatedProperty_AfterModifyValue_MayAppearedInAddedUnpublishedProperties()
        {
            // Arrange
            KWProperty testProperty;
            UnpublishedPropertyChanges changes;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                PropertyManager.UpdatePropertyValue(testProperty, "مقدار جدید ویژگی آزمایشی");
                changes = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(changes.AddedProperties.Contains(testProperty));
            Assert.IsFalse(changes.ModifiedProperties.Contains(testProperty));
        }

        [TestMethod()]
        public void CreatedProperty_AfterCommitChangesWithoutMappingFakeIdBefore_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            KWProperty testProperty;
            var fakePropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            UnpublishedPropertyChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                // نگاشت شناسه‌های نامعتبر به معتبر جهت انجام تست افزوده نشده
                PropertyManager.CommitUnpublishedChanges(fakePropertyIds, modifiedPropertyIDs, 100);
                unpublishedChangesAfterCommit = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsTrue(unpublishedChangesAfterCommit.AddedProperties.Contains(testProperty));
        }

        [TestMethod()]
        public void CreatedProperty_AfterCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            KWProperty testProperty;
            long propertyIdAfterSupposalPublish = GenerateSupposalStoredPropertyId();
            var fakePropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            UnpublishedPropertyChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                fakePropertyIds.Add(testProperty.ID);
                PropertyManager.CommitUnpublishedChanges(fakePropertyIds, modifiedPropertyIDs, 100);
                unpublishedChangesAfterCommit = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedProperties.Contains(testProperty));
            Assert.IsFalse(unpublishedChangesAfterCommit.ModifiedProperties.Contains(testProperty));
        }

        [TestMethod()]
        public void CreatedProperty_AfterCommitChanges_MayNotKnownAsUnpublishedProperty()
        {
            // Arrange
            KWProperty testProperty;
            long propertyIdAfterSupposalPublish = GenerateSupposalStoredPropertyId();
            var fakePropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            bool isTestPropertyUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                fakePropertyIds.Add(testProperty.ID);
                PropertyManager.CommitUnpublishedChanges(fakePropertyIds, modifiedPropertyIDs, 100);
                isTestPropertyUnpublished = PropertyManager.IsUnpublishedProperty(testProperty);
            }
            // Assert
            Assert.IsFalse(isTestPropertyUnpublished);
        }

        [TestMethod()]
        public void CreatedProperty_AfterModifyAndCommitChanges_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            KWProperty testProperty;
            long propertyIdAfterSupposalPublish = GenerateSupposalStoredPropertyId();
            var newPropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            UnpublishedPropertyChanges unpublishedChangesAfterCommit;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                PropertyManager.UpdatePropertyValue(testProperty, "مقدار جدید ویژگی آزمایشی");
                newPropertyIds.Add(testProperty.ID);
                PropertyManager.CommitUnpublishedChanges(newPropertyIds, modifiedPropertyIDs, 100);
                unpublishedChangesAfterCommit = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(unpublishedChangesAfterCommit.AddedProperties.Contains(testProperty));
            Assert.IsFalse(unpublishedChangesAfterCommit.ModifiedProperties.Contains(testProperty));
        }

        [TestMethod()]
        public void CreatedProperty_AfterModifyAndCommitChanges_MayNotKnownAsUnpublishedProperty()
        {
            // Arrange
            KWProperty testProperty;
            long propertyIdAfterSupposalPublish = GenerateSupposalStoredPropertyId();
            var fakePropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            bool isTestPropertyUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                PropertyManager.UpdatePropertyValue(testProperty, "مقدار جدید ویژگی آزمایشی");
                fakePropertyIds.Add(testProperty.ID);
                PropertyManager.CommitUnpublishedChanges(fakePropertyIds, modifiedPropertyIDs, 100);
                isTestPropertyUnpublished = PropertyManager.IsUnpublishedProperty(testProperty);
            }
            // Assert
            Assert.IsFalse(isTestPropertyUnpublished);
        }

        [TestMethod()]
        public void CreatedProperty_AfterModifyAndCommitChanges_MayHaveNewValue()
        {
            // Arrange
            KWProperty testProperty;
            string propertyNewValue = "مقدار جدید ویژگی آزمایشی";
            long propertyIdAfterSupposalPublish = GenerateSupposalStoredPropertyId();
            var fakePropertyIds = new List<long>();
            var modifiedPropertyIDs = new long[] { };
            bool isTestPropertyUnpublished;
            using (ShimsContext.Create())
            {
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                // Act
                testProperty = PropertyManager.CreateNewProperty("نوع ویژگی آزمایشی", "مقدار ویژگی آزمایشی", testObject);
                PropertyManager.UpdatePropertyValue(testProperty, propertyNewValue);
                fakePropertyIds.Add(testProperty.ID);
                PropertyManager.CommitUnpublishedChanges(fakePropertyIds, modifiedPropertyIDs, 100);
                isTestPropertyUnpublished = PropertyManager.IsUnpublishedProperty(testProperty);
            }
            // Assert
            Assert.AreEqual(propertyNewValue, testProperty.Value);
        }

        [TestMethod()]
        public async Task StoredProperty_MayBeAccessableByItsId()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> givenPropertiesForId;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        {
                            Id = testObject.ID
                        };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };

                // Act
                givenPropertiesForId = await PropertyManager.GetPropertyListByIdAsync
                    (new long[] { propertyIdForSupposalPropertyOfTestObject });
            }
            // Assert
            givenPropertiesForId
                .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
        }

        [TestMethod()]
        public async Task StoredProperty_MayBeAccessableForItsOwnerObject()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> propertiesForObject;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyForObjectAsyncKObject = async (sc, o) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        {
                            Id = testObject.ID
                        };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (o) =>
                {
                    return false;
                };

                // Act
                propertiesForObject = await PropertyManager.GetPropertiesForObjectAsync(testObject);
            }
            // Assert
            propertiesForObject
                .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
        }

        [TestMethod()]
        public async Task StoredProperty_MayBeAccessableByItsOwnerObjectAndType()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            string specifiedPropertyType = "نوع ویژگی آزمایشی";
            IEnumerable<KWProperty> specifiedPropertiesForObject;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetSpecifiedPropertiesOfObjectsByTypesAsyncInt64ArrayStringArray = async (sc, a, b) =>
                     {
                         await Task.Delay(0);
                         var owner = new KObject()
                         {
                             Id = testObject.ID,
                             TypeUri = testObject.TypeURI,
                             IsGroup = false
                         };
                         var proeprtyForTestObject = new KProperty()
                         {
                             Id = propertyIdForSupposalPropertyOfTestObject,
                             TypeUri = specifiedPropertyType,
                             Value = "مقدار ویژگی آزمایشی",
                             Owner = owner
                         };
                         return new KProperty[] { proeprtyForTestObject };
                     };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (obj) =>
                {
                    if (obj.Equals(testObject))
                        return false;
                    else
                        return ObjectManager.IsUnpublishedObject(obj);
                };

                // Act
                specifiedPropertiesForObject = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync
                    (new KWObject[] { testObject }
                    , new string[] { specifiedPropertyType });
            }
            // Assert
            specifiedPropertiesForObject
                .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
        }

        [TestMethod()]
        public async Task StoredProperty_MayNotKnownAsUnpublishedProperty()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> propertiesForObject;
            KWProperty retrievedPropertyForObject;
            bool isRetrievedPropertyKnownAsUnpublished;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyForObjectAsyncKObject = async (sc, o) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        {
                            Id = testObject.ID
                        };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };
                Fakes.ShimObjectManager.IsUnpublishedObjectKWObject = (o) =>
                {
                    return false;
                };

                // Act
                propertiesForObject = await PropertyManager.GetPropertiesForObjectAsync(testObject);
                retrievedPropertyForObject = propertiesForObject
                    .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
                isRetrievedPropertyKnownAsUnpublished = PropertyManager.IsUnpublishedProperty(retrievedPropertyForObject);
            }
            // Assert
            Assert.IsFalse(isRetrievedPropertyKnownAsUnpublished);
        }

        [TestMethod()]
        public async Task StoredProperty_AfterModify_MayAppearedInUnpublishedChanges()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> retrievedProperties;
            KWProperty retrievedProperty;
            UnpublishedPropertyChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        { Id = testObject.ID };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };

                // Act
                retrievedProperties = await PropertyManager.GetPropertyListByIdAsync(new long[] { propertyIdForSupposalPropertyOfTestObject });
                retrievedProperty = retrievedProperties
                    .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
                PropertyManager.UpdatePropertyValue(retrievedProperty, "مقدار جدید ویژگی آزمایشی");
                changes = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedProperties.Contains(retrievedProperty));
            Assert.IsTrue(changes.ModifiedProperties.Contains(retrievedProperty));
        }

        [TestMethod()]
        public async Task StoredProperty_AfterModifyAndCommitTheModification_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> retrievedProperties;
            KWProperty retrievedProperty;
            var emptyfakePropertyIds = new List<long>();
            long[] modifiedPropertyIDs;
            UnpublishedPropertyChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        { Id = testObject.ID };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };

                // Act
                retrievedProperties = await PropertyManager.GetPropertyListByIdAsync(new long[] { propertyIdForSupposalPropertyOfTestObject });
                retrievedProperty = retrievedProperties
                    .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
                PropertyManager.UpdatePropertyValue(retrievedProperty, "مقدار جدید ویژگی آزمایشی");
                modifiedPropertyIDs = new long[] { propertyIdForSupposalPropertyOfTestObject };
                PropertyManager.CommitUnpublishedChanges(emptyfakePropertyIds, modifiedPropertyIDs, 100);
                changes = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedProperties.Contains(retrievedProperty));
            Assert.IsFalse(changes.ModifiedProperties.Contains(retrievedProperty));
        }
        [TestMethod()]
        public async Task StoredProperty_AfterModifyWithoutCommitTheModification_MayNotAppearedInUnpublishedChanges()
        {
            // Arrange
            long propertyIdForSupposalPropertyOfTestObject = GenerateSupposalStoredPropertyId();
            IEnumerable<KWProperty> retrievedProperties;
            KWProperty retrievedProperty;
            var emptyfakePropertyIds = new List<long>();
            long[] modifiedPropertyIDs = new long[] { };
            UnpublishedPropertyChanges changes;
            using (ShimsContext.Create())
            {// Fake Arrange!
                ShimCreateNewPropertyPreparation();
                KWObject testObject = ObjectManager.CreateNewObject("نوع شئ آزمایشی", "نمونه شئ آزمایشی");
                ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances
                    .GetPropertyListByIdAsyncInt64Array = async (sc, i) =>
                    {
                        await Task.Delay(0);
                        var owner = new KObject()
                        { Id = testObject.ID };
                        var proeprtyForTestObject = new KProperty()
                        {
                            Id = propertyIdForSupposalPropertyOfTestObject,
                            TypeUri = "نوع ویژگی آزمایشی",
                            Value = "مقدار ویژگی آزمایشی",
                            Owner = owner
                        };
                        return new KProperty[] { proeprtyForTestObject };
                    };

                // Act
                retrievedProperties = await PropertyManager.GetPropertyListByIdAsync(new long[] { propertyIdForSupposalPropertyOfTestObject });
                retrievedProperty = retrievedProperties
                    .Single(m => m.ID.Equals(propertyIdForSupposalPropertyOfTestObject));
                PropertyManager.UpdatePropertyValue(retrievedProperty, "مقدار جدید ویژگی آزمایشی");
                PropertyManager.CommitUnpublishedChanges(emptyfakePropertyIds, modifiedPropertyIDs, 100);
                changes = PropertyManager.GetUnpublishedChanges();
            }
            // Assert
            Assert.IsFalse(changes.AddedProperties.Contains(retrievedProperty));
            Assert.IsTrue(changes.ModifiedProperties.Contains(retrievedProperty));
        }
    }
}