using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class MappingWindow : Window
    {
        #region Property

        public MappingViewModel MappingViewModel { get; set; }        

        #endregion

        #region Method

        public MappingWindow(IDataSource selectedDataSource)
        {
            InitializeComponent();
            MappingViewModel = new MappingViewModel(selectedDataSource.Map);
            MappingUserControl.DataContext = MappingViewModel;

            if (selectedDataSource is TabularDataSourceModel)
            {
                MappingViewModel.CurrentMappingControl = MappingControlType.TabularMappingFirstStep;
            }
            else if (selectedDataSource is UnstructuredDataSourceModel)
            {
                MappingViewModel.CurrentMappingControl = MappingControlType.UnstructuredMapping;
            }

            MappingViewModel.FillRecentMapCollection();
        }

        private void WarningListItemOnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is MapWarningModel selectedWarning)
            {
                MappingViewModel.Map.SelectedWarning = selectedWarning;
                MappingViewModel.Map.SelectedWarning = null;
            }

            if (((ListViewItem)sender).Tag is ObjectMapModel relatedObject)
            {
                relatedObject.IsSelected = true;
                MappingViewModel.Map.SelectedObject = relatedObject;
            }
            else if (((ListViewItem)sender).Tag is PropertyMapModel relatedProperty)
            {
                relatedProperty.OwnerObject.IsSelected = true;
                MappingViewModel.Map.SelectedObject = relatedProperty.OwnerObject;

                //اگر ویژگی انتخاب شده چندتایی باشد
                if (relatedProperty.ParentProperty == null)
                {
                    relatedProperty.IsSelected = true;
                    MappingViewModel.Map.SelectedObject.SelectedProperty = relatedProperty;
                }
                //اگر ویژگی انتخاب شده تکی باشد
                else
                {
                    PropertyMapModel property = relatedProperty;
                    while (property != null)
                    {
                        property.IsSelected = true;
                        property = property.ParentProperty;
                    }
                }
            }
        }

        private void DefectionsButton_Click(object sender, RoutedEventArgs e)
        {
            DefectionsToolBar.IsPopupOpen = true;
        }

        #endregion
    }
}
