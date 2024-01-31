using GPAS.Dispatch.Entities.Jobs;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class JobStateIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentOutOfRangeException();

            switch ((JobRequestStatus)value)
            {
                case JobRequestStatus.Busy:
                    return MaterialDesignThemes.Wpf.PackIconKind.PlayArrow;
                case JobRequestStatus.Failed:
                    return MaterialDesignThemes.Wpf.PackIconKind.AlertOctagon;
                case JobRequestStatus.Pending:
                    return MaterialDesignThemes.Wpf.PackIconKind.TimerSand;
                case JobRequestStatus.Success:
                    return MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
                case JobRequestStatus.Terminated:
                    return MaterialDesignThemes.Wpf.PackIconKind.HandLeft;
                case JobRequestStatus.Timeout:
                    return MaterialDesignThemes.Wpf.PackIconKind.TimerOff;
                case JobRequestStatus.Pause:
                    return MaterialDesignThemes.Wpf.PackIconKind.Pause;
                case JobRequestStatus.Resume:
                    return MaterialDesignThemes.Wpf.PackIconKind.PlayArrow;
            }

            throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
