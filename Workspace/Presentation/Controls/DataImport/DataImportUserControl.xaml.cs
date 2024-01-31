using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.EventArguments;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Windows.DataImport;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for DataImportUserControl.xaml
    /// </summary>
    public partial class DataImportUserControl
    {
        #region Properties

        private DataImportViewModel viewModel;
        private DependencyObject clickedElement;
        private List<IDataSource> draggingDataSources;
        private UserControl draggingItem;
        private bool isDraggingStarted;
        private Point startPoint;
        private Point clickPoint;
        private ScrollViewer mainScrollViewer;

        private readonly string[] supportedUnstructuredFileExtensions = OntologyProvider.GetOntology().GetDocumentSubTypeURIs();
        private readonly string[] supportedStructuredFileExtensions = OntologyProvider.GetOntology().GetTabularFileTypes();
        List<string> allSupportedExtensions;
        private string mapPasteMessage;
        private string permissionPasteMessage;

        public DefectionType SelectedDefectionType
        {
            get { return (DefectionType)GetValue(SelectedDefectionTypeProperty); }
            set { SetValue(SelectedDefectionTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDefectionType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDefectionTypeProperty =
            DependencyProperty.Register(nameof(SelectedDefectionType), typeof(DefectionType), typeof(DataImportUserControl),
                new PropertyMetadata(DefectionType.None));

        #endregion

        #region Events

        public event EventHandler<AddFilesEventArgs> AddFiles;

        private void OnAddFiles(string[] filesPath)
        {
            AddFiles?.Invoke(this, new AddFilesEventArgs(filesPath));
        }

        public event EventHandler<EventArgs> RemoveDataSources;

        private void OnRemoveDataSources()
        {
            RemoveDataSources?.Invoke(this, new EventArgs());
        }

        public event EventHandler<SelectionChangedEventArgs> DataSourceListSelectionChanged;

        private void OnDataSourceListSelectionChanged(SelectionChangedEventArgs e)
        {
            DataSourceListSelectionChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public DataImportUserControl()
        {
            InitializeComponent();
            DataContextChanged += DataImportUserControl_DataContextChanged;
            allSupportedExtensions =
            supportedUnstructuredFileExtensions.Concat(supportedStructuredFileExtensions).Distinct().ToList();
            RemoveCommand = new RelayCommand(RemoveSelectedDataSourcesCommandMethod);
        }

        private void DataImportUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is DataImportViewModel)
            {
                viewModel = (DataImportViewModel)DataContext;
            }
        }

        private void DataSourcesGridDragOver(object sender, DragEventArgs e)
        {

        }

        private void DataSourcesGridDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] selectedFilesPath))
                return;

            List<string> supportedFiles = new List<string>();

            //تعداد تمام فایل‌های شناسایی شده چه آنهایی که پشتیبانی ‌می‌شوند
            //چه آنهایی که پشتیبانی نمی‌شوند
            int filesDetectedCount = 0;

            foreach (string path in selectedFilesPath)
            {
                FileAttributes attributes = File.GetAttributes(path);

                if (attributes.HasFlag(FileAttributes.Directory))
                {
                    filesDetectedCount += Directory.GetFiles(path).Length;
                    supportedFiles.AddRange(Directory.GetFiles(path).Where(FileIsSupported).ToList());
                }
                else
                {
                    filesDetectedCount += 1;

                    if (FileIsSupported(path))
                    {
                        supportedFiles.Add(path);
                    }
                }
            }

            if (supportedFiles.Count > 0)
                OnAddFiles(supportedFiles.ToArray());

            if (supportedFiles.Count != filesDetectedCount)
            {
                NotSupportedFileMessageSnackBar.Show();
            }
        }

        private bool FileIsSupported(string filePath)
        {
            return File.Exists(filePath) && allSupportedExtensions.Contains(Path.GetExtension(filePath)?.Substring(1).ToUpper());
        }

        private bool AllFilesAreSupported(string[] filesPath)
        {
            IEnumerable<string> allSupportedExtensions = supportedUnstructuredFileExtensions.Concat(supportedStructuredFileExtensions).Distinct();

            return filesPath.All(path => File.Exists(path) && allSupportedExtensions.Contains(Path.GetExtension(path)?.Substring(1).ToUpper()));
        }

        /// <summary>
        /// تغییر نحوه نمایش منابع داده
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewButtonClick(object sender, RoutedEventArgs e)
        {
            switch (ViewButton.Icon)
            {
                case PackIconKind.ViewGrid:
                    SetListViewMode();
                    break;
                case PackIconKind.FormatListBulletedSquare:
                    SetGridViewMode();
                    break;
            }
        }

        public void Reset()
        {
            SetListViewMode();
            PreviewHeaderRadioButton.IsChecked = true;
            DataSourceHistogramUserControl.Reset();
        }

        private void SetListViewMode()
        {
            DataSourcesContentControl.Template = (ControlTemplate)Resources["CardTemplate"];
            ViewButton.Icon = PackIconKind.FormatListBulletedSquare;
        }

        private void SetGridViewMode()
        {
            DataSourcesContentControl.Template = (ControlTemplate)Resources["ListTemplate"];
            ViewButton.Icon = PackIconKind.ViewGrid;
        }

        private void DeselectAllListViewItems()
        {
            var template = DataSourcesContentControl.Template;
            var dataSourceListView = (ListView)template.FindName("DataSourceListView", DataSourcesContentControl);
            if (dataSourceListView != null)
            {
                dataSourceListView.UnselectAll();
            }
        }

        private void OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = GenerateFilterTypeForFileDialog(supportedUnstructuredFileExtensions.Concat(supportedStructuredFileExtensions).Distinct())
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != true)
                return;

            OnAddFiles(openFileDialog.FileNames);
        }

        private string GenerateFilterTypeForFileDialog(IEnumerable<string> extensions)
        {
            StringBuilder allSupportedFilesExtension = new StringBuilder(string.Empty);
            StringBuilder addFileDialogFilter = new StringBuilder(string.Empty);

            foreach (string extension in extensions)
            {
                addFileDialogFilter.Append(string.Format("|{0} files (*.{1}) | *.{1};", extension.ToUpper(), extension.ToLower()));
                allSupportedFilesExtension.Append($" *.{extension.ToLower()};");
            }

            return $"All supported files |{allSupportedFilesExtension}" + addFileDialogFilter;
        }

        /// <summary>
        /// با انتخاب هر جایی از سطر در منوی مرتب‌ سازی
        /// آن گزینه انتخاب ‌می‌شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPopUpMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            RadioButton radioButton = (RadioButton)((Grid)sender).Children[1];
            radioButton.IsChecked = true;
        }

        private void AddDataSourceButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }

        private void AddDataSourceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }


        private void AttachDatabaseButtonClick(object sender, RoutedEventArgs e)
        {
            ShowDisplayDatabasesWindow();
        }

        private void AddDatabaseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowDisplayDatabasesWindow();
        }

        private void ShowDisplayDatabasesWindow()
        {
            DisplayDataBasesWindow displayDataBasesWindow = new DisplayDataBasesWindow(viewModel);
            displayDataBasesWindow.Owner = Window.GetWindow(this);
            displayDataBasesWindow.ShowDialog();
        }

        private void RemoveDataSourceButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedDataSources();
        }

        private void RemoveAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllDataSources();
        }

        private void RemoveAllDataSources()
        {
            if (viewModel?.DataSourceCollection == null || viewModel.DataSourceCollection.Count == 0)
                return;

            MessageBoxResult result = KWMessageBox.Show(Properties.Resources.StringRemoveAllDataSources,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                viewModel.DataSourceCollection.Clear();
        }

        private void RemoveSelectedDataSources()
        {
            if (viewModel.SelectedDataSourceCollection.Count <= 0)
                return;

            MessageBoxResult result = KWMessageBox.Show(Properties.Resources.StringRemoveDataSources,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                OnRemoveDataSources();
        }

        private void ImportAllButtonClick(object sender, RoutedEventArgs e)
        {
            Import(viewModel?.DataSourceCollection);
        }

        private void ImportAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Import(viewModel?.DataSourceCollection);
        }
        private void ImportSelectedItemButtonClick(object sender, RoutedEventArgs e)
        {
            Import(viewModel?.SelectedDataSourceCollection);
        }

        private void Import(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return;

            IEnumerable<IDataSource> readyDataSources = GetAllReadyDataSourceForImport(dataSources);
            foreach (IDataSource dataSource in readyDataSources)
            {
                viewModel.Import(new List<IDataSource>() { dataSource });
            }
        }

        private IEnumerable<IDataSource> GetAllReadyDataSourceForImport(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return new List<IDataSource>();

            return dataSources.Where(ds => ds?.ImportStatus == DataSourceImportStatus.Ready);
        }

        private void MappingHyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            DeselectAllListViewItems();
            ListViewItem selectedItem = FindParent<ListViewItem>(((System.Windows.Documents.Hyperlink)sender).Parent as TextBlock);
            selectedItem.IsSelected = true;

            ShowMappingWindow();
        }

        private void PermissionHyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            DeselectAllListViewItems();
            ListViewItem selectedItem = FindParent<ListViewItem>(((System.Windows.Documents.Hyperlink)sender).Parent as TextBlock);
            selectedItem.IsSelected = true;

            ShowPermissionWindow();
        }

        private void DataSourceListViewOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedItemsCount = ((ListView)sender).SelectedItems.Count;
            if (selectedItemsCount.Equals(0))
            {
                SelectedItemsTextBlock.Visibility = Visibility.Collapsed;
                RemoveDataSourceButton.IsEnabled = false;
            }
            else
            {
                SelectedItemsTextBlock.Visibility = Visibility.Visible;
                RemoveDataSourceButton.IsEnabled = true;
            }

            if (e.AddedItems.Count != 0)
            {
                foreach (IDataSource dataSource in e.AddedItems)
                {
                    dataSource.IsSelected = true;
                }
            }

            if (e.RemovedItems.Count != 0)
            {
                foreach (IDataSource dataSource in e.RemovedItems)
                {
                    dataSource.IsSelected = false;
                }
            }

            OnDataSourceListSelectionChanged(e);
        }

        /// <summary>
        /// وقتی روی قسمت خالی از لیست منابع داده کلیک می‌شود منابع داده انتخاب شده 
        /// لغو انتخاب می‌شوند
        /// </summary>
        /// <param name="sender"/>
        /// <param name="e"/>
        private void DataSourceListViewOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ItemsPresenter || e.OriginalSource is Border || e.OriginalSource is Rectangle)
                return;

            ((ListView)sender).UnselectAll();
        }

        private void MappingMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            ShowMappingWindow();
        }
        private void PermissionMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            ShowPermissionWindow();
        }

        private void ShowMappingWindow()
        {
            if (viewModel?.SelectedDataSource == null)
                return;

            MappingWindow setMappingWindow =
                new MappingWindow(viewModel.SelectedDataSource)
                {
                    Owner = App.MainWindow
                };
            setMappingWindow.ShowDialog();
        }

        private void ShowPermissionWindow()
        {
            if (viewModel?.SelectedDataSource == null)
                return;

            SetPermissionWindow setPermissionWindow =
                new SetPermissionWindow(viewModel.SelectedDataSource.Acl)
                {
                    Owner = App.MainWindow
                };
            setPermissionWindow.ShowDialog();
        }

        private void CopyMapMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            viewModel?.CopyTheMapOfSelectedDataSource();
            CopyMapMessageSnackBar.Show();
        }
        private void PasteMapMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            string message = viewModel?.PasteMapForSelectedDataSources();

            if (string.IsNullOrEmpty(message))
            {
                PasteMapMessageSnackBar.Show();
            }
            else
            {
                PasteMapErrorMessageSnackBar.Show();
                mapPasteMessage = message;
            }
        }
        private void CopyPermissionMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            viewModel?.CopyThePermissionOfSelectedDataSource();
            CopyPermissionMessageSnackBar.Show();
        }

        private void PastePermissionMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = viewModel?.PastePermissionForSelectedDataSources();
                if (string.IsNullOrEmpty(message))
                {
                    PastePermissionMessageSnackBar.Show();
                }
                else
                {
                    PastePermissionErrorMessageSnackBar.Show();
                    permissionPasteMessage = message;
                }
            }
            catch (Exception ex)
            {
                PastePermissionErrorMessageSnackBar.Show();
                permissionPasteMessage = ex.Message;
            }
        }

        private void SetCsvSeparatorMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            SelectorCsvSepratorWindow selectorCsvSeparatorWindow = new SelectorCsvSepratorWindow(viewModel)
            {
                Owner = Window.GetWindow(this)
            };

            selectorCsvSeparatorWindow.ShowDialog();
        }
        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedDataSources();
        }
        private void ManageGroupMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            GroupDataSourcesWindow groupDataSourcesWindow = new GroupDataSourcesWindow(viewModel)
            {
                Owner = Window.GetWindow(this)
            };

            groupDataSourcesWindow.ShowDialog();
        }

        private void CreateNewGroupMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            viewModel.MakeEmlDataSourcesGroup();
        }

        private void DataSourceListViewItemsOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (viewModel.SelectedDataSource != null)
            {
                ShowMappingWindow();
            }
        }

        private void MappingButtonOnClick(object sender, RoutedEventArgs e)
        {
            ShowMappingWindow();
        }


        private void AclButtonOnClick(object sender, RoutedEventArgs e)
        {
            ShowPermissionWindow();
        }

        private void PasteMessageSnackBarOnShowDetailsClicked(object sender, EventArgs e)
        {
            KWMessageBox.Show(mapPasteMessage, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void PermissionPasteMessageSnackBarOnShowDetailsClicked(object sender, EventArgs e)
        {
            KWMessageBox.Show(permissionPasteMessage, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DataSourceListViewItemOnDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(e.Data.GetFormats()[0]) is ListViewItem item))
                return;

            if (!(((ListViewItem)sender).DataContext is TabularGroupDataSourceModel tabularGroupDataSource))
                return;

            if (draggingDataSources.Count == 1 && tabularGroupDataSource == draggingDataSources.First())
                return;

            if (!viewModel.AddDataSourcesToTabularGroup(draggingDataSources, tabularGroupDataSource))
            {
                OnlyEmlDataSourcesMessageSnackBar.Show();
            }

            e.Handled = true;
        }

        private void DataSourceListViewItemOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is IDataSource dataSource)
            {
                clickedElement = (DependencyObject)sender;
                startPoint = e.GetPosition(sender as ListViewItem);
                clickPoint = e.GetPosition(MainGrid);

                if (dataSource.IsSelected)
                {
                    draggingItem = new DraggingDataSourceUserControl
                    {
                        Visibility = Visibility.Hidden,
                        IsHitTestVisible = false,
                        IconCollection = new ObservableCollection<BitmapSource>(
                            viewModel.SelectedDataSourceCollection.Select(x => x.LargeIcon))
                    };

                    draggingDataSources = viewModel.SelectedDataSourceCollection.ToList();
                }
                else
                {
                    draggingItem = new DraggingDataSourceUserControl
                    {
                        Visibility = Visibility.Hidden,
                        IsHitTestVisible = false,
                        IconCollection = new ObservableCollection<BitmapSource>
                        {
                            dataSource.LargeIcon
                        }
                    };

                    draggingDataSources = new List<IDataSource>
                    {
                        dataSource
                    };
                }

                MainGrid.Children.Add(draggingItem);
                isDraggingStarted = true;
            }
        }

        private void DataSourceListViewItemOnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is IDataSource dataSource)
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                    Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    viewModel.DeselectAllDataSources();
                }
                dataSource.IsSelected = true;
            }
        }

        private void DataSourceListViewItemOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingStarted = false;
        }

        private void MainGrid_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!isDraggingStarted)
                    return;

                if (clickPoint == e.GetPosition(MainGrid))
                    return;

                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Panel.SetZIndex(draggingItem, MainGrid.Children.Count);
                    draggingItem.Visibility = Visibility.Visible;
                    mainScrollViewer = GetScroll();
                    GiveFeedback += DragSource_GiveFeedback;
                    DragDrop.DoDragDrop(clickedElement, clickedElement, DragDropEffects.Copy);
                    isDraggingStarted = false;
                    draggingItem.Visibility = Visibility.Hidden;
                    draggingItem.Margin = new Thickness(0);
                    MaxHeight = double.PositiveInfinity;
                    GiveFeedback -= DragSource_GiveFeedback;
                    MainGrid.Children.Remove(draggingItem);
                }
            }
            catch
            {
                // ignored
            }
        }

        private ScrollViewer GetScroll()
        {
            var template = DataSourcesContentControl.Template;
            var dataSourceListView = (ListView)template.FindName("DataSourceListView", DataSourcesContentControl);
            if (dataSourceListView != null)
            {
                return GetChildOfType<ScrollViewer>(dataSourceListView);
            }

            return null;
        }

        private void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                ListViewItem item = (ListViewItem)e.OriginalSource;
                System.Drawing.Point p = System.Windows.Forms.Control.MousePosition;
                Point myPos = PointToScreen(new Point(0, 0));
                Point mousePosition = new Point(p.X - myPos.X, p.Y - myPos.Y);
                double left = mousePosition.X - 40;
                double top = mousePosition.Y - 90;

                draggingItem.Margin = new Thickness(left, top, 0, 0);

                if (top + item.ActualHeight > mainScrollViewer.ActualHeight)
                {
                    mainScrollViewer.LineDown();
                    mainScrollViewer.LineDown();
                }
                if (top < mainScrollViewer.ContentVerticalOffset)
                {
                    mainScrollViewer.LineUp();
                }
            }
            finally
            {
            }
        }

        private void DataSourcesGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel?.SelectedDataSourceCollection?.Clear();
        }

        private void DefectiosButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingStarted = false;
        }

        private void DefectiosButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeselectAllListViewItems();
            ListView listViewInDefectionPupop = (ListView)((StackPanel)((PopupBox)((Button)sender).Content).PopupContent).Children[1];
            listViewInDefectionPupop.ItemsSource = ((IDataSource)((Button)sender).DataContext).DefectionMessageCollection;
            ListViewItem selectedItem = FindParent<ListViewItem>((Button)sender);
            selectedItem.IsSelected = true;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }

            return null;
        }

        private void DefectionsListItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedDefectionType = ((DefectionModel)((ListViewItem)sender).DataContext).DefectionType;

            if (SelectedDefectionType == DefectionType.NotValidMap)
            {
                ShowMappingWindow();

            }
            else if (SelectedDefectionType == DefectionType.NotValidGroup)
            {
                if (viewModel.SelectedDataSource != null)
                {
                    GroupDataSourcesWindow groupDataSourcesWindow = new GroupDataSourcesWindow(viewModel)
                    {
                        Owner = Window.GetWindow(this)
                    };

                    groupDataSourcesWindow.ShowDialog();
                }
                else
                {
                    KWMessageBox.Show("You must select data source and select defection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            else if (SelectedDefectionType == DefectionType.PreviewHasAnError)
            {
                PreviewHeaderRadioButton.IsChecked = true;
                SelectedDefectionType = DefectionType.None;
                e.Handled = true;
            }
            else if (SelectedDefectionType == DefectionType.None)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Commands

        public RelayCommand RemoveCommand { get; set; }

        private void RemoveSelectedDataSourcesCommandMethod(object obj)
        {
            RemoveSelectedDataSources();
        }

        private void ImportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Import(viewModel?.SelectedDataSourceCollection);
        }


        #endregion

        private void CardTemplateUserControl_MappingButtonClicked(object sender, EventArgs e)
        {
            DeselectAllListViewItems();
            ListViewItem selectedItem = FindParent<ListViewItem>((CardTemplateUserControl)sender);
            selectedItem.IsSelected = true;

            ShowMappingWindow();
        }

        private void CardTemplateUserControl_AclButtonClicked(object sender, EventArgs e)
        {
            DeselectAllListViewItems();
            ListViewItem selectedItem = FindParent<ListViewItem>((CardTemplateUserControl)sender);
            selectedItem.IsSelected = true;

            ShowPermissionWindow();
        }

        private void CardTemplateUserControl_DefectionsListItemMouseDown(object sender, EventArgs e)
        {
            SelectedDefectionType = ((DefectionModel)FindParent<ListViewItem>(((MouseButtonEventArgs)e).OriginalSource
                as DependencyObject).DataContext).DefectionType;

            if (SelectedDefectionType == DefectionType.NotValidMap)
            {
                ShowMappingWindow();
            }
            else if (SelectedDefectionType == DefectionType.NotValidGroup)
            {
                if (viewModel.SelectedDataSource != null)
                {
                    GroupDataSourcesWindow groupDataSourcesWindow = new GroupDataSourcesWindow(viewModel)
                    {
                        Owner = Window.GetWindow(this)
                    };

                    groupDataSourcesWindow.ShowDialog();
                }
                else
                {
                    KWMessageBox.Show("You must select data source and select defection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            else if (SelectedDefectionType == DefectionType.PreviewHasAnError)
            {
                PreviewHeaderRadioButton.IsChecked = true;
                SelectedDefectionType = DefectionType.None;
                ((MouseButtonEventArgs)e).Handled = true;
            }
            else if (SelectedDefectionType == DefectionType.None)
            {
                ((MouseButtonEventArgs)e).Handled = true;
            }
        }

        private void CardTemplateUserControl_DefectiosButtonPreviewMouseLeftButtonDown(object sender, EventArgs e)
        {
            DeselectAllListViewItems();
            ListView listViewInDefectionPupop = (ListView)((StackPanel)((PopupBox)((CardTemplateUserControl)sender).DefectiosButton.Content).PopupContent).Children[1];
            listViewInDefectionPupop.ItemsSource = ((IDataSource)((CardTemplateUserControl)sender).DataContext).DefectionMessageCollection;
            ListViewItem selectedItem = FindParent<ListViewItem>((CardTemplateUserControl)sender);
            selectedItem.IsSelected = true;
        }

        private void CardTemplateUserControl_DefectiosButtonMouseLeftButtonUp(object sender, EventArgs e)
        {
            isDraggingStarted = false;
        }

        public event EventHandler<ShowOnGraphRequestedEventArgs> ShowOnGraphRequested;
        protected void OnShowOnGraphRequested(ShowOnGraphRequestedEventArgs e)
        {
            ShowOnGraphRequested?.Invoke(this, e);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ((ContextMenu)sender).DataContext = DataContext;
        }
    }
}
