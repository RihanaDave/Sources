using GPAS.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GPAS.DataImport.Material.SemiStructured.Tests
{
    [TestClass()]
    public class MaterialBaseTests
    {
        StreamUtility utilities = new StreamUtility();

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest_EmptyMateral_MayDeserializedEmpty()
        {
            // ============== Arrange ==============
            var testMaterial = new CsvFileMaterial();

            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            MaterialBase resultMaterial;
            // ==============   Act   ==============
            // سری‌سازی کلاس خالی
            serializer.Serialize(testMappingStreamToSerailize, testMaterial);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new MaterialBaseSerializer();
            resultMaterial = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.IsTrue(resultMaterial is CsvFileMaterial);
        }
        
        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest_EmptyCsvMateral_MayDeserializedWithDefaultSeparator()
        {
            // ============== Arrange ==============
            var testMaterial = new CsvFileMaterial();

            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CsvFileMaterial resultMaterial;
            // ==============   Act   ==============
            // سری‌سازی کلاس خالی
            serializer.Serialize(testMappingStreamToSerailize, testMaterial);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new MaterialBaseSerializer();
            resultMaterial = (CsvFileMaterial)serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(',', resultMaterial.Separator);
        }

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest_EmptyCsvMateral_MayDeserializedWithEmptyPath()
        {
            // ============== Arrange ==============
            var testMaterial = new CsvFileMaterial();

            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CsvFileMaterial resultMaterial;
            // ==============   Act   ==============
            // سری‌سازی کلاس خالی
            serializer.Serialize(testMappingStreamToSerailize, testMaterial);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new MaterialBaseSerializer();
            resultMaterial = (CsvFileMaterial)serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(string.Empty, resultMaterial.FileJobSharePath);
        }

        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest_CsvMateralWithSpecificSeparator_MayDeserializedWithTheSpecifiedSeparator()
        {
            // ============== Arrange ==============
            var testMaterial = new CsvFileMaterial() { Separator = ';' };

            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CsvFileMaterial resultMaterial;
            // ==============   Act   ==============
            // سری‌سازی کلاس خالی
            serializer.Serialize(testMappingStreamToSerailize, testMaterial);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new MaterialBaseSerializer();
            resultMaterial = (CsvFileMaterial)serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(';', resultMaterial.Separator);
        }


        [TestMethod()]
        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest_CsvMateralWithSpecificPath_MayDeserializedWithTheSpecifiedPath()
        {
            // ============== Arrange ==============
            string testPath = "\testpath\testfile.csv";
            var testMaterial = new CsvFileMaterial() { FileJobSharePath = testPath };

            MaterialBaseSerializer serializer = new MaterialBaseSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CsvFileMaterial resultMaterial;
            // ==============   Act   ==============
            // سری‌سازی کلاس خالی
            serializer.Serialize(testMappingStreamToSerailize, testMaterial);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new MaterialBaseSerializer();
            resultMaterial = (CsvFileMaterial)serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(testPath, resultMaterial.FileJobSharePath);
        }
    }
}