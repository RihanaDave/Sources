using System;

namespace GPAS.TagCloudViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «خودکار» گره‌ها در گراف
    /// </summary>
    public class TagCloudLayoutParameters : ILayoutParameters
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        internal TagCloudLayoutParameters()
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
            return new TagCloudLayoutParameters()
                {
                    HorizontalGapBetweenVertices = this.HorizontalGapBetweenVertices
                    , VerticalGapBetweenVertices = this.VerticalGapBetweenVertices
                };
        }
    }
}