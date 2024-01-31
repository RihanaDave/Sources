using GPAS.SearchServer.Access.FileRepositoryService;

namespace GPAS.SearchServer.Access.DataClient
{
    public class FileRepositoryDataClient
    {
        public long GetFileSizeInBytes(string path)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetMediaFileSizeInBytes(path);
            }
            finally
            {
                proxy.Close();
            }
        }

        public byte[] GetDocumentContentFromFileRepository(long id)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.DownloadDocumentFile(id);
            }
            finally
            {
                proxy.Close();
            }
        }

        public byte[] GetMediaContentFromFileRepository(string mediaUri)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.DownloadMediaFile(mediaUri);
            }
            finally
            {
                proxy.Close();
            }
        }
    }
}