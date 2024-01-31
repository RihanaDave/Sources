using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.ObjectExplorerHistogramViewer
{
    public class HistogramSuperCategory : INotifyPropertyChanged
    {
        public HistogramSuperCategory()
        {
            HistogramSubCategories = new ObservableCollection<HistogramSubCategory>();
        }

        private string superCategory;
        public string SuperCategory
        {
            get { return this.superCategory; }
            set
            {
                if (this.superCategory != value)
                {
                    this.superCategory = value;
                    this.NotifyPropertyChanged("SuperCategory");
                }
            }
        }

        public ObservableCollection<HistogramSubCategory> HistogramSubCategories { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
