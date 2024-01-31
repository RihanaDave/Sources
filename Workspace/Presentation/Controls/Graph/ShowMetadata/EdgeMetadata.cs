using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    /// <summary>
    /// فراداده و توابع مورد نیاز برای نمایش تمام لینک‌های بین دو شئ؛
    /// هر نمونه‌ی این کلاس متناظر با یک یال در گراف است
    /// </summary>
    public class EdgeMetadata
    {
        public EdgeMetadata(KWObject from, KWObject to)
        {
            From = from;
            To = to;
            RelationshipBasedLinks = new HashSet<RelationshipBasedKWLink>();
            EventBasedLinks = new HashSet<EventBasedKWLink>();
            PropertyBasedLinks = new List<PropertyBasedKWLink>();
            NLRelationshipBasedLinkIDs = new HashSet<long>();
            NLEventBasedLinkInnerIdPairs = new HashSet<EventBasedResultInnerRelationships>();
            NLEventBasedLinkInnerIdPairsPerInnerRelID = new Dictionary<long, HashSet<EventBasedResultInnerRelationships>>();
            RelatedEdge = null;
            IsShownOnGraph = false;
        }

        public KWObject From { get; private set; }
        public KWObject To { get; private set; }
        public HashSet<RelationshipBasedKWLink> RelationshipBasedLinks { get; }
        public HashSet<EventBasedKWLink> EventBasedLinks { get; private set; }
        public List<PropertyBasedKWLink> PropertyBasedLinks { get; private set; }
        public HashSet<long> NLRelationshipBasedLinkIDs { get; private set; }
        public NotLoadedRelationshipBasedKWLink GetNotLoadedRelationshipBasedKWLink()
        {
            return new NotLoadedRelationshipBasedKWLink()
            {
                Source = From,
                Target = To,
                IntermediaryRelationshipIDs = NLRelationshipBasedLinkIDs
            };
        }

        #region Not-Loaded Event-Based links data structure and functions
        /// <summary>
        /// زوج-رابطه‌های داخلی تشکیل‌دهنده‌ی لینک‌های مبتنی بر رخداد لود نشده
        /// این ساختمان‌داده برای نگهداری تمامی لینک‌های مبتنی بر رخدادی لود نشده و همچنین بازیابی بهینه این لینک‌ها استفاده می‌شود
        /// </summary>
        private HashSet<EventBasedResultInnerRelationships> NLEventBasedLinkInnerIdPairs { get; set; }
        /// <summary>
        /// شناسه‌ی رابطه‌های داخلی تشکیل‌دهنده‌ی لینک‌های مبتنی بر رخداد  لود نشده و لینک‌های متناظر با هر یک از آن‌ها
        /// این ساختمان‌داده علارغم ایجاد افزونگی، برای بهینه‌سازی بازیابی‌های مبتنی بر روابط تشکیل دهنده، تعبیه شده است
        /// </summary>
        private Dictionary<long, HashSet<EventBasedResultInnerRelationships>> NLEventBasedLinkInnerIdPairsPerInnerRelID { get; set; }
        public void AddNewNLEventBasedLinkInnerIDPairIfNotExist(EventBasedResultInnerRelationships newPair)
        {
            if (!NLEventBasedLinkInnerIdPairs.Contains(newPair))
                NLEventBasedLinkInnerIdPairs.Add(newPair);
            AddRelationshipIdToNLEventBasedLinkInnerIdPairsPerInnerRelID(newPair.FirstRelationshipID, newPair);
            AddRelationshipIdToNLEventBasedLinkInnerIdPairsPerInnerRelID(newPair.SecondRelationshipID, newPair);
        }
        private void AddRelationshipIdToNLEventBasedLinkInnerIdPairsPerInnerRelID(long innerRelID, EventBasedResultInnerRelationships hostPair)
        {
            if (!NLEventBasedLinkInnerIdPairsPerInnerRelID.ContainsKey(innerRelID))
                NLEventBasedLinkInnerIdPairsPerInnerRelID.Add(innerRelID, new HashSet<EventBasedResultInnerRelationships>());
            HashSet<EventBasedResultInnerRelationships> innerPairsPerRelID = NLEventBasedLinkInnerIdPairsPerInnerRelID[innerRelID];
            if (!innerPairsPerRelID.Contains(hostPair))
                innerPairsPerRelID.Add(hostPair);
        }

        public bool TryGetNLEventBasedLinkInnerIDPairsThatContainsRel(long relID, out HashSet<EventBasedResultInnerRelationships> relatedParis)
        {
            return NLEventBasedLinkInnerIdPairsPerInnerRelID.TryGetValue(relID, out relatedParis);
        }

        public void RemoveNLEventBasedLinkInnerIDPairIfExist(EventBasedResultInnerRelationships newPair)
        {
            if (NLEventBasedLinkInnerIdPairs.Contains(newPair))
                NLEventBasedLinkInnerIdPairs.Remove(newPair);
            RemoveRelationshipIdFromNLEventBasedLinkInnerIdPairsPerInnerRelID(newPair.FirstRelationshipID, newPair);
            RemoveRelationshipIdFromNLEventBasedLinkInnerIdPairsPerInnerRelID(newPair.SecondRelationshipID, newPair);
        }
        private void RemoveRelationshipIdFromNLEventBasedLinkInnerIdPairsPerInnerRelID(long innerRelID, EventBasedResultInnerRelationships hostPair)
        {
            if (NLEventBasedLinkInnerIdPairsPerInnerRelID.ContainsKey(innerRelID))
            {
                HashSet<EventBasedResultInnerRelationships> innerPairsPerRelID = NLEventBasedLinkInnerIdPairsPerInnerRelID[innerRelID];
                if (innerPairsPerRelID.Contains(hostPair))
                    innerPairsPerRelID.Remove(hostPair);
                if (innerPairsPerRelID.Count == 0)
                    NLEventBasedLinkInnerIdPairsPerInnerRelID.Remove(innerRelID);
            }
        }

        public HashSet<EventBasedResultInnerRelationships> GetNLEventBasedLinkInnerIdPairs()
        {
            return NLEventBasedLinkInnerIdPairs;
        }
        public NotLoadedEventBasedKWLink GetNotLoadedEventBasedKWLink()
        {
            return new NotLoadedEventBasedKWLink()
            {
                Source = From,
                Target = To,
                IntermediaryLinksRelationshipIDs = NLEventBasedLinkInnerIdPairs
            };
        }

        internal bool TryGetNLEventBasedLinkInnerIDsPairByInnerRelIDs(long iD1, long iD2, out EventBasedResultInnerRelationships relatedPair)
        {
            EventBasedResultInnerRelationships checkingValue = new EventBasedResultInnerRelationships()
            {
                FirstRelationshipID = iD1,
                SecondRelationshipID = iD2
            };
            return NLEventBasedLinkInnerIdPairs.TryGetValue(checkingValue, out relatedPair);
        }
        #endregion

        internal bool IsEmpty()
        {
            return !RelationshipBasedLinks.Any()
                && !EventBasedLinks.Any()
                && !PropertyBasedLinks.Any()
                && !NLRelationshipBasedLinkIDs.Any()
                && !NLEventBasedLinkInnerIdPairs.Any();
        }

        public Edge RelatedEdge { get; set; }
        public bool IsShownOnGraph { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is EdgeMetadata)
            {
                return (From.Equals(((EdgeMetadata)obj).From) && To.Equals(((EdgeMetadata)obj).To))
                    || (From.Equals(((EdgeMetadata)obj).To) && To.Equals(((EdgeMetadata)obj).From));
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (From.ID ^ To.ID).GetHashCode();
        }

        public EdgeDirection GetDirection()
        {
            if (this.IsEmpty()
                 || NLEventBasedLinkInnerIdPairs.Count > 0
                 || NLRelationshipBasedLinkIDs.Count > 0
                 || PropertyBasedLinks.Count > 0)
            {
                return EdgeDirection.Bidirectional;
            }

            EdgeDirection firstLinkDirection;
            KWObject firstLinkSource;
            KWObject firstLinkTarget;
            if (EventBasedLinks.Count > 0)
            {
                firstLinkSource = EventBasedLinks.First().Source;
                firstLinkTarget = EventBasedLinks.First().Target;
                firstLinkDirection = GraphControlEdgeManager.GetEdgeDirectionFromLinkDirection(EventBasedLinks.First().LinkDirection);
            }
            else if (RelationshipBasedLinks.Count > 0)
            {
                firstLinkSource = RelationshipBasedLinks.First().Source;
                firstLinkTarget = RelationshipBasedLinks.First().Target;
                firstLinkDirection = GraphControlEdgeManager.GetEdgeDirectionFromLinkDirection(RelationshipBasedLinks.First().LinkDirection);
            }
            else
                throw new InvalidOperationException();

            if (firstLinkDirection != EdgeDirection.Bidirectional)
            {
                foreach (KWLink currentLink in ((IEnumerable<KWLink>)EventBasedLinks).Concat(RelationshipBasedLinks))
                {
                    if (currentLink.Source == firstLinkSource && currentLink.Target == firstLinkTarget)
                    {
                        if (GraphControlEdgeManager.GetEdgeDirectionFromLinkDirection(currentLink.LinkDirection) != firstLinkDirection)
                        {
                            firstLinkDirection = EdgeDirection.Bidirectional;
                            break;
                        }
                    }
                    else if (currentLink.Source == firstLinkTarget && currentLink.Target == firstLinkSource)
                    {
                        if ((firstLinkDirection == EdgeDirection.FromSourceToTarget && currentLink.LinkDirection != LinkDirection.TargetToSource)
                         || (firstLinkDirection == EdgeDirection.FromTargetToSource && currentLink.LinkDirection != LinkDirection.SourceToTarget))
                        {
                            firstLinkDirection = EdgeDirection.Bidirectional;
                            break;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(Properties.Resources.Unable_to_get_direction_for_links_with_different_source_and_target);
                    }
                }
            }
            if (firstLinkDirection == EdgeDirection.Bidirectional
            || (firstLinkSource.Equals(From) && firstLinkTarget.Equals(To)))
            {
                return firstLinkDirection;
            }
            else if (firstLinkSource.Equals(To) && firstLinkTarget.Equals(From))
            {
                // در این حالت ترتیب دو سر یال با لینک‌ها هماهنگ نیست و باید جهت یال برعکس نمایش داده شود
                if (firstLinkDirection == EdgeDirection.FromSourceToTarget)
                {
                    return EdgeDirection.FromTargetToSource;
                }
                else if (firstLinkDirection == EdgeDirection.FromTargetToSource)
                {
                    return EdgeDirection.FromSourceToTarget;
                }
                else
                {
                    throw new NotSupportedException(Properties.Resources.Unknow_Relationship_Direction);
                }
            }
            else
            {
                throw new InvalidOperationException(Properties.Resources.Link_ends_are_not_match_with_the_edge);
            }
        }

        public string GetTitle()
        {
            int nonExplicitTitledRelationshipsCount = 0;
            int nonExplicitTitledSameEventsCount = 0;
            int nonExplicitTitledSameProperties = 0;
            StringBuilder titleBuilder = new StringBuilder();

            if (RelationshipBasedLinks.Count > 0)
            {
                if (RelationshipBasedLinks.Count >= 1)
                    titleBuilder.Append(RelationshipBasedLinks.ElementAt(0).Text);
                if (RelationshipBasedLinks.Count >= 2)
                    titleBuilder.AppendFormat(Properties.Resources.__0_, RelationshipBasedLinks.ElementAt(1).Text);
                if (RelationshipBasedLinks.Count > 2)
                    nonExplicitTitledRelationshipsCount = RelationshipBasedLinks.Count - 2;
            }
            if (EventBasedLinks.Count > 0)
            {
                if (EventBasedLinks.Count >= 1)
                    titleBuilder.AppendFormat((titleBuilder.Length == 0) ? "{0}" : Properties.Resources.__0_, EventBasedLinks.ElementAt(0).Text);
                if (EventBasedLinks.Count >= 2)
                    titleBuilder.AppendFormat(Properties.Resources.__0_, EventBasedLinks.ElementAt(1).Text);
                if (EventBasedLinks.Count > 2)
                    nonExplicitTitledSameEventsCount = EventBasedLinks.Count - 2;
            }
            if (PropertyBasedLinks.Count > 0)
            {
                if (PropertyBasedLinks.Count >= 1)
                    titleBuilder.AppendFormat((titleBuilder.Length == 0) ? "{0}" : Properties.Resources.__0_, PropertyBasedLinks.ElementAt(0).Text);
                if (PropertyBasedLinks.Count >= 2)
                    titleBuilder.AppendFormat(Properties.Resources.__0_, PropertyBasedLinks.ElementAt(1).Text);
                if (PropertyBasedLinks.Count > 2)
                    nonExplicitTitledSameProperties = PropertyBasedLinks.Count - 2;
            }
            nonExplicitTitledRelationshipsCount += NLRelationshipBasedLinkIDs.Count;
            nonExplicitTitledSameEventsCount += NLEventBasedLinkInnerIdPairs.Count;

            if (nonExplicitTitledRelationshipsCount > 0)
            {
                if (titleBuilder.Length == 0)
                    if (nonExplicitTitledRelationshipsCount == 1)
                        titleBuilder.AppendFormat(Properties.Resources._0_relationship, nonExplicitTitledRelationshipsCount.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._0_relationships, nonExplicitTitledRelationshipsCount.ToString());
                else
                {
                    if (nonExplicitTitledRelationshipsCount == 1)
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_relationship, nonExplicitTitledRelationshipsCount.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_relationships, nonExplicitTitledRelationshipsCount.ToString());
                }
            }
            if (nonExplicitTitledSameEventsCount > 0)
            {
                if (titleBuilder.Length == 0)
                    if (nonExplicitTitledSameEventsCount == 1)
                        titleBuilder.AppendFormat(Properties.Resources._0_event, nonExplicitTitledSameEventsCount.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._0_events, nonExplicitTitledSameEventsCount.ToString());
                else
                {
                    if (nonExplicitTitledSameEventsCount == 1)
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_event, nonExplicitTitledSameEventsCount.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_events, nonExplicitTitledSameEventsCount.ToString());
                }
            }
            if (nonExplicitTitledSameProperties > 0)
            {
                if (titleBuilder.Length == 0)
                    if (nonExplicitTitledSameProperties == 1)
                        titleBuilder.AppendFormat(Properties.Resources._0_same_property, nonExplicitTitledSameProperties.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._0_same_properties, nonExplicitTitledSameProperties.ToString());
                else
                {
                    if (nonExplicitTitledSameProperties == 1)
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_same_property, nonExplicitTitledSameProperties.ToString());
                    else
                        titleBuilder.AppendFormat(Properties.Resources._and_0_more_same_properties, nonExplicitTitledSameProperties.ToString());
                }
            }

            return titleBuilder.ToString();
        }

        internal int GetAllLinksCount()
        {
            return RelationshipBasedLinks.Count
                + EventBasedLinks.Count
                + PropertyBasedLinks.Count
                + NLRelationshipBasedLinkIDs.Count
                + NLEventBasedLinkInnerIdPairs.Count;
        }

        internal IEnumerable<KWLink> GetAllLinks()
        {
            return ((IEnumerable<KWLink>)RelationshipBasedLinks)
                .Concat((IEnumerable<KWLink>)EventBasedLinks)
                .Concat((IEnumerable<KWLink>)PropertyBasedLinks)
                .Append(GetNotLoadedRelationshipBasedKWLink())
                .Append(GetNotLoadedEventBasedKWLink());
        }

        public string GetNearestTypeUri()
        {
            if ((RelationshipBasedLinks.Count > 0 || NLRelationshipBasedLinkIDs.Count > 0)
                && EventBasedLinks.Count == 0 && NLEventBasedLinkInnerIdPairs.Count == 0
                && PropertyBasedLinks.Count == 0)
            {
                Ontology.Ontology ontology = Logic.OntologyProvider.GetOntology();
                if (NLRelationshipBasedLinkIDs.Count > 0)
                    return ontology.GetRelationshipTypeURI();
                string edgeTypeUri = RelationshipBasedLinks.First().TypeURI;
                foreach (string relTypeUri in RelationshipBasedLinks.Select(l => l.TypeURI))
                {
                    if (edgeTypeUri.Equals(relTypeUri))
                        continue;
                    if (ontology.GetOnlyAllChilds(edgeTypeUri).Contains(relTypeUri))
                        continue;
                    if (ontology.GetAllParents(edgeTypeUri).Contains(relTypeUri))
                        edgeTypeUri = relTypeUri;
                    if (edgeTypeUri.Equals(ontology.GetRelationshipTypeURI()))
                        break;
                }
                return edgeTypeUri;
            }
            else if (RelationshipBasedLinks.Count == 0 && NLRelationshipBasedLinkIDs.Count == 0
                && (EventBasedLinks.Count > 0 || NLEventBasedLinkInnerIdPairs.Count > 0)
                && PropertyBasedLinks.Count == 0)
            {
                Ontology.Ontology ontology = Logic.OntologyProvider.GetOntology();
                if (NLEventBasedLinkInnerIdPairs.Count > 0)
                    return ontology.GetEventTypeURI();
                string edgeTypeUri = EventBasedLinks.First().TypeURI;
                foreach (string eventTypeUri in EventBasedLinks.Select(l => l.TypeURI))
                {
                    if (edgeTypeUri.Equals(eventTypeUri))
                        continue;
                    if (ontology.GetOnlyAllChilds(edgeTypeUri).Contains(eventTypeUri))
                        continue;
                    if (ontology.GetAllParents(edgeTypeUri).Contains(eventTypeUri))
                        edgeTypeUri = eventTypeUri;
                    if (edgeTypeUri.Equals(ontology.GetEventTypeURI()))
                        break;
                }
                return edgeTypeUri;
            }
            else if (RelationshipBasedLinks.Count == 0 && NLRelationshipBasedLinkIDs.Count == 0
                && EventBasedLinks.Count == 0 && NLEventBasedLinkInnerIdPairs.Count == 0
                && PropertyBasedLinks.Count > 0)
            {
                string samePropTypeUri = PropertyBasedLinks.First().SamePropertyTypeUri;
                foreach (string propTypeUri in PropertyBasedLinks.Select(l => l.SamePropertyTypeUri))
                {
                    if (!samePropTypeUri.Equals(propTypeUri))
                        return string.Empty;
                }
                return samePropTypeUri;
            }
            else
            {
                return string.Empty;
            }
        }

        internal bool IsUnmergable()
        {
            return EventBasedLinks.Count > 0
                || NLRelationshipBasedLinkIDs.Count > 0
                || NLEventBasedLinkInnerIdPairs.Count > 0;
        }

        internal void UpdateLayout()
        {
            RelatedEdge.Text = GetTitle();
            RelatedEdge.ToolTip = RelatedEdge.Text;
            string nearestTypeUri = GetNearestTypeUri();
            if (string.IsNullOrWhiteSpace(nearestTypeUri))
                RelatedEdge.SetIconUri(nearestTypeUri);
            else if (Logic.OntologyProvider.GetOntology().IsProperty(nearestTypeUri))
                RelatedEdge.SetIconUri(Logic.OntologyIconProvider.GetPropertyTypeIconPath(nearestTypeUri));
            else
                RelatedEdge.SetIconUri(Logic.OntologyIconProvider.GetTypeIconPath(nearestTypeUri));
            RelatedEdge.SetDirection(this.GetDirection());
            RelatedEdge.RelatedEdgeControl.UpdateEdge();
            RelatedEdge.RelatedEdgeControl.UpdateLayout();
            //RelatedEdge.RelatedEdgeControl.LabelControl.UpdatePosition();
            //RelatedEdge.RelatedEdgeControl.LabelControl.UpdateLayout();
            RelatedEdge.RelatedEdgeControl.UpdateLabel();
        }

        internal bool ContainsRelationship(long relID)
        {
            return NLRelationshipBasedLinkIDs.Contains(relID)
                || NLEventBasedLinkInnerIdPairsPerInnerRelID.ContainsKey(relID)
                || RelationshipBasedLinks.Any(l => l.Relationship.ID.Equals(relID))
                || EventBasedLinks.Any(l => l.FirstRelationship.ID.Equals(relID) || l.SecondRelationship.ID.Equals(relID));
        }

        internal bool IsEdgeEndsMatchWith(KWLink link)
        {
            if (From.Equals(link.Source) && To.Equals(link.Target)
             || From.Equals(link.Target) && To.Equals(link.Source))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
