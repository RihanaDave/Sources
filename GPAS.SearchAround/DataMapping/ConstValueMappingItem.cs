using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround.DataMapping
{
    /// <summary>
    /// کلاس «اقلام نگاشتی» که یک مقدار ثابت را نشان می‌دهند
    /// </summary>
    [Serializable]
    public class ConstValueMappingItem : SingleValueMappingItem
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        private ConstValueMappingItem()
        { }

        public ConstValueMappingItem(string constValue)
        {
            ConstValue = constValue;
        }

        public string ConstValue
        {
            get;
            set;
        }
    }
}
