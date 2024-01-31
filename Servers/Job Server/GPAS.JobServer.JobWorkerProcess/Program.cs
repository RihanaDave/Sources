using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.ConceptsToGenerate.Serialization;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv;
using GPAS.DataImport.Transformation;
using GPAS.JobServer.Logic;
using GPAS.JobServer.Logic.Entities;
using GPAS.JobServer.Logic.Entities.ConfigElements;
using GPAS.JobServer.Logic.Import;
using GPAS.JobsManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using static GPAS.JobServer.Logic.FileServiceProvider;

namespace GPAS.JobServer.JobWorkerProcess
{
    public class Program
    {
        private static bool ImportInExtractOnlyMode = false;
        private static bool ImportInPublishExtractedConceptsMode = false;
        private static string SavedExtractedConceptsPath = "./ExtractedConcepts-{0}.kxml";
        private static Logger.ExceptionDetailGenerator exDetailGenerator = new Logger.ExceptionDetailGenerator();
        private static Ontology.Ontology CurrentOntology = null;

        public static int JobID
        {
            get { return jobId; }
        }
        private static int jobId;
        private static ProcessLogger logger;

        /// <summary>
        /// </summary>
        /// <param name="args">
        ///     arg[0] : شناسه کاری که می‌خواهیم توسط این پروسه اجرا شود
        ///     arg[1] : پارامتر اجرای کار
        /// </param>

        static int Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "1026", "-extractonly" };
            //args = new string[] { "1043", "-publishextractedconcepts" };
            //args = new string[] { "1" };
#endif
            // اعتبارسنجی شناسه کار ورودی
            // حداقل یک ورودی داشته باشد
            if (args.Length < 1)
                return 0;
            // ورودی اول یک عدد صحیح باشد
            if (int.TryParse(args[0], out jobId))
            {
                // ورودی دوم پارامتر اختیاری برای نحوه‌ی اجرای کار است
                if (args.Length >= 2 && args[1].StartsWith("-"))
                {
                    if (args[1].ToLower().StartsWith("-extractonly"))
                    {
                        if (File.Exists(SavedExtractedConceptsPath))
                        {
                            Console.WriteLine(string.Format("Sorry; File '{0}' exists!", SavedExtractedConceptsPath));
                            return 0;
                        }
                        ImportInExtractOnlyMode = true;
                    }
                    else if (args[1].ToLower().StartsWith("-publishextractedconcepts"))
                    {
                        if (!File.Exists(SavedExtractedConceptsPath))
                        {
                            Console.WriteLine(string.Format("File '{0}' not exists!", SavedExtractedConceptsPath));
                            return 0;
                        }
                        ImportInPublishExtractedConceptsMode = true;
                    }
                }
                return StartJobWork();
            }
            else
                return 0;
        }

        /// <summary>
        /// اجرای یک درخواست از بین درخواست‌های ذخیره شده توسط سرور کار
        /// </summary>
        /// <remarks>
        /// در متن مستندات این پروژه معادل‌های فارسی از قرار زیر می‌باشند:
        ///     Job => کار
        /// </remarks>
        public static int StartJobWork()
        {
            try
            {
                OntologyProvider.Init();
                CurrentOntology = OntologyProvider.GetOntology();
            }
            catch (Exception ex)
            {
                return FinalizeWorkerWithReportFailure(new Exception("Unable to initaialize Ontology", ex));
            }

            SavedExtractedConceptsPath = string.Format(SavedExtractedConceptsPath, jobId);

            Request jobRequest;
            try
            {
                // فراخوانی کار مربوطه و شناسه یکتای کار
                jobRequest = JobsStoreAndRetrieveProvider.GetJobById(jobId);
                if (jobRequest == null)
                    throw new NullReferenceException("Unable to retrive Job data from Job-Server DB");
            }
            catch (Exception ex)
            {
                return FinalizeWorkerWithReportFailure(new Exception($"Unable to Start Job-Worker: {jobId}", ex));
            }

            try
            {
                logger = new ProcessLogger();

                if (ImportInExtractOnlyMode)
                {
                    logger.Initialization(jobId, " - Extract Only");
                }
                else if (ImportInPublishExtractedConceptsMode)
                {
                    logger.Initialization(jobId, " - Publish Extracted Concepts");
                }
                else
                {
                    logger.Initialization(jobId);
                }
                logger.WriteLog(ProcessLogger.LogTypes.JobInitialized);
            }
            catch (Exception ex)
            {
                return FinalizeWorkerWithReportFailure(new Exception("Unable to initaialize Job-Worker Log file", ex));
            }

            // اعتبارسنجی درخواست
            if (jobRequest is SemiStructuredDataImportRequest)
            {
                SemiStructuredDataImportRequest ssImportRequest = jobRequest as SemiStructuredDataImportRequest;
                TypeMapping ssTypeMapping = ssImportRequest.ImportMapping as TypeMapping;
                ACL dataSourceAcl = ssImportRequest.DataSourceACL;

                JobRequestStatus currentJobStatus = JobsStoreAndRetrieveProvider.GetJobStatus(jobId);

                switch (currentJobStatus)
                {
                    case JobRequestStatus.Pending:
                        logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Job '{jobId}' started");
                        return StartJobImporting(ssImportRequest, ssTypeMapping, dataSourceAcl);
                    case JobRequestStatus.Resume:
                        if (File.Exists(SavedExtractedConceptsPath))
                        {
                            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Job '{jobId}' resumed from existing file");
                            return ResumeImportedJob(ssTypeMapping, dataSourceAcl);
                        }
                        else
                        {
                            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Job '{jobId}' resumed");
                            return StartJobImporting(ssImportRequest, ssTypeMapping, dataSourceAcl);
                        }

                    default:
                        throw new InvalidEnumArgumentException("Unrecognized job request status");
                }
            }
            else
            {
                return FinalizeWorkerWithReportFailure("The job request is unknown in this JobWorkerProcess");
            }
        }

        private static int StartJobImporting(SemiStructuredDataImportRequest ssImportRequest, TypeMapping ssTypeMapping, ACL dataSourceAcl)
        {
            JobsStoreAndRetrieveProvider.SetBusyStateForJob(jobId);

            if (ssImportRequest.ImportMaterial is CsvFileMaterial
                || ssImportRequest.ImportMaterial is ExcelSheet
                || ssImportRequest.ImportMaterial is AccessTable
            )
            {
                #region ورود داده‌ها از فایل (CSV, Excel)
                string sourceFileLocalPath;
                jobShareInUseFile = GetSourceFileRemotePathByMaterial(ssImportRequest.ImportMaterial);
                string sourceFileTypeTitle = GetSourceFileTypeTitleByMaterial(ssImportRequest.ImportMaterial);
                try
                {
                    sourceFileLocalPath = DownloadFile(jobShareInUseFile, sourceFileTypeTitle);
                }
                catch (Exception ex)
                {
                    return FinalizeWorkerWithReportFailure(new Exception($"{sourceFileTypeTitle} file download failed", ex));
                }

                List<ImportingObject> importingObjects;
                List<ImportingRelationship> importingRelationships;
                try
                {
                    SemiStructuredDataTransformer transformer = PerformConceptsTransformation(ssImportRequest, sourceFileLocalPath);
                    importingObjects = transformer.GeneratingObjects;
                    importingRelationships = transformer.GeneratingRelationships;

                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo,
                        string.Format("Data Extraction with Internal Resolution process completed. Extracted concepts: Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                        , importingObjects.Count
                        , importingObjects.Sum(io => io.GetProperties().Count())
                        , importingRelationships.Count));
                }
                catch (Exception ex)
                {
                    return FinalizeWorkerWithReportFailure(new Exception($"Extracting concepts from {sourceFileTypeTitle} file data failed", ex));
                }

                long dataSourceID = -1;
                try
                {
                    string dataSourceName = DataSourceMetadata.GetSemiStructuredDataSourceName(ssImportRequest.ImportMaterial);
                    DataSourceType dataSourceType = DataSourceMetadata.GetSemiStructuredDataSourceType(ssImportRequest.ImportMaterial);
                    dataSourceID = RegisterNewDataSourceForSharedSourceFile(jobShareInUseFile, dataSourceName, dataSourceType, dataSourceAcl);
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Registering Data Source failed", ex)); }

                try
                {
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Serialize to file ...");
                    Serializer serializer = new Serializer();
                    serializer.SerializeToFile(SavedExtractedConceptsPath, importingObjects, importingRelationships, dataSourceID);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Serialization compeleted.");
                }
                catch (Exception ex)
                {
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, exDetailGenerator.GetDetails(ex));
                }

                try
                { PublishConcepts(importingObjects, importingRelationships, ssTypeMapping, dataSourceID, dataSourceAcl); }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Publish extracted concepts was failed", ex)); }

                // تعیین وضعیت نهایی کار
                return FinalizeWorkerWithReportSuccessfulFinish
                    (string.Format("Data Import completed successfully. Extracted and published concepts; Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                        , importingObjects.Count
                        , importingObjects.Sum(io => io.GetProperties().Count())
                        , importingRelationships.Count));
                #endregion
            }
            else if (ssImportRequest.ImportMaterial is AttachedDatabaseTableMaterial)
            {
                #region ورود داده‌های از بانک اطلاعاتی الصاق شده
                if (ImportInExtractOnlyMode)
                {
                    logger.WriteLog("'Extract Only Mode' is not supported for 'Attached Database Table/View' import");
                    return 0;
                }

                var material = ssImportRequest.ImportMaterial as AttachedDatabaseTableMaterial;

                #region Read needed configurations
                long BatchMaximumFields;
                if (!long.TryParse(ConfigurationManager.AppSettings["AttachedDatabaseImportBatchMaximumFields"], out BatchMaximumFields))
                    return FinalizeWorkerWithReportFailure("Unable to read App Setting 'AttachedDatabaseImportBatchMaximumFields' from config");

                string serverKey;
                string databaseName;
                try
                {
                    serverKey = AttachedDatabaseTableMaterial.GetServerKeyFromDatabaseUri(material.DatabaseUri);
                    databaseName = AttachedDatabaseTableMaterial.GetDatabaseNameFromDatabaseUri(material.DatabaseUri);
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Unable to get 'Server Key' and/or 'Database Name' from 'Database Uri'", ex)); }

                DatabaseServer SourceServer = null;
                try
                {
                    Configuration serviceConfig = WebConfigurationManager.OpenWebConfiguration("\\", "Job Service");
                    JobServerDatabases configDatabases = (JobServerDatabases)serviceConfig.GetSection("JobServerDatabases");

                    foreach (DatabaseServer server in configDatabases.Servers)
                    {
                        if (server.Key.Equals(serverKey))
                        {
                            SourceServer = server;
                            break;
                        }
                    }
                    if (SourceServer == null)
                        return FinalizeWorkerWithReportFailure("Defined server for the request not matches any database in 'Job-Server Databases' configuration");
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Unable to read 'Job-Server Database' data from config", ex)); }
                #endregion

                #region Retrieve Table/View Schema
                List<ImportingObject> importingObjects;
                List<ImportingRelationship> importingRelationships;
                string[][] dataSourceFields = null;
                int timeout = 2592000; // یک ماه
                SqlConnection connection = null;
                SqlCommand command;
                DbDataReader reader;
                DataColumnCollection tableColumns;
                try
                {
                    string connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};Connection Timeout={4};"
                        , SourceServer.HostAddress, databaseName, SourceServer.UserName, SourceServer.Password, timeout.ToString());

                    connection = new SqlConnection(connectionString);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingStarted, "Start retrieve table/view data...");

                    // Read column headers
                    command = new SqlCommand
                        (string.Format("SELECT TOP 0 * FROM [{0}]", material.TableName), connection);
                    command.CommandTimeout = timeout;

                    connection.Open();
                    reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
                    DataSet emptyDataSet = new DataSet();
                    emptyDataSet.Load(reader, LoadOption.PreserveChanges, new string[] { material.TableName });
                    reader.Close();

                    tableColumns = emptyDataSet.Tables[0].Columns;
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Unable to retrieve Table/View schema", ex)); }
                #endregion

                #region Register New Data Source
                long dataSourceID = -1;
                try
                {
                    byte[] dataSourceContent = DataSourceMetadata.GetNonFileBasedSemiStructuredDataSourceContent(ssImportRequest.ImportMaterial);
                    dataSourceID = RegisterNewDataSourceByContent(dataSourceContent, DataSourceType.AttachedDatabaseTable, "Attached Database Table/View", dataSourceAcl);
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Unable to register Data Source", ex)); }
                #endregion

                long totalPublishedObjectsCount = 0;
                long totalPublishedPropertiesCount = 0;
                long totalPublishedRelationshipsCount = 0;
                long totalBatchesCount = 0;
                try
                {
                    // Initiate fields array
                    long BatchMaximumRows = BatchMaximumFields / tableColumns.Count;
                    if (BatchMaximumRows == 0)
                        throw new InvalidOperationException("Configured 'Batch Maximum Fields' count > Table/View columns count");

                    CreateNewDataSourceFieldsArray(ref dataSourceFields, tableColumns, BatchMaximumRows);

                    // Read table/view data
                    command = new SqlCommand
                        (string.Format("SELECT * FROM [{0}]", material.TableName), connection);
                    command.CommandTimeout = timeout;
                    reader = command.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        throw new ArgumentException("Table/View has no data");
                    }
                    int rowCounter = 1;
                    int previousBatchLastRowNumber = 0;
                    while (reader.Read())
                    {
                        dataSourceFields[rowCounter - previousBatchLastRowNumber] = GetReadRowAsStringArray(reader, tableColumns.Count);
                        if ((rowCounter) % 20000 == 0)
                        {
                            logger.WriteLog(string.Format("{0:N0} rows read", rowCounter), true);
                        }
                        if (rowCounter - previousBatchLastRowNumber >= BatchMaximumRows) // دسته تکمیل شده است
                        {
                            logger.WriteLog(string.Format("{0:N0} rows read (batch is ready)", rowCounter), true);

                            try
                            { TransformDataSource(ref dataSourceFields, out importingObjects, out importingRelationships, ssImportRequest.ImportMapping); }
                            catch (Exception ex)
                            { return FinalizeWorkerWithReportFailure(new Exception("Extracting concepts from table/view data failed", ex)); }

                            try
                            { PublishConcepts(importingObjects, importingRelationships, ssTypeMapping, dataSourceID, dataSourceAcl); }
                            catch (Exception ex)
                            { return FinalizeWorkerWithReportFailure(new Exception("Publish extracted concepts was failed", ex)); }

                            totalPublishedObjectsCount += importingObjects.Count;
                            totalPublishedPropertiesCount += importingObjects.Sum(o => o.Properties.Count);
                            totalPublishedRelationshipsCount += importingRelationships.Count;
                            totalBatchesCount++;

                            CreateNewDataSourceFieldsArray(ref dataSourceFields, tableColumns, BatchMaximumRows);

                            previousBatchLastRowNumber = rowCounter;
                        }
                        rowCounter++;
                    }
                    reader.Close();
                    logger.WriteLog(string.Format("Table/View data ({0:N0} row(s)) retrievation completed", rowCounter - 1), true);

                    if (rowCounter - 1 - previousBatchLastRowNumber > 0)
                    {
                        int remainedFieldsCount = rowCounter - 1 /* to avoid last 'rowCounter++' */ - previousBatchLastRowNumber;
                        Array.Resize(ref dataSourceFields, remainedFieldsCount + 1 /*columns header row*/);

                        try
                        { TransformDataSource(ref dataSourceFields, out importingObjects, out importingRelationships, ssImportRequest.ImportMapping); }
                        catch (Exception ex)
                        { return FinalizeWorkerWithReportFailure(new Exception("Extracting concepts from table/view data failed", ex)); }

                        try
                        { PublishConcepts(importingObjects, importingRelationships, ssTypeMapping, dataSourceID, dataSourceAcl); }
                        catch (Exception ex)
                        { return FinalizeWorkerWithReportFailure(new Exception("Publish extracted concepts was failed", ex)); }

                        totalPublishedObjectsCount += importingObjects.Count;
                        totalPublishedPropertiesCount += importingObjects.Sum(o => o.Properties.Count);
                        totalPublishedRelationshipsCount += importingRelationships.Count;
                        totalBatchesCount++;
                    }

                    // تعیین وضعیت نهایی کار
                    return FinalizeWorkerWithReportSuccessfulFinish
                        (string.Format("Data Import completed successfully in {0:N0} batch(es). Total extracted, internally resolved and published concepts; Objects: {1:N0}, Properties: {2:N0}, Relationships: {3:N0}"
                            , totalBatchesCount, totalPublishedObjectsCount
                            , totalPublishedPropertiesCount, totalPublishedRelationshipsCount));
                }
                catch (Exception ex)
                { return FinalizeWorkerWithReportFailure(new Exception("Table/View data retrieval failed", ex)); }
                finally
                { if (connection != null) connection.Close(); }
                #endregion
            }
            else if (ssImportRequest.ImportMaterial is EmlDirectory)
            {
                #region تبدیل پوشه‌ای از فایل‌های .eml
                string[] sourceCsvFilesLocalPath;
                jobShareInUseDirectory = (ssImportRequest.ImportMaterial as EmlDirectory).DirectoryJobSharePath;
                try
                {
                    sourceCsvFilesLocalPath = ConvertEmlFilesFromDirToLocalCsvs(jobShareInUseDirectory);
                }
                catch (Exception ex)
                {
                    return FinalizeWorkerWithReportFailure(new Exception($".eml files conversion failed", ex));
                }

                long totalImportingObjectsCount = 0;
                long totalImportingPropertiesCount = 0;
                long totalImportingRelationshipsCount = 0;
                for (int sourceCounter = 0; sourceCounter < sourceCsvFilesLocalPath.Length; sourceCounter++)
                {
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $".eml-converted-CSV batch {sourceCounter + 1} of {sourceCsvFilesLocalPath.Length} started...");

                    List<ImportingObject> importingObjects;
                    List<ImportingRelationship> importingRelationships;
                    try
                    {
                        SemiStructuredDataTransformer transformer = PerformConceptsTransformation(ssImportRequest, sourceCsvFilesLocalPath[sourceCounter]);
                        importingObjects = transformer.GeneratingObjects;
                        importingRelationships = transformer.GeneratingRelationships;

                        logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo,
                            string.Format("Data Extraction with Internal Resolution process completed. Extracted concepts: Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                            , importingObjects.Count
                            , importingObjects.Sum(io => io.GetProperties().Count())
                            , importingRelationships.Count));
                    }
                    catch (Exception ex)
                    {
                        return FinalizeWorkerWithReportFailure(new Exception($"Extracting concepts from .eml-sourced-CSV file data failed", ex));
                    }

                    long dataSourceID = -1;
                    try
                    {
                        string jobSharePath = UploadFileToJobShare(sourceCsvFilesLocalPath[sourceCounter]);
                        string dataSourceName = DataSourceMetadata.GetSemiStructuredDataSourceName(ssImportRequest.ImportMaterial);
                        DataSourceType dataSourceType = DataSourceMetadata.GetSemiStructuredDataSourceType(ssImportRequest.ImportMaterial);
                        dataSourceID = RegisterNewDataSourceForSharedSourceFile(jobSharePath, dataSourceName, dataSourceType, dataSourceAcl);
                    }
                    catch (Exception ex)
                    { return FinalizeWorkerWithReportFailure(new Exception("Registering Data Source failed", ex)); }

                    if (ImportInExtractOnlyMode)
                    {
                        try
                        {
                            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Serialize to file ...");
                            Serializer serializer = new Serializer();
                            serializer.SerializeToFile(SavedExtractedConceptsPath, importingObjects, importingRelationships, dataSourceID);
                            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Serialization compeleted.");
                        }
                        catch (Exception ex)
                        {
                            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, exDetailGenerator.GetDetails(ex));
                        }
                        return 0;
                    }

                    try
                    { PublishConcepts(importingObjects, importingRelationships, ssTypeMapping, dataSourceID, dataSourceAcl); }
                    catch (Exception ex)
                    { return FinalizeWorkerWithReportFailure(new Exception("Publish extracted concepts was failed", ex)); }

                    long batchImportingPropertiesCount = importingObjects.Sum(io => io.GetProperties().Count());
                    totalImportingObjectsCount += importingObjects.Count;
                    totalImportingPropertiesCount += batchImportingPropertiesCount;
                    totalImportingRelationshipsCount += importingRelationships.Count;

                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Batch {sourceCounter + 1} completed; Batch extracted and published concepts; Objects: {importingObjects.Count:N0}, Properties: {batchImportingPropertiesCount:N0}, Relationships: {importingRelationships.Count:N0}");
                }

                // تعیین وضعیت نهایی کار
                return FinalizeWorkerWithReportSuccessfulFinish
                    (string.Format("Data Import completed successfully. Total extracted and published concepts; Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                        , totalImportingObjectsCount, totalImportingPropertiesCount, totalImportingRelationshipsCount));
                #endregion
            }
            else
            {
                return FinalizeWorkerWithReportFailure("The import request is not supported in this JobWorkerProcess");
            }
        }

        private static int ResumeImportedJob(TypeMapping ssTypeMapping, ACL dataSourceAcl)
        {
            JobsStoreAndRetrieveProvider.SetBusyStateForJob(jobId);
            List<ImportingObject> importingObjects;
            List<ImportingRelationship> importingRelationships;
            long dataSourceID;
            try
            {
                logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Deserializing stored concepts ...");
                Serializer serializer = new Serializer();
                Tuple<List<ImportingObject>, List<ImportingRelationship>, long> importingConcepts
                    = serializer.DeserializeFromFile(SavedExtractedConceptsPath);
                logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Deserialize compeleted.");
                importingObjects = importingConcepts.Item1;
                importingRelationships = importingConcepts.Item2;
                dataSourceID = importingConcepts.Item3;
            }
            catch (Exception ex)
            {
                logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, exDetailGenerator.GetDetails(ex));
                return 0;
            }

            try
            {
                int lastPublishedObjectIndex = JobsStoreAndRetrieveProvider.GetLastImportingObjectIndex(jobId);
                int lastPublishedRelationIndex = JobsStoreAndRetrieveProvider.GetLastImportingRelationIndex(jobId);

                PublishConcepts(importingObjects, importingRelationships, ssTypeMapping, dataSourceID, dataSourceAcl,
                    lastPublishedObjectIndex, lastPublishedRelationIndex);
            }
            catch (Exception ex)
            { return FinalizeWorkerWithReportFailure(new Exception("Publish extracted concepts was failed", ex)); }

            // تعیین وضعیت نهایی کار
            return FinalizeWorkerWithReportSuccessfulFinish
                (string.Format("Data Import completed successfully. Extracted and published concepts; Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                    , importingObjects.Count
                    , importingObjects.Sum(io => io.GetProperties().Count())
                    , importingRelationships.Count));
        }

        private static void ReportLastIndexeOfImportingObjectInStopTime(int lastImportingObjectIndex, int totalObjectsIndex)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, string.Format("Last published object index is {0}", lastImportingObjectIndex));
            JobsStoreAndRetrieveProvider.SetLastImportingObjectIndex(jobId, lastImportingObjectIndex, totalObjectsIndex);
        }

        private static void ReportLastIndexeOfImportingRelationInStopTime(int lastRelationshipIndex, int totalRelationshipsIndex)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, string.Format("Last published relationship index is {0}", lastRelationshipIndex));
            JobsStoreAndRetrieveProvider.SetLastImportingRelationIndex(jobId, lastRelationshipIndex, totalRelationshipsIndex);
        }

        /// <summary></summary>
        /// <returns>Job-Share relative path</returns>
        public static string UploadFileToJobShare(string localPath)
        {
            return FileServiceProvider.UploadFileToJobShare(localPath);
        }

        private static SemiStructuredDataTransformer PerformConceptsTransformation(SemiStructuredDataImportRequest ssImportRequest, string sourceFileLocalPath)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Transforming data ...");
            SemiStructuredDataTransformer transformer = new SemiStructuredDataTransformer(CurrentOntology, logger);
            if (ssImportRequest.ImportMaterial is CsvFileMaterial)
            {
                CsvFileMaterial csvMaterial = ssImportRequest.ImportMaterial as CsvFileMaterial;
                FileStream csvStream = null;
                try
                {
                    csvStream = new FileStream(sourceFileLocalPath, FileMode.Open, FileAccess.Read);
                    if (csvStream.Length == 0)
                        throw new ArgumentException("File has no content");
                    transformer.TransformConcepts(csvStream, csvMaterial.Separator, ssImportRequest.ImportMapping);
                }
                finally
                {
                    if (csvStream != null)
                        csvStream.Close();
                }
            }
            else if (ssImportRequest.ImportMaterial is ExcelSheet)
            {
                ExcelSheet excelMaterial = ssImportRequest.ImportMaterial as ExcelSheet;
                transformer.TransformConcepts(sourceFileLocalPath, excelMaterial.SheetName, ssImportRequest.ImportMapping);
            }
            else if (ssImportRequest.ImportMaterial is AccessTable)
            {
                AccessTable accessMaterial = ssImportRequest.ImportMaterial as AccessTable;
                transformer.TransformConceptsFromAccessTable(sourceFileLocalPath, accessMaterial.TableName, ssImportRequest.ImportMapping);
            }
            else if (ssImportRequest.ImportMaterial is EmlDirectory)
            {
                EmlDirectory emlSourcedMaterial = ssImportRequest.ImportMaterial as EmlDirectory;
                FileStream csvStream = null;
                try
                {
                    csvStream = new FileStream(sourceFileLocalPath, FileMode.Open, FileAccess.Read);
                    if (csvStream.Length == 0)
                        throw new ArgumentException("File has no content");
                    transformer.TransformConcepts(csvStream, ',', ssImportRequest.ImportMapping);
                }
                finally
                {
                    if (csvStream != null)
                        csvStream.Close();
                }
            }

            else
            {
                throw new NotSupportedException("Unknown material type");
            }
            return transformer;
        }

        private static string GetSourceFileRemotePathByMaterial(MaterialBase importMaterial)
        {
            if (importMaterial is CsvFileMaterial)
                return ((CsvFileMaterial)importMaterial).FileJobSharePath;
            else if (importMaterial is ExcelSheet)
                return ((ExcelSheet)importMaterial).FileJobSharePath;
            else if (importMaterial is AccessTable)
                return ((AccessTable)importMaterial).FileJobSharePath;
            else
                throw new NotSupportedException("Unknown file-based data sourcet type");
        }

        private static string GetSourceFileTypeTitleByMaterial(MaterialBase importMaterial)
        {
            if (importMaterial is CsvFileMaterial)
                return "CSV";
            else if (importMaterial is ExcelSheet)
                return "Excel";
            else if (importMaterial is AccessTable)
                return "Access";
            else
                throw new NotSupportedException("Unknown file-based data sourcet type");
        }

        private static string DownloadFile(string filePath, string fileTypeTitle)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Start downloading {fileTypeTitle} file ...");
            string sourceFileLocalPath = FileServiceProvider.DownloadDataSource
                (filePath, ConfigurationManager.AppSettings["ImportDataSourceTempPath"]);
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"{fileTypeTitle} file downloaded");
            return sourceFileLocalPath;
        }

        private static string[] ConvertEmlFilesFromDirToLocalCsvs(string sourceDirOnJobShare)
        {
            if (sourceDirOnJobShare == null)
                throw new ArgumentNullException(nameof(sourceDirOnJobShare));

            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Prepare .eml files conversion...");

            string importTempDirPath = ConfigurationManager.AppSettings["ImportDataSourceTempPath"];
            if (importTempDirPath == null)
                throw new ConfigurationErrorsException("'ImportDataSourceTempPath' App-setting not defined");
            if (!Directory.Exists(importTempDirPath))
                Directory.CreateDirectory(importTempDirPath);

            DirectoryInfo targetLocalDirInfo = new DirectoryInfo($"{importTempDirPath}\\{sourceDirOnJobShare}");
            if (Directory.Exists(targetLocalDirInfo.FullName))
                Directory.Delete(targetLocalDirInfo.FullName, true);
            targetLocalDirInfo.Create();
            targetLocalDirInfo.Refresh();
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"Import temp path: '{targetLocalDirInfo.FullName}'");

            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Download source directory...");
            FileInfo[] sourceEmlFiles = DownloadDirectoryDataSource(sourceDirOnJobShare, targetLocalDirInfo);
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $"{sourceEmlFiles.Length} files downloaded");

            DirectoryInfo attachmentsDirInfo = targetLocalDirInfo.CreateSubdirectory("_Att");

            Convertor convertor = new Convertor()
            {
                SourceFiles = sourceEmlFiles,
                TargetDirectoryPath = targetLocalDirInfo.FullName,
                AttachmentsPathPrefix = attachmentsDirInfo.FullName,
                Logger = logger,
                SpliteCsvFiles = bool.Parse(ConfigurationManager.AppSettings["EmlToCsvConvertOutputFileSplite"]),
                SplitedCsvFileMaxLinesCount = int.Parse(ConfigurationManager.AppSettings["EmlToCsvConvertOutputFileMaxRows"]),
                ReportFullDetails = bool.Parse(ConfigurationManager.AppSettings["ReportFullDetailsInImportLog"])
            };
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Start .eml files conversion...");
            string[] sourceCsvFilesLocalPath = convertor.PerformConversionToCsvFiles();
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, $".eml files converted.");
            return sourceCsvFilesLocalPath;
        }

        private static long RegisterNewDataSourceForSharedSourceFile(string fileJobSharePath, string dataSourceTitle, DataSourceType dataSourceType, ACL dataSourceAcl)
        {
            DataSourceMetadata dataSource = new DataSourceMetadata()
            {
                Content = null,
                Name = dataSourceTitle,
                Description = "",
                Type = dataSourceType,
                Acl = dataSourceAcl
            };
            long dataSourceID = PublishManager.RegisterNewDataSourceForSharedFile(dataSource, fileJobSharePath, logger);
            return dataSourceID;
        }

        private static long RegisterNewDataSourceByContent(byte[] dataSourceContent, DataSourceType dataSourceType, string dataSourceTitle, ACL dataSourceAcl)
        {
            DataSourceMetadata dataSource = new DataSourceMetadata()
            {
                Content = dataSourceContent,
                Name = dataSourceTitle,
                Description = "",
                Type = dataSourceType,
                Acl = dataSourceAcl
            };
            long dataSourceID = PublishManager.RegisterNewSemiStructuredDataSource(dataSource, logger);
            return dataSourceID;
        }

        private static string[] GetReadRowAsStringArray(DbDataReader reader, int columnsCount)
        {
            var result = new string[columnsCount];
            object[] rowValues = new object[columnsCount];
            reader.GetValues(rowValues);
            for (int i = 0; i < columnsCount; i++)
            {
                result[i] = rowValues[i].ToString();
            }
            return result;
        }

        private static void CreateNewDataSourceFieldsArray(ref string[][] fieldsArray, DataColumnCollection tableColumns, long rowsCount)
        {
            fieldsArray = new string[rowsCount + 1 /*columns header row*/][];
            fieldsArray[0] = new string[tableColumns.Count];
            for (int i = 0; i < tableColumns.Count; i++)
            {
                fieldsArray[0][i] = tableColumns[i].ColumnName;
            }
        }

        private static void TransformDataSource(ref string[][] dataSourceFields, out List<ImportingObject> importingObjects, out List<ImportingRelationship> importingRelationships, TypeMapping importMapping)
        {
            SemiStructuredDataTransformer transformer = new SemiStructuredDataTransformer(CurrentOntology, logger);
            transformer.TransformConcepts(ref dataSourceFields, importMapping);
            importingObjects = transformer.GeneratingObjects;
            importingRelationships = transformer.GeneratingRelationships;

            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo,
                string.Format("Data Extraction with Internal Resolution process completed. Extracted concepts: Objects: {0:N0}, Properties: {1:N0}, Relationships: {2:N0}"
                , importingObjects.Count
                , importingObjects.Sum(io => io.GetProperties().Count())
                , importingRelationships.Count));
        }

        private static void PublishConcepts(
            List<ImportingObject> importingObjects
            , List<ImportingRelationship> importingRelationships
            , TypeMapping ssTypeMapping
            , long dataSourceID
            , ACL dataSourceAcl
            , int lastPublishedObjectIndex = -1
            , int lastPublishedRelationIndex = -1)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Start Publishing extracted concepts ...");
            PublishManager.ImportingObjectBatchPublished += PublishManager_ImportingObjectBatchPublished;
            PublishManager.ImportingRelationBatchPublished += PublishManager_ImportingRelationBatchPublished;
            PublishManager.GenerateImportingExtractedConcepts(importingObjects, importingRelationships, dataSourceID, dataSourceAcl, logger, lastPublishedObjectIndex, lastPublishedRelationIndex);
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Publish completed.");
        }

        private static void PublishManager_ImportingRelationBatchPublished(object sender, PublishedEventArgs e)
        {
            ReportLastIndexeOfImportingRelationInStopTime(e.LastIndexOfConcept, e.TotalImportingConcept);
        }

        private static void PublishManager_ImportingObjectBatchPublished(object sender, PublishedEventArgs e)
        {
            ReportLastIndexeOfImportingObjectInStopTime(e.LastIndexOfConcept, e.TotalImportingConcept);
        }

        static string jobShareInUseDirectory = null;
        static string jobShareInUseFile = null;

        private static void CleanupTempDataSourceCopies()
        {
            try
            {
                FileServiceProvider.CleanupDataSourceTempPath();
                logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Data Source temp path clean-ed up.");
            }
            catch (Exception ex)
            {
                var message = exDetailGenerator.GetDetails(new Exception("Unable to cleanup import data source temp path", ex));
                logger.WriteLog(ProcessLogger.LogTypes.JobWorkingWarning, message);
            }
            if (jobShareInUseFile != null)
            {
                try
                {
                    FileServiceProvider.DeleteJobShareFile(jobShareInUseFile);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Job-Share file deleted.");
                }
                catch (Exception ex)
                {
                    var message = exDetailGenerator.GetDetails(ex);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingWarning, message);
                }
            }
            if (jobShareInUseDirectory != null)
            {
                try
                {
                    FileServiceProvider.DeleteJobShareFolder(jobShareInUseDirectory);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingInfo, "Job-Share directory deleted.");
                }
                catch (Exception ex)
                {
                    var message = exDetailGenerator.GetDetails(ex);
                    logger.WriteLog(ProcessLogger.LogTypes.JobWorkingWarning, message);
                }
            }
        }

        private static int FinalizeWorkerWithReportSuccessfulFinish(string message)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingSuccessed, message);
            JobsStoreAndRetrieveProvider.SetSuccessStateForJob(jobId, message);
            JobsStoreAndRetrieveProvider.RestartJobMonitoringAgent();
            if (File.Exists(SavedExtractedConceptsPath))
            {
                File.Delete(SavedExtractedConceptsPath);
            }

            if (Directory.Exists("C:/JobWorkerLogs/Mappings/"))
            {
                Directory.Delete("C:/JobWorkerLogs/Mappings/");
            }

            CleanupTempDataSourceCopies();
            logger.Finalization();
            return 0;
        }

        private static int FinalizeWorkerWithReportFailure(string message)
        {
            logger.WriteLog(ProcessLogger.LogTypes.JobWorkingFailed, message);
            JobsStoreAndRetrieveProvider.SetFailStateForJob(jobId, message);
            CleanupTempDataSourceCopies();
            logger.Finalization();
            return 0;
        }

        private static int FinalizeWorkerWithReportFailure(Exception exception)
        {
            return FinalizeWorkerWithReportFailure(exDetailGenerator.GetDetails(exception));
        }
    }
}