using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
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
        public ConstValueMappingItem()
        { }

        public ConstValueMappingItem(string constValue)
        {
            ConstValue = constValue;
        }

        public virtual string ConstValue
        {
            get;
            set;
        }
    }
}
