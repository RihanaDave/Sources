using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Windows.DataImport;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for MapPropertiesListUserControl.xaml
    /// </summary>
    public partial class MapPropertiesListUserControl
    {
        public MapPropertiesListUserControl()
        {
            InitializeComponent();
        }

        private void PropertiesListViewOnItemsMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void RemoveButtonOnClick(object sender, RoutedEventArgs e)
        {
            RemoveProperty((PropertyMapModel)PropertiesDataGrid.SelectedItem);
        }

        private void RemoveProperty(PropertyMapModel propertyToRemove)
        {
            try
            {
                ((MappingViewModel)DataContext).RemovePropertyFromSelectedObject(propertyToRemove);
            }
            catch (Exception exception)
            {
                KWMessageBox.Show(exception.Message, MessageBoxButton.OK
                    , MessageBoxImage.Error);
            }
        }

        private void ClearAllProperties()
        {
            try
            {
                ((MappingViewModel)DataContext).ClearAllPropertyFromSelectedObject();
            }
            catch (Exception exception)
            {
                KWMessageBox.Show(exception.Message, MessageBoxButton.OK
                , MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// تغییر ویژگی به عنوان نام نمایشی برای موجودیت
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAsDisplayNameCheckBoxOnChecked(object sender, RoutedEventArgs e)
        {
            ((PropertyMapModel)((CheckBox)sender).DataContext).IsDisplayName = true;
        }

        private void SetAsDisplayNameCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((PropertyMapModel)((CheckBox)sender).DataContext).IsDisplayName = false;
        }

        private void SetResolutionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ((PropertyMapModel)((CheckBox)sender).DataContext).HasResolution = true;
        }

        private void SetResolutionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((PropertyMapModel)((CheckBox)sender).DataContext).HasResolution = false;
        }

        private void ClearAllButtonOnClick(object sender, RoutedEventArgs e)
        {
            ClearAllProperties();
        }

        private void ConfigDateTimeButtonOnClick(object sender, RoutedEventArgs e)
        {
            ShowDateTimeConfig(((Button)sender).DataContext as PropertyMapModel);
        }

        private void ShowDateTimeConfig(PropertyMapModel property)
        {
            if (property is DateTimePropertyMapModel dateTimeProperty)
            {
                DateTimeConfigWindow dateTimeConfigWindow = new DateTimeConfigWindow(dateTimeProperty)
                {
                    Owner = Window.GetWindow(this)
                };
                dateTimeConfigWindow.ShowDialog();
            }
            else if (property is GeoPointPropertyMapModel || property is GeoTimePropertyMapModel)
            {
                GeoPointPropertyMapModel geoPointProperty = null;
                if (property is GeoPointPropertyMapModel)
                    geoPointProperty = property as GeoPointPropertyMapModel;
                else
                    geoPointProperty = (property as GeoTimePropertyMapModel).GeoPoint;

                GeoSpecialFormatWindow geoSpecialFormatWindow = new GeoSpecialFormatWindow(geoPointProperty)
                {
                    Owner = Window.GetWindow(this)
                };
                geoSpecialFormatWindow.ShowDialog();
            }
        }

        /// <summary>
        /// وقتی در هر جایی از دکمه کلیک شود
        /// گزینه "انتخاب به عنوان نام نمایشی" فعال یا غیر‌فعال شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetAsDisplayNameCheckBoxButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is CheckBox)
                return;

            if (((Button)sender).Content is CheckBox setAsDisplayNameCheckBox)
            {
                setAsDisplayNameCheckBox.IsChecked = setAsDisplayNameCheckBox.IsChecked == false;
            }
        }

        private void ResolveButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is CheckBox)
                return;

            if (((Button)sender).Content is CheckBox SetResolutionCheckBox)
            {
                SetResolutionCheckBox.IsChecked = SetResolutionCheckBox.IsChecked == false;
            }
        }
    }
}
