using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for CreateNewUserWindow.xaml
    /// </summary>
    public partial class CreateNewUserWindow
    {
        private readonly UsersManagementViewModel usersManagementViewModel;

        public CreateNewUserWindow()
        {
            InitializeComponent();
            usersManagementViewModel = new UsersManagementViewModel();
            DataContext = usersManagementViewModel;
        }

        public event EventHandler<EventArgs> CreatedUser;

        private void OnCreatedUser()
        {
            CreatedUser?.Invoke(this,new EventArgs());
        }

        /// <summary>
        /// حرکت دادن پنجره با ماوس
        /// </summary>
        /// <param name="sender"/>
        /// <param name="e"/>
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private bool CreateValidation()
        {
            UserNameTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            PasswordPasswordBox.GetBindingExpression(PasswordBoxAssistant.BoundPassword)?.UpdateSource();

            if (!PasswordPasswordBox.Password.Equals(ConfirmPasswordBox.Password))
            {
                MessageBox.Show(Properties.Resources.String_ConfirmationPasswordDidNotMatch);
                return false;
            }

            return  IsValid(MainBorder);
        }

        private async void CreateNewUser()
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                var result = await usersManagementViewModel.CreateNewUser();

                if (result)
                {
                    OnCreatedUser();
                    ConfirmPasswordBox.Password = string.Empty;
                }
                else
                {
                    MessageBox.Show(Properties.Resources.The_username_is_already_exist);
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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CreateValidation())
                return;
            CreateNewUser();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
