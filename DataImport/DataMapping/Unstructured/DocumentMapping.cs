using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.Unstructured
{
    [Serializable]
    public class DocumentMapping : ObjectMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected DocumentMapping()
        { }

        public DocumentMapping(string objectType, string mappingTitle, string documentPathMapping)
            : base(objectType, mappingTitle)
        {
            DocumentPathMapping = documentPathMapping;
        }

        public string DocumentPathMapping { get; set; }

    }
}
