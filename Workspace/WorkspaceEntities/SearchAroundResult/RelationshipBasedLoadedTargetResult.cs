using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
   public class RelationshipBasedLoadedTargetResult
    {
        public RelationshipBasedLoadedTargetResult()
        {
            RelationshipIDs = new List<long>();
        }
        public List<long> RelationshipIDs { set; get; }

        public KWObject TargetObject { set; get; }
    }
}
