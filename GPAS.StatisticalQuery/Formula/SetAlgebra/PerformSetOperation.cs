using GPAS.StatisticalQuery.ObjectSet;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.SetAlgebra
{
    [DataContract]
    public class PerformSetOperation : FormulaStep
    {
        public PerformSetOperation()
        {

        }

        [DataMember]
        public Operator Operator { get; set; }
        [DataMember]
        public StartingObjectSet JoinedSetStartPoint { get; set; }
        [DataMember]
        public List<FormulaStep> JoinedSetFormulaSequence { get; set; }
    }
}
