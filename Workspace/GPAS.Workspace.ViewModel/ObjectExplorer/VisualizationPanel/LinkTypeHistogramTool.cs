using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel
{
    public class LinkTypeHistogramTool : VisualizationPanelToolBase
    {
        public LinkTypeHistogramTool()
        {
            linkTypeStatistics = new List<PreviewStatistic>();
        }
        public List<PreviewStatistic> linkTypeStatistics { get; set; }
    }
}
