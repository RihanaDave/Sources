using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class TabularGroupDataSourceModel : GroupDataSourceModel, ITabularDataSource
    {
        #region Properties

        DataTable preview;
        public DataTable Preview
        {
            get => preview;
            set
            {
                if (SetValue(ref preview, value))
                {
                    SelectedRow = null;
                    ResetTableFieldCollection();
                    ResetDataSourceMetaDataCollection();
                    SetDefaultSelectedRow();

                    OnPreviewChanged();
                }
            }
        }

        DataRow selectedRow;
        public DataRow SelectedRow
        {
            get => selectedRow;
            set
            {
                DataRow old = selectedRow;
                if (SetValue(ref selectedRow, value))
                {
                    OnSetSelectedRowChanged(old, value);
                }
            }
        }

        int selectedRowIndex = -1;
        public int SelectedRowIndex
        {
            get => selectedRowIndex;
            set
            {
                int old = selectedRowIndex;
                if (SetValue(ref selectedRowIndex, value))
                {
                    OnSetSelectedRowIndexChanged(old, value);
                }
            }
        }

        MetaDataItemModel columnCountMetaDataItem = null;
        public MetaDataItemModel ColumnCountMetaDataItem
        {
            get => columnCountMetaDataItem;
            protected set => SetValue(ref columnCountMetaDataItem, value);
        }

        MetaDataItemModel rowCountMetaDataItem = null;
        public MetaDataItemModel RowCountMetaDataItem
        {
            get => rowCountMetaDataItem;
            protected set => SetValue(ref rowCountMetaDataItem, value);
        }

        #endregion

        #region Methods

        public TabularGroupDataSourceModel()
        {
            ColumnCountMetaDataItem = new MetaDataItemModel()
            {
                OwnerDataSource = this,
                Title = "Number of columns",
                Value = double.NaN,
                NeedsRecalculation = true,
                Type = MetaDataType.DataSource,
            };

            RowCountMetaDataItem = new MetaDataItemModel()
            {
                OwnerDataSource = this,
                Title = "Number of rows",
                Value = double.NaN,
                NeedsRecalculation = true,
                Type = MetaDataType.DataSource,
            };
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                Preview = JoinDataTables(DataSourceCollection.OfType<IPreviewableDataSource<DataTable>>()?.Select(pds => pds.Preview),
                    Properties.Settings.Default.ImportPreview_MaximumSampleRows);
            }
            else
            {
                Preview = null;
            }
        }

        private DataTable JoinDataTables(IEnumerable<DataTable> dataTables, int numberOfRows)
        {
            if (dataTables == null)
                return null;

            List<DataTable> dataTableList = dataTables.ToList();
            if (dataTableList.Count == 0)
                return null;

            DataTable joinedDataTable = new DataTable();
            foreach (DataColumn col in dataTableList[0].Columns)
            {
                joinedDataTable.Columns.Add(col.ColumnName);
            }

            foreach (DataRow row in dataTableList.SelectMany(dt => dt.Rows.OfType<DataRow>()))
            {
                joinedDataTable.Rows.Add(row.ItemArray);
                if (joinedDataTable.Rows.Count >= numberOfRows)
                    break;
            }

            return joinedDataTable;
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = base.PrepareDefections();
            if (!IsMatchedDataSources())
            {
                defection.Add(new DefectionModel
                {
                    Message = "The columns of the data sources table in the group are not compatible",
                    DefectionType = DefectionType.NotValidGroup
                });
            }
            return defection;
        }

        protected override bool GetValidation()
        {
            return base.GetValidation() && IsMatchedDataSources();
        }

        protected override bool CanGeneratePreview()
        {
            return base.CanGeneratePreview() && IsMatchedDataSources();
        }

        private bool IsMatchedDataSources()
        {
            if (DataSourceCollection == null || DataSourceCollection.Count == 0)
                return false;

            List<DataTable> tables = DataSourceCollection.OfType<IPreviewableDataSource<DataTable>>()?.Select(pds => pds.Preview).ToList();

            for (int i = 0; i < tables.Count; i++)
            {
                DataTable dt1 = tables[i];
                for (int j = i; j < tables.Count; j++)
                {
                    DataTable dt2 = tables[j];
                    if (dt1 != dt2 && !DataImportUtility.IsMatchTables(dt1, dt2))
                        return false;
                }
            }

            return true;
        }

        private void SetDefaultSelectedRow()
        {
            if (preview == null || preview.Rows.Count == 0)
            {
                SelectedRow = null;
            }
            else
            {
                SelectedRow = Preview.Rows[0];
            }
        }

        private void OnSetSelectedRowChanged(DataRow oldValue, DataRow newValue)
        {
            if (selectedRow == null)
                SelectedRowIndex = -1;
            else
                SelectedRowIndex = preview.Rows.IndexOf(selectedRow);

            UpdateSampleValuesForTableFields();
        }

        private void OnSetSelectedRowIndexChanged(int oldValue, int newValue)
        {
            if (selectedRowIndex < 0)
                SelectedRow = null;
            else
                SelectedRow = preview.Rows[selectedRowIndex];
        }

        private void UpdateSampleValuesForTableFields()
        {
            List<TableFieldModel> tableFields = FieldCollection.OfType<TableFieldModel>().OrderBy(f => f.ColumnIndex).ToList();
            foreach (TableFieldModel tableField in tableFields)
            {
                tableField.SampleValue = SelectedRow == null ? string.Empty : SelectedRow[tableField.ColumnIndex].ToString();
            }
        }

        private void ResetTableFieldCollection()
        {
            RemoveTableFields();
            AddTableFields();
        }

        private void RemoveTableFields()
        {
            if (FieldCollection == null)
                return;

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
                FieldCollection.Except(FieldCollection.OfType< TableFieldModel>())
            );
        }

        private void AddTableFields()
        {
            if (Preview == null)
                return;

            List<TableFieldModel> tableFieldModels = new List<TableFieldModel>();
            foreach (DataColumn col in Preview.Columns)
            {
                tableFieldModels.Add(new TableFieldModel()
                {
                    Title = col.ColumnName,
                    ColumnIndex = col.Ordinal,
                    SampleValue = SelectedRow == null ? string.Empty : SelectedRow[col.Ordinal].ToString()
                });
            }

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
               FieldCollection.Concat(tableFieldModels)
            );
        }

        protected override IEnumerable<MetaDataItemModel> GetDataSourceMetaData()
        {
            List<MetaDataItemModel> result = base.GetDataSourceMetaData().ToList();
            result.Add(CreateNumberOfColumnMetaData());
            result.Add(CreateNumberOfRowsMetaData());

            return result;
        }

        private MetaDataItemModel CreateNumberOfColumnMetaData()
        {
            if (ColumnCountMetaDataItem != null)
            {
                ColumnCountMetaDataItem.NeedsRecalculation = false;
                ColumnCountMetaDataItem.Value = Preview?.Columns == null ? 0 : Preview.Columns.Count;
            }

            return ColumnCountMetaDataItem;
        }

        private MetaDataItemModel CreateNumberOfRowsMetaData()
        {
            if (RowCountMetaDataItem != null && RowCountMetaDataItem.Value.ToString() == double.NaN.ToString())
            {
                RowCountMetaDataItem.NeedsRecalculation = GetNeedsRecalculationForRowCount();
                RowCountMetaDataItem.Value = GetAllRowsCount();
            }

            return RowCountMetaDataItem;
        }

        private long GetAllRowsCount()
        {
            return DataSourceCollection.OfType<ITabularDataSource>().Sum(ds => (long)ds.RowCountMetaDataItem.Value);
        }

        private bool GetNeedsRecalculationForRowCount()
        {
            return DataSourceCollection.OfType<ITabularDataSource>().Any(ds => ds.RowCountMetaDataItem.NeedsRecalculation);
        }

        public override bool CanImportWorkSpaceSide()
        {
            return DataImportUtility.TabularDataSourceCanImportWorkSpaceSide(this);
        }

        protected override object RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            if (RowCountMetaDataItem.Equals(metaDataItem))
            {
                if (!RowCountMetaDataItem.NeedsRecalculation)
                    return RowCountMetaDataItem.Value;

                if (DataSourceCollection == null || DataSourceCollection.Count == 0)
                    return 0;

                IEnumerable<ITabularDataSource> allDataSourcesWithRowNeedReclac = 
                    DataSourceCollection.OfType<ITabularDataSource>().Where(ds => ds.RowCountMetaDataItem.NeedsRecalculation);
                foreach (ITabularDataSource ds in allDataSourcesWithRowNeedReclac)
                {
                    ds.RecalculateMetaDataItemAsync(ds.RowCountMetaDataItem);
                }

                return GetAllRowsCount();
            }

            return base.RecalculateMetaDataItem(metaDataItem);
        }

        #endregion
    }
}
