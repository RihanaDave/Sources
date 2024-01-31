using GPAS.Dispatch.Entities.Concepts.Geo;

namespace GPAS.Workspace.Entities
{
    public class GeoPointKWProperty : KWProperty
    {
        public GeoLocationEntityRawData GeoLocationValue
        {
            get;
            set;
        }
    }
}
