using GPAS.Logger;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Logic
{
    public class RemoveAllData
    {
        private string logPath = ConfigurationManager.AppSettings["AdministrativeEventLogsPath"];

        private async Task TruncateRepositoryData()
        {
            //GPAS.Dispatch.ServiceAccess.RepositoryService.ServiceClient proxy = null;
            //try
            //{
            //    proxy = new GPAS.Dispatch.ServiceAccess.RepositoryService.ServiceClient();
            //    await proxy.TruncateDatabaseAsync();

            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}

        }
        private async Task RemoveFileRepositoryData()
        {
            GPAS.Dispatch.ServiceAccess.FileRepositoryService.ServiceClient proxy = null;
            try
            {
                proxy = new GPAS.Dispatch.ServiceAccess.FileRepositoryService.ServiceClient();
                await proxy.RemoveAllFilesAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        private async Task RemoveSearchIndexes()
        {
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient proxy = null;
            try
            {
                proxy = new GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient();
                await proxy.RemoveSearchIndexesAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        private async Task RemoveHorizonIndexes()
        {
            GPAS.Dispatch.ServiceAccess.HorizonService.ServiceClient proxy = null;
            try
            {
                proxy = new GPAS.Dispatch.ServiceAccess.HorizonService.ServiceClient();
                await proxy.RemoveHorizonIndexesAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        private async Task DeleteHorizonServerUnsyncConcepts()
        {
            SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
            await Task.Run(() => searchIndexesSynchronization.DeleteHorizonServerUnsyncConcepts());
        }

        private async Task DeleteSearchServerUnsyncConcepts()
        {
            SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
            await Task.Run(() => searchIndexesSynchronization.DeleteSearchServerUnsyncConcepts());
        }

        private void AddBackslashCharAtTheEndOfLogPathIfNeeded(ref string logPath)
        {
            string lastChar = logPath.Substring(logPath.Length - 1);
            if (lastChar.Equals("\\"))
            {
                return;
            }
            else
            {
                logPath = logPath + "\\";
            }
        }
        //Function Remove Investigation Sql server
        private  async Task TruncateInvestigationTable(string tableName)
        {
            InvestigationManagement investigationManagement = new InvestigationManagement();
          await Task.Run(()=>investigationManagement.TruncateInvestigationTable(tableName));
        }
        public async Task<bool> RemoveAll()
        {
            AddBackslashCharAtTheEndOfLogPathIfNeeded(ref logPath);
            ProcessLogger logger = new ProcessLogger();
            logger.Initialization(logPath + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".txt");

            try
            {
                logger.WriteLog($"\t Start Deleting Indexes From Search Server.");
                await RemoveSearchIndexes();
                logger.WriteLog($"\t Complete Deleting Indexes From Search Server.");

            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Delete Search Server Indexes. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }
            try
            {
                logger.WriteLog($"\t Start Removing Data From Repository Server.");
                await TruncateRepositoryData();
                logger.WriteLog($"\t Complete Removing Data From Repository Server.");

            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Truncate Repository Server Tables. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }
            try
            {
                logger.WriteLog($"\t Start Removing Files From FileRepository Server.");
                await RemoveFileRepositoryData();
                logger.WriteLog($"\t Complete Removing Files From FileRepository Server.");

            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Remove File Repository Documents. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }
            try
            {
                logger.WriteLog($"\t Start Deleting Unsynchronized Concepts From Horizon Server.");
                await DeleteHorizonServerUnsyncConcepts();
                logger.WriteLog($"\t Complete Deleting Unsynchronized Concepts From Horizon Server.");
            }

            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Delete Horizon Server Unsynchronized Concepts. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }

            try
            {
                logger.WriteLog($"\t Start Deleting Unsynchronized Concepts From Search Server.");
                await DeleteSearchServerUnsyncConcepts();
                logger.WriteLog($"\t Complete Deleting Unsynchronized Concepts From Search Server.");
            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Delete Search Server Unsynchronized Concepts. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }
            try
            {
                logger.WriteLog($"\t Start Deleting Investigation in Sql Server.");
                await TruncateInvestigationTable("Investigations");
                logger.WriteLog($"\t Complete Deleting Investigation in Sql Server.");
            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Delete Investigation in Sql Server Concepts. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }

            try
            {
                logger.WriteLog($"\t Start Deleting Indexes From Horizon Server.");
                await RemoveHorizonIndexes();
                logger.WriteLog($"\t Complete Deleting Indexes From Horizon Server.");

            }
            catch (Exception ex)
            {
                logger.WriteLog($"Unble To Delete Horizon Server Indexes. Exception : {(new ExceptionDetailGenerator()).GetDetails(ex)}");
                return false;
            }

            logger.WriteLog($"\tComplete Remove Data.");
            return true;

        }
    }
}
