using System;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace GPAS.JobServer.JobMonitoringAgent
{
    internal class MonitoringLogger : Logger.ProcessLogger
    {
        const string untitledLogTitle = "untitled";
        string currentLogTitle = untitledLogTitle;
        string LogsPath = "";

        public void Initialization()
        {
            if (IsInitialized())
                return;
            string now = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            currentLogTitle = (now.Substring(6, 4) + '-' + now.Substring(0, 5) + ' ' + now.Substring(11))
                .Replace('/', '-')
                .Replace(':', '-');

            LogsPath = ConfigurationManager.AppSettings["LogsPath"];
            if (LogsPath == null)
            {
                throw new ConfigurationErrorsException("Unable to load app config: 'LogsPath'");
            }

            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);
            string logPath = string.Format("{0}{1}.log", LogsPath, currentLogTitle);
            if (File.Exists(logPath))
                File.Delete(logPath);
            base.Initialization(logPath);
        }
    }
}
