using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GPAS.AccessControl.Tests
{
    [TestClass()]
    public class ACLTests
    {
        [TestMethod()]
        public void SerializeTest()
        {
            ACL baseAcl = new ACL();
            baseAcl.Permissions = new List<ACI>()
            {
                new ACI() { GroupName = "g1", AccessLevel = Permission.Write }
            };
            baseAcl.Classification = "test1";
            ACLSerializer serializer = new ACLSerializer();
            MemoryStream memStream = new MemoryStream();

            serializer.Serialize(memStream, baseAcl);
            
            memStream.Seek(0, SeekOrigin.Begin);
            serializer = new ACLSerializer();
            ACL deserializedACL = serializer.Deserialize(memStream);

            Assert.IsNotNull(deserializedACL);
            Assert.IsNotNull(deserializedACL.Permissions);
            Assert.IsNotNull(deserializedACL.Permissions[0]);
            Assert.AreEqual(deserializedACL.Permissions.Count, baseAcl.Permissions.Count);
            Assert.AreEqual(deserializedACL.Permissions[0].GroupName, baseAcl.Permissions[0].GroupName);
            Assert.AreEqual(deserializedACL.Permissions[0].AccessLevel, baseAcl.Permissions[0].AccessLevel);
            Assert.AreEqual(deserializedACL.Classification, baseAcl.Classification);
        }
    }
}