using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace GPAS.Utility
{
    public class FileUtility
    {
        public void OpenFileWithWindowsAppropriateApplication(string path)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            process.StartInfo = startInfo;

            startInfo.FileName = path;
            process.Start();
        }

        public byte[] ComputeFileHashBytesFromFileStream(Stream fileStream)
        {
            byte[] md5HashOfFile;

            using (var md5 = MD5.Create())
            {
                md5HashOfFile = md5.ComputeHash(fileStream);
            }

            return md5HashOfFile;
        }

        public byte[] ComputeFileBytesFromFileFilePath(string filePath)
        {
            byte[] md5HashOfFile;

            using (var stream = File.OpenRead(filePath))
            {
                md5HashOfFile = ComputeFileHashBytesFromFileStream(stream);
            }

            return md5HashOfFile;
        }

        public FileHash ComputeFileHashFromFileFilePath(string filePath)
        {
            byte[] md5HashOfFile = ComputeFileBytesFromFileFilePath(filePath);
            return new FileHash(md5HashOfFile);
        }
    }
}
