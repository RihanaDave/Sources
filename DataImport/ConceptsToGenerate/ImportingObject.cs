using System;
using System.Collections;
using System.Collections.Generic;

namespace GPAS.DataImport.ConceptsToGenerate
{
    [Serializable]
    public class ImportingObject : IEqualityComparer, IComparer
    {        
        public ImportingObject()
        {}

        public ImportingObject(string typeUri, ImportingProperty labelProperty)
            : this()
        {
            hashCode = Guid.NewGuid().GetHashCode();
            TypeUri = typeUri;
            LabelProperty = labelProperty;
            Properties.Add(labelProperty);
        }

        public string TypeUri { get; set; }
        //public string DisplayName { get; set; }
        public ImportingProperty LabelProperty { get; set; }
        public List<ImportingProperty> Properties = new List<ImportingProperty>();
        public void AddPropertyForObject(ImportingProperty generatingProperty)
        {
            Properties.Add(generatingProperty);
        }
        public void AddPropertyRangeForObject(IEnumerable<ImportingProperty> generatingProperties)
        {
            Properties.AddRange(generatingProperties);
        }
        public IEnumerable<ImportingProperty> GetProperties()
        {
            return Properties;
        }

        public int hashCode;
        public override int GetHashCode()
        {
            return hashCode;
        }
        public int GetHashCode(object obj)
        {
            if (obj == null || !(obj is ImportingObject))
                return 0;
            else
                return hashCode;
        }

        public new bool Equals(object x, object y)
        {           
            return ReferenceEquals(x, y);
        }

        

        public int Compare(object x, object y)
        {
            if (x == null || !(x is ImportingObject)
                || y == null || !(y is ImportingObject))
            {
                throw new ArgumentException();
            }
            return hashCode - (y as ImportingObject).hashCode;
        }
    }
}
