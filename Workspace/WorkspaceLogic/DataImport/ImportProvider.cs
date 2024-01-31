using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv;
using GPAS.DataImport.Transformation;
using GPAS.FtpServiceAccess;
using GPAS.Logger;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic.DataImport;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using GPAS.Workspace.ViewModel.DataImport;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// متصدی ورود داده‌ها در لایه منطق سمت محیط کاربری
    /// </summary>
    public class ImportProvider
    {
        public static readonly string emlDirectoryPath = string.Format("{0}{1}", ConfigurationManager.AppSettings["WorkspaceTempFolderPath"], "Eml Files Attachments");
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private ImportProvider()
        { }


        public static async Task ServerSideSemiStructuredDataImportAsync(Dictionary<MaterialBase, TypeMapping> importDataDictionary)
        {
            await ServerSideSemiStructuredDataImportAsync(importDataDictionary, UserAccountControlProvider.ImportACL);
        }

        /// <summary>
        /// یک لیست از نگاشت ها و اطلاعات مربوط به آن می گیرد
        /// در صورتی که نگاشتی انجام شده باشد آنرا به سرور انتقال می دهد.
        /// </summary>
        /// <param name="importDataDictionary"></param>
        public static async Task ServerSideSemiStructuredDataImportAsync(Dictionary<MaterialBase, TypeMapping> importDataDictionary, ACL acl)
        {
            if (importDataDictionary == null)
                throw new ArgumentNullException("importDataDictionary");
            if (importDataDictionary.Values == null)
                throw new ArgumentNullException("importDataDictionary");

            if (importDataDictionary.Count == 0)
                return;

            var newRequestsMetadata = new SemiStructuredDataImportRequestMetadata[importDataDictionary.Count];

            // اعتبارسنجی و آماده‌سازی مواد خام ورودی
            int requestsCounter = 0;
            foreach (var item in importDataDictionary)
            {
                var material = item.Key;
                if (material is CsvFileMaterial)
                {
                    if (string.IsNullOrWhiteSpace((material as CsvFileMaterial).FileJobSharePath))
                        throw new InvalidOperationException("Data-Source Job-Share path not set");
                }
                else if (material is ExcelSheet)
                {
                    if (string.IsNullOrWhiteSpace((material as ExcelSheet).FileJobSharePath))
                        throw new InvalidOperationException("Data-Source Job-Share path not set");
                }
                else if (material is AccessTable)
                {
                    if (string.IsNullOrWhiteSpace((material as AccessTable).FileJobSharePath))
                        throw new InvalidOperationException("Data-Source Job-Share path not set");
                }
                else if (material is EmlDirectory)
                {
                    if (string.IsNullOrWhiteSpace((material as EmlDirectory).DirectoryJobSharePath))
                        throw new InvalidOperationException("Data-Source Job-Share path not set");
                }

                StreamUtility streamUtil = new StreamUtility();

                MaterialBaseSerializer materialBaseSerializer = new MaterialBaseSerializer();
                MemoryStream materialStream = new MemoryStream();
                materialBaseSerializer.Serialize(materialStream, material);

                byte[] materialBytes = streamUtil.ReadStreamAsBytesArray(materialStream);

                TypeMappingSerializer typeMappingSerializer = new TypeMappingSerializer();
                MemoryStream mappingStream = new MemoryStream();
                typeMappingSerializer.Serialize(mappingStream, item.Value);
                byte[] mappingBytes = streamUtil.ReadStreamAsBytesArray(mappingStream);

                ACLSerializer aCLSerializer = new ACLSerializer();
                MemoryStream aclStream = new MemoryStream();
                aCLSerializer.Serialize(aclStream, acl);
                byte[] aclBytes = streamUtil.ReadStreamAsBytesArray(aclStream);

                newRequestsMetadata[requestsCounter++] = new SemiStructuredDataImportRequestMetadata()
                {
                    serializedMaterialBase = materialBytes,
                    serializedTypeMapping = mappingBytes,
                    serializedACL = aclBytes
                };
            }

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                await sc.RegisterNewImportRequestsAsync(newRequestsMetadata.ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public static WorkSpaceSidePublishResult WorkspaceSideSemiStructuredDataImport
            (List<ImportingObject> generatingObjects, List<ImportingRelationship> generatingRelationships
            , DataSourceMetadata dataSource)
        {
            if (generatingObjects == null)
                throw new ArgumentNullException(nameof(generatingObjects));
            if (generatingRelationships == null)
                throw new ArgumentNullException(nameof(generatingRelationships));
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            var adaptor = new DataImport.PublishAdaptor();

            var logger = new ProcessLogger();
            logger.Initialization(System.GenerateNewImportLogFilePath());

            DataSourceProvider dsProvider = new DataSourceProvider();
            long dataSourceID = dsProvider.RegisterNewDataSource(dataSource, adaptor, publishMaximumRetryTimes, reportFullDetailsInLog, logger);

            var publisher = new ConceptsPublisher();
            publisher.InitToPublishFromSemiStructuredSource(generatingObjects, generatingRelationships, adaptor, dataSourceID, dataSource.Acl, logger);
            publisher.ReportFullDetailsInLog = reportFullDetailsInLog;
            publisher.MaximumNumberOfGlobalResolutionCandidates = maximumNumberOfGlobalResolutionCandidates;
            publisher.PublishConcepts();

            logger.Finalization();
            var result = new WorkSpaceSidePublishResult
            {
                IsAnySearchIndexSynchronizationFailureOccured = publisher.IsAnySearchIndexSynchronizationFailureOccured,
            };
            return result;
        }

        private static readonly int publishMaximumRetryTimes = int.Parse(ConfigurationManager.AppSettings["PublishMaximumRetryTimes"]);
        private static readonly int publishMinimumIntervalBetweenLogsInSeconds = int.Parse(ConfigurationManager.AppSettings["MinimumIntervalBetwweenIterrativeLogsReportInSeconds"]);
        private static readonly bool reportFullDetailsInLog = bool.Parse(ConfigurationManager.AppSettings["ReportFullDetailsInImportLog"]);
        private static readonly double publishAcceptableFailsPercentage = double.Parse(ConfigurationManager.AppSettings["PublishAcceptableFailsPercentage"]);
        private static readonly int maximumNumberOfGlobalResolutionCandidates = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfGlobalResolutionCandidates"]);

        
        public static TransformationResult PerformTransformation(Stream csvStream, char separator, TypeMapping importMapping, ProcessLogger logger = null)
        {
            var result = new TransformationResult();
            SemiStructuredDataTransformer transformer = new SemiStructuredDataTransformer(OntologyProvider.GetOntology(), logger);
            transformer.TransformConcepts(csvStream, separator, importMapping);
            result.GeneratingObjects = transformer.GeneratingObjects;
            result.GeneratingRelationships = transformer.GeneratingRelationships;
            result.InvalidLinesCount = transformer.InvalidRowsCount;
            return result;
        }

        public static TransformationResult PerformTransformation(GPAS.DataImport.DataMapping.Unstructured.TypeMapping importMapping, ProcessLogger logger = null)
        {
            var result = new TransformationResult();
            UnstructuredDataTransformer transformer = new UnstructuredDataTransformer(OntologyProvider.GetOntology(), logger);
            transformer.TransformConcepts(importMapping);
            result.GeneratingObjects = transformer.GeneratingObjects;
            result.GeneratingRelationships = transformer.GeneratingRelationships;
            return result;
        }

        public static TransformationResult PerformTransformation(DataTable dataTable, TypeMapping importMapping, ProcessLogger logger = null)
        {
            var result = new TransformationResult();
            SemiStructuredDataTransformer transformer = new SemiStructuredDataTransformer(OntologyProvider.GetOntology(), logger);
            transformer.TransformConcepts(dataTable, importMapping);
            result.GeneratingObjects = transformer.GeneratingObjects;
            result.GeneratingRelationships = transformer.GeneratingRelationships;
            result.InvalidLinesCount = transformer.InvalidRowsCount;
            return result;
        }

        public static TransformationResult PerformTransformation(MaterialBaseVM importMaterial, TypeMapping importMapping)
        {
            if (importMaterial == null)
                throw new ArgumentNullException("importMaterial");
            if (importMapping == null)
                throw new ArgumentNullException("importMapping");

            string logFilePath = System.GenerateNewImportLogFilePath();
            ProcessLogger logger = new ProcessLogger();
            TransformationResult transformationResult = null;

            DataTable materialTotalData;
            GPAS.DataImport.Transformation.Utility TransformUtility = new GPAS.DataImport.Transformation.Utility();


            try
            {
                logger.Initialization(logFilePath);

                if (importMaterial.relatedMaterialBase is CsvFileMaterial)
                {
                    materialTotalData = TransformUtility.GenerateDataTableFromCsvLines
                        (File.ReadLines((importMaterial as CsvFileMaterialVM).CsvFilePath)
                        , (importMaterial.relatedMaterialBase as CsvFileMaterial).Separator, (importMaterial as CsvFileMaterialVM).CsvFilePath);

                    transformationResult = PerformTransformation(materialTotalData, importMapping, logger);
                }
                else if (importMaterial.relatedMaterialBase is ExcelSheet)
                {
                    materialTotalData = TransformUtility.GetDataTableFromExcel
                        ((importMaterial as ExcelSheetMaterialVM).ExcelLocalFilePath
                        , (importMaterial.relatedMaterialBase as ExcelSheet).SheetName);

                    transformationResult = PerformTransformation(materialTotalData, importMapping, logger);
                }
                else if (importMaterial.relatedMaterialBase is AccessTable)
                {
                    materialTotalData = TransformUtility.GetDataTableFromAccessFile
                        ((importMaterial as AccessTableMaterialVM).AccessLocalFilePath
                        , (importMaterial.relatedMaterialBase as AccessTable).TableName);

                    transformationResult = PerformTransformation(materialTotalData, importMapping, logger);
                }
                else if (importMaterial.relatedMaterialBase is DataLakeSearchResultMaterial)
                {
                    materialTotalData = TransformUtility.GenerateDataTableFromCsvLines
                        ((importMaterial.relatedMaterialBase as DataLakeSearchResultMaterial).CachedSearchResultAsCSV, ',', "");

                    transformationResult = PerformTransformation(materialTotalData, importMapping, logger);
                }
                else if (importMaterial.relatedMaterialBase is EmlDirectory)
                {
                    EmlDirectory material = (importMaterial.relatedMaterialBase as EmlDirectory);
                    EmlFileMaterialVM emlFileMaterialVM = (importMaterial as EmlFileMaterialVM);
                    if (!Directory.Exists(emlDirectoryPath))
                    {
                        Directory.CreateDirectory(emlDirectoryPath);
                    }

                    Convertor convertor = new Convertor();
                    convertor.TargetDirectoryPath = emlDirectoryPath;
                    convertor.SourceFiles = emlFileMaterialVM.GetEmlFiles();
                    convertor.AttachmentsPathPrefix = emlDirectoryPath;
                    convertor.SpliteCsvFiles = false;

                    var utility = new GPAS.DataImport.Transformation.Utility();
                    string[] csvPathes = convertor.PerformConversionToCsvFiles();
                    emlFileMaterialVM.workspceSideCsvFilePath = csvPathes.FirstOrDefault();
                    FileStream csvStream = null;

                    try
                    {                        
                        csvStream = new FileStream(csvPathes.FirstOrDefault(), FileMode.Open, FileAccess.Read);
                        if (csvStream.Length == 0)
                            throw new ArgumentException("File has no content");
                        transformationResult = PerformTransformation(csvStream, ',', importMapping, logger);
                    }
                    finally
                    {                        
                        if (csvStream != null)
                            csvStream.Close();
                    }
                }
                else
                    throw new NotSupportedException();
            }
            finally
            {
                logger.Finalization();
            }

            return transformationResult;
        }

        private static LinkDirection ConvertIRRelationshipDirectionToLinkDirection(ImportingRelationshipDirection direction)
        {
            switch (direction)
            {
                case ImportingRelationshipDirection.SourceToTarget:
                    return LinkDirection.SourceToTarget;
                case ImportingRelationshipDirection.TargetToSource:
                    return LinkDirection.TargetToSource;
                case ImportingRelationshipDirection.Bidirectional:
                    return LinkDirection.Bidirectional;
                default:
                    throw new InvalidOperationException("Unknown direction for relationship");
            }
        }


        /// <summary></summary>
        /// <returns>Job-Share relative path</returns>
        public static async Task<string> UploadFileToJobShare(string localPath)
        {
            var ftpService = new FtpServiceProvider
                (string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"])
                , ConfigurationManager.AppSettings["JobShareService_UserName"]
                , ConfigurationManager.AppSettings["JobShareService_Password"]);

            var folderName = Guid.NewGuid().ToString();
            await ftpService.MakeDirectoryAsync(folderName);

            var fi = new FileInfo(localPath);
            var fs = new FileStream(localPath, FileMode.Open);
            var jobSharePath = string.Format("{0}/{1}", folderName, fi.Name);
            await ftpService.UploadAsync(fs, fi.Length, jobSharePath);
            return jobSharePath;
        }

        public class UploadEmlFilesToJobShareStartedEventArgs : EventArgs
        {
            public string UploadingFileName { get; set; }
        }
        public static EventHandler<UploadEmlFilesToJobShareStartedEventArgs> UploadEmlFilesToJobShareStarted;
        private static void OnUploadEmlFilesToJobShareStarted(string fileName)
        {
            UploadEmlFilesToJobShareStarted?.Invoke(null, new UploadEmlFilesToJobShareStartedEventArgs() { UploadingFileName = fileName });
        }

        public static async Task<string> UploadEmlFilesToJobShare(List<string> localEmlFilePaths)
        {
            FtpServiceProvider ftpService = GetNewFtpServiceInstance();
            string jobShareDirectory = Guid.NewGuid().ToString().Replace("-", string.Empty);
            await ftpService.MakeDirectoryAsync(jobShareDirectory);
            try
            {
                foreach (var currentLocalEmlFilePath in localEmlFilePaths)
                {
                    string specialFolderPath = $"{jobShareDirectory}/{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
                    await ftpService.MakeDirectoryAsync(specialFolderPath);

                    var fi = new FileInfo(currentLocalEmlFilePath);
                    var fs = new FileStream(currentLocalEmlFilePath, FileMode.Open);
                    var jobSharePath = $"{specialFolderPath}/{fi.Name}";
                    OnUploadEmlFilesToJobShareStarted(fi.Name);
                    await ftpService.UploadAsync(fs, fi.Length, jobSharePath);
                }
            }
            catch
            {
                await TryDeleteFolder(ftpService, jobShareDirectory);
                throw;
            }
            return jobShareDirectory;
        }

        private static FtpServiceProvider GetNewFtpServiceInstance()
        {
            string servicePath = string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"]);
            string serviceUserName = ConfigurationManager.AppSettings["JobShareService_UserName"];
            string servicePass = ConfigurationManager.AppSettings["JobShareService_Password"];
            var ftpService = new FtpServiceProvider(servicePath, serviceUserName, servicePass);
            return ftpService;
        }

        private static async Task TryDeleteFolder(FtpServiceProvider ftpService, string jobShareDirectory)
        {
            try
            {
                await ftpService.DeleteDirectoryAsync(jobShareDirectory);
            }
            catch (Exception ex)
            {
                System.WriteExceptionLog(ex);
            }
        }

        public static async Task<Tuple<List<KWObject>, bool, Exception[]>> WorkspaceSideUnstructuredDataImportAsync(
            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials)
        {
            return await WorkspaceSideUnstructuredDataImportAsync(unstructuredMaterials, UserAccountControlProvider.ImportACL);
        }

        public static async Task<Tuple<List<KWObject>, bool, Exception[]>> WorkspaceSideUnstructuredDataImportAsync(
            Dictionary<MaterialBaseVM, GPAS.DataImport.DataMapping.Unstructured.TypeMapping> unstructuredMaterials, ACL acl)
        {
            List<KWObject> documentsWithUploadedSource = new List<KWObject>(unstructuredMaterials.Count);
            List<Exception> processExceptions = new List<Exception>(unstructuredMaterials.Count);
            bool peripheralSearchIndexesWereCompletelyUpdated = true;

            foreach (var currentUnstructuredMaterial in unstructuredMaterials)
            {
                try
                {
                    Tuple<KWObject, PublishResultMetadata> unstructuredPublishresult =
                        await PublishManager.PublishImportedUnstructuredDataSourceAsync(
                            currentUnstructuredMaterial.Key.relatedItemToImport.ItemPath,
                            currentUnstructuredMaterial.Value,
                            acl);

                    KWObject obj = unstructuredPublishresult.Item1;
                    documentsWithUploadedSource.Add(obj);
                    PublishResultMetadata pubResult = unstructuredPublishresult.Item2;
                    if (!pubResult.HorizonServerSynchronized || !pubResult.SearchServerSynchronized)
                        peripheralSearchIndexesWereCompletelyUpdated = false;
                }
                catch (Exception ex)
                {
                    processExceptions.Add(ex);
                }
            }

            return new Tuple<List<KWObject>, bool, Exception[]>
                (documentsWithUploadedSource, peripheralSearchIndexesWereCompletelyUpdated, processExceptions.ToArray());
        }

        public async static Task<List<string>> GetUriOfDatabasesForImport()
        {
            List<string> UriOfDatabasesForImport = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                UriOfDatabasesForImport = (await sc.GetUriOfDatabasesForImportAsync()).ToList();
            }
            finally
            {
                sc.Close();
            }
            return UriOfDatabasesForImport;
        }

        public async static Task<List<TableForImport>> GetTablesAndViewsOfDatabaseForImport(string dbForImportURI)
        {
            DataSet tablesAndViewsOfDatabaseForImport = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                tablesAndViewsOfDatabaseForImport = await sc.GetTablesAndViewsOfDatabaseForImportAsync(dbForImportURI);
            }
            finally
            {
                sc.Close();
            }

            if (tablesAndViewsOfDatabaseForImport == null)
                throw new NullReferenceException("Invalid server Response.");

            return GetTablesFromRemoteTables(tablesAndViewsOfDatabaseForImport);

        }
        private static List<TableForImport> GetTablesFromRemoteTables(DataSet remoteTableForImport)
        {
            List<TableForImport> result = new List<TableForImport>();
            for (int i = 0; i < remoteTableForImport.Tables.Count; i++)
            {
                result.Add(new TableForImport()
                {
                    UniqueName = remoteTableForImport.Tables[i].TableName,
                    Preview = remoteTableForImport.Tables[i]
                });
            }
            return result;
        }

        public static char GetInvariantListSeparatorChar()
        {
            char result;
            if (char.TryParse(CultureInfo.InvariantCulture.TextInfo.ListSeparator, out result))
                return result;
            else
                return ',';
        }
    }
}