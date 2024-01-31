using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for DataSourcePreviewUserControl.xaml
    /// </summary>
    public partial class DataSourcePreviewUserControl
    {
        #region Dependencies

        public IDataSource DataSource
        {
            get => (IDataSource)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(IDataSource), typeof(DataSourcePreviewUserControl),
                new PropertyMetadata(null, OnSetDataSourceChanged));

        private static void OnSetDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataSourcePreviewUserControl)d).OnSetDataSourceChanged(e);
        }

        private void OnSetDataSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DataSource is IPreviewableDataSource)
            {
                (DataSource as IPreviewableDataSource).PreviewChanged -= DataSourcePreviewUserControl_PreviewChanged;
                (DataSource as IPreviewableDataSource).PreviewChanged += DataSourcePreviewUserControl_PreviewChanged;
            }

            ClearAllControl();
            ShowPreview();
            OnDataSourceChanged(e);
        }

        private void DataSourcePreviewUserControl_PreviewChanged(object sender, EventArgs e)
        {
            ClearAllControl();
            ShowPreview();
        }

        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler DataSourceChanged;
        protected void OnDataSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            DataSourceChanged?.Invoke(this, e);
        }

        #endregion 

        public DataSourcePreviewUserControl()
        {
            InitializeComponent();
        }

        private void ClearAllControl()
        {
            ThumbnailImage.Source = null;
            DocumentPreviewControl.FilePath = string.Empty;
            StructuredDataSourceDataGrid.ItemsSource = null;
        }

        private void ShowPreview()
        {
            if (DataSource == null)
                return;

            if(DataSource is IPreviewableDataSource<DataTable> tablePreview)
            {
                ShowTabularPreview(tablePreview.Preview);
            }
            else if(DataSource is IPreviewableDataSource<string> stringPreview)
            {
                ShowTextDocumentPreview(stringPreview.Preview);
            }
            else if(DataSource is IPreviewableDataSource<ImageSource> imagePreview)
            {
                ShowMultimediaPreview(imagePreview.Preview);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ShowMultimediaPreview(ImageSource preview)
        {
            if (preview == null)
                return;

            ThumbnailImage.Source = preview;
        }

        private void ShowTextDocumentPreview(string preview)
        {
            if (preview == null)
                return;

            DocumentPreviewControl.FilePath = preview;
        }

        private void ShowTabularPreview(DataTable preview)
        {
            if (preview == null)
                return;

            StructuredDataSourceDataGrid.ItemsSource = preview.DefaultView;
        }

        private void StructuredDataSourceDataGrid_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            if (DataSource is ITabularDataSource tabularDataSource)
            {
                tabularDataSource.SelectedRow = tabularDataSource?.Preview?.Rows.OfType<DataRow>()
                    .FirstOrDefault(r => r.Equals((StructuredDataSourceDataGrid.SelectedItem as DataRowView)?.Row));
            }
        }
    }
}
