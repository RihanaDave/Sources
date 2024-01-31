using System;

namespace GPAS.SearchAround.DataMapping
{
    [Serializable]
    public class LinkMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده است
        /// </summary>
        private LinkMapping()
        { }

        public LinkMapping(ObjectMapping source, ObjectMapping target, OntologyTypeMappingItem linkType)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (linkType == null)
                throw new ArgumentNullException(nameof(linkType));

            Source = source;
            Target = target;
            LinkDirection = LinkMappingDirection.Bidirectional;
            LinkType = linkType;

            ProcessHashCode();
        }

        private void ProcessHashCode()
        {
            hashCode = Guid.NewGuid().GetHashCode();
        }

        private int hashCode = 0;
        public override int GetHashCode()
        {
            if (hashCode == 0)
                ProcessHashCode();
            return hashCode;
        }

        public LinkMappingDirection LinkDirection
        {
            get;
            set;
        }

        public OntologyTypeMappingItem LinkType
        {
            get;
            set;
        }

        public ObjectMapping Source
        {
            get;
            set;
        }

        public ObjectMapping Target
        {
            get;
            set;
        }
    }
}
