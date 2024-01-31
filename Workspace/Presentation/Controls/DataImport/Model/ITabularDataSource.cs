using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public interface ITabularDataSource : IDataSource, IPreviewableDataSource<DataTable>
    {
        DataRow SelectedRow { get; set; }
        int SelectedRowIndex { get; set; }
        MetaDataItemModel ColumnCountMetaDataItem { get; }
        MetaDataItemModel RowCountMetaDataItem { get; }
    }
}
