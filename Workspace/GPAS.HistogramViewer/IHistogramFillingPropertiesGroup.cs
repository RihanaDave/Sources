using System;
using System.Collections.Generic;

namespace GPAS.HistogramViewer
{
    /// <summary>
    /// واسط مقداردهی گروه ویژگی ها در هیستوگرام
    /// </summary>
    public interface IHistogramFillingPropertiesGroup
    {
        /// <summary>
        /// عنوان (نام نمایشی) گروه -که ویژگی ها را دربرمیگیرد- برای
        /// نمایش در هیستوگرام را برمیگرداند
        /// </summary>
        string HistogramTitle
        {
            get;
        }
        /// <summary>
        /// ویژگی های زیر مجموعه گروه را برمیگرداند
        /// </summary>
        List<IHistogramFillingProperty> HistogramSubItems
        {
            get;
        }

        event EventHandler<NewGroupSubItemAddedEventArgs> NewSubItemAdded;
    }
}