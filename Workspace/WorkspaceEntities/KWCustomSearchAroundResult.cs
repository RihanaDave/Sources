using GPAS.Workspace.Entities.SearchAroundResult;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities
{
    public class KWCustomSearchAroundResult
    {
        public bool IsResultsCountMoreThanThreshold { get; set; }
        public List<EventBasedResultsPerSearchedObjects> EventBasedResult { get; set; }
        public List<RelationshipBasedResultsPerSearchedObjects> RalationshipBasedResult { get; set; }
    }
}
