using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.ETLProvider
{
    public class ETLProvider
    {
        string ServerAddress { get; set; }
        int FileRepositoryPortNumber { get; set; }
        int WatcherPortNumber { get; set; }
        string DirectoryIdentifier { get; set; }
        int UploadFileTimeoutMiliseconds { get; set; } = 1200000;
        long ChunkSizeByByte { get; set; }

        public ETLProvider()
        {
            ServerAddress = ConfigurationManager.AppSettings["FileRepositoryService_ServerAddress"];
            FileRepositoryPortNumber = int.Parse(ConfigurationManager.AppSettings["FileRepositoryService_PortNumber"]);
            ChunkSizeByByte = long.Parse(ConfigurationManager.AppSettings["FileRepositoryService_UploadFileChunkSizeByMegaByte"]) * 1024 * 1024;
            DirectoryIdentifier = ConfigurationManager.AppSettings["FileRepositoryService_DestinationDirectory"];
            WatcherPortNumber = int.Parse(ConfigurationManager.AppSettings["WatcherService_PortNumber"]);
        }

        private string GetHadoopURL()
        {
            return string.Format("http://{0}:{1}/webhdfs/v1", ServerAddress, FileRepositoryPortNumber);
        }

        private string GetWatcherURL()
        {
            return string.Format("http://{0}:{1}", ServerAddress, WatcherPortNumber);
        }

        private List<DirectoryContent> GetDirectoryContent(string path)
        {
            string httpURI = GetHadoopURL() + path + "/?op=LISTSTATUS";
            string result = "";

            using (var client = new WebClient())
            {
                result = client.DownloadString(httpURI);
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

        private string DecodeFromUtf8(string utf8)
        {
            Encoding encoding = Encoding.GetEncoding(1252);
            byte[] temp = Encoding.Default.GetBytes(utf8);
            return Encoding.UTF8.GetString(temp);
        }

        private bool CheckBadPathName(string path)
        {
            if (path.Contains("//") || path.Contains("\\"))
                return true;
            return false;
        }

        private void CreateDirectory(string directoryIdentifier)
        {
            string path = $"/{directoryIdentifier}/";
            if (CheckBadPathName(path))
                throw new ArgumentException("Invalid path in directoryIdentifier");

            List<DirectoryContent> resultList = GetDirectoryContent("/");
            foreach (DirectoryContent item in resultList)
            {
                var directoryInHadoop = item.UriAddress + "/";
                if (directoryInHadoop == path)
                    return;
            }

            string temp = GetHadoopURL() + path + "?op=MKDIRS";
            using (var webClient = new WebClient())
            {
                string result = webClient.UploadString(temp, "Put", "");
            }
        }

        public int GetTotalNumberOfChunks(Stream stream)
        {
            return (int)Math.Ceiling((double)stream.Length / ChunkSizeByByte);
        }

        public async Task UploadFile(Stream stream, string destinationFileName)
        {
            CreateDirectory(DirectoryIdentifier);
            stream.Position = 0;

            string fullName = $"/{DirectoryIdentifier}/{destinationFileName}";
            int totalChunks = (int)Math.Ceiling((double)stream.Length / ChunkSizeByByte);

            using (var response = new HttpClient())
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    string url = string.Empty;
                    long position = (i * (long)ChunkSizeByByte);
                    int toRead = (int)Math.Min(stream.Length - position, ChunkSizeByByte);
                    byte[] buffer = new byte[toRead];
                    stream.Read(buffer, 0, buffer.Length);
                    var b = new ByteArrayContent(buffer, 0, buffer.Length);
                    DateTime dt = DateTime.Now;

                    if (i == 0)
                    {
                        url = GetHadoopURL() + fullName + "?op=CREATE&blocksize=" + ChunkSizeByByte.ToString();
                        await response.PutAsync(url, b);
                    }
                    else
                    {
                        url = GetHadoopURL() + fullName + "?op=APPEND&blocksize=" + ChunkSizeByByte.ToString();
                        await response.PostAsync(url, b);
                    }

                    OnChunkUploaded(new ChunkUploadedEventArgs(stream, i));

                    DateTime dt2 = DateTime.Now;
                    Console.WriteLine(i.ToString() + " " + (dt2 - dt).TotalMilliseconds);
                }

                Console.WriteLine("end of upload file");
            }
        }


        public void CallStartImport(long dataSourceId, string dataSourceName, string mappingFileName = "mapping.xml", string aclFileName = "acl.json")
        {
            RestClient client = new RestClient(GetWatcherURL());
            RestRequest request = new RestRequest("/watcher", Method.POST) { RequestFormat = DataFormat.Json };

            request.AddBody(
                new
                {
                    mainDirectory = DirectoryIdentifier,
                    datasourceId = dataSourceId.ToString(),
                    datasourceFileName = dataSourceName,
                    aclFileName = aclFileName,
                    mappingFileName = mappingFileName
                }
            );

            IRestResponse response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }
        }

        public event EventHandler<ChunkUploadedEventArgs> ChunkUploaded;
        protected void OnChunkUploaded(ChunkUploadedEventArgs e)
        {
            ChunkUploaded?.Invoke(this, e);
        }
    }
}
