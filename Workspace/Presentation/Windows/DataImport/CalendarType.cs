using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    public enum CalendarType
    {
        [Description("Gregorian")]
        Gregorian,

        [Description("Jalali")]
        Jalali,

        [Description("Hijri")]
        Hijri
    }
}
