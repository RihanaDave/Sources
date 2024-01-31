using GPAS.FileRepository.Logic.Entities;
using GPAS.FileRepository.Logic.HierarchicalFileStorage;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;

namespace GPAS.FileRepository.Logic
{
    public class MediaManager
    {
        private static readonly string MediasDirectory = "/Medias/";
        private static string PluginPath = null;
        private const string HierarchicalFileStoragePluginRelativePath = "HierarchicalFileStoragePluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public MediaManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[HierarchicalFileStoragePluginRelativePath];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{HierarchicalFileStoragePluginRelativePath}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }

            // کد ترکیب اسمبلی پلاگین برگرفته از مثال مایکروسافت در آدرس زیر است:
            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        #region Server Management
        public void Init()
        {
            StorageClient.CreateDirectory(MediasDirectory);
            if (!IsStorageAvailable())
            {
                throw new InvalidOperationException("Storage is not available as it's directory was created!");
            }
        }
        public bool IsStorageAvailable()
        {
            return StorageClient.IsAvailable();
        }
        public void RemoveAllFiles()
        {
            StorageClient.RemoveAllFiles();
            StorageClient.CreateDirectory(MediasDirectory);
        }
        #endregion

        #region Medias
        public List<DirectoryContent> GetMediaPathContent(string path)
        {
            return StorageClient.GetDirectoryContent(path);
        }
        public bool CreateMediaDirectory(string path)
        {
            return StorageClient.CreateDirectory(path);
        }
        public bool RenameMediaDirectory(string sourcePath, string targetPath)
        {
            return StorageClient.RenameDirectory(sourcePath, targetPath);
        }
        public bool DeleteMediaDirectory(string path)
        {
            return StorageClient.DeleteDirectory(path);
        }
        public bool UploadMediaFile(byte[] fileContent, string fileName, string targetPath)
        {
            StorageClient.SaveFile(fileContent, fileName, targetPath);
            return true;
        }
        public byte[] DownloadMediaFile(string filePath)
        {
            return StorageClient.LoadFile(filePath);
        }
        public long GetMediaFileSizeInBytes(string filePath)
        {
            return StorageClient.GetFileSizeInBytes(filePath);
        }
        #endregion
    }
}
