using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{    public enum GeoTimeFormat
    {
        [Description("Degrees Minutes Seconds (DMS)")]
        DMS,
        [Description("Decimal Format")]
        Decimal,
        [Description("Compound DMS")]
        CompoundDMS,
        [Description("Compound Decimal")]
        CompoundDecimal,       
    }
}