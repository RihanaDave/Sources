using System;
using System.Collections.Generic;

namespace GPAS.HistogramViewer
{
    public class NewGroupSubItemAddedEventArgs : EventArgs
    {
        public NewGroupSubItemAddedEventArgs(IEnumerable<IHistogramFillingProperty> addedSubItems)
        {
            AddedSubItems = addedSubItems;
        }

        public IEnumerable<IHistogramFillingProperty> AddedSubItems
        {
            get;
            private set;
        }
    }
}