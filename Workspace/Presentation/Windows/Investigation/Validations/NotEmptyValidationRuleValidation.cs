using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Windows.Investigation.Validations
{
    public class NotEmptyValidationRuleValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
             ? new ValidationResult(false, "Field is required.")
             : ValidationResult.ValidResult;
        }
    }
}
