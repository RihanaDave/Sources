using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround
{
    [DataContract]
    public class CustomSearchAroundResult
    {
        [DataMember]
        public bool IsResultsCountMoreThanThreshold { get; set; }
        [DataMember]
        public  EventBasedResultsPerSearchedObjects[] EventBaseKLink { get; set; }
        [DataMember]
        public RelationshipBasedResultsPerSearchedObjects[] Ralationships { get; set; }
    }
}
