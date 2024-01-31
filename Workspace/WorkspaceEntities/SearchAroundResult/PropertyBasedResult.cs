using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class PropertyBasedResult
    {
        public bool IsResultsCountMoreThanThreshold { set; get; }

        public List<PropertyBasedResultsPerSearchedProperty> Results { set; get; }

    }
}
