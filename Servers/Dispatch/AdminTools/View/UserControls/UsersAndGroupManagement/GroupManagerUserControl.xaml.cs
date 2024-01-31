using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.UserControls.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for GroupManagerUserControl.xaml
    /// </summary>
    public partial class GroupManagerUserControl
    {
        public GroupManagerUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> SelectGroup;
        public event EventHandler<EventArgs> DeleteGroup;
        public event EventHandler<EventArgs> DefinePermission;
        public event EventHandler<EventArgs> RefreshGroupsList;
        public event EventHandler<EventArgs> CreateNewGroup;
        public event EventHandler<string> GroupSearch;
        public event EventHandler<EventArgs> GroupClickMember;
        public event EventHandler<EventArgs> Permission;
        
        private void OnPermission()
        {
            Permission?.Invoke(this, new EventArgs());
        }

        private void OnGroupClickMember()
        {
            GroupClickMember?.Invoke(this, new EventArgs());
        }

        private void OnDeleteGroup()
        {
            DeleteGroup?.Invoke(this, new EventArgs());
        }

        private void OnDefinePermission()
        {
            DefinePermission?.Invoke(this, new EventArgs());
        }

        private void OnCreateNewGroup()
        {
            CreateNewGroup?.Invoke(this, new EventArgs());
        }

        private void OnSelectGroup()
        {
            SelectGroup?.Invoke(this, new EventArgs());
        }

        private void OnRefreshUsersList()
        {
            RefreshGroupsList?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// فراخوانی رویداد جستجو در لیست گروه‌ها
        /// </summary>
        private void OnGroupSearch(string userName)
        {
            GroupSearch?.Invoke(this, userName);
        }

        private void CreateNewUserToolBar_Click(object sender, RoutedEventArgs e)
        {
            OnCreateNewGroup();
        }

        private void RefreshToolBarButton_Click(object sender, RoutedEventArgs e)
        {
            OnRefreshUsersList();
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnGroupSearch(FilterTextBox.Text);
        }

        private void UserInfoToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            GroupInfoIcon.Kind = PackIconKind.ChevronLeft;
        }

        private void UserInfoToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            GroupInfoIcon.Kind = PackIconKind.ChevronRight;
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            GroupListContentControl.Template = (ControlTemplate)Resources["ListTemplate"];
        }

        private void TilesButton_Click(object sender, RoutedEventArgs e)
        {
            GroupListContentControl.Template = (ControlTemplate)Resources["CardTemplate"];
        }

        private void EventSetter_Handler(object sender, MouseButtonEventArgs e)
        {
            OnSelectGroup();
        }

        private void ApplicationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectGroup();
        }       

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteGroup();
        }
       
        private void DeleteGroupContext_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteGroup();
        }

        private void DefinePermissionContext_Click(object sender, RoutedEventArgs e)
        {
            OnDefinePermission();
        }

        private void PermissionGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            OnPermission();
        }

        private void EventSetter_HandlerGroup(object sender, MouseButtonEventArgs e)
        {
            OnGroupClickMember();
        }
    }
}
