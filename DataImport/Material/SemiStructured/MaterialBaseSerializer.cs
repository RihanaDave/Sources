using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.DataImport.Material.SemiStructured
{
    public class MaterialBaseSerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(CsvFileMaterial),
                typeof(EmlDirectory),
                typeof(AttachedDatabaseTableMaterial),
                typeof(DataLakeSearchResultMaterial),
                typeof(ExcelSheet),
                typeof(AccessTable)
            };
        }

        /// <summary>
        /// یک نمونه از کلاس مواد خام داده‌ی نیم‌ساختیافته را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, MaterialBase materialToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (materialToSerialize == null)
                throw new ArgumentNullException(nameof(materialToSerialize));

            XmlSerializer serializer = new XmlSerializer(typeof(MaterialBase), GetPreipheralTypes());
            serializer.Serialize(streamWriter, materialToSerialize);
        }
        /// <summary>
        /// یک نمونه از کلاس مواد خام داده‌ی نیم‌ساختیافته را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, MaterialBase materialToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (materialToSerialize == null)
                throw new ArgumentNullException(nameof(materialToSerialize));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, materialToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// یک نمونه از کلاس مواد خام داده‌ی نیم‌ساختیافته را براساس اطلاعات سری شده برمی‌گرداند
        /// </summary>
        public MaterialBase Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            XmlReader xr = XmlReader.Create(streamReader);
            XmlSerializer xs = new XmlSerializer(typeof(MaterialBase), GetPreipheralTypes());
            var materialBase = (MaterialBase)xs.Deserialize(xr);
            return materialBase;
        }
        /// <summary>
        /// یک نمونه از کلاس مواد خام داده‌ی نیم‌ساختیافته را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public MaterialBase DeserializeFromFile(string filePath)
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
            finally
            {
                fs.Close();
            }
        }
    }
}
