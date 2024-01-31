using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.HistogramViewer
{
    class HistogramPropertyNodeValueCountItemsAddedEventArgs : EventArgs
    {
        public HistogramPropertyNodeValueCountItemsAddedEventArgs(IEnumerable<HistogramPropertyValueNode> addedItems)
        {
            AddedItems = addedItems;
        }

        public IEnumerable<HistogramPropertyValueNode> AddedItems
        {
            get;
            set;
        }
    }
}
