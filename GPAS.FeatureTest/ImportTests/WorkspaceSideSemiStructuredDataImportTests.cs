using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.GlobalResolve.Suite;
using GPAS.DataImport.Publish;
using GPAS.Workspace.Logic;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.ImportTests
{
    [TestClass]
    public class WorkspaceSideSemiStructuredDataImportTests
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
            //SaveInternalResolveTestFile();
        }

        [TestCleanup()]
        public void Cleanup()
        {
            //RemoveInternalResolveTestFile();
        }

        private static void SetNeededAppsettingsFakeAssigns()
        {
            global::System.Configuration.Fakes.ShimConfigurationManager.AppSettingsGet = () =>
            {
                var nvc = new global::System.Collections.Specialized.NameValueCollection();
                nvc.Add("ReportFullDetailsInImportLog", "True");
                nvc.Add("PublishMaximumRetryTimes", "5");
                nvc.Add("MinimumIntervalBetwweenIterrativeLogsReportInSeconds", "30");
                nvc.Add("PublishAcceptableFailsPercentage", "5");
                nvc.Add("MaxNumberOfGlobalResolutionCandidates", "50");
                return nvc;
            };
        }

        private string internalResolveTestFilePath = Environment.CurrentDirectory + "tempCsvFile.csv";
        private void SaveInternalResolveTestFile()
        {
            File.WriteAllText(internalResolveTestFilePath, Properties.Resources.Internal_Resolution_Test_File_);
        }
        private void RemoveInternalResolveTestFile()
        {
            if (File.Exists(internalResolveTestFilePath))
                File.Delete(internalResolveTestFilePath);
        }

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceSideSemiStructuredDataImport_NullArg3_ThrowsException()
        {
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                // Act
                ImportProvider.WorkspaceSideSemiStructuredDataImport(new List<ImportingObject>(), new List<ImportingRelationship>(), null, new GlobalResolutionSuite());
                // Assert -> Expected Exception
            }
        }

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkspaceSideSemiStructuredDataImport_NullArg4_ThrowsException()
        {
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                // Act
                ImportProvider.WorkspaceSideSemiStructuredDataImport(new List<ImportingObject>(), new List<ImportingRelationship>(), new DataSourceMetadata(), null);
                // Assert -> Expected Exception
            }
        }

        // Test Import mapping validation

        // TODO: تست‌های این بخش براساس تغییرات نحوه‌ی ورود داده‌ها اصلاح شوند
        #region کدهای غیرفعال شده به خاطر تغییر نحوه‌ی تعریف ورود داده‌ها
        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsColumnWithoutInternalResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var BtsTypeURI = "BTS";
        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(BtsTypeURI), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);
        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                if (typeURI != BtsTypeURI || isMasterOfGroup)
        //                    throw new InvalidOperationException("Invalid object creation arguments");
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsColumnWithInternalResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var BtsTypeURI = "BTS";
        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(BtsTypeURI), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);
        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                if (typeURI != BtsTypeURI || isMasterOfGroup)
        //                    throw new InvalidOperationException("Invalid object creation arguments");
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(4, displayNameOfCreatedObjectsAfterImport.Count);
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5001"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5003"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5004"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5006"));
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsColumn_CreateEquivalentProperties()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var BtsIdTypeURI = "BTS_ID";
        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(BtsIdTypeURI), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);
        //    var valueOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                valueOfCreatedObjectsAfterImport.Add(value);
        //                if (typeURI != BtsIdTypeURI || owner == null)
        //                    throw new InvalidOperationException("Invalid property creation arguments");
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, valueOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsAndBTS2IDsColumnWithoutInternalAndAutoResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var btsObjectMapping1 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping1.SetDisplayName(btsObjectMapping1.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping1);

        //    var btsObjectMapping2 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۲");
        //    btsObjectMapping2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(11, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping2.SetDisplayName(btsObjectMapping2.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping2);

        //    mapping.InterTypeAutoInternalResolution = false;

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(14, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsAndBTS2IDsColumnWithoutInternalAndWithAutoResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var btsObjectMapping1 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping1.SetDisplayName(btsObjectMapping1.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping1);

        //    var btsObjectMapping2 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۲");
        //    btsObjectMapping2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(11, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping2.SetDisplayName(btsObjectMapping2.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping2);

        //    mapping.InterTypeAutoInternalResolution = true;

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(14, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsAndBTS2IDsColumnWithInternalAndWithoutAutoResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var btsObjectMapping1 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping1.SetDisplayName(btsObjectMapping1.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping1);

        //    var btsObjectMapping2 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۲");
        //    btsObjectMapping2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(11, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping2.SetDisplayName(btsObjectMapping2.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping2);

        //    mapping.InterTypeAutoInternalResolution = false;

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(8, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportBTS1IDsAndBTS2IDsColumnWithInternalAndAutoResolution_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var btsObjectMapping1 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping1.SetDisplayName(btsObjectMapping1.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping1);

        //    var btsObjectMapping2 = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۲");
        //    btsObjectMapping2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(11, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping2.SetDisplayName(btsObjectMapping2.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping2);

        //    mapping.InterTypeAutoInternalResolution = true;

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(6, displayNameOfCreatedObjectsAfterImport.Count);
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5001"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5002"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5003"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5004"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5005"));
        //    Assert.IsTrue(displayNameOfCreatedObjectsAfterImport.Contains("5006"));
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportPerson1WithIgnorableProperties_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var personObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Name"), new TableColumnMappingItem(0, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Phone_Number"), new TableColumnMappingItem(1, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.SetDisplayName(personObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(personObjectMapping);

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportPerson1WithFindMatchMustMatchAndIgnorableProperties_CreateEquivalentObjects()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var personObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Name"), new TableColumnMappingItem(0, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Phone_Number"), new TableColumnMappingItem(1, "", PropertyInternalResolutionOption.MustMatch)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch)));
        //    personObjectMapping.SetDisplayName(personObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(personObjectMapping);

        //    var displayNameOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                displayNameOfCreatedObjectsAfterImport.Add(displayName);
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(5, displayNameOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportPerson2WithFindMatchMustMatchAndIgnorableProperties_CreateEquivalentProperties()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var personObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Name"), new TableColumnMappingItem(3, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Phone_Number"), new TableColumnMappingItem(4, "", PropertyInternalResolutionOption.MustMatch)));
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(5, "", PropertyInternalResolutionOption.FindMatch)));
        //    personObjectMapping.SetDisplayName(personObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(personObjectMapping);

        //    var valueOfCreatedObjectsAfterImport = new List<string>();

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                valueOfCreatedObjectsAfterImport.Add(value);
        //                if (owner == null)
        //                    throw new InvalidOperationException("Invalid property creation arguments");
        //                return new KWProperty();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(17, valueOfCreatedObjectsAfterImport.Count);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportPerson1AndBTS1_CreateEquivalentRelationships()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var personObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch)));
        //    personObjectMapping.SetDisplayName(personObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(personObjectMapping);

        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.FindMatch)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);

        //    var person2btsRelationshipMapping = new RelationshipMapping(personObjectMapping, btsObjectMapping, RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary, new ConstValueMappingItem("Appeared In"), new OntologyTypeMappingItem("Appeared_In"));
        //    mapping.AddRelationshipMapping(person2btsRelationshipMapping);

        //    var numberOfCreatedRelationships = 0;

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Fakes.ShimLinkManager.CreateRelationshipBaseLinkAsyncKWObjectKWObjectStringLinkDirectionDirectionDateTimeDateTimeString
        //            = async (a, b, c, d, e, f, g) =>
        //            {
        //                await Task.Delay(0);
        //                numberOfCreatedRelationships++;
        //                return new RelationshipBasedKWLink();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, numberOfCreatedRelationships);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportPerson1AndBTS1WithoutResolution_CreateEquivalentRelationships()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var personObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    personObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.Ignorable)));
        //    personObjectMapping.SetDisplayName(personObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(personObjectMapping);

        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);

        //    var person2btsRelationshipMapping = new RelationshipMapping(personObjectMapping, btsObjectMapping, RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary, new ConstValueMappingItem("Appeared In"), new OntologyTypeMappingItem("Appeared_In"));
        //    mapping.AddRelationshipMapping(person2btsRelationshipMapping);

        //    var numberOfCreatedRelationships = 0;

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                return new Task<KWObject>(() => { return new KWObject(); });
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Fakes.ShimLinkManager.CreateRelationshipBaseLinkAsyncKWObjectKWObjectStringLinkDirectionDirectionDateTimeDateTimeString
        //            = async (a, b, c, d, e, f, g) =>
        //            {
        //                await Task.Delay(0);
        //                numberOfCreatedRelationships++;
        //                return new RelationshipBasedKWLink();
        //            };
        //        Publish.Fakes.ShimPublishManager.PublishSpecifiedAddedConceptsAsyncIEnumerableOfKWObjectIEnumerableOfKWPropertyIEnumerableOfKWRelationship
        //            = (a, b, c) =>
        //            { return Task.Delay(0); };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FileWorkspacePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, numberOfCreatedRelationships);
        //}
        #endregion

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //[TestCategory("تست یکپارچگی")]
        //public async Task WorkspaceSideSemiStructuredDataImport_ImportFullDatasetWithInternalAndAutoResolution_CreateEquivalentConcepts()
        //{
        //    // Arrange
        //    TypeMapping mapping = new TypeMapping();
        //    var person1ObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۱");
        //    person1ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Name"), new TableColumnMappingItem(0, "", PropertyInternalResolutionOption.Ignorable)));
        //    person1ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Phone_Number"), new TableColumnMappingItem(1, "", PropertyInternalResolutionOption.MustMatch)));
        //    person1ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch)));
        //    person1ObjectMapping.SetDisplayName(person1ObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(person1ObjectMapping);

        //    var person2ObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("Person"), "شخص ۲");
        //    person2ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Name"), new TableColumnMappingItem(3, "", PropertyInternalResolutionOption.Ignorable)));
        //    person2ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("Phone_Number"), new TableColumnMappingItem(4, "", PropertyInternalResolutionOption.MustMatch)));
        //    person2ObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("National_Code"), new TableColumnMappingItem(5, "", PropertyInternalResolutionOption.FindMatch)));
        //    person2ObjectMapping.SetDisplayName(person2ObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(person2ObjectMapping);



        //    var btsObjectMapping = new ObjectMapping(new OntologyTypeMappingItem("BTS"), "بی تی اس ۱");
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(9, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("BTS_ID"), new TableColumnMappingItem(10, "", PropertyInternalResolutionOption.Ignorable)));
        //    btsObjectMapping.SetDisplayName(btsObjectMapping.Properties[0]);
        //    mapping.AddObjectMapping(btsObjectMapping);

        //    var person2btsRelationshipMapping = new RelationshipMapping(person1ObjectMapping, btsObjectMapping, RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary, new ConstValueMappingItem("Appeared In"), new OntologyTypeMappingItem("Appeared_In"));
        //    mapping.AddRelationshipMapping(person2btsRelationshipMapping);

        //    var numberOfCreatedRelationships = 0;

        //    using (ShimsContext.Create())
        //    {
        //        Fakes.ShimObjectManager.CreateNewObjectStringStringBoolean
        //            = (typeURI, displayName, isMasterOfGroup) =>
        //            {
        //                return new KWObject();
        //            };
        //        Fakes.ShimPropertyManager.CreateNewPropertyForObjectKWObjectStringString
        //            = (owner, typeURI, value) =>
        //            {
        //                return new KWProperty();
        //            };
        //        Fakes.ShimLinkManager.CreateRelationshipBaseLinkAsyncKWObjectKWObjectStringLinkDirectionDirectionDateTimeDateTimeString
        //            = async (a, b, c, d, e, f, g) =>
        //            {
        //                await Task.Delay(0);
        //                numberOfCreatedRelationships++;
        //                return new RelationshipBasedKWLink();
        //            };

        //        // Act
        //        await ImportProvider.WorkspaceSideSemiStructuredDataImportAsync(new CsvFileMaterial() { FilePath = internalResolveTestFilePath }, mapping);
        //    }
        //    Assert.AreEqual(7, numberOfCreatedRelationships);
        //}

        //[TestMethod()]
        //[TestCategory("ورود داده‌ها")]
        //public void WorkspaceSideSemiStructuredDataImportTest()
        //{
        //    using (ShimsContext.Create())
        //    {
        //        //Arrange
        //        global::System.IO.Fakes.ShimStreamReader.ConstructorString = (sr, s) =>
        //        {
        //            byte[] cSVFile = Encoding.UTF8.GetBytes(LogicTest.Properties.Resources.Internal_Resolution_Test_File_);
        //            sr = new StreamReader(new MemoryStream(cSVFile));

        //            //global::System.IO.Fakes.ShimStreamReader.ConstructorString = (sr, s) =>
        //            //{ sr = fakeStreamReader; };
        //            //global::System.IO.Fakes.ShimStreamReader.AllInstances.Close = (sr) =>
        //            //{ fakeStreamReader.Close(); };
        //            //global::System.IO.Fakes.ShimStreamReader.AllInstances.EndOfStreamGet = (sr) =>
        //            //{ return fakeStreamReader.EndOfStream; };
        //            //global::System.IO.Fakes.ShimStreamReader.AllInstances.ReadLine = (sr) =>
        //            //{ return fakeStreamReader.ReadLine(); };
        //        };

        //        throw new NotImplementedException();

        //        //Act


        //        //Assert
        //    }
        //}
    }
}