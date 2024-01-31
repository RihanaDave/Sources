using GPAS.FilterSearch;
using System;
using System.Runtime.Serialization;

namespace GPAS.SearchAround
{
    [DataContract]
    public class CustomSearchAroundCriteria
    {
        [DataMember]
        public SearchAroundStep[] LinksFromSearchSet { get; set; } = { };

        [DataMember]
        public string[] SourceSetObjectTypes { get; set; } = { };

        public bool IsValid()
        {
            if (LinksFromSearchSet.Length == 0)
                return false;
            if (SourceSetObjectTypes.Length == 0)
                return false;

            return true;
        }

        public static PropertyValueCriteria[] GetStepTargetPropertyFilter(CustomSearchAroundCriteria criteria, Guid stepGuid)
        {
            foreach (SearchAroundStep step in criteria.LinksFromSearchSet)
            {
                if(step.GUID == stepGuid)
                {
                    return step.TargetObjectPropertyCriterias;
                }
            }
            throw new ArgumentException("Given GUID not match to any step of crteria");
        }

        public static SearchAroundStep GetStepByGuid(CustomSearchAroundCriteria criteria, Guid stepGuid)
        {
            foreach (SearchAroundStep step in criteria.LinksFromSearchSet)
            {
                if (step.GUID == stepGuid)
                {
                    return step;
                }
            }
            throw new ArgumentException("Given GUID not match to any step of crteria");
        }
    }
}
