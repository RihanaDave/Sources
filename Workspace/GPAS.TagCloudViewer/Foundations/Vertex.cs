using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using GraphX.PCL.Common.Models;

namespace GPAS.TagCloudViewer.Foundations
{
    /// <summary>
    /// کلاس داده ای گره در گراف
    /// </summary>
    public class Vertex : VertexBase, IComparable<Vertex>, INotifyPropertyChanged
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="vertexTitle">عنوان گره</param>
        /// <param name="relatedVertexControl"></param>
        /// <param name="constructVertexControlIfNotSet">درصورت غلط بودن این مقدار و نال بودن مقدار ورودی کنترل گره، سازنده یک کنترل گره جدید می سازد</param>
        /// <remarks>این سازنده، یک کنترل گره برای گره حاضر می سازد</remarks>
        protected Vertex(string vertexTitle, VertexControl relatedVertexControl = null, bool constructVertexControlIfNotSet = true)
        {
            if (vertexTitle == null)
                throw new ArgumentNullException("vertexTitle");

            ID = GraphElementsUniqueIdGenerator.GenerateUniqueID();
            Text = vertexTitle;
            RelatedVertexControl
                = (relatedVertexControl == null && constructVertexControlIfNotSet)
                ? new VertexControl(this)
                : relatedVertexControl;
        }

        /// <summary>
        /// رخداد «تغییر در ویژگی های شئ»؛
        /// در صورت تغییر در عنوان/تعداد زیرگروه ها، این رخداد صادر می شود
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// عملگر صدور رخداد «تغییر در ویژگی های شئ»
        /// </summary>
        protected virtual void OnPropertyChanged(string[] changedPropertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (var item in changedPropertyNames)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(item));
                }
            }
        }

        private string text;
        /// <summary>
        /// عنوان تعیین شده برای گره را برمی گرداند
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged(new string[] { "Text" });
            }
        }
        public override string ToString()
        {
            return Text;
        }

        private Uri _iconPath = null;
        public Uri IconPath
        {
            get;
            private set;
        }
        /// <summary>
        /// نمونه شی تصویر مربوط به آیکن گره را براساس آدرس تعیین شده برمی گرداند
        /// </summary>
        public System.Drawing.Image GetIconFromPath()
        { return new Bitmap(_iconPath.ToString()); }
        /// <summary>
        /// مسیر آیکن مربوط به گره را پس از اعتبارسنجی مقدار ورودی، مقداردهی می کند
        /// </summary>
        /// <param name="iconUrlToSet">مسیری مطلق/نسبی آیکن مربوط به گره</param>
        public void SetIconUri(string iconUrlToSet)
        {
            if (iconUrlToSet == null)
                throw new ArgumentNullException("iconUrlToSet");
            if (string.IsNullOrWhiteSpace(iconUrlToSet))
                throw new ArgumentException("Invalid argument", "iconUrlToSet");

            Uri iconUri = null;
            if (!Uri.TryCreate(iconUrlToSet, UriKind.RelativeOrAbsolute, out iconUri))
                throw new ArgumentException("Icon path is not a valid URI", "iconUrlToSet");
            IconPath = iconUri;
        }

        public VertexControl RelatedVertexControl
        {
            get;
            protected set;
        }

        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// الگوریتم مقایسه گره ها با هم را نگهداری می کند
        /// </summary>
        private static Func<Vertex, Vertex, int> verticesCompareMethod
            = delegate (Vertex first, Vertex second)
            {
                if (first == null)
                    throw new ArgumentNullException("first");
                if (second == null)
                    throw new ArgumentNullException("second");
                return first.Text.CompareTo(second.Text);
            };
        /// <summary>
        /// الگوریتم مقایسه گره ها با هم را مقدارهی می کند/برمی گرداند
        /// </summary>
        public static Func<Vertex, Vertex, int> VerticesCompareMethod
        {
            get { return verticesCompareMethod; }
            set { verticesCompareMethod = value; }
        }
        /// <summary>
        /// مقایسه گره با یک گره دیگر
        /// </summary>
        public int CompareTo(Vertex other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            return VerticesCompareMethod(this, other);
        }

        /// <summary>
        /// کارخانه (متد فراخوانی سازنده برای) ایجاد گره جدید
        /// </summary>
        public static Vertex VertexFactory(string vertexTitle, VertexControl relatedVertexControl = null)
        {
            if (vertexTitle == null)
                throw new ArgumentNullException("vertexTitle");

            return new Vertex(vertexTitle, relatedVertexControl);
        }
    }
}