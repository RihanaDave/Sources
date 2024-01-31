using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Validation
{
    public class FileSizeValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty(Convert.ToString(value)))
            {
                if (Convert.ToInt32(value) < 0)
                {
                    return new ValidationResult(false, "Enter Valid FileSize");
                }
            }
            return new ValidationResult(true, null);
        }
    }
}
