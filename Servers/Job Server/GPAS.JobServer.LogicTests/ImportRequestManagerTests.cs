using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.JobServer.Logic.SemiStructuredDataImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GPAS.JobServer.Logic.Entities;
using GPAS.DataImport.Material.SemiStructured;

namespace GPAS.JobServer.Logic.Tests
{
    [TestClass()]
    public class ImportRequestManagerTests
    {
        // این تست به خاطر وابستگی به مسیر مطلق خارج از رهیافت، غیرفعال شد
        //[TestMethod()]
        public void ImportFilesTest()
        {
            SemiStructuredDataImportRequestMetadata importFileRequest = new SemiStructuredDataImportRequestMetadata();

            var temp = File.ReadAllBytes(@"C:\Users\arya\Desktop\1.XML");
            importFileRequest.serializedTypeMapping = temp;

            var material = new CsvFileMaterial()
            {
                FileJobSharePath = "63681b57-4bb6-41e7-a68f-36cdcad600c4//1.csv"
            };
            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream materialMemStream = new MemoryStream();
            serializer.Serialize(materialMemStream, material);
            importFileRequest.serializedMaterialBase = materialMemStream.GetBuffer();

            RequestManager importFileProvider = new RequestManager();
            importFileProvider.RegisterNewImportRequests(new SemiStructuredDataImportRequestMetadata[] { importFileRequest });

            Assert.Fail();
        }
    }
}