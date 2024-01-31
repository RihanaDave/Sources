using System;
using System.Collections.Generic;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Observers.Base;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class HomeApplicationContainer : ApplicationContainerBase
    {
        public HomeApplicationContainer()
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
                return iconsResource["HomeApplicationIcon"] as BitmapImage;
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

        private HomeApplication masterApplication = new HomeApplication();
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
                return typeof(HomeApplication);
            }
        }

        public override string Title
        {
            get
            {
                return Properties.Resources.Home_Application;
            }
        }
    }
}

