using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for DisplayDataBasesWindow.xaml
    /// </summary>
    public partial class DisplayDataBasesWindow : Window
    {
        DataImportViewModel dataImportViewModel = null;
        DatabaseViewModel databaseViewModel = new DatabaseViewModel();
       

        public ObservableCollection<DatabaseModel> DatabaseCollection
        {
            get { return (ObservableCollection<DatabaseModel>)GetValue(DatabaseCollectionProperty); }
            set { SetValue(DatabaseCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DatabaseCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DatabaseCollectionProperty =
            DependencyProperty.Register(nameof(DatabaseCollection), typeof(ObservableCollection<DatabaseModel>),
                typeof(DisplayDataBasesWindow), new PropertyMetadata(null));

        public DefectionType SelectedDefectionType
        {
            get { return (DefectionType)GetValue(SelectedDefectionTypeProperty); }
            set { SetValue(SelectedDefectionTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedDefectionType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDefectionTypeProperty =
            DependencyProperty.Register(nameof(SelectedDefectionType), typeof(DefectionType), typeof(DisplayDataBasesWindow),
                new PropertyMetadata(DefectionType.None));

        
        public DisplayDataBasesWindow(DataImportViewModel viewModel)
        {
            InitializeComponent();
            DataContext = databaseViewModel;
            dataImportViewModel = viewModel;
        }

        private async void ReloadDatabasesButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadDatabases();

            SelectFirstDataSource();
            databaseViewModel.UncheckedAllCheckedTablesAndViews();
        }
        
        private async Task ReloadDatabases()
        {
            if (!(dataImportViewModel is DataImportViewModel))
                return;

            try
            {
                MainWaitingControl.Message = "Loading databases";
                MainWaitingControl.TaskIncrement();
                await databaseViewModel.ReloadTotalTablesAndViewsInDatabases();
                dataImportViewModel.SetDatabasesList(databaseViewModel.DatabaseCollection);
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
                MainWaitingControl.Message = string.Empty;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataImportViewModel.DatabasesList == null)
                await ReloadDatabases();
            else
                databaseViewModel.DatabaseCollection = new ObservableCollection<DatabaseModel>(dataImportViewModel.DatabasesList);

            SelectFirstDataSource();
            databaseViewModel.UncheckedAllCheckedTablesAndViews();
        }

        private void SelectFirstDataSource()
        {
            if (databaseViewModel.DatabaseCollection?.Count > 0)
            {
                databaseViewModel.DatabaseCollection[0].IsSelected = false;
                databaseViewModel.DatabaseCollection[0].IsSelected = true;
            }
        }

        private void TablesAndViewsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                foreach (SQLServerDataSourceModel dataSource in e.AddedItems.OfType<SQLServerDataSourceModel>())
                {
                    dataSource.IsSelected = true;
                }
            }

            if (e.RemovedItems?.Count > 0)
            {
                foreach (SQLServerDataSourceModel dataSource in e.RemovedItems.OfType<SQLServerDataSourceModel>())
                {
                    dataSource.IsSelected = false;
                }
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {     
         
            SQLServerDataSourceModel dataSource = (SQLServerDataSourceModel)((DataGridRow)sender)?.DataContext;
            dataSource.IsChecked = !dataSource.IsChecked;
        }

        private async void AddAllCheckedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWaitingControl.Message = "Adding datasources";
                MainWaitingControl.TaskIncrement();
                await dataImportViewModel.AddTablesAndViewsToDataSources(databaseViewModel.CheckedTablesAndViews);
            }
            finally
            {
                databaseViewModel.UncheckedAllCheckedTablesAndViews();
                MainWaitingControl.TaskDecrement();
                MainWaitingControl.Message = string.Empty;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            databaseViewModel.UncheckedAllCheckedTablesAndViews();
        }

        private void DefectiosButton_Click(object sender, RoutedEventArgs e)
        {
            ListView listViewInDefectionPupop = ((ListView)((StackPanel)(((PopupBox)((Button)sender).Content).PopupContent)).Children[1]);
            ObservableCollection<DefectionModel> defections= ((IDataSource)((Button)sender).DataContext).DefectionMessageCollection;
            defections.Remove(defections.Where( ds => ds.DefectionType == DefectionType.NotValidMap).SingleOrDefault());
            listViewInDefectionPupop.ItemsSource = defections;
        }

        private void DefectionsListItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedDefectionType = ((DefectionModel)((ListViewItem)sender).DataContext).DefectionType;
            SelectedDefectionType = DefectionType.None;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
