using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    public class Serializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[]
            {
                typeof(ImportingObject),
                typeof(ImportingDocument),
                typeof(ImportingProperty),
                typeof(ImportingRelationship),
                typeof(ImportingRelationshipDirection),
                typeof(SerializeRelationship),
                typeof(ACL),
                typeof(ACI),
                typeof(Permission), 
                typeof(KObject),
                typeof(MappingEntity)
            };
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری می‌کند
        /// </summary>
        public void Serialize(Stream streamWriter, List<ImportingObject> importingObjects, List<ImportingRelationship> importingRelationships, long dataSourceID)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (importingObjects == null)
                throw new ArgumentNullException(nameof(importingObjects));
            if (importingRelationships == null)
                throw new ArgumentNullException(nameof(importingRelationships));
            if (dataSourceID < 1)
                throw new ArgumentOutOfRangeException(nameof(dataSourceID));

            var importingConcepts = new SerializeConcepts
                (importingObjects, importingRelationships, dataSourceID);

            var attribute = new XmlAttributeOverrides();
            attribute.Add(typeof(ImportingRelationship), "Source", new XmlAttributes() { XmlIgnore = true });
            attribute.Add(typeof(ImportingRelationship), "Target", new XmlAttributes() { XmlIgnore = true });
            var rootAttribute = new XmlRootAttribute();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializeConcepts), attribute, GetPreipheralTypes(), rootAttribute, "GPAS.DataImport.ConceptsToGenerate");
            serializer.Serialize(streamWriter, importingConcepts);
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را سری و در فایل ذخیره می‌کند
        /// </summary>
        public void SerializeToFile(string filePath, List<ImportingObject> importingObjects, List<ImportingRelationship> importingRelationships, long dataSourceID)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (importingObjects == null)
                throw new ArgumentNullException(nameof(importingObjects));
            if (importingRelationships == null)
                throw new ArgumentNullException(nameof(importingRelationships));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            StreamWriter sw = new StreamWriter(filePath/*, false, Encoding.UTF8*/);
            try
            {
                Serialize(sw.BaseStream, importingObjects, importingRelationships, dataSourceID);
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را براساس اطلاعات سری شده برمی‌گرداند
        /// </summary>
        public Tuple<List<ImportingObject>, List<ImportingRelationship>, long> Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            XmlReader xr = XmlReader.Create(streamReader);
            var attribute = new XmlAttributeOverrides();
            attribute.Add(typeof(ImportingRelationship), "Source", new XmlAttributes() { XmlIgnore = true });
            attribute.Add(typeof(ImportingRelationship), "Target", new XmlAttributes() { XmlIgnore = true });
            var rootAttribute = new XmlRootAttribute();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializeConcepts), attribute, GetPreipheralTypes(), rootAttribute, "GPAS.DataImport.ConceptsToGenerate");
            var importingConcepts = (SerializeConcepts)serializer.Deserialize(xr);
            return SerializeConcepts.GetImportingConceptsFromDeserializedInstance(importingConcepts);
        }
        /// <summary>
        /// یک نمونه از کلاس نگاشت داده نیم‌ساختیافته را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public Tuple<List<ImportingObject>, List<ImportingRelationship>, long> DeserializeFromFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            StreamReader sr = new StreamReader(filePath/*, Encoding.UTF8*/);
            //FileStream fs = new FileStream(filePath, FileMode.Open,);
            try
            {
                return Deserialize(sr.BaseStream);
            }
            finally
            {
                sr.Close();
            }
        }




        //////////// Pause and stop ////////////////////////////
        ///
        public void SerializeMappingDictionaryToFile(string filePath, Dictionary<ImportingObject, KObject> mapping)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            SerializeMappings serializeMappings = new SerializeMappings(mapping);

            StreamWriter sw = new StreamWriter(filePath/*, false, Encoding.UTF8*/);
            try
            {
                Serialize(sw.BaseStream, serializeMappings);
            }
            finally
            {
                sw.Close();
            }
        }

        public Dictionary<ImportingObject, KObject> DeserializeMappingFromFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", nameof(filePath));

            StreamReader sr = new StreamReader(filePath/*, Encoding.UTF8*/);
            try
            {
                return DeserializeMapping(sr.BaseStream);
            }
            finally
            {
                sr.Close();
            }
        }


        public void Serialize(Stream streamWriter, SerializeMappings serializeMappings)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));
            if (serializeMappings == null)
                throw new ArgumentNullException(nameof(serializeMappings));

            var attribute = new XmlAttributeOverrides();
            attribute.Add(typeof(ImportingRelationship), "Source", new XmlAttributes() { XmlIgnore = true });
            attribute.Add(typeof(ImportingRelationship), "Target", new XmlAttributes() { XmlIgnore = true });
            var rootAttribute = new XmlRootAttribute();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializeMappings), attribute, GetPreipheralTypes(), rootAttribute, "GPAS.DataImport.ConceptsToGenerate");
            serializer.Serialize(streamWriter, serializeMappings);
        }

        public Dictionary<ImportingObject, KObject> DeserializeMapping(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            XmlReader xr = XmlReader.Create(streamReader);
            var attribute = new XmlAttributeOverrides();
            attribute.Add(typeof(ImportingRelationship), "Source", new XmlAttributes() { XmlIgnore = true });
            attribute.Add(typeof(ImportingRelationship), "Target", new XmlAttributes() { XmlIgnore = true });
            var rootAttribute = new XmlRootAttribute();
            XmlSerializer serializer = new XmlSerializer(typeof(SerializeMappings), attribute, GetPreipheralTypes(), rootAttribute, "GPAS.DataImport.ConceptsToGenerate");
            var mappings = (SerializeMappings)serializer.Deserialize(xr);
            return SerializeMappings.GetMappingDictionaryFromSerializeMappings(mappings);
        }
    }
}
