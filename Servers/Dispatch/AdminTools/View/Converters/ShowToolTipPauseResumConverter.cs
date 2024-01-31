using GPAS.Dispatch.Entities.Jobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class ShowToolTipPauseResumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentOutOfRangeException();

            if ((JobRequestStatus)value == JobRequestStatus.Busy ||
                (JobRequestStatus)value == JobRequestStatus.Timeout)
            {
                return "Puase Button";
            }
            else if ((JobRequestStatus)value == JobRequestStatus.Pause)
            {
                return "Resum Button";
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}