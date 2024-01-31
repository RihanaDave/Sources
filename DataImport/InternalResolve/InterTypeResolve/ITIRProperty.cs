using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.InternalResolve.InterTypeResolve
{
    internal struct ITIRProperty : IEqualityComparer, IComparable, IComparer
    {
        public ITIRProperty(string typeUri, string value)
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
            if (!(obj is ITIRProperty))
                throw new ArgumentException();

            return ((ITIRProperty)obj).hashCode;
        }
        public new bool Equals(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is ITIRProperty))
                throw new ArgumentException();

            return ((ITIRProperty)x).Equals(y);
        }
        public int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is ITIRProperty))
                throw new ArgumentException();

            return ((ITIRProperty)x).CompareTo(y);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ITIRProperty))
                return false;
            if (hashCode != ((ITIRProperty)obj).hashCode)
                return false;
            if (TypeURI.Equals(((ITIRProperty)obj).TypeURI, StringComparison.InvariantCulture)
                && Value.ToLowerInvariant().Equals(((ITIRProperty)obj).Value.ToLowerInvariant(), StringComparison.InvariantCulture))
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
            if (!(obj is ITIRProperty))
                throw new ArgumentException();

            return hashCode - ((ITIRProperty)obj).hashCode;
        }

        public static int GenerateITIRPropertiesHashCode(IEnumerable<ITIRProperty> mustMatchProperties)
        {
            int hashCode = 0;
            foreach (var item in mustMatchProperties)
            {
                hashCode = hashCode ^ item.GetHashCode();
            }
            return hashCode;
        }

        public static bool AreITIRPropertiesEqual(IEnumerable<ITIRProperty> properties1, IEnumerable<ITIRProperty> properties2)
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
