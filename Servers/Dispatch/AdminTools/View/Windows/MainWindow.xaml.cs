using GPAS.Dispatch.AdminTools.ViewModel;
using GPAS.Dispatch.AdminTools.ViewModel.ObjectViewer;
using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;
using Notifications.Wpf;
using System;
using System.Windows;
using System.Windows.Threading;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ObjectViewerViewModel objectViewerViewModel;
        private readonly JobManagerViewModel jobManagerViewModel;
        private readonly MainWindowViewModel mainWindowViewModel;
        private readonly IpToGeoSpacialViewModel ipToGeoSpacialViewModel;
        private readonly DataManagementViewModel dataManagementViewModel;
        private readonly UsersManagementViewModel usersManagementViewModel;
        private readonly ServersSynchronizationViewModel serversSynchronizationViewModel;
        private readonly ServersStatusViewModel serversStatusViewModel;
        private readonly HorizonIndexManagerViewModel horizonIndexManagerViewModel;

        /// <summary>
        /// تایمر گرفتن لیست نتایج
        /// </summary>
        private readonly DispatcherTimer getServersStatusTimer;
        private readonly DispatcherTimer getStatusImpotTimer;

        public MainWindow()
        {
            InitializeComponent();
            objectViewerViewModel = new ObjectViewerViewModel();
            mainWindowViewModel = new MainWindowViewModel();
            jobManagerViewModel = new JobManagerViewModel();
            ipToGeoSpacialViewModel = new IpToGeoSpacialViewModel();
            dataManagementViewModel = new DataManagementViewModel();
            usersManagementViewModel = new UsersManagementViewModel();
            serversSynchronizationViewModel = new ServersSynchronizationViewModel();
            serversStatusViewModel = new ServersStatusViewModel();
            horizonIndexManagerViewModel = new HorizonIndexManagerViewModel();

            ObjectViewerUserControl.DataContext = objectViewerViewModel;
            IpToGeoSpecialControl.DataContext = ipToGeoSpacialViewModel;
            JobManagerControl.DataContext = jobManagerViewModel;
            DataManagementUserControl.DataContext = dataManagementViewModel;
            UsersManagerUserControl.DataContext = usersManagementViewModel;
            GroupManagerUserControl.DataContext = usersManagementViewModel;
            ServersSynchronizationControl.DataContext = serversSynchronizationViewModel;
            ServersStatusUserControl.DataContext = serversStatusViewModel;
            HorizonIndexManagerUserControl.DataContext = horizonIndexManagerViewModel;
            DataContext = mainWindowViewModel;

            getServersStatusTimer = new DispatcherTimer();
            getServersStatusTimer.Interval = TimeSpan.FromSeconds(15);
            getServersStatusTimer.Tick += GetServersStatusTimerOnTick;

           
            getStatusImpotTimer = new DispatcherTimer();
            getStatusImpotTimer.Interval = TimeSpan.FromSeconds(15);
            getStatusImpotTimer.Tick += jobManagerViewModel.GetStatusImportTimerOnTick;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.ExitWorkspace();
        }

        private void SidebarMenuTemplateUserControl_SidebarEvent(object sender, EventArgs e)
        {
            var selectedItem = mainWindowViewModel.SpecifyEvent(SidebarMenuTemplateUserControl.MenuListView.SelectedItem);

            switch (selectedItem)
            {
                case SidebarItems.Home:
                    getServersStatusTimer.Stop();
                    getStatusImpotTimer.Stop();
                    break;
                case SidebarItems.UserManager:
                    GetUsers();
                    break;
                case SidebarItems.GroupManager:
                    GetGroups();
                    break;
                case SidebarItems.IndexesSynchronization:
                    GetServersStability();
                    break;
                case SidebarItems.JobManager:
                    GetJobRequestList();
                    break;
                case SidebarItems.ServersStatus:
                    CheckServersStatus();
                    break;
                case SidebarItems.HorizonIndexManager:
                    HorizonIndexManagerUserControl.GetAllIndexes();
                    break;
                case SidebarItems.RemoveAllData:
                case SidebarItems.ObjectViewer:
                case SidebarItems.IpToGeoSpecial:
                    break;
            }
        }

        private void ServersSynchronizationControl_Refresh(object sender, EventArgs e)
        {
            GetServersStability();
        }

        private async void GetServersStability()
        {
            try
            {
                BeforeRequest(ServersSynchronizationControl);
                await serversSynchronizationViewModel.GetServersStability();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                AfterRequest(ServersSynchronizationControl);
            }
        }

        private void SpecifyModuleToShow(Modules selectedModule)
        {
            switch (selectedModule)
            {
                case Modules.UsersManager:
                    mainWindowViewModel.ShowUsersManagerItems();
                    BaseViewModel.CurrentControl = SidebarItems.UserManager;
                    GetUsers();
                    break;
                case Modules.DataManager:
                    mainWindowViewModel.ShowDataManagerItems();
                    BaseViewModel.CurrentControl = SidebarItems.IndexesSynchronization;
                    GetServersStability();
                    break;
                case Modules.JobManager:
                    mainWindowViewModel.ShowJobManagerItems();
                    BaseViewModel.CurrentControl = SidebarItems.JobManager;
                    GetJobRequestList();
                    getStatusImpotTimer.Start();
                    break;
                case Modules.IpToGeoSpecial:
                    mainWindowViewModel.ShowIpToGeoItems();
                    BaseViewModel.CurrentControl = SidebarItems.IpToGeoSpecial;
                    break;
                case Modules.ServersStatus:
                    mainWindowViewModel.ShowServersStatusItems();
                    BaseViewModel.CurrentControl = SidebarItems.ServersStatus;
                    CheckServersStatus();
                    getServersStatusTimer.Start();
                    break;
            }

            SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
        }

        private void MainUserControl_SelectModule(object sender, Modules e)
        {
            SpecifyModuleToShow(e);
        }

        private async void RemoveAllData()
        {
            MessageBoxResult messageBoxResult =
                MessageBox.Show
                (
                    Properties.Resources.String_RemoveAllData,
                    "Attention",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

            if (messageBoxResult != MessageBoxResult.Yes)
                return;

            try
            {
                BeforeRequest(DataManagementUserControl);
                var result = await dataManagementViewModel.RemoveAllData();

                if (result)
                {
                    MessageBox.Show
                    (
                        Properties.Resources.String_RemoveDataSuccess,
                        "Remove Data Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show
                    (
                        Properties.Resources.String_RemoveDataFailed,
                        "Remove Data Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                AfterRequest(DataManagementUserControl);
            }
        }

        private void DataManagementUserControl_RemoveAllData(object sender, EventArgs e)
        {
            RemoveAllData();
        }

        private void MainUserControl_SelectSidebarItem(object sender, EventArgs e)
        {
            PrepareSidebarItemToShow();
        }

        private void PrepareSidebarItemToShow()
        {
            var selectedItem = MainUserControl.AllSidebarItemsComboBox.SelectedItem;

            switch (mainWindowViewModel.SpecifyEvent(selectedItem))
            {
                case SidebarItems.UserManager:
                    mainWindowViewModel.ShowUsersManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
                    GetUsers();
                    break;
                case SidebarItems.GroupManager:
                    mainWindowViewModel.ShowUsersManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 2;
                    GetGroups();
                    break;
                case SidebarItems.IndexesSynchronization:
                    mainWindowViewModel.ShowDataManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
                    GetServersStability();
                    break;
                case SidebarItems.ObjectViewer:
                    mainWindowViewModel.ShowDataManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 2;
                    break;
                case SidebarItems.RemoveAllData:
                    mainWindowViewModel.ShowDataManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 4;
                    break;
                case SidebarItems.JobManager:
                    mainWindowViewModel.ShowJobManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
                    break;
                case SidebarItems.IpToGeoSpecial:
                    mainWindowViewModel.ShowIpToGeoItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
                    GetJobRequestList();
                    break;
                case SidebarItems.ServersStatus:
                    mainWindowViewModel.ShowServersStatusItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
                    CheckServersStatus();
                    break;
                case SidebarItems.HorizonIndexManager:
                    HorizonIndexManagerUserControl.GetAllIndexes();
                    mainWindowViewModel.ShowDataManagerItems();
                    SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 3;
                    break;
            }
        }

        private void BeforeRequest(UIElement control)
        {
            SidebarMenuTemplateUserControl.MenuListView.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            control.IsEnabled = false;
        }

        private void AfterRequest(UIElement control)
        {
            SidebarMenuTemplateUserControl.MenuListView.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Collapsed;
            control.IsEnabled = true;
        }

        private async void JobManagerControl_PuaseResumeJob(object sender, EventArgs e)
        {
            try
            {
                if (JobManagerControl.JobsList.SelectedItems.Count == 0)
                    return;

                if (jobManagerViewModel.SelectedJobRequest.State == Entities.Jobs.JobRequestStatus.Busy)
                {
                    await jobManagerViewModel.StopJob(JobManagerControl.JobsList.SelectedItem);
                }
                else if (jobManagerViewModel.SelectedJobRequest.State == Entities.Jobs.JobRequestStatus.Pause)
                {
                    await jobManagerViewModel.ResumeJob(JobManagerControl.JobsList.SelectedItem);
                }
                GetJobRequestList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                AfterRequest(JobManagerControl);
            }
        }
    }
}
