using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Logger
{
    public class AdministrativeEventReporter
    {
        static string LogPath;
        static int AdministrativeEventStoreNLastLogs;
        public static void Init()
        {
            LogPath = ConfigurationManager.AppSettings["AdministrativeEventLogsPath"];
            AddBackslashCharAtTheEndOfLogPathIfNeeded(ref LogPath);
            AdministrativeEventStoreNLastLogs = int.Parse(ConfigurationManager.AppSettings["AdministrativeEventStoreNLastLogs"]);
            if (string.IsNullOrEmpty(LogPath))
                throw new ConfigurationErrorsException("'AdministrativeEventLogsPath' app setting not defined");
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
            string testFilePath = LogPath + "test.test";
            File.WriteAllText(testFilePath, "This is a test to check admin event reporter path access");
            File.Delete(testFilePath);

        }

        private static void AddBackslashCharAtTheEndOfLogPathIfNeeded(ref string logPath)
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

        public void Report(string message)
        {
            try
            {
                ProcessLogger logger = new ProcessLogger();
                logger.Initialization(GetNewReportLogPath());
                ManageCountOfLogsFiles();
                logger.WriteLog(message);
                logger.Finalization();
            }
            catch (Exception ex)
            {
                try
                {
                    ExceptionHandler eh = new ExceptionHandler();
                    eh.WriteErrorLog(ex);
                }
                catch
                { }
            }
        }

        private string GetNewReportLogPath()
        {
            return LogPath + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".txt";
        }
        private void ManageCountOfLogsFiles()
        {
            if (AdministrativeEventStoreNLastLogs == 0)
                return;
            int count = Directory.GetFiles(LogPath, "*", SearchOption.AllDirectories).Length;
            if (count >= AdministrativeEventStoreNLastLogs)
            {
                DirectoryInfo info = new DirectoryInfo(LogPath);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                files.First().Delete();
            }
        }
    }
}
