using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.ObjectExplorerHistogramViewer
{
    public class HistogramSubCategory : TreeViewItemBase
    {
        public HistogramSubCategory()
        {
            NotShowingHistogramKeyValuePairs = new ObservableCollection<HistogramKeyValuePair>();
            ShowingHistogramKeyValuePairs = new ObservableCollection<HistogramKeyValuePair>();
        }

        private string subCategory;
        public string SubCategory
        {
            get { return this.subCategory; }
            set
            {
                if (this.subCategory != value)
                {
                    this.subCategory = value;
                    this.NotifyPropertyChanged("SubCategory");
                }
            }
        }

        private int showingItemsCount;
        public int ShowingItemsCount
        {
            get { return this.showingItemsCount; }
            set
            {
                if (this.showingItemsCount != value)
                {
                    this.showingItemsCount = value;
                    this.NotifyPropertyChanged("ShowingItemsCount");
                }
            }
        }

        private int totalItemsCount;
        public int TotalItemsCount
        {
            get { return this.totalItemsCount; }
            set
            {
                if (this.totalItemsCount != value)
                {
                    this.totalItemsCount = value;
                    this.NotifyPropertyChanged("TotalItemsCount");
                }
            }
        }

        public long maxCount { get; set; }

        public string TypeURI { get; set; }

        public ObservableCollection<HistogramKeyValuePair> ShowingHistogramKeyValuePairs { get; set; }
        public ObservableCollection<HistogramKeyValuePair> NotShowingHistogramKeyValuePairs { get; set; }

        public HistogramKeyValuePair LastSelectedKeyValuePair { get; set; }

        public HistogramSuperCategory relatedHistogramSuperCategory { get; set; }
    }
}
