using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.FlatFileStorage;
using GPAS.Logger;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Net;
using System.Text;

namespace GPAS.FileRepository.FlatFileStoragePlugins.HBase
{
    [Export(typeof(IAccessClient))]
    public class MainClass : IAccessClient
    {
        private static string ServerAddress;
        private static bool IsInitialized = false;

        private ExceptionDetailGenerator exceptionDetailGenerator = new ExceptionDetailGenerator();

        public MainClass()
        {
            if (!IsInitialized)
            {
                ServerAddress = ConfigurationManager.AppSettings["GPAS.FileRepository.FlatFileStoragePlugins.HBase.ServerAddress"];
                string address = ServerAddress + "init";
                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadString(address);
                    }
                }
                catch (WebException ex)
                {
                    throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
                }
                IsInitialized = true;
            }
        }

        private string GetBaseURL()
        {
            return "http://" + ServerAddress + ":50070/webhdfs/v1";
        }

        private bool CheckFileExist(string fileIdentifier, string directoryIdentifier)
        {
            return false;
#pragma warning disable CS0162 // Unreachable code detected
            try
#pragma warning restore CS0162 // Unreachable code detected
            {
                var client = new RestClient(ServerAddress);
                var request = new RestRequest("checkfileexist", Method.POST) { RequestFormat = DataFormat.Json };
#pragma warning disable CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
#pragma warning disable CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                request.AddBody(request.AddBody(
                    new
                        { id = fileIdentifier, tableName = directoryIdentifier }
                ));
#pragma warning restore CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
#pragma warning restore CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                var response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    throw response.ErrorException;
                }

                if (response.IsSuccessful)
                {
                    return bool.Parse(response.Content);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

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
        public void CreateDirectory(string directoryIdentifier)
        {
        }

        public byte[] LoadFile(string fileIdentifier, string directoryIdentifier)
        {
            try
            {
                var client = new RestClient(ServerAddress);
                var request = new RestRequest("getfile", Method.POST) { RequestFormat = DataFormat.Json };
#pragma warning disable CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
#pragma warning disable CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                request.AddBody(request.AddBody(
                    new
                    { id = fileIdentifier, tableName = directoryIdentifier }
                ));
#pragma warning restore CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
#pragma warning restore CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                var response = client.DownloadData(request);
                if (response != null)
                {
                    return response;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }

        public void RemoveDirectories(string[] directoryIDs)
        {
            string address = ServerAddress + "removeallfiles";
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadString(address);
                }
            }
            catch (WebException ex)
            {
                throw exceptionDetailGenerator.AppendWebExceptionResonse(ex);
            }
        }
        

        private string ByteToString(byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray);
        }

        private byte[] StringToByte(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public void SaveFileBatchMode(FileInfos fileInfos)
        {
            /*List<FileInfo> hbaseModels = new List<FileInfo>();
            hbaseModels.Add
            (
                new FileInfo
                {
                    id = 0.ToString(),
                    tableName = "Medias",
                    content = "dGhpcyBpcyBhIHRlc3Q="
                }
            );
            hbaseModels.Add
            (
                new FileInfo
                {
                    id = 1.ToString(),
                    tableName = "Medias",
                    content = "dGhpcyBpcyBhIHRlc3Q="
                }
            );


            FileInfos fileInfos = new FileInfos()
            {
                fileInfos = hbaseModels
            };*/

            var client = new RestClient(ServerAddress);
            var request = new RestRequest("uploadfiles", Method.POST) { RequestFormat = DataFormat.Json };
            
            request.AddJsonBody(
                fileInfos
            );
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

        }

        public void SaveFile(byte[] fileContent, string fileIdentifier, string directoryIdentifier)
        {
            if (!CheckFileExist(fileIdentifier, directoryIdentifier))
            {
                var client = new RestClient(ServerAddress);
                var request = new RestRequest("uploadfile", Method.POST) { RequestFormat = DataFormat.Json };
                
#pragma warning disable CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                request.AddBody(
                    new
                    {
                        id = fileIdentifier,
                        tableName = directoryIdentifier,
                        content = ByteToString(fileContent)
                    }
                    );
#pragma warning restore CS0618 // 'RestRequest.AddBody(object)' is obsolete: 'Use AddXmlBody or AddJsonBody'
                var response = client.Execute(request);
                if (!response.IsSuccessful)
                {
                    throw response.ErrorException;
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

    public class FileInfo
    {
        public string id { get; set; }
        public string tableName { get; set; }
        public string content { get; set; }
    }

    public class FileInfos
    {
        public List<FileInfo> fileInfos { get; set; }
    }
}
