using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class MainWindow
    {
        private async void CheckServersStatus()
        {
            try
            {
                BeforeRequest(UsersManagerUserControl);
                await serversStatusViewModel.CheckServersStatus();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                AfterRequest(UsersManagerUserControl);
            }
        }

        private async void RefreshServersStatus()
        {
            try
            {
                await serversStatusViewModel.CheckServersStatus();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        } 

        private void GetServersStatusTimerOnTick(object sender, EventArgs e)
        {
            RefreshServersStatus();
        }

       
    }
}
