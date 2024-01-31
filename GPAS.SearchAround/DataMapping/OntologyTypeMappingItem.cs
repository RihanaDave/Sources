using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround.DataMapping
{
    /// <summary>
    /// کلاس «اقلام نگاشتی» که نوعی در هستان‌شناسی را نشان می‌دهند، براساس داده‌های نیم‌ساختیافته
    /// </summary>
    [Serializable]
    public class OntologyTypeMappingItem : MappingNodeItem
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        private OntologyTypeMappingItem()
        {

        }

        public OntologyTypeMappingItem(string typeUri)
        {
            TypeUri = typeUri;
        }

        public string TypeUri
        {
            get;
            set;
        }
    }
}
