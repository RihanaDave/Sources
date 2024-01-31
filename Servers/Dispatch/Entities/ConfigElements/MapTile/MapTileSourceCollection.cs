using System.Configuration;

namespace GPAS.Dispatch.Entities.ConfigElements.MapTile
{
    public class MapTileSourceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MapTileSource();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MapTileSource)element).UniqueName;
        }
    }
}