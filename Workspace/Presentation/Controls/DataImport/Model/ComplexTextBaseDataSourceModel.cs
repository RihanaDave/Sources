using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class ComplexTextBaseDataSourceModel : TextBaseDataSourceModel, IPreviewableDataSource<ImageSource>
    {
        ImageSource preview;

        public ImageSource Preview
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
                Preview = LargeIcon;
            else
                Preview = null;
        }
    }
}
