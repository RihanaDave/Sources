using GPAS.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GPAS.DataImport.DataMapping.SemiStructured.Tests
{
    [TestClass()]
    public class TypeMappingTests
    {
        StreamUtility utilities = new StreamUtility();

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_EmptyTypeMapping()
        {
            // ============== Arrange ==============
            var testMapping = new TypeMapping();

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultMapping.ObjectsMapping.Count, 0);
        }

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_SingleObjectMappingWithAMultiValuePropertyMapping()
        {
            // ============== Arrange ==============
            var testConstValuePropMap = new ConstValueMappingItem("ثابت ۱");

            var testMutilValuePropMap = new MultiValueMappingItem();
            testMutilValuePropMap.MultiValues.Add(testConstValuePropMap);
            testMutilValuePropMap.ResolutionOption = PropertyInternalResolutionOption.MustMatch;

            var testPropMap = new PropertyMapping(new OntologyTypeMappingItem("نوع ویژگی ۱"), testMutilValuePropMap);
            testPropMap.IsSetAsDisplayName = true;

            var testObjMap = new ObjectMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱");
            testObjMap.AddProperty(testPropMap);

            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testObjMap);
            
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultMapping.ObjectsMapping.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].PropertyType.TypeUri
                , testPropMap.PropertyType.TypeUri);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].IsSetAsDisplayName, true);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].Value.GetType(), typeof(MultiValueMappingItem));

            MultiValueMappingItem deserializedValueMap = (resultMapping.ObjectsMapping[0].Properties[0].Value as MultiValueMappingItem);

            Assert.AreEqual(deserializedValueMap.ResolutionOption, testMutilValuePropMap.ResolutionOption);
            Assert.AreEqual(deserializedValueMap.MultiValues.Count, 1);
            Assert.AreEqual(deserializedValueMap.MultiValues[0].GetType(), typeof(ConstValueMappingItem));
            Assert.AreEqual((deserializedValueMap.MultiValues[0] as ConstValueMappingItem).ConstValue, testConstValuePropMap.ConstValue);
        }


        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_SingleObjectMappingWithATableBasedPropertyMapping()
        {
            // ============== Arrange ==============
            var testTableBasedPropMap = new TableColumnMappingItem(5, "عنوان ۱۱", PropertyInternalResolutionOption.FindMatch);
            testTableBasedPropMap.RegularExpressionPattern = "الگوی ۱";

            var testPropMap = new PropertyMapping(new OntologyTypeMappingItem("نوع ویژگی ۱"), testTableBasedPropMap);
            testPropMap.IsSetAsDisplayName = false;

            var testObjMap = new ObjectMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱");
            testObjMap.AddProperty(testPropMap);

            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testObjMap);

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultMapping.ObjectsMapping.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].PropertyType.TypeUri
                , testPropMap.PropertyType.TypeUri);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].IsSetAsDisplayName, false);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties[0].Value.GetType(), typeof(TableColumnMappingItem));

            var deserializedValueMap = (resultMapping.ObjectsMapping[0].Properties[0].Value as TableColumnMappingItem);

            Assert.AreEqual(deserializedValueMap.ColumnIndex, testTableBasedPropMap.ColumnIndex);
            Assert.AreEqual(deserializedValueMap.ColumnTitle, testTableBasedPropMap.ColumnTitle);
            Assert.AreEqual(deserializedValueMap.RegularExpressionPattern, testTableBasedPropMap.RegularExpressionPattern);
            Assert.AreEqual(deserializedValueMap.ResolutionOption, testTableBasedPropMap.ResolutionOption);
        }

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_OneSimpleDocumentMapping()
        {
            // ============== Arrange ==============
            var testDocMap = new DocumentMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱", new TableColumnMappingItem(0));
            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testDocMap);

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultMapping.ObjectsMapping.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties.Count, 0);
            Assert.IsTrue(resultMapping.ObjectsMapping[0] is DocumentMapping);
            Assert.AreEqual(testDocMap.IsDocumentNameAsDisplayName,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).IsDocumentNameAsDisplayName);
            if (testDocMap.DocumentPathMapping == null)
            {
                Assert.IsNull((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping);
            }
            else
            {
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ColumnIndex
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ColumnIndex);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ColumnTitle
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ColumnTitle);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).RegularExpressionPattern
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).RegularExpressionPattern);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ResolutionOption
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ResolutionOption);
            }
        }

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_OneSimpleDocumentMapping_PathOptionsMayHaveDefaultValues()
        {
            // ============== Arrange ==============
            var testDocMap = new DocumentMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱", new TableColumnMappingItem(0));
            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testDocMap);

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(testDocMap.PathOptions.SingleFile,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.SingleFile);
            Assert.AreEqual(testDocMap.PathOptions.FolderContent,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.FolderContent);
            Assert.AreEqual(testDocMap.PathOptions.SubFoldersContent,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.SubFoldersContent);
        }

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_OneSimpleDocumentMappingWithCustomValues()
        {
            // ============== Arrange ==============
            TableColumnMappingItem pathMapping = new TableColumnMappingItem(10, "عنوان ۲", PropertyInternalResolutionOption.FindMatch)
            {
                RegularExpressionPattern = "test Regex"
            };
            var testDocMap = new DocumentMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱", pathMapping)
            {
                ID = "testID",
                IsDocumentNameAsDisplayName = false
            };
            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testDocMap);

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultMapping.ObjectsMapping.Count, 1);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].ID, testDocMap.ID);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].MappingTitle.ConstValue, testDocMap.MappingTitle.ConstValue);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].ObjectType.TypeUri, testDocMap.ObjectType.TypeUri);
            Assert.AreEqual(resultMapping.ObjectsMapping[0].Properties.Count, 0);
            Assert.IsTrue(resultMapping.ObjectsMapping[0] is DocumentMapping);
            Assert.AreEqual(testDocMap.IsDocumentNameAsDisplayName,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).IsDocumentNameAsDisplayName);
            if (testDocMap.DocumentPathMapping == null)
            {
                Assert.IsNull((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping);
            }
            else
            {
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ColumnIndex
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ColumnIndex);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ColumnTitle
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ColumnTitle);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).RegularExpressionPattern
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).RegularExpressionPattern);
                Assert.AreEqual((testDocMap.DocumentPathMapping as TableColumnMappingItem).ResolutionOption
                    , ((resultMapping.ObjectsMapping[0] as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ResolutionOption);
            }
        }

        [TestCategory("ورود داده‌ها")]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        [TestMethod()]
        public void Serialize_OneSimpleDocumentMappingWithCustomValues_PathOptionsMayHaveCustomValues()
        {
            // ============== Arrange ==============
            var testDocMap = new DocumentMapping(new OntologyTypeMappingItem("نوع شئ ۱"), "عنوان ۱", new TableColumnMappingItem(0))
            {
                PathOptions = new DocumentPathOptions()
                {
                    SingleFile = false,
                    FolderContent = false,
                    SubFoldersContent = false
                }
            };
            var testMapping = new TypeMapping();
            testMapping.ObjectsMapping.Add(testDocMap);

            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream testMappingStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            TypeMapping resultMapping;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testMappingStreamToSerailize, testMapping);
            serializedDataBytesArray = utilities.ReadStreamAsBytesArray(testMappingStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new TypeMappingSerializer();
            resultMapping = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(testDocMap.PathOptions.SingleFile,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.SingleFile);
            Assert.AreEqual(testDocMap.PathOptions.FolderContent,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.FolderContent);
            Assert.AreEqual(testDocMap.PathOptions.SubFoldersContent,
                (resultMapping.ObjectsMapping[0] as DocumentMapping).PathOptions.SubFoldersContent);
        }
    }
}