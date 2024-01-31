using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Helpers.Filter.EventsArgs
{
    public class FilterSearchCompletedEventArgs : EventArgs
    {
        public FilterSearchCompletedEventArgs(IEnumerable<KWObject> result)
        {
            Result = result;
        }

        public IEnumerable<KWObject> Result { get; set; }
    }
}
