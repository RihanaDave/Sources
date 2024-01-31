using System;

namespace GPAS.TimelineViewer
{
    public class Utility 
    {
        public static readonly DateTime MinValue = DateTime.MinValue;
        public static readonly DateTime MaxValue = new DateTime(9000, 1, 1).AddTicks(-1);

        public static bool DateTimeTryParse(ref DateTime dateTime)
        {
            if (dateTime > MaxValue)
            {
                dateTime = MaxValue;
                return false;
            }

            if (dateTime < MinValue)
            {
                dateTime = MinValue;
                return false;
            }

            return true;
        }

        public static bool DateTimeTryParse(ref DateTime? dateTime)
        {
            if (dateTime > MaxValue)
            {
                dateTime = MaxValue;
                return false;
            }

            if (dateTime < MinValue)
            {
                dateTime = MinValue;
                return false;
            }

            return true;
        }

        public static bool DateTimeAddTryParse(DateTime dateTime, TimeSpan value, out DateTime result)
        {
            try
            {
                result = dateTime.Add(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value.Ticks < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static bool DateTimeAddTryParse(DateTime dateTime, TimeSpan value, out DateTime? result)
        {
            try
            {
                result = dateTime.Add(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value.Ticks < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static bool DateTimeAddMonthsTryParse(DateTime dateTime, int value, out DateTime result)
        {
            try
            {
                result = dateTime.AddMonths(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static bool DateTimeAddMonthsTryParse(DateTime dateTime, int value, out DateTime? result)
        {
            try
            {
                result = dateTime.AddMonths(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static bool DateTimeAddYearsTryParse(DateTime dateTime, int value, out DateTime result)
        {
            try
            {
                result = dateTime.AddYears(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static bool DateTimeAddYearsTryParse(DateTime dateTime, int value, out DateTime? result)
        {
            try
            {
                result = dateTime.AddYears(value);
                return DateTimeTryParse(ref result);
            }
            catch
            {
                result = value < 0 ? MinValue : MaxValue;
                return false;
            }
        }

        public static DateTime CenterRange(DateTime from, DateTime to)
        {
            return new DateTime((long)((from.Ticks + to.Ticks) / 2));
        }

        public static DateTime Ceiling(DateTime dateTime, BinSize binSize)
        {
            DateTimeAddTryParse(dateTime, binSize.GetDuration(dateTime), out dateTime);
            if (dateTime < MaxValue)
                dateTime = Floor(dateTime, binSize);

            return dateTime;
        }

        public static DateTime Floor(DateTime dateTime, BinSize binSize)
        {
            switch (binSize.BinScale)
            {
                case BinScaleLevel.Year:
                    int remindYear = (int)((dateTime.Year - 1) % binSize.BinFactor);
                    if (dateTime.Year == remindYear)
                        return Utility.MinValue;

                    DateTime dtYear = new DateTime();
                    Utility.DateTimeAddYearsTryParse(dateTime, -remindYear, out dtYear);
                    return new DateTime(dtYear.Year, 1, 1, 0, 0, 0, 0);
                case BinScaleLevel.Month:
                    int totalMonths = ((dateTime.Year - 1) * 12) + dateTime.Month - 1;
                    int remindMonths = (int)(totalMonths % binSize.BinFactor);
                    if (totalMonths == remindMonths)
                        return Utility.MinValue;

                    DateTime dtMonth = new DateTime();
                    Utility.DateTimeAddMonthsTryParse(dateTime, -remindMonths, out dtMonth);
                    return new DateTime(dtMonth.Year, dtMonth.Month, 1, 0, 0, 0, 0);
                case BinScaleLevel.Day:
                case BinScaleLevel.Hour:
                case BinScaleLevel.Minute:
                case BinScaleLevel.Second:
                case BinScaleLevel.Millisecond:
                    DateTime result = new DateTime();
                    TimeSpan duration = binSize.GetDuration(dateTime);
                    var extra = new TimeSpan(-(dateTime.Ticks % duration.Ticks));
                    Utility.DateTimeAddTryParse(dateTime, extra, out result);
                    return result;
                case BinScaleLevel.Unknown:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
