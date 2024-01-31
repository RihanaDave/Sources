using GPAS.Ontology;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// متصدی استفاده از هستان شناسی در لایه منطق سمت محیط کاربری
    /// </summary>
	public class OntologyProvider
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private OntologyProvider()
        { }

        /// <summary>
        /// درصورت بارگذاری قبلی هستان شناسی کنونی در حال استفاده در سمت محیط کاربری را نگهداری می کند
        /// </summary>
        private static Ontology.Ontology currentOntology = null;
        private static DynamicOntology.DynamicOntology dynamicOntology = null;
        /// <summary>
        /// آماده سازی اولیه قبلی متصدی هستان شناسی را برمی گرداند
        /// </summary>
        /// <returns>در صورت آماده سازی اولیه قبلی مقدار «صحیح» و در غیراینصورت «غلط» را برمی گرداند</returns>
        public static bool IsInitialized()
        { return currentOntology != null; }
        /// <summary>
        /// مسیر موقت مورد استقاده جهت کار با هستان شناسی را نگه می دارد
        /// </summary>
        internal static readonly string OntologyTempFolderPath = ConfigurationManager.AppSettings["OntologyTempFolderPath"];
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
        private static readonly string folderNameOfOntology = "ontology";

        public static IEnumerable<string> GetAllChilds(string objectType)
        {
            List<string> result = new List<string>();
            foreach (string item in currentOntology.GetAllChilds(objectType))
                result.Add(item);
            return result;
        }

        /// نام پوشه نهایی هستان شناسی
        /// </summary>
        private static readonly string folderNameOfFinalOntology = "ontology2";
        private static object ontologyLockObject = new object();
        /// <summary>
        /// آماده سازی اولیه متصدی هستان شناسی را انجام می هد
        /// </summary>
        public async static Task InitAsync()
        {
            string zipPath = string.Format("{0}{1}.zip", OntologyTempFolderPath, folderNameOfOntology);
            string extractPath = string.Format("{0}{1}", OntologyTempFolderPath, folderNameOfOntology);
            string finalPath = string.Format("{0}{1}", OntologyTempFolderPath, folderNameOfFinalOntology);

            // دریافت فایل هستان شناسی از سرویس دهنده راه دور
            var compressedOntology = await GetCompressedOntologyFromDispatchAsync();

            // دریافت فایل بسته نمایه های انواع هستان شناسی از سرویس دهنده راه دور
            var compressedOntologyIconPack = await OntologyIconProvider.GetCompressedOntologyIconPackFromDispatchAsync();

            lock (ontologyLockObject)
            {                
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
                if (File.Exists(extractPath + "\\" + fileNameOfMetadataFile))
                    File.Delete(extractPath + "\\" + fileNameOfMetadataFile);
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                if (!Directory.Exists(OntologyTempFolderPath))
                    Directory.CreateDirectory(OntologyTempFolderPath);

                FileStream fileStream = null;
                try
                {
                    fileStream = File.Create(zipPath);
                    compressedOntology.CopyTo(fileStream);
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

                // آماده سازی اولیه متصدی نمایه (آیکن) های انواع هستان شناسی
                OntologyIconProvider.InitAsync(compressedOntologyIconPack);
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
                //InitAsync().Wait();
                throw new Exception("Ontology is not initialized");
            return currentOntology;
        }

        /// <summary>
        /// جریان (استریم) فایل هستان شناسی کنونی را از سرویس دهنده توزیع دریافت کرده و برمی گرداند
        /// </summary>
        private async static Task<Stream> GetCompressedOntologyFromDispatchAsync()
        {
            // دریافت جریان فایل هستان شناسی از سرویس دهنده راه دور
            WorkspaceServiceClient client = RemoteServiceClientFactory.GetNewClient();
            Stream result = null;
            try
            {
                client.Open();
                result = await client.GetOntologyAsync();
            }
            finally
            {
                client.Close();
            }

            if (result == null)
                throw new NullReferenceException("Invalid server response");
            // بازگرداندن جریان فایل
            return result;
        }

        /// <summary>
        /// عنوان کاربر پسند یک شناسه نوع هستان شناسی را برمی گرداند
        /// </summary>
        public static string GetUserFriendlyNameOfOntologyTypeNode(string nodeTypeURIToGetItsUserFriendlyName)
        {
            if (nodeTypeURIToGetItsUserFriendlyName == null)
                throw new ArgumentNullException("nodeTypeURIToGetItsUserFriendlyName");

            if (string.IsNullOrWhiteSpace(nodeTypeURIToGetItsUserFriendlyName))
                throw new ArgumentException("Invalid argument", "nodeTypeURIToGetItsUserFriendlyName");

            if (currentOntology.IsDocument(nodeTypeURIToGetItsUserFriendlyName) &&
                !nodeTypeURIToGetItsUserFriendlyName.Equals(currentOntology.GetDocumentTypeURI()))
                return string.Format("سند {0}", nodeTypeURIToGetItsUserFriendlyName);

            return nodeTypeURIToGetItsUserFriendlyName.Replace('_', ' ');
        }
        /// <summary>
        /// این تابع مفهوم جدید را به هستان شناسی اضافه میکند 
        /// </summary>
        public static string InsertNewConceptIntoTheOntology(string newConceptName, string parentName, List<DataType> selectedProperties, int count)
        {
            if (newConceptName == null)
                throw new ArgumentNullException("newConceptName");
            if (parentName == null)
                throw new ArgumentNullException("parentName");
            if (string.IsNullOrWhiteSpace(newConceptName))
                throw new ArgumentException("Invalid argument", "newConceptName");
            if (string.IsNullOrWhiteSpace(parentName))
                throw new ArgumentException("Invalid argument", "parentName");


            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.InsertNewConceptIntoTheOntology(newConceptName, parentName, selectedProperties, WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile, currentOntology.GetBaseUri(WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile), count);
        }
        public static string InsertNewConceptIntoTheOntology(string newConceptName, string parentName, string ontologyPath, List<DataType> selectedProperties, int count)
        {
            if (newConceptName == null)
                throw new ArgumentNullException("newConceptName");
            if (parentName == null)
                throw new ArgumentNullException("parentName");
            if (string.IsNullOrWhiteSpace(newConceptName))
                throw new ArgumentException("Invalid argument", "newConceptName");
            if (string.IsNullOrWhiteSpace(parentName))
                throw new ArgumentException("Invalid argument", "parentName");

            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.InsertNewConceptIntoTheOntology(newConceptName, parentName, selectedProperties, ontologyPath, currentOntology.GetBaseUri(ontologyPath), count);
        }
        public static string AssignNewPropertyToTheObject(string newPropertyName, string newPropertyType, string objectType, int count)
        {
            if (newPropertyName == null)
                throw new ArgumentNullException("newPropertyName");
            if (newPropertyType == null)
                throw new ArgumentNullException("newPropertyType");
            if (objectType == null)
                throw new ArgumentNullException("objectType");
            if (string.IsNullOrWhiteSpace(newPropertyName))
                throw new ArgumentException("Invalid argument", "newPropertyName");
            if (string.IsNullOrWhiteSpace(newPropertyType))
                throw new ArgumentException("Invalid argument", "newPropertyType");
            if (string.IsNullOrWhiteSpace(objectType))
                throw new ArgumentException("Invalid argument", "objectType");

            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.AssignNewPropertyToTheObject(newPropertyName, newPropertyType, objectType, WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile, currentOntology.GetBaseUri(WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile), count);
        }
        public static string AddNewDomainToProperty(string propertyName, string newDomainName, int count)
        {
            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.AddNewDomainToProperty(propertyName, newDomainName, WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile, currentOntology.GetBaseUri(WorkspaceTemperoryFiles.GetTempPath() + folderNameOfOntology + "\\" + fileNameOfOntologyFile), count);
        }
        public static string AssignNewPropertyToTheObject(string newPropertyName, string newPropertyType, string ontologyPath, string objectType, int count)
        {
            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.AssignNewPropertyToTheObject(newPropertyName, newPropertyType, objectType, ontologyPath, currentOntology.GetBaseUri(ontologyPath), count);
        }
        public static string AddNewDomainToProperty(string PropertyName, string newDomainName, string ontologyPath, int count)
        {
            dynamicOntology = new DynamicOntology.DynamicOntology();
            return dynamicOntology.AddNewDomainToProperty(PropertyName, newDomainName, ontologyPath, currentOntology.GetBaseUri(ontologyPath), count);
        }
        public static List<DataType> GetUnionProperties(string parentName)
        {
            List<DataType> ontologyProperties = new List<DataType>();
            ontologyProperties = currentOntology.GetUnionOfPropertiesOfSiblingsOfObject(parentName);
            return ontologyProperties;
        }
        public static List<DataType> GetUnionProperties(string parentName, Ontology.Ontology newOntology)
        {
            List<DataType> ontologyProperties = new List<DataType>();
            ontologyProperties = newOntology.GetUnionOfPropertiesOfSiblingsOfObject(parentName);
            return ontologyProperties;
        }
        public static List<DataType> GetIntersectProperties(string parentName)
        {
            List<DataType> ontologyProperties = new List<DataType>();
            ontologyProperties = currentOntology.GetIntersectOfPropertiesOfSiblingsOfObject(parentName);
            return ontologyProperties;
        }
        public static List<DataType> GetIntersectProperties(string parentName, Ontology.Ontology newOntology)
        {
            List<DataType> ontologyProperties = new List<DataType>();
            ontologyProperties = newOntology.GetIntersectOfPropertiesOfSiblingsOfObject(parentName);
            return ontologyProperties;
        }
        public async static Task updateOntologyAsync(Stream file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            //Stream stream = new Stream(file);
            try
            {
                await sc.UpdateOntologyFileAsync(file);
            }
            finally
            {
                sc.Close();
            }
        }

        public static BaseDataTypes GetBaseDataTypeOfProperty(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");
            return currentOntology.GetBaseDataTypeOfProperty(propertyName);
        }

        public static bool IsUnstructuredFileType(string fileExtension)
        {
            if (currentOntology.IsDocument(fileExtension.ToUpper())
                && !currentOntology.GetDocumentTypeURI().Equals(fileExtension))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // تابع زیر به صورت موقت اضافه شده است و باید اصلاح شود
        public static bool IsSemiStructuredFileType(string fileExtension)
        {
            if (fileExtension.ToLower() == "csv" || fileExtension.ToLower() == "xlsx")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}