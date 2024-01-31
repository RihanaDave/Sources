using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class CompoundEdge : Edge
    {
        public static CompoundEdge Factory(IEnumerable<Edge> edgesToComposite)
        {
            if (edgesToComposite == null)
                throw new ArgumentNullException(nameof(edgesToComposite));
            if (!edgesToComposite.Any())
                throw new ArgumentException(nameof(edgesToComposite));

            var innerEdges = edgesToComposite.ToList();

            var firstEdge = innerEdges[0];
            EdgeDirection newEdgeDirection = firstEdge.Direction;

            for (int i = 1; i < innerEdges.Count; i++)
            {
                var currentEdge = innerEdges[i];

                if (currentEdge.Source == firstEdge.Source && currentEdge.Target == firstEdge.Target)
                {
                    if (newEdgeDirection != EdgeDirection.Bidirectional && currentEdge.Direction != newEdgeDirection)
                    {
                        newEdgeDirection = EdgeDirection.Bidirectional;
                    }
                }
                else if (currentEdge.Source == firstEdge.Target && currentEdge.Target == firstEdge.Source)
                {
                    if (newEdgeDirection != EdgeDirection.Bidirectional)
                    {
                        if ((newEdgeDirection == EdgeDirection.FromSourceToTarget && currentEdge.Direction != EdgeDirection.FromTargetToSource)
                         || (newEdgeDirection == EdgeDirection.FromTargetToSource && currentEdge.Direction != EdgeDirection.FromSourceToTarget))
                        {
                            newEdgeDirection = EdgeDirection.Bidirectional;
                        }
                    }
                }
                else
                    throw new InvalidOperationException("Unable to generate compound edge for edges with differant source and target");
            }

            return new CompoundEdge
                (firstEdge.Source
                , firstEdge.Target
                , newEdgeDirection
                , innerEdges
                , GetCompoundEdgeTitleByInnerEdges(innerEdges)
                , GetCompoundEdgeTooltipByInnerEdges(innerEdges));
        }

        private static string GetCompoundEdgeTooltipByInnerEdges(List<Edge> innerEdges)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < innerEdges.Count; i++)
            {
                if (i < 7)
                {
                    if (result.Length != 0)
                    {
                        result.Append(Environment.NewLine);
                    }
                    result.AppendFormat("{0}{1}{0}", "\"", innerEdges.ElementAt(i).Text);
                }
                else
                {
                    result.Append(Environment.NewLine);
                    result.AppendFormat("{0} and {1} more linke(s) {0}", "\"", (innerEdges.Count - 7));
                    break;
                }

            }

            return result.ToString();
        }

        private static string GetCompoundEdgeTitleByInnerEdges(List<Edge> innerEdges)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < innerEdges.Count; i++)
            {
                if (i < 7)
                {
                    if (result.Length != 0)
                    {
                        result.AppendFormat("{0} ", Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);
                    }
                    result.AppendFormat("{0}{1}{0}", "\"", innerEdges.ElementAt(i).Text);
                }
                else
                {
                    result.AppendFormat("{0} and {1} more linke(s) {0}", "\"", (innerEdges.Count - 7));
                    break;
                }

            }

            return result.ToString();
        }

        private CompoundEdge(Vertex source, Vertex target, EdgeDirection direction, List<Edge> innerEdges, string title, string tooltip)
            : base(source, target, direction, title, null, tooltip)
        {
            InnerEdges = innerEdges;
        }

        public List<Edge> InnerEdges
        {
            get;
            private set;
        }
    }
}
