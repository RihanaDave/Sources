using GPAS.AccessControl;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Applications
{
    public class ShowingDataSource : TreeViewItemBase
    {        
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

        private BitmapImage downloadIcon;
        public BitmapImage DownloadIcon
        {
            get { return this.downloadIcon; }
            set
            {
                if (this.downloadIcon != value)
                {
                    this.downloadIcon = value;
                    this.NotifyPropertyChanged("DownloadIcon");
                }
            }
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }

        private string createdTime;
        public string CreatedTime
        {
            get { return this.createdTime; }
            set
            {
                if (this.createdTime != value)
                {
                    this.createdTime = value;
                    this.NotifyPropertyChanged("CreatedTime");
                }
            }
        }

        private string createdBy;
        public string CreatedBy
        {
            get { return this.createdBy; }
            set
            {
                if (this.createdBy != value)
                {
                    this.createdBy = value;
                    this.NotifyPropertyChanged("CreatedBy");
                }
            }
        }

        private string description;
        public string Description
        {
            get { return this.description; }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.NotifyPropertyChanged("Description");
                }
            }
        }
        

        private bool showMoreHyperlink;
        public bool ShowMoreHyperlink
        {
            get { return this.showMoreHyperlink; }
            set
            {
                if (this.showMoreHyperlink != value)
                {
                    this.showMoreHyperlink = value;
                    this.NotifyPropertyChanged("ShowMoreHyperlink");
                }
            }
        }

        public DataSourceInfo relatedDataSourceInfo { get; set; }

        public ShowingDataSources relatedShowingDataSources { get; set; }
    }
}
