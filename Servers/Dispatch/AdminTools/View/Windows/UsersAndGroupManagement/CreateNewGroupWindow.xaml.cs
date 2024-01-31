using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for CreateNewGroupWindow.xaml
    /// </summary>
    public partial class CreateNewGroupWindow
    {
        private readonly UsersManagementViewModel usersManagementViewModel;

        public CreateNewGroupWindow()
        {
            InitializeComponent();
            usersManagementViewModel = new UsersManagementViewModel();
            DataContext = usersManagementViewModel;
        }

        public event EventHandler<EventArgs> CreatedGroup;

        private void OnCreatedGroup()
        {
            CreatedGroup?.Invoke(this, new EventArgs());
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CreateValidation())
                return;
            CreateNewGroup();
        }

        private async void CreateNewGroup()
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                var result = await usersManagementViewModel.CreateNewGroup();

                if (result)
                {
                    OnCreatedGroup();
                }
                else
                {
                    MessageBox.Show(Properties.Resources.The_groupname_is_already_exist);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private bool CreateValidation()
        {
            GroupName.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            return IsValid(GroupMainBorder);
        }

        private bool IsValid(DependencyObject control)
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
