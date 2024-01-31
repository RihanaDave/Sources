using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PropertyBarValue
    {
        public long Count { get; set; }
        public bool IsSelected { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
    }
}
