using System;
using System.IO;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    [Serializable]
    public class LoadedFileModel : BaseModel
    {
        #region Properties

        string fileName = string.Empty;
        public string FileName
        {
            get => fileName;
            set => SetValue(ref fileName, value);
        }

        string fullPath = string.Empty;
        public string FullPath
        {
            get => fullPath;
            set
            {
                if (SetValue(ref fullPath, value))
                {
                    SetFileName();
                }
            }
        }

        DateTime dateUsed;
        public DateTime DateUsed
        {
            get => dateUsed;
            set
            {
                if (SetValue(ref dateUsed, value))
                {
                    OnDateUsedChanged();
                }
            }
        }

        bool pinned = false;
        public bool Pinned
        {
            get => pinned;
            set
            {
                if (SetValue(ref pinned, value))
                {
                    OnPinnedChanged();
                }
            }
        }

        #endregion

        #region Methods

        public LoadedFileModel()
        {

        }

        private void SetFileName()
        {
            if (FullPath == null)
                FileName = string.Empty;
            else
                FileName = FullPath.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar,
                    Path.PathSeparator, Path.VolumeSeparatorChar).LastOrDefault();
        }

        #endregion

        #region Events 

        public event EventHandler DateUsedChanged;
        protected void OnDateUsedChanged()
        {
            DateUsedChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PinnedChanged;
        protected void OnPinnedChanged()
        {
            PinnedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
