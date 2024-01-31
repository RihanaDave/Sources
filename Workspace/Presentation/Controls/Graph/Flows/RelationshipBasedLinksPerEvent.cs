using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Graph.Flows
{
    public class RelationshipBasedLinksPerEvent
    {
        public RelationshipBasedLinksPerEvent(KWObject intermediaryEvent)
        {
            IntermediaryEvent = intermediaryEvent;
            IncommingLinks = new List<RelationshipBasedKWLink>();
            OutgoingLinks = new List<RelationshipBasedKWLink>();
        }

        public KWObject IntermediaryEvent { get; private set; }
        public List<RelationshipBasedKWLink> IncommingLinks { get; set; }
        public List<RelationshipBasedKWLink> OutgoingLinks { get; set; }
    }
}
