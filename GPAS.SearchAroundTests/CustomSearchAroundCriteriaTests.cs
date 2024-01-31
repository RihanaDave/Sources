using GPAS.FilterSearch;
using GPAS.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GPAS.SearchAround.Tests
{
    [TestClass()]
    public class CustomSearchAroundCriteriaTests
    {
        StreamUtility streamUtil = new StreamUtility();

        [TestCategory("Custum Search-Around")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeEmptyCriteria_DeserializeSameCriteria()
        {
            // Arrange
            CustomSearchAroundCriteria testCriteria = new CustomSearchAroundCriteria();

            //  Arrange - Aux
            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            MemoryStream testCrteriaStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CustomSearchAroundCriteria resultCriteria;
            // Act
            // سری‌سازی درخواست خالی
            serializer.Serialize(testCrteriaStreamToSerailize, testCriteria);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testCrteriaStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new CustomSearchAroundCriteriaSerializer();
            resultCriteria = serializer.Deserialize(serializedDataStream);
            // Assert
            Assert.AreEqual(resultCriteria.SourceSetObjectTypes.Length, 0);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet.Length, 0);
        }

        [TestCategory("Custum Search-Around")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeCriteriaWithOneLink_DeserializeSameCriteria()
        {
            // Arrange
            CustomSearchAroundCriteria testCriteria = new CustomSearchAroundCriteria();
            SearchAroundStep testStep = new SearchAroundStep()
            {
                LinkTypeUri = new string[] { "نوع ۱" },
                TargetObjectTypeUri = new string[] { "نوع ۲" },
                TargetObjectPropertyCriterias = new PropertyValueCriteria[] { }
            };
            testCriteria.LinksFromSearchSet = new SearchAroundStep[] { testStep };

            //  Arrange - Aux
            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            MemoryStream testCrteriaStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CustomSearchAroundCriteria resultCriteria;
            // Act
            // سری‌سازی درخواست خالی
            serializer.Serialize(testCrteriaStreamToSerailize, testCriteria);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testCrteriaStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new CustomSearchAroundCriteriaSerializer();
            resultCriteria = serializer.Deserialize(serializedDataStream);
            // Assert
            Assert.AreEqual(resultCriteria.SourceSetObjectTypes.Length, 0);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].LinkTypeUri.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].LinkTypeUri[0], testCriteria.LinksFromSearchSet[0].LinkTypeUri[0]);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectTypeUri.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectTypeUri[0], testCriteria.LinksFromSearchSet[0].TargetObjectTypeUri[0]);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias.Length, 0);
        }

        [TestCategory("Custum Search-Around")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeCriteriaWithOneLinkToAnObjectWithProperty_DeserializeSameCriteria()
        {
            // Arrange
            CustomSearchAroundCriteria testCriteria = new CustomSearchAroundCriteria();
            LongPropertyCriteriaOperatorValuePair testPropertyOpertatorValuePair = new LongPropertyCriteriaOperatorValuePair()
            {
                CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan,
                CriteriaValue = 123
            };
            PropertyValueCriteria testPropertyCriteria = new PropertyValueCriteria()
            {
                OperatorValuePair = testPropertyOpertatorValuePair,
                PropertyTypeUri = "نوع ۳"
            };
            SearchAroundStep testStep = new SearchAroundStep()
            {
                LinkTypeUri = new string[] { "نوع ۱" },
                TargetObjectTypeUri = new string[] { "نوع ۲" },
                TargetObjectPropertyCriterias = new PropertyValueCriteria[] { testPropertyCriteria }
            };
            testCriteria.LinksFromSearchSet = new SearchAroundStep[] { testStep };

            //  Arrange - Aux
            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            MemoryStream testCrteriaStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            CustomSearchAroundCriteria resultCriteria;
            // Act
            // سری‌سازی درخواست خالی
            serializer.Serialize(testCrteriaStreamToSerailize, testCriteria);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testCrteriaStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new CustomSearchAroundCriteriaSerializer();
            resultCriteria = serializer.Deserialize(serializedDataStream);
            // Assert
            Assert.AreEqual(resultCriteria.SourceSetObjectTypes.Length, 0);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].LinkTypeUri.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].LinkTypeUri[0], testCriteria.LinksFromSearchSet[0].LinkTypeUri[0]);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectTypeUri.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectTypeUri[0], testCriteria.LinksFromSearchSet[0].TargetObjectTypeUri[0]);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias.Length, 1);
            Assert.AreEqual(resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].PropertyTypeUri, testPropertyCriteria.PropertyTypeUri);
            Assert.IsInstanceOfType(resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair, typeof(LongPropertyCriteriaOperatorValuePair));
            Assert.AreEqual((resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaOperator
                , testPropertyOpertatorValuePair.CriteriaOperator);
            Assert.AreEqual((resultCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaValue
                , testPropertyOpertatorValuePair.CriteriaValue);
        }
    }
}