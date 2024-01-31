using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Validation
{
    public class DatePickerValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string dateString = System.Convert.ToString(value);
            DateTime resultDateTime;
            if (DateTime.TryParse(dateString, out resultDateTime))
            {
                return new ValidationResult(true, null);
            }
            return new ValidationResult(false, "Please Enter Correct Format");
        }
    }
}
