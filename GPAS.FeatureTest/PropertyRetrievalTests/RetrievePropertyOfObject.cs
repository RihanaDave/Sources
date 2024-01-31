using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.PropertyRetrievalTests
{
    [TestClass]
    public class RetrievePropertyOfObject
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
            KWObjectWithPropertiesForTest Obj = new KWObjectWithPropertiesForTest();

            var properties1 = new Dictionary<string, List<string>>();
            properties1.Add("سن", new List<string> { "15", "50" });
            properties1.Add("قد", new List<string> { "150" });

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
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

            Obj = (await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));
            Obj.Properties.Add(Obj.KWObject.DisplayName);
            List<long> propertiesID = Obj.Properties.Select(o => o.ID).ToList();
            var publishResult = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { Obj.KWObject }, Obj.Properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            //Act
            var resultProperties = (await PropertyManager.GetPropertiesOfObjectAsync(Obj.KWObject)).ToList();

            // Assert
            foreach (var propertyID in propertiesID)
            {
                Assert.IsTrue(resultProperties.Select(o=>o.ID).Contains(propertyID));
            }
        }

        [TestMethod]
        public async Task GetUnPublishedObject()
        {
            // Assign
            KWObjectWithPropertiesForTest Obj = new KWObjectWithPropertiesForTest();

            var properties1 = new Dictionary<string, List<string>>();
            properties1.Add("سن", new List<string> { "15", "50" });
            properties1.Add("قد", new List<string> { "150" });

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
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

            Obj = (await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));
            Obj.Properties.Add(Obj.KWObject.DisplayName);
            List<long> propertiesID = Obj.Properties.Select(o => o.ID).ToList();

            //Act
            var resultProperties = (await PropertyManager.GetPropertiesOfObjectAsync(Obj.KWObject)).ToList();

            // Assert
            foreach (var propertyID in propertiesID)
            {
                Assert.IsTrue(resultProperties.Select(o => o.ID).Contains(propertyID));
            }
        }

        [TestMethod]
        public async Task GetPublishedObjectWithoutProperties()
        {
            // Assign
            KWObjectWithPropertiesForTest Obj = new KWObjectWithPropertiesForTest();

            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> { };

            Obj = (await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));
            Obj.Properties.Add(Obj.KWObject.DisplayName);
            List<long> propertiesID = Obj.Properties.Select(o => o.ID).ToList();
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                new List<KWObject>() { Obj.KWObject }, Obj.Properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            //Act
            var resultProperties = (await PropertyManager.GetPropertiesOfObjectAsync(Obj.KWObject)).ToList();

            // Assert
            foreach (var propertyID in propertiesID)
            {
                Assert.IsTrue(resultProperties.Select(o => o.ID).Contains(propertyID));
            }
        }

        [TestMethod]
        public async Task GetUnPublishedObjectWithoutProperties()
        {
            // Assign
            KWObjectWithPropertiesForTest Obj = new KWObjectWithPropertiesForTest();

            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> { };

            Obj = (await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));
            Obj.Properties.Add(Obj.KWObject.DisplayName);
            List<long> propertiesID = Obj.Properties.Select(o => o.ID).ToList();
            //Act
            var resultProperties = (await PropertyManager.GetPropertiesOfObjectAsync(Obj.KWObject)).ToList();

            // Assert
            foreach (var propertyID in propertiesID)
            {
                Assert.IsTrue(resultProperties.Select(o => o.ID).Contains(propertyID));
            }
        }
    }
}
