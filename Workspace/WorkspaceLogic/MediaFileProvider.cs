using GPAS.DataImport.Publish;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    public class MediaFileProvider
    {
        public async static Task<List<MediaPathContent>> GetMediaPathContentAsync(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("directory");

            DirectoryContent[] remoteDirectories = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                remoteDirectories = (await sc.GetMediaPathContentAsync(directory));
            }
            finally
            {
                sc.Close();
            }

            if (remoteDirectories == null)
                throw new NullReferenceException("Invalid server Response.");

            return GetContentsFromRemoteContents(remoteDirectories);
        }
        public static List<MediaPathContent> GetMediaPathContent(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("directory");

            DirectoryContent[] remoteDirectories = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                remoteDirectories = sc.GetMediaPathContent(directory);
            }
            finally
            {
                sc.Close();
            }

            if (remoteDirectories == null)
                throw new NullReferenceException("Invalid server Response.");

            return GetContentsFromRemoteContents(remoteDirectories);
        }
        private static List<MediaPathContent> GetContentsFromRemoteContents(DirectoryContent[] remoteDirectories)
        {
            List<MediaPathContent> directoryResultList = new List<MediaPathContent>();
            foreach (var item in remoteDirectories)
            {
                MediaPathContent newDirectory = new MediaPathContent();
                newDirectory.Type = GetContentTypeFromRemoteContentType(item.ContentType);
                newDirectory.UriAddress = item.UriAddress;
                newDirectory.DisplayName = item.DisplayName;
                directoryResultList.Add(newDirectory);
            }
            return directoryResultList;
        }
        private static MediaPathContentType GetContentTypeFromRemoteContentType(DirectoryContentType contentType)
        {
            switch (contentType)
            {
                case DirectoryContentType.Directory:
                    return MediaPathContentType.Directory;
                case DirectoryContentType.File:
                    return MediaPathContentType.File;
                default:
                    throw new NotSupportedException("Unknown 'DirectoryContentType'");
            }
        }

        /// <summary>
        /// عملکرد درخواست ایجاد یک پوشه در سرور داده های غیر ساخت یافته
        /// </summary>
        /// <param name="directoryName">نام پوشه</param>
        /// <param name="directoryToCreate">مسیر ایجاد پوشه در سرور</param>
        /// <returns></returns>
        public async static Task<bool> CreateMediaDirectoryAsync(string directoryName, string directoryToCreate)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName");

            if (string.IsNullOrEmpty(directoryToCreate))
                throw new ArgumentNullException("directoryToCreate");

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            string directory = directoryToCreate + "/" + directoryName;
            bool result = false;
            try
            {
                result = await sc.CreateMediaDirectoryAsync(directory);
            }
            finally
            {
                sc.Close();
            }

            return result;
        }
        
        /// <summary>
        /// عملکرد آپلود یک فایل روی سرور داده های غیر ساخت یافته
        /// </summary>
        public async static Task<MediaPathContent> UploadMediaFileAsync(byte[] fileByte, string fileName, string destinationDirectory)
        {
            if (fileByte.Length == 0)
                throw new ArgumentException(nameof(fileByte));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrWhiteSpace(destinationDirectory))
                throw new ArgumentNullException(nameof(destinationDirectory));

            string uriAddress = destinationDirectory + "/" + fileName;
            bool result = false;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                result = await sc.UploadMediaFileAsync(fileByte, fileName, destinationDirectory);
            }
            finally
            {
                sc.Close();
            }
            if (result)
            {
                MediaPathContent newDirectory = new MediaPathContent();
                newDirectory.DisplayName = fileName;
                newDirectory.UriAddress = uriAddress;
                newDirectory.Type = MediaPathContentType.File;
                return newDirectory;
            }
            else
            {
                throw new InvalidOperationException("The file path already exist in file-repository!");
            }
        }
        
        /// <summary>
        /// عملکرد دانلود یک فایل از روی سرور داده های غیر ساخت یافته
        /// </summary>
        /// <param name="mediaPath"></param>
        /// <param name="localTargetPath"></param>
        /// <returns></returns>
        public async static Task DownloadMediaFile(string mediaPath, string localTargetPath)
        {
            if (string.IsNullOrEmpty(mediaPath))
                throw new ArgumentNullException("mediaPath");
            if (string.IsNullOrWhiteSpace(mediaPath))
                throw new ArgumentException("mediaPath");
            if (string.IsNullOrEmpty(localTargetPath))
                throw new ArgumentNullException("localTargetPath");
            if (string.IsNullOrWhiteSpace(localTargetPath))
                throw new ArgumentException("localTargetPath");

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                var fi = new FileInfo(localTargetPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                if (fi.Exists)
                    return;

                byte[] docContent = await sc.DownloadMediaFileAsync(mediaPath);
                if (docContent != null)
                {

                    FileStream fs = fi.OpenWrite();
                    fs.Write(docContent, 0, docContent.Length);
                    fs.Close();
                    fi.IsReadOnly = false;
                    File.SetAttributes(localTargetPath,
                        FileAttributes.Normal
                    );
                }
            }
            finally
            {
                sc.Close();
            }
        }
    }
}
