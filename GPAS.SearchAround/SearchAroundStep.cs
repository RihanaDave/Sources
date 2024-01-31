using GPAS.FilterSearch;
using System.Runtime.Serialization;
using System;

namespace GPAS.SearchAround
{
    [DataContract]
    public class SearchAroundStep
    {
        public SearchAroundStep()
        {
            // یک شناسه‌ی جدید اختصاص داده می‌شود که برای ایجاد گام جدید (با 
            // شناسه‌ي جدید کاربرد دارد) در عین حال در صورت دی‌سریال شدن کلاس
            // مقدار دی‌سریال شده در این شناسه بازنویسی می‌شود که مطلوب است
            GUID = Guid.NewGuid();
        }

        [DataMember]
        public string[] LinkTypeUri { get; set; } = null;

        [DataMember]
        public bool IsEvent { get; set; } = false;

        [DataMember]
        public string[] TargetObjectTypeUri { get; set; } = null;

        [DataMember]
        public PropertyValueCriteria[] TargetObjectPropertyCriterias { get; set; } = { };

        [DataMember]
        public PropertyValueCriteria[] EventObjectPropertyCriterias { get; set; } = { };

        [DataMember]
        public Guid GUID { get; set; }
    }
}
