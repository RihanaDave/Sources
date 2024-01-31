using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.AccessControl;

namespace GPAS.DataImport.Publish
{
    public class DataSourceRegisterationProvider
    {
        private void ValidateDataSource(DataSourceMetadata metadata)
        {
            if ((metadata.Type != AccessControl.DataSourceType.ManuallyEntered
                && metadata.Type != AccessControl.DataSourceType.Graph
                && metadata.Type != AccessControl.DataSourceType.CsvFile /*برای زمانی که بخواهیم فایل را از مسیر به اشتراک گذاشته شده آپلود کنیم*/
                && metadata.Type != AccessControl.DataSourceType.ExcelSheet
                && metadata.Type != AccessControl.DataSourceType.AccessTable)
                && metadata.Content.Length == 0)
            {
                throw new ArgumentException(nameof(metadata.Content));
            }
            if (string.IsNullOrWhiteSpace(metadata.Name))
            {
                throw new ArgumentException(nameof(metadata.Name));
            }
            if (metadata.Acl == null)
            {
                throw new ArgumentNullException(nameof(metadata.Acl));
            }
            if (metadata.Description == null)
            {
                metadata.Description = string.Empty;
            }
        }
        
        long dataSourceID = -1;
        bool dataSourceRegistered = false;
        PublishAdaptor adaptor = null;
        long documentObjID = -1;
        string sharedFileBasedDataSourcePath = null;
        ProcessLogger logger = null;

        public int ProcessMaximumRetryTimes { get; set; }
        public bool ReportFullDetailsInLog { get; set; }
        public DataSourceMetadata DataSource { get; private set; }
        public long DataSourceID
        {
            get
            {
                if (!dataSourceRegistered)
                    throw new InvalidOperationException("Unable to get Data Source ID before dataSource Registeration");
                return dataSourceID;
            }
            private set { dataSourceID = value; }
        }

        public DataSourceRegisterationProvider(DataSourceMetadata dataSource, PublishAdaptor publishAdaptor, ProcessLogger progressLogger = null)
        {
            if (DataSource != null)
                throw new InvalidOperationException("Data Source metadata defined before!");
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (publishAdaptor == null)
                throw new ArgumentNullException(nameof(publishAdaptor));

            ValidateDataSource(dataSource);
            DataSource = dataSource;
            adaptor = publishAdaptor;
            logger = progressLogger;

            ProcessMaximumRetryTimes = 5;
            ReportFullDetailsInLog = false;
        }

        /// <summary>
        /// Registers a shared file as Data Source
        /// </summary>
        public void Register(string sharedFilePath)
        {
            if (string.IsNullOrWhiteSpace(sharedFilePath))
                throw new ArgumentException("Invalid path", nameof(sharedFilePath));
            if (DataSource.Type != AccessControl.DataSourceType.CsvFile 
                && DataSource.Type != AccessControl.DataSourceType.ExcelSheet
                && DataSource.Type != AccessControl.DataSourceType.AccessTable
                )
                throw new InvalidOperationException("Unable to set shared file path for a non-file Data Source");
            if (DataSource.Content != null)
                throw new InvalidOperationException("Unable to set shared file path for a File-based Data Source with previously set content");

            sharedFileBasedDataSourcePath = sharedFilePath;
            Register();
        }

        /// <summary>
        /// Registers a Document-typed Data Source
        /// </summary>
        /// <param name="dcoumentID">Object ID of the Document</param>
        public void Register(long documentID)
        {
            if (documentID < 1)
                throw new ArgumentOutOfRangeException(nameof(documentID));
            if (DataSource.Type != AccessControl.DataSourceType.Document)
                throw new InvalidOperationException("Unable to set Document ID for a non-Document Data Source");

            documentObjID = documentID;
            Register();
        }

        /// <summary>
        /// Registers a non-Document Data Source
        /// </summary>
        public void Register()
        {
            if (dataSourceRegistered)
                throw new InvalidOperationException("Data Source is registered before!");
            if (DataSource.Type == AccessControl.DataSourceType.Document && documentObjID < 1)
                throw new InvalidOperationException("Unable to register Document-typed Data Source without setting Document ID");

            dataSourceID = GetNewDataSourceID();
            UploadDataSourceIfNeeded();
            RegisterNewDataSourceToRepositoryServer();
            SynchronizeNewDataSourceInSearchServer();

            dataSourceRegistered = true;
        }

        private void RegisterNewDataSourceToRepositoryServer()
        {
            WriteLog("Registering data source to data repository...");

            bool registered = false;
            int retryTimes = 0;
            TimeSpan processDuration = new TimeSpan();
            while (!registered && retryTimes < ProcessMaximumRetryTimes)
            {
                try
                {
                    DateTime StartTime = DateTime.Now;
                    adaptor.RegisterNewDataSourceToRepositoryServer(dataSourceID, DataSource.Name, DataSource.Type, DataSource.Acl, DataSource.Description);
                    processDuration = DateTime.Now - StartTime;
                    registered = true;
                }
                catch (Exception ex)
                {
                    retryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to register data source to data repository (try {retryTimes} of {ProcessMaximumRetryTimes}); Data Source ID: \"{dataSourceID}\" Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTimes < ProcessMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTimes, "Retry registering data source to data repository...");
                    }
                    else
                    {
                        WriteLog($"Data Source registeration failed to data repository.");
                        throw;
                    }
                }
            }
            WriteLog($"Data Source registered to data repository; Duration: {processDuration:h\\:mm\\:ss\\.fff}");
        }

        private void SynchronizeNewDataSourceInSearchServer()
        {
            WriteLog("Registering data source to search server...");

            bool registered = false;
            int retryTimes = 0;
            TimeSpan processDuration = new TimeSpan();
            while (!registered && retryTimes < ProcessMaximumRetryTimes)
            {
                try
                {
                    DateTime StartTime = DateTime.Now;
                    adaptor.SynchronizeNewDataSourceInSearchServer(dataSourceID, DataSource.Name, DataSource.Type, DataSource.Acl, DataSource.Description);
                    processDuration = DateTime.Now - StartTime;
                    registered = true;
                }
                catch (Exception ex)
                {
                    retryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to register data source to search server (try {retryTimes} of {ProcessMaximumRetryTimes}); Data Source ID: \"{dataSourceID}\" Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTimes < ProcessMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTimes, "Retry registering data source to search server...");
                    }
                    else
                    {
                        WriteLog($"Data Source registeration failed to search server.");
                        throw;
                    }
                }
            }
            WriteLog($"Data Source registered to search server; Duration: {processDuration:h\\:mm\\:ss\\.fff}");
        }

        private void UploadDataSourceIfNeeded()
        {
            switch (DataSource.Type)
            {
                case AccessControl.DataSourceType.ManuallyEntered:
                case AccessControl.DataSourceType.Graph:
                    // Upload not needed
                    break;
                case AccessControl.DataSourceType.Document:
                    UploadDocumentToFileRepository();
                    break;
                case AccessControl.DataSourceType.CsvFile:
                case AccessControl.DataSourceType.ExcelSheet:
                case AccessControl.DataSourceType.AccessTable:
                case AccessControl.DataSourceType.AttachedDatabaseTable:
                case AccessControl.DataSourceType.DataLakeSearchResult:
                    UploadSemiStructuredDataSourceToFileRepository();
                    break;
                default:
                    throw new NotSupportedException("Unknown Data Source Type");
            }
        }

        private void UploadDocumentToFileRepository()
        {
            WriteLog($"Uploading document to file repository...");
            bool uploaded = false;
            int retryTime = 0;
            TimeSpan processDuration = new TimeSpan();
            while (!uploaded && retryTime < ProcessMaximumRetryTimes)
            {
                try
                {
                    DateTime StartTime = DateTime.Now;
                    adaptor.UploadDocumentToFileRepository(DataSource.Content, documentObjID, dataSourceID);
                    processDuration = DateTime.Now - StartTime;
                    uploaded = true;
                }
                catch (Exception ex)
                {
                    retryTime++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to upload document (try {retryTime} of {ProcessMaximumRetryTimes}); Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTime < ProcessMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTime, "Retry upload Document ...");
                    }
                    else
                    {
                        WriteLog($"Upload failed.");
                        throw;
                    }
                }
            }
            WriteLog($"Document uploaded with Doc. ID: '{documentObjID}' and Data Source ID: '{dataSourceID}' | Duration: {processDuration:h\\:mm\\:ss\\.fff}");
        }

        private void UploadSemiStructuredDataSourceToFileRepository()
        {
            WriteLog($"Uploading data source to file repository...");
            bool uploaded = false;
            int retryTime = 0;
            TimeSpan processDuration = new TimeSpan();
            while (!uploaded && retryTime < ProcessMaximumRetryTimes)
            {
                try
                {
                    DateTime StartTime = DateTime.Now;
                    if ((DataSource.Type == AccessControl.DataSourceType.CsvFile 
                         || DataSource.Type == AccessControl.DataSourceType.ExcelSheet
                         || DataSource.Type == AccessControl.DataSourceType.AccessTable
                         )
                        && sharedFileBasedDataSourcePath != null)
                    {
                        adaptor.UploadSharedSemiStructuredDataSouurceToFileRepository(sharedFileBasedDataSourcePath, dataSourceID);
                    }
                    else
                    {
                        if (DataSource.Content == null || DataSource.Content.Length == 0)
                            throw new InvalidOperationException("Unable to accept Data Source with empty content in current configuration");
                        adaptor.UploadSemiStructuredDataSouurceToFileRepository(DataSource.Content, dataSourceID);
                    }
                    processDuration = DateTime.Now - StartTime;
                    uploaded = true;
                }
                catch (Exception ex)
                {
                    retryTime++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to upload data source (try {retryTime} of {ProcessMaximumRetryTimes}); Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTime < ProcessMaximumRetryTimes && !(ex is FileAlreadyExistInFileRepositoryException))
                    {
                        WaitProcessForRetry(retryTime, "Retry upload Data Source ...");
                    }
                    else
                    {
                        WriteLog($"Upload failed.");
                        throw;
                    }
                }
            }
            WriteLog($"Data Source uploaded with ID: '{dataSourceID}'; Duration: {processDuration:h\\:mm\\:ss\\.fff}");
        }

        private long GetNewDataSourceID()
        {
            WriteLog("Getting new Data Source ID...");

            bool retrieved = false;
            long newID = -1;

            int retryTimes = 0;
            TimeSpan processDuration = new TimeSpan();
            while (!retrieved && retryTimes < ProcessMaximumRetryTimes)
            {
                try
                {
                    DateTime StartTime = DateTime.Now;
                    newID = adaptor.GetNewDataSourceID();
                    processDuration = DateTime.Now - StartTime;
                    retrieved = true;
                }
                catch (Exception ex)
                {
                    retryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to get ID (try {retryTimes} of {ProcessMaximumRetryTimes}); Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTimes < ProcessMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTimes, "Retry getting IDs...");
                    }
                    else
                    {
                        string message = $"Unable to get ID after {ProcessMaximumRetryTimes} retri(es); Inner-exception contains the last try exception";
                        WriteLog(message);
                        throw new UnableToPublishException(message, ex);
                    }
                }
            }
            WriteLog($"Data Source ID given: '{newID}'; Duration: {processDuration:h\\:mm\\:ss\\.fff}");
            return newID;
        }

        private void WaitProcessForRetry(int uploadRetryTime, string postWaitPrompt)
        {
            int waitMiliseconds = (int)(Math.Pow(2, uploadRetryTime) * 250);
            WriteLog(string.Format("Wait {0} miliseconds...", waitMiliseconds), false);
            Task.Delay(waitMiliseconds).Wait();
            WriteLog(postWaitPrompt);
        }

        private static string GetExceptionDetailsString(Exception ex)
        {
            var detailGenerator = new ExceptionDetailGenerator();
            string exceptionDetails = detailGenerator.GetDetails(ex);
            return exceptionDetails;
        }

        private void WriteLog(string message, bool isDetailedLog = true, bool reportInuseMemory = false)
        {
            if (logger != null)
            {
                if (isDetailedLog && !ReportFullDetailsInLog)
                    return;
                logger.WriteLog(message, reportInuseMemory);
            }
        }

        
    }
}
