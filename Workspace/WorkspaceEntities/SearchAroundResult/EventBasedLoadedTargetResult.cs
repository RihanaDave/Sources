using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public  class EventBasedLoadedTargetResult
    {
        public EventBasedLoadedTargetResult()
        {
            InnerRelationshipIDs = new List<EventBasedResultInnerRelationships>();
        }
        public List<EventBasedResultInnerRelationships> InnerRelationshipIDs { set; get; }
        public KWObject TargetObject { set; get; }
    }
}
