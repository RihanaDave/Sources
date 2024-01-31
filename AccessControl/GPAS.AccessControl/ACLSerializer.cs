using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.AccessControl
{
    public class ACLSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(ACI),
                typeof(Permission)
            };
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, ACL aclToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (aclToSerialize == null)
                throw new ArgumentNullException(nameof(aclToSerialize));

            XmlSerializer serializer = new XmlSerializer(typeof(ACL), GetPreipheralTypes());
            serializer.Serialize(streamWriter, aclToSerialize);
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, ACL aclToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (aclToSerialize == null)
                throw new ArgumentNullException(nameof(aclToSerialize));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, aclToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را براساس اطلاعات سری شده برمی‌گرداند
        /// </summary>
        public ACL Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            XmlReader xr = XmlReader.Create(streamReader);
            XmlSerializer xs = new XmlSerializer(typeof(ACL), GetPreipheralTypes());
            ACL acl = (ACL)xs.Deserialize(xr);
            return acl;
        }
    }
}
