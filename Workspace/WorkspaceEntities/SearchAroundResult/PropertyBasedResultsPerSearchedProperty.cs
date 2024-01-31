using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class PropertyBasedResultsPerSearchedProperty
    {
        public KWProperty SearchedProperty { set; get; }

        public List<PropertyBasedKWLink> LoadedResults { set; get; }

        public List<long> NotLoadedResultPropertyIDs { set; get; }
    }
}