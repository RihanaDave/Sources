using GPAS.Histogram;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for DataSourceHistogramUserControl.xaml
    /// </summary>
    public partial class DataSourceHistogramUserControl
    {
        #region Dependencies

        public IDataSource DataSource
        {
            get => (IDataSource)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(IDataSource),
            typeof(DataSourceHistogramUserControl), new PropertyMetadata(null, OnSetDataSourceChanged));



        protected ObservableCollection<HistogramItem> Items
        {
            get { return (ObservableCollection<HistogramItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<HistogramItem>), typeof(DataSourceHistogramUserControl),
                new PropertyMetadata(null));



        #endregion

        #region Methodes

        public DataSourceHistogramUserControl()
        {
            InitializeComponent();
            Items = new ObservableCollection<HistogramItem>();
        }

        private static void OnSetDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSourceHistogramUserControl)d).OnSetDataSourceChanged(e);
        }

        private async void OnSetDataSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IDataSource oldDataSource)
            {
                oldDataSource.ImportingObjectCollectionChanged -= DataSource_ImportingObjectCollectionChanged;
                oldDataSource.IsCompletedGenerationImportingObjectCollectionChanged -= DataSource_IsCompletedGenerationImportingObjectCollectionChanged;

                if (oldDataSource.ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Importing ||
                    oldDataSource.ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.InQueue ||
                    oldDataSource.ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Transforming ||
                    oldDataSource.ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Publishing )
                    oldDataSource.ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;
            }

            if (DataSource != null)
            {
                DataSource.ImportingObjectCollectionChanged -= DataSource_ImportingObjectCollectionChanged;
                DataSource.ImportingObjectCollectionChanged += DataSource_ImportingObjectCollectionChanged;

                DataSource.IsCompletedGenerationImportingObjectCollectionChanged -= DataSource_IsCompletedGenerationImportingObjectCollectionChanged;
                DataSource.IsCompletedGenerationImportingObjectCollectionChanged += DataSource_IsCompletedGenerationImportingObjectCollectionChanged;

                await GenerateImportingObjectForDataSource();
            }

            UpdateHistogram();
        }

        private async void DataSource_IsCompletedGenerationImportingObjectCollectionChanged(object sender, EventArgs e)
        {
            await GenerateImportingObjectForDataSource();
        }

        private async Task GenerateImportingObjectForDataSource()
        {
            try
            {
                MainWaitingControl.TaskIncrement();

                if (DataSource.ImportingObjectCollectionGenerationStatus != DataSourceImportStatus.Ready)
                    return;

                DataSource.ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Importing;

                if (DataSource.Map == null || !DataSource.Map.IsValid)
                {
                    DataSource.ImportingObjectCollection?.Clear();
                }
                else
                {
                    //create importing concepts
                }

                if (DataSource != null)
                    DataSource.ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Completed;
            }
            catch
            {
                if (DataSource != null)
                    DataSource.ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;
                throw;
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        private void DataSource_ImportingObjectCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateHistogram();
        }

        private void UpdateHistogram()
        {
            Items.Clear();
            if (DataSource != null)
            {
                FillHistogram();
            }
        }

        private void FillHistogram()
        {
            if (DataSource.ImportingObjectCollection == null)
                return;

            List<ImportingProperty> allImportingProperties = new List<ImportingProperty>();

            foreach (ImportingObject importingObject in DataSource.ImportingObjectCollection)
            {
                allImportingProperties.AddRange(importingObject.Properties);
            }

            IEnumerable<IGrouping<string, ImportingProperty>> propertiesList = allImportingProperties.GroupBy(x => x.TypeUri);
            IEnumerable<IGrouping<string, ImportingObject>> objectsList = DataSource.ImportingObjectCollection.ToList().GroupBy(x => x.TypeUri);

            HistogramItem objectsBranch = new HistogramItem
            {
                Title = "Object"
            };

            foreach (IGrouping<string, ImportingObject> objectItem in objectsList)
            {
                HistogramItem item = new HistogramItem
                {
                    Title = objectItem.First().UserFriendlyName,
                    Icon = new BitmapImage(new Uri(objectItem.First().IconPath)),
                    Value = objectItem.Count()
                };

                objectsBranch.Items.Add(item);
            }

            HistogramItem propertiesBranch = new HistogramItem
            {
                Title = "Property"
            };

            foreach (IGrouping<string, ImportingProperty> propertyItem in propertiesList)
            {
                HistogramItem item = new HistogramItem
                {
                    Title = propertyItem.First().Title,
                    Icon = new BitmapImage(new Uri(propertyItem.First().IconPath)),
                    Value = propertyItem.Count()
                };

                propertiesBranch.Items.Add(item);
            }

            Items.Add(objectsBranch);
            Items.Add(propertiesBranch);
        }

        public void Reset()
        {
            DataSourcesHistogram.Reset();
        }

        #endregion
    }
}
