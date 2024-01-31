using GPAS.Workspace.Entities.Datalake;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Datalake
{
    public class ImportProvider
    {
        public async static Task ImportFileToDatalake(IngestionFile fileToImport)
        {            
            //string follderNameFromEnteredDate = GetFollderNameFromEnteredDate(fileToImport);
            var service = new FtpServiceProvider
                (string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"])
                , ConfigurationManager.AppSettings["JobShareService_UserName"]
                , ConfigurationManager.AppSettings["JobShareService_Password"]);

            FileStream fileContentStream = new FileStream(fileToImport.FilePath, FileMode.Open, FileAccess.ReadWrite);

            Guid id = Guid.NewGuid();
            fileToImport.id = id;

            List<string> contentAndHeaderPathes = GenerateContentAndHeaderFTPPath(service, fileToImport);

            string fileName = GetFileExtention(fileToImport.FileSeparator, fileToImport.FilePath.Split('\\').Last());

            await service.UploadAsync(fileContentStream, (int)fileContentStream.Length, contentAndHeaderPathes.ElementAt(0), fileName);

            File.WriteAllLines(GetSelectedFolderFromFilePath(fileToImport.FilePath), new string[] { fileToImport.Headers });

            FileStream headerfileStream = new FileStream(GetSelectedFolderFromFilePath(fileToImport.FilePath), FileMode.Open, FileAccess.ReadWrite);                       
            
            await service.UploadAsync(headerfileStream, (int)headerfileStream.Length, contentAndHeaderPathes.ElementAt(1), "header.csv");
            
            await JobProvider.InsertFileIngestionJobStatus(fileToImport);

        }

        private static string GetSelectedFolderFromFilePath(string filePath)
        {
            string selectedDirectory = string.Empty;
            List<string> filePathArr = filePath.Split(new string[] { "\\" }, StringSplitOptions.None).ToList();
            for (int i = 0; i < filePathArr.Count - 1; i++)
            {
                selectedDirectory += filePathArr.ElementAt(i) + "\\";
            }
            return selectedDirectory + "headers.csv";

        }        

        private static string GetFileExtention(FileSeparator fileSeparator, string currentExtention)
        {
            string fileExtention = string.Empty;
            switch (fileSeparator)
            {
                case FileSeparator.Tab:
                    fileExtention = "tsv";
                    break;
                case FileSeparator.Comma:
                    fileExtention = "csv";
                    break;
                case FileSeparator.Pipe:
                    fileExtention = "psv";
                    break;
                case FileSeparator.Sharp:
                    fileExtention = "sharpsv";
                    break;
                case FileSeparator.Slash:
                    fileExtention = "slashsv";
                    break;
                default:
                    break;
            }
            string result = string.Empty;
            int index = currentExtention.LastIndexOf(".");
            if (index > 0)
                result = string.Format("{0}.{1}", currentExtention.Substring(0, index), fileExtention);

            return result;
        }

        private static List<string> GenerateContentAndHeaderFTPPath(FtpServiceProvider service, IngestionFile fileToImport)
        {
            List<string> result = new List<string>();            
            string ftpServerRootPath = string.Format("ftp://" + "{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"]);                                     
            string follderNameFromEnteredDate = GetFollderNameFromEnteredDate(fileToImport);
            string follderNameFromEnteredFileSeparator = GetFollderNameFromEnteredFileSeparator(fileToImport);

            string datalakePath = string.Format("{0}/{1}",
                ftpServerRootPath,
                "Datalake");
            if (!IsExistDirectory(service, ftpServerRootPath, datalakePath, "Datalake"))
            {
                service.CreateDirectory(datalakePath);
            }
            string fileIngestionPath = string.Format("{0}/{1}/{2}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion");
            if (!IsExistDirectory(service, datalakePath, fileIngestionPath, "FileIngestion"))
            {
                service.CreateDirectory(fileIngestionPath);
            }
            string separatorPath = string.Empty;
            separatorPath = string.Format("{0}/{1}/{2}/{3}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion",
                follderNameFromEnteredFileSeparator);
            if (!IsExistDirectory(service, fileIngestionPath, separatorPath, follderNameFromEnteredFileSeparator))
            {
                service.CreateDirectory(separatorPath);
            }
            string categoryPath = string.Empty;
            categoryPath = string.Format("{0}/{1}/{2}/{3}/{4}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion",
                follderNameFromEnteredFileSeparator,
                fileToImport.Category);
            if (!IsExistDirectory(service, separatorPath, categoryPath, fileToImport.Category))
            {
                service.CreateDirectory(categoryPath);
            }
            string datePath = string.Format("{0}/{1}/{2}/{3}/{4}/{5}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion",
                follderNameFromEnteredFileSeparator,
                fileToImport.Category,
                follderNameFromEnteredDate);
            if (!IsExistDirectory(service, categoryPath, datePath, follderNameFromEnteredDate))
            {
                service.CreateDirectory(datePath);
            }
            
            string idPath = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion",
                follderNameFromEnteredFileSeparator,
                fileToImport.Category,
                follderNameFromEnteredDate,
                fileToImport.id.ToString());
            if (!IsExistDirectory(service, datePath, idPath, fileToImport.id.ToString()))
            {
                service.CreateDirectory(idPath);
            }

            string headerPath = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}",
                ftpServerRootPath,
                "Datalake",
                "FileIngestion",
                follderNameFromEnteredFileSeparator,
                fileToImport.Category,
                follderNameFromEnteredDate,
                fileToImport.id.ToString(),
                "Header");
            if (!IsExistDirectory(service, idPath, headerPath, "Header"))
            {
                service.CreateDirectory(headerPath);
            }

            result.Add(idPath);
            result.Add(headerPath);
            return result;
        }

        private static string GetFollderNameFromEnteredFileSeparator(IngestionFile fileToImport)
        {
            string result = string.Empty;
            switch (fileToImport.FileSeparator)
            {
                case FileSeparator.Tab:
                    result = "TabSeparated";
                    break;
                case FileSeparator.Comma:
                    result = "CommaSeparated";
                    break;
                case FileSeparator.Pipe:
                    result = "PipeSeparated";
                    break;
                case FileSeparator.Sharp:
                    result = "SharpSeparated";
                    break;
                case FileSeparator.Slash:
                    result = "SlashSeparated";
                    break;
                default:
                    break;
            }
            return result;
        }

        private static string GetFollderNameFromEnteredDate(IngestionFile fileToImport)
        {
            string result = string.Empty;
            return string.Format("{0}-{1}-{2}", fileToImport.DataFlowDateTime.Year, fileToImport.DataFlowDateTime.Month,
                fileToImport.DataFlowDateTime.Day);
        }

        private static bool IsExistDirectory(FtpServiceProvider service, string parentPath, string pathToCheck, string directoryName)
        {
            bool result = false;
            var availableDirectories = service.GetDirectoryInformation(parentPath);
            foreach (var currentDirectory in availableDirectories)
            {
                if (currentDirectory.IsDirectory && (@currentDirectory.Name == directoryName))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static StreamReader GetDataSourceStream(string sourceFilePathOnJobShare, string importDataSourceTempPath)
        {
            //    var service = new FileServer.FtpService.FtpService
            //        (string.Format("{0}:{1}", Properties.Settings.Default.JobShareService_ServerAddress, Properties.Settings.Default.JobShareService_PortNumber)
            //        , Properties.Settings.Default.JobShareService_UserName
            //        , Properties.Settings.Default.JobShareService_Password);
            var service = new FtpServiceProvider
                (string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"])
                , ConfigurationManager.AppSettings["JobShareService_UserName"]
                , ConfigurationManager.AppSettings["JobShareService_Password"]);
            if (!Directory.Exists(importDataSourceTempPath))
                Directory.CreateDirectory(importDataSourceTempPath);
            var tempTargetPath = importDataSourceTempPath + Guid.NewGuid();
            Task.WaitAll(service.DownloadAsync(sourceFilePathOnJobShare, tempTargetPath));
            return new StreamReader(tempTargetPath);
        }

        public static string DownloadDataSource(string sourceFilePathOnJobShare, string importDataSourceTempPath)
        {
            //    var service = new FileServer.FtpService.FtpService
            //        (string.Format("{0}:{1}", Properties.Settings.Default.JobShareService_ServerAddress, Properties.Settings.Default.JobShareService_PortNumber)
            //        , Properties.Settings.Default.JobShareService_UserName
            //        , Properties.Settings.Default.JobShareService_Password);
            var service = new FtpServiceProvider
                (string.Format("{0}:{1}", ConfigurationManager.AppSettings["JobShareService_ServerAddress"], ConfigurationManager.AppSettings["JobShareService_PortNumber"])
                , ConfigurationManager.AppSettings["JobShareService_UserName"]
                , ConfigurationManager.AppSettings["JobShareService_Password"]);
            if (!Directory.Exists(importDataSourceTempPath))
                Directory.CreateDirectory(importDataSourceTempPath);
            var tempTargetPath = importDataSourceTempPath + Guid.NewGuid();
            Task.WaitAll(service.DownloadAsync(sourceFilePathOnJobShare, tempTargetPath));
            return tempTargetPath;
        }
    }
}
