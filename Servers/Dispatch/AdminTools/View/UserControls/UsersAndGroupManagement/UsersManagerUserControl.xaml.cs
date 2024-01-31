using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.UserControls.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for UsersManagerUserControl.xaml
    /// </summary>
    public partial class UsersManagerUserControl
    {
        public UsersManagerUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> SelectUser;
        public event EventHandler<EventArgs> ChangePassword;
        public event EventHandler<EventArgs> ApplyUserEdit;
        public event EventHandler<EventArgs> ManageUserGroups;
        public event EventHandler<EventArgs> RefreshUsersList;
        public event EventHandler<EventArgs> CreateNewUser;
        public event EventHandler<EventArgs> DeleteUser;
        public event EventHandler<EventArgs> ShowChangePasswordWindow;
        public event EventHandler<string> UserSearch;

        private void OnSelectUser()
        {
            SelectUser?.Invoke(this, new EventArgs());
        }

        private void OnChangePassword()
        {
            ChangePassword?.Invoke(this, new EventArgs());
        }

        private void OnApplyUserEdit()
        {
            ApplyUserEdit?.Invoke(this, new EventArgs());
        }

        private void OnManageUserGroups()
        {
            ManageUserGroups?.Invoke(this, new EventArgs());
        }

        private void OnRefreshUsersList()
        {
            RefreshUsersList?.Invoke(this, new EventArgs());
        }

        private void OnCreateNewUser()
        {
            CreateNewUser?.Invoke(this, new EventArgs());
        }

        private void OnDeleteUser()
        {
            DeleteUser?.Invoke(this,new EventArgs());
        }

        private void OnShowChangePasswordWindow()
        {
            ShowChangePasswordWindow?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// فراخوانی رویداد جستجو در لیست کاربران
        /// </summary>
        private void OnUserSearch(string userName)
        {
            UserSearch?.Invoke(this, userName);
        }

        private void UserInfoToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            UserInfoIcon.Kind = PackIconKind.ChevronLeft;
        }

        private void UserInfoToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UserInfoIcon.Kind = PackIconKind.ChevronRight;
        }

        private void EventSetter_Handler(object sender, MouseButtonEventArgs e)
        {
            OnSelectUser();
        }

        private void SavePasswordChangesButton_Click(object sender, RoutedEventArgs e)
        {
            OnChangePassword();
        }

        private void ApplyEditButton_Click(object sender, RoutedEventArgs e)
        {
            OnApplyUserEdit();
        }

        private void EditUserGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            OnManageUserGroups();
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteUser();
        }

        private void RefreshToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            OnRefreshUsersList();
        }

        private void CreateNewUserToolBar_Click(object sender, RoutedEventArgs e)
        {
            OnCreateNewUser();
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            UsersListContentControl.Template = (ControlTemplate)Resources["ListTemplate"];
        }

        private void TilesButton_Click(object sender, RoutedEventArgs e)
        {
            UsersListContentControl.Template = (ControlTemplate)Resources["CardTemplate"];
        }

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectUser();
        }

        private void ManageGroupsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            OnManageUserGroups();
        }

        private void ChangePasswordContextMenu_Click(object sender, RoutedEventArgs e)
        {
            OnShowChangePasswordWindow();
        }

        private void DeleteUserContextMenu_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteUser();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnUserSearch(FilterTextBox.Text);
        }
    }
}
