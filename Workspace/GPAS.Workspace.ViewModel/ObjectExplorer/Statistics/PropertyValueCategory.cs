using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PropertyValueCategory
    {
        public string Title { get; set; }
        public int LoadedValuesCount { get; set; }
        public int TotalValuesCount { get; set; }
        public List<PropertyValueStatistic> LoadedValues { get; set; }
        public long MinimumLoadableValueCount { get; set; }
        public bool HasUnloadableValues { get; set; }
        public string TypeUri { get; set; }
    }
}
