using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Datalake
{
    public class FtpServiceProvider
    {
        /// <summary></summary>
        /// <param name="ftpServicePath">Consist of Server IP/name and Service Port if exist</param>
        /// <param name="ftpUserID"></param>
        /// <param name="ftpPassword"></param>
        public FtpServiceProvider(string ftpServicePath, string ftpUserID, string ftpPassword)
        {
            FtpServicePath = ftpServicePath;
            FtpUserID = ftpUserID;
            FtpPassword = ftpPassword;
        }
        public string FtpServicePath { get; set; }
        public string FtpUserID { get; set; }
        public string FtpPassword { get; set; }
        public async Task UploadAsync(FileStream fileStream, long FileLengthInByteUnit, string path, string fileName)
        {
            await UploadOprationAsync(fileStream, FileLengthInByteUnit, path, fileName);
        }
        /// <summary>
        /// Download file from FTP server
        /// </summary>
        /// <param name="sourceFilePath">File path on FTP server</param>
        /// <param name="targetPathToSaveFile">Target file path to save the file (target file name may included in target path)</param>
        public async Task DownloadAsync(string sourceFilePath, string targetPathToSaveFile)
        {
            await DownloadOprationAsync(sourceFilePath, targetPathToSaveFile);
        }
        public void CreateDirectory(string directoryToCreate)
        {
            FtpWebRequest reqFTP;

            // Create FtpWebRequest object from the Uri provided
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(directoryToCreate));

            // Provide the WebPermission Credintials
            reqFTP.Credentials = new NetworkCredential(FtpUserID,
                                                       FtpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            reqFTP.KeepAlive = false;

            // Specify the command to be executed.
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;

            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

        }        

        public struct DirectoryItem
        {
            public Uri BaseUri;

            public string AbsolutePath
            {
                get
                {
                    return string.Format("{0}/{1}", BaseUri, Name);
                }
            }
            public DateTime DateCreated;
            public bool IsDirectory;
            public string Name;
            public List<DirectoryItem> Items;
        }

        public List<DirectoryItem> GetDirectoryInformation(string address)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(address));
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            List<DirectoryItem> returnValue = new List<DirectoryItem>();
            string[] list = null;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (string line in list)
            {
                // Windows FTP Server Response Format
                // DateCreated    IsDirectory    Name
                string data = line;

                // Parse date
                string date = data.Substring(0, 17);
                DateTime dateTime = DateTime.Parse(date);
                data = data.Remove(0, 24);

                // Parse <DIR>
                string dir = data.Substring(0, 5);
                bool isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);
                data = data.Remove(0, 5);
                data = data.Remove(0, 10);

                // Parse name
                string name = data;

                // Create directory info
                DirectoryItem item = new DirectoryItem();
                item.BaseUri = new Uri(address);
                item.DateCreated = dateTime;
                item.IsDirectory = isDirectory;
                item.Name = name;

                //item.Items = item.IsDirectory ? GetDirectoryInformation(item.AbsolutePath) : null;

                returnValue.Add(item);
            }

            return returnValue;
        }
        private async Task UploadOprationAsync(FileStream fileStream, long FileLengthInByteUnit, string path, string fileName)
        {
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path + "/" + fileName));
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            reqFTP.UseBinary = true;
            reqFTP.ContentLength = FileLengthInByteUnit;
            int buffLength = 8000;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileStream;
            Stream strm = reqFTP.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            while (contentLen != 0)
            {
                await strm.WriteAsync(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            strm.Close();
            fs.Close();
        }
        /// <summary>
        /// Download file from FTP server
        /// </summary>
        /// <param name="sourceFilePath">File path on FTP server</param>
        /// <param name="targetPathToSaveFile">Target file path to save the file (target file name may included in target path)</param>
        private async Task DownloadOprationAsync(string sourceFilePath, string targetPathToSaveFile)
        {
            //filePath = <<The full path where the 
            //file is to be created. the>>, 
            //fileName = <<Name of the file to be createdNeed not 
            //name on FTP server. name name()>>
            FileStream outputStream = new FileStream(targetPathToSaveFile, FileMode.Create);

            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" +
                                    FtpServicePath + "/" + sourceFilePath));
            reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            FtpWebResponse response = (FtpWebResponse)(await reqFTP.GetResponseAsync());

            Stream ftpStream = response.GetResponseStream();
            //long cl = response.ContentLength;
            int bufferSize = 2048;
            int readCount;
            byte[] buffer = new byte[bufferSize];

            readCount = await ftpStream.ReadAsync(buffer, 0, bufferSize);
            while (readCount > 0)
            {
                await outputStream.WriteAsync(buffer, 0, readCount);
                readCount = await ftpStream.ReadAsync(buffer, 0, bufferSize);
            }

            ftpStream.Close();
            outputStream.Close();
            response.Close();
        }
        public async Task<bool> DeleteAsync(string path, string ftpUsername, string ftpPassword)
        {
            Uri serverUri = new Uri(path);
            // The serverUri parameter should use the ftp:// scheme.
            // It contains the name of the server file that is to be deleted.
            // Example: ftp://contoso.com/someFile.txt.
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return false;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + FtpServicePath + "/" + serverUri);
            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync());
            //Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }
        public async Task MakeDirectoryAsync(string DirectoryName)
        {
            WebRequest request = WebRequest.Create("ftp://" + FtpServicePath + "/" + DirectoryName + "");
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            using (var resp = (FtpWebResponse)(await request.GetResponseAsync()))
            {
                Console.WriteLine(resp.StatusCode);
            }
        }
    }
}
