using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace TimelineViewerDemo
{
    public class DateTimePropertyValueStatistics
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Parent { get; set; }
        public BitmapImage Icon { get; set; }
        public List<ValueTimePair> ValueTimes { get; set; } = new List<ValueTimePair>();
    }
}
