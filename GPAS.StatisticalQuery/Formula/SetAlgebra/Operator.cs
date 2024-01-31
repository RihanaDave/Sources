using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.SetAlgebra
{
    [DataContract]
    public enum Operator
    {
        Unknown,
        Union,
        Intersection,
        SubtractRight,
        SubtractLeft,
        ExclusiveOr
    }
}
