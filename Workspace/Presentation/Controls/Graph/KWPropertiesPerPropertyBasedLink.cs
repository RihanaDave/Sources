using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    internal class KWPropertiesPerPropertyBasedLink
    {
        public GraphArrangment.PropertyBasedLink ArrangmentLink { get; set; }
        public KWProperty SourceProperty { get; set; }
        public KWProperty TargetProperty { get; set; }
    }
}
