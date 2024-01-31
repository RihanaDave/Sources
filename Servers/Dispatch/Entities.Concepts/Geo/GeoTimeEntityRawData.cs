using Newtonsoft.Json;

namespace GPAS.Dispatch.Entities.Concepts.Geo
{
    public class GeoTimeEntityRawData
    {
        public string TimeBegin { set; get; }
        public string TimeEnd { set; get; }
        public string Latitude { set; get; }
        public string Longitude { set; get; }
    }
}
