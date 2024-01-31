using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PropertyValueStatistic
    {
        public PropertyValueCategory Category { get; set; }
        public string PropertyValue { get; set; }
        public long Count { get; set; }
        public bool IsSelected { get; set; }
    }
}
