using GPAS.Dispatch.AdminTools.ViewModel.ObjectViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    /// <summary>
    /// Interaction logic for PermissionsWindow.xaml
    /// </summary>
    public partial class PermissionsWindow 
    {
        private readonly PermissionsViewModel _permissionsViewModel;

        public PermissionsWindow(object selectedProperty)
        {
            InitializeComponent();
            _permissionsViewModel = new PermissionsViewModel();
            DataContext = _permissionsViewModel;
            _selectedProperty = selectedProperty;
        }

        private object _selectedProperty;

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _permissionsViewModel.ShowPropertyPermissions(_selectedProperty);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
