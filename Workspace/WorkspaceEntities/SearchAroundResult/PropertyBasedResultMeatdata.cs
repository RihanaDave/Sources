using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class PropertyBasedResultMeatdata
    {
        public bool IsResultsCountMoreThanThreshold { set; get; }

        public Dictionary<long, PropertyBasedResultMetadatasPerSearchedProperty> ResultsPerSearchedPropertyID { set; get; }

    }
}
