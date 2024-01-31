using GPAS.PropertiesValidation.Geo.Formats;
using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class GeoTimePropertyMapping : PropertyMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected GeoTimePropertyMapping()
        { }

        public GeoTimePropertyMapping(OntologyTypeMappingItem propertyType, GeoTimeValueMappingItem value)
            : base(propertyType, value)
        {
        }
        public GeoSpecialTypes GeoSpecialFormat
        {
            get;
            set;
        }


    }
}
