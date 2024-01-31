using GPAS.FileRepository.Logic;
using GPAS.FileRepository.Logic.Entities;
using GPAS.Logger;
using System;
using System.Collections.Generic;

namespace GPAS.FileRepository
{
    public class Service : IService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        #region Server Management
        public string test()
        {
            return "Server is Ready.";
        }
        public bool IsServiceAvailable()
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                bool IsDataSourceManagerAvailable = dsManager.IsStorageAvailable();
                var mediaManager = new MediaManager();
                bool IsMediaManagerAvailable = mediaManager.IsStorageAvailable();
                return IsDataSourceManagerAvailable && IsMediaManagerAvailable;
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void RemoveAllFiles()
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.RemoveAllFiles();
                var mediaManager = new MediaManager();
                mediaManager.RemoveAllFiles();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Medias
        /// <summary>
        /// این تابع لیست پوشه ها و فایل ها را در مسیر مورد نظر  بر می گرداند.
        /// </summary>
        /// <param name="path"> رشته مسیر را به عنوان ورودی دریافت می کند.</param>
        public List<DirectoryContent> GetMediaPathContent(string path)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.GetMediaPathContent(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        /// در مسیر مورد نظر یک پوشه ایجاد می کند.
        /// </summary>
        /// <param name="path"> رشته آدرس را از کاربر دریافت میکند</param>
        /// <returns> در صورتی که قبلا در مسیر مورد نظر پوشه وجود داشت مقدار False و در غیر این صورت پوشه را ایجاد و مقدار True را ارسال می کند.</returns>     
        public bool CreateMediaDirectory(string path)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.CreateMediaDirectory(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
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
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.RenameMediaDirectory(sourcePath, targetPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public bool DeleteMediaDirectory(string path)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.DeleteMediaDirectory(path);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        /// این تابع فایل را دریافت کرده و در مکان تعیین شده آپلود می کند.
        /// </summary>
        /// <param name="fileContent"> این پارامتر آرایه ای ز بایت های محتویات فایل را از سرویس‌گیرنده دریافت می کتند</param>
        /// <param name="fileName">این پارامتر نام فایل و پسنود ان را از سرویس‌گیرنده دریافت  می کند </param>
        /// <param name="targetPath">این پارامتر مسیری را که فایل در آنجا آپلود می شود را تعیین می کند </param>
        /// <returns> خروجی این فایل در صورتی که آپلود با موفقیت انجام بپذیرد مقدار TRUE را بر می گرداند و در صورتی فایل به دلیلی آپلود نشود مثدار False را بر می گرداند.</returns>
        public bool UploadMediaFile(byte[] fileContent, string fileName, string targetPath)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.UploadMediaFile(fileContent, fileName, targetPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        /// <summary>
        ///ین تابع مسیری فایلی را از ورودی دریافت کرده و آن را از هدوپ سرور دانلود کرده و فایل را به صورت رشته ای از بایت ها برای کاربر بر می گرداند. 
        /// </summary>
        /// <param name="mediaPath">این پارامتر مسیر فایلی را که قصد دانلود آن را داریم ار ورودی دریافت می کند</param>
        /// <returns> رشته از بایت های فایل درخواست شده کاربر را برای کاربر ارسال می کند./</returns>
        public byte[] DownloadMediaFile(string mediaPath)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.DownloadMediaFile(mediaPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public long GetMediaFileSizeInBytes(string mediaPath)
        {
            try
            {
                var mediaManager = new MediaManager();
                return mediaManager.GetMediaFileSizeInBytes(mediaPath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        #endregion

        #region Data-Sources & Documents

        public void UploadDocumentFileByName(string docName, byte[] docContent)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadDocumentFileByName(docName, docContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void UploadDocumentFile(long docID, byte[] docContent)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadDocumentFile(docID, docContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadDataSourceFile(dataSourceID, dataSourceContent);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadFileAsDocumentAndDataSource(byte[] fileToUpload, long docID, long dataSourceID)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadFileAsDocumentAndDataSource(fileToUpload, docID, dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDocumentFromJobShare(long docID, string docJobSharePath)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadDocumentFromJobShare(docID, docJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                dsManager.UploadDataSourceFromJobShare(dataSourceID, dataSourceJobSharePath);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public byte[] DownloadDocumentFile(long docID)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                return dsManager.DownloadDocumentFile(docID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public byte[] DownloadDataSourceFile(long dataSourceID)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                return dsManager.DownloadDataSourceFile(dataSourceID);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public byte[] DownloadDataSourceFileByName(string dataSourceName)
        {
            try
            {
                var dsManager = new DataSourceAndDocumentManager();
                return dsManager.DownloadDataSourceFile(dataSourceName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void IsAvailable()
        {
        }

        public long GetDataSourceAndDocumentFileSizeInBytes(string docId)
        {
            try
            {
                var dataSourceAndDocumentManagermediaManager = new DataSourceAndDocumentManager();
                return dataSourceAndDocumentManagermediaManager.GetDataSourceAndDocumentFileSizeInBytes(docId);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        #endregion
    }
}
