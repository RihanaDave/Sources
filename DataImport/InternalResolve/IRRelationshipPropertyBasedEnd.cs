using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImport.InternalResolve
{
    public class IRRelationshipPropertyBasedEnd : IRRelationshipEnd
    {
        public IRRelationshipPropertyBasedEnd(IEnumerable<IRMustMatchProperty> endObjectMustMatchProperties)
        {
            EndObjectMustMatchProperties = endObjectMustMatchProperties;
            hashCode = IRMustMatchProperty.GenerateMustMatchPropertiesHashCode(endObjectMustMatchProperties);
        }
        public IEnumerable<IRMustMatchProperty> EndObjectMustMatchProperties
        {
            get;
            private set;
        }
        private int hashCode;

        public override int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is IRRelationshipPropertyBasedEnd))
                throw new ArgumentException();

            return (x is IRRelationshipPropertyBasedEnd).CompareTo(y);
        }
        public override int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRRelationshipPropertyBasedEnd))
                throw new ArgumentException();

            return GetHashCode() - (obj as IRRelationshipPropertyBasedEnd).GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRRelationshipPropertyBasedEnd))
                throw new ArgumentException();

            return ((IRRelationshipPropertyBasedEnd)obj).GetHashCode();
        }

        private bool ArePropertiesEquals(IRRelationshipPropertyBasedEnd propEnd)
        {
            if (GetHashCode() != propEnd.GetHashCode())
                return false;
            if (!EndObjectMustMatchProperties.Any())
                return false;

            return IRMustMatchProperty.AreMustMatchPropertiesEqual(EndObjectMustMatchProperties, propEnd.EndObjectMustMatchProperties);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IRRelationshipPropertyBasedEnd))
                return false;

            return ArePropertiesEquals(obj as IRRelationshipPropertyBasedEnd);
        }

        public override bool Equals(object x, object y)
        {
            if (x == null || !(x is IRRelationshipPropertyBasedEnd))
                return false;

            return (x is IRRelationshipPropertyBasedEnd).Equals(y);
        }
    }
}
