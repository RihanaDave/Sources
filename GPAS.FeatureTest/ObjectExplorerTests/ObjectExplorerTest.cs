using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.StatisticalQuery.ResultNode;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.ObjectExplorerTests
{
    [TestClass]
    public class ObjectExplorerTest
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
        public async Task ExplorAllObjects()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";
            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newPublishPerson1 }, new List<KWProperty>() { newPublishPerson1.DisplayName }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> ofObjectTypes
                = new List<TypeBasedDrillDownPortionBase>()
                    { new OfObjectType() { ObjectTypeUri = "شخص" } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = ofObjectTypes;
            QueryResult statistics = await statisticalQueryProvider.RunQuery(query);
        }

        [TestMethod]
        public async Task ExplorOfObjectsType()
        {
            //Assign
            string personGUID = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{personGUID}Person 1";
            string newPersonLabe2 = $"Person{personGUID}Person 2";
            string organizationGUID = Guid.NewGuid().ToString();
            string newOrganizationLabel = $"{organizationGUID}Organization 1";
            string organizationTypeUri = "سازمان";
            string personTypeUri = "شخص";
            string labelTypeUri = "label";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabel);
            KWObject newPublishPerson2 = await ObjectManager.CreateNewObject(personTypeUri, newPersonLabe2);
            KWObject newPublishPorganization = await ObjectManager.CreateNewObject(organizationTypeUri, newOrganizationLabel);

            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, labelTypeUri, newPersonLabel);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newPublishPerson2, labelTypeUri, newPersonLabe2);
            KWProperty kWProperty3 = PropertyManager.CreateNewPropertyForObject(newPublishPorganization, labelTypeUri, newOrganizationLabel);


            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            properties.Add(kWProperty3);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);
            kwObjects.Add(newPublishPerson2);
            kwObjects.Add(newPublishPorganization);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> ofObjectTypes
                = new List<TypeBasedDrillDownPortionBase>()
                    { new OfObjectType() { ObjectTypeUri = personTypeUri}
                    , new OfObjectType() { ObjectTypeUri =  organizationTypeUri} };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = ofObjectTypes;

            // Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    kwObjects,
                    properties,
                    new List<KWMedia>(),
                    new List<KWRelationship>(),
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            QueryResult statistics = await statisticalQueryProvider.RunQuery(query);

            // Assert
            Assert.IsTrue(statistics.PropertyTypePreview.Count >= 3);
        }

        [TestMethod]
        public async Task ExplorHasPropertiesWithType()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";
            string newPersonLabe2 = $"Person{guid}";

            string propertyValue1 = $"{Guid.NewGuid().ToString()} ali";
            string propertyValue2 = $"{Guid.NewGuid().ToString()} hassan";
            string PropertyTypeUri = "نام";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWObject newPublishPerson2 = await ObjectManager.CreateNewObject("شخص", newPersonLabe2);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, PropertyTypeUri, propertyValue1);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newPublishPerson2, PropertyTypeUri, propertyValue2);

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);
            kwObjects.Add(newPublishPerson2);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> hasPropertyWithType
                = new List<TypeBasedDrillDownPortionBase>()
                    { new HasPropertyWithType() { PropertyTypeUri = PropertyTypeUri } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = hasPropertyWithType;

            // Act

            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                   kwObjects, properties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            QueryResult statistics = await statisticalQueryProvider.RunQuery(query);

            // Assert
            Assert.IsTrue(statistics.PropertyTypePreview.Count >= 2);
        }

        [TestMethod]
        public async Task ExplorHasPropertiesWithTypeRetrieveObjectsByQuery()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";
            string newPersonLabe2 = $"Person{guid}";

            string propertyValue1 = $"{Guid.NewGuid().ToString()} ali";
            string propertyValue2 = $"{Guid.NewGuid().ToString()} hassan";
            string PropertyTypeUri = "نام";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWObject newPublishPerson2 = await ObjectManager.CreateNewObject("شخص", newPersonLabe2);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, PropertyTypeUri, propertyValue1);
            KWProperty kWProperty2 = PropertyManager.CreateNewPropertyForObject(newPublishPerson2, PropertyTypeUri, propertyValue2);

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            properties.Add(kWProperty2);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);
            kwObjects.Add(newPublishPerson2);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> hasPropertyWithType
                = new List<TypeBasedDrillDownPortionBase>()
                    { new HasPropertyWithType() { PropertyTypeUri = PropertyTypeUri } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = hasPropertyWithType;

            // Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                   kwObjects, properties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            var objcestsResult = await statisticalQueryProvider.RetrieveObjectsByQuery(query);

            // Assert
            Assert.IsTrue(objcestsResult.Count >= 2);
        }


        [TestMethod]
        public async Task ExplorLongPropertyBarValues()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";

            long propertyValue1 = 24;
            string PropertyTypeUri = "سن";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, PropertyTypeUri, propertyValue1.ToString());

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> hasPropertyWithType
                = new List<TypeBasedDrillDownPortionBase>()
                    { new HasPropertyWithType() { PropertyTypeUri = PropertyTypeUri } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = hasPropertyWithType;

            // Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                   kwObjects, properties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            PropertyBarValues propertyBarValues = await statisticalQueryProvider.RetrievePropertyBarValuesStatistics(query, PropertyTypeUri, 100, 0, 100);

            // Assert
            Assert.IsTrue(propertyBarValues.Bars.Count >= 1);
        }

        [TestMethod]
        public async Task ExplorFloatPropertyBarValues()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";

            double propertyValue1 = 175.5;
            string PropertyTypeUri = "قد";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, PropertyTypeUri, propertyValue1.ToString());

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<TypeBasedDrillDownPortionBase> hasPropertyWithType
                = new List<TypeBasedDrillDownPortionBase>()
                    { new HasPropertyWithType() { PropertyTypeUri = PropertyTypeUri } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = hasPropertyWithType;

            // Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                   kwObjects, properties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            PropertyBarValues propertyBarValues = await statisticalQueryProvider.RetrievePropertyBarValuesStatistics(query, PropertyTypeUri, 100, 100, 200);

            // Assert
            Assert.IsTrue(propertyBarValues.Bars.Count >= 1);
        }

        [TestMethod]
        public async Task ExplorPropertyBarValuesRetrieveObjectsByQuery()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string newPersonLabel = $"Person{guid}";

            long propertyValue1 = 34;
            string PropertyTypeUri = "سن";

            KWObject newPublishPerson1 = await ObjectManager.CreateNewObject("شخص", newPersonLabel);
            KWProperty kWProperty1 = PropertyManager.CreateNewPropertyForObject(newPublishPerson1, PropertyTypeUri, propertyValue1.ToString());

            List<KWProperty> properties = new List<KWProperty>();
            properties.Add(kWProperty1);
            List<KWObject> kwObjects = new List<KWObject>();
            kwObjects.Add(newPublishPerson1);

            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();

            PropertyValueRangeDrillDown propertyValueRangeDrill = new PropertyValueRangeDrillDown()
            {
                DrillDownDetails = new PropertyValueRangeStatistics()
                {
                    Bars = new List<PropertyValueRangeStatistic>()
                    {
                        new PropertyValueRangeStatistic()
                        {
                            Start = 33,
                            End = 35
                        }
                    },
                    BucketCount = 0,
                    MaxValue = 0,
                    MinValue = 0,
                    NumericPropertyTypeUri = "سن"
                }
            };

            Query query = new Query();
            query.FormulaSequence.Add(propertyValueRangeDrill);

            // Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                   kwObjects, properties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );

            List<KWObject> retrievedObjects = await statisticalQueryProvider.RetrieveObjectsByQuery(query);

            // Assert
            Assert.IsTrue(retrievedObjects.Count >= 1);
        }
    }
}
