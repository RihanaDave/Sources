using GPAS.Dispatch.AdminTools.ViewModel;
using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for JobManagerControl.xaml
    /// </summary>
    public partial class JobManagerUserControl
    {
        public JobManagerUserControl()
        {
            InitializeComponent();
        }
        
        public event EventHandler<EventArgs> RefreshList;
        public event EventHandler<EventArgs> RestartJob;
        public event EventHandler<EventArgs> PuaseResumeJob;


        private void OnRefreshList()
        {
            RefreshList?.Invoke(this, new EventArgs());
        }

        private void OnRestartJob()
        {
            RestartJob?.Invoke(this, new EventArgs());
        }

        private void OnPuaseResumeJob()
        {
            PuaseResumeJob?.Invoke(this, new EventArgs());
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            OnRefreshList();
        }

        private void RestartJobButton_Click(object sender, RoutedEventArgs e)
        {
            OnRestartJob();
        }
        
        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogHostForShowStatusMessage.IsOpen = false;
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogHostForShowStatusMessage.IsOpen = true;
        }

        private void PuaseResumeJobButton_Click(object sender, RoutedEventArgs e)
        {
            OnPuaseResumeJob();
        }
        public JobManagerViewModel ViewModel { get; set; }
        private void BaseUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is JobManagerViewModel)
            {
                ViewModel = (JobManagerViewModel)DataContext;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }   
            else
            {
                ViewModel = null;
            }    
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsExistSelectedJobRequestAfterRefresh"))
            {
                if (ViewModel.IsExistSelectedJobRequestAfterRefresh &&
                    ShowDialogHostForShowStatusMessage.IsOpen)
                {
                    ShowDialogHostForShowStatusMessage.IsOpen = true;
                }
                else
                {
                    ShowDialogHostForShowStatusMessage.IsOpen = false;
                }
            }
        }

        private void PaginationUserControl_ItemPerPageChanged(object sender, Gpas.Pagination.Events.PaginationEventHandler e)
        {
            ((JobManagerViewModel)DataContext).FillJobsRequestList();
        }

        private void PaginationUserControl_PageNumberChanged(object sender, Gpas.Pagination.Events.PaginationEventHandler e)
        {
            ((JobManagerViewModel)DataContext).FillJobsRequestList();
        }
    }
}
