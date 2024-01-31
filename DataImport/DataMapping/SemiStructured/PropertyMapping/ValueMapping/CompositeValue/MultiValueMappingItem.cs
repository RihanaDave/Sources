using System;
using System.Collections.Generic;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class MultiValueMappingItem : ValueMappingItem, IResolvableValueMappingItem
    {
        private MultiValueMappingItem()
        {
            MultiValues = new List<SingleValueMappingItem>();
        }
        public MultiValueMappingItem(PropertyInternalResolutionOption resolutionOption = PropertyInternalResolutionOption.Ignorable)
            : this()
        {
            ResolutionOption = resolutionOption;
        }

        public List<SingleValueMappingItem> MultiValues
        {
            get;
            set;
        }

        public PropertyInternalResolutionOption ResolutionOption
        {
            get;
            set;
        }
    }
}
