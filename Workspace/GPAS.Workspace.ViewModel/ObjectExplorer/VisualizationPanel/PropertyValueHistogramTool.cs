using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel
{
    public class PropertyValueHistogramTool : VisualizationPanelToolBase
    {
        public PropertyValueCategory PropertyValueCategory { get; set; }

        public PropertyValueHistogramSortType SortType { get; set; }

        public int ShowingCount { get; set; }

        public PreviewStatistic ExploringPreviewStatistic { get; set; }
    }
}
