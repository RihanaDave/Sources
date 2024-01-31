using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PropertyBarValues
    {
        public string Title { get; set; }
        public string TypeUri { get; set; }
        public string Unit { get; set; }
        public int BucketCount { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public List<PropertyBarValue> Bars { get; set; }
        public PreviewStatistic ExploringPreviewStatistic { get; set; }
    }
}
