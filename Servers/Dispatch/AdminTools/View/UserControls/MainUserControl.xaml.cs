using System;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<Modules> SelectModule;
        public event EventHandler<EventArgs> SelectSidebarItem;

        private void OnSelectModule(Modules moduleType)
        {
            SelectModule?.Invoke(this, moduleType);
        }

        private void OnSelectSidebarItem()
        {
            SelectSidebarItem?.Invoke(this, new EventArgs());
        }

        private void UsersManagerButton_Click(object sender, RoutedEventArgs e)
        {
            OnSelectModule((Modules)((Button)sender).Tag);
        }

        private void DataButton_Click(object sender, RoutedEventArgs e)
        {
            OnSelectModule((Modules)((Button)sender).Tag);
        }

        private void JobManagerButton_Click(object sender, RoutedEventArgs e)
        {
            OnSelectModule((Modules)((Button)sender).Tag);
        }

        private void IpToGeoButton_Click(object sender, RoutedEventArgs e)
        {
            OnSelectModule((Modules)((Button)sender).Tag);
        }

        private void ServersStatusButton_Click(object sender, RoutedEventArgs e)
        {
            OnSelectModule((Modules)((Button)sender).Tag);
        }

        private void AllSidebarItemsComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (AllSidebarItemsComboBox.SelectedItem == null)
                return;

            OnSelectSidebarItem();
        }
    }
}
