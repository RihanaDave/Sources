using GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class EmlDataSourceModel : SemiStructuredDataSourceModel
    {
        GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();
        Convertor convertor = new Convertor();
        public static readonly string emlDirectoryPath = $"{ConfigurationManager.AppSettings["WorkspaceTempFolderPath"]}{"Eml Files Attachments"}";

        public EmlDataSourceModel()
        {
            convertor.TargetDirectoryPath = emlDirectoryPath;
            convertor.SourceFiles = new FileInfo[1];
            convertor.AttachmentsPathPrefix = emlDirectoryPath;
        }
        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = base.PrepareDefections() ;
            if (!FileExtensionIsEML())
            {
                defection.Add(new DefectionModel
                {
                    Message = "The file extension is not EML",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defection;
        }
        protected override bool GetValidation()
        {
            return base.GetValidation() && FileExtensionIsEML();
        }

        protected override bool CanGeneratePreview()
        {
            return base.CanGeneratePreview() && FileExtensionIsEML();
        }

        private bool FileExtensionIsEML()
        {
            return FileInfo.IsEqualExtension("EML");
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
            {
                convertor.SourceFiles[0] = new FileInfo(FileInfo.FullPath);
                string[][] previewMatrix = convertor.PerformConversionToInMemoryMatrix(Properties.Settings.Default.ImportPreview_MaximumSampleRows);

                Preview = util.GenerateDataTableFromStringArray(previewMatrix, "", "");
            }
            else
            {
                Preview = null;
            }
        }

        protected override long GetAllRowsCount()
        {
            return convertor.GetRowCountFromExtractedTable();
        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("EmlFileImage");
        }
    }
}
