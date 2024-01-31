using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public class TableFieldModel : DataSourceFieldModel
    {
        int columnIndex = -1;
        public int ColumnIndex
        {
            get => columnIndex;
            set => SetValue(ref columnIndex, value);
        }

        public TableFieldModel()
        {
            Type = FieldType.Tabular;
        }
    }
}
