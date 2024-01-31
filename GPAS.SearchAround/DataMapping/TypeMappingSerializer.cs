using GPAS.FilterSearch;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.SearchAround.DataMapping
{
    public class TypeMappingSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(ObjectMapping),
                typeof(SourceSetObjectMapping),
                typeof(PropertyMapping),
                typeof(LinkMapping),
                typeof(OntologyTypeMappingItem),
                typeof(ConstValueMappingItem),
                typeof(SingleValueMappingItem),
                typeof(PropertyCriteriaOperatorValuePair),
                typeof(BooleanPropertyCriteriaOperatorValuePair),
                typeof(DateTimePropertyCriteriaOperatorValuePair),
                typeof(FloatPropertyCriteriaOperatorValuePair),
                typeof(LongPropertyCriteriaOperatorValuePair),
                typeof(StringPropertyCriteriaOperatorValuePair),
                typeof(BooleanPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(DateTimePropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(FloatPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(LongPropertyCriteriaOperatorValuePair.RelationalOperator),
                typeof(StringPropertyCriteriaOperatorValuePair.RelationalOperator)
            };
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, TypeMapping mappingToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException("streamWriter");
            if (mappingToSerialize == null)
                throw new ArgumentNullException("mappingToSerialize");

            XmlSerializer serializer = new XmlSerializer(typeof(TypeMapping), GetPreipheralTypes());
            serializer.Serialize(streamWriter, mappingToSerialize);
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, TypeMapping mappingToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (mappingToSerialize == null)
                throw new ArgumentNullException("mappingToSerialize");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, mappingToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را براساس اطلاعات سری شده برمی‌گرداند
        /// </summary>
        public TypeMapping Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            XmlReader xr = XmlReader.Create(streamReader);
            XmlSerializer xs = new XmlSerializer(typeof(TypeMapping), GetPreipheralTypes());
            TypeMapping typeMapping = (TypeMapping)xs.Deserialize(xr);
            return typeMapping;
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public TypeMapping DeserializeFromFile(string filePath)
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
