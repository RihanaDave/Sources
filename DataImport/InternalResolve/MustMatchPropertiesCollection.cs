using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.InternalResolve
{
    internal class MustMatchPropertiesCollection : IEqualityComparer, IComparer, IComparable
    {
        internal MustMatchPropertiesCollection(IEnumerable<IRMustMatchProperty> properties)
        {
            Properties = properties;
            hashCode = IRMustMatchProperty.GenerateMustMatchPropertiesHashCode(properties);
        }
        internal IEnumerable<IRMustMatchProperty> Properties
        {
            get;
            private set;
        }

        private int hashCode;
        public override int GetHashCode()
        {
            return hashCode;
        }
        public int GetHashCode(object obj)
        {
            if (obj == null || !(obj is MustMatchPropertiesCollection))
                return 0;
            return (obj as MustMatchPropertiesCollection).GetHashCode();
        }

        public new bool Equals(object x, object y)
        {
            if (x == null || !(x is MustMatchPropertiesCollection))
            {
                return false;
            }
            return (x as MustMatchPropertiesCollection).Equals(y);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MustMatchPropertiesCollection))
            {
                return false;
            }
            return IRMustMatchProperty.AreMustMatchPropertiesEqual(this.Properties, (obj as MustMatchPropertiesCollection).Properties);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if(!(x is MustMatchPropertiesCollection))
                throw new ArgumentException();
            return (x as MustMatchPropertiesCollection).CompareTo(y);
        }
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if(!(obj is MustMatchPropertiesCollection))
                throw new ArgumentException();
            return hashCode - (obj as MustMatchPropertiesCollection).hashCode;
        }
    }
}