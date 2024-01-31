using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for IPToGeoSpecialUserControl.xaml
    /// </summary>
    public partial class IpToGeoSpecialUserControl 
    {
        public IpToGeoSpecialUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> AddIpGeoEventButton;
        public event EventHandler<EventArgs> ChooseFileEventButton;
        public event EventHandler<EventArgs> ImportIpGeoEventButton;

        private void OnAddIpGeoButton()
        {
            AddIpGeoEventButton?.Invoke(this, new EventArgs());
        }
        
        private void OnChooseFileButton()
        {
            ChooseFileEventButton?.Invoke(this, new EventArgs());
        }

        private void OnImportIpGeoButton()
        {
            ImportIpGeoEventButton?.Invoke(this, new EventArgs());
        }

        private void AddIpGeoButton_Click(object sender, RoutedEventArgs e)
        {
            OnAddIpGeoButton();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OnChooseFileButton();
        }

        private void ImportIpGeoButton_Click(object sender, RoutedEventArgs e)
        {
            OnImportIpGeoButton();
        }
    }
}
