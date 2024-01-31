using GPAS.Workspace.Presentation.Windows.DataImport;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public class DateTimeValidationModel
    {
        public string StringFormat { get; set; }
        public long TimeZoneTimestamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CalendarType CalenderType { get; set; }
        public string DateTimeValue { get; set; }
    }
}
