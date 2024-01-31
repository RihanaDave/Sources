using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    /// <summary>
    /// کلاس «اقلام نگاشتی» که مقداری در جدول نیم‌ساختیافته را نشان می‌دهد؛
    /// از آنجایی که نگاشت مربوط به یک ردیف است، این کلاس اندیس ستونی که این قلم داده در آن وجود دارد، را دربرمی‌گیرد
    /// </summary>
    [Serializable]
    public class TableColumnMappingItem : SingleValueMappingItem, IResolvableValueMappingItem
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected TableColumnMappingItem()
        { }
        public TableColumnMappingItem(int columnIndex, string columnTitle = "", PropertyInternalResolutionOption resolutionOption = PropertyInternalResolutionOption.Ignorable)
        {
            ColumnIndex = columnIndex;
            if (columnTitle == "")
                ColumnTitle = string.Format("(Column with index: {0})", columnIndex);
            else
                ColumnTitle = columnTitle;
            ResolutionOption = resolutionOption;
            RegularExpressionPattern = string.Empty;
        }

        public int ColumnIndex
        {
            get;
            set;
        }

        public string ColumnTitle
        {
            get;
            set;
        }

        public override string ToString()
        {
            return ColumnTitle;
        }

        public string RegularExpressionPattern
        {
            get;
            set;
        }

        public PropertyInternalResolutionOption ResolutionOption
        {
            get;
            set;
        }
    }
}
