using System.ComponentModel;

namespace GPAS.PropertiesValidation.Geo.Formats
{
    public enum GeoSpecialTypes
    {
        [Description("Degrees Minutes Seconds (DMS)")]
        DMS,
        [Description("Decimal Format")]
        Decimal,
        [Description("Compound DMS")]
        CompoundDMS,
        [Description("Compound Decimal")]
        CompoundDecimal,
    };
}