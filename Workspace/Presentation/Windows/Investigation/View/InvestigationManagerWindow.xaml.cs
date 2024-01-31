using GPAS.Logger;
using GPAS.Workspace.Presentation.Windows.Investigation.Enums;
using GPAS.Workspace.Presentation.Windows.Investigation.EventArguments;
using GPAS.Workspace.Presentation.Windows.Investigation.Models;
using GPAS.Workspace.Presentation.Windows.Investigation.ViewModels;
using Notifications.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Windows.Investigation
{
    /// <summary>
    /// Interaction logic for InvestigationManagerWindow.xaml
    /// </summary>
    public partial class InvestigationManagerWindow : Window
    {
        InvestigationViewModel investigationViewModel;

        public object GroupListView { get; private set; }

        public InvestigationManagerWindow(InvestigationViewModel investigationViewModel)
        {
            InitializeComponent();           
            this.DataContext = investigationViewModel;
            this.investigationViewModel = investigationViewModel;
            LoadSavedInvestigations();
        }
        private async void LoadSavedInvestigations()
        {
            try
            {
                WaitingControl.TaskIncrement();
                ObservableCollection<InvestigationModel> investigationModels = null;
                await Task.Run(() =>
                {
                    investigationModels = investigationViewModel.LoadSavedInvestigations();
                });

                foreach (var item in investigationModels)
                {
                    investigationViewModel.Items.Add(item);
                }

                WaitingControl.TaskDecrement();
            }
            catch (Exception ex)
            {

                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                MessageBox.Show(ex.Message, Properties.Resources.Object_Explorer_Application, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Events

        public event EventHandler<LoadInvestigationEventArgs> LoadInvestigationButtonClicked;
        protected virtual void OnLoadInvestigationButtonClicked(InvestigationModel investigationStatus)
        {
            LoadInvestigationButtonClicked?.Invoke(this, new LoadInvestigationEventArgs(investigationStatus));
        }

        #endregion

        #region Function 

        private void ShowNotification(string title, string message)
        {
            var notificationManager = new NotificationManager();

            notificationManager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Information
            });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginValidation())
            {
                try
                {
                    WaitingControl.TaskIncrement();
                    investigationViewModel.SaveCurrentInvestigation();
                    ShowNotification(Properties.Resources.Current_Investigation_Saved,
                        Properties.Resources.Save_Investigation);
                    Close();
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedInvestigation();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadSelectedInvestigation();
        }

        private async void LoadSelectedInvestigation()
        {
            if (investigationViewModel.SelectedItem == null)
                return;

            investigationViewModel.SelectedItem.Status = await investigationViewModel.GetStatus(investigationViewModel.SelectedItem.IDentifier);
            OnLoadInvestigationButtonClicked(investigationViewModel.SelectedItem);
            Close();
        }

        private bool LoginValidation()
        {
            TitleInvestigationTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            return IsValid(MainGrid);
        }

        public bool IsValid(DependencyObject control)
        {
            if (Validation.GetHasError(control))
                return false;

            for (var i = 0; i != VisualTreeHelper.GetChildrenCount(control); ++i)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                if (!IsValid(child)) { return false; }
            }
            return true;
        }

        private void DescriptionLink_Click(object sender, RoutedEventArgs e)
        {
            investigationViewModel.SelectedShowPopup = ShowInvestigationPopupEnum.DescriptionLink;
            ShowDialogHost.IsOpen = true;
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogHost.IsOpen = false;
        }

        private void PreviewImageButton_Click(object sender, RoutedEventArgs e)
        {
            ((InvestigationModel)((Button)sender).DataContext).IsSelected = true;
            investigationViewModel.SelectedShowPopup = ShowInvestigationPopupEnum.Image;
            ShowDialogHost.IsOpen = true;
        }

        #endregion
    }
}
