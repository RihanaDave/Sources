using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Graph.GraphViewer.LayoutAlgorithms;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public partial class BindableMappingGraphUserControl
    {
        public BindableMappingGraphUserControl()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        private readonly Dictionary<ObjectMapModel, Vertex> verticesDictionary = new Dictionary<ObjectMapModel, Vertex>();
        private readonly Dictionary<RelationshipMapModel, Edge> edgesDictionary = new Dictionary<RelationshipMapModel, Edge>();

        public MapModel Map
        {
            get { return (MapModel)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(nameof(Map), typeof(MapModel), typeof(BindableMappingGraphUserControl),
                new PropertyMetadata(null, OnSetMapChanged));

        #endregion

        #region Methods

        private static void OnSetMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BindableMappingGraphUserControl)d).OnSetMapChanged(e);
        }

        private void OnSetMapChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Map != null)
            {
                if (MainGraphViewer.IsViewerInitialized)
                {
                    AddObjectsToGraph(Map.ObjectCollection);
                    AddLinksToGraph(Map.RelationshipCollection);
                }

                Map.ObjectCollectionChanged -= Map_ObjectCollectionChanged;
                Map.ObjectCollectionChanged += Map_ObjectCollectionChanged;

                Map.RelationshipCollectionChanged -= Map_RelationshipCollectionChanged;
                Map.RelationshipCollectionChanged += Map_RelationshipCollectionChanged;
            }
        }

        private void Map_RelationshipCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
                AddLinksToGraph(e.NewItems);

            if (e.OldItems?.Count > 0)
                RemoveLinksFromGraph(e.OldItems);

            if ((e.NewItems == null || e.NewItems.Count == 0) && (e.OldItems == null || e.OldItems.Count == 0))
            {
                edgesDictionary.Clear();
                MainGraphViewer.ClearGraph();
            }
        }

        private void Map_ObjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
                AddObjectsToGraph(e.NewItems);

            if (e.OldItems?.Count > 0)
                RemoveObjectsFromGraph(e.OldItems);

            if ((e.NewItems == null || e.NewItems.Count == 0) && (e.OldItems == null || e.OldItems.Count == 0))
            {
                verticesDictionary.Clear();
                MainGraphViewer.ClearGraph();
            }
        }

        private void AddObjectsToGraph(IEnumerable newObjects)
        {
            if (!MainGraphViewer.IsViewerInitialized)
                return;

            if (newObjects == null)
                return;

            foreach (ObjectMapModel newObject in newObjects)
            {
                Vertex newVertex = Vertex.VertexFactory(OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(newObject.Title));
                newVertex.SetIconUri(newObject.IconPath);

                BindObjectWithRelatedVertex(newObject, newVertex);

                AddVertex(newVertex);
                verticesDictionary.Add(newObject, newVertex);
            }

            MainGraphViewer.RelayoutVertices(verticesDictionary.Values, LayoutAlgorithmTypeEnum.MinimumCrossing);
        }

        private void AddVertex(Vertex vertex)
        {
            MainGraphViewer.AddVertex(vertex);
            MainGraphViewer.AnimateVerticesAdd(new[] { vertex });
        }

        private void BindObjectWithRelatedVertex(ObjectMapModel objectMap, Vertex newVertex)
        {
            Binding binding = new Binding(nameof(objectMap.IsSelected))
            {
                Source = objectMap,
                Mode = BindingMode.TwoWay,
                IsAsync = true,
            };
            newVertex.RelatedVertexControl.SetBinding(VertexControl.IsSelectedProperty, binding);
        }

        private void RemoveObjectsFromGraph(IEnumerable objectsToRemove)
        {
            if (objectsToRemove == null)
                throw new ArgumentNullException(nameof(objectsToRemove));

            foreach (ObjectMapModel objectToRemove in objectsToRemove)
            {
                verticesDictionary.TryGetValue(objectToRemove, out Vertex relatedVertex);

                if (relatedVertex == null)
                    throw new InvalidOperationException(Properties.Resources.No_Vertex_Exist_For_Mapping);

                verticesDictionary.Remove(objectToRemove);
                RemoveRelatedLink(objectToRemove);
                MainGraphViewer.RemoveVertex(relatedVertex);
            }
        }

        private void RemoveRelatedLink(ObjectMapModel objectToRemove)
        {
            RemoveLinksFromGraph(edgesDictionary.Keys.Where(edgesDictionaryKey =>
                    edgesDictionaryKey.Source.Id == objectToRemove.Id ||
                    edgesDictionaryKey.Target.Id == objectToRemove.Id)
                .ToList());
        }

        private void AddLinksToGraph(IEnumerable links)
        {
            if (!MainGraphViewer.IsViewerInitialized)
                return;

            if (links == null)
                return;

            foreach (RelationshipMapModel link in links)
            {
                EdgeDirection relatedEdgeDirection = ConvertLinkDirectionToEdgeDirection(link.Direction);

                Edge relatedEdge = new Edge(verticesDictionary.Single(x => x.Key.Id == link.Source.Id).Value,
                    verticesDictionary.Single(x => x.Key.Id == link.Target.Id).Value,
                    relatedEdgeDirection, link.Title, new Uri(link.IconPath), link.Description);

                BindRelationshipWithRelatedEdge(link, relatedEdge);

                MainGraphViewer.AddEdge(relatedEdge);
                edgesDictionary.Add(link, relatedEdge);
            }

            MainGraphViewer.RelayoutVertices(verticesDictionary.Values, LayoutAlgorithmTypeEnum.MinimumCrossing);
        }

        private void BindRelationshipWithRelatedEdge(RelationshipMapModel relationship, Edge edge)
        {
            Binding binding = new Binding(nameof(relationship.IsSelected))
            {
                Source = relationship,
                Mode = BindingMode.TwoWay,
                IsAsync = true,
            };
            edge.RelatedEdgeControl.SetBinding(EdgeControl.IsSelectedProperty, binding);
        }

        private EdgeDirection ConvertLinkDirectionToEdgeDirection(LinkDirection linkDirection)
        {
            switch (linkDirection)
            {
                case LinkDirection.SourceToTarget:
                    return EdgeDirection.FromSourceToTarget;
                case LinkDirection.TargetToSource:
                    return EdgeDirection.FromTargetToSource;
                case LinkDirection.Bidirectional:
                    return EdgeDirection.Bidirectional;
                default:
                    throw new ArgumentException(Properties.Resources.Unknow_Relationship_Direction, nameof(linkDirection));
            }
        }

        public void RemoveLinksFromGraph(IEnumerable linksToRemove)
        {
            if (linksToRemove == null)
                return;

            foreach (RelationshipMapModel linkToRemove in linksToRemove)
            {
                edgesDictionary.TryGetValue(linkToRemove, out Edge relatedEdge);
                if (relatedEdge == null || !edgesDictionary.Remove(linkToRemove))
                    continue;

                MainGraphViewer.RemoveEdge(relatedEdge);
            }
        }

        private void MainGraphViewerOnViewerInitializationCompleted(object sender, EventArgs e)
        {
            AddObjectsToGraph(Map?.ObjectCollection);
            AddLinksToGraph(Map?.RelationshipCollection);
        }

        #endregion
    }
}
