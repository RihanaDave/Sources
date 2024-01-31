using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using System.Windows;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class ImageAnalysisApplicatioinContainer : ApplicationContainerBase
    {
        private ImageAnalysisApplicatioin masterApplication = new ImageAnalysisApplicatioin();
        public override PresentationApplication MasterApplication
        {
            get
            {
                return masterApplication;
            }
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
                return iconsResource["ImageAnalysisApplicationIcon"] as BitmapImage;
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

        public override Type MasterApplicationType
        {
            get
            {
                return typeof(ImageAnalysisApplicatioin);
            }
        }

        public override string Title
        {
            get
            {
                return Properties.Resources.Image_Analysis_Application;
            }
        }
    }
}
