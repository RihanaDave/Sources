using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel
{
    public class BarChartTool : VisualizationPanelToolBase
    {
        public PropertyBarValues PropertyBarValues { get; set; }
        public PropertyBarValues DefaultPropertyBarValues { get; set; }
        public PreviewStatistic ExploringPreviewStatistic { get; set; }
    }
}
