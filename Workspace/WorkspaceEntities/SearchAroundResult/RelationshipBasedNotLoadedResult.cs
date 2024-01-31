using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class RelationshipBasedNotLoadedResult
    {
        public RelationshipBasedNotLoadedResult()
        {
            RelationshipIDs = new List<long>();
        }
        public List<long> RelationshipIDs { set; get; }
        public long TargetObjectID { set; get; }
    }
}