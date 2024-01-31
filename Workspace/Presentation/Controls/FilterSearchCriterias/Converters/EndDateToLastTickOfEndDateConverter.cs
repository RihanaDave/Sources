using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class EndDateToLastTickOfEndDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime?)
            {
                return ConvertDateToLastTickOfDate(value as DateTime?);
            }

            return value;
        }

        private DateTime? ConvertDateToLastTickOfDate(DateTime? endDate)
        {
            return new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day).AddDays(1).AddTicks(-1);
        }
    }
}
