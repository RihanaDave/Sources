using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class EventBasedResultInnerRelationships : IEquatable<EventBasedResultInnerRelationships>
    {
        public long FirstRelationshipID { set; get; }
        public long SecondRelationshipID { set; get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as EventBasedResultInnerRelationships);
        }

        public bool Equals(EventBasedResultInnerRelationships other)
        {
            return other != null && (
                FirstRelationshipID == other.FirstRelationshipID && SecondRelationshipID == other.SecondRelationshipID ||
                FirstRelationshipID == other.SecondRelationshipID && SecondRelationshipID == other.FirstRelationshipID);
        }

        public override int GetHashCode()
        {
            var hashCode = FirstRelationshipID.GetHashCode() ^ SecondRelationshipID.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(EventBasedResultInnerRelationships relationships1, EventBasedResultInnerRelationships relationships2)
        {
            return EqualityComparer<EventBasedResultInnerRelationships>.Default.Equals(relationships1, relationships2);
        }

        public static bool operator !=(EventBasedResultInnerRelationships relationships1, EventBasedResultInnerRelationships relationships2)
        {
            return !(relationships1 == relationships2);
        }
    }
}
