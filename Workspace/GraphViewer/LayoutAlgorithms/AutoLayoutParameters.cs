using System;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «خودکار» گره‌ها در گراف
    /// </summary>
    public class AutoLayoutParameters : ILayoutParameters
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        internal AutoLayoutParameters()
        {
            HorizontalGapBetweenVertices = 180;
            VerticalGapBetweenVertices = 180;
        }

        public float HorizontalGapBetweenVertices
        {
            get;
            set;
        }

        public int Seed
        {
            get;
            set;
        }

        public float VerticalGapBetweenVertices
        {
            get;
            set;
        }

        /// <summary>
        /// ایجاد یک رونوشت از نمونه کلاس
        /// </summary>
        public object Clone()
        {
            return new AutoLayoutParameters()
                {
                    HorizontalGapBetweenVertices = this.HorizontalGapBetweenVertices
                    , VerticalGapBetweenVertices = this.VerticalGapBetweenVertices
                };
        }
    }
}