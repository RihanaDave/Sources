using GPAS.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GPAS.FilterSearch.Tests
{
    [TestClass()]
    public class QueryTests
    {
        StreamUtility streamUtil = new StreamUtility();

        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeEmptyQuery_DeserializeSameQuery()
        {
            // ============== Arrange ==============
            Query testQuery = new Query();

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 0);
        }

        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeQueryWithTwoKeywordCriterias_DeserializeSameQuery()
        {

            // ============== Arrange ==============
            Query testQuery = new Query();
            // مقداردهی درخواست اولیه با انواع معیارها
            const string testKeyword1 = "کلید 1";
            const string testKeyword2 = "کلید 2";
            testQuery.CriteriasSet.Criterias.Add(new KeywordCriteria() { Keyword = testKeyword1 });
            testQuery.CriteriasSet.Criterias.Add(new KeywordCriteria() { Keyword = testKeyword2 });

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 2);
            resultQuery.CriteriasSet.Criterias.Single(c => (c as KeywordCriteria).Keyword.Equals(testKeyword1));
            resultQuery.CriteriasSet.Criterias.Single(c => (c as KeywordCriteria).Keyword.Equals(testKeyword2));
        }

        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeQueryWithTwoObjectTypeCriterias_DeserializeSameQuery()
        {
            // ============== Arrange ==============
            Query testQuery = new Query();
            // مقداردهی درخواست اولیه با انواع معیارها
            const string testObjectType11 = "شئ آزمایشی نوع ۱ـ۱";
            const string testObjectType12 = "شئ آزمایشی نوع ۱ـ۲";
            ObservableCollection<string> testObjectTypes1 = new ObservableCollection<string>();
            testObjectTypes1.Add(testObjectType11);
            testObjectTypes1.Add(testObjectType12);
            testQuery.CriteriasSet.Criterias.Add(new ObjectTypeCriteria() { ObjectsTypeUri = testObjectTypes1 });
            const string testObjectType21 = "شئ آزمایشی نوع ۲ـ۱";
            ObservableCollection<string> testObjectTypes2 = new ObservableCollection<string>();
            testObjectTypes2.Add(testObjectType21);
            testQuery.CriteriasSet.Criterias.Add(new ObjectTypeCriteria() { ObjectsTypeUri = testObjectTypes2 });

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 2);
            ObjectTypeCriteria resultQueryObjectTypeCriteria1
                = (ObjectTypeCriteria)resultQuery.CriteriasSet.Criterias.Single
                    (c => (c as ObjectTypeCriteria).ObjectsTypeUri.Count == testObjectTypes1.Count);
            resultQueryObjectTypeCriteria1.ObjectsTypeUri.Single(t => t.Equals(testObjectType11));
            resultQueryObjectTypeCriteria1.ObjectsTypeUri.Single(t => t.Equals(testObjectType12));
            ObjectTypeCriteria resultQueryObjectTypeCriteria2
                = (ObjectTypeCriteria)resultQuery.CriteriasSet.Criterias.Single
                    (c => (c as ObjectTypeCriteria).ObjectsTypeUri.Count == testObjectTypes2.Count);
            resultQueryObjectTypeCriteria2.ObjectsTypeUri.Single(t => t.Equals(testObjectType21));
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeQueryWithTwoPropertyValueCriterias_DeserializeSameQuery()
        {
            // ============== Arrange ==============
            Query testQuery = new Query();
            // مقداردهی درخواست اولیه با انواع معیارها
            const string testPropertyTypeUri1 = "ویژگی آزمایشی نوع ۱";
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri1 });
            const string testPropertyTypeUri2 = "ویژگی آزمایشی نوع 2";
            LongPropertyCriteriaOperatorValuePair testOperatorValuePair2 = new LongPropertyCriteriaOperatorValuePair() { CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals, CriteriaValue = 125 };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri2, OperatorValuePair = testOperatorValuePair2 });
            const string testPropertyTypeUri3 = "ویژگی آزمایشی نوع 3";
            FloatPropertyCriteriaOperatorValuePair testOperatorValuePair3 = new FloatPropertyCriteriaOperatorValuePair() { CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals, CriteriaValue = 150.50F, EqualityPrecision = 2 };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri3, OperatorValuePair = testOperatorValuePair3 });
            const string testPropertyTypeUri4 = "ویژگی آزمایشی نوع 4";
            StringPropertyCriteriaOperatorValuePair testOperatorValuePair4 = new StringPropertyCriteriaOperatorValuePair() { CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like, CriteriaValue = "test_value_4" };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri4, OperatorValuePair = testOperatorValuePair4 });
            const string testPropertyTypeUri5 = "ویژگی آزمایشی نوع 5";
            DateTimePropertyCriteriaOperatorValuePair testOperatorValuePair5 = new DateTimePropertyCriteriaOperatorValuePair() { CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals, CriteriaValue = DateTime.MinValue };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri5, OperatorValuePair = testOperatorValuePair5 });
            const string testPropertyTypeUri6 = "ویژگی آزمایشی نوع 6";
            BooleanPropertyCriteriaOperatorValuePair testOperatorValuePair6 = new BooleanPropertyCriteriaOperatorValuePair() { CriteriaOperator = BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals, CriteriaValue = false };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri6, OperatorValuePair = testOperatorValuePair6 });
            const string testPropertyTypeUri7 = "ویژگی آزمایشی نوع 7";
            var testOperatorValuePair7 = new LongPropertyCriteriaOperatorValuePair() { CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals, CriteriaValue = 1024 };
            testQuery.CriteriasSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri7, OperatorValuePair = testOperatorValuePair7 });

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 7);
            // درصورت عدم انتساب یک زوج عملکرد-مقدار برای معیار مبتنی بر ویژگی، می‌بایست یک زوج
            // از نوع خالی در نظر گرفته شود
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri1)
                    && (c as PropertyValueCriteria).OperatorValuePair is EmptyPropertyCriteriaOperatorValuePair);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri2)
                    && ((c as PropertyValueCriteria).OperatorValuePair is LongPropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair2.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair2.CriteriaValue);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri3)
                    && ((c as PropertyValueCriteria).OperatorValuePair is FloatPropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair3.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair3.CriteriaValue
                    && ((c as PropertyValueCriteria).OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).EqualityPrecision == testOperatorValuePair3.EqualityPrecision);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri4)
                    && ((c as PropertyValueCriteria).OperatorValuePair is StringPropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as StringPropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair4.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as StringPropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair4.CriteriaValue);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri5)
                    && ((c as PropertyValueCriteria).OperatorValuePair is DateTimePropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair5.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair5.CriteriaValue);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri6)
                    && ((c as PropertyValueCriteria).OperatorValuePair is BooleanPropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair6.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair6.CriteriaValue);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri7)
                    && ((c as PropertyValueCriteria).OperatorValuePair is LongPropertyCriteriaOperatorValuePair)
                    && ((c as PropertyValueCriteria).OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaOperator == testOperatorValuePair7.CriteriaOperator
                    && ((c as PropertyValueCriteria).OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaValue == testOperatorValuePair7.CriteriaValue);
        }

        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeQueryWithTwoDateRangeCriterias_DeserializeSameQuery()
        {
            // ============== Arrange ==============
            Query testQuery = new Query();
            // مقداردهی درخواست اولیه با انواع معیارها
            DateTime testStartTime1 = DateTime.MinValue;
            DateTime testEndTime1 = DateTime.MaxValue;
            testQuery.CriteriasSet.Criterias.Add(new DateRangeCriteria()
            {
                StartTime = testStartTime1.ToString(CultureInfo.InvariantCulture),
                EndTime = testEndTime1.ToString(CultureInfo.InvariantCulture)
            });
            DateTime testStartTime2 = new DateTime(2010, 01, 01, 12, 0, 0);
            DateTime testEndTime2 = new DateTime(2020, 01, 01, 12, 0, 0);
            testQuery.CriteriasSet.Criterias.Add(new DateRangeCriteria()
            {
                StartTime = testStartTime2.ToString(CultureInfo.InvariantCulture),
                EndTime = testEndTime2.ToString(CultureInfo.InvariantCulture)
            });

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 2);
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as DateRangeCriteria).StartTime.Equals(testStartTime1.ToString(CultureInfo.InvariantCulture))
                    && (c as DateRangeCriteria).EndTime.Equals(testEndTime1.ToString(CultureInfo.InvariantCulture)));
            resultQuery.CriteriasSet.Criterias.Single
                (c => (c as DateRangeCriteria).StartTime.Equals(testStartTime2.ToString(CultureInfo.InvariantCulture))
                    && (c as DateRangeCriteria).EndTime.Equals(testEndTime2.ToString(CultureInfo.InvariantCulture)));
        }

        [TestCategory("سری‌سازی درخواست جستجوی فیلتری")]
        [TestMethod()]
        public void SerializeDeserialize_SerializeQueryWithOneNastedCriteria_DeserializeSameQuery()
        {
            // ============== Arrange ==============
            Query testQuery = new Query();
            // مقداردهی درخواست اولیه با انواع معیارها
            CriteriaSet testNastedCriteriaSet = new CriteriaSet();
            const string testKeyword = "کلید واژه";
            testNastedCriteriaSet.Criterias.Add(new KeywordCriteria() { Keyword = testKeyword });
            const string testObjectType1 = "شئ آزمایشی نوع ۱";
            const string testObjectType2 = "شئ آزمایشی نوع 2";
            const string testObjectType3 = "شئ آزمایشی نوع 3";
            ObservableCollection<string> testObjectTypes = new ObservableCollection<string>();
            testObjectTypes.Add(testObjectType1);
            testObjectTypes.Add(testObjectType2);
            testObjectTypes.Add(testObjectType3);
            testNastedCriteriaSet.Criterias.Add(new ObjectTypeCriteria() { ObjectsTypeUri = testObjectTypes });
            const string testPropertyTypeUri = "نوع ویژگی آزمایشی";
            testNastedCriteriaSet.Criterias.Add(new PropertyValueCriteria() { PropertyTypeUri = testPropertyTypeUri });
            DateTime testStartTime = DateTime.MinValue;
            DateTime testEndTime = DateTime.MaxValue;
            testNastedCriteriaSet.Criterias.Add(new DateRangeCriteria()
            {
                StartTime = testStartTime.ToString(CultureInfo.InvariantCulture),
                EndTime = testEndTime.ToString(CultureInfo.InvariantCulture)
            });
            testNastedCriteriaSet.SetOperator = BooleanOperator.Any;
            testQuery.CriteriasSet.Criterias.Add(new ContainerCriteria() { CriteriaSet = testNastedCriteriaSet });

            QuerySerializer serializer = new QuerySerializer();
            MemoryStream testQueryStreamToSerailize = new MemoryStream();
            byte[] serializedDataBytesArray;
            MemoryStream serializedDataMemoryStream;
            string serializedDataString;
            Stream serializedDataStream;
            Query resultQuery;
            // ==============   Act   ==============
            // سری‌سازی درخواست خالی
            serializer.Serialize(testQueryStreamToSerailize, testQuery);
            serializedDataBytesArray = streamUtil.ReadStreamAsBytesArray(testQueryStreamToSerailize);
            serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            serializer = new QuerySerializer();
            resultQuery = serializer.Deserialize(serializedDataStream);
            // ============== Assert  ==============
            Assert.AreEqual(resultQuery.CriteriasSet.Criterias.Count, 1);
            ContainerCriteria resultNastedCriteria = (resultQuery.CriteriasSet.Criterias.First() as ContainerCriteria);
            Assert.AreEqual(resultNastedCriteria.CriteriaSet.Criterias.Count, testNastedCriteriaSet.Criterias.Count);
            resultNastedCriteria.CriteriaSet.Criterias.Single(c => c is KeywordCriteria && (c as KeywordCriteria).Keyword == testKeyword);
            resultNastedCriteria.CriteriaSet.Criterias.Single(c => c is ObjectTypeCriteria && (c as ObjectTypeCriteria).ObjectsTypeUri.Count == testObjectTypes.Count);
            resultNastedCriteria.CriteriaSet.Criterias.Single(c => c is PropertyValueCriteria && (c as PropertyValueCriteria).PropertyTypeUri.Equals(testPropertyTypeUri) && (c as PropertyValueCriteria).OperatorValuePair is EmptyPropertyCriteriaOperatorValuePair);
            resultNastedCriteria.CriteriaSet.Criterias.Single(c => c is DateRangeCriteria && (c as DateRangeCriteria).StartTime.Equals(testStartTime.ToString(CultureInfo.InvariantCulture)) && (c as DateRangeCriteria).EndTime.Equals(testEndTime.ToString(CultureInfo.InvariantCulture)));
        }
    }
}