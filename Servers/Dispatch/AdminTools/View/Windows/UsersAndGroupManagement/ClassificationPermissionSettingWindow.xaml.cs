using GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.Windows.UsersAndGroupManagement
{
    /// <summary>
    /// Interaction logic for ClassificationPermissionSettingWindow.xaml
    /// </summary>
    public partial class ClassificationPermissionSettingWindow
    {
        UsersManagementViewModel UsersManagementViewModel { get; set; }

        public int Counter { get; set; }

        public ClassificationPermissionSettingWindow(string nameGroup)
        {

            InitializeComponent();
            Counter = 0;
            UsersManagementViewModel = new UsersManagementViewModel
            {
                GroupNameInPermission = nameGroup
            };
            DataContext = UsersManagementViewModel;
            PermossionCombobox.ItemsSource = UsersManagementViewModel.GeneratePermissionsToShow();
            ClassificationTreeview.ItemsSource = UsersManagementViewModel.GenerateTree(UsersManagementViewModel.GroupNameInPermission);
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ClassificationTreeview_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (ClassificationTreeview.SelectedItem != null)
                {
                    var selectedNodeItem = ClassificationTreeview.SelectedItem;
                    UsersManagementViewModel.SelectTreeNode(selectedNodeItem);
                }
            }
            catch (Exception)
            {
                //Nothing Code
            }
        }

        private void PermossionCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Counter != 0)
                {
                    var selectionComboBox = (sender as ComboBox)?.SelectedItem;
                    if ((sender as ComboBox)?.SelectedItem != null)
                    {
                        UsersManagementViewModel.SelectionComboBox(selectionComboBox,
                            ClassificationTreeview.SelectedItem, ClassificationTreeview.ItemsSource);
                    }
                }
                Counter++;
            }
            catch (Exception)
            {
                //Nothing Code
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersManagementViewModel.GroupNameInPermission != null)
                {
                    bool result = UsersManagementViewModel.SavePermission(ClassificationTreeview.ItemsSource);

                    if (result)
                    {
                        Close();
                    }
                }
            }
            catch (Exception)
            {
                //Nothing Code
            }
        }
    }
}
