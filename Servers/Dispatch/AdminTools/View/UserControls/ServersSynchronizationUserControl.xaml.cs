using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for SearchServersSynchronizationUserControl.xaml
    /// </summary>
    public partial class ServersSynchronizationUserControl
    {
        public ServersSynchronizationUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> Refresh;

        private void OnRefresh()
        {
            Refresh?.Invoke(this, new EventArgs());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            OnRefresh();
        }
    }
}
