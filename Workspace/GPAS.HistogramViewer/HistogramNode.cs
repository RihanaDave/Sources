using System;
using System.Collections.Generic;
using System.Windows.Controls;
using GPAS.DataBarViewer;

namespace GPAS.HistogramViewer
{
    ///
    /// این کلاس ها منفعل ند و عملکرد آن ها وابسته به دیگر کلاس های
    /// هیستوگرام است.
    /// 
    /// این کلاس ها رخدادهای ایجاد شده را اظهار می کنند و کلاس
    /// استفاده کنننده از آنها موظف به نگهداری، و اموری مانند لیست
    /// گره های انتخاب شده می باشد.
    /// 

    public abstract class HistogramNode
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="NodeTitle">نام (عنوان نمایشی) گره</param>
        internal HistogramNode(string NodeTitle)
        {
            relatedLabel = new Label();
            relatedLabel.Content = NodeTitle;
            relatedLabel.Tag = this;
            relatedLabel.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        }

        /// <summary>
        /// نام (عنوان نمایشی) گره را برمیگرداند
        /// </summary>
        internal string Title
        {
            get { return relatedLabel.Content.ToString(); }
        }

        /// <summary>
        /// کنترل برچسب مربوط به این گره را دربرمیگیرد
        /// </summary>
        protected Label relatedLabel = null;
        /// <summary>
        /// کنترل برچسب مربوط به گره را برمیگرداند
        /// </summary>
        /// <remarks>
        /// این کنترل همان کنترلی ست که در رابط کاربری به نمایش درخواهد آمد
        /// این کنترل در زمان ایجاد گره ساخته می شود
        /// کنترل اصلی واسط بین منطق و رابط کاربری هیستوگرام، این کنترل است
        /// ویژگی «تگ» این کنترل، به این گره اشاره خواهد کرد
        /// </remarks>
        internal Label RelatedLabel
        {
            get { return relatedLabel; }
        }

        public object Tag
        {
            get;
            set;
        }
    }
}