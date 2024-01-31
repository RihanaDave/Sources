using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Logger
{
    public class ExceptionHandler
    {
        public static bool ExceptionHandlerLogRegistration { get; private set; }
        public static string ExceptionHandlerLogPath { get; private set; }
        public static int ExceptionHandlerStoreNLastExceptionLogs { get; private set; }
        private ExceptionDetailGenerator detailGenerator;

        public static void Init()
        {
            ExceptionHandlerLogRegistration = bool.Parse(ConfigurationManager.AppSettings["ExceptionHandlerLogRegistration"]);
            if (!ExceptionHandlerLogRegistration)
                return;
            ExceptionHandlerLogPath = ConfigurationManager.AppSettings["ExceptionHandlerLogPath"];
            ExceptionHandlerStoreNLastExceptionLogs = int.Parse(ConfigurationManager.AppSettings["ExceptionHandlerStoreNLastExceptionLogs"]);

            if (!Directory.Exists(ExceptionHandlerLogPath))
                Directory.CreateDirectory(ExceptionHandlerLogPath);
            string testFilePath = ExceptionHandlerLogPath + "test.test";
            File.WriteAllText(testFilePath, "This is a test to check exception logger path access");
            File.Delete(testFilePath);
        }

        public ExceptionHandler()
        {
            detailGenerator = new ExceptionDetailGenerator();
        }

        private void ManageCountOfLogsFiles()
        {
            if (ExceptionHandlerStoreNLastExceptionLogs == 0)
                return;
            int count = Directory.GetFiles(ExceptionHandlerLogPath, "*", SearchOption.AllDirectories).Length;
            if (count >= ExceptionHandlerStoreNLastExceptionLogs)
            {
                DirectoryInfo info = new DirectoryInfo(ExceptionHandlerLogPath);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                files.First().Delete();
            }
        }

        private string GetErrorLogPath()
        {
            if (ExceptionHandlerLogPath == null)
                throw new ConfigurationErrorsException("ExceptionHandlerLogPath");
            return ExceptionHandlerLogPath +
                      DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff") + ".txt";
        }

        public void WriteErrorLog(string errorMessage, MemoryStream logStream = null, string logStreamTitle = "")
        {
            try
            {
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                messageBuilder.Append(";");
                messageBuilder.Append(errorMessage);
                if (logStream != null)
                {
                    messageBuilder.Append(Environment.NewLine);
                    if (logStreamTitle != null)
                    {
                        messageBuilder.AppendFormat("{0}:{1}", logStreamTitle, Environment.NewLine);
                    }
                    Utility.StreamUtility utility = new Utility.StreamUtility();
                    messageBuilder.Append(utility.GetStringFromStream(logStream));
                }

                ManageCountOfLogsFiles();
                string errorLogPath = GetErrorLogPath();
                StreamWriter streamWriter = new StreamWriter(errorLogPath);
                streamWriter.WriteLine(messageBuilder.ToString());
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch (Exception)
            {
            }
        }

        public void WriteErrorLog(Exception ex, MemoryStream logStream = null, string logStreamTitle = "")
        {
            WriteErrorLog(detailGenerator.GetDetails(ex), logStream, logStreamTitle);
        }
    }
}
