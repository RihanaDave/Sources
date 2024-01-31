using System;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «جدولی» گره ها در گراف
    /// </summary>
    public class GridLayoutParameters : ILayoutParameters
    {
        #region مقادیر ثابت پیش فرض
        /// <summary>
        /// فاصله پیش فرض افقی بین گره ها در چینش «جدولی» را براساس پیکسل نگهداری می کند
        /// </summary>
        public const double DefaultHorizontalInterVertexGap = 45;
        /// <summary>
        /// فاصله پیش فرض عمودی بین گره ها در چینش «جدولی» را براساس پیکسل نگهداری می کند
        /// </summary>
        public const double DefaultVerticalInterVertexGap = 45;
        #endregion
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public GridLayoutParameters(double horizontalGapBetweenVertices = DefaultHorizontalInterVertexGap, double verticalGapBetweenVertices = DefaultVerticalInterVertexGap)
        {
            // مقداردهی ویژگی های الگوریتم براساس ورودی ها
            HorizontalGapBetweenVertices = horizontalGapBetweenVertices;
            VerticalGapBetweenVertices = verticalGapBetweenVertices;
        }
        /// <summary>
        /// فاصله افقی بین گره ها در چینش «جدولی» را براساس پیکسل نگهداری می کند
        /// </summary>
        public double HorizontalGapBetweenVertices
        {
            get;
            set;
        }

        public int Seed
        {
            get;
            set;
        }

        /// <summary>
        /// فاصله عمودی بین گره ها در چینش «جدولی» را براساس پیکسل نگهداری می کند
        /// </summary>
        public double VerticalGapBetweenVertices
        {
            get;
            set;
        }
        /// <summary>
        /// ایجاد یک رونوشت از نمونه کلاس
        /// </summary>
        public object Clone()
        {
            return new GridLayoutParameters(this.HorizontalGapBetweenVertices, this.VerticalGapBetweenVertices);
        }
    }
}
