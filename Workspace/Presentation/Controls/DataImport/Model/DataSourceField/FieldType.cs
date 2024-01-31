using System;
using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public enum FieldType
    {
        [Description("None")]
        None,
        [Description("Columns")]
        Tabular,
        [Description("Path Base")]
        Path,
        [Description("Meta Data")]
        MetaData,
        [Description("Constant Value")]
        Const,
    }
}
