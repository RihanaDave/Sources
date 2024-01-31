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
    public class RetrievePropertyOfMultiObjects
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
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();
            properties1.Add("سن", new List<string> { "25" });

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
            };

            ObjList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1));

            var properties2 = new Dictionary<string, List<string>>();
            properties2.Add("سن", new List<string> { "17", "35" });
            properties2.Add("قد", new List<string> { "180" });

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (45).ToString(),
                    Longitude = (90).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2));

            var properties3 = new Dictionary<string, List<string>>();
            properties3.Add("سن", new List<string> { "15" });

            var geoTimeProperties3 = new List<GeoTimeEntityRawData> {
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

            ObjList.Add(await KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3));

            List<KWObject> objects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
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
            var publishResult = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                objects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );
            Assert.IsTrue(publishResult.HorizonServerSynchronized);
            Assert.IsTrue(publishResult.SearchServerSynchronized);

            List<long> propertiesID = properties.Select(o => o.ID).ToList();
            Workspace.Logic.Publish.PublishManager.DiscardAllChanges();
            //Act
            List<KWProperty> resultProperties = (await PropertyManager.GetPropertiesOfObjectsAsync(
                objects,
                new string[] {
                    OntologyProvider.GetOntology().GetDefaultDisplayNamePropertyTypeUri(),
                    OntologyProvider.GetOntology().GetDateRangeAndLocationPropertyTypeUri(),
                    "سن",
                    "قد"
                }
            )).ToList();

            // Assert
            foreach (var propertyID in propertiesID)
            {
                Assert.IsTrue(resultProperties.Select(o => o.ID).Contains(propertyID));
            }

        }
    }
}
