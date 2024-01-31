using Microsoft.Win32;
using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class MainWindow
    {
        private async void AddGeoSpecialInformation()
        {
            try
            {
                BeforeRequest(IpToGeoSpecialControl);
                await ipToGeoSpacialViewModel.AddGeoSpecialInformation();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                AfterRequest(IpToGeoSpecialControl);
            }
        }

        private async void ImportIpGeo()
        {
            try
            {
                BeforeRequest(IpToGeoSpecialControl);
                await ipToGeoSpacialViewModel.ImportGeoSpacialFile();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                AfterRequest(IpToGeoSpecialControl);
            }
        }


        private void IpToGeoSpecialControl_AddIpGeoEventButton(object sender, EventArgs e)
        {
            AddGeoSpecialInformation();
        }

        private void IpToGeoSpecialControl_ChooseFileEventButton(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };

            if (openFileDialog.ShowDialog() == true)
            {
                ipToGeoSpacialViewModel.GeoInfoModel.GeoFilePath = openFileDialog.FileName;
            }
        }

        private void IpToGeoSpecialControl_ImportIpGeoEventButton(object sender, EventArgs e)
        {
            ImportIpGeo();
        }
    }
}
