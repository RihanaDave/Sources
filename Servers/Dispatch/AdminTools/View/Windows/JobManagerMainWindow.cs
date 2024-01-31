using System;
using System.Windows;
using GPAS.Dispatch.AdminTools.ViewModel;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class MainWindow
    {
        private async void GetJobRequestList()
        {
            try
            {
                BeforeRequest(JobManagerControl);
                await jobManagerViewModel.GetJobsRequest();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
               AfterRequest(JobManagerControl);
            }
        }

        private async void RestartJob()
        {
            try
            {
                BeforeRequest(JobManagerControl);

                if (JobManagerControl.JobsList.SelectedItems.Count == 0)
                    return;

                var selectedJob = JobManagerControl.JobsList.SelectedItem;

                await jobManagerViewModel.RestartJob(selectedJob);
                await jobManagerViewModel.GetJobsRequest();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                AfterRequest(JobManagerControl);
            }
        }

        private void JobManagerControl_RefreshList(object sender, EventArgs e)
        {
            GetJobRequestList();
        }

        private void JobManagerControl_RestartJob(object sender, EventArgs e)
        {
            RestartJob();
        }
    }
}
