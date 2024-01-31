using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class RelationshipBasedResult
    {
        public RelationshipBasedResult()
        {
            Results = new List<RelationshipBasedResultsPerSearchedObjects>();
        }
        public bool IsResultsCountMoreThanThreshold { set; get; }

        public List<RelationshipBasedResultsPerSearchedObjects> Results { set; get; }
    }
}
