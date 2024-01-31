using System;
using GPAS.TimelineViewer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Timeline
{
    public class TimeRange : RelayNotifyPropertyChanged
    {
        #region Properties

        private DateTime from = new DateTime();
        public DateTime From
        {
            get { return from; }
            set { SetValue(ref from, value); }
        }

        private DateTime to = new DateTime();
        public DateTime To
        {
            get { return to; }
            set { SetValue(ref to, value); }
        }

        public TimeSpan Duration
        {
            get
            {
                return To - From;
            }
        }

        private object tag = null;
        public object Tag
        {
            get { return tag; }
            set { SetValue(ref tag, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// اشتراک بازه زمانی را با بازه زمانی ورودی برمی گرداند.
        /// </summary>
        /// <param name="otherRange">بازه زمانی ورودی</param>
        /// <returns>اشتراک</returns>
        public TimeRange Intersect(TimeRange otherRange)
        {
            return TimeRange.Intersect(this, otherRange);
        }

        /// <summary>
        /// اشتراک دو بازه زمانی را برمی گرداند.
        /// </summary>
        /// <param name="range1">بازه اول</param>
        /// <param name="range2">بازه دوم</param>
        /// <returns>اشتراک</returns>
        public static TimeRange Intersect(TimeRange range1, TimeRange range2)
        {
            if (range1.To < range2.From || range1.From > range2.To)
                return null;

            TimeRange intersectRange = new TimeRange();
            intersectRange.From = (range1.From > range2.From) ? range1.From : range2.From;
            intersectRange.To = (range1.To < range2.To) ? range1.To : range2.To;
            return intersectRange;
        }


        public virtual bool Equals(TimeRange otherRange)
        {
            return From == otherRange.From && To == otherRange.To;
        }

        public virtual bool Equals(Range otherRange)
        {
            return From == otherRange.From && To == otherRange.To;
        }

        /// <summary>
        /// مقدار فیت شده بازه زمانی جاری را در مقیاس داده شده محاسبه می کند.
        /// به طور مثال مقدار فیت شده بازه زمانی 29/12/1999 تا 2/1/2000 در مقیاس یک ساله
        /// برابر با 1/1/1999 تا 31/12/2000 می باشد.
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی مقیاس</param>
        /// <param name="binFactor">طول مقیاس</param>
        /// <returns>مقدار فیت شده</returns>
        public TimeRange FitToBoundBinSizeRange(BinScaleLevel binScaleLevel, double binFactor)
        {
            BinSize binSize = new BinSize()
            {
                BinFactor = binFactor,
                BinScale = binScaleLevel,
            };

            return new TimeRange()
            {
                From = TimelineViewer.Utility.Floor(From, binSize),
                To = TimelineViewer.Utility.Ceiling(To, binSize).AddTicks(-1),
            };
        }

        /// <summary>
        /// لیستی از میله های مشترک با بازه زمانی جاری را برمی گرداند.
        /// این میله ها بر حسب مقیاس زمانی هستند
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی مقیاس</param>
        /// <param name="binFactor">طول مقیاس</param>
        /// <returns>لیستی از میله ها</returns>
        public IEnumerable<TimeRange> GetRelatedBars(BinScaleLevel binScaleLevel, double binFactor)
        {
            List<TimeRange> bars = new List<TimeRange>();
            TimeRange boundRange = FitToBoundBinSizeRange(binScaleLevel, binFactor);

            BinSize binSize = new BinSize() { BinFactor = binFactor, BinScale = binScaleLevel };

            while (boundRange.Duration.Ticks > 0)
            {
                TimeRange bar = new TimeRange()
                {
                    From = boundRange.From,
                    To = binSize.BinAddToDate(boundRange.From, 1)
                };

                boundRange.From = boundRange.From.Add(bar.Duration);
                bar.To = bar.To.AddTicks(-1);

                bars.Add(bar);
            }

            return bars;
        }

        /// <summary>
        /// لیستی از میله های مشترک با بازه زمانی جاری را در یک محدوده زمانی خاص برمی گرداند.
        /// این میله ها بر حسب مقیاس زمانی هستند
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی مقیاس</param>
        /// <param name="binFactor">طول مقیاس</param>
        /// <param name="lowerBound">کران پایین محدوده زمانی</param>
        /// <param name="upperBound">کران بالای محدوده زمانی</param>
        /// <returns>لیستی از میله ها</returns>
        public IEnumerable<TimeRange> GetRelatedBarsInBound(BinScaleLevel binScaleLevel, double binFactor, DateTime lowerBound, DateTime upperBound)
        {
            List<TimeRange> bars = new List<TimeRange>();
            TimeRange boundRange = FitToBoundBinSizeRange(binScaleLevel, binFactor);

            if (boundRange.From < lowerBound)
                boundRange.From = lowerBound;

            if (boundRange.To > upperBound)
                boundRange.To = upperBound;

            BinSize binSize = new BinSize() { BinFactor = binFactor, BinScale = binScaleLevel };

            while (boundRange.Duration.Ticks > 0)
            {
                TimeRange bar = new TimeRange()
                {
                    From = boundRange.From,
                    To = binSize.BinAddToDate(boundRange.From, 1)
                };

                boundRange.From = boundRange.From.Add(bar.Duration);
                bar.To = bar.To.AddTicks(-1);

                bars.Add(bar);
            }

            return bars;
        }

        /// <summary>
        /// تبدیل بازه زمانی شناخته شده برای TimelineViewer
        /// به بازه زمانی شناخته شده برای TimelineControl
        /// </summary>
        /// <param name="range">بازه زمانی TimelineViewer</param>
        /// <returns>بازه زمانی TimelineControl</returns>
        public static TimeRange ConvertRangeToTimeRange(Range range)
        {
            return new TimeRange()
            {
                From = range.From,
                To = range.To,
                Tag = range.Tag,
            };
        }

        /// <summary>
        /// تبدیل بازه زمانی جاری به بازه زمانی شناخته شده برای TimelineViewer.
        /// </summary>
        /// <returns>بازه زمانی TimelineViewer</returns>
        public Range ConvertToRange()
        {
            return ConvertTimeRangeToRange(this);
        }

        /// <summary>
        /// تبدیل بازه زمانی شناخته شده برای TimelineControl
        /// به بازه زمانی شناخته شده برای TimelineViewer
        /// </summary>
        /// <param name="timeRange">بازه زمانی TimelineControl</param>
        /// <returns>بازه زمانی TimelineViewer</returns>
        public static Range ConvertTimeRangeToRange(TimeRange timeRange)
        {
            return new Range()
            {
                From = timeRange.From,
                To = timeRange.To,
                Tag = timeRange.Tag,
            };
        }

        /// <summary>
        /// تبدیل بازه زمانی شناخته شده برای پنجره های فیلتر در TimelineViewer
        /// به بازه های زمانی شناخته شده برای TimelineControl.
        /// </summary>
        /// <param name="filterRange">بازه زمانی پنجره های فیلتر TimelineViewer</param>
        /// <returns>بازه زمانی TimelineControl</returns>
        public static TimeRange ConvertFilterRangeToTimeRange(FilterRange filterRange)
        {
            return ConvertRangeToTimeRange(filterRange.ConvertToRange());
        }

        public override int GetHashCode()
        {
            return From.GetHashCode() ^ To.GetHashCode();
        }

        /// <summary>
        /// بازه های زمانی بزرگ را تشخیص می دهد.
        /// بازه زمانی بزرگ، بازه زمانی است که طول آن 1000 برابر مقیاس زمانی داده شده باشد.
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی مقیاس</param>
        /// <param name="binFactor">طول مقیاس</param>
        /// <returns></returns>
        public bool IsBigRange(BinScaleLevel binScaleLevel, double binFactor)
        {
            BinSize binSize = new BinSize() { BinFactor = binFactor, BinScale = binScaleLevel };
            var fitDuration = FitToBoundBinSizeRange(binScaleLevel, binFactor).Duration;
            var barDuration = BinSizes.FindBinSizeFromEnum(BinSizes.FindEnumFromBinSize(binSize), From, To, (int)binFactor).GetDuration(From);

            return fitDuration.Ticks > barDuration.Ticks * 1000;
        }

        #endregion
    }
}
