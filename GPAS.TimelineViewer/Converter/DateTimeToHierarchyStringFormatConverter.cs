using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TimelineViewer.Converter
{
    internal class DateTimeToHierarchyStringFormatConverter : IValueConverter
    {
        public BinSize BinSize { get; set; } = BinSizes.Default;

        private DateTime? previousDateTime { get; set; } = null;
        private long counter = 0;
        int labelStep = 6;

        public void Reset(BinSize binSize)
        {
            BinSize = binSize;
            previousDateTime = null;
            counter = 0;
            labelStep = 6;
            switch (BinSize.TimeAxisLabelsScale)
            {
                case BinScaleLevel.Year:
                    break;
                case BinScaleLevel.Month:
                    break;
                case BinScaleLevel.Day:
                    break;
                case BinScaleLevel.Hour:
                    break;
                case BinScaleLevel.Minute:
                    break;
                case BinScaleLevel.Second:
                    break;
                case BinScaleLevel.Millisecond:
                    break;
                case BinScaleLevel.Unknown:
                    break;
                default:
                    break;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            counter++;
            DateTime dateTime = DateTime.Parse(value.ToString());
            string format = string.Empty;

            if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Year)
            {
                format = "{0:yyyy}";
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Month)
            {
                if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Year))
                {
                    format = "{0:yyyy}";
                }
                else
                {
                    format = "{0:MMM}";
                }
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Day)
            {
                if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Year))
                {
                    format = "{0:yyyy}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Month))
                {
                    format = "{0:MMM}";
                }
                else
                {
                    format = "{0:dd}";
                }
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Hour)
            {
                if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Year))
                {
                    format = "{0:yyyy}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Month))
                {
                    format = "{0:MMM}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Day))
                {
                    format = "{0:dd MMM}";
                }
                else
                {
                    format = "{0:HH}";
                }
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Minute)
            {
                if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Year))
                {
                    format = "{0:yyyy}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Month))
                {
                    format = "{0:MMM}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Day))
                {
                    format = "{0:dd MMM}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Hour))
                {
                    format = "{0:HH}";
                }
                else
                {
                    format = "{0:mm}\'";
                }
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Second)
            {
                if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Year))
                {
                    format = "{0:yyyy}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Month))
                {
                    format = "{0:MMM}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Day))
                {
                    format = "{0:dd MMM}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Hour))
                {
                    format = "{0:HH}";
                }
                else if (IsFirstLabelOfScale(dateTime, BinScaleLevel.Minute))
                {
                    format = "{0:mm}\'";
                }
                else
                {
                    format = "{0:ss}\"";
                }
            }

            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }
            else
            {
                string result = string.Format(format, dateTime);
                if (!IsChangedValueOfScale(dateTime) || counter < labelStep)
                {
                    return string.Empty;
                }
                else
                {
                    counter = 0;
                    previousDateTime = dateTime;
                    return result;
                }
            }
        }

        private bool IsChangedValueOfScale(DateTime dateTime)
        {
            if (!previousDateTime.HasValue)
                return true;

            if(BinSize.TimeAxisLabelsScale == BinScaleLevel.Year)
            {
                return previousDateTime.Value.Year != dateTime.Year;
            }
            else if(BinSize.TimeAxisLabelsScale == BinScaleLevel.Month)
            {
                return previousDateTime.Value.Year != dateTime.Year
                       || previousDateTime.Value.Month != dateTime.Month;
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Day)
            {
                return previousDateTime.Value.Year != dateTime.Year
                    || previousDateTime.Value.Month != dateTime.Month
                    || previousDateTime.Value.Day != dateTime.Day;

            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Hour)
            {
                return previousDateTime.Value.Year != dateTime.Year
                    || previousDateTime.Value.Month != dateTime.Month
                    || previousDateTime.Value.Day != dateTime.Day
                    || previousDateTime.Value.Hour != dateTime.Hour;
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Minute)
            {
                return previousDateTime.Value.Year != dateTime.Year
                    || previousDateTime.Value.Month != dateTime.Month
                    || previousDateTime.Value.Day != dateTime.Day
                    || previousDateTime.Value.Hour != dateTime.Hour
                    || previousDateTime.Value.Minute != dateTime.Minute;
            }
            else if (BinSize.TimeAxisLabelsScale == BinScaleLevel.Second)
            {
                return previousDateTime.Value.Year != dateTime.Year
                    || previousDateTime.Value.Month != dateTime.Month
                    || previousDateTime.Value.Day != dateTime.Day
                    || previousDateTime.Value.Hour != dateTime.Hour
                    || previousDateTime.Value.Minute != dateTime.Minute
                    || previousDateTime.Value.Second != dateTime.Second;
            }

            return false;
        }

        private bool IsFirstLabelOfScale(DateTime dateTime, BinScaleLevel binScaleLevel)
        {
            try
            {
                if (!previousDateTime.HasValue)
                    return false;
                if (binScaleLevel == BinScaleLevel.Year)
                {
                    return previousDateTime.Value.Year != dateTime.Year;
                }
                else if (binScaleLevel == BinScaleLevel.Month)
                {
                    return previousDateTime.Value.Year != dateTime.Year
                        || previousDateTime.Value.Month != dateTime.Month;

                }
                else if (binScaleLevel == BinScaleLevel.Day)
                {
                    return previousDateTime.Value.Year != dateTime.Year
                        || previousDateTime.Value.Month != dateTime.Month
                        || previousDateTime.Value.Day != dateTime.Day;

                }
                else if (binScaleLevel == BinScaleLevel.Hour)
                {
                    return previousDateTime.Value.Year != dateTime.Year
                        || previousDateTime.Value.Month != dateTime.Month
                        || previousDateTime.Value.Day != dateTime.Day
                        || previousDateTime.Value.Hour != dateTime.Hour;
                }
                else if (binScaleLevel == BinScaleLevel.Minute)
                {
                    return previousDateTime.Value.Year != dateTime.Year
                        || previousDateTime.Value.Month != dateTime.Month
                        || previousDateTime.Value.Day != dateTime.Day
                        || previousDateTime.Value.Hour != dateTime.Hour
                        || previousDateTime.Value.Minute != dateTime.Minute;
                }
                else if (binScaleLevel == BinScaleLevel.Second)
                {
                    return previousDateTime.Value.Year != dateTime.Year
                        || previousDateTime.Value.Month != dateTime.Month
                        || previousDateTime.Value.Day != dateTime.Day
                        || previousDateTime.Value.Hour != dateTime.Hour
                        || previousDateTime.Value.Minute != dateTime.Minute
                        || previousDateTime.Value.Second != dateTime.Second;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}