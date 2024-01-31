using System;

namespace GPAS.DataImport.InternalResolve
{
    public class IRRelationshipObjectBasedEnd : IRRelationshipEnd
    {
        public IRRelationshipObjectBasedEnd(IRObject endObject)
        {
            EndObject = endObject;
        }
        public IRObject EndObject
        {
            get;
            private set;
        }

        public override int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is IRRelationshipObjectBasedEnd))
                throw new ArgumentException();

            return (x is IRRelationshipObjectBasedEnd).CompareTo(y);
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRRelationshipObjectBasedEnd))
                throw new ArgumentException();

            return GetHashCode() - (obj as IRRelationshipObjectBasedEnd).GetHashCode();
        }

        public override int GetHashCode()
        {
            return EndObject.GetHashCode();
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRRelationshipObjectBasedEnd))
                throw new ArgumentException();

            return ((IRRelationshipObjectBasedEnd)obj).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IRRelationshipObjectBasedEnd))
                return false;
            if (GetHashCode() != obj.GetHashCode())
                return false;

            return ReferenceEquals(this.EndObject, (obj as IRRelationshipObjectBasedEnd).EndObject);
        }

        public override bool Equals(object x, object y)
        {
            if (x == null || !(x is IRRelationshipObjectBasedEnd))
                return false;

            return (x is IRRelationshipObjectBasedEnd).Equals(y);
        }
    }
}
