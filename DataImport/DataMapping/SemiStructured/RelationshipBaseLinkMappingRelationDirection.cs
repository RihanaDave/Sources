using System.Runtime.Serialization;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [DataContract]
    public enum RelationshipBaseLinkMappingRelationDirection
    {
        PrimaryToSecondary,
        SecondaryToPrimary,
        Bidirectional
    }
}
