using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class PropertyBasedResultMetadatasPerSearchedProperty
    {
        public KWProperty SearchedProperty { set; get; }

        public List<KWProperty> LoadedResults { set; get; }

        public List<long> NotLoadedResultPropertyIDs { set; get; }
    }
}