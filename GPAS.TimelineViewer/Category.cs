using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;

namespace GPAS.TimelineViewer
{
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool isChecked = true;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
                    NotifyPropertyChanged();

                    if (Parent != null && Parent.SubCategories != null)
                        if (Parent.SubCategories.All(sc => sc.IsChecked == value))
                        {
                            Parent.IsChecked = value;
                        }
                        else
                        {
                            Parent.IsChecked = null;
                        }
                }
            }
        }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (value != title)
                {
                    title = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BitmapImage icon;
        public BitmapImage Icon
        {
            get
            {
                return icon;
            }
            set
            {
                if (value != icon)
                {
                    icon = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private object identifier;
        public object Identifier
        {
            get
            {
                return identifier;
            }
            set
            {
                if (value != identifier)
                {
                    identifier = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Brush legendColor;
        public Brush LegendColor
        {
            get
            {
                return legendColor;
            }
            set
            {
                if (value != legendColor)
                {
                    legendColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime? lowerBound = null;
        public DateTime? LowerBound
        {
            get
            {
                return lowerBound;
            }
            set
            {
                if (value != lowerBound)
                {
                    lowerBound = value;

                    UpdateInBoundDataItems(new List<TimelineItemsSegment>(DataItems));

                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime? upperBound = null;
        public DateTime? UpperBound
        {
            get
            {
                return upperBound;
            }
            set
            {
                if (value != upperBound)
                {
                    upperBound = value;

                    UpdateInBoundDataItems(new List<TimelineItemsSegment>(DataItems));

                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<TimelineItemsSegment> dataItems = new ObservableCollection<TimelineItemsSegment>();
        public ObservableCollection<TimelineItemsSegment> DataItems
        {
            get
            {
                return dataItems;
            }
            set
            {
                if (value != dataItems)
                {
                    dataItems = value;
                    DataItems.CollectionChanged -= DataItems_CollectionChanged;
                    DataItems.CollectionChanged += DataItems_CollectionChanged;

                    UpdateInBoundDataItems(dataItems);

                    NotifyPropertyChanged();
                }
            }
        }

        private void DataItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                List<TimelineItemsSegment> newItems = new List<TimelineItemsSegment>();
                foreach (TimelineItemsSegment item in e.NewItems)
                {
                    newItems.Add(item);
                }

                UpdateInBoundDataItems(newItems);
            }

            if (e.OldItems?.Count > 0)
            {
                foreach (TimelineItemsSegment item in e.OldItems)
                {
                    InBoundDataItems.Remove(item);
                }

                UpdateInBoundDataItems(dataItems);
            }
        }

        private ObservableCollection<TimelineItemsSegment> inBoundDataItems = new ObservableCollection<TimelineItemsSegment>();
        public ObservableCollection<TimelineItemsSegment> InBoundDataItems
        {
            get
            {
                return inBoundDataItems;
            }
            set
            {
                if (value != inBoundDataItems)
                {
                    inBoundDataItems = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private SuperCategory parent;
        public SuperCategory Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                if (value != parent)
                {
                    parent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private object tag;
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                if (value != tag)
                {
                    tag = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BarSeries relatedSeries;
        internal BarSeries RelatedSeries
        {
            get
            {
                return relatedSeries;
            }
            set
            {
                if (value != relatedSeries)
                {
                    relatedSeries = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private RadCartesianChart relatedChart;
        internal RadCartesianChart RelatedChart
        {
            get
            {
                return relatedChart;
            }
            set
            {
                if (value != relatedChart)
                {
                    relatedChart = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void UpdateInBoundDataItems(IEnumerable<TimelineItemsSegment> dataItems)
        {
            try
            {
                dataItems = RemoveOutBoundDataItems(dataItems);
                InBoundDataItems.Clear();
                foreach (var dataItem in dataItems)
                {
                    InBoundDataItems.Add(dataItem);
                }
            }
            catch
            {

            }
        }

        public static IEnumerable<TimelineItemsSegment> RemoveOutBoundDataItems(IEnumerable<TimelineItemsSegment> dataItems, DateTime lowerBound, DateTime upperBound)
        {
            List<TimelineItemsSegment> inBound = new List<TimelineItemsSegment>();
            if (dataItems?.Count() > 0)
            {
                DateTime lb = lowerBound;
                DateTime ub = upperBound;
                TimeSpan duration = dataItems.First().Duration;
                Utility.DateTimeAddTryParse(lowerBound, -duration, out lb);
                Utility.DateTimeAddTryParse(upperBound, duration, out ub);

                foreach (var di in dataItems)
                {
                    if (di.Type == DataItemType.TotalLowerBoundExtraItem || di.Type == DataItemType.TotalUpperBoundExtraItem ||
                        di.Type == DataItemType.LowerBoundExtraItem || di.Type == DataItemType.UpperBoundExtraItem)
                    {
                        inBound.Add(di);
                    }
                    else
                    {
                        if (di.From >= lb && di.To <= ub)
                        {
                            inBound.Add(di);
                        }
                    }
                }
            }

            return inBound;
        }

        private IEnumerable<TimelineItemsSegment> RemoveOutBoundDataItems(IEnumerable<TimelineItemsSegment> dataItems)
        {
            if (LowerBound.HasValue && UpperBound.HasValue && dataItems != null && dataItems?.Count() > 0)
            {
                return RemoveOutBoundDataItems(dataItems, LowerBound.Value, UpperBound.Value);
            }

            return dataItems;
        }
    }
}
