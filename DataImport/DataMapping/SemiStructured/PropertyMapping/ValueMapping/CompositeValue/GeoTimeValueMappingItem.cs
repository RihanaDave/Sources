using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class GeoTimeValueMappingItem : ValueMappingItem, IResolvableValueMappingItem
    {
        public GeoTimeValueMappingItem()
        { }

        public SingleValueMappingItem Latitude { get; set; }
        public SingleValueMappingItem Longitude { get; set; }
        public SingleValueMappingItem TimeBegin { get; set; }
        public SingleValueMappingItem TimeEnd { get; set; }
        
        public PropertyInternalResolutionOption ResolutionOption
        {
            get;
            set;
        }
    }
}