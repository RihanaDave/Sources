using GPAS.Dispatch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using FileRepository = GPAS.Dispatch.ServiceAccess.FileRepositoryService;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس مدیرت درخواست به سمت هدوپ سرور را انجام می دهد
    /// </summary>
    public class MediaProvider
    {
        /// <summary>
        /// این تابع لیست پوشه ها و فایل ها را در مسیر مورد نظر  بر می گرداند.
        /// </summary>
        /// <param name="path"> رشته مسیر را به عنوان ورودی دریافت می کند.</param>
        /// <returns> لیستی از DirectoryContent را بر می گرداند.</returns>
        public List<DirectoryContent> GetMediaPathContent(string path)
        {
            FileRepository.ServiceClient proxy = null;
            List<FileRepository.DirectoryContent> remotePathContents = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                remotePathContents = proxy.GetMediaPathContent(path).ToList();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
            return GetContentFromFileRepositoryContent(remotePathContents);
        }

        private List<DirectoryContent> GetContentFromFileRepositoryContent(List<FileRepository.DirectoryContent> pathContents)
        {
            return pathContents.Select(c => GetContentFromFileRepositoryContent(c)).ToList();
        }

        private DirectoryContent GetContentFromFileRepositoryContent(FileRepository.DirectoryContent pathContent)
        {
            return new DirectoryContent()
            {
                DisplayName = pathContent.DisplayName,
                ContentType = GetContentTypeFromFileRepositoryContentType(pathContent.ContentType),
                UriAddress = pathContent.UriAddress
            };
        }

        private DirectoryContentType GetContentTypeFromFileRepositoryContentType(FileRepository.DirectoryContentType contentType)
        {
            switch (contentType)
            {
                case FileRepository.DirectoryContentType.Directory:
                    return Entities.DirectoryContentType.Directory;
                case FileRepository.DirectoryContentType.File:
                    return Entities.DirectoryContentType.File;
                default:
                    throw new NotSupportedException("Unknown 'FileRepositoryPathContentType'");
            }
        }

        public bool DeleteMediaDirectory(string path)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                bool deleteDirectoryCheck = proxy.DeleteMediaDirectory(path);
                return deleteDirectoryCheck;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        /// <summary>
        /// در مسیر مورد نظر یک پوشه ایجاد می کند.
        /// </summary>
        /// <param name="path"> رشته آدرس را از کاربر دریافت میکند</param>
        /// <returns> در صورتی که قبلا در مسیر مورد نظر پوشه وجود داشت مقدار False و در غیر این صورت پوشه را ایجاد و مقدار True را ارسال می کند.</returns>
        public bool CreateMediaDirectory(string path)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                bool createDirectoryCheck = proxy.CreateMediaDirectory(path);
                return createDirectoryCheck;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        /// <summary>
        /// این تابع پوشه مورد نظر را به مکان با اسم مورد انتقال می دهد.
        /// </summary>
        /// <param name="sourcePath">این پارامتر پوشه مورد نظر برای تغییر را دریافت می کند.</param>
        /// <param name="targetPath">این پارامتر پوشه مقصد را برای انتقال و تغییر نام دریافت می کند.</param>
        /// <returns></returns>
        public bool RenameMediaDirectory(string sourcePath, string targetPath)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                bool renameDirectoryCheck = proxy.RenameMediaDirectory(sourcePath, targetPath);
                return renameDirectoryCheck;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        /// <summary>
        ///ین تابع مسیری فایلی را از ورودی دریافت کرده و آن را از سرور فایل دانلود کرده و فایل را به صورت رشته ای از بایت ها برای کاربر بر می گرداند. 
        /// </summary>
        /// <param name="sourcePath">این پارامتر مسیر فایلی را که قصد دانلود آن را داریم ار ورودی دریافت می کند</param>
        /// <returns> رشته از بایت های فایل درخواست شده کاربر را برای کاربر ارسال می کند./</returns>
        public byte[] DownloadMediaFile(string sourcePath)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                byte[] resultFile = proxy.DownloadMediaFile(sourcePath);
                return resultFile;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        /// <summary>
        /// این تابع فایل را دریافت کرده و در مکان تعیین شده آپلود می کند.
        /// </summary>
        /// <param name="fileToUpload"> این پارامتر رشته ای ز بایت های تابع را از کاربر دریافت می کتند</param>
        /// <param name="fileName">این پارامتر نام فایل و پسوند ان را از کاربر دریافت  می کند </param>
        /// <param name="targetPath">این پارامتر مسیری را که فایل در آنجا آپلوذ می شود را تعیین می کند </param>
        /// <returns> خروجی این فایل در صورتی که آپلود با موفقیت انجام بپذیرد مقدار TRUE را بر می گرداند و در صورتی فایل به دلیلی آپلود نشود مثدار False را بر می گرداند.</returns>
        public bool UploadMediaFile(byte[] fileToUpload, string fileName, string targetPath)
        {
            FileRepository.ServiceClient proxy = null;
            try
            {
                proxy = new FileRepository.ServiceClient();
                bool renameDirectoryCheck = proxy.UploadMediaFile(fileToUpload, fileName, targetPath);
                return renameDirectoryCheck;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}