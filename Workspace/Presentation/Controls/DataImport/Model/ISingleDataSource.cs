namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public interface ISingleDataSource : IDataSource
    {
        FileInfoModel FileInfo { get; set; }
    }
}
