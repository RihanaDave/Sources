using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    /// <summary>
    /// کلاس «نگاشت یک شئ» براساس داده‌های نیم‌ساختیافته
    /// </summary>
    [Serializable]
    public class ObjectMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected ObjectMapping()
        {

        }
        public ObjectMapping(OntologyTypeMappingItem objectType, string mappingTitle)
        {
            if (objectType == null)
                throw new ArgumentNullException("objectType");

            ID = Guid.NewGuid().ToString();
            ProcessHashCode();
            ObjectType = objectType;
            MappingTitle = new ConstValueMappingItem(mappingTitle);
        }

        public ConstValueMappingItem MappingTitle
        {
            get;
            set;
        }

        public OntologyTypeMappingItem ObjectType
        {
            get;
            set;
        }

        public virtual ValueMappingItem GetDisplayName()
        {
            foreach (var property in Properties)
            {
                if (property.IsSetAsDisplayName)
                    return property.Value;
            }
            return MappingTitle;
        }

        public virtual void SetDisplayName(PropertyMapping propertyToSetAsDisplayName)
        {
            if (!Properties.Contains(propertyToSetAsDisplayName))
                throw new InvalidOperationException("To set property as display-name, the property may be added to properties of object mapping");

            foreach (var property in Properties)
            {
                if (property == propertyToSetAsDisplayName)
                    property.IsSetAsDisplayName = true;
                else
                    property.IsSetAsDisplayName = false;
            }
        }

        public string ID
        {
            get;
            set;
        }

        /// <summary>
        /// ویژگی‌های شئ برای نگاشت
        /// </summary>
        public List<PropertyMapping> Properties = new List<PropertyMapping>();
        /// <summary>
        /// افزودن نگاشت یکی از ویژگی‌های شئ
        /// </summary>
        public void AddProperty(PropertyMapping propertyToAdd)
        {
            if (propertyToAdd == null)
                throw new ArgumentNullException("propertyToAdd");

            Properties.Add(propertyToAdd);
        }
        /// <summary>
        /// حذف یکی از نگاشت‌های ویژگی‌های تعریف شده برای شئ در صورت وجود بین ویژگی‌ها
        /// </summary>
        public void RemoveProperty(PropertyMapping propertyToRemove)
        {
            if (propertyToRemove == null)
                throw new ArgumentNullException("propertyToRemove");

            if (Properties.Contains(propertyToRemove))
                Properties.Remove(propertyToRemove);
        }

        private void ProcessHashCode()
        {
            hashCode = ID.GetHashCode();
        }
        private int hashCode = 0;
        public override int GetHashCode()
        {
            if (hashCode == 0)
                ProcessHashCode();
            return hashCode;
        }

        public bool IsFullyConstMapping()
        {
            if (this is DocumentMapping
            && !(this as DocumentMapping).DocumentPathMapping.IsFullyConstMapping())
            {
                return false;
            }
            else if(Properties.Count == 0)
            {
                return false;
            }
            else if (Properties.Any(p => !p.Value.IsFullyConstMapping()))
            {
                return false;
            }
            else if (!GetDisplayName().IsFullyConstMapping())
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