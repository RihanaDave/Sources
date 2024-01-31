using System.Globalization;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.ValidationRules
{
    /// <summary>
    /// اعتبار سنجی مقادیر ضرورری
    /// </summary>
    public class RequiredFieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return !string.IsNullOrEmpty(value?.ToString()) ?
                new ValidationResult(true, null) : new ValidationResult(false, "This field is required");
        }
    }
}
