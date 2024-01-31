using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// ساختار چینش گراف؛
    /// این ساختار داخلی، جهت نگهداری موقت چینش گراف -پس از پارز شدن رشته نگهدارنده چینش- استفاده می شود
    /// </summary>
    public class GraphArrangment
    {
        public GraphArrangment()
        {
            Objects = new List<GraphArrangment.ObjectNode>();
            CollapsedGroupsRelativePoistions = new List<GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode>();
            RelationshipBasedLinksExceptGroupInnerLinks = new List<GraphArrangment.RelationshipBasedLink>();
            EventBasedLinks = new List<GraphArrangment.EventBasedLink>();
            PropertyBasedLinks = new List<GraphArrangment.PropertyBasedLink>();
            NotLoadedRelationshipBasedLinks = new List<GraphArrangment.NotLoadedRelationshipBasedLink>();
            NotLoadedEventBasedLinks = new List<GraphArrangment.NotLoadedEventBasedLink>();
        }

        public struct Point
        {
            public double X;
            public double Y;
        }

        public struct ObjectNode
        {
            public long NotResolvedObjectId;
            public Point Position;
            public bool IsVisible;
            public bool IsMasterOfGroup;
            public bool IsMasterOfACollapsedGroup;
        }
        public struct CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode
        {
            public long NotResolvedGroupMasterObjectId;
            public Dictionary<long, Point> GroupMembersRelativePositionsByObjectId;
        }
        public struct RelationshipBasedLink
        {
            public long RelationshipId;
            public long SourceObjectId;
            public long TargetObjectId;
        }
        public struct EventBasedLink
        {
            public long intermediaryEventId;
            public long SourceObjectId;
            public long TargetObjectId;
            public long FirstRelationshipId;
            public long SecondRelationshipId;
        }
        public struct PropertyBasedLink
        {
            public long SourceObjectId;
            public long TargetObjectId;
            public string SamePropertyTypeUri;
            public string SamePropertyValue;
        }
        public struct NotLoadedRelationshipBasedLink
        {
            public long SourceObjectId;
            public long TargetObjectId;
            public long[] RelationshipIDs;
        }
        public struct NotLoadedEventBasedLink
        {
            public long SourceObjectId;
            public long TargetObjectId;
            public NotLoadedEventBasedLinkInnerRelIdPair[] RelationshipIdPairs;
        }
        public struct NotLoadedEventBasedLinkInnerRelIdPair
        {
            public long FirstRelID;
            public long SecondRelID;
        }

        public List<ObjectNode> Objects;
        public List<CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode> CollapsedGroupsRelativePoistions;
        public List<RelationshipBasedLink> RelationshipBasedLinksExceptGroupInnerLinks;
        public List<EventBasedLink> EventBasedLinks;
        public List<PropertyBasedLink> PropertyBasedLinks;
        public List<NotLoadedRelationshipBasedLink> NotLoadedRelationshipBasedLinks;
        public List<NotLoadedEventBasedLink> NotLoadedEventBasedLinks;

        public int GetNodesCount()
        {
            return Objects.Where(o => o.IsVisible).Count();
        }
    }
}
