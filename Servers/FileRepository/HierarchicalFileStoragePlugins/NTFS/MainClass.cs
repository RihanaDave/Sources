using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.HierarchicalFileStorage;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Security.AccessControl;

namespace GPAS.FileRepository.HierarchicalFileStoragePlugins.NTFS
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static string RootPath = "";

        public MainClass()
        {
            if (!string.IsNullOrEmpty(RootPath))
                return;

            RootPath = ConfigurationManager.AppSettings["GPAS.FileRepository.HierarchicalFileStoragePlugins.NTFS.RootPath"];

            if (string.IsNullOrEmpty(RootPath))
                throw new ConfigurationErrorsException(Properties.Resources.String_AppSettingIsNotDefined);
            if (!IsAvailable())
                throw new DirectoryNotFoundException(Properties.Resources.String_RootPathNotAccessible);
        }

        public bool CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            string folderPath = Path.Combine(RootPath, path);

            if (Directory.Exists(folderPath))
                return false;

            Directory.CreateDirectory(folderPath);

            return true;
        }

        public bool DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            string folderPath = Path.Combine(RootPath, path);

            if (!Directory.Exists(folderPath))
                return false;

            Directory.Delete(folderPath, true);

            return true;
        }

        public List<DirectoryContent> GetDirectoryContent(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            string folderPath = Path.Combine(RootPath, path);
            List<DirectoryContent> directoryContents = new List<DirectoryContent>();

            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                directoryContents.Add(new DirectoryContent
                {
                    ContentType = DirectoryContentType.File,
                    DisplayName = file.Name,
                    UriAddress = file.FullName.Replace(RootPath + "\\", "")
                });
            }

            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                directoryContents.Add(new DirectoryContent
                {
                    ContentType = DirectoryContentType.Directory,
                    DisplayName = directory.Name,
                    UriAddress = directory.FullName.Replace(RootPath + "\\", "")
                });
            }

            return directoryContents;
        }

        public long GetFileSizeInBytes(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string path = Path.Combine(RootPath, filePath);

            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format(Properties.Resources.String_FileNotExist, path));

            FileInfo file = new FileInfo(path);
            return file.Length;
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

        public byte[] LoadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string path = Path.Combine(RootPath, filePath);

            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = File.ReadAllBytes(path);
                fileStream.Read(bytes, 0, Convert.ToInt32(fileStream.Length));
                fileStream.Close();

                return bytes;
            }
        }

        public void RemoveAllFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(RootPath);

            try
            {
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                {
                    directory.Delete(true);
                }
            }
            catch (DirectoryNotFoundException)
            { }
            catch (FileNotFoundException)
            { }

        }

        public bool RenameDirectory(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            if (!Directory.Exists(Path.Combine(RootPath, sourcePath)))
                throw new DirectoryNotFoundException();

            Directory.Move(Path.Combine(RootPath, sourcePath), Path.Combine(RootPath, targetPath));

            return true;
        }

        public void SaveFile(byte[] fileContent, string fileName, string targetPath)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            string path = Path.Combine(RootPath, targetPath, fileName);

            if (!File.Exists(path))
            {
                using (File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read)) { }
            }

            File.WriteAllBytes(path, fileContent);
        }
    }
}
