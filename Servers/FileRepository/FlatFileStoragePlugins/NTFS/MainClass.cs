using GPAS.FileRepository.Logic.FlatFileStorage;
using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
//using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace GPAS.FileRepository.FlatFileStoragePlugins.NTFS
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static string RootPath = "";

        public MainClass()
        {
            if (!string.IsNullOrEmpty(RootPath))
                return;

            RootPath = ConfigurationManager.AppSettings["GPAS.FileRepository.FlatFileStoragePlugins.NTFS.RootPath"];

            if (string.IsNullOrEmpty(RootPath))
                throw new ConfigurationErrorsException(Properties.Resources.String_AppSettingIsNotDefined);
            if (!IsAvailable())
                throw new DirectoryNotFoundException(Properties.Resources.String_RootPathNotAccessible);
        }

        public void CreateDirectory(string directoryIdentifier)
        {
            if (string.IsNullOrEmpty(directoryIdentifier))
                throw new ArgumentNullException(nameof(directoryIdentifier));

            string path = Path.Combine(RootPath, directoryIdentifier);

            Directory.CreateDirectory(path);
        }

        public bool IsAvailable()
        {
            if (!Directory.Exists(RootPath))
                return false;

            var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(RootPath);
            if (accessControlList == null)
                throw new NullReferenceException(Properties.Resources.String_DirectorySecurity);

            var accessRules = accessControlList.GetAccessRules(true, true,
                typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                throw new Exception(Properties.Resources.String_AuthorizationRuleCollection);

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }

        public byte[] LoadFile(string fileIdentifier, string directoryIdentifier)
        {
            if (string.IsNullOrEmpty(directoryIdentifier))
                throw new ArgumentNullException(nameof(directoryIdentifier));
            if (string.IsNullOrEmpty(fileIdentifier))
                throw new ArgumentNullException(nameof(fileIdentifier));

            string path = Path.Combine(RootPath, directoryIdentifier, fileIdentifier);

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = File.ReadAllBytes(path);
                fileStream.Read(bytes, 0, Convert.ToInt32(fileStream.Length));
                fileStream.Close();

                return bytes;
            }
        }

        public void RemoveDirectories(string[] directoryIDs)
        {
            if (directoryIDs.Length == 0)
                throw new ArgumentOutOfRangeException();

            foreach (var directoryId in directoryIDs)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(RootPath, directoryId));
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (DirectoryNotFoundException)
                { }
            }
        }

        public void SaveFile(byte[] fileContent, string fileIdentifier, string directoryIdentifier)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));
            if (string.IsNullOrEmpty(directoryIdentifier))
                throw new ArgumentNullException(nameof(directoryIdentifier));
            if (string.IsNullOrEmpty(fileIdentifier))
                throw new ArgumentNullException(nameof(fileIdentifier));

            string path = Path.Combine(RootPath, directoryIdentifier, fileIdentifier);

            if (!File.Exists(path))
            {
                using (File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read)) { }
            }

            File.WriteAllBytes(path, fileContent);
        }

        public void SaveFileInTwoPathes(byte[] fileContent, string firstFileID, string firstDirectoryID, string secondFileID, string secondDirectoryID)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));
            if (string.IsNullOrEmpty(firstFileID))
                throw new ArgumentNullException(nameof(firstFileID));
            if (string.IsNullOrEmpty(firstDirectoryID))
                throw new ArgumentNullException(nameof(firstDirectoryID));
            if (string.IsNullOrEmpty(secondFileID))
                throw new ArgumentNullException(nameof(secondFileID));
            if (string.IsNullOrEmpty(secondDirectoryID))
                throw new ArgumentNullException(nameof(secondDirectoryID));
            
            SaveFile(fileContent, firstFileID, firstDirectoryID);

            // روش هارد لینک به خاطر خطای شناسایی شده در تست‌های خودکار موقتا غیرفعال شد و 
            // ذخیره‌سازی مجدد فایل در مسیر دوم جایگزین آن شد؛
            // شرح خطا: در برخی موارد که تعدادی درخواست به صورت همزمان به این تابع فرستاده
            // می‌شد، محتوای برخی فایل‌های هارد لینک شده، با محتوای فایل اصلی یکسان نبود

            //string firstPath = Path.Combine(RootPath, firstDirectoryID, firstFileID);
            //string secondPath = Path.Combine(RootPath, secondDirectoryID, secondFileID);
            //lock (this)
            //{
            //    bool linkCreationResult = CreateHardLink(secondPath, firstPath, IntPtr.Zero);
            //    if(linkCreationResult == false)
            //    {
            //        int errorCode = Marshal.GetLastWin32Error();
            //        throw new IOException($"Error hard link creation with code '{errorCode}'", errorCode);
            //    }
            //}

            SaveFile(fileContent, secondFileID, secondDirectoryID);
        }

        //[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        //static extern bool CreateHardLink(
        //    string newLinkPath,
        //    string sourcePath,
        //    IntPtr securityAttributes
        //);

        public long GetFileSizeInBytes(string fileIdentifier, string directoryIdentifier)
        {
            if (string.IsNullOrEmpty(fileIdentifier))
                throw new ArgumentNullException(nameof(fileIdentifier));
            if (string.IsNullOrEmpty(directoryIdentifier))
                throw new ArgumentNullException(nameof(directoryIdentifier));
            
            string path = Path.Combine(RootPath, directoryIdentifier, fileIdentifier);

            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("FileNotExist", path));

            FileInfo file = new FileInfo(path);
            return file.Length;
        }
    }
}
