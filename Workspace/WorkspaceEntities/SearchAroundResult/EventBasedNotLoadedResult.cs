using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class EventBasedNotLoadedResult
    {
        public EventBasedNotLoadedResult()
        {
            InnerRelationships = new List<EventBasedResultInnerRelationships>();
        }
        public List<EventBasedResultInnerRelationships> InnerRelationships { set; get; }
        public long TargetObjectID { set; get; }
    }
}