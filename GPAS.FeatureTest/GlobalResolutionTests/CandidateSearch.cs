using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.GlobalResolve.Suite;
using GPAS.DataImport.Publish;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.GlobalResolution;
using GPAS.Workspace.Entities.GraphResolution;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.FeatureTest.GlobalResolutionTests
{
    [TestClass]
    public class CandidateSearch
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

        [TestMethod]
        public async Task GetCandidateStringPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string propId = Guid.NewGuid().ToString();
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{propId} name");
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{propId} name");
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{propId} name conflict");
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty("نام", $"{propId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateStringPropertyNoConflict()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string propId = Guid.NewGuid().ToString();
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{propId} name");
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{propId} name");
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{propId} name conflict");
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty("نام", $"{propId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.NoConflict,
                typeURI = "نام"
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(1, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateLongPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "سن";
            long property1Value = 14;
            long property2Value = 16;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateLongPropertyNoConflict()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "سن";
            long property1Value = 14;
            long property2Value = 16;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));
            importingPerson.AddPropertyForObject(new ImportingProperty(propertyType, property1Value.ToString()));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };

            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };
            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.NoConflict,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(1, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateFloatPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "قد";
            double property1Value = 14.9;
            double property2Value = 16.9;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateFloatPropertyNoConflict()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "قد";
            double property1Value = 14.9;
            double property2Value = 16.9;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.NoConflict,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(1, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateDateTimePropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "تاریخ_تولد";
            string date1String = "7/10/1974 7:10:24 AM";
            DateTime property1Value =
                DateTime.Parse(date1String, System.Globalization.CultureInfo.InvariantCulture);

            string date2String = "7/11/1974 7:10:24 AM";
            DateTime property2Value =
                DateTime.Parse(date2String, System.Globalization.CultureInfo.InvariantCulture);

            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        [TestMethod]
        public async Task GetCandidateDateTimePropertyNoConflict()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "تاریخ_تولد";
            string date1String = "7/10/1974 7:10:24 AM";
            DateTime property1Value =
                DateTime.Parse(date1String, System.Globalization.CultureInfo.InvariantCulture);

            string date2String = "7/11/1974 7:10:24 AM";
            DateTime property2Value =
                DateTime.Parse(date2String, System.Globalization.CultureInfo.InvariantCulture);

            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.NoConflict,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);
            //Assert
            Assert.AreEqual(1, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);
        }

        private void GenerateCandidatesContainers(List<ImportingObject> unresolvedObjects, CandidatesMatchingCriteria candidateFindingCriteria, ref List<CandidatesContainer> candidatesContainers, ref List<ImportingObject> importingObjectsWithEmptyLinkingProperty)
        {
            foreach (var imObj in unresolvedObjects)
            {
                TargetingObjTypeWithRelatedLinkingProperties targetingObjectWithLinkingProperties
                    = candidateFindingCriteria.TargetingObjectTypeAndLinkingProperties.FirstOrDefault(tl => tl.TargetingObjectType.typrUri.Equals(imObj.TypeUri));

                if (targetingObjectWithLinkingProperties != null
                    && targetingObjectWithLinkingProperties.LinkingProperties.Length != 0
                    // درصورتی که شئ فاقد یکی از ویژگی‌های مورد بررسی باشد، هیچ نتیجه‌ای برای آن برنخواهد گشت
                    && targetingObjectWithLinkingProperties.LinkingProperties.All(lp => imObj.Properties.Any(p => p.TypeURI.Equals(lp.typeURI))))
                {
                    CandidatesContainer candidatesContainer = new CandidatesContainer()
                    {
                        LinkingProperties = targetingObjectWithLinkingProperties.LinkingProperties.ToList(),
                        GlobalResolutionCandidates = new List<ResolutionCandidate>(),
                        Master = imObj
                    };
                    candidatesContainers.Add(candidatesContainer);
                }
                else
                {
                    importingObjectsWithEmptyLinkingProperty.Add(imObj);
                }
            }
        }
        
        [TestMethod]
        public async Task ResolveObjectsWithStringPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "سن";
            long property1Value = 14;
            long property2Value = 16;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);


            //resolve
            KWObject publishedPerson = await ObjectManager.CreateNewObject(objectType, importingPersonLabel);

            List<ObjectResolutionMap> objectsResolutionMap = new List<ObjectResolutionMap>();
            objectsResolutionMap.Add(new ObjectResolutionMap()
            {
                ResolvedObjects = unpublishedObjects,
                ResolveMaster = publishedPerson
            });

            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { publishedPerson },
                new List<KWProperty>()
               , new List<KWMedia>(), new List<KWRelationship>(), objectsResolutionMap
                );

            var unPublishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            var publishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);
            var resolvedTo = await ObjectManager.GetObjectById(publishedPerson1.ID);
            var properties = (await PropertyManager.GetPropertiesOfObjectAsync(publishedPerson)).ToList();
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);

            Assert.AreEqual(unPublishedResult.ID, resolvedTo.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson1Property.ID).FirstOrDefault(), publishedPerson1Property.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson2Property2.ID).FirstOrDefault(), publishedPerson2Property2.ID);

        }

        [TestMethod]
        public async Task ResolveObjectsWithLongPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "سن";
            long property1Value = 14;
            long property2Value = 16;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty ageLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                ageLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);


            //resolve
            KWObject publishedPerson = await ObjectManager.CreateNewObject(objectType, importingPersonLabel);

            List<ObjectResolutionMap> objectsResolutionMap = new List<ObjectResolutionMap>();
            objectsResolutionMap.Add(new ObjectResolutionMap()
            {
                ResolvedObjects = unpublishedObjects,
                ResolveMaster = publishedPerson
            });

            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { publishedPerson },
                new List<KWProperty>()
               , new List<KWMedia>(), new List<KWRelationship>(), objectsResolutionMap
                );

            var unPublishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            var publishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);
            var resolvedTo = await ObjectManager.GetObjectById(publishedPerson1.ID);
            var properties = (await PropertyManager.GetPropertiesOfObjectAsync(publishedPerson)).ToList();
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);

            Assert.AreEqual(unPublishedResult.ID, resolvedTo.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson1Property.ID).FirstOrDefault(), publishedPerson1Property.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson2Property2.ID).FirstOrDefault(), publishedPerson2Property2.ID);

        }

        [TestMethod]
        public async Task ResolveObjectsWithFloatPropertyExactMatch()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "قد";
            double property1Value = 14.9;
            double property2Value = 16.9;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty floatLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                floatLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);


            //resolve
            KWObject publishedPerson = await ObjectManager.CreateNewObject(objectType, importingPersonLabel);

            List<ObjectResolutionMap> objectsResolutionMap = new List<ObjectResolutionMap>();
            objectsResolutionMap.Add(new ObjectResolutionMap()
            {
                ResolvedObjects = unpublishedObjects,
                ResolveMaster = publishedPerson
            });

            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { publishedPerson },
                new List<KWProperty>()
               , new List<KWMedia>(), new List<KWRelationship>(), objectsResolutionMap
                );

            var unPublishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            var publishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);
            var resolvedTo = await ObjectManager.GetObjectById(publishedPerson1.ID);
            var properties = (await PropertyManager.GetPropertiesOfObjectAsync(publishedPerson)).ToList();
            //Assert
            Assert.AreEqual(2, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);

            Assert.AreEqual(unPublishedResult.ID, resolvedTo.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson1Property.ID).FirstOrDefault(), publishedPerson1Property.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson2Property2.ID).FirstOrDefault(), publishedPerson2Property2.ID);

        }


        [TestMethod]
        public async Task ResolveObjectsWithFloatPropertyNoConflict()
        {
            //Assign
            string publishedPersonLabel1 = $"{Guid.NewGuid().ToString()} Published Person 1";
            string publishedPersonLabel2 = $"{Guid.NewGuid().ToString()} Published Person 2";
            string importingPersonLabel = $"{Guid.NewGuid().ToString()} Importing Person";

            string namePropId = Guid.NewGuid().ToString();
            string propertyType = "قد";
            double property1Value = 14.9;
            double property2Value = 16.9;
            string objectType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWProperty> unpublishedProperties = new List<KWProperty>();

            KWObject publishedPerson1 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel1);
            KWProperty publishedPerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, propertyType, property1Value.ToString());
            unpublishedObjects.Add(publishedPerson1);
            unpublishedProperties.Add(publishedPerson1Property);
            KWProperty publishedNamePerson1Property = PropertyManager.CreateNewPropertyForObject(publishedPerson1, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson1Property);

            KWObject publishedPerson2 = await ObjectManager.CreateNewObject(objectType, publishedPersonLabel2);
            KWProperty publishedPerson2Property1 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property1Value.ToString());
            KWProperty publishedPerson2Property2 = PropertyManager.CreateNewPropertyForObject(publishedPerson2, propertyType, property2Value.ToString());
            unpublishedObjects.Add(publishedPerson2);
            unpublishedProperties.Add(publishedPerson2Property1);
            unpublishedProperties.Add(publishedPerson2Property2);
            KWProperty publishedNamePerson2Property = PropertyManager.CreateNewPropertyForObject(publishedPerson2, "نام", $"{namePropId} name");
            unpublishedProperties.Add(publishedNamePerson2Property);

            ImportingObject importingPerson = new ImportingObject(objectType, new ImportingProperty(propertyType, property1Value.ToString()));
            importingPerson.AddPropertyForObject(new ImportingProperty("نام", $"{namePropId} name"));
            importingPerson.AddPropertyForObject(new ImportingProperty(propertyType, property1Value.ToString()));

            CandidatesMatchingCriteria matchingCriteria = new CandidatesMatchingCriteria();
            TargetingObjTypeWithRelatedLinkingProperties objTypeWithLinkingProperties = new TargetingObjTypeWithRelatedLinkingProperties();
            objTypeWithLinkingProperties.TargetingObjectType = new DataImport.GlobalResolve.TargetingObject()
            {
                typrUri = "شخص"
            };
            DataImport.GlobalResolve.LinkingProperty nameLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.ExactMatch,
                typeURI = "نام"
            };

            DataImport.GlobalResolve.LinkingProperty floatLinkingProperty = new DataImport.GlobalResolve.LinkingProperty()
            {
                resolutionOption = DataImport.GlobalResolve.ResolutionOption.NoConflict,
                typeURI = propertyType
            };
            objTypeWithLinkingProperties.LinkingProperties = new DataImport.GlobalResolve.LinkingProperty[]
            {
                floatLinkingProperty,
                nameLinkingProperty
            };
            matchingCriteria.TargetingObjectTypeAndLinkingProperties =
                new TargetingObjTypeWithRelatedLinkingProperties[] { objTypeWithLinkingProperties };

            List<CandidatesContainer> candidatesContainerList = new List<CandidatesContainer>();
            List<ImportingObject> importingObjectsWithEmptyLinkingProperty = new List<ImportingObject>();

            //Act
            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());
            GenerateCandidatesContainers(new List<ImportingObject>() { importingPerson }, matchingCriteria, ref candidatesContainerList, ref importingObjectsWithEmptyLinkingProperty);
            Workspace.Logic.DataImport.PublishAdaptor adaptor = new Workspace.Logic.DataImport.PublishAdaptor();
            await adaptor.GetTypeBasedResolutionCandidates(candidatesContainerList);


            //resolve
            KWObject publishedPerson = await ObjectManager.CreateNewObject(objectType, importingPersonLabel);

            List<ObjectResolutionMap> objectsResolutionMap = new List<ObjectResolutionMap>();
            objectsResolutionMap.Add(new ObjectResolutionMap()
            {
                ResolvedObjects = unpublishedObjects,
                ResolveMaster = publishedPerson
            });

            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { publishedPerson },
                new List<KWProperty>()
               , new List<KWMedia>(), new List<KWRelationship>(), objectsResolutionMap
                );

            var unPublishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);

            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            var publishedResult = await ObjectManager.GetObjectById(publishedPerson.ID);
            var resolvedTo = await ObjectManager.GetObjectById(publishedPerson1.ID);
            var properties = (await PropertyManager.GetPropertiesOfObjectAsync(publishedPerson)).ToList();
            //Assert
            Assert.AreEqual(1, candidatesContainerList.First().GlobalResolutionCandidates.Count);
            Assert.AreEqual(importingPerson, candidatesContainerList.First().Master);

            Assert.AreEqual(unPublishedResult.ID, resolvedTo.ID);
            Assert.AreEqual(properties.Select(p => p.ID).Where(p => p == publishedPerson2Property2.ID).FirstOrDefault(), publishedPerson2Property2.ID);
        }
    }
}
