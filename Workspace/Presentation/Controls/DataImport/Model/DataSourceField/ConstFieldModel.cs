using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public class ConstFieldModel : DataSourceFieldModel
    {
        public ConstFieldModel()
        {
            Title = "Constant";
            Type = FieldType.Const;
        }
    }
}
