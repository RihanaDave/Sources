using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.HistogramViewer
{
    public class HistogramGroupNode : HistogramNode
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="GroupTitle">نام (عنوان نمایشی) گروه</param>
        internal HistogramGroupNode(string GroupTitle)
            : base(GroupTitle)
        {
            relatedLabel.Background = Appearance.Groups.RowBackground;
            relatedLabel.Foreground = Appearance.Groups.RowForeground;
            relatedLabel.FontSize = Appearance.Groups.RowFontSize;
            relatedLabel.FontWeight = Appearance.Groups.RowFontWeight;
            subProperties = new List<HistogramPropertyNode>();
        }
        /// <summary>
        /// لیست ویژگی های زیرمجموعه این گروه را در برمیگیرد
        /// </summary>
        private List<HistogramPropertyNode> subProperties;

        /// <summary>
        /// لیست ویژگی های زیرمجموعه این گروه را برمیگرداند
        /// </summary>
        internal List<HistogramPropertyNode> SubProperties
        {
            get { return subProperties; }
        }
        /// <summary>
        /// یک ویژگی را به گروه اضافه می کند
        /// </summary>
        /// <param name="PropertyToAdd"></param>
        internal void AddASubProperty(HistogramPropertyNode PropertyToAdd)
        {
            subProperties.Add(PropertyToAdd);
        }
    }
}