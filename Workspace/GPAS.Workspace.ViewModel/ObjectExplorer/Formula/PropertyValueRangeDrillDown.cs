using System.Collections.Generic;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Formula
{
    public class PropertyValueRangeDrillDown : FormulaBase
    {
        public string PropertyTypeUri { get; set; }
        public List<NumericPropertyValueRange> ValueRanges { get; set; }
    }
}
