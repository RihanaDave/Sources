using System;
using System.Collections;
using System.Collections.Generic;

namespace GPAS.DataImport.InternalResolve
{
    public class IRObject : IEqualityComparer, IComparer
    {
        public IRObject(List<IRMustMatchProperty> findMatchProperties, List<IRMustMatchProperty> nonFindMatchMustMatchProperties)
        {
            TypeUri = "";
            DisplayName = "";
            //FindMatchProperties = findMatchProperties;
            //NonFindMatchMustMatchProperties = nonFindMatchMustMatchProperties;
            MustMatchProperties = new IRMustMatchProperty[findMatchProperties.Count + nonFindMatchMustMatchProperties.Count];
            for (int i = 0; i < findMatchProperties.Count; i++)
            {
                MustMatchProperties[i] = findMatchProperties[i];
            }
            for (int i = 0; i < nonFindMatchMustMatchProperties.Count; i++)
            {
                MustMatchProperties[findMatchProperties.Count + i] = nonFindMatchMustMatchProperties[i];
            }
            //MustMatchPropertiesHashCode = IRMustMatchProperty.GenerateMustMatchPropertiesHashCode(MustMatchProperties);
            hashCode = Guid.NewGuid().GetHashCode();
        }

        //public List<IRMustMatchProperty> FindMatchProperties
        //{ get; }
        //public List<IRMustMatchProperty> NonFindMatchMustMatchProperties
        //{ get; }
        public List<IRIgnorableProperty> IgnorableProperties;
        public string TypeUri;
        public string DisplayName;

        public IRMustMatchProperty[] MustMatchProperties
        { get; private set; }
        //public int MustMatchPropertiesHashCode
        //{
        //    get;
        //    private set;
        //}
        
        private int hashCode;
        public override int GetHashCode()
        {
            return hashCode;
        }
        public int GetHashCode(object obj)
        {
            if (obj == null || !(obj is IRObject))
                return 0;
            else
                return (obj as IRObject).GetHashCode();
        }

        public new bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        public int Compare(object x, object y)
        {
            if (x == null || !(x is IRObject)
                || y == null || !(y is IRObject))
            {
                throw new ArgumentException();
            }
            return hashCode - (y as IRObject).hashCode;
        }
    }
}