using System;

namespace GPAS.TimelineViewer
{
#pragma warning disable CS0659 // 'BinSize' overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class BinSize
#pragma warning restore CS0659 // 'BinSize' overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public double BinFactor { get; set; }
        public BinScaleLevel BinScale { get; set; }
        public BinScaleLevel TimeAxisLabelsScale { get; set; }

        /// <summary>
        /// به اندازه BinSize به تاریخ ورودی اضافه کرده و تاریخ حاصل شده را از تاریخ ورودی کم می کند.
        /// </summary>
        /// <param name="beginTime"></param>
        /// <returns></returns>
        public TimeSpan GetDuration(DateTime beginTime)
        {
            return ProcessTimeDuration(beginTime);
        }
        public BinSizesEnum RelatedBinSizesEnum { get; set; } = BinSizesEnum.Custom;

        private const double daysOfYear = 365.2425;
        private const double daysOfMonth = daysOfYear / 12;
        private const long TicksPerMonth = (long)(daysOfMonth * TimeSpan.TicksPerDay);
        private const long TicksPerYear = (long)(daysOfYear * TimeSpan.TicksPerDay);

        private TimeSpan ProcessTimeDuration(DateTime beginTime)
        {
            DateTime ub = new DateTime();
            switch (BinScale)
            {
                case BinScaleLevel.Second:
                    return new TimeSpan((long)(TimeSpan.TicksPerSecond * BinFactor));
                case BinScaleLevel.Minute:
                    return new TimeSpan((long)(TimeSpan.TicksPerMinute * BinFactor));
                case BinScaleLevel.Hour:
                    return new TimeSpan((long)(TimeSpan.TicksPerHour * BinFactor));
                case BinScaleLevel.Day:
                    return new TimeSpan((long)(TimeSpan.TicksPerDay * BinFactor));
                case BinScaleLevel.Month:
                    Utility.DateTimeAddMonthsTryParse(Utility.MaxValue, (int)-BinFactor, out ub);
                    if (ub < beginTime) //برای جلوگیری از بالارفتن از  MathDateTime.MaxValue
                        return Utility.MaxValue - beginTime;

                    DateTime dateAddedMonth = new DateTime();
                    Utility.DateTimeAddMonthsTryParse(beginTime, (int)BinFactor, out dateAddedMonth);
                    return dateAddedMonth - beginTime;
                case BinScaleLevel.Year:
                    Utility.DateTimeAddYearsTryParse(Utility.MaxValue, (int)-BinFactor, out ub);
                    if (ub < beginTime) //برای جلوگیری از بالارفتن از  MathDateTime.MaxValue
                        return Utility.MaxValue - beginTime;
                    DateTime dateAddedYear = new DateTime();
                    Utility.DateTimeAddYearsTryParse(beginTime, (int)BinFactor, out dateAddedYear);
                    return dateAddedYear - beginTime;
                case BinScaleLevel.Unknown:
                default:
                    return TimeSpan.Zero;
            }
        }

        public static BinSize GenerateCustomBinSize(DateTime beginTime, DateTime endTime, int barCount)
        {
            BinSize binSize = new BinSize();
            TimeSpan allDuration = endTime - beginTime;
            TimeSpan barDuration = new TimeSpan((long)((double)allDuration.Ticks / barCount));

            if (barDuration.Ticks < TimeSpan.TicksPerMinute)
            {
                binSize.BinScale = BinScaleLevel.Second;
                binSize.BinFactor = (double)barDuration.Ticks / TimeSpan.TicksPerSecond;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Minute;
            }
            else if (barDuration.Ticks < TimeSpan.TicksPerHour)
            {
                binSize.BinScale = BinScaleLevel.Minute;
                binSize.BinFactor = (double)barDuration.Ticks / TimeSpan.TicksPerMinute;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Hour;
            }
            else if (barDuration.Ticks < TimeSpan.TicksPerDay)
            {
                binSize.BinScale = BinScaleLevel.Hour;
                binSize.BinFactor = (double)barDuration.Ticks / TimeSpan.TicksPerHour;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Day;
            }
            else if (barDuration.Ticks < TicksPerMonth)
            {
                binSize.BinScale = BinScaleLevel.Day;
                binSize.BinFactor = (double)barDuration.Ticks / TimeSpan.TicksPerDay;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Month;
            }
            else if (barDuration.Ticks < TicksPerYear)
            {
                binSize.BinScale = BinScaleLevel.Month;
                binSize.BinFactor = (double)barDuration.Ticks / TicksPerMonth;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Year;
            }
            else
            {
                binSize.BinScale = BinScaleLevel.Year;
                binSize.BinFactor = (double)barDuration.Ticks / TicksPerYear;
                binSize.TimeAxisLabelsScale = BinScaleLevel.Year;
            }

            return binSize;
        }

        public DateTime BinAddToDate(DateTime dateTime, double value)
        {
            DateTime result = new DateTime();
            if(BinScale == BinScaleLevel.Millisecond)
            {
                Utility.DateTimeAddTryParse(dateTime, new TimeSpan(0, 0, 0, 0, (int)(value * BinFactor)), out result);
            }
            else if (BinScale == BinScaleLevel.Second)
            {
                Utility.DateTimeAddTryParse(dateTime, new TimeSpan(0, 0, (int)(value * BinFactor)), out result);
            }
            else if (BinScale == BinScaleLevel.Minute)
            {
                Utility.DateTimeAddTryParse(dateTime, new TimeSpan(0, (int)(value * BinFactor), 0), out result);
            }
            else if (BinScale == BinScaleLevel.Hour)
            {
                Utility.DateTimeAddTryParse(dateTime, new TimeSpan((int)(value * BinFactor), 0, 0), out result);
            }
            else if (BinScale == BinScaleLevel.Day)
            {
                Utility.DateTimeAddTryParse(dateTime, new TimeSpan((int)(value * BinFactor), 0, 0, 0), out result);
            }
            else if (BinScale == BinScaleLevel.Month)
            {
                Utility.DateTimeAddMonthsTryParse(dateTime, (int)(value * BinFactor), out result);
            }
            else if (BinScale == BinScaleLevel.Year)
            {
                Utility.DateTimeAddYearsTryParse(dateTime, (int)(value * BinFactor), out result);
            }
            else
            {
                throw new NotSupportedException();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            BinSize binSize = (BinSize)obj;

            return binSize != null && BinFactor == binSize.BinFactor && BinScale == binSize.BinScale;
        }
    }
}
