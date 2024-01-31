using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.FeatureTest.PublishTests
{
    [TestClass]
    public class PublishObject
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
        public async Task CreateNewObjectBasedOnGuid()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string label = $"{guid}obj1";
            KWObject newObj = await ObjectManager.CreateNewObject("شخص", label);

            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newObj }, new List<KWProperty>() { newObj.DisplayName }, new List<KWMedia>(), new List<KWRelationship>() , new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            KWObject retrivePublishedObj = (await ObjectManager.RetriveObjectsAsync(new List<long> { newObj.ID })).FirstOrDefault();
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(retrivePublishedObj.ID, newObj.ID);
        }

        [TestMethod]
        public async Task CreateNewObjectBasedOnName()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string label = "Justin1988";
            string typeURI = "شخص";
            KWObject newObj = await ObjectManager.CreateNewObject(typeURI, label);

            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newObj }, new List<KWProperty>() { newObj.DisplayName }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            KWObject retrivePublishedObj = (await ObjectManager.RetriveObjectsAsync(new List<long> { newObj.ID })).FirstOrDefault();
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(retrivePublishedObj.ID, newObj.ID);
        }
        [TestMethod]
        public async Task CreateNewGeoTimeProperty()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj = await ObjectManager.CreateNewObject("شخص", label);
            Random random = new Random();
            GeoTimeEntityRawData geoTime = new GeoTimeEntityRawData()
            {
                Latitude = (random.NextDouble() * (180) - 90).ToString(),
                Longitude = (random.NextDouble() * (360) - 180).ToString(),
                TimeBegin = (DateTime.Now).ToString(),
                TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
            };
            KWProperty newGeoTimeProperty =
                PropertyManager.CreateNewPropertyForObject(newObj, ontology.GetDateRangeAndLocationPropertyTypeUri(), GeoTime.GetGeoTimeStringValue(geoTime));
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newObj }, new List<KWProperty>() { newObj.DisplayName, newGeoTimeProperty }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            KWObject retrivePublishedObj = (await ObjectManager.RetriveObjectsAsync(new List<long> { newObj.ID })).FirstOrDefault();
            List<KWProperty> retrivedPublishedObjProperties = (await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList();
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(retrivePublishedObj.ID, newObj.ID);
            foreach (var retrivedPublishedObjProperty in retrivedPublishedObjProperties)
            {
                if (ontology.GetDateRangeAndLocationPropertyTypeUri() == retrivedPublishedObjProperty.TypeURI)
                    Assert.AreEqual(retrivedPublishedObjProperty.ID, newGeoTimeProperty.ID);
            }
        }
        [TestMethod]
        public async Task ModifyGeoTimeProperty()
        {
            //Assign
            string guid = Guid.NewGuid().ToString();
            string label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj = await ObjectManager.CreateNewObject("شخص", label);
            Random random = new Random();
            GeoTimeEntityRawData geoTime = new GeoTimeEntityRawData()
            {
                Latitude = (random.NextDouble() * (180) - 90).ToString(),
                Longitude = (random.NextDouble() * (360) - 180).ToString(),
                TimeBegin = (DateTime.Now).ToString(CultureInfo.InvariantCulture),
                TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString(CultureInfo.InvariantCulture)
            };
            KWProperty newGeoTimeProperty =
                PropertyManager.CreateNewPropertyForObject(newObj, ontology.GetDateRangeAndLocationPropertyTypeUri(), GeoTime.GetGeoTimeStringValue(geoTime));
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>() { newObj }, new List<KWProperty>() { newObj.DisplayName, newGeoTimeProperty }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            KWObject retrivePublishedObj = (await ObjectManager.RetriveObjectsAsync(new List<long> { newObj.ID })).FirstOrDefault();
            List<KWProperty> modifyPublishedObjProperties = (await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList();
            GeoTimeEntityRawData modifyGeoTimeProperty = GeoTime.GeoTimeEntityRawData(modifyPublishedObjProperties[1].Value);
            modifyGeoTimeProperty.TimeBegin = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            modifyGeoTimeProperty.TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString(CultureInfo.InvariantCulture);
            modifyGeoTimeProperty.Longitude = (random.NextDouble() * (360) - 180).ToString();
            modifyGeoTimeProperty.Latitude = (random.NextDouble() * (180) - 90).ToString();
            modifyPublishedObjProperties[1].Value = GeoTime.GetGeoTimeStringValue(modifyGeoTimeProperty);
            publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>(), new List<KWProperty>() { modifyPublishedObjProperties[1] }, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            retrivePublishedObj = (await ObjectManager.RetriveObjectsAsync(new List<long> { newObj.ID })).FirstOrDefault();
            List<KWProperty> retrivedPublishedObjProperties = (await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList();
            //Assert
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Assert.AreEqual(retrivePublishedObj.ID, newObj.ID);
            foreach (var retrivedPublishedObjProperty in retrivedPublishedObjProperties)
            {
                if (ontology.GetDateRangeAndLocationPropertyTypeUri() == retrivedPublishedObjProperty.TypeURI)
                    Assert.AreEqual(retrivedPublishedObjProperty.ID, newGeoTimeProperty.ID);
            }
        }

        [TestMethod]
        public async Task CreateDateTimeProperty()
        {
            //Assign
            //obj1
            string guid = Guid.NewGuid().ToString();
            string obj1Label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            KWProperty dateTime1 = PropertyManager.CreateNewPropertyForObject(newObj1, "تاریخ_تولد", DateTime.Now.ToString(CultureInfo.InvariantCulture));

            //obj2
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            KWProperty dateTime2 = PropertyManager.CreateNewPropertyForObject(newObj2, "تاریخ_تولد", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            //event1 
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "ایمیل", LinkDirection.Bidirectional, null, null, "myDiscription");
            KWProperty sendDate = PropertyManager.CreateNewPropertyForObject(event1.IntermediaryEvent, "زمان_ارسال", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            //Add To List
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            List<List<KWProperty>> objectsProperties = new List<List<KWProperty>>();
            objectsProperties.Add(new List<KWProperty>() { newObj1.DisplayName, dateTime1 });
            objectsProperties.Add(new List<KWProperty>() { newObj2.DisplayName, dateTime2 });
            objectsProperties.Add(new List<KWProperty>() { event1.IntermediaryEvent.DisplayName, sendDate });



            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs,
                    new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName, event1.IntermediaryEvent.DisplayName, dateTime1, dateTime2, sendDate },
                    new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship },
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWObject> retrivePublishedObjs = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsProperties = new List<List<KWProperty>>();
            foreach (var retrivePublishedObj in retrivePublishedObjs)
            {
                retrivedObjectsProperties.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList());
            }
            //Assert

            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            for (int i = 0; i < retrivePublishedObjs.Count; i++)
            {
                Assert.AreEqual(retrivePublishedObjs[i].ID, objs[i].ID);
                for (int j = 0; j < retrivedObjectsProperties[i].Count; j++)
                {
                    Assert.AreEqual(retrivedObjectsProperties[i][j].ID, objectsProperties[i][j].ID);
                }
            }

        }
        [TestMethod]
        public async Task ModifyDateTimeProperty()
        {
            //Assign
            //obj1
            string guid = Guid.NewGuid().ToString();
            string obj1Label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            KWProperty dateTime1 = PropertyManager.CreateNewPropertyForObject(newObj1, "تاریخ_تولد", DateTime.Now.ToString(CultureInfo.InvariantCulture));

            //obj2
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            KWProperty dateTime2 = PropertyManager.CreateNewPropertyForObject(newObj2, "تاریخ_تولد", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            //event1 Email
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "ایمیل", LinkDirection.Bidirectional, null, null, "myDiscription");
            KWProperty sendDate = PropertyManager.CreateNewPropertyForObject(event1.IntermediaryEvent, "زمان_ارسال", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            //Add To List
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            List<List<KWProperty>> objectsProperties = new List<List<KWProperty>>();
            objectsProperties.Add(new List<KWProperty>() { newObj1.DisplayName, dateTime1 });
            objectsProperties.Add(new List<KWProperty>() { newObj2.DisplayName, dateTime2 });
            objectsProperties.Add(new List<KWProperty>() { event1.IntermediaryEvent.DisplayName, sendDate });
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs,
                    new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName, event1.IntermediaryEvent.DisplayName, dateTime1, dateTime2, sendDate },
                    new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship },
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            //
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            List<KWObject> retrivePublishedObjsForModify = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsPropertiesForModify = new List<List<KWProperty>>();
            foreach (var retrivePublishedObj in retrivePublishedObjsForModify)
            {
                retrivedObjectsPropertiesForModify.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList());
            }
            List<KWProperty> modifiedProperties = new List<KWProperty>();
            foreach (var retriveObjectPropertiesForModify in retrivedObjectsPropertiesForModify)
            {
                foreach (var retriveObjectPropertyForModify in retriveObjectPropertiesForModify)
                {
                    if (ontology.GetBaseDataTypeOfProperty(retriveObjectPropertyForModify.TypeURI) == Ontology.BaseDataTypes.DateTime)
                    {
                        retriveObjectPropertyForModify.Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                        modifiedProperties.Add(retriveObjectPropertyForModify);
                    }
                }
            }
            publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>(), modifiedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            //Assert
            List<KWObject> retrivePublishedObjs = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsProperties = new List<List<KWProperty>>();
            foreach (var retrivePublishedObj in retrivePublishedObjs)
            {
                retrivedObjectsProperties.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList());
            }


            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            foreach (var retriveObjectProperties in retrivedObjectsProperties)
            {
                foreach (var retriveObjectProperty in retriveObjectProperties)
                {
                    if (ontology.GetBaseDataTypeOfProperty(retriveObjectProperty.TypeURI) == Ontology.BaseDataTypes.DateTime)
                    {
                        foreach (var modifiedPrperty in modifiedProperties)
                        {
                            if (retriveObjectProperty.ID == modifiedPrperty.ID)
                            {
                                Assert.AreEqual(retriveObjectProperty.ID, modifiedPrperty.ID);
                            }
                        }
                    }
                }
            }
        }
        [TestMethod]
        public async Task CreateNumericProperty()
        {            
            //Assign
            //obj1
            Random random = new Random();
            string guid = Guid.NewGuid().ToString();
            string obj1Label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            KWProperty doubleProperty = PropertyManager.CreateNewPropertyForObject(newObj1, "قد", "2.5");

            //obj2
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            KWProperty numericProperty2 = PropertyManager.CreateNewPropertyForObject(newObj2, "سن", random.Next(1, 120).ToString());
            //event1 Email
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "خرید", LinkDirection.Bidirectional, null, null, "myDiscription");
            KWProperty event1LongProperty = PropertyManager.CreateNewPropertyForObject(event1.IntermediaryEvent, "مبلغ", NextRandomLong(random, 0, long.MaxValue).ToString());
            //Add To List
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            List<List<KWProperty>> objectsProperties = new List<List<KWProperty>>();
            objectsProperties.Add(new List<KWProperty>() { newObj1.DisplayName, doubleProperty });
            objectsProperties.Add(new List<KWProperty>() { newObj2.DisplayName, numericProperty2 });
            objectsProperties.Add(new List<KWProperty>() { event1.IntermediaryEvent.DisplayName, event1LongProperty });
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs,
                    new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName, event1.IntermediaryEvent.DisplayName, doubleProperty, numericProperty2, event1LongProperty },
                    new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship },
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWObject> retrivePublishedObjs = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsProperties = new List<List<KWProperty>>();
            foreach (var retrivePublishedObj in retrivePublishedObjs)
            {
                retrivedObjectsProperties.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList());
            }
            //Assert

            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            for (int i = 0; i < retrivePublishedObjs.Count; i++)
            {
                Assert.AreEqual(retrivePublishedObjs[i].ID, objs[i].ID);
                for (int j = 0; j < retrivedObjectsProperties[i].Count; j++)
                {
                    Assert.AreEqual(retrivedObjectsProperties[i][j].ID, objectsProperties[i][j].ID);
                }
            }
        }
        [TestMethod]
        public async Task ModifyNumericProperty()
        {
            //Assign
            //obj1
            Random random = new Random();
            string guid = Guid.NewGuid().ToString();
            string obj1Label = $"{guid}obj1";
            var ontology = OntologyProvider.GetOntology();
            KWObject newObj1 = await ObjectManager.CreateNewObject("شخص", obj1Label);
            KWProperty doubleProperty = PropertyManager.CreateNewPropertyForObject(newObj1, "قد", "10.5");

            //obj2
            string guid2 = Guid.NewGuid().ToString();
            string obj2Label = $"{guid2}obj2";
            KWObject newObj2 = await ObjectManager.CreateNewObject("شخص", obj2Label);
            KWProperty numericProperty = PropertyManager.CreateNewPropertyForObject(newObj2, "سن", random.Next(1, 120).ToString());
            //event1 Email
            EventBasedKWLink event1 = LinkManager.CreateEventBaseLink(newObj1, newObj2, "خرید", LinkDirection.Bidirectional, null, null, "myDiscription");
            KWProperty event1LongProperty = PropertyManager.CreateNewPropertyForObject(event1.IntermediaryEvent, "مبلغ", NextRandomLong(random, 0, long.MaxValue).ToString());
            //Add To List
            List<KWObject> objs = new List<KWObject>();
            objs.Add(newObj1);
            objs.Add(newObj2);
            objs.Add(event1.IntermediaryEvent);
            List<List<KWProperty>> objectsProperties = new List<List<KWProperty>>();
            objectsProperties.Add(new List<KWProperty>() { newObj1.DisplayName, doubleProperty });
            objectsProperties.Add(new List<KWProperty>() { newObj2.DisplayName, numericProperty });
            objectsProperties.Add(new List<KWProperty>() { event1.IntermediaryEvent.DisplayName, event1LongProperty });
            //Act
            PublishResultMetadata publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    objs,
                    new List<KWProperty>() { newObj1.DisplayName, newObj2.DisplayName, event1.IntermediaryEvent.DisplayName, doubleProperty, numericProperty, event1LongProperty },
                    new List<KWMedia>(), new List<KWRelationship>() { event1.FirstRelationship, event1.SecondRelationship },
                    new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWObject> retrivePublishedObjsForModify = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsPropertiesForModify = new List<List<KWProperty>>();
            foreach (var retrivePublishedObjForModify in retrivePublishedObjsForModify)
            {
                retrivedObjectsPropertiesForModify.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObjForModify)).ToList());
            }
            List<KWProperty> modifiedProperties = new List<KWProperty>();

            foreach (var retriveObjectPropertiesForModify in retrivedObjectsPropertiesForModify)
            {
                foreach (var retriveObjectPropertyForModify in retriveObjectPropertiesForModify)
                {
                    if (retriveObjectPropertyForModify.ID == doubleProperty.ID)
                    {
                        retriveObjectPropertyForModify.Value = (Math.Abs(random.NextDouble() * (220) - 220)).ToString();
                        modifiedProperties.Add(retriveObjectPropertyForModify);
                    }
                    if (retriveObjectPropertyForModify.ID == event1LongProperty.ID)
                    {
                        retriveObjectPropertyForModify.Value = (Math.Abs(random.NextDouble() * (220) - 220)).ToString();
                        modifiedProperties.Add(retriveObjectPropertyForModify);
                    }
                    if (retriveObjectPropertyForModify.ID == doubleProperty.ID)
                    {
                        retriveObjectPropertyForModify.Value = NextRandomLong(random, 0, long.MaxValue).ToString();
                        modifiedProperties.Add(retriveObjectPropertyForModify);
                    }
                    if (retriveObjectPropertyForModify.ID == numericProperty.ID)
                    {
                        retriveObjectPropertyForModify.Value = random.Next(1, 120).ToString();
                        modifiedProperties.Add(retriveObjectPropertyForModify);
                    }
                }
            }
            publishResult =
                await PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                    new List<KWObject>(), modifiedProperties, new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
                    );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            //Assert
            List<KWObject> retrivePublishedObjs = (await ObjectManager.RetriveObjectsAsync(objs.Select(o => o.ID))).ToList();
            List<List<KWProperty>> retrivedObjectsProperties = new List<List<KWProperty>>();
            foreach (var retrivePublishedObj in retrivePublishedObjs)
            {
                retrivedObjectsProperties.Add((await PropertyManager.GetPropertiesOfObjectAsync(retrivePublishedObj)).ToList());
            }


            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            foreach (var retriveObjectProperties in retrivedObjectsProperties)
            {
                foreach (var retriveObjectProperty in retriveObjectProperties)
                {
                    foreach (var modifiedPrperty in modifiedProperties)
                    {
                        if (retriveObjectProperty.ID == modifiedPrperty.ID)
                        {
                            Assert.AreEqual(retriveObjectProperty.ID, modifiedPrperty.ID);
                        }
                    }
                }
            }
        }
        private static long NextRandomLong(Random random, long min, long max)
        {
            if (max <= min)
                throw new ArgumentOutOfRangeException("max", "max must be > min!");

            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }
    }
}
