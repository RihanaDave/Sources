using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class DataSourceApplicationContainer : ApplicationContainerBase
    {
        public DataSourceApplicationContainer()
        {
        }

        private List<PresentationHelper> helpers = new List<PresentationHelper>();
        public override List<PresentationHelper> Helpers
        {
            get
            {
                return helpers;
            }
        }

        public override BitmapImage Icon
        {
            get
            {
                ResourceDictionary iconsResource = new ResourceDictionary();
                iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
                return iconsResource["DataSourceApplicationIcon"] as BitmapImage;
            }
        }
        public override BitmapImage SelectedArrow
        {
            get
            {
                ResourceDictionary iconsResource = new ResourceDictionary();
                iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
                return iconsResource["SelectedArrowIcon"] as BitmapImage;
            }
        }

        private DataSourceApplication masterApplication = new DataSourceApplication();
        //private BigDataSearchApplication masterApplication = new BigDataSearchApplication();
        public override PresentationApplication MasterApplication
        {
            get
            {
                return masterApplication;
            }
        }
        

        public override string Title
        {
            get
            {
                return Properties.Resources.BigData_Search_Application;
            }
        }

        public override Type MasterApplicationType => throw new NotImplementedException();
    }
}
