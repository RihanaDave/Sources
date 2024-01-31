using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TimelineViewer.Converter
{
    internal class BinScaleLevelToStringResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is BinScaleLevel)
            {
                BinScaleLevel binScaleLevel = (BinScaleLevel)value;
                switch (binScaleLevel)
                {
                    case BinScaleLevel.Year:
                        return Properties.Resources.Year;
                    case BinScaleLevel.Month:
                        return Properties.Resources.Month;
                    case BinScaleLevel.Day:
                        return Properties.Resources.Day;
                    case BinScaleLevel.Hour:
                        return Properties.Resources.Hour;
                    case BinScaleLevel.Minute:
                        return Properties.Resources.Minute;
                    case BinScaleLevel.Second:
                        return Properties.Resources.Second;
                    case BinScaleLevel.Millisecond:
                        return Properties.Resources.Millisecond;
                    case BinScaleLevel.Unknown:
                    default:
                        return value.ToString();
                }
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
