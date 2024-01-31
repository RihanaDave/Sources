using GPAS.Dispatch.Entities.Concepts.Geo;
using Newtonsoft.Json;

namespace GPAS.PropertiesValidation.Geo
{
    public class GeoCircle
    {
        public static string GetGeoCircleStringValue(GeoCircleEntityRawData geoCircleEntityRawData)
        {
            return JsonConvert.SerializeObject(geoCircleEntityRawData);
        }
        public static GeoCircleEntityRawData GeoCircleEntityRawData(string geoCircleStringValue)
        {
            return JsonConvert.DeserializeObject<GeoCircleEntityRawData>(geoCircleStringValue);
        }
    }
}
