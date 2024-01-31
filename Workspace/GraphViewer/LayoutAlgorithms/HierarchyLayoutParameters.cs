using System;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// پارامترهای موردنیاز برای الگوریتم چینش «سلسله‌مراتبی» گره ها در گراف
    /// </summary>
    public class HierarchyLayoutParameters : ILayoutParameters
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public HierarchyLayoutParameters()
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
            return new HierarchyLayoutParameters();
        }
    }
}
