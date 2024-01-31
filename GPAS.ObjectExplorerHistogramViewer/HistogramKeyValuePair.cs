using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.ObjectExplorerHistogramViewer
{
    public class HistogramKeyValuePair : TreeViewItemBase
    {
       private HisogramKeyValueType keyValueType;
        public HisogramKeyValueType KeyValueType
        {
            get { return this.keyValueType; }
            set
            {
                if (this.keyValueType != value)
                {
                    this.keyValueType = value;
                    this.NotifyPropertyChanged("KeyValueType");
                }
            }
        }

        private bool canShowMore;
        public bool CanShowMore
        {
            get { return this.canShowMore; }
            set
            {
                if (this.canShowMore != value)
                {
                    this.canShowMore = value;
                    this.NotifyPropertyChanged("CanShowMore");
                }
            }
        }

        private bool canShowFewer;
        public bool CanShowFewer
        {
            get { return this.canShowFewer; }
            set
            {
                if (this.canShowFewer != value)
                {
                    this.canShowFewer = value;
                    this.NotifyPropertyChanged("CanShowFewer");
                }
            }
        }

        private bool canShowAll;
        public bool CanShowAll
        {
            get { return this.canShowAll; }
            set
            {
                if (this.canShowAll != value)
                {
                    this.canShowAll = value;
                    this.NotifyPropertyChanged("CanShowAll");
                }
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private BitmapImage icon;
        public BitmapImage Icon
        {
            get { return this.icon; }
            set
            {
                if (this.icon != value)
                {
                    this.icon = value;
                    this.NotifyPropertyChanged("Icon");
                }
            }
        }

        private string type;
        public string Type
        {
            get { return this.type; }
            set
            {
                if (this.type != value)
                {
                    this.type = value;
                    this.NotifyPropertyChanged("Type");
                }
            }
        }

        public string TypeURI { get; set; }

        private long value;
        public long Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.NotifyPropertyChanged("Value");
                }
            }
        }

        private double histogramPercent;
        public double HistogramPercent
        {
            get { return this.histogramPercent; }
            set
            {
                if (this.histogramPercent != value)
                {
                    this.histogramPercent = value;
                    this.NotifyPropertyChanged("HistogramPercent");
                }
            }
        }

        private double histogramMax;
        public double HistogramMax
        {
            get { return this.histogramMax; }
            set
            {
                if (this.histogramMax != value)
                {
                    this.histogramMax = value;
                    this.NotifyPropertyChanged("HistogramMax");
                }
            }
        }

        private long minimumLoadableValueCount;
        public long MinimumLoadableValueCount
        {
            get { return this.minimumLoadableValueCount; }
            set
            {
                if (this.minimumLoadableValueCount != value)
                {
                    this.minimumLoadableValueCount = value;
                    this.NotifyPropertyChanged("MinimumLoadableValueCount");
                }
            }
        }

        public HistogramSubCategory relatedHistogramSubCategory { get; set; }
    }

    public enum HisogramKeyValueType
    {
        LowerBound = 0,
        ChangeHistogramShowingSize = 1,
        KeyValue = 3
    }
}
