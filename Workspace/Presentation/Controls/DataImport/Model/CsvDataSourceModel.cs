using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class CsvDataSourceModel : StructuredDataSourceModel
    {
        GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();

        char separator = ',';
        public char Separator
        {
            get => separator;
            set
            {
                if (CanSeparatorChange())
                {
                    if (SetValue(ref separator, value))
                    {
                        RegeneratePreview();
                    }
                }
                else if (value != Separator)
                {
                    throw new NotSupportedException("The csv separator cannot be changed because there are properties in the mapping of this data source whose values include tabular fields.");
                }
            }
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = base.PrepareDefections();
            if (!FileExtensionIsCSV())
            {
                defection.Add(new DefectionModel
                {
                    Message = "The file extension is not CSV",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defection;
        }
        protected override bool GetValidation()
        {
            return base.GetValidation() && FileExtensionIsCSV();
        }

        protected override bool CanGeneratePreview()
        {
            return base.CanGeneratePreview() && FileExtensionIsCSV();
        }

        private bool FileExtensionIsCSV()
        {
            return FileInfo.IsEqualExtension("CSV");
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                FileStream fileStream = null;
                DataTable tempPreview = null;

                try
                {
                    fileStream = FileInfo.ReadFileStream();
                    tempPreview = util.GenerateDataTableFromCsvContentStream(fileStream, separator,
                        Properties.Settings.Default.ImportPreview_MaximumSampleRows, FileInfo.FullPath, 3000);
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }

                Preview = tempPreview;
            }
            else
            {
                Preview = null;
            }
        }

        protected override long GetAllRowsCount()
        {
            return GetAllRowsCount(true, Properties.Settings.Default.WorkspaceSideImport_MaximumImportingObjects);
        }

        protected override long GetAllRowsCount(bool hasLimit, int limit)
        {
            if (IsValid)
                return util.GetRowCountFromCSV(FileInfo.FullPath, HasHeader, hasLimit, limit);

            return 0;
        }

        protected override bool GetNeedsRecalculationForRowCount()
        {
            if (IsValid)
                return util.IsBigCSV(FileInfo.FullPath, HasHeader, Properties.Settings.Default.WorkspaceSideImport_MaximumImportingObjects);

            return false;
        }

        protected override object RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            if (RowCountMetaDataItem.Equals(metaDataItem))
                return GetAllRowsCount(false, -1);

            return base.RecalculateMetaDataItem(metaDataItem);
        }

        public bool CanSeparatorChange()
        {
            IEnumerable<ValueMapModel> allTabularValues = Map?.ObjectCollection?.SelectMany(o => o.GetAllProperties())?.
                SelectMany(p => p?.GetAllValues())?.Where(v => v?.Field is TableFieldModel);

            return allTabularValues == null || allTabularValues.Count() == 0;
        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("CsvFileImage");
        }
    }
}
