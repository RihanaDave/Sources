using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    internal class GraphContent
    {
        internal GraphContent()
        { }
        internal List<KWObject> NotResolvedObjectsToShowOnGraph = new List<KWObject>();
        internal List<KWLink> LinksToShowOnGraph = new List<KWLink>();
        internal List<KWProperty> PropertiesToShowOnGraph = new List<KWProperty>();
    }
}