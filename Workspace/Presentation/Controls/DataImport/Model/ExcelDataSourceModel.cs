using System.Collections.ObjectModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class ExcelDataSourceModel : StructuredDataSourceModel
    {
        GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();

        protected override bool GetValidation()
        {
            return base.GetValidation() && FileExtensionIsMsExcel();
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = base.PrepareDefections();
            if (!FileExtensionIsMsExcel())
            {
                defection.Add(new DefectionModel
                {
                    Message = "The file extension is not MS Excel",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defection;
        }
        protected override bool CanGeneratePreview()
        {
            return base.CanGeneratePreview() && FileExtensionIsMsExcel();
        }

        private bool FileExtensionIsMsExcel()
        {
            return FileInfo.IsEqualExtension("XLSX");
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                Preview = util.GetDataTableFromExcel(FileInfo.FullPath, Title, Properties.Settings.Default.ImportPreview_MaximumSampleRows, true, HasHeader);
            }
            else
            {
                Preview = null;
            }
        }

        protected override long GetAllRowsCount()
        {
            if (IsValid)
                return util.GetRowCountFromExcel(FileInfo.FullPath, Title, HasHeader);

            return 0;
        }

        protected override long GetAllRowsCount(bool hasLimit, int limit)
        {
            return GetAllRowsCount();
        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("ExcelSheetImage");
        }
    }
}
