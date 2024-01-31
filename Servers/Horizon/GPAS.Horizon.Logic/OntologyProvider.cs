using GPAS.Ontology;
using System;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using GPAS.Horizon.Access.DispatchService;

namespace GPAS.Horizon.Logic
{
    public class OntologyProvider : IDisposable
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private OntologyProvider()
        {
        }

        /// <summary>
        /// درصورت بارگذاری قبلی هستان شناسی کنونی در حال استفاده در سمت محیط کاربری را نگهداری می کند
        /// </summary>
        private static Ontology.Ontology currentOntology = null;
        /// <summary>
        /// آماده سازی اولیه قبلی متصدی هستان شناسی را برمی گرداند
        /// </summary>
        /// <returns>در صورت آماده سازی اولیه قبلی مقدار «صحیح» و در غیراینصورت «غلط» را برمی گرداند</returns>
        public static bool IsInitialized()
        { return currentOntology != null; }
        /// <summary>
        /// مسیر موقت مورد استقاده جهت کار با هستان شناسی را نگه می دارد
        /// </summary>
        protected static string pathToUseForOntology = ConfigurationManager.AppSettings["OntologyPath"];
        /// <summary>
        /// نام فایل هستان شناسی
        /// </summary>
        private static string fileNameOfOntologyFile = "4.rdf";
        /// <summary>
        /// نام فایل فراداده
        /// </summary>
        private static string fileNameOfMetadataFile = "Ontology Metadata.csv";
        /// <summary>
        /// نام پوشه فشرده شده هستان شناسی
        /// </summary>
        private static string folderNameOfOntology = Guid.NewGuid().ToString();
        /// نام پوشه نهایی هستان شناسی
        /// </summary>
        private static string folderNameOfFinalOntology = Guid.NewGuid().ToString();

        private static string zipPath = string.Empty;
        private static string extractPath = string.Empty;
        private static object ontologyLockObject = new object();
        /// <summary>
        /// آماده سازی اولیه متصدی هستان شناسی را انجام می هد
        /// </summary>
        public static void Init()
        {
            if (IsInitialized())
                throw new Exception("Ontology is initialized brefore");

            // دریافت فایل هستان شناسی از سرویس دهنده راه دور
            var response = GetCompressedOntologyFromDispatch();

            zipPath = string.Format("{0}{1}.zip", pathToUseForOntology, folderNameOfOntology);
            extractPath = string.Format("{0}{1}", pathToUseForOntology, folderNameOfOntology);
            string finalPath = string.Format("{0}{1}", pathToUseForOntology, folderNameOfFinalOntology);

            lock (ontologyLockObject)
            {
                if (!Directory.Exists(pathToUseForOntology))
                {
                    Directory.CreateDirectory(pathToUseForOntology);
                }
                else
                {
                    foreach (var subDir in Directory.GetDirectories(pathToUseForOntology))
                    {
                        Directory.Delete(subDir, true);
                    }
                    foreach (var file in Directory.GetFiles(pathToUseForOntology))
                    {
                        File.Delete(file);
                    }
                }
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                if (File.Exists(extractPath + "\\" + fileNameOfMetadataFile))
                    File.Delete(extractPath + "\\" + fileNameOfMetadataFile);
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                FileStream fileStream = null;
                try
                {
                    fileStream = File.Create(zipPath);
                    response.CopyTo(fileStream);
                }
                finally
                {
                    if (fileStream != null)
                        fileStream.Close();
                }
                if (File.Exists(extractPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(extractPath);
                    dir.Delete(true);
                    dir = null;
                }
                if (File.Exists(finalPath))
                {
                    DirectoryInfo dir1 = new DirectoryInfo(finalPath);
                    dir1.Delete(true);
                    dir1 = null;
                }

                Decompress(zipPath, extractPath);
                (currentOntology = new Ontology.Ontology())
                    .LoadOntology(extractPath + "\\" + fileNameOfOntologyFile, extractPath + "\\Ontology Metadata.csv");
            }
        }
        
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
        /// <summary>
        /// هستان شناسی جاری سمت محیط کاربری را برمی گرداند
        /// </summary>
        /// <remarks>این عملکرد در صورت عدم آماده سازی قبلی متصدی هستان شناسی، آماده سازی اولیه را انجام می دهد و هستان شناسی جاری را برمی گرداند</remarks>
        public static Ontology.Ontology GetOntology()
        {
            if (!IsInitialized())
                Init();
                //throw new Exception("Ontology is not initialized");
            return currentOntology;
        }
        /// <summary>
        /// جریان (استریم) فایل هستان شناسی کنونی را از سرویس دهنده توزیع دریافت کرده و برمی گرداند
        /// </summary>
        private static Stream GetCompressedOntologyFromDispatch()
        {
            // دریافت جریان فایل هستان شناسی از سرویس دهنده راه دور
            var client = new InfrastructureServiceClient();
            Stream result = null;
            try
            {
                result = client.GetOntology();
            }
            finally
            {
                client.Close();
            }
            if (result == null)
                throw new NullReferenceException("Invalid server response");
            //بازگرداندن جریان فایل
            return result;
        }

        public void Dispose()
        {
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            if (File.Exists(extractPath + "\\" + fileNameOfMetadataFile))
                File.Delete(extractPath + "\\" + fileNameOfMetadataFile);

            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }

        ~OntologyProvider()
        {
            Dispose();
        }
    }
}
