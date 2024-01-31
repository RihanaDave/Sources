using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Applications.Validations
{
   public class RequiredFieldValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        { 
            return !string.IsNullOrEmpty(value?.ToString()) ?
                new ValidationResult(true, null) : new ValidationResult(false, "This field is required");
        }
    }
}
