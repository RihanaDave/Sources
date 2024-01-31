using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.GeoSearch;
using GPAS.Ontology;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Geo;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    public class Geo
    {
        public async static Task<Dictionary<KWObject, HashSet<GeoLocationEntity>>> GetGeoDataForObjectAsync(IEnumerable<KWObject> objects)
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            var result = new Dictionary<KWObject, HashSet<GeoLocationEntity>>();

            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            string ontologyDateRangeAndLocationType = ontology.GetDateRangeAndLocationPropertyTypeUri();
            //string ontologyLocationType = ontology.GetLocationPropertyTypeUri();
            var geoTimePropertyTypes = new string[] { ontologyDateRangeAndLocationType };

            IEnumerable<GeoTimeKWProperty> geoTimePropertiesOfObjects
                = (await PropertyManager.GetPropertiesOfObjectsAsync(objects, geoTimePropertyTypes))
                    .Select(p => (GeoTimeKWProperty)p);

            if (geoTimePropertiesOfObjects.Count() != 0)
            {
                foreach (GeoTimeKWProperty geoTimeProp in geoTimePropertiesOfObjects)
                {
                    if (PropertiesValidation.ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoTime, geoTimeProp.Value,
                            out var geoTimePropEntityObject).Status == PropertiesValidation.ValidationStatus.Invalid)
                    {
                        continue;
                    }

                    GeoLocationEntity geoPropPosition = ((GeoTimeEntity)geoTimePropEntityObject).Location;
                    AddObjectPositionToDictionary(geoPropPosition, geoTimeProp.Owner, ref result);
                }
            }

            // گرفتن موقعیت جغرافیایی از طریق آی.پی
            IEnumerable<KWProperty> ipPropertiesOfObjects
                = await PropertyManager.GetPropertiesOfObjectsAsync
                    (objects, new string[] { ontology.GetDefaultIpPropertyTypeURI() });

            foreach (KWProperty ipProp in ipPropertiesOfObjects)
            {
                // موقعیت مرتبط با اولین آی.پی که دارای نگاشت باشد را برمی‌گرداند
                GeoLocationEntity ipRelatedPosition = (await GetGeoDateByIpAsync(ipProp.Value));
                AddObjectPositionToDictionary(ipRelatedPosition, ipProp.Owner, ref result);
            }

            IEnumerable<string> geoPointTypes = ontology.GetAllPropertiesOfBaseDataType(BaseDataTypes.GeoPoint);
            if (geoPointTypes != null)
            {
                IEnumerable<GeoPointKWProperty> geoPointPropertiesOfObjects
                    = (await PropertyManager.GetPropertiesOfObjectsAsync(objects, geoPointTypes)).Select(p => (GeoPointKWProperty)p);

                foreach (GeoPointKWProperty geoPointProp in geoPointPropertiesOfObjects)
                {
                    if (PropertiesValidation.ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoPoint, geoPointProp.Value,
                            out var geoPointPropEntityObject).Status == PropertiesValidation.ValidationStatus.Invalid)
                    {
                        continue;
                    }

                    AddObjectPositionToDictionary((GeoLocationEntity)geoPointPropEntityObject, geoPointProp.Owner, ref result);
                }
            }

            return result;
        }

        private static void AddObjectPositionToDictionary(GeoLocationEntity position, KWObject positionOwner, ref Dictionary<KWObject, HashSet<GeoLocationEntity>> targetDictionary)
        {
            if (!targetDictionary.ContainsKey(positionOwner))
            {
                targetDictionary.Add(positionOwner, new HashSet<GeoLocationEntity>());
            }
            if (!targetDictionary[positionOwner].Contains(position))
            {
                targetDictionary[positionOwner].Add(position);
            }
        }


        public static string[] GetMapTileSources()
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                return sc.GetMapTileSources();
            }
            finally
            {
                sc.Close();
            }
        }
        public static byte[] GetMapTileImage(string tileSource, int zoomLevel, MapTilePosition mapTilePosition)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            byte[] result;
            try
            {
                result = sc.GetMapTileImage(tileSource, zoomLevel, mapTilePosition.X, mapTilePosition.Y);
            }
            finally
            {
                sc.Close();
            }
            return result;
        }

        /// <summary>
        /// </summary>
        /// <returns>در صورتی که نگاشتی برای موقعیت آی.پی ثبت نشده باشد، Empty برگردانده می‌شود</returns>
        public async static Task<GeoLocationEntity> GetGeoDateByIpAsync(string ipAddress)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            GeographicalLocationModel location;
            try
            {
                location = await sc.GetGeoLocationBaseOnIPAsync(ipAddress);
            }
            finally
            {
                sc.Close();
            }

            GeoLocationEntity result;
            if (location == null)
                result = GeoLocationEntity.Empty;
            else
                result = new GeoLocationEntity()
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude
                };
            return result;
        }

        public async static Task<List<KWObject>> PerformGeoCircleSearchAsync(CircleSearchCriteria circleSearchCriteria, int maxResult = 1000)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            //List<long> geoSearchResultIDs = null;
            List<KObject> kObjects = null;
            try
            {
                kObjects = (await sc.PerformGeoCircleSearchAsync(circleSearchCriteria, maxResult)).ToList();
            }
            finally
            {
                sc.Close();
            }
            return await ObjectManager.GetObjectsById(kObjects);
        }

        public async static Task<List<KWObject>> PerformGeoPolygonSearchAsync(PolygonSearchCriteria polygonSearchCriteria, int maxResult = 1000)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            List<KObject> kObjects = null;
            try
            {
                polygonSearchCriteria.Vertices = RepairVerticesForSearchEngine(polygonSearchCriteria);
                kObjects = (await sc.PerformGeoPolygonSearchAsync(polygonSearchCriteria, maxResult)).ToList();
            }
            finally
            {
                sc.Close();
            }

            return await ObjectManager.GetObjectsById(kObjects);
        }

        private static List<GeoPoint> RepairVerticesForSearchEngine(PolygonSearchCriteria polygonSearchCriteria)
        {
            List<GeoPoint> points = new List<GeoPoint>();
            if (polygonSearchCriteria.Vertices != null && polygonSearchCriteria.Vertices.Count > 0)
            {
                for (int i = 0; i < polygonSearchCriteria.Vertices.Count; i++)
                {
                    GeoPoint firstPoint = firstPoint = polygonSearchCriteria.Vertices[i], lastPoint;
                    if (i == polygonSearchCriteria.Vertices.Count - 1)
                    {
                        lastPoint = polygonSearchCriteria.Vertices[0];
                    }
                    else
                    {
                        lastPoint = polygonSearchCriteria.Vertices[i + 1];
                    }

                    points.Add(firstPoint);

                    double diffLng = Math.Abs(firstPoint.Lng - lastPoint.Lng);
                    if (diffLng >= 180)
                    {
                        GeoPoint midPoint = new GeoPoint();
                        midPoint.Lng = (firstPoint.Lng + lastPoint.Lng) / 2;
                        midPoint.Lat = (firstPoint.Lat + lastPoint.Lat) / 2;

                        points.Add(midPoint);
                    }
                }

                return points;
            }
            else
            {
                return polygonSearchCriteria.Vertices;
            }
        }

        public async static Task<List<KWObject>> PerformGeoPolygonSearchAsync(List<PolygonSearchCriteria> polygonSearchCriterias, int maxResult = 1000)
        {
            List<KWObject> result = new List<KWObject>(maxResult);
            foreach (var criteria in polygonSearchCriterias)
            {
                result.AddRange(await PerformGeoPolygonSearchAsync(criteria, maxResult));
            }
            return result;
        }
    }
}
