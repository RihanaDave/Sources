using System;

namespace GPAS.BarChartViewer
{
    public class RangeChangedEventArgs : EventArgs
    {
        public RangeChangedEventArgs(double newStart, double newEnd, double oldStart, double oldEnd)
        {
            NewStart = newStart;
            NewEnd = newEnd;
            OldStart = oldStart;
            OldEnd = oldEnd;
        }

        public double OldStart { get; private set; }
        public double NewStart { get; private set; }
        public double OldEnd { get; private set; }
        public double NewEnd { get; private set; }
    }
}
