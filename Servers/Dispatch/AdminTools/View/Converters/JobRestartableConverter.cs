using GPAS.Dispatch.Entities.Jobs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class JobRestartableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentOutOfRangeException();

            if ((JobRequestStatus)value == JobRequestStatus.Terminated ||
                (JobRequestStatus)value == JobRequestStatus.Timeout||
                (JobRequestStatus)value == JobRequestStatus.Failed)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
          
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
