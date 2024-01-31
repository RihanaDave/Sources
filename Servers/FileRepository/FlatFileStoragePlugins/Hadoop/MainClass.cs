using GPAS.FileRepository.Logic;
using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.FlatFileStorage;
using GPAS.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Net;
using System.Text;

namespace GPAS.FileRepository.FlatFileStoragePlugins.Hadoop
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static string ServerAddress;
        private static string Port;
        private static int UploadFileTimeoutMiliseconds;
        private static bool IsInitialized = false;

        private ExceptionDetailGenerator exceptionDetailGenerator = new ExceptionDetailGenerator();

        public MainClass()
        {
            if(!IsInitialized)
            {
                ServerAddress = ConfigurationManager.AppSettings["GPAS.FileRepository.FlatFileStoragePlugins.Hadoop.ServerAddress"];
                Port = ConfigurationManager.AppSettings["GPAS.FileRepository.FlatFileStoragePlugins.Hadoop.Port"];
                UploadFileTimeoutMiliseconds = int.Parse(ConfigurationManager.AppSettings["GPAS.FileRepository.FlatFileStoragePlugins.Hadoop.UploadFileTimeoutMiliseconds"]);
                IsInitialized = true;
            }
        }

        private string GetBaseURL()
        {
            //return "http://" + ServerAddress + ":9870/webhdfs/v1";
            return string.Format("http://{0}:{1}/webhdfs/v1",ServerAddress,Port);
        }
        private bool CheckFileExist(string fileName, string path)
        {
            List<DirectoryContent> listDirectory = GetDirectoryContent(path);
            foreach (var item in listDirectory)
            {
                if (item.ContentType == DirectoryContentType.File)
                {
                    if (item.DisplayName == fileName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool CheckBadPathName(string path)
        {
            if (path.Contains("//") || path.Contains("\\"))
                return true;
            return false;
        }

        public void CreateDirectory(string directoryIdentifier)
        {
            string path = $"/{directoryIdentifier}/";
            if (CheckBadPathName(path))
                throw new ArgumentException("Invalid path defined");
            List<DirectoryContent> resultList = GetDirectoryContent("/");
            foreach (DirectoryContent item in resultList)
            {
                var directoryInHadoop = item.UriAddress + "/";
                if (directoryInHadoop == path)
                    return;
            }
            string result = "";

            string temp = GetBaseURL() + path + "?op=MKDIRS";
            try
            {
                using (var webClient = new WebClient())
                {
                    result = webClient.UploadString(temp, "Put", "");
                }
            }
            catch (WebException ex)
            {
                throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
            }
            JObject json = JObject.Parse(result);
            //return (bool)json["boolean"];
        }

        public byte[] LoadFile(string fileIdentifier, string directoryIdentifier)
        {
            string filePath = $"/{directoryIdentifier}/{fileIdentifier}";
            string downloadUrl = GetBaseURL() + filePath + "?op=OPEN";
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadData(downloadUrl);
                }
            }
            catch (WebException ex)
            {
                // زمانیکه مسیر فایل نامعتبر باشد یا فایلی برای دانلود وجود نداشته
                // باشد Null برمیگرداند
                return null;
            }
        }

        public void SaveFile(byte[] fileContent, string fileIdentifier, string directoryIdentifier)
        {
            string fullName = $"/{directoryIdentifier}/{fileIdentifier}";
            long blockSize = 134217728 /*128 MB*/;
            double requiredSizeRoundedToMBs = Math.Ceiling(fileContent.Length / 1048576.0 /*1 MB*/);
            if (requiredSizeRoundedToMBs < 128)
                blockSize = (long)requiredSizeRoundedToMBs * 1048576 /*1 MB*/;
            if (!CheckFileExist(fileIdentifier, $"/{directoryIdentifier}"))
            {
                string temp = GetBaseURL() + fullName + "?op=CREATE&blocksize=" + blockSize.ToString();
                TimeoutBasedWebClient client = null;
                try
                {
                    client = new TimeoutBasedWebClient
                    {
                        TimeoutMiliSeconds = UploadFileTimeoutMiliseconds
                    };
                    var result = client.UploadData(temp, "Put", fileContent);
                }
                finally
                {
                    if (client != null)
                        client.Dispose();
                }
            }
            else
            {
                throw new InvalidOperationException("A file with given name is already exist in specified path");
            }
        }

        private string DecodeFromUtf8(string utf8)
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            byte[] temp = Encoding.Default.GetBytes(utf8);
            return Encoding.UTF8.GetString(temp);
        }
        private List<DirectoryContent> GetDirectoryContent(string path)
        {
            string httpURI = GetBaseURL() + path + "/?op=LISTSTATUS";
            string result = "";
            try
            {
                using (var client = new WebClient())
                {
                    result = client.DownloadString(httpURI);
                }
            }
            catch (WebException ex)
            {
                throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
            }

            JObject json = JObject.Parse(result);
            List<DirectoryContent> resultList = new List<DirectoryContent>();
            foreach (var child in json["FileStatuses"].Children().Children())
            {
                var temp = child.First;
                for (; temp != null; temp = temp.Next)
                {
                    DirectoryContent tempHadoopResult = new DirectoryContent
                    {
                        ContentType = GetHadoopContentTypeFromJToken(temp["type"]),
                        DisplayName = DecodeFromUtf8(temp["pathSuffix"].ToString())
                    };
                    if (path.Equals("/"))
                        tempHadoopResult.UriAddress = path + DecodeFromUtf8(temp["pathSuffix"].ToString());
                    else
                        tempHadoopResult.UriAddress = path + "/" + DecodeFromUtf8(temp["pathSuffix"].ToString());
                    resultList.Add(tempHadoopResult);
                }
            }
            return resultList;
        }
        private DirectoryContentType GetHadoopContentTypeFromJToken(JToken typeJToken)
        {
            string typeString = typeJToken.ToString();
            if (typeString.Equals("DIRECTORY"))
                return DirectoryContentType.Directory;
            else if (typeString.Equals("FILE"))
                return DirectoryContentType.File;
            else
                throw new NotSupportedException();
        }

        public void RemoveDirectories(string[] directoryIDs)
        {
            //string result = "";
            using (var client = new WebClient())
            {
                try
                {
                    foreach (string dir in directoryIDs)
                    {
                        string temp = $"{GetBaseURL()}/{dir}?op=DELETE&recursive=true";
                        /*result = */
                        client.UploadString(temp, "Delete", "");
                        // TODO: نتایج فراخوانی درخواست نیاز به بررسی ندارد؟
                    }
                }
                catch (WebException ex)
                {
                    throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
                }
            }
        }

        /// <summary>
        /// این تابع در صورت روشن بودن سرور هدوپ True برمی گرداند.
        /// </summary>
        /// <returns>  این تابع در صورت روشن بودن سرور هدوپ True برمی گرداند در غیر این صورت false برمی گرداند. </returns>
        public bool IsAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadString(GetBaseURL() + "/?op=LISTSTATUS");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SaveFileInTwoPathes(byte[] fileContent
            , string firstFileID, string firstDirectoryID
            , string secondFileID, string secondDirectoryID)
        {
            SaveFile(fileContent, firstFileID, firstDirectoryID);
            SaveFile(fileContent, secondFileID, secondDirectoryID);
        }

        public long GetFileSizeInBytes(string fileIdentifier, string directoryIdentifier)
        {
            string fullURI = GetBaseURL() + fileIdentifier + "?op=GETCONTENTSUMMARY";
            string result;
            try
            {
                using (var client = new WebClient())
                {
                    result = client.DownloadString(fullURI);
                }
            }
            catch (WebException ex)
            {
                throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
            }

            JObject json = JObject.Parse(result);
            var ContentSummary = JObject.Parse(json["ContentSummary"].ToString());

            foreach (var child in ContentSummary)
            {
                if (child.Key == "length")
                {
                    return long.Parse(child.Value.ToString());
                }
            }
            return 0;
        }
    }
}
