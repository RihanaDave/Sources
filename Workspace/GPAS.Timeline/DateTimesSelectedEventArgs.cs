using System;
using System.Collections.Generic;

namespace GPAS.TimelineViewer
{
    public class DateTimesSelectedEventArgs
    {
        public DateTimesSelectedEventArgs(List<DateTime> selectedDateTimes)
        {
            SelectedDateTimes = selectedDateTimes;
        }

        public List<DateTime> SelectedDateTimes { get; private set; }
    }
}
