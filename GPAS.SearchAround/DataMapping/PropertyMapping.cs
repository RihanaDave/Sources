using GPAS.FilterSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround.DataMapping
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

        public PropertyCriteriaOperatorValuePair Comparator
        {
            get;
            set;
        }
    }
}
