using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    public partial class GroupDataSourcesWindow
    {
        #region Properties

        private readonly DataImportViewModel viewModel;
        private readonly GroupDataSourceModel currentGroupDataSource;

        public ObservableCollection<GroupDataSourceModel> OtherGroupsCollection
        {
            get => (ObservableCollection<GroupDataSourceModel>)GetValue(OtherGroupsCollectionProperty);
            set => SetValue(OtherGroupsCollectionProperty, value);
        }

        public static readonly DependencyProperty OtherGroupsCollectionProperty =
            DependencyProperty.Register(nameof(OtherGroupsCollection),
                typeof(ObservableCollection<GroupDataSourceModel>), typeof(DataImportViewModel));

        public DefectionType SelectedDefectionType
        {
            get { return (DefectionType)GetValue(SelectedDefectionTypeProperty); }
            set { SetValue(SelectedDefectionTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDefectionType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDefectionTypeProperty =
            DependencyProperty.Register(nameof(SelectedDefectionType), typeof(DefectionType), typeof(GroupDataSourcesWindow),
                new PropertyMetadata(DefectionType.None));

        #endregion

        #region Methods

        public GroupDataSourcesWindow(DataImportViewModel dataImportViewModel)
        {
            InitializeComponent();
            viewModel = dataImportViewModel;
            currentGroupDataSource = viewModel.SelectedDataSource as GroupDataSourceModel;
            OtherGroupsCollection = new ObservableCollection<GroupDataSourceModel>();
            RemoveCommand = new RelayCommand(RemoveSelectedDataSourcesCommandMethod);
        }

        private void ViewPopUpMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            var radioButton = (RadioButton)((Grid)sender).Children[1];
            radioButton.IsChecked = true;
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void FillGroupsComboBox()
        {
            if (OtherGroupsCollection.Count != 0)
                OtherGroupsCollection.Clear();

            var groupsDataSources = viewModel.DataSourceCollection.OfType<GroupDataSourceModel>().Cast<IDataSource>().ToList();

            IEnumerable<IDataSource> matchedGroup = new List<IDataSource>();
            if (currentGroupDataSource is ITabularDataSource currentTabularGroup)
                matchedGroup = DataImportUtility.GetMatchDataSourcesWithTabularDataSource(groupsDataSources, currentTabularGroup);

            foreach (var dataSource in matchedGroup)
            {
                if (dataSource is GroupDataSourceModel groupDataSourceModel)
                {
                    OtherGroupsCollection.Add(groupDataSourceModel);
                }
            }

            OtherGroupsCollection.Remove(viewModel.SelectedDataSource as GroupDataSourceModel);
        }

        private void GroupDataSourcesWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            FillGroupsComboBox();
            DataContext = currentGroupDataSource;
        }

        private void GroupDataSourcesWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            DataContext = null;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedDataSources();
        }

        private void UnGroupButton_OnClick(object sender, RoutedEventArgs e)
        {
            currentGroupDataSource.UngroupDataSources(currentGroupDataSource.SelectedDataSources);
        }

        private void MoveToButton_OnClick(object sender, RoutedEventArgs e)
        {
            currentGroupDataSource.MoveDataSources(currentGroupDataSource.SelectedDataSources,
                GroupsComboBox.SelectedItem as GroupDataSourceModel);
        }

        private void RemoveSelectedDataSources()
        {
            var result = KWMessageBox.Show(Properties.Resources.StringRemoveDataSources, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                currentGroupDataSource.RemoveRangeDataSources(currentGroupDataSource.SelectedDataSources);
        }

        private void DataSourceDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {

                foreach (var item in e.AddedItems)
                {
                    if (!(item is DefectionModel))
                    {
                        ((IDataSource)item).IsSelected = true;
                    }
                }
            }

            if (e.RemovedItems.Count != 0)
            {
                foreach (var item in e.RemovedItems)
                {
                    if (!(item is DefectionModel))
                    {
                        ((IDataSource)item).IsSelected = false;
                    }
                }
            }
        }

        private void DefectiosButton_Click(object sender, RoutedEventArgs e)
        {
            ListView listViewInDefectionPupop = ((ListView)((StackPanel)(((PopupBox)((Button)sender).Content).PopupContent)).Children[1]);
            listViewInDefectionPupop.ItemsSource = ((IDataSource)((Button)sender).DataContext).DefectionMessageCollection;
        }
        private void DefectionsListItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedDefectionType = ((DefectionModel)((ListViewItem)sender).DataContext).DefectionType;
            if (SelectedDefectionType == DefectionType.NotValidMap)
            {
                //   ShowMappingWindow();

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
            }
        }

        #endregion

        #region Commands

        public RelayCommand RemoveCommand { get; set; }

        private void RemoveSelectedDataSourcesCommandMethod(object obj)
        {
            RemoveSelectedDataSources();
        }


        #endregion

    }
}
