using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.JobServer.JobWorkerProcess
{
    internal class ProcessLogger : Logger.ProcessLogger
    {
        public void Initialization(int jobID, string LogNameSuffix = "")
        {
            if (IsInitialized())
                return;
            string logsDirectoryPath = ConfigurationManager.AppSettings["LogsPath"];
            if (!Directory.Exists(logsDirectoryPath))
                Directory.CreateDirectory(logsDirectoryPath);
            string logPath = string.Format("{0}{1}-{2}.log", logsDirectoryPath, jobID.ToString(), DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            if (File.Exists(logPath))
                File.Delete(logPath);
            base.Initialization(logPath);
        }

        internal void WriteLog(LogTypes logType, string message = "")
        {
            string logMessage =
                string.Format("{0}\t{1}", logType.ToString(), message);
            base.WriteLog(logMessage);
        }

        internal enum LogTypes
        {
            JobInitialized,         // "Busy" Status in Job-DB

            JobWorkingStarted,      // "Busy" Status in Job-DB
            JobWorkingFailed,       // "Failed" Status in Job-DB
            JobWorkingSuccessed,    // "Success" Status in Job-DB
            JobWorkingWarning,
            JobWorkingInfo,

            JobBatchUnitSuccess,
            JobBatchUnitSuccessButSomeFailed,
            JobUnitFailed
        }
    }
}
