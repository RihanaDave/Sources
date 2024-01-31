using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.SearchAround.DataMapping
{
    /// <summary>
    /// کلاس نگاشت ورود از داده‌های نیم‌ساختیافته؛
    /// از این کلاس برای ارائه‌ی تعاریف نگاشت هر یک از ردیف‌های جدول داده‌های نیم‌ساختیافته استفاده می‌شود.
    /// </summary>
    [Serializable]
    public class TypeMapping
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public TypeMapping()
        { }
        /// <summary>
        /// افزودن یک نگاشت جدید
        /// </summary>
        public void AddObjectMapping(ObjectMapping objectMappingToAdd)
        {
            ObjectsMapping.Add(objectMappingToAdd);
        }
        // TODO: Clean!
        public void RemoveObjectMapping(ObjectMapping objectMappingToRemove)
        {
            if (objectMappingToRemove == null)
                throw new ArgumentNullException("objectMappingToRemove");

            if (ObjectsMapping.Contains(objectMappingToRemove))
            {
                RemoveAllRelationshipsRelatedWith(objectMappingToRemove);
                ObjectsMapping.Remove(objectMappingToRemove);
            }
            else
                throw new ArgumentException("Mapping not exist", "objectMappingToRemove");
        }

        /// <summary>
        /// تمام روابط نگاشت شده ی مرتبط با یک نگاشت شی، از روی مجموعه نگاشت‌ها حذف می کند
        /// </summary>
        protected void RemoveAllRelationshipsRelatedWith(ObjectMapping objectMappingToRemoveRelatedRelationships)
        {
            if (objectMappingToRemoveRelatedRelationships == null)
                throw new ArgumentNullException("objectMappingToRemoveRelatedRelationships");

            // حذف روابط نگاشت شده مرتبط با نگاشت شی مورد نظر (مبدا یا مقصد رابطه این شی باشد) از نگاشت
            int last = LinksMapping.Count();
            for (int i = last; i >= 1; i--)
                if (LinksMapping.ElementAt(i - 1).Source.ID == objectMappingToRemoveRelatedRelationships.ID || LinksMapping.ElementAt(i - 1).Target.ID == objectMappingToRemoveRelatedRelationships.ID)
                    RemoveLinkMapping(LinksMapping.ElementAt(i - 1));
        }

        /// <summary>
        /// نگاشت‌های تعریف شده برای ورود هر یک از ردیف‌های داده‌ی غیرساختیافته
        /// </summary>
        public List<ObjectMapping> ObjectsMapping = new List<ObjectMapping>();

        public TypeMapping Copy()
        {
            StreamUtility utilities = new StreamUtility();
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, this);
            byte[] serializedDataBytesArray = utilities.ReadStreamAsBytesArray(memoryStream);
            MemoryStream serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            string serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            Stream serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);

            TypeMapping resultMapping = serializer.Deserialize(serializedDataStream);

            return resultMapping;
        }

        /// <summary>
        /// افزودن یک نگاشت جدید
        /// </summary>
        public void AddLinkMapping(LinkMapping linkMappingToAdd)
        {
            LinksMapping.Add(linkMappingToAdd);
        }
        // TODO: Clean!
        public void RemoveLinkMapping(LinkMapping linkMappingToRemove)
        {
            if (linkMappingToRemove == null)
                throw new ArgumentNullException(nameof(linkMappingToRemove));

            if (LinksMapping.Contains(linkMappingToRemove))
                LinksMapping.Remove(linkMappingToRemove);
            else
                throw new ArgumentException("Mapping not exist", nameof(linkMappingToRemove));
        }
        // TODO: Clean!
        public List<LinkMapping> LinksMapping = new List<LinkMapping>();


        public Dictionary<string, List<ObjectMapping>> GetObjectTypeMappingsByTypeUri()
        {
            var result = new Dictionary<string, List<ObjectMapping>>();
            foreach (var mapping in ObjectsMapping)
            {
                if (!result.ContainsKey(mapping.ObjectType.TypeUri))
                {
                    var newMappingList = new List<ObjectMapping>();
                    newMappingList.Add(mapping);
                    result.Add(mapping.ObjectType.TypeUri, newMappingList);
                }
                else
                {
                    result[mapping.ObjectType.TypeUri].Add(mapping);
                }
            }

            return result;
        }

        public ObjectMapping GetSourceSetObject()
        {
            SourceSetObjectMapping result = null;
            foreach (var currentObject in ObjectsMapping)
            {
                if (currentObject is SourceSetObjectMapping)
                {
                    result = currentObject as SourceSetObjectMapping;
                    break;
                }
            }
            return result;
        }
        public bool IsMappingValid()
        {
            if (ObjectsMapping.Count == 0)
            {
                return false;
            }            
            return true;
        }
    }
}
