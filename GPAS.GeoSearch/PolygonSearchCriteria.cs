using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace GPAS.GeoSearch
{
    [DataContract]
    public class PolygonSearchCriteria 
    {
        [DataMember]
        public List<GeoPoint> Vertices { set; get; }
        [DataMember]
        public double LengthInMeters = double.NaN;
        [DataMember]
        public double WidthInMeters = double.NaN;
        [DataMember]
        public double perimeterInMeters = double.NaN;
        [DataMember]
        public bool isAnyVectorCrossed = false;
        [DataMember]
        public bool isAnyVectorCoincident = false;
    }
    [DataContract]
    public class GeoPoint
    {
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double Lng { get; set; }

        public GeoPoint(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
        public GeoPoint()
        {
            Lat = 0;
            Lng = 0;
        }

    }
}