using GPAS.DataBarViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace GPAS.HistogramViewer
{
    class HistogramPropertyValueNode : HistogramNode
    {
        /// <summary>
        /// سازنده کلاس
        /// گره مقدار-تعداد به صورت خودکار به گره های زیرمجموعه گره ویژگی افزوده خواهد شد
        /// </summary>
        /// <remarks>
        /// کنترل نوار داده اینجا ایجاد می شود
        /// ویژگی «تک» کنترل داده برابر کنترل برچسب این گره قرار می گیرد تا در مواقع نیاز به عنوان کنترل اصلی بتواند استفاده شود
        /// </remarks>
        /// <param name="ValueTitle">عنوان نمایشی (نام) مقدار ویژگی</param>
        internal HistogramPropertyValueNode(string ValueTitle, int ValuesCount, HistogramPropertyNode RelatedPropertyNode, bool IsMostTopValueCount = false)
            : base(ValueTitle)
        {
            relatedDataBar = new DataBar();
            relatedDataBar.Value = ValuesCount;
            relatedDataBar.Tag = this.relatedLabel;
            IsTopMostNode = IsMostTopValueCount;

            relatedDataBar.OversizedBarBrush = Appearance.PropertyValue.DataBarValueBrushOfOversizedValues;

            relatedDataBar.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
            relatedDataBar.BorderThickness = new System.Windows.Thickness(0, 0, 3, 0);

            relatedLabel.MouseLeftButtonDown += relatedLabel_MouseLeftButtonDown;
            relatedDataBar.MouseLeftButtonDown += relatedDataBar_MouseLeftButtonDown;
            relatedLabel.MouseLeftButtonUp += relatedLabel_MouseLeftButtonUp;
            relatedDataBar.MouseLeftButtonUp += relatedDataBar_MouseLeftButtonUp;

            relatedLabel.MouseEnter += relatedLabel_MouseEnter;
            relatedDataBar.MouseEnter += relatedDataBar_MouseEnter;
            relatedLabel.MouseLeave += relatedLabel_MouseLeave;
            relatedDataBar.MouseLeave += relatedDataBar_MouseLeave;

            NodeMouseLeftButtonDown += NodeMouseLeftButtonDown_EmptyEventHandler;
            NodeMouseLeftButtonUp += NodeMouseLeftButtonUp_EmptyEventHandler;

            propertyNode = RelatedPropertyNode;
            relatedLabel.Foreground = relatedDataBar.Foreground = Appearance.Properties.RowForeground;
            // از آنجایی که رنگ پس زمینه هر گره مقدار براساس سرجمع
            // گره ها تعیین می شود، پس از افزودن هر گره، رنگ پس زمینه
            // همه گره ها بازنشانی می شود.
            //
            // TODO: => این کد به خاطر کار نکردن و همچنین سربار نمایی غیرفعال شد.
            //
            //foreach (HistogramPropertyValueNode value_Count in propertyNode.ValueCounts)
            //    value_Count.RelatedLabel.Background =
            //        value_Count.RelatedDataBar.Background =
            //        (value_Count.IsListInAnEvenRow()) ? Appearance.PropertyValue.EvenRowBackground : Appearance.PropertyValue.OddRowBackground;
        }

        /// <summary>
        /// فراوانی (تعداد) ویژگی های دارای مقدار این گره را برمیگرداند
        /// </summary>
        internal int Count
        {
            get { return int.Parse(relatedDataBar.Value.ToString()); }
        }

        internal bool IsTopMostNode
        {
            get;
            private set;
        }

        /// <summary>
        /// گره ویژگی که این گره مربوط به یکی از مقادیر آن است را دربرمیگیرد
        /// </summary>
        private HistogramPropertyNode propertyNode = null;
        /// <summary>
        /// گره ویژگی مربوط به این گره (که این گره مربوط به یکی از مقادیر آن است) را برمیگرداند
        /// </summary>
        internal HistogramPropertyNode PropertyNode
        {
            get { return propertyNode; }
        }

        /// <summary>
        /// کنترل نوار داده این گره را در برمیگیرد
        /// </summary>
        private DataBar relatedDataBar = null;
        /// <summary>
        /// کنترل نوار داده مربوط به این گره را برمیگرداند
        /// </summary>
        internal DataBar RelatedDataBar
        {
            get { return relatedDataBar; }
        }

        /// <summary>
        /// نشان می دهد که گره در لیست مقادیر ویژگی مربوطه اش در یک ردیف زوج است یا یک ردیف فرد؛
        /// در صورت عدم انتساب این گره به یک ویژگی خاص، این عملگر با استثناء مواجه خواهد شد
        /// </summary>
        /// <returns>در صورت لیست شدن در یک ردیف زوج، مقدار صحیح را برمیگرداند</returns>
        internal bool IsListInAnEvenRow()
        {
            return (propertyNode.ValueCounts.IndexOf(this) % 2) == 0;
        }

        /// <summary>
        /// گره را به حالت انتخاب شده می برد
        /// </summary>
        public void SelectNode()
        {
            relatedLabel.Background =
                relatedDataBar.Background = Appearance.PropertyValue.SelectedRowBackground;
        }

        /// <summary>
        /// گره را از حالت انتخاب شده خارج می کند
        /// </summary>
        public void DeselectNode()
        {
            relatedLabel.Background =
                relatedDataBar.Background =
                (this.IsListInAnEvenRow()) ? Appearance.PropertyValue.EvenRowBackground : Appearance.PropertyValue.OddRowBackground;
        }

        /// <summary>
        /// در حالت انتخاب بودن یا نبودن گره را برمی گرداند
        /// </summary>
        /// <returns>درصورت انتخاب شده بودن گره، مقدار صحیح را برمیگرداند</returns>
        internal bool IsSelected()
        {
            return relatedLabel.Background == Appearance.PropertyValue.SelectedRowBackground;
        }

        #region رخدادها و رخداد گردان ها
        // رخدادگردان های مربوط به کلیک چپ روی کنترل های گره
        void relatedDataBar_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonUp.Invoke(this, e);
        }
        void relatedLabel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonUp.Invoke(this, e);
        }
        void relatedDataBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonDown.Invoke(this, e);
        }
        void relatedLabel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonDown.Invoke(this, e);
        }

        // رخدادگردان های مربوط به عبور موس از روی گره ها
        void relatedDataBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((DataBar)e.Source).Background == Appearance.PropertyValue.PointedRowBackground)
                ((DataBar)e.Source).Background =
                    ((Label)(((DataBar)e.Source).Tag)).Background =
                    (((HistogramPropertyValueNode)(((Label)(((DataBar)e.Source).Tag)).Tag)).IsListInAnEvenRow()) ?
                    Appearance.PropertyValue.EvenRowBackground :
                    Appearance.PropertyValue.OddRowBackground;
        }
        void relatedLabel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((Label)e.Source).Background == Appearance.PropertyValue.PointedRowBackground)
                ((Label)e.Source).Background =
                    ((HistogramPropertyValueNode)(((Label)e.Source).Tag)).RelatedDataBar.Background =
                    (((HistogramPropertyValueNode)(((Label)e.Source).Tag)).IsListInAnEvenRow()) ?
                    Appearance.PropertyValue.EvenRowBackground :
                    Appearance.PropertyValue.OddRowBackground;
        }
        void relatedDataBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((HistogramPropertyValueNode)(((Label)(((DataBar)e.Source).Tag)).Tag)).IsSelected())
                return;
            if (((DataBar)e.Source).Background != Appearance.PropertyValue.PointedRowBackground)
                ((DataBar)e.Source).Background =
                    ((Label)(((DataBar)e.Source).Tag)).Background = Appearance.PropertyValue.PointedRowBackground;
        }
        void relatedLabel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (((HistogramPropertyValueNode)(((Label)e.Source).Tag)).IsSelected())
                return;
            if (((Label)e.Source).Background != Appearance.PropertyValue.PointedRowBackground)
                ((Label)e.Source).Background =
                    ((HistogramPropertyValueNode)(((Label)e.Source).Tag)).RelatedDataBar.Background = Appearance.PropertyValue.PointedRowBackground;
        }

        /// <summary>
        /// زمانی رخ می دهد که کلیک چپ موس روی گره فشرده شده باشد
        /// </summary>
        public event HistogramPropertyValueCountNodeMouseButtonEventHandler NodeMouseLeftButtonDown;
        /// <summary>
        /// زمانی رخ می دهد که کلیک چپ موس از روی گره رها شود
        /// </summary>
        public event HistogramPropertyValueCountNodeMouseButtonEventHandler NodeMouseLeftButtonUp;

        /// <summary>
        /// ساختار رخدادگردان عمل کلیک موس روی این نوع گره
        /// </summary>
        public delegate void HistogramPropertyValueCountNodeMouseButtonEventHandler
            (HistogramPropertyValueNode sender, System.Windows.Input.MouseButtonEventArgs e);

        static void NodeMouseLeftButtonUp_EmptyEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        { }
        static void NodeMouseLeftButtonDown_EmptyEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        { }
        #endregion
    }
}