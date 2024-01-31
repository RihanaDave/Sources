using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.HistogramViewer
{
    public enum HistogramPropertyNodeOrderBy
    {
        Count = 0,
        Title = 1
    }

    public class HistogramCategoryOrderByChangedEventArgs : EventArgs
    {
        public HistogramCategoryOrderByChangedEventArgs(HistogramPropertyNodeOrderBy oldValue, HistogramPropertyNodeOrderBy newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public HistogramPropertyNodeOrderBy OldValue
        {
            get; set;
        }

        public HistogramPropertyNodeOrderBy NewValue
        {
            get; set;
        }
    }

    public delegate void HistogramCategoryOrderByChangedHandler(object source, HistogramCategoryOrderByChangedEventArgs e);
}
