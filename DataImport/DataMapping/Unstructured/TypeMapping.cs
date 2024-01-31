using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.Unstructured
{
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
            int last = RelationshipsMapping.Count();
            for (int i = last; i >= 1; i--)
                if (RelationshipsMapping.ElementAt(i - 1).SourceId == objectMappingToRemoveRelatedRelationships.ID || RelationshipsMapping.ElementAt(i - 1).TargetId == objectMappingToRemoveRelatedRelationships.ID)
                    RemoveRelationshipMapping(RelationshipsMapping.ElementAt(i - 1));
        }

        /// <summary>
        /// نگاشت‌های تعریف شده برای ورود هر یک از ردیف‌های داده‌ی غیرساختیافته
        /// </summary>
        public List<ObjectMapping> ObjectsMapping = new List<ObjectMapping>();

        internal TypeMapping Copy(string unstructuredFileTitle)
        {
            StreamUtility utilities = new StreamUtility();
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, this);
            byte[] serializedDataBytesArray = utilities.ReadStreamAsBytesArray(memoryStream);
            MemoryStream serializedDataMemoryStream = new MemoryStream(serializedDataBytesArray);
            string serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            Stream serializedDataStream = utilities.GenerateStreamFromString(serializedDataString);

            TypeMapping resultMapping = serializer.Deserialize(serializedDataStream, unstructuredFileTitle);

            return resultMapping;
        }

        /// <summary>
        /// افزودن یک نگاشت جدید
        /// </summary>
        public void AddRelationshipMapping(RelationshipMapping relationshipMappingToAdd)
        {
            RelationshipsMapping.Add(relationshipMappingToAdd);
        }
        // TODO: Clean!
        public void RemoveRelationshipMapping(RelationshipMapping relationshipMappingToRemove)
        {
            if (relationshipMappingToRemove == null)
                throw new ArgumentNullException("relationshipMappingToRemove");

            if (RelationshipsMapping.Contains(relationshipMappingToRemove))
                RelationshipsMapping.Remove(relationshipMappingToRemove);
            else
                throw new ArgumentException("Mapping not exist", "relationshipMappingToRemove");
        }
        // TODO: Clean!
        public List<RelationshipMapping> RelationshipsMapping = new List<RelationshipMapping>();
        public bool InterTypeAutoInternalResolution { get; set; }        

        public Dictionary<string, List<ObjectMapping>> GetObjectTypeMappingsByTypeUri()
        {
            var result = new Dictionary<string, List<ObjectMapping>>();
            foreach (var mapping in ObjectsMapping)
            {
                if (!result.ContainsKey(mapping.ObjectType))
                {
                    var newMappingList = new List<ObjectMapping>();
                    newMappingList.Add(mapping);
                    result.Add(mapping.ObjectType, newMappingList);
                }
                else
                {
                    result[mapping.ObjectType].Add(mapping);
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
            else
            {
                return true;
            }
            
        }
    }
}
