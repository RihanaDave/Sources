using System;
using System.Collections.Generic;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Observers.Base;
using System.Windows;
using System.Windows.Media.Imaging;
using GPAS.Workspace.Entities.Investigation;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class BrowserApplicationContainer : ApplicationContainerBase
    {
        public BrowserApplicationContainer()
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
                return iconsResource["BrowserApplicationIcon"] as BitmapImage;
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

        private BrowserApplication masterApplication = new BrowserApplication();
        public override PresentationApplication MasterApplication
        {
            get
            {
                return masterApplication;
            }
        }

        public override Type MasterApplicationType
        {
            get
            {
                return typeof(BrowserApplication);
            }
        }

        public override string Title
        {
            get
            {
                return Properties.Resources.Browser_Application;
            }
        }

        internal BrowserApplicationStatus GetBrowserApplicationStatus()
        {
            BrowserApplicationStatus browserAppStatus = masterApplication.GetBrowserApplicationStatus();
            return browserAppStatus;
        }

        internal async Task SetBrowserApplicationStatus(BrowserApplicationStatus browserAppStatus)
        {
            await masterApplication.SetBrowserApplicationStatus(browserAppStatus);
        }
    }
}
