using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GPAS.FtpServiceAccess
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

        public async Task UploadAsync(FileStream fileStream, long FileLengthInByteUnit, string fileName)
        {
            await UploadOprationAsync(fileStream, FileLengthInByteUnit, fileName);
        }
        public void Upload(FileStream fileStream, long FileLengthInByteUnit, string fileName)
        {
            UploadOpration(fileStream, FileLengthInByteUnit, fileName);
        }

        /// <summary>
        /// Download file from FTP server
        /// </summary>
        /// <param name="sourceFilePath">File path on FTP server</param>
        /// <param name="targetFilePathToSaveFile">Target file path to save the file (target file name may included in target path)</param>
        public async Task DownloadAsync(string sourceFilePath, string targetFilePathToSaveFile)
        {
            await DownloadOprationAsync(sourceFilePath, targetFilePathToSaveFile);
        }
        public async Task<List<FileInfo>> DownloadDirectoryContent(string sourceDirectoryPathOnRemoteServer, DirectoryInfo targetDirectory, int parallelismFactor = 8)
        {
            List<DirectoryContentItem> sourceItems
                = (await GetRemoteDirectoryContentAsync(sourceDirectoryPathOnRemoteServer));
            List<FileInfo> targetFiles = new List<FileInfo>();

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = parallelismFactor };
            Parallel.ForEach(sourceItems, options, (currentItem) =>
            {
                IEnumerable<FileInfo> relatedFiles;
                if (currentItem.IsDirectory)
                {
                    DirectoryInfo newSubDir = targetDirectory.CreateSubdirectory(currentItem.Name);
                    Task<List<FileInfo>> downloadDirTask = DownloadDirectoryContent(currentItem.FullPath, newSubDir);
                    Task.WaitAll(new Task[] { downloadDirTask });
                    relatedFiles = downloadDirTask.Result;
                }
                else
                {
                    FileInfo fi = new FileInfo(currentItem.FullPath);
                    string localFilePath = $"{targetDirectory.FullName}\\{fi.Name}";
                    Task downloadFileTask = DownloadOprationAsync(currentItem.FullPath, localFilePath);
                    Task.WaitAll(new Task[] { downloadFileTask });
                    relatedFiles = new FileInfo[] { new FileInfo(localFilePath) };
                }
                lock (DownloadDirectoryContentLockObject)
                {
                    targetFiles.AddRange(relatedFiles);
                }
            });
            return targetFiles;
        }
        private readonly object DownloadDirectoryContentLockObject = new object();
        private async Task<List<DirectoryContentItem>> GetRemoteDirectoryContentAsync(string sourceDirectoryPath)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest reqFTP = GetListDirectoryRequest(sourceDirectoryPath);
                response = (FtpWebResponse)(await reqFTP.GetResponseAsync());
                return GetDirectoryContentsFromListDirectoryResponse(sourceDirectoryPath, response);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
        private List<DirectoryContentItem> GetRemoteDirectoryContent(string sourceDirectoryPath)
        {
            FtpWebResponse response = null;
            try
            {
                FtpWebRequest reqFTP = GetListDirectoryRequest(sourceDirectoryPath);
                response = (FtpWebResponse)(reqFTP.GetResponse());
                return GetDirectoryContentsFromListDirectoryResponse(sourceDirectoryPath, response);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
        private List<DirectoryContentItem> GetDirectoryContentsFromListDirectoryResponse(string listedDirectoryPath, FtpWebResponse response)
        {
            List<DirectoryContentItem> DirectoryContent = new List<DirectoryContentItem>();
            StreamReader streamReader = null;
            try
            {
                streamReader = new StreamReader(response.GetResponseStream());

                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    string subItemName = line.Split(new string[] { " " }, 4, StringSplitOptions.RemoveEmptyEntries)[3];
                    DirectoryContentItem newItem = new DirectoryContentItem()
                    {
                        FullPath = $"{listedDirectoryPath}/{subItemName}",
                        Name = subItemName,
                        IsDirectory = line.Contains("<DIR>")
                    };
                    DirectoryContent.Add(newItem);
                    line = streamReader.ReadLine();
                }
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
            }
            return DirectoryContent;
        }

        private FtpWebRequest GetListDirectoryRequest(string sourceDirectoryPath)
        {
            FtpWebRequest reqFTP = null;
            reqFTP = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + FtpServicePath + "/" + sourceDirectoryPath));
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            //reqFTP.UseBinary = true;
            return reqFTP;
        }

        private async Task UploadOprationAsync(FileStream fileStream, long FileLengthInByteUnit, string fileName)
        {
            FtpWebRequest reqFTP = GetUploadRequest(FileLengthInByteUnit, fileName);

            // The buffer size is set to 2kb
            int buffLength = 8000;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read 
            //the file to be uploaded
            FileStream fs = fileStream;

            // Stream to which the file to be upload is written
            Stream strm = await reqFTP.GetRequestStreamAsync();

            // Read from the file stream 2kb at a time
            contentLen = await fs.ReadAsync(buff, 0, buffLength);

            // Till Stream content ends
            while (contentLen != 0)
            {
                // Write Content from the file stream to the 
                // FTP Upload Stream
                await strm.WriteAsync(buff, 0, contentLen);
                contentLen = await fs.ReadAsync(buff, 0, buffLength);
            }

            // Close the file stream and the Request Stream
            strm.Close();
            fs.Close();
        }
        private void UploadOpration(FileStream fileStream, long FileLengthInByteUnit, string fileName)
        {
            FtpWebRequest reqFTP = GetUploadRequest(FileLengthInByteUnit, fileName);

            // The buffer size is set to 2kb
            int buffLength = 8000;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read 
            //the file to be uploaded
            FileStream fs = fileStream;

            // Stream to which the file to be upload is written
            Stream strm = reqFTP.GetRequestStream();

            // Read from the file stream 2kb at a time
            contentLen = fs.Read(buff, 0, buffLength);

            // Till Stream content ends
            while (contentLen != 0)
            {
                // Write Content from the file stream to the 
                // FTP Upload Stream
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }

            // Close the file stream and the Request Stream
            strm.Close();
            fs.Close();
        }
        private FtpWebRequest GetUploadRequest(long FileLengthInByteUnit, string fileName)
        {
            //FileInfo fileInf = new FileInfo(filePath);
            //string uri = "ftp://" + FtpServicePath + "/" + fileName;
            FtpWebRequest reqFTP;

            // Create FtpWebRequest object from the Uri provided
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(
                      "ftp://" + FtpServicePath + "/" + fileName));

            // Provide the WebPermission Credintials
            reqFTP.Credentials = new NetworkCredential(FtpUserID,
                                                       FtpPassword);

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            reqFTP.KeepAlive = false;

            // Specify the command to be executed.
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            reqFTP.UseBinary = true;

            // Notify the server about the size of the uploaded file
            reqFTP.ContentLength = FileLengthInByteUnit;
            return reqFTP;
        }

        /// <summary>
        /// Download file from FTP server
        /// </summary>
        /// <param name="sourceFilePath">File path on FTP server</param>
        /// <param name="targetFilePathToSaveFile">Target file path to save the file (target file name may included in target path)</param>
        private async Task DownloadOprationAsync(string sourceFilePath, string targetFilePathToSaveFile)
        {
            FileStream outputStream = null;
            FtpWebResponse response = null;
            Stream ftpStream = null;
            try
            {
                //filePath = <<The full path where the 
                //file is to be created. the>>, 
                //fileName = <<Name of the file to be createdNeed not 
                //name on FTP server. name name()>>
                outputStream = new FileStream(targetFilePathToSaveFile, FileMode.Create);

                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" +
                                        FtpServicePath + "/" + sourceFilePath));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                response = (FtpWebResponse)(await reqFTP.GetResponseAsync());

                ftpStream = response.GetResponseStream();
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
            }
            finally
            {
                if (ftpStream != null) ftpStream.Close();
                if (response != null) response.Close();
                if (outputStream != null) outputStream.Close();
            }
        }

        public byte[] DownloadData(string sourceFilePath)
        {
            string sourceFullPath = $"ftp://{FtpServicePath}/{sourceFilePath}";
            NetworkCredential requestCridential = new NetworkCredential(FtpUserID, FtpPassword);
            using (WebClient downloader = new WebClient())
            {
                downloader.Credentials = requestCridential;
                try
                {
                    return downloader.DownloadData(sourceFullPath);
                }
                catch (WebException ex)
                {
                    ExceptionDetailGenerator exDetailGenerator = new ExceptionDetailGenerator();
                    throw exDetailGenerator.AppendWebExceptionResonse(ex);
                }
                finally
                {
                    if (downloader == null)
                        downloader.Dispose();
                }
            }
        }
        public async Task<bool> DeleteFileAsync(string path)
        {
            //Uri serverUri = new Uri(path);
            //// The serverUri parameter should use the ftp:// scheme.
            //// It contains the name of the server file that is to be deleted.
            //// Example: ftp://contoso.com/someFile.txt.
            //if (serverUri.Scheme != Uri.UriSchemeFtp)
            //{
            //    return false;
            //}
            FtpWebRequest request = GetDeleteFileRequest(path);

            FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync());
            //Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }
        public void DeleteFile(string path)
        {
            //Uri serverUri = new Uri(path);
            //// The serverUri parameter should use the ftp:// scheme.
            //// It contains the name of the server file that is to be deleted.
            //// Example: ftp://contoso.com/someFile.txt.
            //if (serverUri.Scheme != Uri.UriSchemeFtp)
            //{
            //    return false;
            //}
            FtpWebRequest request = GetDeleteFileRequest(path);

            FtpWebResponse response = (FtpWebResponse)(request.GetResponse());
            //Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
        }
        private FtpWebRequest GetDeleteFileRequest(string path)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + FtpServicePath + "/" + path);
            request.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            return request;
        }

        public void DeleteDirectory(string path)
        {
            List<DirectoryContentItem> pathContents = GetRemoteDirectoryContent(path);
            foreach (DirectoryContentItem item in pathContents)
            {
                if (item.IsDirectory)
                {
                    DeleteDirectory(item.FullPath);
                }
                else
                {
                    DeleteFile(item.FullPath);
                }
            }
            FtpWebRequest request = GetRemoveEmptyDirectoryRequest(path);
            FtpWebResponse response = (FtpWebResponse)(request.GetResponse());
            //Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
        }
        public async Task DeleteDirectoryAsync(string path)
        {
            List<DirectoryContentItem> pathContents = GetRemoteDirectoryContent(path);
            foreach (DirectoryContentItem item in pathContents)
            {
                if (item.IsDirectory)
                {
                    await DeleteDirectoryAsync(item.FullPath);
                }
                else
                {
                    await DeleteFileAsync(item.FullPath);
                }
            }
            FtpWebRequest request = GetRemoveEmptyDirectoryRequest(path);
            FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync());
            //Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
        }
        private FtpWebRequest GetRemoveEmptyDirectoryRequest(string path)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + FtpServicePath + "/" + path);
            request.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            request.Method = WebRequestMethods.Ftp.RemoveDirectory;
            return request;
        }
        public async Task MakeDirectoryAsync(string DirectoryName)
        {
            WebRequest request = GetMakeDirectoryRequest(DirectoryName);
            using (var resp = (FtpWebResponse)(await request.GetResponseAsync()))
            {
                Console.WriteLine(resp.StatusCode);
            }
        }
        public void MakeDirectory(string DirectoryName)
        {
            WebRequest request = GetMakeDirectoryRequest(DirectoryName);
            using (var resp = (FtpWebResponse)(request.GetResponse()))
            {
                Console.WriteLine(resp.StatusCode);
            }
        }
        private WebRequest GetMakeDirectoryRequest(string DirectoryName)
        {
            WebRequest request = WebRequest.Create("ftp://" + FtpServicePath + "/" + DirectoryName + "");
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            return request;
        }
    }
}
