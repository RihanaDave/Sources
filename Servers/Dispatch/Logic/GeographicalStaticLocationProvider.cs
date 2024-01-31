using GPAS.Dispatch.GeographicalStaticLocation;
using GPAS.Dispatch.Entities;

namespace GPAS.Dispatch.Logic
{
    public class GeographicalStaticLocationProvider
    {
     
        public static void Init()
        {
            GeographicalLocationAccess geographicalLocationAccess = new GeographicalLocationAccess();
            geographicalLocationAccess.CreateDatabase();
            geographicalLocationAccess.CreateTable(); 
        }

        public GeographicalLocationModel GetGeoLocationBaseOnIP(string ip)
        {
            GeographicalLocationAccess geographicalLocationAccess = new GeographicalLocationAccess();
            return geographicalLocationAccess.GetGeoLocationBaseOnIP(ip);
        }

        public bool InsertGeoSpecialInformationBasedOnIP(string ip, double latitude, double longitude)
        {
            GeographicalLocationAccess geographicalLocationAccess = new GeographicalLocationAccess();
            return geographicalLocationAccess.InsertGeoSpecialInformationBasedOnIP(ip, latitude, longitude);
        }
    }
}

