using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Windows
{
    /// <summary>
    /// این کنترل انتزاعی به عنوان پدر تمام پنجره های لایه ارائه در نظر گرفته شده
    /// </summary>
    public abstract class PresentationWindow : Window
    {
        private bool canCloseWindow = true;

        /// <summary>
        /// سازنده
        /// </summary>
        protected PresentationWindow()
        {

            // تعیین عنوان پنجره
            Title = App.AssemblyTitle;
            Title += string.Format(" {0}", App.AssemblyVersion);
//#if DEBUG
//            Title += string.Format(" [Dispatch Host: {0}]", Logic.System.GetRemoteServiceMachineAddress());
//#endif
            // تعیین آیکن پنجره
            ResourceDictionary iconsResource = new ResourceDictionary();
            iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
            Icon = iconsResource["ApplicationIcon"] as BitmapImage;
            // به صورت پیش فرض تمام پنجره های ساخته شده از این کلاس در وسط صفحه قرار می گیرند
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            FlowDirection
                = Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;

            Closed += PresentationWindow_Closed;
            Closing += PresentationWindow_Closing;
            IsVisibleChanged += PresentationWindow_IsVisibleChanged;
        }

        private void PresentationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canCloseWindow)
            {
                e.Cancel = true;
            }
        }

        private void PresentationWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible == false && App.MainWindow != null)
                App.MainWindow.Focus();
        }

        private void PresentationWindow_Closed(object sender, EventArgs e)
        {
            if (App.MainWindow != null)
                App.MainWindow.Focus();
        }
    }
}