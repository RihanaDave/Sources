using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class RelationshipBasedResult
    {
        [DataMember]
        public bool IsResultsCountMoreThanThreshold { set; get; }

        [DataMember]
        public RelationshipBasedResultsPerSearchedObjects[] Results { set; get; }
    }
}
