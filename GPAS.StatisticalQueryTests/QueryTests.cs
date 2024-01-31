using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace GPAS.StatisticalQuery.Tests
{
    [TestClass()]
    public class QueryTests
    {
        [TestMethod()]
        public void Serialize_DeSerlizeTest()
        {
            //Assign
            List<TypeBasedDrillDownPortionBase> ofObjectTypes = new List<TypeBasedDrillDownPortionBase>() { new OfObjectType() { ObjectTypeUri = "شخص" } };
            Query query = new Query();
            query.FormulaSequence.Add(new TypeBasedDrillDown());
            (query.FormulaSequence[0] as TypeBasedDrillDown).Portions = ofObjectTypes;
            //Act
            MemoryStream streamWriter = new MemoryStream();
            QuerySerializer querySerializer = new QuerySerializer();
            querySerializer.Serialize(query, streamWriter);
            StreamUtility streamUtil = new StreamUtility();
            byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);

            querySerializer = new QuerySerializer();
            Query result = querySerializer.Deserialize(streamWriter);
            //Assert
            for (int i = 0; i < query.FormulaSequence.Count; i++)
            {
                Assert.AreEqual(query.FormulaSequence[i].ToString(), result.FormulaSequence[i].ToString());
            }
        }
    }
}