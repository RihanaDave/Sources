using GPAS.FileRepository.Logic;
using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.HierarchicalFileStorage;
using GPAS.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Net;
using System.Text;

namespace GPAS.FileRepository.HierarchicalFileStoragePlugins.Hadoop
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static string ServerAddress;
        private static int UploadFileTimeoutMiliseconds;
        private static bool IsInitialized = false;
        private ExceptionDetailGenerator exceptionDetailGenerator = new ExceptionDetailGenerator();

        public MainClass()
        {
            if (!IsInitialized)
            {
                ServerAddress = ConfigurationManager.AppSettings["GPAS.FileRepository.HierarchicalFileStoragePlugins.Hadoop.ServerAddress"];
                UploadFileTimeoutMiliseconds = int.Parse(ConfigurationManager.AppSettings["GPAS.FileRepository.HierarchicalFileStoragePlugins.Hadoop.UploadFileTimeoutMiliseconds"]);
                IsInitialized = true;
            }
        }

        private string GetBaseURL()
        {
            return "http://" + ServerAddress + ":50070/webhdfs/v1";
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

        /// <summary>
        /// در مسیر مورد نظر یک پوشه ایجاد می کند.
        /// </summary>
        /// <param name="path"> رشته آدرس را از کاربر دریافت میکند</param>
        /// <returns> در صورتی که قبلا در مسیر مورد نظر پوشه وجود داشت مقدار False و در غیر این صورت پوشه را ایجاد و مقدار True را ارسال می کند.</returns>
        public bool CreateDirectory(string path)
        {
            if (CheckBadPathName(path))
                return false;
            List<DirectoryContent> resultList = GetDirectoryContent("/");
            foreach (DirectoryContent item in resultList)
            {
                var directoryInHadoop = item.UriAddress + "/";
                if (directoryInHadoop == path)
                    return true;
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
            return (bool)json["boolean"];
        }

        public bool DeleteDirectory(string path)
        {
            string result = "";
            string temp = GetBaseURL() + path + "?op=DELETE&recursive=true";
            try
            {
                using (var client = new WebClient())
                {
                    result = client.UploadString(temp, "Delete", "");
                }
            }
            catch (WebException ex)
            {
                throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
            }
            JObject json = JObject.Parse(result);
            return (bool)json["boolean"];
        }

        public byte[] LoadFile(string filePath)
        {
            string temp = GetBaseURL() + filePath + "?op=OPEN";
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadData(temp);
                }
            }
            catch (WebException)
            {
                // زمانیکه مسیر فایل نامعتبر باشد یا فایلی برای دانلود وجود نداشته
                // باشد Null برمیگرداند
                return null;
            }
        }

        public long GetFileSizeInBytes(string uri)
        {
            string fullURI = GetBaseURL() + uri + "?op=GETCONTENTSUMMARY";
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

        public void RemoveAllFiles()
        {
            //string result = "";
            using (var client = new WebClient())
            {
                try
                {
                    string temp = GetBaseURL() + "/Medias" + "?op=DELETE&recursive=true";
                    /*result = */
                    client.UploadString(temp, "Delete", "");
                    temp = GetBaseURL() + "/Documents" + "?op=DELETE&recursive=true";
                    /*result = */
                    client.UploadString(temp, "Delete", "");
                    temp = GetBaseURL() + "/DataSource" + "?op=DELETE&recursive=true";
                    /*result = */
                    client.UploadString(temp, "Delete", "");
                    // TODO: نتایج فراخوانی درخواست نیاز به بررسی ندارد؟
                }
                catch (WebException ex)
                {
                    throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
                }
            }
        }

        /// <summary>
        /// این تابع پوشه مورد نظر را به مکان با اسم مورد انتقال می دهد.
        /// </summary>
        /// <param name="sourcePath">این پارامتر پوشه مورد نظر برای تغییر را دریافت می کند.</param>
        /// <param name="targetPath">این پارامتر پوشه مقصد را برای انتقال و تغییر نام دریافت می کند.</param>
        /// <returns></returns>
        public bool RenameDirectory(string sourcePath, string targetPath)
        {
            string result = "";
            string temp = GetBaseURL() + sourcePath + "?op=RENAME&destination=" + targetPath;
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
            return (bool)json["boolean"];
        }

        public void SaveFile(byte[] fileToUpload, string fileName, string targetPath)
        {
            string fullName = targetPath + "/" + fileName;
            long blockSize = 134217728 /*128 MB*/;
            double requiredSizeRoundedToMBs = Math.Ceiling(fileToUpload.Length / 1048576.0 /*1 MB*/);
            if (requiredSizeRoundedToMBs < 128)
                blockSize = (long)requiredSizeRoundedToMBs * 1048576 /*1 MB*/;
            if (!CheckFileExist(fileName, targetPath))
            {
                string temp = GetBaseURL() + fullName + "?op=CREATE&blocksize=" + blockSize.ToString();
                TimeoutBasedWebClient client = null;
                try
                {
                    client = new TimeoutBasedWebClient
                    {
                        TimeoutMiliSeconds = UploadFileTimeoutMiliseconds
                    };
                    var result = client.UploadData(temp, "Put", fileToUpload);
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

        public List<DirectoryContent> GetDirectoryContent(string path)
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
    }
}
