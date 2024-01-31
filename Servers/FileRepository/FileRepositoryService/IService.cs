using GPAS.FileRepository.Logic.Entities;
using System.Collections.Generic;
using System.ServiceModel;

namespace GPAS.FileRepository
{
    [ServiceContract]
    interface IService
    {
        #region Server Management
        [OperationContract]
        string test();
        [OperationContract]
        bool IsServiceAvailable();
        [OperationContract]
        void RemoveAllFiles();
        #endregion

        #region Medias
        [OperationContract]
        List<DirectoryContent> GetMediaPathContent(string path);
        [OperationContract]
        bool CreateMediaDirectory(string path);
        [OperationContract]
        bool RenameMediaDirectory(string sourcePath, string targetPath);
        [OperationContract]
        bool DeleteMediaDirectory(string path);
        [OperationContract]
        bool UploadMediaFile(byte[] fileToUpload, string fileName, string targetPath);
        [OperationContract]
        byte[] DownloadMediaFile(string mediaPath);
        [OperationContract]
        long GetMediaFileSizeInBytes(string mediaPath);
        #endregion

        #region Data-Sources & Documents
        [OperationContract]
        void UploadDocumentFileByName(string docName, byte[] docContent);
        [OperationContract]
        void UploadDocumentFile(long docID, byte[] docContent);
        [OperationContract]
        void UploadDataSourceFile(long dataSourceID, byte[] dataSourceContent);
        [OperationContract]
        void UploadFileAsDocumentAndDataSource(byte[] fileContent, long docID, long dataSourceID);
        [OperationContract]
        void UploadDocumentFromJobShare(long docID, string docJobSharePath);
        [OperationContract]
        void UploadDataSourceFromJobShare(long dataSourceID, string dataSourceJobSharePath);
        [OperationContract]
        byte[] DownloadDocumentFile(long docID);
        [OperationContract]
        byte[] DownloadDataSourceFile(long dataSourceID);

        [OperationContract]
        long GetDataSourceAndDocumentFileSizeInBytes(string docId);
        #endregion

        [OperationContract]
        void IsAvailable();

        [OperationContract]
        byte[] DownloadDataSourceFileByName(string dataSourceName);
    }
}
