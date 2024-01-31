using System.Collections;

namespace GPAS.DataImport.InternalResolve
{
    public struct IRRelationship : IEqualityComparer
    {
        public string TypeURI;
        public IRRelationshipDirection Direction;
        public IRRelationshipEnd SourceEnd;
        public IRRelationshipEnd TargetEnd;
        public string Description;

        public override int GetHashCode()
        {
            return TypeURI.GetHashCode()
                ^ SourceEnd.GetHashCode()
                ^ TargetEnd.GetHashCode()
                ^ Direction.GetHashCode()
                ^ Description.GetHashCode();
        }
        public int GetHashCode(object obj)
        {
            if (obj == null || !(obj is IRRelationship))
                return 0;
            else
                return ((IRRelationship)obj).GetHashCode();
        }

        public new bool Equals(object x, object y)
        {
            if (x == null || !(x is IRRelationship))
                return false;
            if (y == null || !(y is IRRelationship))
                return false;
            return ((IRRelationship)x).TypeURI.Equals(((IRRelationship)y).TypeURI)
                && ((IRRelationship)x).SourceEnd.Equals(((IRRelationship)y).SourceEnd)
                && ((IRRelationship)x).TargetEnd.Equals(((IRRelationship)y).TargetEnd)
                && ((IRRelationship)x).Direction.Equals(((IRRelationship)y).Direction)
                && ((IRRelationship)x).Description.Equals(((IRRelationship)y).Description);
        }
    }
}
