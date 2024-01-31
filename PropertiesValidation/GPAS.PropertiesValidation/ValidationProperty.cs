using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GPAS.PropertiesValidation
{
    public class ValidationProperty
    {
        public string Message { get; set; }

        private ValidationStatus status;
        [JsonConverter(typeof(StringEnumConverter))]
        public ValidationStatus Status 
        {
            get => status;
            set
            {
                status = value;
                Message = status.ToString();
            }
        }
    }
}
