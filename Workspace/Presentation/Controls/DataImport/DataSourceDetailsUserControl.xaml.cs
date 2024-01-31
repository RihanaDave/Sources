using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for DataSourceDetailsUserControl.xaml
    /// </summary>
    public partial class DataSourceDetailsUserControl
    {
        #region Properties

        public IDataSource DataSource
        {
            get => (IDataSource)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(IDataSource), typeof(DataSourceDetailsUserControl),
                new PropertyMetadata(null, OnSetDataSourceChanged));

        private static void OnSetDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSourceDetailsUserControl)d).OnSetDataSourceChanged(e);
        }

        private void OnSetDataSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DataSource != null)
            {
                DataSource.MetaDataCollectionChanged += DataSource_MetaDataCollectionChanged;
                DataSource.MetaDataCollectionChanged += DataSource_MetaDataCollectionChanged;
            }

            ResetMetaDataCollection();
        }

        protected ObservableCollection<MetaDataItemModel> FileMetaDataCollection
        {
            get => (ObservableCollection<MetaDataItemModel>)GetValue(FileMetaDataCollectionProperty);
            set => SetValue(FileMetaDataCollectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for FileMetaDataCollection.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty FileMetaDataCollectionProperty =
            DependencyProperty.Register(nameof(FileMetaDataCollection), typeof(ObservableCollection<MetaDataItemModel>),
                typeof(DataSourceDetailsUserControl),
                new PropertyMetadata(null));

        protected ObservableCollection<MetaDataItemModel> DataSourceMetaDataCollection
        {
            get => (ObservableCollection<MetaDataItemModel>)GetValue(DataSourceMetaDataCollectionProperty);
            set => SetValue(DataSourceMetaDataCollectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for DataSourceMetaDataCollection.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty DataSourceMetaDataCollectionProperty =
            DependencyProperty.Register(nameof(DataSourceMetaDataCollection), typeof(ObservableCollection<MetaDataItemModel>), 
                typeof(DataSourceDetailsUserControl),
                new PropertyMetadata(null));

        #endregion

        #region Methods

        public DataSourceDetailsUserControl()
        {
            InitializeComponent();
        }

        private void DataSource_MetaDataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ResetMetaDataCollection();
        }

        private void ResetMetaDataCollection()
        {
            if (DataSource == null)
            {
                FileMetaDataCollection = null;
                DataSourceMetaDataCollection = null;
            }
            else
            {
                FileMetaDataCollection = new ObservableCollection<MetaDataItemModel>(DataSource.MetaDataCollection.Where(m => m.Type == MetaDataType.File));
                DataSourceMetaDataCollection = new ObservableCollection<MetaDataItemModel>(DataSource.MetaDataCollection.Where(m => m.Type == MetaDataType.DataSource));
            }
        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MaterialDesignThemes.Wpf.ButtonProgressAssist.SetIsIndicatorVisible(button, true);

            MetaDataItemModel metaDataItem = (MetaDataItemModel)button.DataContext;
            if (metaDataItem == null) return;
            RecalculateMetaDataItem(metaDataItem);
        }

        private async void RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            if (DataSource != null)
                await DataSource.RecalculateMetaDataItemAsync(metaDataItem);
        }

        #endregion
    }
}
