using GPAS.AccessControl;
using GPAS.DataImport.DataMapping;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using System;
using System.IO;
using System.Xml.Serialization;

namespace GPAS.JobsManagement
{
    public class RequestSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(ObjectMapping),
                typeof(DocumentMapping),
                typeof(DocumentPathOptions),
                typeof(PropertyMapping),
                typeof(RelationshipMapping),
                typeof(OntologyTypeMappingItem),
                typeof(TableColumnMappingItem),
                typeof(ConstValueMappingItem),
                typeof(PathPartMappingItem),
                typeof(PathPartDirectionMappingItem),
                typeof(PathPartTypeMappingItem),
                typeof(MultiValueMappingItem),
                typeof(SingleValueMappingItem),
                typeof(GeoTimeValueMappingItem),
                typeof(DateTimeTableColumnMappingItem),
                typeof(SemiStructuredDataImportRequest),
                typeof(DataImportRequest),
                typeof(MaterialBase),
                typeof(CsvFileMaterial),
                typeof(AttachedDatabaseTableMaterial),
                typeof(DataLakeSearchResultMaterial),
                typeof(ExcelSheet),
                typeof(AccessTable),
                typeof(EmlDirectory),
                typeof(ACL),
                typeof(ACI),
                typeof(Permission),
                typeof(GeoTimePropertyMapping)
            };
        }

        /// <summary>
        /// یک نمونه از کلاس درخواست را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, Request requestToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (requestToSerialize == null)
                throw new ArgumentNullException(nameof(requestToSerialize));
            
            XmlSerializer serializer = new XmlSerializer(typeof(Request), GetPreipheralTypes());
            serializer.Serialize(streamWriter, requestToSerialize);
        }
        /// <summary>
        /// یک نمونه از کلاس درخواست را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, Request requestToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (requestToSerialize == null)
                throw new ArgumentNullException("mappingToSerialize");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, requestToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// یک نمونه از کلاس درخواست را براساس اطلاعات سری شده برمی‌گرداند
        /// </summary>
        public Request Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            XmlSerializer xs = new XmlSerializer(typeof(Request), GetPreipheralTypes());
            return (Request)xs.Deserialize(streamReader);
        }
        /// <summary>
        /// یک نمونه از کلاس درخواست را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public Request DeserializeFromFile(string filePath)
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
