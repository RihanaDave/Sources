using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Formula
{
    public class PropertyValueBasedDrillDown : FormulaBase
    {
        public List<PropertyValueStatistic> FilteredBy { get; set; }
    }
}