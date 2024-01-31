using GPAS.TimelineViewer.BinSize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.TimelineViewer
{
    internal class ChartMetadata
    {
        public ChartMetadata(List<DateTime> chartData, BinSize.BinSize bin)
        {
            Bin = bin;
            LowerBound = chartData.Min();
            UpperBound = chartData.Max();

            DateTime firstTimeToShow = f1(LowerBound, bin.TimeAxisLabelsScale);
            //var beginingVoidTimeData = LowerBound;
            //while (firstTimeToShow < beginingVoidTimeData)
            //{
            //    Utility.TimeIntervals.DecreaseTimeOneStep(ref beginingVoidTimeData, bin);
            //}
            Utility.TimeIntervals.DecreaseTimeOneStep(ref firstTimeToShow, bin);

            DateTime lastTimeToShow = f2(UpperBound, bin.TimeAxisLabelsScale);
            //var endingVoidTimeData = UpperBound;
            //while (lastTimeToShow > endingVoidTimeData)
            //{
            //    Utility.TimeIntervals.IncreaseTimeOneStep(ref endingVoidTimeData, bin);
            //}
            Utility.TimeIntervals.IncreaseTimeOneStep(ref lastTimeToShow, bin);

            //Intervals = Utility.TimeIntervals.GetIntervalsStartTime(beginingVoidTimeData, endingVoidTimeData, bin);
            Intervals = Utility.TimeIntervals.GetIntervalsStartTime(firstTimeToShow, lastTimeToShow, bin);

            ShowingTimesPerIntervalStartTime = new SortedDictionary<DateTime, List<DateTime>>();
            ShowingTimesPerIntervalStartTime.Add(Intervals[0], new List<DateTime>());
            foreach (var showingDate in chartData)
            {
                var startingPointForItem = Utility.TimeIntervals.GetIntervalStartTimeCoversTime(showingDate, bin);
                if (!ShowingTimesPerIntervalStartTime.ContainsKey(startingPointForItem))
                    ShowingTimesPerIntervalStartTime.Add(startingPointForItem, new List<DateTime>());
                ShowingTimesPerIntervalStartTime[startingPointForItem].Add(showingDate);
            }
            ShowingTimesPerIntervalStartTime.Add(Intervals[Intervals.Count - 1], new List<DateTime>());

            MaximumFrequency = ShowingTimesPerIntervalStartTime.Values.Select(l => l.Count).Max();
        }

        private DateTime f2(DateTime upperBound, BinScaleLevel timeAxisLabelsScale)
        {
            switch (timeAxisLabelsScale)
            {
                case BinScaleLevel.Year:
                    return upperBound.AddYears(1);
                case BinScaleLevel.Month:
                    return (new DateTime(upperBound.Year, upperBound.Month, 1)).AddMonths(1);
                case BinScaleLevel.Day:
                    return (new DateTime(upperBound.Year, upperBound.Month, upperBound.Day)).AddDays(1);
                case BinScaleLevel.Hour:
                    return (new DateTime(upperBound.Year, upperBound.Month, upperBound.Day, upperBound.Hour, 0, 0)).AddHours(1);
                case BinScaleLevel.Minute:
                    return (new DateTime(upperBound.Year, upperBound.Month, upperBound.Day, upperBound.Hour, upperBound.Minute, 0)).AddMinutes(1);
                case BinScaleLevel.Second:
                    return (new DateTime(upperBound.Year, upperBound.Month, upperBound.Day, upperBound.Hour, upperBound.Minute, upperBound.Second)).AddSeconds(1);
                default:
                    throw new NotSupportedException();
            }
        }

        private DateTime f1(DateTime lowerBound, BinScaleLevel timeAxisLabelsScale)
        {
            switch (timeAxisLabelsScale)
            {
                case BinScaleLevel.Year:
                    return lowerBound.AddYears(-1);
                case BinScaleLevel.Month:
                    return (new DateTime(lowerBound.Year, lowerBound.Month, 1)).AddMonths(-1);
                case BinScaleLevel.Day:
                    return (new DateTime(lowerBound.Year, lowerBound.Month, lowerBound.Day)).AddDays(-1);
                case BinScaleLevel.Hour:
                    return (new DateTime(lowerBound.Year, lowerBound.Month, lowerBound.Day, lowerBound.Hour, 0, 0)).AddHours(-1);
                case BinScaleLevel.Minute:
                    return (new DateTime(lowerBound.Year, lowerBound.Month, lowerBound.Day, lowerBound.Hour, lowerBound.Minute, 0)).AddMinutes(-1);
                case BinScaleLevel.Second:
                    return (new DateTime(lowerBound.Year, lowerBound.Month, lowerBound.Day, lowerBound.Hour, lowerBound.Minute, lowerBound.Second)).AddSeconds(-1);
                default:
                    throw new NotSupportedException();
            }
        }

        public BinSize.BinSize Bin
        {
            private set;
            get;
        }

        public SortedDictionary<DateTime, List<DateTime>> ShowingTimesPerIntervalStartTime
        {
            get;
            private set;
        }
        public DateTime LowerBound
        {
            get;
            private set;
        }
        public DateTime UpperBound
        {
            get;
            private set;
        }
        public int MinimumFrequency
        {
            get { return 0; }
        }
        public int MaximumFrequency
        {
            get;
            private set;
        }
        public int RowNumber
        {
            get { return MaximumFrequency; }
        }
        public int DataColumnsNumber
        {
            get { return ShowingTimesPerIntervalStartTime.Count; }
        }
        /// <summary>
        /// Returns all (zero and non-zero freq.) intervals starting times
        /// </summary>
        public List<DateTime> Intervals
        {
            private set;
            get;
        }
    }
}