using GPAS.Graph.GraphViewer;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class MergeProvider
    {
        public GraphControl Graph { get; private set; }
        private GraphViewer Viewer { get; set; }
        public IEnumerable<KWObject> MergingObjects { get; private set; }
        public bool CanMergeObjects { get; private set; }

        private IEnumerable<EventsWithTwoRelationships> mergingEventsRelationships;

        private static List<EventBasedKWLink> mergingLinks = new List<EventBasedKWLink>();

        public MergeProvider(GraphControl graphControl, IEnumerable<KWObject> objects)
        {
            if (objects == null)
                throw new NullReferenceException("objects");
            if (graphControl == null)
                throw new NullReferenceException("graphControl");

            Graph = graphControl;
            Viewer = graphControl.Viewer;
            MergingObjects = objects;
            CanMergeObjects = TryGetEventsRelationshipsForSelectedObjects(out mergingEventsRelationships);
        }

        private bool TryGetEventsRelationshipsForSelectedObjects(out IEnumerable<EventsWithTwoRelationships> eventRelationships)
        {
            eventRelationships = null;
            // فقط برای رخدادها ممکن است
            if (!MergingObjects.All(o => OntologyProvider.GetOntology().IsEvent(o.TypeURI)))
            {
                return false;
            }
            var eventsRelationshipsByEventId = new Dictionary<long, EventsWithTwoRelationships>();
            foreach (var selectedEvent in MergingObjects)
            {
                var er = new EventsWithTwoRelationships()
                { IntermediaryEvent = selectedEvent };
                eventsRelationshipsByEventId.Add(selectedEvent.ID, er);
            }

            foreach (RelationshipBasedKWLink link in Graph.GetShowingRelationshipBasedLinks())
            {
                if (eventsRelationshipsByEventId.ContainsKey(link.Source.ID))
                {
                    if (eventsRelationshipsByEventId[link.Source.ID].FirstRelationship == null)
                    {
                        eventsRelationshipsByEventId[link.Source.ID].FirstRelationship = link;
                    }
                    else if (eventsRelationshipsByEventId[link.Source.ID].SecondRelationship == null)
                    {
                        eventsRelationshipsByEventId[link.Source.ID].SecondRelationship = link;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (eventsRelationshipsByEventId.ContainsKey(link.Target.ID))
                {
                    if (eventsRelationshipsByEventId[link.Target.ID].FirstRelationship == null)
                    {
                        eventsRelationshipsByEventId[link.Target.ID].FirstRelationship = link;
                    }
                    else if (eventsRelationshipsByEventId[link.Target.ID].SecondRelationship == null)
                    {
                        eventsRelationshipsByEventId[link.Target.ID].SecondRelationship = link;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            foreach (EventsWithTwoRelationships eventsRelationships in eventsRelationshipsByEventId.Values)
            {
                // رخدادهایی که مبدا یا مقصد هیچ لینکی نباشند قابل ادغام نیستند
                if (eventsRelationships.FirstRelationship == null || eventsRelationships.SecondRelationship == null)
                {
                    return false;
                }
            }
            eventRelationships = eventsRelationshipsByEventId.Values;
            return true;
        }
        public void MergeObjects()
        {
            if (!CanMergeObjects)
                throw new InvalidOperationException(Properties.Resources.Unable_to_merge_selected_objects_as_a_link);

            int waitsCount = 0;
            while (mergingLinks.Count > 0)
            {
                Task.Run(() => { Task.Delay(500); });

                if (++waitsCount >= 5)
                    return;
            }

            foreach (var er in mergingEventsRelationships)
            {
                EventBasedKWLink link = LinkManager.GetEventBaseKWLinkFromLinkInnerRelationships(er.FirstRelationship, er.SecondRelationship);
                mergingLinks.Add(link);
            }

            List<EventBasedLinksPerEnds> mergingLinksPerEnds = EventBasedLinksPerEnds.SeperateLinksByTheirEnds(mergingLinks);
            foreach (var linksPerEnds in mergingLinksPerEnds)
            {
                Point end1Position = Viewer.GetVertexPosition(Graph.GetRelatedVertex(linksPerEnds.End1));
                Point end2Position = Viewer.GetVertexPosition(Graph.GetRelatedVertex(linksPerEnds.End2));
                Point endsMiddlePosition = new Point
                    ((end1Position.X + end2Position.X) / 2
                    , (end1Position.Y + end2Position.Y) / 2);
                Viewer.VerticesMoveAnimationCompleted += MergeVerticesMoveAnimationCompleted;
                AnimateVerticesToSpecifiedPosition(linksPerEnds.Links.Select(l => Graph.GetRelatedVertex(l.IntermediaryEvent)), endsMiddlePosition);
            }
        }
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async void MergeVerticesMoveAnimationCompleted(object sender, EventArgs e)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            Viewer.VerticesMoveAnimationCompleted -= MergeVerticesMoveAnimationCompleted;
            Graph.RemoveObjects(mergingLinks.Select(l => l.IntermediaryEvent).ToList());
            Graph.ShowLinks(mergingLinks);
            mergingLinks.Clear();
        }

        private void AnimateVerticesToSpecifiedPosition(IEnumerable<Vertex> vertices, Point position)
        {
            var verticesFinalPosition = new Dictionary<Vertex, Point>();
            foreach (var vertex in vertices)
            {
                verticesFinalPosition.Add(vertex, position);
            }
            Viewer.AnimateVerticesMove(verticesFinalPosition);
        }
    }
}