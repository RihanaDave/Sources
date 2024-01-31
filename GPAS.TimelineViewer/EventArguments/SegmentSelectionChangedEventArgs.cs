using System;
using System.Collections.Generic;

namespace GPAS.TimelineViewer.EventArguments
{
    public class SegmentSelectionChangedEventArgs: EventArgs
    {
        public SegmentSelectionChangedEventArgs(List<Range> addedRanges, List<Range> removedRanges)
        {
            AddedRanges = addedRanges;
            RemovedRanges = removedRanges;
        }

        public List<Range> AddedRanges { get; private set; }
        public List<Range> RemovedRanges { get; private set; }
    }
}
