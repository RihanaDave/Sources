using GPAS.HistogramViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class HistogramCategoryValueCountAddedEventArgs : EventArgs
    {
        public HistogramCategoryValueCountAddedEventArgs(IEnumerable<IHistogramFillingValueCountPair> itemsAdded)
        {
            ItemsAdded = itemsAdded;
        }

        public IEnumerable<IHistogramFillingValueCountPair> ItemsAdded
        {
            get;
            set;
        }
    }
}
