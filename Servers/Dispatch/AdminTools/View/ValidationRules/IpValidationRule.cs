using System.Globalization;
using System.Net;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.ValidationRules
{
    public class IpValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
                return new ValidationResult(false, Properties.Resources.String_InvalidIpAddress);

            string[] splitValues = value.ToString().Split('.');

            bool result = IPAddress.TryParse(value.ToString(), out _) && splitValues.Length == 4;

            return result ? new ValidationResult(true, null) :
                new ValidationResult(false, Properties.Resources.String_InvalidIpAddress);
        }
    }
}
