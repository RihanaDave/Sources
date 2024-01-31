using GPAS.FilterSearch;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.SearchAround
{
    public class CustomSearchAroundCriteriaSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(SearchAroundStep),
                typeof(Guid),
                typeof(PropertyValueCriteria),
                typeof(PropertyCriteriaOperatorValuePair),
                typeof(FloatPropertyCriteriaOperatorValuePair),
                typeof(FloatPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(StringPropertyCriteriaOperatorValuePair),
                typeof(StringPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(DateTimePropertyCriteriaOperatorValuePair),
                typeof(DateTimePropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(BooleanPropertyCriteriaOperatorValuePair),
                typeof(BooleanPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(LongPropertyCriteriaOperatorValuePair),
                typeof(LongPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(GeoPointPropertyCriteriaOperatorValuePair),
                typeof(GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(EmptyPropertyCriteriaOperatorValuePair),
            };
        }

        public void Serialize(Stream streamWriter, CustomSearchAroundCriteria queryToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException("streamWriter");
            if (queryToSerialize == null)
                throw new ArgumentNullException("queryToSerialize");
            XmlTextWriter xmlTextWriter = new XmlTextWriter(streamWriter, Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(CustomSearchAroundCriteria), GetPreipheralTypes());
            serializer.Serialize(xmlTextWriter, queryToSerialize);
        }

        public void SerializeToFile(string filePath, CustomSearchAroundCriteria queryToSerialize)
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

        public CustomSearchAroundCriteria Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            MemoryStream serializedPrimaryQueryStream = new MemoryStream();
            using (MemoryStream resultQueryStream = new MemoryStream(serializedPrimaryQueryStream.ToArray()))
            {
                using (StreamReader xmlStreamReader = new StreamReader(resultQueryStream, Encoding.UTF8))
                {
                    XmlReader xr = XmlReader.Create(streamReader);
                    XmlSerializer xs = new XmlSerializer(typeof(CustomSearchAroundCriteria), GetPreipheralTypes());
                    return (CustomSearchAroundCriteria)xs.Deserialize(xr);
                }
            }
        }
    }
}
