using GPAS.Workspace.Logic;
using GPAS.Dispatch.Entities.Concepts.Geo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.GeoSearch;
using GPAS.Workspace.Entities;

namespace GPAS.FeatureTest.PropertyRetrievalTests
{
    [TestClass]
    public class RetrievePropertyByID
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
        public async Task GetPublishedObject()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjectWithPropertyList = new List<KWObjectWithPropertiesForTest>();
            Dictionary<string, List<string>> properties1 = new Dictionary<string, List<string>>();
            properties1.Add("سن", new List<string> { "25" });
            List<GeoTimeEntityRawData> geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
            };
            ObjectWithPropertyList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));
            Dictionary<string, List<string>> properties2 = new Dictionary<string, List<string>>();
            properties2.Add("سن", new List<string> { "17", "35" });
            properties2.Add("قد", new List<string> { "180" });

            List<GeoTimeEntityRawData> geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (45).ToString(),
                    Longitude = (90).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjectWithPropertyList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2));

            Dictionary<string, List<string>> properties3 = new Dictionary<string, List<string>>();
            properties3.Add("سن", new List<string> { "15" });
            List<GeoTimeEntityRawData> geoTimeProperties3 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (.008).ToString(),
                    Longitude = (.009).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (-30).ToString(),
                    Longitude = (60).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }

            };

            ObjectWithPropertyList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3));

            List<KWObject> objects = ObjectWithPropertyList.Select(o => o.KWObject).ToList();
            List<long> objectsIds = objects.Select(o => o.ID).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjectWithPropertyList)
            {
                properties.AddRange(o.Properties);
            }
            foreach (KWObject obj in objects)
            {
                if (!properties.Contains(obj.DisplayName))
                {
                    properties.Add(obj.DisplayName);
                }
            }

            //Act
            var publishResult = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                objects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            List<long> propertiesID = properties.Select(o => o.ID).ToList();
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            List<KWProperty> resultProperties = (await PropertyManager.RetriveProeprtiesByIdAsync(propertiesID)).ToList();
        
            // Assert

            for (int i = 0; i < propertiesID.Count; i++)
            {
                Assert.IsTrue(propertiesID.Contains(resultProperties[i].ID));
            }
        }

        [TestMethod]
        public async Task GetUnPublishedObject()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjectWithPropertyList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();
            properties1.Add("سن", new List<string> { "25" });
            properties1.Add("نام", new List<string> { "John" });

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
            };

            ObjectWithPropertyList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));

            var properties2 = new Dictionary<string, List<string>>();
            properties2.Add("سن", new List<string> { "40", "35" });
            properties2.Add("قد", new List<string> { "193" });

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> { };

            ObjectWithPropertyList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2));

            List<KWObject> objects = ObjectWithPropertyList.Select(o => o.KWObject).ToList();
            List<long> objectsIds = objects.Select(o => o.ID).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjectWithPropertyList)
            {
                properties.AddRange(o.Properties);
            }
            foreach (KWObject obj in objects)
            {
                if (!properties.Contains(obj.DisplayName))
                {
                    properties.Add(obj.DisplayName);
                }
            }
            List<long> propertiesID = properties.Select(o => o.ID).ToList();
            List<KWObject> resultObj = (await ObjectManager.RetriveObjectsAsync(objectsIds)).ToList();
            List<KWProperty> resultProperties = (await PropertyManager.RetriveProeprtiesByIdAsync(propertiesID)).ToList();
            // Assert

            foreach (var property in properties)
            {
                Assert.IsTrue(resultProperties.Select(o => o.ID).Contains(property.ID));
            }
        }
    }
}
