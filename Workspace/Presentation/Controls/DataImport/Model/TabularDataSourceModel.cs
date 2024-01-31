using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public abstract class TabularDataSourceModel : SingleDataSourceModel, ITabularDataSource
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
                    IsValid = GetValidation();
                    DefectionMessageCollection = PrepareDefections();
                    SelectedRow = null;
                    ResetTableFieldCollection();
                    ResetDataSourceMetaDataCollection();
                    SetDefaultSelectedRow();

                    Preview.ColumnChanged -= Preview_ColumnChanged;
                    Preview.ColumnChanged += Preview_ColumnChanged;

                    OnPreviewChanged();
                }
            }
        }

        private void Preview_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            IsValid = GetValidation();
            DefectionMessageCollection = PrepareDefections();
        }

        bool hasHeader = true;
        public bool HasHeader
        {
            get => hasHeader;
            set => SetValue(ref hasHeader, value);
        }

        DataRow selectedRow;
        public DataRow SelectedRow
        {
            get => selectedRow;
            set
            {
                if (SetValue(ref selectedRow, value))
                {
                    OnSetSelectedRowChanged();
                }
            }
        }

        int selectedRowIndex = -1;
        public int SelectedRowIndex
        {
            get => selectedRowIndex;
            set
            {
                if (SetValue(ref selectedRowIndex, value))
                {
                    OnSetSelectedRowIndexChanged();
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

        public TabularDataSourceModel()
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

        private void OnSetSelectedRowChanged()
        {
            if (selectedRow == null)
                SelectedRowIndex = -1;
            else
                SelectedRowIndex = preview.Rows.IndexOf(selectedRow);

            UpdateSampleValuesForTableFields();
        }

        private void OnSetSelectedRowIndexChanged()
        {
            if (selectedRowIndex < 0)
                SelectedRow = null;
            else
                SelectedRow = preview.Rows[selectedRowIndex];
        }

        public override void RegeneratePreview()
        {
            throw new NotImplementedException(string.Format("{0}", FileInfo.FullPath));
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
                FieldCollection.Except(FieldCollection.OfType<TableFieldModel>())
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

        private void UpdateSampleValuesForTableFields()
        {
            List<TableFieldModel> tableFields = FieldCollection.OfType<TableFieldModel>().OrderBy(f => f.ColumnIndex).ToList();
            foreach (TableFieldModel tableField in tableFields)
            {
                tableField.SampleValue = SelectedRow == null ? string.Empty : SelectedRow[tableField.ColumnIndex].ToString();
            }
        }

        protected override void ResetFieldCollection()
        {
            base.ResetFieldCollection();
            ResetTableFieldCollection();
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

        protected virtual long GetAllRowsCount()
        {
            throw new NotImplementedException();
        }

        protected virtual long GetAllRowsCount(bool hasLimit, int limit)
        {
            throw new NotImplementedException();
        }

        protected virtual bool GetNeedsRecalculationForRowCount()
        {
            return false;
        }

        public override bool CanImportWorkSpaceSide()
        {
            return DataImportUtility.TabularDataSourceCanImportWorkSpaceSide(this);
        }

        protected override bool GetValidation()
        {
            return base.GetValidation() &&
                Preview?.Columns?.Count > 0 &&
                Preview?.Rows?.Count > 0;
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defections = base.PrepareDefections();
            if (Preview?.Columns == null || Preview.Columns.Count == 0)
            {
                defections.Add(new DefectionModel
                {
                    Message = "There are no columns in the data source table",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            if (Preview?.Rows == null || Preview.Rows.Count == 0)
            {
                defections.Add(new DefectionModel
                {
                    Message = "There are no rows in the data source table",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defections;
        }
        #endregion

        #region Events

        #endregion
    }
}
