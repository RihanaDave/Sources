using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.BarChartViewer
{
    public class BarChartSelectionChangedEventArgs : EventArgs
    {
#pragma warning disable CS0169 // The field 'BarChartSelectionChangedEventArgs.added' is never used
        private object added;
#pragma warning restore CS0169 // The field 'BarChartSelectionChangedEventArgs.added' is never used

        public BarChartSelectionChangedEventArgs(IEnumerable<Range> added, IEnumerable<Range> removed)
        {
            AddedBar = added;
            RemovedBar = removed;
        }

        public IEnumerable<Range> AddedBar { get; private set; }
        public IEnumerable<Range> RemovedBar { get; private set; }
    }
}
