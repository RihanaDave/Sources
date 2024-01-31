using GPAS.Workspace.Entities.SearchAroundResult;
using System;
using System.Collections.Generic;
using System.Linq;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Entities
{
    public class NotLoadedEventBasedKWLink : CompoundKWLink
    {
        public static string BaseEventTypeUri = string.Empty;

        public override KWObject Source { get; set; }
        public override KWObject Target { get; set; }

        public HashSet<EventBasedResultInnerRelationships> IntermediaryLinksRelationshipIDs { get; set; }

        public override LinkDirection LinkDirection => LinkDirection.Bidirectional;
        public override string Text => $"{IntermediaryLinksRelationshipIDs.Count} event(s)";
        public override string TypeURI => BaseEventTypeUri;
        public override bool IsUnmergable => true;

        public override bool ContainsLink(KWLink kwlinkToCompair)
        {
            if (kwlinkToCompair is RelationshipBasedKWLink)
            {
                long compairingRelID = (kwlinkToCompair as RelationshipBasedKWLink).Relationship.ID;
                return IntermediaryLinksRelationshipIDs.Any
                    (innerRelIDs
                        => innerRelIDs.FirstRelationshipID.Equals(compairingRelID)
                        || innerRelIDs.SecondRelationshipID.Equals(compairingRelID));
            }
            else if (kwlinkToCompair is EventBasedKWLink)
            {
                EventBasedKWLink linkToCompair = kwlinkToCompair as EventBasedKWLink;
                EventBasedResultInnerRelationships l = new EventBasedResultInnerRelationships()
                {
                    FirstRelationshipID = linkToCompair.FirstRelationship.ID,
                    SecondRelationshipID = linkToCompair.SecondRelationship.ID
                };

                return IntermediaryLinksRelationshipIDs.Contains(l);
            }
            return false;
        }

        public override HashSet<long> GetAllLinks()
        {
            HashSet<long> result = new HashSet<long>(IntermediaryLinksRelationshipIDs.Select(i => i.FirstRelationshipID)
                    .Concat(IntermediaryLinksRelationshipIDs.Select(i => i.SecondRelationshipID)));

            return result;
        }

        public List<EventBasedResultInnerRelationships> GetIntermediaryLinksByID(long relationshipID)
        {
            List<EventBasedResultInnerRelationships> result = new List<EventBasedResultInnerRelationships>();

            result = IntermediaryLinksRelationshipIDs.Where(i => i.FirstRelationshipID == relationshipID || i.SecondRelationshipID == relationshipID).ToList();
            return result;
        }
        public List<EventBasedResultInnerRelationships> GetIntermediaryLinksByID(long relationshipID1, long relationshipID2)
        {
            List<EventBasedResultInnerRelationships> result = new List<EventBasedResultInnerRelationships>();
            EventBasedResultInnerRelationships l = new EventBasedResultInnerRelationships()
            {
                FirstRelationshipID = relationshipID1,
                SecondRelationshipID = relationshipID2
            };

            result = IntermediaryLinksRelationshipIDs.Where(i => i == l).ToList();
            return result;
        }

        public override bool Equals(object x)
        {
            if (x == null || !(x is NotLoadedEventBasedKWLink))
                return false;

            return this.Equals(x as NotLoadedEventBasedKWLink);
        }

        public bool Equals(KWLink compairLink)
        {
            if (!(compairLink is NotLoadedEventBasedKWLink))
                return false;
            return
                (Source.Equals((compairLink as NotLoadedEventBasedKWLink).Source)
                    && Target.Equals((compairLink as NotLoadedEventBasedKWLink).Target))
                || Source.Equals((compairLink as NotLoadedEventBasedKWLink).Target)
                    && Target.Equals((compairLink as NotLoadedEventBasedKWLink).Source);
        }

        public override int GetHashCode()
        {
            return (Source.ID ^ Target.ID).GetHashCode();
        }
    }
}
