using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public class PathPartFieldModel : DataSourceFieldModel
    {
        PathPartType partType = PathPartType.None;
        public PathPartType PartType
        {
            get => partType;
            set => SetValue(ref partType, value);
        }

        int directoryIndex = -1;
        public int DirectoryIndex
        {
            get => directoryIndex;
            set => SetValue(ref directoryIndex, value);
        }

        public PathPartFieldModel()
        {
            Type = FieldType.Path;
        }
    }
}
