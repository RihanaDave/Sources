using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.Unstructured
{
    public class RelationshipMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده است
        /// </summary>
        private RelationshipMapping()
        { }

        public RelationshipMapping(ObjectMapping source, ObjectMapping target, RelationshipBaseLinkMappingRelationDirection relationshipDirection, string relationshipDescription, string relationshipType)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (relationshipDescription == null)
                throw new ArgumentNullException("relationshipDescription");
            if (relationshipType == null)
                throw new ArgumentNullException("relationshipType");
            //if (relationshipTimeBegin == null)
            //    throw new ArgumentNullException("timeBegin");
            //if (relationshipTimeEnd == null)
            //    throw new ArgumentNullException("timeEnd");

            SourceId = source.ID;
            TargetId = target.ID;
            RelationshipDirection = relationshipDirection;
            RelationshipDescription = relationshipDescription;
            RelationshipType = relationshipType;
            //RelationshipTimeBegin = relationshipTimeBegin;
            //RelationshipTimeEnd = relationshipTimeEnd;

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

        public RelationshipBaseLinkMappingRelationDirection RelationshipDirection
        {
            get;
            set;
        }

        public string RelationshipDescription
        {
            get;
            set;
        }

        public string RelationshipType
        {
            get;
            set;
        }

        //public ValueMappingItem RelationshipTimeBegin
        //{
        //    get;
        //    set;
        //}

        //public ValueMappingItem RelationshipTimeEnd
        //{
        //    get;
        //    set;
        //}

        public string SourceId
        {
            get;
            set;
        }

        public string TargetId
        {
            get;
            set;
        }
    }
}
