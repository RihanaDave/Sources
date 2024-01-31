using System;
using System.Collections.Generic;

namespace GPAS.TimelineViewer.EventArguments
{
    public class FilterWindowsChangedEventArgs : EventArgs
    {
        public FilterWindowsChangedEventArgs(IEnumerable<FilterRange> addedWindows, IEnumerable<FilterRange> removedWindows)
        {
            AddedWindows = addedWindows;
            RemovedWindows = removedWindows;
        }

        public IEnumerable<FilterRange> AddedWindows { get; private set; }
        public IEnumerable<FilterRange> RemovedWindows { get; private set; }
    }
}
