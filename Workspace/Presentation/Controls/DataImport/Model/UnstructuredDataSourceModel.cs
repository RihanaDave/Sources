using System.Windows.Media.Imaging;
using GPAS.Workspace.Logic;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public abstract class UnstructuredDataSourceModel : SingleDataSourceModel
    {
        protected UnstructuredDataSourceModel()
        {
            Type = "Unstructured";
        }

        protected override void SetIcon()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FileInfo?.FullPath))
                    LargeIcon = SmallIcon = null;
                else
                {
                    LargeIcon = FileInfo?.GetLargeThumbnail();
                    SmallIcon = FileInfo?.GetSmallThumbnail();
                }
            }
            catch
            {
                if (FileInfo?.Extension != null)
                {
                    BitmapImage icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(FileInfo.Extension.ToUpper()));
                    icon?.Freeze();
                    LargeIcon = SmallIcon = icon;
                }
            }
        }
    }
}
