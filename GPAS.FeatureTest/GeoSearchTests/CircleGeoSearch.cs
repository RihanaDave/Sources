using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.GeoSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoPoint = GPAS.GeoSearch.GeoPoint;

namespace GPAS.FeatureTest.GeoSearchTests
{
    [TestClass]
    public class CircleGeoSearch
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
        public async Task GetPublishedObjectInBound()
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

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

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

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

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

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3).Result);
            
            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 0,
                    Lng = 0
                },
                RediusInKiloMeters = 15
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> Result = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        Result.Add(gsr);
                }
            }

            // Assert
            Assert.AreEqual(2, Result.Count);
        }

        [TestMethod]
        public async Task GetPublishedObjectOnStroke()
        {
            //Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (30).ToString(),
                    Longitude = (-30).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (0.00898315284119522).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()//0.00898315284119522
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);
            
            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (45).ToString(),
                    Longitude = (90).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 0,
                    Lng = 0
                },
                RediusInKiloMeters = 1
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> Result = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        Result.Add(gsr);
                }
            }

            // Assert
            Assert.AreEqual(1, Result.Count);
        }

        [TestMethod]
        public async Task GetPublishedObjectOutBound()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (45.1).ToString(),
                    Longitude = (89.9).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (30).ToString(),
                    Longitude = (-60).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (-45).ToString(),
                    Longitude = (-90).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 45,
                    Lng = 90
                },
                RediusInKiloMeters = 100
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> ResultInBound = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        ResultInBound.Add(gsr);
                }
            }

            List<KWObject> ResultOutBound = kwObjects.Except(ResultInBound).ToList();

            // Assert
            Assert.AreEqual(1, ResultInBound.Count);
            Assert.AreEqual(1, ResultOutBound.Count);
        }

        [TestMethod]
        public async Task GetPublishedObjectWithMultiGeoTimeProperty()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (45.1).ToString(),
                    Longitude = (89.9).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (30.2).ToString(),
                    Longitude = (-60.3).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 30,
                    Lng = -60
                },
                RediusInKiloMeters = 50
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );


            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> Result = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        Result.Add(gsr);
                }
            }

            // Assert
            Assert.AreEqual(1, Result.Count);
        }

        [TestMethod]
        public async Task GetPublishedObjectWithGeoTimePropertyLngOver90()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (0).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (45.1).ToString(),
                    Longitude = (90.1).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
                new GeoTimeEntityRawData()
                {
                    Latitude = (30).ToString(),
                    Longitude = (-60).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (-45.1).ToString(),
                    Longitude = (-90.00001).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 45,
                    Lng = 90
                },
                RediusInKiloMeters = 500
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> ResultInBound = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        ResultInBound.Add(gsr);
                }
            }

            // Assert
            Assert.AreEqual(1, ResultInBound.Count);
        }

        [TestMethod]
        public async Task GetPublishedObjectWithLargeRadius()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (-5).ToString(),
                    Longitude = (10).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (-45).ToString(),
                    Longitude = (89).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = 5,
                    Lng = -10
                },
                RediusInKiloMeters = 10000
            };

            List<KWObject> kwObjects = ObjList.Select(o => o.KWObject).ToList();
            List<KWProperty> properties = new List<KWProperty>();
            foreach (KWObjectWithPropertiesForTest o in ObjList)
            {
                properties.AddRange(o.Properties);
            }

            //Act
            await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(
                kwObjects, properties,
                new List<KWMedia>(), new List<KWRelationship>(), new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>()
            );

            List<KWObject> geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);
            List<KWObject> Result = new List<KWObject>(geoSearchResults.Capacity);

            foreach (KWObject gsr in geoSearchResults)
            {
                foreach (KWObject o in kwObjects)
                {
                    if (gsr.ID == o.ID)
                        Result.Add(gsr);
                }
            }

            // Assert
            Assert.AreEqual(1, Result.Count);
        }
    }
}
