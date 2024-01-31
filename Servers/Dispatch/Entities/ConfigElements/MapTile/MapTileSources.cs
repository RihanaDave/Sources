using System.Configuration;

namespace GPAS.Dispatch.Entities.ConfigElements.MapTile
{
    public class MapTileSources : ConfigurationSection
    {
        [ConfigurationProperty("TileSources")]
        [ConfigurationCollection(typeof(MapTileSourceCollection))]
        public MapTileSourceCollection Sources
        {
            get
            {
                return (MapTileSourceCollection)this["TileSources"];
            }
            set
            {
                this["TileSources"] = value;
            }
        }
    }
}