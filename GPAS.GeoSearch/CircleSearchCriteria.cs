using System.Drawing;
using System.Runtime.Serialization;

namespace GPAS.GeoSearch
{
    [DataContract]
    public class CircleSearchCriteria 
    {
        [DataMember]
        public GeoPoint Center { set; get; }

        [DataMember]
        public double RediusInKiloMeters { set; get; }
    }
}