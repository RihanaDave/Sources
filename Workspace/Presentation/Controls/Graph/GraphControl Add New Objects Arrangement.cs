using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls
{
    public partial class GraphControl
    {
        int addArrangmentInterObjectGap = 150;
        private void ArrangeAddedVertices(IEnumerable<Vertex> addedVertices)
        {
            if (!addedVertices.Any())
            {
                return;
            }
            
            HashSet<Vertex> verticesToArrange = new HashSet<Vertex>(addedVertices);

            Point center = GetGraphCurrentViewCenterPosition();
            int sq = Convert.ToInt32(Math.Ceiling(Math.Sqrt(verticesToArrange.Count)));
            Point firstVertexCenter
                = new Point(center.X - (sq - 1) / 2 * addArrangmentInterObjectGap, center.Y - (sq - 1) / 2 * addArrangmentInterObjectGap);

            List<Rect> previouslyExistVerticesPositions = GetPreviouslyExistVerticesPositions(verticesToArrange);

            int positionsCounter = 0;
            foreach (var vertex in verticesToArrange)
            {
                bool positionAssigned = false;
                Size size = graphviewerMain.GetVertexActualSize(vertex);
                Point proposedTopLeftPoint = new Point();
                do
                {
                    positionsCounter++;
                    Point proposedCenterPoint = GetAddArrangmentProposedPoint(firstVertexCenter, positionsCounter);
                    proposedTopLeftPoint = new Point(proposedCenterPoint.X - size.Width / 2, proposedCenterPoint.Y - size.Height / 2);
                    Rect proposedPlace = new Rect(proposedTopLeftPoint, size);
                    if (!IsOverlapedWithPreviouslyExistVertices(proposedPlace, previouslyExistVerticesPositions))
                    {
                        positionAssigned = true;
                    }
                } while (!positionAssigned);
                graphviewerMain.SetVertexPosition(vertex, proposedTopLeftPoint.X, proposedTopLeftPoint.Y);
            }
        }
        
        private List<Rect> GetPreviouslyExistVerticesPositions(HashSet<Vertex> verticesToAdd)
        {
            List<Rect> previouslyExistVerticesPositions = new List<Rect>(graphviewerMain.Vertices.Count);
            foreach (Vertex previouslyExistVertex
                in graphviewerMain.Vertices.Where(v => !verticesToAdd.Contains(v)))
            {
                previouslyExistVerticesPositions.Add(graphviewerMain.GetVertexActualRect(previouslyExistVertex));
            }
            return previouslyExistVerticesPositions;
        }

        private bool IsOverlapedWithPreviouslyExistVertices(Rect checkingRect, List<Rect> previouslyExistVerticesPositions)
        {
            bool result = false;
            foreach (var existPosition in previouslyExistVerticesPositions)
            {
                if (existPosition.IntersectsWith(checkingRect))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <remarks>
        /// Arrangment Sample:
        ///     1   2   5   10
        ///     3   4   6   11
        ///     7   8   9   12
        ///     13  14  15  16
        /// </remarks>
        private Point GetAddArrangmentProposedPoint(Point firstPosition, int positionNumber)
        {
            if (positionNumber <= 0)
                throw new InvalidOperationException();

            if (positionNumber == 1)
                return firstPosition;

            int levelNumber = Convert.ToInt32(Math.Ceiling(Math.Sqrt(positionNumber)));
            int levelFirstPositionNumber = Convert.ToInt32(Math.Pow(levelNumber - 1, 2)) + 1;
            int levelLastPositionNumber = Convert.ToInt32(Math.Pow(levelNumber, 2));
            int levelCapacity = levelNumber * 2 - 1;
            int levelMedian = (levelCapacity / 2);
            int positionPlaceInLevel = (positionNumber - levelFirstPositionNumber + 1);
            if (positionPlaceInLevel <= levelMedian)
            {
                return new Point(firstPosition.X + ((levelNumber - 1) * addArrangmentInterObjectGap), firstPosition.Y + ((positionPlaceInLevel - 1) * addArrangmentInterObjectGap));
            }
            else
            {
                return new Point(firstPosition.X + ((positionPlaceInLevel - levelMedian - 1) * addArrangmentInterObjectGap), firstPosition.Y + ((levelNumber - 1) * addArrangmentInterObjectGap));
            }
        }
    }
}
