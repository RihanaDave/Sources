using GPAS.FilterSearch;
using System;

namespace GPAS.Workspace.Presentation.Helpers.Filter.EventsArgs
{
    public class FilterAppliedEventArgs : EventArgs
    {
        public FilterAppliedEventArgs(Query query)
        {
            Query = query;
        }

        public Query Query { get; set; }
    }
}
