using System;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «خطی مرتب شده» گره ها در گراف
    /// </summary>
    public class SortedLinerLayoutParameters : ILayoutParameters
    {
        #region مقادیر ثابت پیش فرض
        /// <summary>
        /// فاصله پیش فرض (افقی) بین گره ها در چینش «خطی مرتب شده» را براساس پیکسل نگهداری می کند
        /// </summary>
        public const double DefaultInterVertexGap = 25;
        #endregion

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="gapBetweenVertices">فاصله (افقی) بین گره ها گره ها پس از اعمال چینش برحسب پیکسل</param>
        public SortedLinerLayoutParameters(double gapBetweenVertices = DefaultInterVertexGap)
        {
            GapBetweenVertices = gapBetweenVertices;
        }

        /// <summary>
        /// فاصله افقی بین گره ها در چینش «خطی مرتب شده» را براساس پیکسل نگهداری می کند
        /// </summary>
        public double GapBetweenVertices
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
        /// ایجاد یک رونوشت از نمونه کلاس
        /// </summary>
        public object Clone()
        {
            return new SortedLinerLayoutParameters(GapBetweenVertices);
        }
    }
}