using GPAS.Logger;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// زیرساخت موردنیاز برای به کارگیری لایه منطق سمت محیط کاربری را فراهم می آورد
    /// </summary>
    public class System
    {
        public static bool NLPServiceVisibility { get; set; }

        public static bool ImageAnalysisServiceVisibility { get; set; }
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private System()
        { }

        public static void MinimalInitialization()
        {
            ExceptionHandler.Init();
        }

        /// <summary>
        /// آماده سازی اولیه لایه منطق سمت محیط کاربری را انجام می دهد
        /// </summary>
        public async static Task InitializationAsync()
        {
            CleanupTempFolders();

            // آماده سازی اولیه متصدی هستان شناسی لایه منطق
            if (!OntologyProvider.IsInitialized())
                await OntologyProvider.InitAsync();

            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            var groupMembershipRelationshipType = ontology.DefaultGroupRelationshipType();
            string labelPropertyTypeUri = ontology.GetDefaultDisplayNamePropertyTypeUri();
            DataAccessManager.ObjectManager.Initialization(labelPropertyTypeUri, groupMembershipRelationshipType);
            DataAccessManager.System.Init(ontology);
            DataSourceProvider.Init();
            Entities.NotLoadedRelationshipBasedKWLink.BaseRelationshipTypeUri = ontology.GetRelationshipTypeURI();
            Entities.NotLoadedEventBasedKWLink.BaseEventTypeUri = ontology.GetEventTypeURI();
            NLPServiceVisibility = await GetNLPServiceVisibilityStatus();
            ImageAnalysisServiceVisibility = await GetImageAnalysisServiceVisibilityStatus();
        }

        public async static Task<bool> GetImageAnalysisServiceVisibilityStatus()
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return (await remoteServiceClient.IsMachneVisonServiceInstalledAsync());
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async static Task<bool> GetNLPServiceVisibilityStatus()
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return (await remoteServiceClient.IsNLPServiceInstalledAsync());
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

#if DEBUG
        public static string GetRemoteServiceMachineAddress()
        {
            ServiceAccess.RemoteService.WorkspaceServiceClient sc = ServiceAccess.RemoteServiceClientFactory.GetNewClient();
            return sc.Endpoint.Address.Uri.Host;
        }
#endif

        /// <summary>
        /// یک GUID برای استفاده در لایه منطق برمی گرداند؛ توصیه می شود نیاز به یک شناسه یکتا برای محیط کاربری از این شناسه تامین گردند
        /// </summary>
        /// <remarks>
        /// در صورت در دسترس بودن شناسه ایجاد شده قبلی برای لایه منطق، این عملکرد همان شناسه را برمی گرداند، در غیر اینصورت یک شناسه جدید ایجاد می کند
        /// این شناسه برای استفاده بعدی، در تنظیمات پروژه لایه منطق محیط کاربری ذخیره می شود
        /// </remarks>
        internal static string GetGUID()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.InuseGUID))
            {
                Properties.Settings.Default.InuseGUID = Guid.NewGuid().ToString();
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.InuseGUID))
                throw new Exception("Unable to generate new GUID");
            return Properties.Settings.Default.InuseGUID;
        }
        /// <summary>
        /// خاتمه کار لایه منطق سمت محیط کاربری را انجام می دهد
        /// </summary>
        public static void Finalization(bool silentFinalization = true)
        {
            try
            {
                GC.Collect();
                CleanupTempFolders();
            }
            catch (Exception)
            {
                if (!silentFinalization)
                    throw;
            }
        }

        private static void CleanupTempFolders()
        {
            if (Directory.Exists(WorkspaceTemperoryFiles.GetTempPath()))
                // پوشه استفاده موقت سمت محیط کاربری را حذف می نماید
                Directory.Delete(WorkspaceTemperoryFiles.GetTempPath(), true);
            if (!OntologyProvider.IsInitialized())
                if (Directory.Exists(OntologyProvider.OntologyTempFolderPath))
                    Directory.Delete(OntologyProvider.OntologyTempFolderPath, true);
            if (Directory.Exists(DataSourceProvider.LocalUnstructuredFolderPath))
                Directory.Delete(DataSourceProvider.LocalUnstructuredFolderPath, true);
        }

        public static void WriteExceptionLog(Exception ex)
        {
            var exLogger = new ExceptionHandler();
            exLogger.WriteErrorLog(ex);
        }

        public static string GetImportLogFolderPath()
        {
            var logPath = ConfigurationManager.AppSettings["ImportLogPath"];
            if (Directory.Exists(logPath) == false)
                Directory.CreateDirectory(logPath);
            return logPath;
        }

        internal static string GenerateNewImportLogFilePath()
        {
            string path;
            int retryCounter = -1;
            do
            {
                path = string.Format("{0}{1:yyyy-MM-dd_hh-mm-ss-tt-ms}.{2}", GetImportLogFolderPath(), DateTime.Now, "txt");
                retryCounter++;
                if (retryCounter >= 1)
                    Task.Delay(1).Wait();
                if (retryCounter > 10000)
                    throw new Exception("Unable to generate new file path!");
            } while (File.Exists(path));
            return path;
        }
    }
}
