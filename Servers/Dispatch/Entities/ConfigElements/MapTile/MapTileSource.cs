using System.Configuration;

namespace GPAS.Dispatch.Entities.ConfigElements.MapTile
{
    public class MapTileSource : ConfigurationElement
    {
        [ConfigurationProperty("UniqueName", DefaultValue = "_", IsRequired = true, IsKey = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string UniqueName
        {
            get
            {
                return (string)this["UniqueName"];
            }
            set
            {
                this["UniqueName"] = value;
            }
        }

        [ConfigurationProperty("SourceType", DefaultValue = "LocallyStored", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 30)]
        public string SourceType
        {
            get
            {
                return (string)this["SourceType"];
            }
            set
            {
                this["SourceType"] = value;
            }
        }

        static readonly GenericEnumConverter mapTileSourceTypeConverter = new GenericEnumConverter(typeof(MapTileSourceType));

        public MapTileSourceType Type
        {
            get
            {
                return (MapTileSourceType)mapTileSourceTypeConverter.ConvertFromString(SourceType);
            }
        }

        [ConfigurationProperty("AccessPattern", DefaultValue = @".\MapImages\{z}\x{x}y{y}.png", IsRequired = true)]
        [StringValidator(MinLength = 1, MaxLength = 1000)]
        public string AccessPattern
        {
            get
            {
                return (string)this["AccessPattern"];
            }
            set
            {
                this["AccessPattern"] = value;
            }
        }
    }
}