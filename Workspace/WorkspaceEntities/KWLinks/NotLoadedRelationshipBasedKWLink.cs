using System;
using System.Collections.Generic;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Entities
{
    public class NotLoadedRelationshipBasedKWLink : CompoundKWLink
    {
        public static string BaseRelationshipTypeUri = string.Empty;

        public override KWObject Source { get; set; }
        public override KWObject Target { get; set; }

        public HashSet<long> IntermediaryRelationshipIDs { get; set; }

        public override LinkDirection LinkDirection => LinkDirection.Bidirectional;
        public override string Text => $"{IntermediaryRelationshipIDs.Count} relationship(s)";
        public override string TypeURI => BaseRelationshipTypeUri;
        public override bool IsUnmergable => true;

        public override bool ContainsLink(KWLink kwlinkToCompair)
        {
            if(kwlinkToCompair is RelationshipBasedKWLink
                && IntermediaryRelationshipIDs.Contains((kwlinkToCompair as RelationshipBasedKWLink).Relationship.ID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override HashSet<long> GetAllLinks()
        {
            return IntermediaryRelationshipIDs;
        }

        public override bool Equals(object x)
        {
            if (x == null || !(x is NotLoadedRelationshipBasedKWLink))
                return false;

            return this.Equals(x as NotLoadedRelationshipBasedKWLink);
        }

        public bool Equals(KWLink compairLink)
        {
            if (!(compairLink is NotLoadedRelationshipBasedKWLink))
                return false;
            return
                (Source.Equals((compairLink as NotLoadedRelationshipBasedKWLink).Source)
                    && Target.Equals((compairLink as NotLoadedRelationshipBasedKWLink).Target))
                || Source.Equals((compairLink as NotLoadedRelationshipBasedKWLink).Target)
                    && Target.Equals((compairLink as NotLoadedRelationshipBasedKWLink).Source);
        }

        public override int GetHashCode()
        {
            return (Source.ID ^ Target.ID).GetHashCode();
        }
    }
}
