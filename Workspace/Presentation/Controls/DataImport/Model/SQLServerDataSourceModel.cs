using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System.Collections.ObjectModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class SQLServerDataSourceModel : StructuredDataSourceModel
    {
        public SQLServerDataSourceModel()
        {

        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("Table");
        }

        public async override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                Preview = await DataImportUtility.GetTableOrViewFromDatabase(Title, FileInfo.FullPath);
            }
            else
            {
                Preview = null;
            }
        }

        protected override bool CanGeneratePreview()
        {
            return FileInfo != null && !string.IsNullOrWhiteSpace(FileInfo.FullPath) && !string.IsNullOrWhiteSpace(Title);
        }
        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = base.PrepareDefections();
            if (FileInfo == null || string.IsNullOrWhiteSpace(FileInfo.FullPath) || string.IsNullOrWhiteSpace(Title))
            {
                defection.Add(new DefectionModel
                {
                    Message = "Data source table or view is not available",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            if (Preview?.Columns == null || Preview.Columns.Count == 0)
            {
                defection.Add(new DefectionModel
                {
                    Message = "There are no columns in the data source table",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            if (Preview?.Rows == null || Preview.Rows.Count == 0)
            {
                defection.Add(new DefectionModel
                {
                    Message = "There are no rows in the data source table",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }

            return defection;
        }
        protected override bool GetValidation()
        {
            return FileInfo != null && !string.IsNullOrWhiteSpace(FileInfo.FullPath) && !string.IsNullOrWhiteSpace(Title) &&
                Preview?.Columns?.Count > 0 && Preview?.Rows?.Count > 0;
        }

        protected override long GetAllRowsCount()
        {
            if (IsValid)
                return Preview.Rows.Count;

            return -1;
        }

        protected override bool GetNeedsRecalculationForRowCount()
        {
            if (IsValid)
                return Preview.Rows.Count >= Properties.Settings.Default.ImportPreview_MaximumSampleRows;

            return false;
        }

        protected override object RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            if (RowCountMetaDataItem.Equals(metaDataItem))
                return "Unknown";

            return base.RecalculateMetaDataItem(metaDataItem);
        }

        protected override void AfterTitleChanged()
        {
            RegeneratePreview();
        }

        public override bool CanImportWorkSpaceSide()
        {
            return false;
        }
    }
}
