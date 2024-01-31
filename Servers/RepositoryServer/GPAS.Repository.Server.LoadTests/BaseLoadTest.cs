using GPAS.RepositoryServer;
using System;
using System.Diagnostics;
using System.IO;

namespace GPAS.Repository.Server.LoadTests
{
    public class BaseLoadTest
    {
        public static int MaxObjectId = 1;
        public static long MaxPropertyId = 1;
        public static int MaxRelationId = 1;
        public static int PropertiesCount = 20;
        public static string ClearAllDataTime = "";
        public static string PublishDataTime = "";
        public string TestTime = "";
        public string OutPutTimeFormat = @"hh\:mm\:ss\.fff";
        public static long BatchCount { get; set; } = 100;
        public static int RetrieveItemsCount { get; set; } = 100;
        public static string ExtensionFileToSave;

        public readonly Random Random = new Random();
        public readonly Service RepositoryService = new Service();

        public void WriteLog(string message)
        {
            try
            {
                Debug.WriteLine(message);

                string resultFolderPath = Properties.Resource.String_LogFolderPath;
                string resultFilePath = Path.Combine(resultFolderPath, "Load test results_" + ExtensionFileToSave);

                if (!Directory.Exists(resultFolderPath))
                {
                    Directory.CreateDirectory(resultFolderPath);
                }

                if (!File.Exists(resultFilePath))
                {
                    using (StreamWriter streamWriter = File.CreateText(resultFilePath))
                    {
                        streamWriter.WriteLine("");
                        streamWriter.WriteLine(message);
                    }

                    return;
                }

                using (StreamWriter streamWriter = File.AppendText(resultFilePath))
                {
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine(message);
                }

            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                // Do nothing
            }
        }
    }
}
