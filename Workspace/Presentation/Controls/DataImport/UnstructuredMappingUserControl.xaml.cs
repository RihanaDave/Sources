using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.OntologyPickers;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for UnstructuredMappingUserControl.xaml
    /// </summary>
    public partial class UnstructuredMappingUserControl
    {
        public UnstructuredMappingUserControl()
        {
            InitializeComponent();
        }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            SaveMap();
        }

        private void LoadMapButtonOnClick(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        private void PropertyPickerOnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void PropertyPicker_SelectedItemReselected(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void AddPropertyToSelectedObject(Ontology.PropertyNode propertyNode)
        {
            if (propertyNode == null)
                return;

            ((MappingViewModel)DataContext).AddPropertyToSelectedObject(propertyNode.TypeUri);
        }

        private void DeleteButtonOnClick(object sender, RoutedEventArgs e)
        {
            DeleteFromRecentMap((SavedMapModel)((Button)sender).DataContext);
        }

        private void RecentMapListViewOnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
        }

        private void RecentMapListViewOnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                DeleteFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoadFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
            }
        }
    }
}
