using GPAS.LoadTest.Core;
using GPAS.LoadTest.Presenter.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.LoadTest.Presenter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
        }

        /// <summary>
        /// فراخوانی عملیات اجرای‌ تست‌ها
        /// </summary>
        private async void RunTests()
        {
            try
            {
                MessageTextBlock.Text = Properties.Resources.String_TestsAreRunning;
                StopButton.IsHitTestVisible = true;
                StopIcon.Visibility = Visibility.Visible;
                StopProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.Visibility = Visibility.Visible;
                SetRunTestsInfoGrid.IsEnabled = false;
                SidebarMenuListView.IsEnabled = false;

                await mainViewModel.RunTests(TestsTypeComboBox.SelectedItem);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                SetRunTestsInfoGrid.IsEnabled = true;
                SidebarMenuListView.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// فراخوانی عملیات متوقف کردن تست‌ها
        /// </summary>
        private async void StopTests()
        {
            try
            {
                await mainViewModel.RunTests(TestsTypeComboBox.SelectedItem);

                StopButton.IsHitTestVisible = false;
                StopIcon.Visibility = Visibility.Collapsed;
                StopProgressBar.Visibility = Visibility.Visible;
                MessageTextBlock.Text = Properties.Resources.String_StopProcess;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void EventSetter_Handler(object sender, MouseButtonEventArgs e)
        {
            var selectedServer = ((ListViewItem)sender).Tag;

            switch (selectedServer)
            {
                case ServerType.DataRepository:
                    HeaderTextBlock.Text = Properties.Resources.String_DataRepository;
                    break;
                case ServerType.FileRepository:
                    HeaderTextBlock.Text = Properties.Resources.String_FileRepository;
                    break;
                case ServerType.HorizonServer:
                    HeaderTextBlock.Text = Properties.Resources.String_HorizonServer;
                    break;
                case ServerType.SearchServer:
                    HeaderTextBlock.Text = Properties.Resources.String_SearchServer;
                    break;
            }

            mainViewModel.FillAvailableLoadTests(selectedServer);
        }

        private void RunTestsButton_Click(object sender, RoutedEventArgs e)
        {
            RunTests();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopTests();
        }
    }
}
