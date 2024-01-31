namespace GPAS.Dispatch.Entities.Concepts.Geo
{
    public class GeoLocationEntity
    {
        public double Latitude { set; get; }

        public double Longitude { set; get; }


        public static readonly GeoLocationEntity Empty = new GeoLocationEntity()
        {
            Latitude = double.NaN,
            Longitude = double.NaN
        };

        public override bool Equals(object obj)
        {
            if (!(obj is GeoLocationEntity))
                return false;
            if (Latitude.Equals(((GeoLocationEntity)obj).Latitude)
                && Longitude.Equals(((GeoLocationEntity)obj).Longitude))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Latitude:{0}, Longitude{1}", Latitude, Longitude);
        }
    }
}