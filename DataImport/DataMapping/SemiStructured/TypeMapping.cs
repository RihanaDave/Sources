using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPAS.DataImport.DataMapping.SemiStructured
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
            int last = RelationshipsMapping.Count();
            for (int i = last; i >= 1; i--)
                if (RelationshipsMapping.ElementAt(i - 1).SourceId == objectMappingToRemoveRelatedRelationships.ID || RelationshipsMapping.ElementAt(i - 1).TargetId == objectMappingToRemoveRelatedRelationships.ID)
                    RemoveRelationshipMapping(RelationshipsMapping.ElementAt(i - 1));
        }

        /// <summary>
        /// نگاشت‌های تعریف شده برای ورود هر یک از ردیف‌های داده‌ی غیرساختیافته
        /// </summary>
        public List<ObjectMapping> ObjectsMapping = new List<ObjectMapping>();

        internal TypeMapping Copy()
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

        public bool IsMappingAutoResolvable()
        {
            for (int i = 0; i < ObjectsMapping.Count; i++)
            {
                for (int j = i + 1; j < ObjectsMapping.Count; j++)
                {
                    if (ObjectsMapping[i].ObjectType.TypeUri == ObjectsMapping[j].ObjectType.TypeUri
                        && (IsAtLeastOneOfPropertiesForObjectMappingFindMatch(ObjectsMapping[i])
                            && IsAtLeastOneOfPropertiesForObjectMappingFindMatch(ObjectsMapping[j])))
                        return true;
                }
            }
            return false;
        }

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

        public static string GetFriendlyNameForInternalResolutionOption(PropertyInternalResolutionOption option)
        {
            switch (option)
            {
                case PropertyInternalResolutionOption.FindMatch:
                    return Properties.Resources.InternalResolutionOption_FindMatch_Description;
                case PropertyInternalResolutionOption.MustMatch:
                    return Properties.Resources.InternalResolutionOption_MustMatch_Description;
                case PropertyInternalResolutionOption.Ignorable:
                    return Properties.Resources.InternalResolutionOption_Ignorable_Description;
                default:
                    return string.Empty;
            }
        }

        public bool IsMappingValid()
        {
            if (ObjectsMapping.Count == 0)
            {
                return false;
            }
            foreach (var objectMapping in ObjectsMapping)
            {
                if (!IsDisplayNameDefinedForObjectMapping(objectMapping))
                {
                    return false;
                }
                if (!(objectMapping is DocumentMapping)
                    && !IsObjectMappingContainsFMPropertyMapping(objectMapping)
                    && !IsAllPropertiesForObjectMappingIgnorable(objectMapping))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsDisplayNameDefinedForObjectMapping(ObjectMapping objectMapping)
        {
            foreach (var propertyMapping in objectMapping.Properties)
            {
                if (propertyMapping.IsSetAsDisplayName)
                {
                    return true;
                }
            }
            if (objectMapping is DocumentMapping
                && (objectMapping as DocumentMapping).IsDocumentNameAsDisplayName)
            {
                return true;
            }
            return false;
        }

        private static bool IsAllPropertiesForObjectMappingIgnorable(ObjectMapping objectMapping)
        {
            if (objectMapping.Properties.Count == 0)
                return false;

            bool result = true;
            foreach (var propertyMapping in objectMapping.Properties)
            {
                if (propertyMapping.Value is TableColumnMappingItem)
                    if ((propertyMapping.Value as TableColumnMappingItem).ResolutionOption
                        != PropertyInternalResolutionOption.Ignorable)
                    {
                        result = false;
                        break;
                    }
            }
            return result;
        }

        private bool IsAtLeastOneOfPropertiesForObjectMappingFindMatch(ObjectMapping objectMapping)
        {
            if (objectMapping.Properties.Count == 0)
                return false;

            bool result = false;
            foreach (var propertyMapping in objectMapping.Properties)
            {
                if (propertyMapping.Value is TableColumnMappingItem)
                    if ((propertyMapping.Value as TableColumnMappingItem).ResolutionOption
                        == PropertyInternalResolutionOption.FindMatch)
                    {
                        result = true;
                        break;
                    }
            }
            return result;
        }

        private static bool IsObjectMappingContainsFMPropertyMapping(ObjectMapping objectMapping)
        {
            foreach (var propertyMapping in objectMapping.Properties)
            {
                if (propertyMapping.Value is TableColumnMappingItem
                    && (propertyMapping.Value as TableColumnMappingItem).ResolutionOption
                        == PropertyInternalResolutionOption.FindMatch)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AreInternalResolutionOptionsSetValidForAnObjectMapping(IEnumerable<PropertyInternalResolutionOption> options)
        {
            if (options
                .Where(o => o == PropertyInternalResolutionOption.FindMatch)
                .Count() > 0)
                return true;
            if (options
                .Where(o => o == PropertyInternalResolutionOption.Ignorable)
                .Count() == options.Count())
                return true;
            return false;
        }

        public static TypeMapping SerializeFile(string fileName)
        {
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            return serializer.DeserializeFromFile(fileName);
        }

        public MemoryStream ToStreamXml()
        {
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            MemoryStream memoryStream = new MemoryStream();
            serializer.Serialize(memoryStream, this);
            return memoryStream;
        }

        public string ToStringXml()
        {
            StreamUtility utilities = new StreamUtility();
            MemoryStream memoryStream = ToStreamXml();
            byte[] serializedDataBytesArray = utilities.ReadStreamAsBytesArray(memoryStream);
            string serializedDataString = utilities.ByteArrayToStringUtf8(serializedDataBytesArray);
            return serializedDataString;
        }
    }
}