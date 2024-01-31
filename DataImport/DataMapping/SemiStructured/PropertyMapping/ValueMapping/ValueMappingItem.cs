using System;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    /// <summary>
    /// کلاس انتزاعی «اقلام نگاشتی» که مقداری را نشان می‌دهند، براساس داده‌های نیم‌ساختیافته
    /// </summary>
    [Serializable]
    public abstract class ValueMappingItem : MappingNodeItem
    {
        public bool IsFullyConstMapping()
        {
            if (this is ConstValueMappingItem)
            {
                return true;
            }
            else if (this is GeoTimeValueMappingItem)
            {
                var geoTimeValueMapping = this as GeoTimeValueMappingItem;
                if (geoTimeValueMapping.Latitude.IsFullyConstMapping()
                && geoTimeValueMapping.Longitude.IsFullyConstMapping()
                && geoTimeValueMapping.TimeBegin.IsFullyConstMapping()
                && geoTimeValueMapping.TimeEnd.IsFullyConstMapping())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (this is MultiValueMappingItem)
            {
                var multiValueMapping = this as MultiValueMappingItem;
                if (multiValueMapping.MultiValues.Count < 1)
                {
                    return false;
                }
                foreach (ValueMappingItem subValueMapping in multiValueMapping.MultiValues)
                {
                    if (!subValueMapping.IsFullyConstMapping())
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}