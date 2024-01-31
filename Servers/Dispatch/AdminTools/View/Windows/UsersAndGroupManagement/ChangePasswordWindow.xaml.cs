using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;

namespace GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow
    {
        private readonly UsersManagementViewModel usersManagementViewModel;
        private object user;

        public ChangePasswordWindow(object selectedUser)
        {
            InitializeComponent();
            user = selectedUser;
            usersManagementViewModel = new UsersManagementViewModel();
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

        private bool ChangePasswordValidation()
        {
            if (string.IsNullOrEmpty(NewPasswordBox?.Password))
            {
                MessageBox.Show(Properties.Resources.String_InvalidValue);
                return false;
            }

            if (!NewPasswordBox.Password.Equals(ConfirmPasswordBox?.Password))
            {
                MessageBox.Show(Properties.Resources.String_ConfirmationPasswordDidNotMatch);
                return false;
            }

            return true;
        }

        private async void ChangePassword()
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;

                await usersManagementViewModel.ChangePassword(user, NewPasswordBox.Password);
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChangePasswordValidation())
                ChangePassword();
        }
    }
}
