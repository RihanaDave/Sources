using GPAS.AccessControl;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using GPAS.Workspace.ViewModel.DataImport;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    public class DataSourceProvider
    {
        public static readonly string LocalUnstructuredFolderPath = ConfigurationManager.AppSettings["LocalUnstructuredFolderPath"];

        public static void Init()
        {
            if (!Directory.Exists(LocalUnstructuredFolderPath))
                Directory.CreateDirectory(LocalUnstructuredFolderPath);
        }

        public long RegisterNewManualyEnteredDataSource()
        {
            var dataSource = new DataSourceMetadata();
            dataSource.Name = "Manually Entered Data";
            dataSource.Type = GPAS.AccessControl.DataSourceType.ManuallyEntered;
            dataSource.Content = new byte[] { };
            dataSource.Description = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            dataSource.Acl = UserAccountControlProvider.ManuallyEnteredDataACL;

            return RegisterNewNonDocumentDataSource(dataSource);
        }

        public long RegisterNewImportedUnstructuredDataSource(byte[] docContent, KWObject document, ACL acl)
        {
            var dataSource = new DataSourceMetadata();
            dataSource.Name = document.GetObjectLabel();
            dataSource.Type = GPAS.AccessControl.DataSourceType.Document;
            dataSource.Content = docContent;
            dataSource.Description = string.Empty;
            dataSource.Acl = acl;

            return RegisterNewDocumentDataSource(dataSource, document.ID);
        }

        public long RegisterNewImportedFileBasedSemiStructuredDataSource(MaterialBase dataSourceMaterial, byte[] dataSourceFileContent)
        {
            var dataSource = new DataSourceMetadata();
            dataSource.Name = DataSourceMetadata.GetSemiStructuredDataSourceName(dataSourceMaterial);
            dataSource.Type = DataSourceMetadata.GetSemiStructuredDataSourceType(dataSourceMaterial);
            dataSource.Content = dataSourceFileContent;
            dataSource.Description = string.Empty;
            dataSource.Acl = UserAccountControlProvider.ImportACL;

            return RegisterNewNonDocumentDataSource(dataSource);
        }

        public long RegisterNewImportedNonFileBasedSemiStructuredDataSource(MaterialBase dataSourceMaterial)
        {
            if (dataSourceMaterial is CsvFileMaterial)
                throw new NotSupportedException();

            var dataSource = new DataSourceMetadata();
            dataSource.Name = DataSourceMetadata.GetSemiStructuredDataSourceName(dataSourceMaterial);
            dataSource.Type = DataSourceMetadata.GetSemiStructuredDataSourceType(dataSourceMaterial);
            dataSource.Content = DataSourceMetadata.GetNonFileBasedSemiStructuredDataSourceContent(dataSourceMaterial);
            dataSource.Description = string.Empty;
            dataSource.Acl = UserAccountControlProvider.ImportACL;

            return RegisterNewNonDocumentDataSource(dataSource);
        }

        public DataSourceMetadata GenerateImportDataSourceMetadata(MaterialBaseVM materialVM)
        {
            return GenerateImportDataSourceMetadata(materialVM, UserAccountControlProvider.ImportACL);
        }

        public DataSourceMetadata GenerateImportDataSourceMetadata(MaterialBaseVM materialVM, ACL acl)
        {
            var dataSource = new DataSourceMetadata();
            dataSource.Name = materialVM.Title;
            dataSource.Type = DataSourceMetadata.GetSemiStructuredDataSourceType(materialVM.relatedMaterialBase);
            dataSource.Content = GetSemiStructuredDataSourceContent(materialVM);
            dataSource.Description = string.Empty;
            dataSource.Acl = acl;
            return dataSource;
        }

        private byte[] GetSemiStructuredDataSourceContent(MaterialBaseVM materialVM)
        {
            if (materialVM is CsvFileMaterialVM)
            {
                return File.ReadAllBytes(((CsvFileMaterialVM)materialVM).CsvFilePath);
            }
            else if (materialVM is ExcelSheetMaterialVM)
            {
                return File.ReadAllBytes(((ExcelSheetMaterialVM)materialVM).ExcelLocalFilePath);
            }
            else if (materialVM is AccessTableMaterialVM)
            {
                return File.ReadAllBytes(((AccessTableMaterialVM)materialVM).AccessLocalFilePath);
            }
            else if (materialVM is EmlFileMaterialVM)
            {
                return File.ReadAllBytes(((EmlFileMaterialVM)materialVM).workspceSideCsvFilePath);
            }
            else
            {
                return DataSourceMetadata.GetNonFileBasedSemiStructuredDataSourceContent(materialVM.relatedMaterialBase);
            }
        }

        public long RegisterNewNonDocumentDataSource(DataSourceMetadata dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (dataSource.Type == GPAS.AccessControl.DataSourceType.Document)
                throw new InvalidOperationException("Unable to register Document-typed data source in this method");

            var adaptor = new DataImport.PublishAdaptor();
            var regProvider = new DataSourceRegisterationProvider(dataSource, adaptor);
            regProvider.Register();
            return regProvider.DataSourceID;
        }

        public long RegisterNewDocumentDataSource(DataSourceMetadata dataSource, long documentId)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (dataSource.Type != GPAS.AccessControl.DataSourceType.Document)
                throw new InvalidOperationException("Unable to register non-Document-typed data source in this method");

            var adaptor = new DataImport.PublishAdaptor();
            var regProvider = new DataSourceRegisterationProvider(dataSource, adaptor);
            regProvider.Register(documentId);
            return regProvider.DataSourceID;
        }

        public long RegisterNewDataSource(DataSourceMetadata dataSource, PublishAdaptor adaptor, int publishMaximumRetryTimes, bool reportFullDetailsInLog, ProcessLogger logger = null)
        {
            var regProvider = new DataSourceRegisterationProvider(dataSource, adaptor, logger);
            regProvider.ProcessMaximumRetryTimes = publishMaximumRetryTimes;
            regProvider.ReportFullDetailsInLog = reportFullDetailsInLog;
            regProvider.Register();
            return regProvider.DataSourceID;
        }


        public async Task<DataSourceInfo[]> GetAllDataSourcesAsync(string filter)
        {
            DataSourceInfo[] result = null;
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                result = await remoteServiceClient.GetAllDataSourcesAsync(filter);

            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
            return result;
        }

        public async Task<DataSourceInfo[]> GetDataSourcesAsync(DataSourceType dataSourceType, int star, string filter)
        {

            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                DataSourceInfo[] result = await remoteServiceClient.GetDataSourcesAsync((long)dataSourceType, star, filter);
                return result;
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
        

        public async static Task<string> DownloadDocumentAsync(KWObject doc)
        {
            if (!OntologyProvider.GetOntology().IsDocument(doc.TypeURI))
                throw new InvalidOperationException("Given object is not a Document");

            WorkspaceServiceClient sc = null;
            byte[] documentContent = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                documentContent = await sc.DownloadDocumentFileAsync(doc.ID);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }

            if (documentContent == null)
                throw new InvalidDataException("Retrieved Document content is 'null'!");

            string localPath = GetDocumentFileLocalPath(doc);
            await SaveFileContentAsFileAsync(documentContent, localPath);
            return localPath;
        }

        public async static Task DownloadDataSourceAsync(long dataSourceId, string localPath)
        {
            WorkspaceServiceClient sc = null;
            byte[] dataSourceContent = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                dataSourceContent = await sc.DownloadDataSourceFileAsync(dataSourceId);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }

            if (dataSourceContent == null)
                throw new InvalidDataException("Retrieved Data-Source content is 'null'!");

            await SaveFileContentAsFileAsync(dataSourceContent, localPath);
        }

        public async static Task DownloadDataSourceByNameAsync(string dataSourceName, string localPath)
        {
            WorkspaceServiceClient sc = null;
            byte[] dataSourceContent = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                dataSourceContent = await sc.DownloadDataSourceFileByNameAsync(dataSourceName);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }

            if (dataSourceContent == null)
                throw new InvalidDataException("Retrieved Data-Source content is 'null'!");

            await SaveFileContentAsFileAsync(dataSourceContent, localPath);
        }


        private async static Task SaveFileContentAsFileAsync(byte[] fileContent, string targetFilePath)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));
            if (string.IsNullOrEmpty(targetFilePath))
                throw new ArgumentNullException(nameof(targetFilePath));
            if (string.IsNullOrWhiteSpace(targetFilePath))
                throw new ArgumentException(nameof(targetFilePath));

            var fi = new FileInfo(targetFilePath);
            if (!fi.Directory.Exists)
                fi.Directory.Create();
            if (fi.Exists)
                return;

            FileStream fs = null;
            try
            {
                fs = fi.OpenWrite();
                await fs.WriteAsync(fileContent, 0, fileContent.Length);
                fs.Close();
                fi.IsReadOnly = false;
                File.SetAttributes(targetFilePath, FileAttributes.Normal);
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }

            if (!File.Exists(targetFilePath))
            {
                throw new FileNotFoundException();
            }
        }

        private static string GetDocumentFileLocalPath(KWObject doc)
        {
            if (!OntologyProvider.GetOntology().IsDocument(doc.TypeURI))
            {
                throw new InvalidOperationException("Given object is not a Document");
            }
            StringBuilder sb = new StringBuilder(LocalUnstructuredFolderPath);
            sb.Append(doc.ID);
            if (!doc.TypeURI.Equals(OntologyProvider.GetOntology().GetDocumentTypeURI()))
            {
                sb.AppendFormat(".{0}", doc.TypeURI);
            }
            return sb.ToString();
        }

        public void UploadDocument(byte[] documentContent, long documentId, long dataSourceID)
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.UploadFileAsDocumentAndDataSource(documentContent, documentId, dataSourceID);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public void UploadDataSource(long dataSourceID, byte[] dataSourceContent)
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.UploadDataSourceFile(dataSourceID, dataSourceContent);
                
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void UploadDataSourceByName(string dataSourceName, byte[] dataSourceContent)
        {
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                sc.UploadDataSourceFileByName(dataSourceName, dataSourceContent);

            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
    }
}
