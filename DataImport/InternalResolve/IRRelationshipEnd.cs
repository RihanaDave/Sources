using System;
using System.Collections;

namespace GPAS.DataImport.InternalResolve
{
    public abstract class IRRelationshipEnd : IEqualityComparer, IComparable, IComparer
    {
        public abstract int Compare(object x, object y);
        public abstract new bool Equals(object x, object y);
        public abstract int GetHashCode(object obj);

        public abstract int CompareTo(object obj);
        public override abstract bool Equals(object obj);
        public override abstract int GetHashCode();
    }
}