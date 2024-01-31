using System.Collections.ObjectModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class AccessDataSourceModel : StructuredDataSourceModel
    {
        GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defections = base.PrepareDefections();
            if (!FileExtensionIsMsAccess())
            {
                defections.Add(new DefectionModel
                {
                    Message = "The file extension is not MS Access",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defections;
        }
        protected override bool GetValidation()
        {
            return base.GetValidation() && FileExtensionIsMsAccess() && !string.IsNullOrWhiteSpace(Title);
        }

        protected override bool CanGeneratePreview()
        {
            return base.CanGeneratePreview() && FileExtensionIsMsAccess() && !string.IsNullOrWhiteSpace(Title);
        }

        private bool FileExtensionIsMsAccess()
        {
            return (FileInfo.IsEqualExtension("MDB") || FileInfo.IsEqualExtension("ACCDB"));
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                Preview = util.GetDataTableFromAccessFile(FileInfo.FullPath, Title, Properties.Settings.Default.ImportPreview_MaximumSampleRows, true, HasHeader);
            }
            else
            {
                Preview = null;
            }
        }

        protected override long GetAllRowsCount()
        {
            if (IsValid)
                return util.GetNumberOfRowAccessFile(FileInfo.FullPath, Title);

            return 0;
        }

        protected override long GetAllRowsCount(bool hasLimit, int limit)
        {
            return GetAllRowsCount();
        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("AccessTableImage");
        }
    }
}
