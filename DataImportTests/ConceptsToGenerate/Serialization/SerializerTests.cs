using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.DataImport.ConceptsToGenerate.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization.Tests
{
    [TestClass()]
    public class SerializerTests
    {
        [TestMethod()]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void SerializeTest()
        {
            ImportingObject supposalObj1 = new ImportingObject("objType1", new ImportingProperty("Label","obj1"));
            supposalObj1.AddPropertyForObject(new ImportingProperty("propType1", "prop1"));
            supposalObj1.AddPropertyForObject(new ImportingProperty("propType2", "prop2"));
            ImportingObject supposalObj2 = new ImportingObject("objType2", new ImportingProperty("Label", "obj2"));
            ImportingObject supposalObj3 = new ImportingObject("objType3", new ImportingProperty("Label", "obj3"));
            ImportingRelationship suppposalRel1 = new ImportingRelationship(supposalObj1, supposalObj2, "relType1", InternalResolve.IRRelationshipDirection.SourceToTarget, DateTime.MinValue, DateTime.MaxValue, "desc1");
            ImportingRelationship suppposalRel2 = new ImportingRelationship(supposalObj2, supposalObj3, "relType2", InternalResolve.IRRelationshipDirection.TargetToSource, new DateTime(1999, 11, 23), new DateTime(2016, 1, 1, 15, 38, 59, 999), "desc1");
            long supposalDataSourceID = 15;

            MemoryStream memStream = new MemoryStream();
            Serializer s = new Serializer();
            s.Serialize
                (memStream
                , (new ImportingObject[] { supposalObj1, supposalObj2, supposalObj3 }).ToList()
                , (new ImportingRelationship[] { suppposalRel1, suppposalRel2 }).ToList()
                , supposalDataSourceID);

            memStream.Seek(0, SeekOrigin.Begin);
            Serializer s2 = new Serializer();
            Tuple<List<ImportingObject>, List<ImportingRelationship>, long> des = s2.Deserialize(memStream);

            Assert.AreEqual(des.Item1.Count, 3);
            Assert.IsTrue(des.Item1.Any(o
                => o.TypeUri.Equals(supposalObj1.TypeUri)
                && o.LabelProperty.Value.Equals(supposalObj1.LabelProperty.Value)
                && o.Properties.Count == 3
                && o.Properties.Any(p
                    => p.TypeURI.Equals(supposalObj1.Properties[0].TypeURI)
                    && p.Value.Equals(supposalObj1.Properties[0].Value))
                && o.Properties.Any(p
                    => p.TypeURI.Equals(supposalObj1.Properties[1].TypeURI)
                    && p.Value.Equals(supposalObj1.Properties[1].Value))));
            Assert.IsTrue(des.Item1.Any(o
                => o.TypeUri.Equals(supposalObj2.TypeUri)
                && o.LabelProperty.Value.Equals(supposalObj2.LabelProperty.Value)
                && o.Properties.Count == 1));
            Assert.IsTrue(des.Item1.Any(o
                => o.TypeUri.Equals(supposalObj3.TypeUri)
                && o.LabelProperty.Value.Equals(supposalObj3.LabelProperty.Value)
                && o.Properties.Count == 1));
            Assert.AreEqual(des.Item2.Count, 2);
            // TODO: Relationships' content can be check here
            Assert.AreEqual(des.Item3, supposalDataSourceID);
        }
        
        [TestMethod()]
        [TestCategory("سری سازی ساختمان داده‌ها")]
        public void Serialize_OneSimpleImportingDocument()
        {
            var supposalDoc1 = new ImportingDocument("objType1", new ImportingProperty("Label","obj1"))
            { DocumentPath = "test path" };
            long supposalDataSourceID = 15;

            MemoryStream memStream = new MemoryStream();
            Serializer s = new Serializer();
            s.Serialize
                (memStream
                , (new ImportingObject[] { supposalDoc1 }).ToList()
                , (new List<ImportingRelationship>())
                , supposalDataSourceID);

            memStream.Seek(0, SeekOrigin.Begin);
            Serializer s2 = new Serializer();
            Tuple<List<ImportingObject>, List<ImportingRelationship>, long> des = s2.Deserialize(memStream);

            Assert.AreEqual(des.Item1.Count, 1);
            Assert.AreEqual(des.Item2.Count, 0);
            Assert.IsTrue(des.Item1[0] is ImportingDocument);
            Assert.AreEqual(des.Item1[0].TypeUri, supposalDoc1.TypeUri);
            Assert.AreEqual(des.Item1[0].LabelProperty.Value, supposalDoc1.LabelProperty.Value);
            Assert.AreEqual((des.Item1[0] as ImportingDocument).DocumentPath, supposalDoc1.DocumentPath);
            Assert.AreEqual(des.Item1[0].Properties.Count, supposalDoc1.Properties.Count);
            Assert.AreEqual(des.Item3, supposalDataSourceID);
        }
    }
}