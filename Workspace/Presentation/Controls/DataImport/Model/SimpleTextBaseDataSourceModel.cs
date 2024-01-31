namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class SimpleTextBaseDataSourceModel : TextBaseDataSourceModel, IPreviewableDataSource<string>
    {
        string preview;
        public string Preview
        {
            get => preview;
            set
            {
                if (SetValue(ref preview, value))
                {
                    OnPreviewChanged();
                }
            }
        }

        public override void RegeneratePreview()
        {
            if (CanGeneratePreview())
                Preview = FileInfo.FullPath;
            else
                Preview = null;
        }
    }
}
