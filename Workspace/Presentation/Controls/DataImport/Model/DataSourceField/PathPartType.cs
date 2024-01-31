using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public enum PathPartType
    {
        None,
        Root,
        Directory,
        Extension,
        FileName,
        FullPath
    }
}