using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class RelationshipBasedResultsPerSearchedObjects
    {
        public RelationshipBasedResultsPerSearchedObjects()
        {
            LoadedResults = new List<RelationshipBasedLoadedTargetResult>();
            NotLoadedResults = new List<RelationshipBasedNotLoadedResult>();
        }
        public KWObject SearchedObject { set; get; }

        public List<RelationshipBasedLoadedTargetResult> LoadedResults { set; get; }

        public List<RelationshipBasedNotLoadedResult> NotLoadedResults { set; get; }
    }
}
