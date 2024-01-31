using GPAS.Dispatch.Entities.Jobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class JobPauseOrResumeColorIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentOutOfRangeException();

            if ((JobRequestStatus)value == JobRequestStatus.Busy ||
                (JobRequestStatus)value == JobRequestStatus.Timeout)
            {
                return Brushes.Red;
            }
            else if ((JobRequestStatus)value == JobRequestStatus.Pause)
            {
                return  Brushes.Green;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}