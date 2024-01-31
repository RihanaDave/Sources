using GPAS.FileRepository.Logic.Entities;
using System.Collections.Generic;

namespace GPAS.FileRepository.Logic.FlatFileStorage
{
    /// <summary>
    /// Flat File Storage
    /// یک نگهدارنده‌ی فایل است که برای نگهداری فایل‌ها، فقط یک پوشه درنظر می‌گیرد
    /// و فاقد ساختار درختی است؛ برای ذخیره یا بازیابی هر فایل باید پوشه‌ی و شناسه‌ی
    /// آن مشخص شود
    /// </summary>
    /// <remarks>
    /// توصیه می‌شود خواندن تنظیمات، یک بار و در سازنده‌ی کلاس پیاده‌سازی شده انجام گیرد
    /// </remarks>
    public interface IAccessClient
    {
        /// <summary>
        /// تمام فایل‌ها و پوشه‌بندی‌ها را حذف می‌کند
        /// </summary>
        void RemoveDirectories(params string[] directoryIDs);
        /// <summary>
        /// در دسترس بودن و ممکن بودن ذخیره/بازیابی زیرساخت مورد استفاده را بررسی می‌کند
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// در مسیر مورد نظر یک پوشه جدید ایجاد می کند
        /// </summary>
        void CreateDirectory(string directoryIdentifier);
        /// <summary>
        /// محتوای تعیین شده برای فایل را با نام و مسیر مشخص شده ذخیره‌سازی می‌کند
        /// </summary>
        void SaveFile(byte[] fileContent, string fileIdentifier, string directoryIdentifier);
        /// <summary>
        /// محتوای یکسان تعیین شده برای فایل را با نام‌ها و مسیرهای مشخص شده ذخیره‌سازی می‌کند
        /// </summary>
        /// <remarks>
        /// در پیاده‌سازی این تابع، در صورت پشتیبانی زیرساخت مورد استفاده، برای
        /// صرفه‌جویی در فضای ذخیره‌سازی، از ذخیره‌سازی رونوشت‌های مختلف خودداری کرده
        /// و آدرس(های) جدید را به فایل انتساب دهید
        /// </remarks>
        void SaveFileInTwoPathes(byte[] fileContent
            , string firstFileID, string firstDirectoryID
            , string secondFileID, string secondDirectoryID);
        /// <summary>
        /// محتوای فایل ذخیره شده در مسیر را بازیابی می‌کند
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        byte[] LoadFile(string fileIdentifier, string directoryIdentifier);
        /// <summary>
        /// حجم اشغال شده برای ذخیره‌سازی فایل را برحسب بایت برمی‌گرداند 
        /// </summary>
        long GetFileSizeInBytes(string fileIdentifier, string directoryIdentifier);
    }
}
