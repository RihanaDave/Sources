using System;
using System.Collections;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs
{
    public class CriteriaItemsChangedEventArgs : EventArgs
    {
        public CriteriaItemsChangedEventArgs(IList oldItems, IList newItems)
        {
            OldItems = oldItems;
            NewItems = newItems;
        }

        public IList OldItems { get; protected set; }
        public IList NewItems { get; protected set; }
    }
}
