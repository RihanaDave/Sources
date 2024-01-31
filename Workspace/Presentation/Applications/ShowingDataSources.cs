using GPAS.AccessControl;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Applications
{
    public class ShowingDataSources : TreeViewItemBase
    {

        public ShowingDataSources()
        {
            DataSources = new ObservableCollection<ShowingDataSource>();
            NumberOfMoreRequested = 0;
        }

        public int NumberOfMoreRequested { get; set; }

        private DataSourceType dataSourceType;
        public DataSourceType DataSourceType
        {
            get { return this.dataSourceType; }
            set
            {
                if (this.dataSourceType != value)
                {
                    this.dataSourceType = value;
                    this.NotifyPropertyChanged("DataSourceType");
                }
            }
        }        
        public ObservableCollection<ShowingDataSource> DataSources { get; set; }
    }
}
