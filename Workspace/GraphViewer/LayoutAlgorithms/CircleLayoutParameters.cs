using System;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «دایره ای» گره ها در گراف
    /// </summary>
    public class CircleLayoutParameters : ILayoutParameters
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public CircleLayoutParameters()
        { }

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
            return new CircleLayoutParameters();
        }

        public double VerticesMinimumDistanceFromCenter
        {
            get;
            set;
        }
    }
}
