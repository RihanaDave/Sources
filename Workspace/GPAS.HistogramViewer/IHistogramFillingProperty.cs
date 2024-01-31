using System.Collections.Generic;

namespace GPAS.HistogramViewer
{
    /// <summary>
    /// واسط دریافت داده های ویژگی ها برای استفاده در هیستوگرام
    /// </summary>
    public interface IHistogramFillingProperty
    {
        /// <summary>
        /// عنوان (نام نمایشی) گره ویژگی هیستوگرام که می خواهیم تعداد
        /// مقادیر آن را در هیستوگرام نمایش دهیم را برمیگرداند
        /// </summary>
        /// <remarks>این همان عنوانی است که چند زوج «مقدار-تعداد« زیرمجموعه آن می باشند</remarks>
        string HistogramTitle
        {
            get;
        }
        /// <summary>
        /// لیست مقادیر گره های ویژگی و تعداد مربوط به هر کدام را
        /// برمیگرداند
        /// </summary>
        /// <remarks>لیست زوج «مقدار-تعداد تکرار» مربوط به ویژگی خاص</remarks>
        List<IHistogramFillingValueCountPair> HistgramValueCounts
        {
            get;
        }

        HistogramPropertyNodeOrderBy OrderBy
        {
            get;
            set;
        }

        void HistogramValueCountsOrderBy(HistogramPropertyNodeOrderBy orderBy);
    }
}
