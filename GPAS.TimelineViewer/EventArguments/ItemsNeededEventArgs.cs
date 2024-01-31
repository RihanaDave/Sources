using System;
using System.Collections.Generic;
using System.Windows;

namespace GPAS.TimelineViewer.EventArguments
{
    public class ItemsNeededEventArgs : EventArgs
    {
        internal ItemsNeededEventArgs(DateTime beginTime, DateTime endTime, BinScaleLevel binScaleLevel, double binFactor, ItemsNeededAction action,
            DateTime? focusTime, Point? focusPosition, DateTime totalLowerBound, DateTime totalUpperBound, double maximumCount)
        {
            BeginTime = beginTime;
            EndTime = endTime;
            BinScaleLevel = binScaleLevel;
            BinFactor = binFactor;
            Action = action;
            FocusTime = focusTime;
            FocusPosition = focusPosition;
            TotalLowerBound = totalLowerBound;
            TotalUpperBound = totalUpperBound;
            MaximumCount = maximumCount;
        }

        public DateTime BeginTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public BinScaleLevel BinScaleLevel { get; private set; }
        public double BinFactor { get; private set; }
        internal Point? FocusPosition { get; private set; }
        public DateTime? FocusTime { get; private set; }
        public ItemsNeededAction Action { get; private set; }
        public List<SuperCategory> FetchedItems { get; set; }
        public DateTime TotalLowerBound { get; set; } = Utility.MinValue;
        public DateTime TotalUpperBound { get; set; } = Utility.MaxValue;
        public double MaximumCount { get; set; }
    }
}
