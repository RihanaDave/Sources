using GPAS.AccessControl;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows
{
    public partial class SettingsWindow : Window
    {
        private ThemeApplication initialTheme;
        private ThemeApplication newTheme;

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        private void OnThemeChanged(ThemeApplication theme)
        {
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }

        public SettingsWindow()
        {
            InitializeComponent();
            SetCurrentTheme();
            ACLModel acl = LoadAcl();
            SetPermissionUserControl.DataContext = new ACLViewModel(acl);
        }

        private ACLModel LoadAcl()
        {
            AccessControl.ACL oldAcl = Logic.UserAccountControlProvider.ManuallyEnteredDataACL;

            ACLModel acl = new ACLModel()
            {
                Classification = new ClassificationModel()
                {
                    Identifier = oldAcl.Classification,
                }
            };

            foreach (ACI permission in oldAcl.Permissions)
            {
                acl.Permissions.Add(new ACIModel
                {
                    GroupName = permission.GroupName,
                    AccessLevel = permission.AccessLevel
                });
            }

            return acl;
        }

        private void SaveAcl()
        {
            AccessControl.ACL newAcl = new AccessControl.ACL();
            List<ACI> newPermissionsList = new List<ACI>();

            foreach (ACIModel permission in ((ACLViewModel)SetPermissionUserControl.DataContext).Acl.Permissions)
            {
                newPermissionsList.Add(new ACI
                {
                    AccessLevel = permission.AccessLevel,
                    GroupName = permission.GroupName
                });
            }

            newAcl.Classification = ((ACLViewModel)SetPermissionUserControl.DataContext).Acl.Classification.Identifier;
            newAcl.Permissions = newPermissionsList;
            Logic.UserAccountControlProvider.ManuallyEnteredDataACL = newAcl;
        }

        private void SetCurrentTheme()
        {
            initialTheme = (ThemeApplication)int.Parse(ConfigurationManager.AppSettings["Theme"]);

            if (initialTheme == ThemeApplication.Dark)
            {
                DarkRadioButton.IsChecked = true;
            }
            else
            {
                LightRadioButton.IsChecked = true;
            }
        }

        private void SetNewTheme()
        {
            switch (newTheme)
            {
                case ThemeApplication.Dark:
                    SetDarkTheme();
                    break;
                case ThemeApplication.Light:
                    SetLightTheme();
                    break;
            }
        }

        private void LightRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            newTheme = ThemeApplication.Light;
        }

        private void DarkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            newTheme = ThemeApplication.Dark;
        }

        private void PermissionListViewItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PermissionGrid.Visibility = Visibility.Visible;
            ThemeGrid.Visibility = Visibility.Collapsed;
        }

        private void ThemeListViewItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ThemeGrid.Visibility = Visibility.Visible;
            PermissionGrid.Visibility = Visibility.Collapsed;
        }

        private void SetLightTheme()
        {
            (Application.Current as App)?.SetLightTheme();
            OnThemeChanged(ThemeApplication.Light);
        }

        private void SetDarkTheme()
        {
            (Application.Current as App)?.SetDarkTheme();
            OnThemeChanged(ThemeApplication.Dark);
        }       

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            SaveAcl();
            SetNewTheme();
            Close();
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
