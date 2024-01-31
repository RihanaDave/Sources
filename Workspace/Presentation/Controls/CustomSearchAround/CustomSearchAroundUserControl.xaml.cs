using GPAS.Ontology;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.Graph;
using GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model;
using GPAS.Workspace.Presentation.Controls.OntologyPickers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround
{
    /// <summary>
    /// Interaction logic for CustomSearchAroundUserControl.xaml
    /// </summary>
    public partial class CustomSearchAroundUserControl : UserControl
    {
        CustomSearchAroundViewModel ViewModel = null;

        public CustomSearchAroundUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = (CustomSearchAroundViewModel)DataContext;
            propertyTypePickerControl.ExceptDataTypeCollection = new ObservableCollection<BaseDataTypes>() { BaseDataTypes.GeoTime };
        }

        private void MainGrid_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                if (e.Data.GetData(DataFormats.FileDrop, true) is string[] fileNames)
                {
                    if (fileNames.Length > 1 || fileNames.Any(filename => Path.GetExtension(filename)?.ToLower() != ".csa"))
                    {
                        dropEnabled = false;
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }

            if (dropEnabled)
                return;

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void MainGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] selectedFilesPath))
                return;

            Load(selectedFilesPath[0]);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_Custom_SearchAround;
                WaitingControl.TaskIncrement();
                KWCustomSearchAroundResult result = null;
                await Task.Run(() => result = ViewModel?.Search());
                OnSearchRequest(new CustomSearchAroundSearchRequestEventArgs(result));
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }
        }

        public event EventHandler<CustomSearchAroundSearchRequestEventArgs> SearchRequest;

        protected void OnSearchRequest(CustomSearchAroundSearchRequestEventArgs e)
        {
            SearchRequest?.Invoke(this, e);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDialog();
        }

        private void LoadDialog()
        {
            OpenFileDialog addFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Custom Search Araound Mapping files (*.csa)|*.csa"
            };

            bool? result = addFileDialog.ShowDialog();

            if (result == true)
            {
                Load(addFileDialog.FileName);
            }
        }

        private void Load(string filePath)
        {
            if (ViewModel?.CustomSearchAroundModel?.ObjectCollection != null &&
                ViewModel.CustomSearchAroundModel.ObjectCollection.Any())
            {
                MessageBoxResult result = KWMessageBox.Show
                (Properties.Resources.Load_a_mapping_will_remove_currently_defined_mapping
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Warning
                    , MessageBoxResult.No);

                if (result != MessageBoxResult.Yes)
                    return;
            }
            try
            {
                LoadedResult loadedResult = ViewModel?.Load(filePath);
                if (!loadedResult.Success)
                {
                    string message = $"Selected map is not match with selected set of objects because:{Environment.NewLine}{Environment.NewLine}";
                    message += string.Join(Environment.NewLine, loadedResult.Messages);

                    KWMessageBox.Show(message, MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_to_load_saved_Custom_Search_Around_Mapping, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                Filter = "Custom Search Araound Mapping files (*.csa)|*.csa"
            };

            bool? saveResult = saveFileDialog.ShowDialog();
            if (saveResult != true)
                return;

            ViewModel?.Save(saveFileDialog.FileName);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is LoadedFileModel loadedFile)
                LoadFromRecentFile(loadedFile);
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is LoadedFileModel loadedFile)
                LoadFromRecentFile(loadedFile);
        }

        protected void LoadFromRecentFile(LoadedFileModel loadedFile)
        {
            if (File.Exists(loadedFile.FullPath))
            {
                Load(loadedFile.FullPath);
            }
            else
            {
                MessageBoxResult result = KWMessageBox.Show(string.Format(Properties.Resources.String_ShortcutDoseNotExist,
                    loadedFile.FileName),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    ViewModel?.RemoveItemFromRecentLoadedList(loadedFile);
                }
            }
        }

        private void PinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is LoadedFileModel loadedFile)
                loadedFile.Pinned = true;
        }

        private void UnpinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is LoadedFileModel loadedFile)
                loadedFile.Pinned = false;
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is LoadedFileModel loadedFile)
                ViewModel?.RemoveItemFromRecentLoadedList(loadedFile);
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is LoadedFileModel loadedFile)
                loadedFile.Pinned = !loadedFile.Pinned;
        }

        private void ListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is LoadedFileModel loadedFile)
            {
                if (e.Key == Key.Delete)
                {
                    ViewModel?.RemoveItemFromRecentLoadedList(loadedFile);
                }
                else if (e.Key == Key.Enter)
                {
                    LoadFromRecentFile(loadedFile);
                }
            }
        }

        private void ObjectsListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                ViewModel?.RemoveObject(ViewModel?.CustomSearchAroundModel?.SelectedObject);
        }

        private void PropertyTypePickerControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void PropertyTypePickerControl_SelectedItemReselected(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void AddPropertyToSelectedObject(Ontology.PropertyNode propertyNode)
        {
            if (propertyNode == null)
                return;

            ViewModel?.AddPropertyToObject(propertyNode.TypeUri, ViewModel?.CustomSearchAroundModel?.SelectedObject);
        }

        private void ClearAllPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RemoveAllPropertyFromObject(ViewModel?.CustomSearchAroundModel?.SelectedObject);
        }

        private void RemovePropertyButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedProperty();
        }

        private void PropertiesDataGridRow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                RemoveSelectedProperty();
        }

        private void RemoveSelectedProperty()
        {
            ViewModel?.RemovePropertyFromObject(ViewModel?.CustomSearchAroundModel?.SelectedObject?.SelectedProperty,
                ViewModel?.CustomSearchAroundModel?.SelectedObject);
        }
    }
}
