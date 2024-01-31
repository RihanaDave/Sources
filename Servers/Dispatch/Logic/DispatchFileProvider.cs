using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Configuration;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس دسترسی به آیکون و هستان شناسی موجود در File Server را فراهم می کند.
    /// </summary>
    public class DispatchFileProvider
    {
        private static readonly string iconsFolderPath = ConfigurationManager.AppSettings["IconsFolderPath"];
        private static readonly string ontologyFolderPath = ConfigurationManager.AppSettings["OntologyFolderPath"];
        public static readonly string GlobalResolutionSuitePrefix = ".grs";

        private static object createIconPackLockObject = new object();
        /// <summary>
        /// آیکون های موجود را برای فضای کاری ارسال می کند.
        /// این تابع آیکون ها را به صورت فایل زیپ پاس می دهد.
        /// </summary>
        /// <returns> آیکون ها به صورت فایل زیپ </returns>
        public Stream GetIconPack()
        {
            string zipPath = iconsFolderPath.TrimEnd('\\') + ".zip";
            lock (createIconPackLockObject)
            {
                if (!File.Exists(zipPath))
                    ZipFile.CreateFromDirectory(iconsFolderPath, zipPath);
                string path = zipPath;
                FileStream file = File.OpenRead(path);
                return file;
            }                       
        }
        private static object createOntologyLockObject = new object();
        private static object updateOntologyLockObject = new object();
        /// <summary>
        /// آنتولوژی که یک فایل ایکس ام ال است را برای فضای کاری ارسال می کند.
        /// </summary>
        /// <returns> آنتولوژی را به صورت Stream برمی گرداند. </returns>
        public Stream GetOntology()
        {
            string zipPath = ontologyFolderPath.TrimEnd('\\') + ".zip";
            lock (createOntologyLockObject)
            {                
                if (!File.Exists(zipPath))
                    ZipFile.CreateFromDirectory(ontologyFolderPath, zipPath);
                string path = zipPath;
                FileStream file = File.OpenRead(path);
                return file;
            }            
        }
        /// <summary>
        /// این تابع آنتولوژی به روز رسانی شده را از کاربر دریافت می کند و جایگزین انتولوژی قبلی می کند.
        /// </summary>
        /// <param name="reader">این پارامتر رشته ای از فایل انتولوزی را دریافت می کند.</param>
        public void UpdateOntology(Stream reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            lock (updateOntologyLockObject)
            {
                if (File.Exists(ontologyFolderPath))
                {
                    File.Delete(ontologyFolderPath);
                }
                FileStream fileStream = File.Create(ontologyFolderPath);
                reader.CopyTo(fileStream);
                fileStream.Close();
            }            
        }
    }
}
