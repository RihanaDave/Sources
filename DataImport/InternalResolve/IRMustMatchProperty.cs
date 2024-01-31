using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.InternalResolve
{
    public struct IRMustMatchProperty : IEqualityComparer, IComparable, IComparer
    {
        public IRMustMatchProperty(string typeUri, string value)
        {
            TypeURI = typeUri;
            Value = value;
            hashCode = typeUri.GetHashCode() ^ value.ToLowerInvariant().GetHashCode();
        }

        public string TypeURI
        { get; }
        public string Value
        { get; }

        private int hashCode;

        public int GetHashCode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRMustMatchProperty))
                throw new ArgumentException();

            return ((IRMustMatchProperty)obj).hashCode;
        }
        public new bool Equals(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is IRMustMatchProperty))
                throw new ArgumentException();

            return ((IRMustMatchProperty)x).Equals(y);
        }
        public int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is IRMustMatchProperty))
                throw new ArgumentException();

            return ((IRMustMatchProperty)x).CompareTo(y);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IRMustMatchProperty))
                return false;
            if (hashCode != ((IRMustMatchProperty)obj).hashCode)
                return false;
            if (TypeURI.Equals(((IRMustMatchProperty)obj).TypeURI, StringComparison.InvariantCulture)
                && Value.ToLowerInvariant().Equals(((IRMustMatchProperty)obj).Value.ToLowerInvariant(), StringComparison.InvariantCulture))
            {
                return true;
            }
            else
                return false;
        }
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is IRMustMatchProperty))
                throw new ArgumentException();

            return hashCode - ((IRMustMatchProperty)obj).hashCode;
        }
        
        public static int GenerateMustMatchPropertiesHashCode(IEnumerable<IRMustMatchProperty> mustMatchProperties)
        {
            int hashCode = 0;
            foreach (var item in mustMatchProperties)
            {
                hashCode = hashCode ^ item.GetHashCode();
            }
            return hashCode;
        }

        public static bool AreMustMatchPropertiesEqual(IEnumerable<IRMustMatchProperty> properties1, IEnumerable<IRMustMatchProperty> properties2)
        {
            foreach (var objProperty in properties1)
            {
                if (string.IsNullOrWhiteSpace(objProperty.Value))
                    return false;

                bool properties2ContainsOnlyOneFullyEquivalentForCurrentProperty =
                    properties2
                        .Where(mmp => mmp.Equals(objProperty))
                        .Count() == 1;
                if (!properties2ContainsOnlyOneFullyEquivalentForCurrentProperty)
                    return false;
            }
            return true;
        }
    }
}
