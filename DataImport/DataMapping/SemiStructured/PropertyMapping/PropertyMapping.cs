using GPAS.Ontology;
using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    /// <summary>
    /// کلاس «نگاشت یک ویژگی از شئ» براساس داده‌های نیم‌ساختیافته؛
    /// </summary>
    [Serializable]
    public class PropertyMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected PropertyMapping()
        {

        }
        public PropertyMapping(OntologyTypeMappingItem propertyType, ValueMappingItem value)
        {
            PropertyType = propertyType;
            Value = value;
        }
        public OntologyTypeMappingItem PropertyType
        {
            get;
            set;
        }

        public ValueMappingItem Value
        {
            get;
            set;
        }

        public bool IsSetAsDisplayName
        {
            get;
            set;
        }

        public BaseDataTypes DataType 
        {
            get;
            set; 
        }
    }
}
