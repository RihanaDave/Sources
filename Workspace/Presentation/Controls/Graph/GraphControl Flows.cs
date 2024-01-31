using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Controls.Graph.Flows;
using GPAS.Workspace.Presentation.Controls.Graph.ShowMetadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Controls
{
    public partial class GraphControl
    {
        public bool IsFlowsShown
        {
            get { return (bool)GetValue(IsFlowsShownProperty); }
            set { SetValue(IsFlowsShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlowsShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlowsShownProperty =
            DependencyProperty.Register("IsFlowsShown", typeof(bool), typeof(GraphControl), new PropertyMetadata(false, OnSetIsFlowsShownChanged));

        private static void OnSetIsFlowsShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetIsFlowsShownChanged(e);
        }

        private async void OnSetIsFlowsShownChanged(DependencyPropertyChangedEventArgs e)
        {
            await ResetFlowsIfShown();
        }

        public SourceObjects FlowsSourceObjects
        {
            get { return (SourceObjects)GetValue(FlowsSourceObjectsProperty); }
            set { SetValue(FlowsSourceObjectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowsSourceObjects.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowsSourceObjectsProperty =
            DependencyProperty.Register("FlowsSourceObjects", typeof(SourceObjects), typeof(GraphControl),
                new PropertyMetadata(SourceObjects.AllObjects, OnSetFlowsSourceObjectsChanged));

        private static void OnSetFlowsSourceObjectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetFlowsSourceObjectsChanged(e);
        }

        private async void OnSetFlowsSourceObjectsChanged(DependencyPropertyChangedEventArgs e)
        {
            await ResetFlowsIfShown();
        }

        public FlowVisualStyle FlowsVisualStyle
        {
            get { return (FlowVisualStyle)GetValue(FlowsVisualStyleProperty); }
            set { SetValue(FlowsVisualStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowsVisualStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowsVisualStyleProperty =
            DependencyProperty.Register("FlowsVisualStyle", typeof(FlowVisualStyle), typeof(GraphControl),
                new PropertyMetadata(FlowVisualStyle.Animated, OnSetFlowsVisualStyleChanged));

        private static void OnSetFlowsVisualStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetFlowsVisualStyleChanged(e);
        }

        private void OnSetFlowsVisualStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (graphviewerMain.TotalFlowPathes == null)
                return;

            foreach (FlowPathVM flowPath in graphviewerMain.TotalFlowPathes)
            {
                flowPath.VisualStyle = FlowsVisualStyle;
                flowPath.UpdateFlowShape();
            }
        }

        public bool IncludeWeightInFlows
        {
            get { return (bool)GetValue(IncludeWeightInFlowsProperty); }
            set { SetValue(IncludeWeightInFlowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IncludeWeightInFlows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IncludeWeightInFlowsProperty =
            DependencyProperty.Register("IncludeWeightInFlows", typeof(bool), typeof(GraphControl), new PropertyMetadata(true,
                OnSetIncludeWeightInFlowsChanged));

        private static void OnSetIncludeWeightInFlowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetIncludeWeightInFlowsChanged(e);
        }

        private void OnSetIncludeWeightInFlowsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (graphviewerMain.TotalFlowPathes == null)
                return;

            foreach (FlowPathVM flowPath in graphviewerMain.TotalFlowPathes)
            {
                flowPath.IncludeWeightInFlow = IncludeWeightInFlows;
            }
        }

        public int FlowsSpeedInMiliSeconds
        {
            get { return (int)GetValue(FlowsSpeedInMiliSecondsProperty); }
            set { SetValue(FlowsSpeedInMiliSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowsSpeedInMiliSeconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowsSpeedInMiliSecondsProperty =
            DependencyProperty.Register("FlowsSpeedInMiliSeconds", typeof(int), typeof(GraphControl), new PropertyMetadata(4000,
                                            OnSetFlowsSpeedInMiliSecondsChanged));

        private static void OnSetFlowsSpeedInMiliSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetFlowsSpeedInMiliSecondsChanged(e);
        }

        private void OnSetFlowsSpeedInMiliSecondsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (graphviewerMain.TotalFlowPathes == null)
                return;

            foreach (FlowPathVM flowPath in graphviewerMain.TotalFlowPathes)
            {
                flowPath.FlowSpeedInMiliSeconds = FlowsSpeedInMiliSeconds;
            }
        }

        public Brush FlowsColor
        {
            get { return (Brush)GetValue(FlowsColorProperty); }
            set { SetValue(FlowsColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowsColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowsColorProperty =
            DependencyProperty.Register("FlowsColor", typeof(Brush), typeof(GraphControl), new PropertyMetadata(Brushes.Red,
                OnSetFlowsColorChanged));

        private static void OnSetFlowsColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetFlowsColorChanged(e);
        }

        private void OnSetFlowsColorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (graphviewerMain.TotalFlowPathes == null)
                return;

            foreach (FlowPathVM flowPath in graphviewerMain.TotalFlowPathes)
            {
                flowPath.AnimateShapeFlowColor = FlowsColor;
                flowPath.StaticShapeFlowColor = FlowsColor;
            }
        }

        public List<FlowPathMetaData> TotalFlowPathes
        {
            get { return (List<FlowPathMetaData>)GetValue(TotalFlowPathesProperty); }
            set { SetValue(TotalFlowPathesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalFlowPathes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalFlowPathesProperty =
            DependencyProperty.Register("TotalFlowPathes", typeof(List<FlowPathMetaData>), typeof(GraphControl), new PropertyMetadata(null));

        public double MinFlowPathWeight
        {
            get { return (double)GetValue(MinFlowPathWeightProperty); }
            set { SetValue(MinFlowPathWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinFlowPathWeightProperty =
            DependencyProperty.Register("MinFlowPathWeight", typeof(double), typeof(GraphControl), new PropertyMetadata((double)0));

        public double MaxFlowPathWeight
        {
            get { return (double)GetValue(MaxFlowPathWeightProperty); }
            set { SetValue(MaxFlowPathWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxFlowPathWeightProperty =
            DependencyProperty.Register("MaxFlowPathWeight", typeof(double), typeof(GraphControl), new PropertyMetadata((double)100));

        public HashSet<DataSource> FlowsDataSources
        {
            get { return (HashSet<DataSource>)GetValue(FlowsDataSourcesProperty); }
            set { SetValue(FlowsDataSourcesProperty, value); }
        }

        public double MinFlowPathWeightToShow
        {
            get { return (double)GetValue(MinFlowPathWeightToShowProperty); }
            set { SetValue(MinFlowPathWeightToShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeightToShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinFlowPathWeightToShowProperty =
            DependencyProperty.Register("MinFlowPathWeightToShow", typeof(double), typeof(GraphControl),
                new PropertyMetadata((double)0, OnSetMinFlowPathWeightToShowChanged));

        private static void OnSetMinFlowPathWeightToShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetMinFlowPathWeightToShowChanged(e);
        }

        private void OnSetMinFlowPathWeightToShowChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateFlowRangeToShow();
        }

        public double MaxFlowPathWeightToShow
        {
            get { return (double)GetValue(MaxFlowPathWeightToShowProperty); }
            set { SetValue(MaxFlowPathWeightToShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxFlowPathWeightToShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxFlowPathWeightToShowProperty =
            DependencyProperty.Register("MaxFlowPathWeightToShow", typeof(double), typeof(GraphControl),
                new PropertyMetadata((double)100, OnSetMaxFlowPathWeightToShowChanged));

        private static void OnSetMaxFlowPathWeightToShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphControl).OnSetMaxFlowPathWeightToShowChanged(e);
        }

        private void OnSetMaxFlowPathWeightToShowChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateFlowRangeToShow();
        }


        // Using a DependencyProperty as the backing store for FlowsDataSources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowsDataSourcesProperty =
            DependencyProperty.Register("FlowsDataSources", typeof(HashSet<DataSource>), typeof(GraphControl), new PropertyMetadata(null));

        public DataSource SelectedFlowsDataSource
        {
            get { return (DataSource)GetValue(SelectedFlowsDataSourceProperty); }
            set { SetValue(SelectedFlowsDataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedFlowsDataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedFlowsDataSourceProperty =
            DependencyProperty.Register("SelectedFlowsDataSource", typeof(DataSource), typeof(GraphControl), new PropertyMetadata(null));

        private void UpdateFlowRangeToShow()
        {
            foreach (FlowPathMetaData flowPath in TotalFlowPathes)
            {
                flowPath.IsShown = flowPath.Weight >= MinFlowPathWeightToShow && flowPath.Weight <= MaxFlowPathWeightToShow;
                if (IsFlowsShown)
                {
                    graphviewerMain.TotalFlowPathes.Where(tfp => tfp.Tag == flowPath).FirstOrDefault().IsShown = flowPath.IsShown;
                }
            }
        }

        public async Task ResetFlowsIfShown()
        {
            if (IsFlowsShown == true)
            {
                await ResetFlowsDataStructures();
                await RedrawFlows();
            }
        }
        
        CancellationTokenSource ResetFlowsDataStructuresCTS = new CancellationTokenSource();
        private async Task ResetFlowsDataStructures()
        {
            ResetFlowsDataStructuresCTS.Cancel();
            ResetFlowsDataStructuresCTS = new CancellationTokenSource();

            if (FlowsSourceObjects == SourceObjects.AllObjects)
            {
                await ResetFlowsDataStructures(GetShowingObjects(), TotalEdges.Where(e => e.IsShownOnGraph), ResetFlowsDataStructuresCTS);
            }
            else if (FlowsSourceObjects == SourceObjects.SelectedObjects)
            {
                await ResetFlowsDataStructures(GetSelectedObjects(), TotalEdges.Where(e => e.IsShownOnGraph), ResetFlowsDataStructuresCTS);
            }

            AssignMinAndMaxFlowPathWeight();
        }

        private async Task ResetFlowsDataStructures(IEnumerable<KWObject> objects, IEnumerable<EdgeMetadata> edges, CancellationTokenSource cts)
        {
            List<FlowPathMetaData> totalFlowPathes = new List<FlowPathMetaData>();

            foreach (EdgeMetadata edge in edges)
            {
                if (cts.IsCancellationRequested)
                {
                    return;
                }

                List<FlowPathMetaData> eventFlowPathes = new List<FlowPathMetaData>();

                if (FlowsSourceObjects == SourceObjects.AllObjects)
                    eventFlowPathes = GenerateEventFlowPathes(edge);
                else if (FlowsSourceObjects == SourceObjects.SelectedObjects)
                    if (Viewer.IsSelectedEdge(edge.RelatedEdge))
                        eventFlowPathes = GenerateEventFlowPathes(edge);

                totalFlowPathes.AddRange(eventFlowPathes);
            }

            List<FlowPathMetaData> relationshipFlowPathes = await GenerateRelationshipFlowPathes(objects, edges, cts);

            totalFlowPathes.AddRange(relationshipFlowPathes);
            TotalFlowPathes = totalFlowPathes;
        }

        private void AssignMinAndMaxFlowPathWeight()
        {
            if (TotalFlowPathes.Count > 0)
            {
                MinFlowPathWeight = TotalFlowPathes.Min(tfp => tfp.Weight);
                MaxFlowPathWeight = TotalFlowPathes.Max(tfp => tfp.Weight);
                if (MinFlowPathWeight == MaxFlowPathWeight)
                    MaxFlowPathWeight += 1;
            }
            else
            {
                MinFlowPathWeight = 0;
                MaxFlowPathWeight = 0;
            }
        }

        private List<FlowPathMetaData> GenerateEventFlowPathes(EdgeMetadata edge)
        {
            List<FlowPathMetaData> result = new List<FlowPathMetaData>();

            if (edge.EventBasedLinks.Count + edge.GetNLEventBasedLinkInnerIdPairs().Count > 0)
            {
                FlowPathMetaData eventFlowPath = GenerateFlowPathFromEventBasedLink(edge);
                result.Add(eventFlowPath);
            }

            return result;
        }

        private async Task<List<FlowPathMetaData>> GenerateRelationshipFlowPathes(IEnumerable<KWObject> objects, IEnumerable<EdgeMetadata> edge, 
                                                                                        CancellationTokenSource cts)
        {
            List<FlowPathMetaData> result = new List<FlowPathMetaData>();

            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            HashSet<KWObject> eventObjects = new HashSet<KWObject>();
            foreach (KWObject obj in objects)
            {
                if (cts.IsCancellationRequested)
                {
                    return new List<FlowPathMetaData>(); 
                }

                if (ontology.IsEvent(obj.TypeURI))
                {
                    eventObjects.Add(obj);
                }
            }

            foreach (var currentEvent in eventObjects)
            {
                if (cts.IsCancellationRequested)
                {
                    return new List<FlowPathMetaData>();
                }

                List<RelationshipBasedKWLink> relatedRelationsWithIntemediateEventObject = new List<RelationshipBasedKWLink>();

                foreach (var currentEdgeMetadata in edge)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return new List<FlowPathMetaData>();
                    }

                    // Loaded relations

                    foreach (var currentRelation in currentEdgeMetadata.RelationshipBasedLinks)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return new List<FlowPathMetaData>();
                        }

                        if (currentRelation.Source.ID == currentEvent.ID ||
                            currentRelation.Target.ID == currentEvent.ID)
                        {
                            relatedRelationsWithIntemediateEventObject.Add(currentRelation);
                        }
                    }                    
                }

                // Generate flow path from loaded relations
                if (relatedRelationsWithIntemediateEventObject.Count == 2)
                {
                    Tuple<KWRelationship, KWRelationship> relatedEventRelations = GenerateRelatedEventRelations(relatedRelationsWithIntemediateEventObject);

                    FlowPathMetaData generatedFlowPath = await GenerateFlowPathFromGeneratedEventBasedKWLinkIfPossible(relatedEventRelations.Item1, relatedEventRelations.Item2);

                    if (generatedFlowPath != null)
                    {
                        result.Add(generatedFlowPath);
                    }
                }
                else
                {
                    foreach (int[] selectedSubgroupIndexes in Combinations(2, relatedRelationsWithIntemediateEventObject.Count))
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return new List<FlowPathMetaData>();
                        }

                        List<RelationshipBasedKWLink> currentCombinationElements = new List<RelationshipBasedKWLink>();

                        currentCombinationElements.Add(relatedRelationsWithIntemediateEventObject[selectedSubgroupIndexes.ToList()[0] - 1]);
                        currentCombinationElements.Add(relatedRelationsWithIntemediateEventObject[selectedSubgroupIndexes.ToList()[1] - 1]);

                        Tuple<KWRelationship, KWRelationship> relatedEventRelations = GenerateRelatedEventRelations(currentCombinationElements);

                        FlowPathMetaData generatedFlowPath = await GenerateFlowPathFromGeneratedEventBasedKWLinkIfPossible(relatedEventRelations.Item1,
                            relatedEventRelations.Item2);

                        if (generatedFlowPath != null)
                        {
                            result.Add(generatedFlowPath);
                        }
                    }
                }
            }

            return result;
        }

        private Tuple<KWRelationship, KWRelationship> GenerateRelatedEventRelations(List<RelationshipBasedKWLink> relationshipBasedKWLinks)
        {
            Tuple<KWRelationship, KWRelationship> result = null;
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            if (relationshipBasedKWLinks.Count == 2)
            {                
                if (relationshipBasedKWLinks[0].LinkDirection == LinkDirection.SourceToTarget &&
                    relationshipBasedKWLinks[1].LinkDirection != LinkDirection.Bidirectional)
                {
                    if (ontology.IsEvent(relationshipBasedKWLinks[0].Source.TypeURI))
                    {
                        result = new Tuple<KWRelationship, KWRelationship>(relationshipBasedKWLinks[1].Relationship,
                            relationshipBasedKWLinks[0].Relationship);
                    }

                    if (ontology.IsEvent(relationshipBasedKWLinks[0].Target.TypeURI))
                    {
                        result = new Tuple<KWRelationship, KWRelationship>(relationshipBasedKWLinks[0].Relationship,
                            relationshipBasedKWLinks[1].Relationship);
                    }
                }
                else if (relationshipBasedKWLinks[0].LinkDirection == LinkDirection.TargetToSource &&
                    relationshipBasedKWLinks[1].LinkDirection != LinkDirection.Bidirectional)
                {
                    if (ontology.IsEvent(relationshipBasedKWLinks[0].Source.TypeURI))
                    {
                        result = new Tuple<KWRelationship, KWRelationship>(relationshipBasedKWLinks[1].Relationship,
                            relationshipBasedKWLinks[0].Relationship);
                    }

                    if (ontology.IsEvent(relationshipBasedKWLinks[0].Target.TypeURI))
                    {
                        result = new Tuple<KWRelationship, KWRelationship>(relationshipBasedKWLinks[0].Relationship,
                            relationshipBasedKWLinks[1].Relationship);
                    }
                }
                else
                {
                    result = new Tuple<KWRelationship, KWRelationship>(relationshipBasedKWLinks[0].Relationship,
                            relationshipBasedKWLinks[1].Relationship);
                }
            }


            return result;
        }

        private async Task<FlowPathMetaData> GenerateFlowPathFromGeneratedEventBasedKWLinkIfPossible(KWRelationship firstRelationship, KWRelationship secondRelationship)
        {
            FlowPathMetaData result = null;
            Tuple<bool, EventBasedKWLink> generateEventBasedLinkResult = await LinkManager.TryGenerateEventBasedLinkByInnerRelationshipsAsync(firstRelationship,
                secondRelationship);

            if (generateEventBasedLinkResult.Item1 == true &&
                (generateEventBasedLinkResult.Item2.LinkDirection == LinkDirection.SourceToTarget ||
                 generateEventBasedLinkResult.Item2.LinkDirection == LinkDirection.TargetToSource))
            {
                EdgeMetadata firstEdgeMetadata = RelationshipsMapping[generateEventBasedLinkResult.Item2.FirstRelationship.ID].HostEdges.First();
                EdgeMetadata secondEdgeMetadata = RelationshipsMapping[generateEventBasedLinkResult.Item2.SecondRelationship.ID].HostEdges.First();

                result = new FlowPathMetaData()
                {
                    Type = PathType.OneWay,
                    Weight = 1,
                };

                if (generateEventBasedLinkResult.Item2.LinkDirection == LinkDirection.SourceToTarget)
                    result.PathOrderedEdges = new List<EdgeMetadata>() { firstEdgeMetadata, secondEdgeMetadata };
                else
                    result.PathOrderedEdges = new List<EdgeMetadata>() { secondEdgeMetadata, firstEdgeMetadata };

                result.IsShown = result.Weight >= MinFlowPathWeightToShow && result.Weight <= MaxFlowPathWeightToShow;
            }
            else if (generateEventBasedLinkResult.Item1 == true &&
                generateEventBasedLinkResult.Item2.LinkDirection == LinkDirection.Bidirectional &&
                firstRelationship.LinkDirection == LinkDirection.Bidirectional &&
                secondRelationship.LinkDirection == LinkDirection.Bidirectional)
            {
                EdgeMetadata firstEdgeMetadata = RelationshipsMapping[generateEventBasedLinkResult.Item2.FirstRelationship.ID].HostEdges.First();
                EdgeMetadata secondEdgeMetadata = RelationshipsMapping[generateEventBasedLinkResult.Item2.SecondRelationship.ID].HostEdges.First();

                result = new FlowPathMetaData()
                {
                    Type = PathType.TwoWay,
                    Weight = 1,
                    PathOrderedEdges = new List<EdgeMetadata>() { firstEdgeMetadata, secondEdgeMetadata }
                };

                if (firstEdgeMetadata.From == secondEdgeMetadata.To)
                {
                    result.PathOrderedEdges = new List<EdgeMetadata>() { secondEdgeMetadata, firstEdgeMetadata };
                }
                result.IsShown = result.Weight >= MinFlowPathWeightToShow && result.Weight <= MaxFlowPathWeightToShow;
            }

            return result;
        }

        public static IEnumerable<int[]> Combinations(int m, int n)
        {
            int[] result = new int[m];
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();

                while (value < n)
                {
                    result[index++] = ++value;
                    stack.Push(value);

                    if (index == m)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }

        private FlowPathMetaData GenerateFlowPathFromEventBasedLink(EdgeMetadata edge)
        {
            FlowPathMetaData result = new FlowPathMetaData();
            result.Weight = edge.EventBasedLinks.Count + edge.GetNLEventBasedLinkInnerIdPairs().Count;
            result.PathOrderedEdges = new List<EdgeMetadata>() { edge };
            result.IsShown = result.Weight >= MinFlowPathWeightToShow && result.Weight <= MaxFlowPathWeightToShow;

            int eventsDirectionTypes = edge.EventBasedLinks.Select(e => e.LinkDirection).Distinct().Count();

            if (eventsDirectionTypes == 1)
            {
                LinkDirection allEventsLinkDirection = edge.EventBasedLinks.Select(e => e.LinkDirection).First();
                if (allEventsLinkDirection == LinkDirection.Bidirectional)
                {
                    result.Type = PathType.TwoWay;
                }
                else
                {
                    result.Type = PathType.OneWay;
                }
            }
            else
            {
                result.Type = PathType.TwoWay;
            }

            return result;
        }

        CancellationTokenSource RedrawFlowsCTS = new CancellationTokenSource();
        private async Task RedrawFlows()
        {
            RedrawFlowsCTS.Cancel();
            RedrawFlowsCTS = new CancellationTokenSource();
            await RedrawFlows(RedrawFlowsCTS);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task RedrawFlows(CancellationTokenSource cts)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            List<FlowPathVM> flows = new List<FlowPathVM>();

            foreach (FlowPathMetaData flow in TotalFlowPathes)
            {
                if (cts.IsCancellationRequested)
                    return;

                flows.Add(FlowPathMetaDataToFlowPathVMConverter(flow));
            }

            graphviewerMain.TotalFlowPathes = new ObservableCollection<FlowPathVM>(flows);
            graphviewerMain.RedrawFlows();
        }

        private FlowPathVM FlowPathMetaDataToFlowPathVMConverter(FlowPathMetaData flow)
        {
            FlowPathVM flowVM = new FlowPathVM()
            {
                IsShown = flow.IsShown,
                Weight = flow.Weight,
                Type = flow.Type,
                AnimateShapeFlowColor = FlowsColor,
                StaticShapeFlowColor = FlowsColor,
                FlowSpeedInMiliSeconds = FlowsSpeedInMiliSeconds,
                IncludeWeightInFlow = IncludeWeightInFlows,
                VisualStyle = FlowsVisualStyle,
                Tag = flow,
                Parent = graphviewerMain
            };

            flowVM.PathGeometryChanged += FlowVM_PathGeometryChanged;
            flowVM.UpdatePathOrderedEdges(flow.PathOrderedEdges.Select(poe => poe.RelatedEdge.RelatedEdgeControl));
            return flowVM;
        }

        bool graphChanged = true;
        bool delayStart = false;
        CancellationTokenSource FlowsAnimateSynchronizationCTS = new CancellationTokenSource();
        private async void FlowVM_PathGeometryChanged(object sender, EventArgs e)
        {
            if (IsFlowsShown == true)
            {
                if (!graphChanged)
                {
                    graphChanged = true;
                }
                if (graphChanged)
                {
                    if (!delayStart)
                    {
                        delayStart = true;
                        if (FlowsVisualStyle == FlowVisualStyle.Animated && TotalFlowPathes.Count > 50)
                            await Task.Delay(TotalFlowPathes.Count);

                        graphChanged = false;
                        await Dispatcher.BeginInvoke(new Action(async () => await FlowsAnimateSynchronization(FlowsAnimateSynchronizationCTS)), DispatcherPriority.Normal);
                        delayStart = false;
                    }
                }
            }
        }

        DateTime lastFlowsAnimateSynchronization = DateTime.Now;
        private async Task FlowsAnimateSynchronization(CancellationTokenSource cts)
        {
            if (IsFlowsShown == true)
            {
                lastFlowsAnimateSynchronization = DateTime.Now;
                foreach (var flowPath in graphviewerMain.TotalFlowPathes)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    flowPath.UpdateFlowShape();
                }

                await Task.Delay(FlowsSpeedInMiliSeconds);
                if (DateTime.Now - lastFlowsAnimateSynchronization > new TimeSpan(0, 0, 0, 0, FlowsSpeedInMiliSeconds))
                {
                    foreach (var flowPath in graphviewerMain.TotalFlowPathes)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        flowPath.UpdateFlowShape();
                    }
                }
            }
        }
    }
}