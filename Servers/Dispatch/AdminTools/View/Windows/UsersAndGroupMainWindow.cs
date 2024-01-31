using GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement;
using GPAS.Dispatch.AdminTools.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class MainWindow
    {
        private async void GetUsers()
        {
            try
            {
                BeforeRequest(UsersManagerUserControl);
                await usersManagementViewModel.GetUsers();
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

        private async void GetGroups()
        {
            try
            {
                BeforeRequest(UsersManagerUserControl);
                await usersManagementViewModel.GetGroups();
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

        private async void ChangePassword()
        {
            try
            {
                BeforeRequest(UsersManagerUserControl);
                await usersManagementViewModel.ChangePassword(selectedUserToChangePassword, newUserPassword);
                AfterRequest(UsersManagerUserControl);

                //چرخش کارت کاربر
                ButtonAutomationPeer peer = new ButtonAutomationPeer(flipToForwardButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv?.Invoke();
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

        private async void EditUserInfo()
        {
            try
            {
                BeforeRequest(UsersManagerUserControl);
                await usersManagementViewModel.EditUserInfo();
                GetUsers();
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

        private void ManageUserGroups()
        {
            var template = UsersManagerUserControl.UsersListContentControl.Template;
            var usersListView = (ListView)template.FindName("UsersListView", UsersManagerUserControl.UsersListContentControl);

            var selectedUser = usersListView.SelectedItem;
            if (selectedUser == null) return;

            EditUserGroupsWindow editUserGroupsWindow = new EditUserGroupsWindow(selectedUser);
            editUserGroupsWindow.ShowDialog();
        }

        private string newUserPassword = string.Empty;
        private object selectedUserToChangePassword;
        private Button flipToForwardButton;

        private bool ChangePasswordValidation()
        {
            PasswordBox newPassword = null;
            PasswordBox confirmPassword = null;

            var template = UsersManagerUserControl.UsersListContentControl.Template;
            var usersListView = (ListView)template.FindName("UsersListView", UsersManagerUserControl.UsersListContentControl);

            var selectedUserModel = usersListView.SelectedItem;
            if (selectedUserModel == null) return false;

            // get the current selected item
            ListViewItem selectedUser = usersListView.ItemContainerGenerator.ContainerFromIndex(usersListView.SelectedIndex) as ListViewItem;

            if (selectedUser == null) return false;

            //get the item's template parent
            var templateParent = GetFrameworkElementByName<ContentPresenter>(selectedUser);

            //get the DataTemplate that TextBlock in.
            var dataTemplate = usersListView.ItemTemplate;

            if (dataTemplate != null && templateParent != null)
            {
                newPassword = dataTemplate.FindName("NewPasswordBox", templateParent) as PasswordBox;
                confirmPassword = dataTemplate.FindName("ConfirmPasswordBox", templateParent) as PasswordBox;
                flipToForwardButton = dataTemplate.FindName("FlipToForwardButton", templateParent) as Button;
            }

            if (string.IsNullOrEmpty(newPassword?.Password))
            {
                MessageBox.Show(Properties.Resources.String_InvalidValue);
                return false;
            }

            if (!newPassword.Password.Equals(confirmPassword?.Password))
            {
                MessageBox.Show(Properties.Resources.String_ConfirmationPasswordDidNotMatch);
                return false;
            }

            newUserPassword = newPassword.Password;
            selectedUserToChangePassword = selectedUserModel;

            return true;
        }

        /// <summary>
        /// یافتن آیتم مورد نظر با استفاده از نام آن آیتم
        /// در یک قالب
        /// </summary>
        /// <typeparam name="T">نام آیتمی که به دنبال آن هستیم</typeparam>
        /// <param name="referenceElement">قالب مورد نظر</param>
        /// <returns>آیتمی که به دنبال آن بودیم</returns>
        private static T GetFrameworkElementByName<T>(FrameworkElement referenceElement) where T : FrameworkElement
        {
            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                Debug.WriteLine(child);

                if (child != null && child.GetType() == typeof(T))

                { break; }

                if (child != null)
                {
                    child = GetFrameworkElementByName<T>(child);

                    if (child != null && child.GetType() == typeof(T))
                    {
                        break;
                    }
                }
            }

            return child as T;
        }

        private void CreateNewUser()
        {
            CreateNewUserWindow createNewUserWindow = new CreateNewUserWindow();
            createNewUserWindow.CreatedUser += CreateNewUserWindowOnCreatedUser;
            createNewUserWindow.ShowDialog();
            GetUsers();
        }

        private void CreateNewUserWindowOnCreatedUser(object sender, EventArgs e)
        {
            GetUsers();
        }

        private async void DeleteUser()
        {
            var result = MessageBox.Show(Properties.Resources.String_DeleteUser,
                Properties.Resources.String_DeleteUserTitle,
                MessageBoxButton.YesNo,MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                var template = UsersManagerUserControl.UsersListContentControl.Template;
                var usersListView = (ListView)template.FindName("UsersListView", UsersManagerUserControl.UsersListContentControl);

                var selectedUser = usersListView.SelectedItem;
                if (selectedUser == null)
                    return;

                BeforeRequest(UsersManagerUserControl);
                await usersManagementViewModel.DeleteUser(selectedUser);
                GetUsers();
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

        private void UsersManagerUserControl_SelectUser(object sender, EventArgs e)
        {
            var template = UsersManagerUserControl.UsersListContentControl.Template;
            var listView = (ListView)template.FindName("UsersListView", UsersManagerUserControl.UsersListContentControl);

            if (listView?.SelectedItem != null)
            {
                usersManagementViewModel.SelectUserToShowDetails(listView.SelectedItem);
            }
        }

        private void ShowChangePasswordWindow()
        {
            var template = UsersManagerUserControl.UsersListContentControl.Template;
            var usersListView = (ListView)template.FindName("UsersListView", UsersManagerUserControl.UsersListContentControl);

            var selectedUser = usersListView.SelectedItem;
            if (selectedUser == null) return;

            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(selectedUser);
            changePasswordWindow.Show();
        }

        private void UsersManagerUserControl_ChangePassword(object sender, EventArgs e)
        {
            if (!ChangePasswordValidation()) return;
            ChangePassword();
        }

        private void UsersManagerUserControl_ApplyUserEdit(object sender, EventArgs e)
        {
            EditUserInfo();
        }

        private void UsersManagerUserControl_ManageUserGroups(object sender, EventArgs e)
        {
            ManageUserGroups();
        }

        private void UsersManagerUserControl_RefreshUsersList(object sender, EventArgs e)
        {
            GetUsers();
        }

        private void UsersManagerUserControl_CreateNewUser(object sender, EventArgs e)
        {
            CreateNewUser();
        }

        private void UsersManagerUserControl_DeleteUser(object sender, EventArgs e)
        {
            DeleteUser();
        }

        private void UsersManagerUserControl_ShowChangePasswordWindow(object sender, EventArgs e)
        {
            ShowChangePasswordWindow();
        }

        private void UsersManagerUserControl_UserSearch(object sender, string e)
        {
            usersManagementViewModel.FilterUsersList(e);
        }

        private async void DeleteGroup()
        {
            var messageResult = MessageBox.Show
            (
                Properties.Resources.Are_You_Sure_You_Want_To_Delete_The_Group,
                Properties.Resources.Delete_Group,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes
            );

            if (messageResult == MessageBoxResult.No)
                return;

            var template = GroupManagerUserControl.GroupListContentControl.Template;
            var listView = (ListView)template.FindName("GroupListView", GroupManagerUserControl.GroupListContentControl);
            if (listView == null) return;
            var selectedGroup = listView.SelectedItem;
            await usersManagementViewModel.DeleteGroup(selectedGroup);
            GetGroups();
        }

        private void GroupManagerUserControl_SelectGroup(object sender, EventArgs e)
        {
            var template = GroupManagerUserControl.GroupListContentControl.Template;
            var listView = (ListView)template.FindName("GroupListView", GroupManagerUserControl.GroupListContentControl);
            if (listView != null)
            {
                var selectedUser = listView.SelectedItem;
                usersManagementViewModel.SelectGroupToShowDetails(selectedUser);
            }

            //این بخش برای اضافه کردن اعضا به که لیست اعضا در گروه ها است
            if (listView != null)
            {
                var selectedUser = listView.SelectedItem;
                usersManagementViewModel.GetShipGroupUserMember(selectedUser);
            }
        }

        private void GroupManagerUserControl_CreateNewGroup(object sender, EventArgs e)
        {
            CreateNewGroupWindow createNewGroupWindow = new CreateNewGroupWindow();
            createNewGroupWindow.CreatedGroup += CreateNewGroupWindowOnCreatedGroup;
            createNewGroupWindow.ShowDialog();
            GetGroups();
        }

        private void CreateNewGroupWindowOnCreatedGroup(object sender, EventArgs e)
        {
            GetGroups();
        }

        private void GroupManagerUserControl_GroupSearch(object sender, string e)
        {
            usersManagementViewModel.FilterGroupList(e);
        }

        private void GroupManagerUserControl_RefreshGroupsList(object sender, EventArgs e)
        {
            GetGroups();
        }

        private void GroupManagerUserControl_DeleteGroup(object sender, EventArgs e)
        {
            DeleteGroup();
        }

        private void GroupManagerUserControl_GroupClickMember(object sender, EventArgs e)
        {
            usersManagementViewModel.SelectGroupMemberList(GroupManagerUserControl.GroupMembersListView.SelectedItem);
            SidebarMenuTemplateUserControl.MenuListView.SelectedIndex = 1;
            BaseViewModel.CurrentControl = SidebarItems.UserManager;
        }

        private void GroupManagerUserControl_Permission(object sender, EventArgs e)
        {
            var template = GroupManagerUserControl.GroupListContentControl.Template;
            var listView = (ListView)template.FindName("GroupListView", GroupManagerUserControl.GroupListContentControl);
            ClassificationPermissionSettingWindow permissionWindow = new ClassificationPermissionSettingWindow(usersManagementViewModel.GetNameGroup(listView.SelectedItem));
            permissionWindow.ShowDialog();
        }
    }
}
