using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.Ontology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.Unstructured
{
    [Serializable]
    public class PropertyMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected PropertyMapping()
        {

        }
        public PropertyMapping(string propertyType, ConstValueMappingItem value)
        {
            PropertyType = propertyType;
            Value = value;
        }
        public string PropertyType
        {
            get;
            set;
        }

        public ConstValueMappingItem Value
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
