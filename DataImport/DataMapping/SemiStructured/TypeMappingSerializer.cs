using GPAS.DataImport.Material.SemiStructured;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    public class TypeMappingSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(ObjectMapping),
                typeof(DocumentMapping),
                typeof(DocumentPathOptions),
                typeof(PropertyMapping),
                typeof(GeoTimePropertyMapping),
                typeof(RelationshipMapping),
                typeof(OntologyTypeMappingItem),
                typeof(TableColumnMappingItem),
                typeof(ConstValueMappingItem),
                typeof(PathPartMappingItem),
                typeof(PathPartDirectionMappingItem),
                typeof(PathPartTypeMappingItem),
                typeof(SingleValueMappingItem),
                typeof(MultiValueMappingItem),
                typeof(GeoTimeValueMappingItem),
                typeof(DateTimeTableColumnMappingItem),
                typeof(AccessTable)
            };
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, TypeMapping mappingToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (mappingToSerialize == null)
                throw new ArgumentNullException(nameof(mappingToSerialize));

            XmlSerializer serializer = new XmlSerializer(typeof(TypeMapping), GetPreipheralTypes());
            serializer.Serialize(streamWriter, mappingToSerialize);
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, TypeMapping mappingToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (mappingToSerialize == null)
                throw new ArgumentNullException(nameof(mappingToSerialize));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

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
                throw new ArgumentNullException(nameof(streamReader));

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
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            FileStream fs = new FileStream(filePath, FileMode.Open);

            try
            {
                return Deserialize(fs);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
