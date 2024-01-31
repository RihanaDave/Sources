using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;
using System;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for EditUserGroupsWindow.xaml
    /// </summary>
    public partial class EditUserGroupsWindow
    {
        private readonly UsersManagementViewModel usersManagementViewModel;
        private readonly object user;

        public EditUserGroupsWindow(object selectedUser)
        {
            InitializeComponent();
            user = selectedUser;
            usersManagementViewModel = new UsersManagementViewModel();
            UserNameTextBlock.Text = usersManagementViewModel.GetUserNameOfUser(selectedUser);
            DataContext = usersManagementViewModel;
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

        private async void GetUserGroups(bool firstTime)
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                await usersManagementViewModel.GetUserGroups(user, firstTime);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private async void RemoveFromGroups()
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                await usersManagementViewModel.RemoveUserFromGroups(user);
                GetUserGroups(false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private async void AddToGroups()
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                await usersManagementViewModel.AddUserToGroups(user);
                GetUserGroups(false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private async void RestoreUserGroups()
        {
            try
            {
                IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;

                await usersManagementViewModel.RestoreUserGroups(user);
                GetUserGroups(false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveFromGroups();
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            AddToGroups();
        }

        private void EditUserGroupsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GetUserGroups(true);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreUserGroups();
        }
    }
}
