using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public class SavedMapModel : BaseModel
    {
        private string name;
        public string Name
        {
            get => name;
            set => SetValue(ref name, value);
        }

        private string shortcutPath;
        public string ShortcutPath
        {
            get => shortcutPath;
            set => SetValue(ref shortcutPath, value);
        }

        private string targetPath;
        public string TargetPath
        {
            get => targetPath;
            set => SetValue(ref targetPath, value);
        }

        private DateTime lastAccessTime;
        public DateTime LastAccessTime
        {
            get => lastAccessTime;
            set => SetValue(ref lastAccessTime, value);
        }
    }
}
