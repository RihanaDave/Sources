using System.Globalization;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.ValidationRules
{
    public class LatitudeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return new ValidationResult(false, Properties.Resources.String_InvalidValue);

            if (double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out var longitude))
            {
                return longitude <= 90.0 && longitude >= -90.0 ?
                    new ValidationResult(true, null) :
                    new ValidationResult(false, Properties.Resources.String_InvalidValue);
            }

            return new ValidationResult(false, Properties.Resources.String_InvalidValue);
        }
    }
}
