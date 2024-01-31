using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.FlatFileStorage;
using GPAS.FtpServiceAccess;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Globalization;

namespace GPAS.FileRepository.Logic
{
    public class DataSourceAndDocumentManager
    {
        private static readonly string dataDirectory = "data";
        private static readonly string DocumentsDirectory = "Documents";
        private static readonly string DataSourceDirectory = "DataSource";
        private static readonly string DataDirectory = "data";
        private static JobShareServiceAccessMetaData jobShareAccessMetaData;
        private static string PluginPath = null;
        private const string FlatFileStoragePluginName = "FlatFileStoragePluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public DataSourceAndDocumentManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[FlatFileStoragePluginName];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{FlatFileStoragePluginName}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }
            // کد ترکیب اسمبلی پلاگین برگرفته از مثال مایکروسافت در آدرس زیر است:
            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        #region Server Management
        public void Init()
        {
            jobShareAccessMetaData = new JobShareServiceAccessMetaData()
            {
                ServerAddress = ConfigurationManager.AppSettings["JobShareService_ServerAddress"],
                PortNumber = ConfigurationManager.AppSettings["JobShareService_PortNumber"],
                UserName = ConfigurationManager.AppSettings["JobShareService_UserName"],
                Password = ConfigurationManager.AppSettings["JobShareService_Password"]
            };
            
            //StorageClient.CreateDirectory(DocumentsDirectory);
            //StorageClient.CreateDirectory(DataSourceDirectory);
            //StorageClient.CreateDirectory(dataDirectory);

            if (!IsStorageAvailable())
            {
                throw new InvalidOperationException("Storage is not available as it's directory was created!");
            }
        }
        public bool IsStorageAvailable()
        {
            return StorageClient.IsAvailable();
        }
        public void RemoveAllFiles()
        {
            StorageClient.RemoveDirectories(DocumentsDirectory, DataSourceDirectory,dataDirectory);
            StorageClient.CreateDirectory(DocumentsDirectory);
            StorageClient.CreateDirectory(DataSourceDirectory);
            StorageClient.CreateDirectory(dataDirectory);
        }
        #endregion

        #region Data-Sources & Documents

        public void UploadDocumentFileByName(string docName, byte[] docContent)
        {
            StorageClient.SaveFile(docContent, docName.ToString(CultureInfo.InvariantCulture), dataDirectory);
        }

        public void UploadDocumentFile(long docID, byte[] docContent)
        {
            StorageClient.SaveFile(docContent, docID.ToString(CultureInfo.InvariantCulture), DocumentsDirectory);
        }
        public void UploadDataSourceFile(long dataSourceID, byte[] docContent)
        {
            StorageClient.SaveFile(docContent, dataSourceID.ToString(CultureInfo.InvariantCulture), DataSourceDirectory);
        }
        public void UploadFileAsDocumentAndDataSource(byte[] fileToUpload, long docID, long dataSourceID)
        {
            StorageClient.SaveFileInTwoPathes(fileToUpload
                , docID.ToString(CultureInfo.InvariantCulture), DocumentsDirectory
                , dataSourceID.ToString(CultureInfo.InvariantCulture), DataSourceDirectory);
        }
        public void UploadDocumentFromJobShare(long docID, string jobShareSourceFilePath)
        {
            var ftpProvider = new FtpServiceProvider($"{jobShareAccessMetaData.ServerAddress}:{jobShareAccessMetaData.PortNumber}", jobShareAccessMetaData.UserName, jobShareAccessMetaData.Password);
            byte[] sharedFileContent = ftpProvider.DownloadData(jobShareSourceFilePath);
            StorageClient.SaveFile(sharedFileContent, docID.ToString(CultureInfo.InvariantCulture), DocumentsDirectory);
        }
        public void UploadDataSourceFromJobShare(long dataSourceID, string jobShareSourceFilePath)
        {
            var ftpProvider = new FtpServiceProvider($"{jobShareAccessMetaData.ServerAddress}:{jobShareAccessMetaData.PortNumber}", jobShareAccessMetaData.UserName, jobShareAccessMetaData.Password);
            byte[] sharedFileContent = ftpProvider.DownloadData(jobShareSourceFilePath);
            StorageClient.SaveFile(sharedFileContent, dataSourceID.ToString(CultureInfo.InvariantCulture), DataSourceDirectory);
        }
        public byte[] DownloadDocumentFile(long docID)
        {
            return StorageClient.LoadFile(docID.ToString(CultureInfo.InvariantCulture), DocumentsDirectory);
        }
        public byte[] DownloadDataSourceFile(long dataSourceID)
        {
            return StorageClient.LoadFile(dataSourceID.ToString(CultureInfo.InvariantCulture), DataDirectory);
        }
        public long GetDataSourceAndDocumentFileSizeInBytes(string docId)
        {
            return StorageClient.GetFileSizeInBytes(docId, DocumentsDirectory);
        }
        public byte[] DownloadDataSourceFile(string dataSource)
        {
            return StorageClient.LoadFile(dataSource, DataDirectory);
        }
        #endregion
    }
}
