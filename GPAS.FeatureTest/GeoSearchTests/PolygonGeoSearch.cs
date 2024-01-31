using GMap.NET;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.GeoSearch;
using GPAS.MapViewer;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.GeoSearchTests
{
    [TestClass]
    public class PolygonGeoSearch
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
                    Latitude = (45.008).ToString(),
                    Longitude = (90.009).ToString(),
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

            var points = new List<PointLatLng>()
            {
                new PointLatLng(35, 80),
                new PointLatLng(55, 90),
                new PointLatLng(45, 100)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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

            var properties3 = new Dictionary<string, List<string>>();

            var geoTimeProperties3 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (25).ToString(),
                    Longitude = (25).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3).Result);

            var points = new List<PointLatLng>()
            {
                new PointLatLng(50, 0),
                new PointLatLng(0, -50),
                new PointLatLng(-50, 0),
                new PointLatLng(0, 50)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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
        public async Task GetPublishedObjectOnCorners()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (2.56).ToString(),
                    Longitude = (-1.33).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
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

            var properties3 = new Dictionary<string, List<string>>();

            var geoTimeProperties3 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (25).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3).Result);

            var points = new List<PointLatLng>()
            {
                new PointLatLng(25, 0),
                new PointLatLng(0, -25),
                new PointLatLng(-25, 0),
                new PointLatLng(0, 25)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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
        public async Task GetPublishedObjectOutBound()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (15).ToString(),
                    Longitude = (17).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (80).ToString(),
                    Longitude = (-170).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);

            var points = new List<PointLatLng>()
            {
                new PointLatLng(40, 0),
                new PointLatLng(25, -85),
                new PointLatLng(-32.5, -60),
                new PointLatLng(-32.5, 60),
                new PointLatLng(25, 85)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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
        public async Task GetPublishedObjectWithBigArea()
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
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties1, geoTimeProperties1).Result);

            var properties2 = new Dictionary<string, List<string>>();

            var geoTimeProperties2 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (84).ToString(), //max passed = 80.15625, min passed = -80.15624 
                    Longitude = (160).ToString(), //its OK => 180
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties2, geoTimeProperties2).Result);
            
            var points = new List<PointLatLng>()
            {
                new PointLatLng(85, 165),
                new PointLatLng(85, -165),
                new PointLatLng(-85, -165),
                new PointLatLng(-85, 165)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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
                    Latitude = (49).ToString(),
                    Longitude = (99).ToString(),
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

            var points = new List<PointLatLng>()
            {
                new PointLatLng(55, 105),
                new PointLatLng(55, 95),
                new PointLatLng(45, 95),
                new PointLatLng(45, 105)
            };

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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
        public async Task GetPublishedObjectWithLotsOfVertex()
        {
            // Assign
            List<KWObjectWithPropertiesForTest> ObjList = new List<KWObjectWithPropertiesForTest>();
            var properties1 = new Dictionary<string, List<string>>();

            var geoTimeProperties1 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (2.56).ToString(),
                    Longitude = (-1.33).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                },
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

            var properties3 = new Dictionary<string, List<string>>();

            var geoTimeProperties3 = new List<GeoTimeEntityRawData> {
                new GeoTimeEntityRawData()
                {
                    Latitude = (0).ToString(),
                    Longitude = (25).ToString(),
                    TimeBegin = (DateTime.Now).ToString(),
                    TimeEnd = (DateTime.Now + new TimeSpan(5, 12, 50, 0, 0)).ToString()
                }
            };

            ObjList.Add(KWObjectWithPropertiesForTest.Create("شخص", $"Person {Guid.NewGuid().ToString()}", properties3, geoTimeProperties3).Result);

            var points = new List<PointLatLng>();
            int num = 1000;
            PointLatLng startPoint = new PointLatLng(-30, -30);
            PointLatLng endPoint = new PointLatLng(30, 30);
            double stepLat = (endPoint.Lat - startPoint.Lat) / (num / 4);
            double stepLng = (endPoint.Lng - startPoint.Lng) / (num / 4);
            for (int i = 0; i < 4; i++)
            {

                for (int j = 0; j < num/4; j++)
                {
                    PointLatLng point = new PointLatLng();
                    if (i == 0)
                    {
                        point.Lat = startPoint.Lat + (j * stepLat);
                        point.Lng = startPoint.Lng;
                    }
                    else if (i == 1)
                    {
                        point.Lat = endPoint.Lat;
                        point.Lng = startPoint.Lng + (j * stepLng);
                    }
                    else if (i == 2)
                    {
                        point.Lat = endPoint.Lat - (j * stepLat);
                        point.Lng = endPoint.Lng;
                    }
                    else if (i == 3)
                    {
                        point.Lat = startPoint.Lat;
                        point.Lng = endPoint.Lng - (j * stepLng);
                    }

                    points.Add(point);
                }
            }

            PolygonSearchCriteria polygonSearchCriteria = ConvertPointsToPolygonSearchCriteria(points);

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

            List<KWObject> geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
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

        private PolygonSearchCriteria ConvertPointsToPolygonSearchCriteria(List<PointLatLng> points)
        {
            var lines = new List<LineLatLng>(points.Capacity);
            for (int i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1)
                {
                    lines.Add(new LineLatLng(points[i], points[0]));
                }
                else
                {
                    lines.Add(new LineLatLng(points[i], points[i + 1]));
                }
            }

            var Vertices = ConvertPointToGeoPoint(points);

            Double perimeterInMeters = 0;
            foreach (var line in lines)
            {
                perimeterInMeters += line.LengthInMeter;
            }

            bool isAnyVectorCoincident = false;

            for (int i = 0; i < lines.Count; i++)
            {
                LineLatLng l1 = lines[i];
                for (int j = 0; j < lines.Count; j++)
                {
                    LineLatLng l2 = lines[j];
                    if (l1 != l2)
                    {
                        if (l1.CoincidentLine(l2) != null)
                        {
                            isAnyVectorCoincident = true;
                            break;
                        }
                    }
                }
            }

            bool isAnyVectorCrossed = false;

            for (int i = 0; i < lines.Count; i++)
            {
                LineLatLng l1 = lines[i];
                for (int j = 0; j < lines.Count; j++)
                {
                    LineLatLng l2 = lines[j];
                    if (l1 != l2)
                    {
                        if (!l1.CrossPoint(l2).IsEmpty)
                        {
                            isAnyVectorCrossed = true;
                            break;
                        }
                    }
                }
            }

            return new PolygonSearchCriteria()
            {
                Vertices = Vertices,
                perimeterInMeters = perimeterInMeters,
                isAnyVectorCoincident = isAnyVectorCoincident,
                isAnyVectorCrossed = isAnyVectorCrossed
            };
        }

        private List<GeoPoint> ConvertPointToGeoPoint(List<PointLatLng> points)
        {
            List<GeoPoint> result = new List<GeoPoint>();
            foreach (var currentPoint in points)
            {
                result.Add(new GeoPoint()
                {
                    Lat = currentPoint.Lat,
                    Lng = currentPoint.Lng
                });
            }
            return result;
        }
    }
}
