using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for DataManagementUserControl.xaml
    /// </summary>
    public partial class DataManagementUserControl 
    {
        public DataManagementUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> RemoveAllData;
        private void OnRemoveAllData()
        {
            RemoveAllData?.Invoke(this, new EventArgs());
        }

        private void RemoveAllDataButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveAllData();
        }
    }
}
