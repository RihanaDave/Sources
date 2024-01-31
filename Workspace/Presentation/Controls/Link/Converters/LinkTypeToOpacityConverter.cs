using System;
using System.Globalization;
using System.Windows.Data;
using GPAS.Workspace.Presentation.Properties;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    /// <summary>
    /// در این مبدل تا زمانی که نوع لینک مورد نظر
    /// مشخص نشود شفافیت کنترل‌های دیگر کم می‌شود
    /// </summary>
    public class LinkTypeToOpacityConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.5;

            if (value.ToString().Equals(Resources.Select_A_Type) ||
                value.ToString().Equals(Resources.Not_Initialized) ||
                value.ToString().Equals(string.Empty))
            {
                return 0.5;
            }

            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
