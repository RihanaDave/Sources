using GPAS.FileRepository.Logic.Entities;
using System.Collections.Generic;

namespace GPAS.FileRepository.Logic.HierarchicalFileStorage
{
    /// <summary>
    /// Hierarchical File Storage
    /// یک نگهدارنده‌ی فایل است که دارای ساختار درختی است و برای آدرس‌دهی مسیرها
    /// می‌توان از سلسله‌پوشه‌های تودرتویی که با اسلش از یکدیگر جدا شده‌اند استفاده کرد
    /// </summary>
    /// <remarks>
    /// توصیه می‌شود خواندن تنظیمات، یک بار و در سازنده‌ی کلاس پیاده‌سازی شده انجام گیرد
    /// </remarks>
    public interface IAccessClient
    {
        /// <summary>
        /// تمام فایل‌ها و پوشه‌بندی‌ها را حذف می‌کند
        /// </summary>
        void RemoveAllFiles();
        /// <summary>
        /// در دسترس بودن و ممکن بودن ذخیره/بازیابی زیرساخت مورد استفاده را بررسی می‌کند
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// در مسیر مورد نظر یک پوشه جدید ایجاد می کند
        /// </summary>
        bool CreateDirectory(string path);
        /// <summary>
        /// لیست پوشه‌ها و فایل‌ها موجود در مسیر مورد نظر  را برمی‌گرداند
        /// </summary>
        List<DirectoryContent> GetDirectoryContent(string path);
        /// <summary>
        /// پوشه‌ی تعیین شده در مسیر را حذف می‌کند
        /// </summary>
        /// <returns></returns>
        bool DeleteDirectory(string path);
        /// <summary>
        /// این تابع پوشه مورد نظر را به مکان تعیین شده انتقال می دهد و در صورت یکسان بودن مسیر، نام آن را تغییر می‌دهد
        /// </summary>
        /// <param name="sourcePath">این پارامتر پوشه مورد نظر برای تغییر را دریافت می کند.</param>
        /// <param name="targetPath">این پارامتر پوشه مقصد را برای انتقال/تغییر نام دریافت می کند.</param>
        bool RenameDirectory(string sourcePath, string targetPath);
        /// <summary>
        /// محتوای تعیین شده برای فایل را با نام و مسیر مشخص شده ذخیره‌سازی می‌کند
        /// </summary>
        void SaveFile(byte[] fileContent, string fileName, string targetPath);
        /// <summary>
        /// محتوای فایل ذخیره شده در مسیر را بازیابی می‌کند
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        byte[] LoadFile(string filePath);
        /// <summary>
        /// حجم اشغال شده برای ذخیره‌سازی فایل را برحسب بایت برمی‌گرداند 
        /// </summary>
        long GetFileSizeInBytes(string filePath);
    }
}
