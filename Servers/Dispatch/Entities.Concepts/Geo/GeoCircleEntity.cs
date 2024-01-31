using System;
using System.Globalization;

namespace GPAS.Dispatch.Entities.Concepts.Geo
{
    public class GeoCircleEntity
    {
        public GeoLocationEntity Center { get; set; }
        public double Radius { set; get; }

        public static readonly GeoCircleEntity Empty = new GeoCircleEntity()
        {
            Center = new GeoLocationEntity(),
            Radius = double.NaN,
        };

        public override bool Equals(object obj)
        {
            return obj is GeoCircleEntity geoCircle &&
                Center.Equals(geoCircle.Center) && Radius.Equals(geoCircle.Radius);
        }
        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }
    }
}
