using System;

namespace GPAS.TimelineViewer
{
    public class TimelineItemsSegment : Range
    {
        public DateTime PaddedFrom
        {
            get
            {
                DateTime paddedFrom = Utility.CenterRange(From, To);
                return paddedFrom;
            }
        }

        private double frequencyCount = 0.0;
        public double FrequencyCount
        {
            get { return frequencyCount; }
            set
            {
                if (value != frequencyCount)
                {
                    frequencyCount = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DataItemType type = DataItemType.Regular;
        public DataItemType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (value != type)
                {
                    type = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
