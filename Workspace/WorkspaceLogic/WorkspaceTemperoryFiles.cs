using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// مدیریت عملکردهای مرتبط با فشرده سازی
    /// </summary>
    public class WorkspaceTemperoryFiles
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private WorkspaceTemperoryFiles()
        { }

        /// <summary>
        /// آدرس یک پوشه را دریافت و فایل فشرده متناسب با آن را ایجاد می کند
        /// </summary>
        /// <remarks>نوع فشرده سازی، فشرده سازی «زیپ» می باشد و این عملکرد برای فشرده سازی از عملکردهای «دات نت» استفاده می کند</remarks>
        internal static void Compress(string directoryPathToCompressItsContent, string zipFilePathToSaveCompressedContentFile)
        {
            if (directoryPathToCompressItsContent == null)
                throw new ArgumentNullException("directoryPathToCompressItsContent");
            if (zipFilePathToSaveCompressedContentFile == null)
                throw new ArgumentNullException("zipFilePathToSaveCompressedContentFile");
            if (string.IsNullOrWhiteSpace(directoryPathToCompressItsContent))
                throw new ArgumentException("Invalid argument", "directoryPathToCompressItsContent");
            if (string.IsNullOrWhiteSpace(zipFilePathToSaveCompressedContentFile))
                throw new ArgumentException("Invalid argument", "zipFilePathToSaveCompressedContentFile");

            ZipFile.CreateFromDirectory(directoryPathToCompressItsContent, zipFilePathToSaveCompressedContentFile);
        }
        /// <summary>
        /// آدرس یک فایل فشرده را دریافت و محتوای آن را پس از بازگشایی در پوشه ی با آدرس داده شده ذخیره می نماید
        /// </summary>
        /// <remarks>نوع فشرده سازی/بازگشایی، «زیپ» می باشد و این عملکرد برای فشرده سازی/بازگشایی از عملکردهای «دات نت» استفاده می کند</remarks>
        internal static void Decompress(string zipFilePathToExtractItsContent, string directoryPathToSaveExtractedContent)
        {
            if (zipFilePathToExtractItsContent == null)
                throw new ArgumentNullException("zipFilePathToExtractItsContent");
            if (directoryPathToSaveExtractedContent == null)
                throw new ArgumentNullException("directoryPathToSaveExtractedContent");
            if (string.IsNullOrWhiteSpace(zipFilePathToExtractItsContent))
                throw new ArgumentException("Invalid argument", "zipFilePathToExtractItsContent");
            if (string.IsNullOrWhiteSpace(directoryPathToSaveExtractedContent))
                throw new ArgumentException("Invalid argument", "directoryPathToSaveExtractedContent");

            ZipFile.ExtractToDirectory(zipFilePathToExtractItsContent, directoryPathToSaveExtractedContent);
        }

        private static readonly string workspaceTempFolderPath = ConfigurationManager.AppSettings["WorkspaceTempFolderPath"];

        /// <summary>
        /// مسیر یک پوشه استفاده موقت برای سمت محیط کاربری را برمی گرداند؛ توصیه می شود در صورت نیاز به محلی برای ذخیره سازی موقت داده ها در دیسک محلی، از این آدرس استفاده شود.
        /// </summary>
        /// <remarks>این مسیر شامل مسیر موقت کاربر جاری + عنوان نرم افزار + مقدار «جی یو آی دی» کنونی لایه منطق می باشد</remarks>
        public static string GetTempPath()
        {
            string result = string.Format("{0}{1}\\", workspaceTempFolderPath, System.GetGUID());
            if (!Directory.Exists(result))
                Directory.CreateDirectory(result);
            return result;
        }
    }
}
