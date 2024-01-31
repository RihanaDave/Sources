using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class EventBasedResult
    {
        public bool IsResultsCountMoreThanThreshold { set; get; }

        public List<EventBasedResultsPerSearchedObjects> Results { set; get; }

    }
}
