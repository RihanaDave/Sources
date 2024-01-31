using GPAS.Dispatch.Entities.Jobs;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class JobStateIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentOutOfRangeException();

            switch ((JobRequestStatus)value)
            {
                case JobRequestStatus.Busy:
                    return new SolidColorBrush(Colors.Black);
                case JobRequestStatus.Pause:
                    return new SolidColorBrush(Colors.DimGray);
                case JobRequestStatus.Resume:
                    return new SolidColorBrush(Colors.DimGray);
                case JobRequestStatus.Failed:
                    return new SolidColorBrush(Colors.Red);
                case JobRequestStatus.Pending:
                    return new SolidColorBrush(Colors.DodgerBlue);
                case JobRequestStatus.Success:
                    return new SolidColorBrush(Colors.Green);
                case JobRequestStatus.Terminated:
                    return new SolidColorBrush(Colors.DarkOrange);
                case JobRequestStatus.Timeout:
                    return new SolidColorBrush(Colors.DimGray);
            }

            throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
