using System;
using System.IO;
using System.IO.Compression;

namespace GPAS.OntologyLoader
{
    public class OntologyLoader
    {
        /// <summary>
        /// درصورت بارگذاری قبلی هستان شناسی کنونی در حال استفاده در سمت محیط کاربری را نگهداری می کند
        /// </summary>
        private static Ontology.Ontology currentOntology;

        /// <summary>
        /// آماده سازی اولیه قبلی متصدی هستان شناسی را برمی گرداند
        /// </summary>
        /// <returns>در صورت آماده سازی اولیه قبلی مقدار «صحیح» و در غیراینصورت «غلط» را برمی گرداند</returns>
        public static bool IsInitialized()
        {
            return currentOntology != null;
        }

        /// <summary>
        /// نام فایل هستان شناسی
        /// </summary>
        private static readonly string fileNameOfOntologyFile = "4.rdf";

        /// <summary>
        /// نام فایل فراداده
        /// </summary>
        private static readonly string fileNameOfMetadataFile = "Ontology Metadata.csv";

        /// <summary>
        /// نام پوشه فشرده شده هستان شناسی
        /// </summary>
        private static string folderNameOfOntology = string.Empty;

        private static string folderOfOntologyIcons = string.Empty;

        static string extractPath = string.Empty;

        static string zipPath = string.Empty;

        private static readonly object ontologyLockObject = new object();

        private static readonly object ontologyIconsLockObject = new object();

        /// <summary>
        /// آماده سازی اولیه متصدی هستان شناسی را انجام می هد
        /// </summary>
        public static void Init(IOntologyBuilder ontologyDownLoader, string pathToUseForOntology, string pathToUseForIcons)
        {
            InitOntology(ontologyDownLoader, pathToUseForOntology);
            InitOntologyIcon(ontologyDownLoader, pathToUseForIcons);
        }

        private static void InitOntology(IOntologyBuilder ontologyDownLoader, string pathToUseForOntology)
        {
            folderNameOfOntology = Guid.NewGuid().ToString();
            zipPath = Path.Combine(pathToUseForOntology, $"{folderNameOfOntology}.zip");
            extractPath = Path.Combine(pathToUseForOntology, folderNameOfOntology);

            // دریافت فایل هستان شناسی 
            var ontologyStream = ontologyDownLoader.DownloadOntologyStream();

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

                if (File.Exists(Path.Combine(extractPath, fileNameOfMetadataFile)))
                    File.Delete(Path.Combine(extractPath, fileNameOfMetadataFile));

                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                FileStream fileStream = null;
                try
                {
                    fileStream = File.Create(zipPath);
                    ontologyStream.CopyTo(fileStream);
                }
                finally
                {
                    fileStream?.Close();
                }
                if (File.Exists(extractPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(extractPath);
                    dir.Delete(true);
                }

                Decompress(zipPath, extractPath);

                (currentOntology = new Ontology.Ontology()).LoadOntology(Path.Combine(extractPath, fileNameOfOntologyFile),
                        Path.Combine(extractPath, fileNameOfMetadataFile));
            }
        }

        private static void InitOntologyIcon(IOntologyBuilder ontologyDownLoader, string pathToUseForIcons)
        {
            string folderNameOfOntologyIcons = "ontology_pack_icon";
            string zipIconsPath = Path.Combine(pathToUseForIcons, $"{folderNameOfOntologyIcons}.zip");
            string extractIconsPath = Path.Combine(pathToUseForIcons, folderNameOfOntologyIcons);

            var ontologyIconStream = ontologyDownLoader.DownloadOntologyPackIconStream();

            lock (ontologyIconsLockObject)
            {
                if (!Directory.Exists(pathToUseForIcons))
                {
                    Directory.CreateDirectory(pathToUseForIcons);
                }
                else
                {
                    foreach (var subDir in Directory.GetDirectories(pathToUseForIcons))
                    {
                        Directory.Delete(subDir, true);
                    }
                    foreach (var file in Directory.GetFiles(pathToUseForIcons))
                    {
                        File.Delete(file);
                    }
                }

                if (Directory.Exists(extractIconsPath))
                    Directory.Delete(extractIconsPath, true);

                if (File.Exists(zipIconsPath))
                    File.Delete(zipIconsPath);

                FileStream fileStream = null;
                try
                {
                    fileStream = File.Create(zipIconsPath);
                    ontologyIconStream.CopyTo(fileStream);
                }
                finally
                {
                    fileStream?.Close();
                }

                if (File.Exists(extractIconsPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(extractIconsPath);
                    dir.Delete(true);
                }

                Decompress(zipIconsPath, extractIconsPath);

                folderOfOntologyIcons = extractIconsPath;
            }
        }

        internal static void Decompress(string ontologyZipPath, string ontologyExtractPath)
        {
            if (ontologyZipPath == null)
                throw new ArgumentNullException(nameof(ontologyZipPath));

            if (ontologyExtractPath == null)
                throw new ArgumentNullException(nameof(ontologyExtractPath));

            if (string.IsNullOrWhiteSpace(ontologyZipPath))
                throw new ArgumentException("Invalid argument", nameof(ontologyZipPath));

            if (string.IsNullOrWhiteSpace(ontologyExtractPath))
                throw new ArgumentException("Invalid argument", nameof(ontologyExtractPath));

            ZipFile.ExtractToDirectory(ontologyZipPath, ontologyExtractPath);
        }
        /// <summary>
        /// هستان شناسی جاری سمت محیط کاربری را برمی گرداند
        /// </summary>
        /// <remarks>این عملکرد در صورت عدم آماده سازی قبلی متصدی هستان شناسی، آماده سازی اولیه را انجام می دهد و هستان شناسی جاری را برمی گرداند</remarks>
        public static Ontology.Ontology GetOntology()
        {
            if (!IsInitialized())
                throw new Exception("Ontology is not initialized");
            return currentOntology;
        }

        public static string GetOntologyIconsPath()
        {
            if (!IsInitialized())
                throw new Exception("Ontology is not initialized");

            return folderOfOntologyIcons;
        }

        public void Dispose()
        {
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            if (File.Exists(Path.Combine(extractPath, fileNameOfMetadataFile)))
                File.Delete(Path.Combine(extractPath, fileNameOfMetadataFile));

            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }

        ~OntologyLoader()
        {
            Dispose();
        }
    }
}
