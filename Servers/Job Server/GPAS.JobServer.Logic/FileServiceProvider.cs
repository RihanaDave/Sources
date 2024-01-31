using GPAS.FtpServiceAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace GPAS.JobServer.Logic
{
    public static class FileServiceProvider
    {
        private static FtpServiceProvider GetNewFtpServiceInstance()
        {
            return new FtpServiceProvider
                (string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"])
                , ConfigurationManager.AppSettings["JobShareService_UserName"]
                , ConfigurationManager.AppSettings["JobShareService_Password"]);
        }

        /// <summary></summary>
        /// <param name="sourceFilePathOnJobShare"></param>
        /// <param name="importDataSourceTempPath"></param>
        /// <returns>Final Path</returns>
        public static string DownloadDataSource(string sourceFilePathOnJobShare, string importDataSourceTempPath)
        {
            FtpServiceProvider service = GetNewFtpServiceInstance();
            if (!Directory.Exists(importDataSourceTempPath))
                Directory.CreateDirectory(importDataSourceTempPath);
            var tempTargetPath = importDataSourceTempPath + Guid.NewGuid();
            Task.WaitAll(service.DownloadAsync(sourceFilePathOnJobShare, tempTargetPath));
            return tempTargetPath;
        }

        public static FileInfo[] DownloadDirectoryDataSource(string sourceFilePathOnJobShare, DirectoryInfo downloadTargetLocalDirectory)
        {
            if (!downloadTargetLocalDirectory.Exists)
                throw new ArgumentException("Target directory not exist or is not accessable");
            FtpServiceProvider service = GetNewFtpServiceInstance();
            Task<List<FileInfo>> downloadTask = service.DownloadDirectoryContent(sourceFilePathOnJobShare, downloadTargetLocalDirectory);
            Task.WaitAll(new Task[] { downloadTask });
            return downloadTask.Result.ToArray();
        }

        public static void CleanupDataSourceTempPath()
        {
            string importDataSourceTempPath = ConfigurationManager.AppSettings["ImportDataSourceTempPath"];
            DirectoryInfo tempDirInfo = new DirectoryInfo(importDataSourceTempPath);
            if (tempDirInfo.Exists)
            {
                DeleteDirectoryContent(tempDirInfo);
            }
        }

        private static void DeleteDirectoryContent(DirectoryInfo dirInfo)
        {
            if (!dirInfo.Exists)
                return;
            foreach (FileInfo subFileInfo in dirInfo.GetFiles())
            {
                File.Delete(subFileInfo.FullName);
            }
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                DeleteDirectoryContent(subDirInfo);
                Directory.Delete(subDirInfo.FullName, true);
            }
        }

        /// <summary></summary>
        /// <returns>Job-Share relative path</returns>
        public static string UploadFileToJobShare(string localPath)
        {
            FtpServiceProvider ftpService = GetNewFtpServiceInstance();

            var folderName = Guid.NewGuid().ToString();
            ftpService.MakeDirectory(folderName);

            var fi = new FileInfo(localPath);
            var fs = new FileStream(localPath, FileMode.Open);
            var jobSharePath = string.Format("{0}/{1}", folderName, fi.Name);
            ftpService.Upload(fs, fi.Length, jobSharePath);
            return jobSharePath;
        }

        public static void DeleteJobShareFolder(string jobShareDirectoryToRemove)
        {
            FtpServiceProvider ftpService = GetNewFtpServiceInstance();
            ftpService.DeleteDirectory(jobShareDirectoryToRemove);
        }

        public static void DeleteJobShareFile(string jobShareInUseFile)
        {
            FtpServiceProvider ftpService = GetNewFtpServiceInstance();
            ftpService.DeleteFile(jobShareInUseFile);
        }
    }
}
