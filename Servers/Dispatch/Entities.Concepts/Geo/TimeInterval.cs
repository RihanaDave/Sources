using System;

namespace GPAS.Dispatch.Entities.Concepts.Geo
{
    public class TimeInterval
    {
        public DateTime TimeBegin { set; get; }
        public DateTime TimeEnd { set; get; }

        public TimeInterval Intersect(TimeInterval otherRange)
        {
            return TimeInterval.Intersect(this, otherRange);
        }

        public static TimeInterval Intersect(TimeInterval range1, TimeInterval range2)
        {
            if (range1.TimeEnd < range2.TimeBegin || range1.TimeBegin > range2.TimeEnd)
                return null;

            TimeInterval intersectRange = new TimeInterval();
            intersectRange.TimeBegin = (range1.TimeBegin > range2.TimeBegin) ? range1.TimeBegin : range2.TimeBegin;
            intersectRange.TimeEnd = (range1.TimeEnd < range2.TimeEnd) ? range1.TimeEnd : range2.TimeEnd;
            return intersectRange;
        }
    }
}