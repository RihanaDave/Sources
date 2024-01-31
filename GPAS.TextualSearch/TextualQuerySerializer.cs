using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.TextualSearch
{
    public class TextualQuerySerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(BaseSearchCriteria),
                typeof(StringBasedCriteria),
                typeof(DoubleBasedCriteria),
                typeof(DoubleRangeBasedCriteria),
                typeof(DateBaseCriteria),
                typeof(DateRangeBasedCriteria)
            };
        }
        public void Serialize(Stream streamWriter, TextualSearchQuery queryToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (queryToSerialize == null)
                throw new ArgumentNullException(nameof(queryToSerialize));

            //FileStream file = new FileStream("D:/test.xml", FileMode.Create, FileAccess.Write);
            //streamWriter.CopyTo(file);
            //file.Close();
            //streamWriter.Close();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(streamWriter, Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(TextualSearchQuery), GetPreipheralTypes());
            serializer.Serialize(xmlTextWriter, queryToSerialize);
        }
        public void SerializeToFile(string filePath, TextualSearchQuery queryToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (queryToSerialize == null)
                throw new ArgumentNullException("queryToSerialize");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, queryToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        public TextualSearchQuery Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            MemoryStream serializedPrimaryQueryStream = new MemoryStream();
            using (MemoryStream resultQueryStream = new MemoryStream(serializedPrimaryQueryStream.ToArray()))
            {
                using (StreamReader xmlStreamReader = new StreamReader(resultQueryStream, Encoding.UTF8))
                {
                    XmlReader xr = XmlReader.Create(streamReader);
                    XmlSerializer xs = new XmlSerializer(typeof(TextualSearchQuery), GetPreipheralTypes());
                    return (TextualSearchQuery)xs.Deserialize(xr);
                }
            }
        }

        /// <summary>
        /// یک نمونه از کلاس «درخواست جستجوی فیلتری» را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public TextualSearchQuery DeserializeFromFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            FileStream fs = new FileStream(filePath, FileMode.Open);
            try
            {
                return Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
