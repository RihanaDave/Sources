using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class FileInfoModel : BaseModel
    {
        #region Properties

        private string fullPath = string.Empty;
        public string FullPath
        {
            get => fullPath;
            set
            {
                if (SetValue(ref fullPath, value))
                {
                    Name = Path.GetFileName(FullPath);
                    NameWithoutExtension = Path.GetFileNameWithoutExtension(FullPath);
                    Extension = Path.GetExtension(FullPath)?.Substring(1);
                    if (IsExist())
                        Size = new FileInfo(FullPath).Length;
                    OnFullPathChanged();
                }
            }
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            protected set => SetValue(ref name, value);
        }

        private string nameWithoutExtension;
        public string NameWithoutExtension
        {
            get => nameWithoutExtension;
            protected set => SetValue(ref nameWithoutExtension, value);
        }

        private string extension = string.Empty;
        public string Extension
        {
            get => extension;
            protected set => SetValue(ref extension, value);
        }

        private long size = 0;
        public long Size
        {
            get => size;
            set
            {
                if (SetValue(ref size, value))
                {

                }
            }
        }

        #endregion

        #region Methods

        public bool IsExist()
        {
            return File.Exists(FullPath);
        }

        public bool IsEqualExtension(string extension)
        {
            return Extension.ToUpper().Equals(extension.ToUpper());
        }

        public FileStream ReadFileStream()
        {
            if (IsExist())
            {
                return File.OpenRead(FullPath);
            }
            else
            {
                throw new NotImplementedException($"{FullPath}");
            }
        }

        /// <summary>
        /// منظور از root یا همان ریشه در مسیر فایل
        /// نام درایو در مسیرهای ویندوزی،
        /// نام کامپیوتر در مسیرهای تحت شبکه است 
        /// در مسیرهای لینوکسی نیز همان جداکننده استاندارد مسیرهای فایل می باشد.
        /// </summary>
        /// <returns>ریشه مسیر فایل را برمی گرداند.</returns>
        public string GetRoot()
        {
            if (IsNetWorkPath())
                return GetComputerName();
            else if (IsLinuxPath())
                return GetLinuxRoot();
            else
                return GetDriveName();
        }

        public bool IsNetWorkPath()
        {
            return FullPath.StartsWith(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString());
        }

        private string GetComputerName()
        {
            if (IsNetWorkPath())
                return FullPath.Split(Path.DirectorySeparatorChar)[2];

            throw new Exception($"{FullPath} is not a network path.");
        }

        public bool IsLinuxPath()
        {
            if (FullPath.Length > 1)
                return FullPath[0].Equals(Path.DirectorySeparatorChar) && !FullPath[1].Equals(Path.DirectorySeparatorChar);
            else if (FullPath.Length > 0)
                return FullPath[0].Equals(Path.DirectorySeparatorChar);
            else
                return false;
        }

        public IEnumerable<MetaDataItemModel> GetMetaData()
        {
            if (IsExist())
            {
                ShellObject shellFile = ShellFile.FromParsingName(FullPath);
                return shellFile.Properties.DefaultPropertyCollection
                    .Where(p => p.ValueAsObject != null && !string.IsNullOrWhiteSpace(p.Description.DisplayName))
                    .Select(p => new MetaDataItemModel()
                    {
                        Title = p.Description.DisplayName,
                        Value = p.ValueAsObject,
                        Type = MetaDataType.File,
                        ShellMetaData = p
                    });
            }

            return null;
        }

        private string GetLinuxRoot()
        {
            if (IsLinuxPath())
                return Path.DirectorySeparatorChar.ToString();

            throw new Exception($"{FullPath} is not a Linux path.");
        }

        private string GetDriveName()
        {
            return FullPath.Split(Path.DirectorySeparatorChar)[0];
        }

        public List<string> GetDirectories()
        {
            string fp = FullPath;
            if (IsNetWorkPath())
                fp = fp.Substring(2);

            if (IsLinuxPath())
                fp = fp.Substring(1);

            List<string> result = Path.GetDirectoryName(fp).Split(Path.DirectorySeparatorChar).ToList();
            if (result.Count > 0)
                result.RemoveAt(0);

            return result;
        }

        public BitmapSource GetLargeThumbnail()
        {
            if (IsExist())
            {
                BitmapSource largeThumbnail = ShellFile.FromFilePath(FullPath).Thumbnail.ExtraLargeBitmapSource;
                largeThumbnail?.Freeze();
                if (largeThumbnail == null)
                    throw new Exception("File thumbnail image not found!");
                else
                    return largeThumbnail;
            }
            else
            {
                throw new FileNotFoundException("File not found!", FullPath);
            }
        }

        public BitmapSource GetSmallThumbnail()
        {
            if (IsExist())
            {
                BitmapSource smallThumbnail = ShellFile.FromFilePath(FullPath).Thumbnail.SmallBitmapSource;
                smallThumbnail?.Freeze();
                if (smallThumbnail == null)
                    throw new Exception("File thumbnail image not found!");
                else
                    return smallThumbnail;
            }
            else
            {
                throw new FileNotFoundException("File not found!", FullPath);
            }
        }

        #endregion

        #region Events

        public event EventHandler FullPathChanged;
        protected void OnFullPathChanged()
        {
            FullPathChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}