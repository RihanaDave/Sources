using System;

namespace GPAS.TimelineViewer.EventArguments
{
    public class RangeChangedEventArgs : EventArgs
    {
        public RangeChangedEventArgs(DateTime newStart, DateTime newEnd, DateTime oldStart, DateTime oldEnd)
        {
            NewStart = newStart;
            NewEnd = newEnd;
            OldStart = oldStart;
            OldEnd = oldEnd;
        }

        public DateTime OldStart { get; private set; }
        public DateTime NewStart { get; private set; }
        public DateTime OldEnd { get; private set; }
        public DateTime NewEnd { get; private set; }
    }
}
