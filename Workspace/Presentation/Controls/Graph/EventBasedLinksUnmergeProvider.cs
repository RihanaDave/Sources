using GPAS.Graph.GraphViewer;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class EventBasedLinksUnmergeProvider
    {
        private static int countOfUnmergeLinksWarning = -1;
        private const string CountOfUnmergeLinksWarning_SettingName = "GraphApplication_CountOfUnmergeLinksWarning";
        public static int CountOfUnmergeLinksWarning
        {
            get
            {
                if (countOfUnmergeLinksWarning != -1)
                {
                    return countOfUnmergeLinksWarning;
                }
                else
                {
                    if (int.TryParse(ConfigurationManager.AppSettings[CountOfUnmergeLinksWarning_SettingName], out countOfUnmergeLinksWarning))
                        return countOfUnmergeLinksWarning;
                    else
                        throw new ConfigurationErrorsException(string.Format(Properties.Resources.Unable_to_load_configuration_0_, CountOfUnmergeLinksWarning_SettingName));
                }
            }
        }

        private const double UnmergedLinksTotalVerticalDistanceGapPixels = 80;
        private const double UnmergedLinksTotalHorizontalDistanceGapPixels = 70;
        private const double UnmergedLinksInterEventsGapPixels = 5;

        public IEnumerable<EventBasedKWLink> UnmergingLinks { get; private set; }
        internal List<EventBasedLinksPerEnds> UnmergingLinksPerEnds { get; set; }
        public GraphControl Graph { get; private set; }
        private GraphViewer Viewer { get; set; }

        public EventBasedLinksUnmergeProvider(GraphControl graphControl, IEnumerable<EventBasedKWLink> links)
        {
            if (graphControl == null)
                throw new NullReferenceException(nameof(graphControl));
            if (links == null)
                throw new NullReferenceException(nameof(links));

            Graph = graphControl;
            Viewer = graphControl.Viewer;
            UnmergingLinks = links;
            UnmergingLinksPerEnds = EventBasedLinksPerEnds.SeperateLinksByTheirEnds(links);
        }

        public async Task UnmergeLinks()
        {
            if (!UnmergingLinks.Any())
            {
                Graph.DeselectAllLinks();
                return;
            }

            List<KWObject> totalUnmergedEvents = new List<KWObject>();
            List<NotLoadedEventBasedKWLink> relatedMasters = new List<NotLoadedEventBasedKWLink>();
            foreach (EventBasedLinksPerEnds linksPerEnds in UnmergingLinksPerEnds)
            {
                KWObject[] intermediaryEvents = linksPerEnds.Links.Select(l => l.IntermediaryEvent).ToArray();

                foreach (KWObject intermediary in intermediaryEvents)
                {
                    if (OntologyProvider.GetOntology().IsEvent(intermediary.TypeURI))
                    {
                        foreach (EventBasedKWLink showingEventBasedLink in Graph.GetShowingEventBasedLinks())
                        {
                            if (showingEventBasedLink.IntermediaryEvent.Equals(intermediary))
                            {
                                var internalRelationshipsPair = LinkManager.ConvertEventBaseKWLinkToRelationshipBasedKWLink(showingEventBasedLink);
                                relatedMasters.AddRange(Graph.GetNotLoadedEventBasedMasterLinksFor(internalRelationshipsPair.Item1));
                                relatedMasters.AddRange(Graph.GetNotLoadedEventBasedMasterLinksFor(internalRelationshipsPair.Item2));
                            }
                        }
                    }
                }
            }
            if (relatedMasters.Count > 0)
            {
                //ابتدا لینک های مبتنی بر رخداد که هنوز لود نشده اند و با لینک های انتخاب شده ارتباط دارند باز می شوند
                await Graph.UnmergeNotLoadedEventBasedMasterLinks(relatedMasters.Distinct().ToList());
            }

            foreach (EventBasedLinksPerEnds linksPerEnds in UnmergingLinksPerEnds)
            {
                KWObject[] intermediaryEvents = linksPerEnds.Links.Select(l => l.IntermediaryEvent).ToArray();

                if (linksPerEnds.End1.Equals(linksPerEnds.End2))
                {
                    // TODO: Convert to pend mode
                    Graph.ShowObjectsAround(linksPerEnds.End1, intermediaryEvents);
                    //foreach (KWObject intermediaryEvent in intermediaryEvents)
                    //{
                    //    await Graph.BreakDownShowingEventBasedLinkWithIntermediaryEvent(intermediaryEvent);
                    //}
                }
                else
                {
                    Point end1Position = Viewer.GetVertexPosition(Graph.GetRelatedVertex(linksPerEnds.End1));
                    Point end2Position = Viewer.GetVertexPosition(Graph.GetRelatedVertex(linksPerEnds.End2));
                    Point showStartPoint = new Point
                        ((end1Position.X + end2Position.X) / 2
                        , (end1Position.Y + end2Position.Y) / 2);
                    List<ObjectShowMetadata> showMetadatas = new List<ObjectShowMetadata>(intermediaryEvents.Length);
                    foreach (KWObject intermediaryEvent in intermediaryEvents)
                    {
                        showMetadatas.Add(new ObjectShowMetadata()
                        {
                            ObjectToShow = intermediaryEvent,
                            NonDefaultPositionX = showStartPoint.X,
                            NonDefaultPositionY = showStartPoint.Y
                        });
                        //await Graph.BreakDownShowingEventBasedLinkWithIntermediaryEvent(intermediaryEvent);
                    }
                    // TODO: Convert to pend mode
                    Graph.ShowObjects(showMetadatas);
                    totalUnmergedEvents.AddRange(intermediaryEvents);
                    double arrangeGradient = GetUnmergeArrangeGradientByLinkEndPositions(end1Position, end2Position);
                    AnimateVerticesToUnmergeFinalPosition(showStartPoint, arrangeGradient, intermediaryEvents.Select(e => Graph.GetRelatedVertex(e)));
                }
            }
            Graph.UpdateLayout();
            Graph.DeselectAllLinks();
            Graph.SelectObjects(totalUnmergedEvents);
        }
        private double GetUnmergeArrangeGradientByLinkEndPositions(Point pos1, Point pos2)
        {
            double endsGradient;
            if (pos2.X == pos1.X)
                endsGradient = double.PositiveInfinity;
            else
                endsGradient = (pos2.Y - pos1.Y) / (pos2.X - pos1.X);

            double unmergeArrangeGradient;
            if (double.IsInfinity(endsGradient))
                unmergeArrangeGradient = 0;
            else if (endsGradient == 0)
                unmergeArrangeGradient = double.PositiveInfinity;
            else
                unmergeArrangeGradient = -(1 / endsGradient);

            return unmergeArrangeGradient;
        }
        private void AnimateVerticesToUnmergeFinalPosition(Point startPosition, double arrangeGradient, IEnumerable<Vertex> intermediaryVertices)
        {
            var verticesFinalPosition = new Dictionary<Vertex, Point>();
            if (arrangeGradient > 1 || arrangeGradient < -1)
            {
                foreach (var vertex in intermediaryVertices)
                {
                    Size vertexSize = Viewer.GetVertexActualSize(vertex);
                    double finalY
                        = (verticesFinalPosition.Count % 2 == 0)
                        ? startPosition.Y + ((verticesFinalPosition.Count / 2 + 1) * UnmergedLinksTotalVerticalDistanceGapPixels)
                        : startPosition.Y - ((verticesFinalPosition.Count / 2 + 1) * UnmergedLinksTotalVerticalDistanceGapPixels);
                    double finalX
                        = (double.IsInfinity(arrangeGradient))
                        ? startPosition.X
                        : ((finalY - startPosition.Y) / arrangeGradient) + startPosition.X;
                    if (!verticesFinalPosition.ContainsKey(vertex))
                    {
                        verticesFinalPosition.Add(vertex, new Point(finalX, finalY));
                    }
                }
            }
            else
            {
                foreach (var vertex in intermediaryVertices)
                {
                    Size vertexSize = Viewer.GetVertexActualSize(vertex);
                    double finalX
                        = (verticesFinalPosition.Count % 2 == 0)
                        ? startPosition.X + ((verticesFinalPosition.Count / 2 + 1) * UnmergedLinksTotalHorizontalDistanceGapPixels)
                        : startPosition.X - ((verticesFinalPosition.Count / 2 + 1) * UnmergedLinksTotalHorizontalDistanceGapPixels);
                    double finalY = (arrangeGradient * (finalX - startPosition.X)) + startPosition.Y;
                    if (!verticesFinalPosition.ContainsKey(vertex))
                    {
                        verticesFinalPosition.Add(vertex, new Point(finalX, finalY));
                    }
                }
            }
            Viewer.AnimateVerticesMove(verticesFinalPosition);
        }
        public bool IsUnmergeNeedsWarningForUnmergedLinkCount()
        {
            foreach (EventBasedLinksPerEnds linksPerEnds in UnmergingLinksPerEnds)
            {
                if (linksPerEnds.Links.Count > CountOfUnmergeLinksWarning)
                    return true;
            }
            return false;
        }

    }
}
