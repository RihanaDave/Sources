using GraphX.PCL.Common.Models;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace GPAS.Graph.GraphViewer.Foundations
{
    /// <summary>
    /// کلاس داده ای یال در گراف
    /// </summary>
    public class Edge : EdgeBase<Vertex>, INotifyPropertyChanged
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="source">گره مبدا یال جدید</param>
        /// <param name="target">گره مقصد یال جدید</param>
        /// <param name="direction">جهت یال جدید نسبت به مبدا و مقصد آن</param>
        public Edge(Vertex source, Vertex target, EdgeDirection direction, string displayText = "", Uri iconPath = null, string tooltip = "")
            : base(source, target, 1)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (displayText == null)
                throw new ArgumentNullException("displayText");

            // انتساب های اولیه
            ID = GraphElementsUniqueIdGenerator.GenerateUniqueID();
            Direction = direction;
            Text = displayText;
            IconPath = iconPath;
            RealSource = Source;
            RealTarget = Target;
            // ایجاد یک کنترل (نمایشی) یال جدید برای این یال
            RegenerateRelatedEdgeControl();
            // اعمال تنظیمات موردنیاز کنترل یال مربوط به این یال
            RelatedEdgeControl.AlignLabelsToEdges = true;
            ToolTip = (tooltip != "") ? tooltip : displayText;
            RelatedEdgeControl.SetBinding(EdgeControl.ToolTipProperty, new Binding("ToolTip")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string toolTip;
        public string ToolTip
        {
            get
            {
                return toolTip;
            }
            set
            {
                if(value != toolTip)
                {
                    toolTip = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string text;

        /// <summary>
        /// متنی که روی بال نمایش داده خواهد شد
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if(value != text)
                {
                    text = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override string ToString()
        { return Text; }

        private Uri iconPath;
        public Uri IconPath
        {
            get
            {
                return iconPath;
            }
            private set
            {
                if (value != iconPath)
                {
                    iconPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// نمونه شی تصویر مربوط به آیکن یال را براساس آدرس تعیین شده برمی گرداند
        /// </summary>
        public System.Drawing.Image GetIconFromPath()
        { return new Bitmap(IconPath.ToString()); }
        /// <summary>
        /// مسیر آیکن مربوط به یال را پس از اعتبارسنجی مقدار ورودی، مقداردهی می کند
        /// </summary>
        /// <param name="iconUrlToSet">مسیری مطلق/نسبی آیکن مربوط به یال</param>
        public void SetIconUri(string iconUrlToSet)
        {
            Uri iconUri = null;
            if (!Uri.TryCreate(iconUrlToSet, UriKind.RelativeOrAbsolute, out iconUri))
                throw new ArgumentException("Icon path is not a valid URI", "iconUrlToSet");
            IconPath = iconUri;
        }
        public void SetIconUri(Uri iconUrlToSet)
        {
            if (iconUrlToSet == null)
                throw new ArgumentNullException();
            
            IconPath = iconUrlToSet;
        }

#pragma warning disable CS0169 // The field 'Edge.direction' is never used
        private EdgeDirection direction;
#pragma warning restore CS0169 // The field 'Edge.direction' is never used
        /// <summary>
        /// جهتدار بودن یال از سمت مبدا به مقصد را نشان می دهد
        /// </summary>
        public EdgeDirection Direction
        {
            get;
            private set;
        }

        public void SetDirection(EdgeDirection newDirection)
        {
            if (Direction != newDirection)
            {
                if(RelatedEdgeControl != null)
                {
                    RelatedEdgeControl.UpdateDirection(newDirection);
                }
                Direction = newDirection;
            }
        }

        /// <summary>
        /// کنترل یال مربوط به این یال را نگهداری می کند
        /// </summary>
        /// <remarks>کنترل یال کنترل نمایشی برای یک یال (که یک تعریف منطقی می باشد) است</remarks>
        public EdgeControl RelatedEdgeControl
        {
            get;
            protected set;
        }
        /// <summary>
        /// یک کنترل (نمایشی) یال جدید برای این یال می سازد
        /// </summary>
        internal void RegenerateRelatedEdgeControl()
        {
            RelatedEdgeControl = new EdgeControl(this);
        }

        public object Tag
        {
            get;
            set;
        }

        /// <summary></summary>
        /// <remarks>این پیاده سازی به خاطر جلوگیری از مقداردهی توسط کلاس پدر پیاده سازی شده است</remarks>
        public new double Weight
        { get; }
        #region ویژگی ها و عملکردهای مربوط به پیاده سازی گروه بندی گره ها
        /// <summary>
        /// مبدا واقعی یال را نگه داری می کند
        /// </summary>
        /// <remarks>
        /// کاربرد این ویژگی در جمع کردن و باز کردن گره گروه بندی شده است؛
        /// زمانی یک که گره گروه بندی شده جمع می شود، یال های گره های زیرمجموعه آن گروه به گره های دیگری انتساب داده می شود، که این مساله در جمع و باز کردن های تودرتو کار را پیچیده می کند چرا که در هر باز کردن می بایست یال به گرهی در گراف جدید انتساب داده شود
        /// با توجه به نگاشتی که برای گره های گروه بندی شده گراف صورت می گیرد، در هر وضعیت از جمع/باز شدن این گره ها، می توان با داشتن گرهی که در اصل این یال مربوط به آن است، گره صحیح برای انتساب به یال را یافت
        /// </remarks>
        internal Vertex RealSource
        {
            get;
            private set;
        }
        /// <summary>
        /// مقصد واقعی یال را نگه داری می کند
        /// </summary>
        /// <remarks>
        /// کاربرد این ویژگی در جمع کردن و باز کردن گره گروه بندی شده است؛
        /// زمانی یک که گره گروه بندی شده جمع می شود، یال های گره های زیرمجموعه آن گروه به گره های دیگری انتساب داده می شود، که این مساله در جمع و باز کردن های تودرتو کار را پیچیده می کند چرا که در هر باز کردن می بایست یال به گرهی در گراف جدید انتساب داده شود
        /// با توجه به نگاشتی که برای گره های گروه بندی شده گراف صورت می گیرد، در هر وضعیت از جمع/باز شدن این گره ها، می توان با داشتن گرهی که در اصل این یال مربوط به آن است، گره صحیح برای انتساب به یال را یافت
        /// </remarks>
        internal Vertex RealTarget
        {
            get;
            private set;
        }
        /// <summary>
        /// یال را (در هر حالتی باشد) برای اعمال جمع شدن گره های گروه بندی شده ماسک می کند
        /// </summary>
        /// <param name="afterCollapseSource">گره مبدا برای ماسک کردن جمع شدن یال</param>
        /// <param name="afterCollapseTarget">گره مقصد برای ماسک کردن جمع شدن یال</param>
        /// <param name="graphviewerThatMastersTheEdge">نمایشگر گرافی که «ماسک کردن/حذف ماسک» یال می بایست روی آن صورت گیرد</param>
        /// <remarks>
        /// در زمان جمع کردن گره های گروه بندی شده، می بایست مبدا یا مقصد یال تغییر کند که این عمل را اصطلاحا ماسک کردن می گوییم
        /// حذف ماسک از مبدا یا مقصد یک یال به معنای برگرداندن یال به حالت اتصال به مبدا یا مثصد واقعی آن است
        /// </remarks>
        internal void CollapseMask(Vertex afterCollapseSource, Vertex afterCollapseTarget, GraphViewer graphviewerThatMastersTheEdge)
        {
            if (afterCollapseSource == null)
                throw new ArgumentNullException("afterCollapseSource");
            if (afterCollapseTarget == null)
                throw new ArgumentNullException("afterCollapseTarget");
            if (graphviewerThatMastersTheEdge == null)
                throw new ArgumentNullException("graphviewerThatMastersTheEdge");

            if (!IsCollapseMask())
            {
                RealSource = Source;
                RealTarget = Target;
            }
            Source = afterCollapseSource;
            Target = afterCollapseTarget;
            ApplyEdgeSourceTargetChangesToGraphViewer(graphviewerThatMastersTheEdge);
        }
        /// <summary>
        /// نشان می دهد که آیا یال در حال حاضر (از مبدا یا مقصد) ماسک شده یا خیر
        /// </summary>
        /// <returns>در صورت ماسک شدن از مبدا یا مقصد مقدار صحیح و در غیراینصورت غلط را برمیگرداند</returns>
        internal bool IsCollapseMask()
        {
            return RealSource != Source || RealTarget != Target;
        }

        /// <summary>
        /// مبدا یال که جهت پیاده سازی جمع/باز کردن گره های گروه بندی شده اعمال شده، ماسک شده بود را به حالت عادی خود (قبل از گروه بندی) برمی گرداند
        /// </summary>
        /// <param name="graphviewerThatMastersTheEdge">نمایشگر گرافی که «ماسک کردن/حذف ماسک» یال می بایست روی آن صورت گیرد</param>
        /// <remarks>
        /// در زمان جمع کردن گره های گروه بندی شده، می بایست مبدا یا مقصد یال تغییر کند که این عمل را اصطلاحا ماسک کردن می گوییم
        /// حذف ماسک از مبدا یا مقصد یک یال به معنای برگرداندن یال به حالت اتصال به مبدا یا مثصد واقعی آن است
        /// </remarks>
        internal void RemoveCollapseMaskSource(GraphViewer graphviewerThatMastersTheEdge)
        {
            if (graphviewerThatMastersTheEdge == null)
                throw new ArgumentNullException("graphviewerThatMastersTheEdge");

            if (!IsCollapseMask())
                return;
            Source = RealSource;
            ApplyEdgeSourceTargetChangesToGraphViewer(graphviewerThatMastersTheEdge);
        }
        /// <summary>
        /// مقصد یال که جهت پیاده سازی جمع/باز کردن گره های گروه بندی شده اعمال شده، ماسک شده بود را به حالت عادی خود (قبل از گروه بندی) برمی گرداند
        /// </summary>
        /// <param name="graphviewerThatMastersTheEdge">نمایشگر گرافی که «ماسک کردن/حذف ماسک» یال می بایست روی آن صورت گیرد</param>
        /// <remarks>
        /// در زمان جمع کردن گره های گروه بندی شده، می بایست مبدا یا مقصد یال تغییر کند که این عمل را اصطلاحا ماسک کردن می گوییم
        /// حذف ماسک از مبدا یا مقصد یک یال به معنای برگرداندن یال به حالت اتصال به مبدا یا مثصد واقعی آن است
        /// </remarks>
        internal void RemoveCollapseMaskTarget(GraphViewer graphviewerThatMastersTheEdge)
        {
            if (graphviewerThatMastersTheEdge == null)
                throw new ArgumentNullException("graphviewerThatMastersTheEdge");

            if (!IsCollapseMask())
                return;
            Target = RealTarget;
            ApplyEdgeSourceTargetChangesToGraphViewer(graphviewerThatMastersTheEdge);
        }

        /// <summary>
        /// این عملگر براساس آخرین مقادیر داده شده به ویژگی های مبدا و مقصد این یال، نمایشگر گراف را به روز رسانی می کند
        /// </summary>
        /// <param name="graphviewerThatMastersTheEdge">نمایشگر گرافی که در باز تولید کنترل یال می بایست روی آن صورت گیرد</param>
        /// <remarks>
        /// پس از تغییر مبدا یا مقصد یال، تغییرات می بایست جهت اعمال (نمایشی) به نمایشگر گراف اعمال شود
        /// این عملکرد عموما در زمان ماسک کردن یال اتفاق می افتد
        /// </remarks>
        private void ApplyEdgeSourceTargetChangesToGraphViewer(GraphViewer graphviewerThatMastersTheEdge)
        {
            if (graphviewerThatMastersTheEdge == null)
                throw new ArgumentNullException("graphviewerThatMastersTheEdge");

            RelatedEdgeControl.Source = Source.RelatedVertexControl;
            RelatedEdgeControl.Target = Target.RelatedVertexControl;
        }
        #endregion
    }
}
