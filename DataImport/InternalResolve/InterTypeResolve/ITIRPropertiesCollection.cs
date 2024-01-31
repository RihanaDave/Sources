using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.InternalResolve.InterTypeResolve
{
    internal class ITIRPropertiesCollection : IEqualityComparer, IComparer, IComparable
    {
        internal ITIRPropertiesCollection(IEnumerable<ITIRProperty> properties)
        {
            Properties = properties;
            hashCode = ITIRProperty.GenerateITIRPropertiesHashCode(properties);
        }
        internal IEnumerable<ITIRProperty> Properties
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
            if (obj == null || !(obj is ITIRPropertiesCollection))
                return 0;
            return (obj as ITIRPropertiesCollection).GetHashCode();
        }

        public new bool Equals(object x, object y)
        {
            if (x == null || !(x is ITIRPropertiesCollection))
            {
                return false;
            }
            return (x as ITIRPropertiesCollection).Equals(y);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ITIRPropertiesCollection))
            {
                return false;
            }
            return ITIRProperty.AreITIRPropertiesEqual(this.Properties, (obj as ITIRPropertiesCollection).Properties);
        }

        public int Compare(object x, object y)
        {
            if (x == null)
                throw new ArgumentNullException();
            if (!(x is ITIRPropertiesCollection))
                throw new ArgumentException();
            return (x as ITIRPropertiesCollection).CompareTo(y);
        }
        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            if (!(obj is ITIRPropertiesCollection))
                throw new ArgumentException();
            return hashCode - (obj as ITIRPropertiesCollection).hashCode;
        }
    }
}
