using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Ontology;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.Graph;
using GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround
{
    /// <summary>
    /// Interaction logic for CSAGraphUserControl.xaml
    /// </summary>
    public partial class CSAGraphUserControl : UserControl
    {
        CustomSearchAroundViewModel ViewModel = null;
        Ontology.Ontology ontology = null;
        private readonly Dictionary<CSAObject, Vertex> verticesDictionary = new Dictionary<CSAObject, Vertex>();
        private Vertex objectSetVertex = null;

        public CSAGraphUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = (CustomSearchAroundViewModel)DataContext;
            if (ViewModel != null)
            {
                ViewModel.SourceObjectCollectionChanged -= ViewModel_SourceObjectCollectionChanged;
                ViewModel.SourceObjectCollectionChanged += ViewModel_SourceObjectCollectionChanged;

                ViewModel.CustomSearchAroundModelChanged -= ViewModel_CustomSearchAroundModelChanged;
                ViewModel.CustomSearchAroundModelChanged += ViewModel_CustomSearchAroundModelChanged;

                if (ViewModel.CustomSearchAroundModel != null)
                {
                    ViewModel.CustomSearchAroundModel.ObjectCollectionChanged -= CustomSearchAroundModel_ObjectCollectionChanged;
                    ViewModel.CustomSearchAroundModel.ObjectCollectionChanged += CustomSearchAroundModel_ObjectCollectionChanged;
                }
            }

            ShowTargetObjectsAndLinks();

            ontology = OntologyProvider.GetOntology();
            LinkTypePickerControl.SourceTypeCollection =
                new ObservableCollection<string>(ViewModel?.SourceObjectCollection?.Select(kwo => kwo.TypeURI).Distinct());
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RemoveObject(ViewModel?.CustomSearchAroundModel?.SelectedObject);
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ClearAll();
        }

        private void LinkTypePickerControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddNewLinkButton.IsEnabled = CanAddLink();

            if (LinkTypePickerControl.SelectedItem != null &&
                OntologyProvider.GetOntology().IsRelationship(LinkTypePickerControl.SelectedItem.TypeUri))
            {
                List<string> objects = ViewModel?.SourceObjectCollection.Select(so => so.TypeURI).ToList();
                ObjectTypePickerControl.ObjectsRelatedLikeType =
                    new Tuple<string, List<string>>(LinkTypePickerControl.SelectedItem.TypeUri, objects);
            }
            else
            {
                ObjectTypePickerControl.ObjectsRelatedLikeType = new Tuple<string, List<string>>(string.Empty, null);
            }

            DeselectObjectTypePickerIfSelectedNotExists();
        }

        private void DeselectObjectTypePickerIfSelectedNotExists()
        {
            if (ObjectTypePickerControl.SelectedItem == null || LinkTypePickerControl.SelectedItem == null)
                return;

            if (OntologyProvider.GetOntology().IsRelationship(LinkTypePickerControl.SelectedItem.TypeUri))
            {
                List<string> objects = ViewModel?.SourceObjectCollection.Select(so => so.TypeURI).ToList();
                List<OntologyNode> permittedDestinationObjects =
                    CustomSearchAroundViewModel.GetPermittedObjectsForLink(LinkTypePickerControl.SelectedItem?.TypeUri, objects);

                if (permittedDestinationObjects.FirstOrDefault(pdo => pdo.TypeUri == ObjectTypePickerControl.SelectedItem?.TypeUri) == null)
                    ObjectTypePickerControl.RemoveSelectedItem();
            }
        }

        private void ObjectTypePickerControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddNewLinkButton.IsEnabled = CanAddLink();
        }

        private void AddNewLinkOnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            AddNewLinkButton.IsEnabled = CanAddLink();
        }

        private bool CanAddLink()
        {
            return ObjectTypePickerControl.SelectedItem != null && LinkTypePickerControl.SelectedItem != null;
        }

        private void AddNewLinkOnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            bool result = false;
            if (eventArgs.Parameter != null)
            {
                if (!bool.TryParse(eventArgs.Parameter.ToString(), out result))
                {
                    eventArgs.Cancel();
                    return;
                }
            }

            if (result &&
                ObjectTypePickerControl.SelectedItem?.TypeUri is string objectTypeUri &&
                LinkTypePickerControl.SelectedItem?.TypeUri is string linkTypeUri)
            {
                ViewModel?.AddObjectAndLink(objectTypeUri, linkTypeUri);
            }
        }

        private void DefectionsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DefectionListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((ListViewItem)sender).DataContext is WarningModel selectedDefection)
            {
                if (selectedDefection.RelatedElement is CSAProperty cSAProperty)
                {
                    cSAProperty.OwnerObject.IsSelected = true;
                    cSAProperty.IsSelected = true;
                }

                selectedDefection.IsSelected = true;
                selectedDefection.IsSelected = false;
            }
        }

        private void MainGraphViewer_VerticesSelectionChanged(object sender, EventArgs e)
        {

        }

        private void MainGraphViewer_EdgesSelectionChanged(object sender, EventArgs e)
        {

        }

        private void MainGraphViewer_ViewerInitializationCompleted(object sender, EventArgs e)
        {
            ShowSourceObjectSet(ViewModel?.SourceObjectCollection);
        }

        private void ViewModel_SourceObjectCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowSourceObjectSet(ViewModel?.SourceObjectCollection);
        }

        public void ShowSourceObjectSet(IEnumerable<Entities.KWObject> subGroupObjects)
        {
            if (objectSetVertex != null)
                MainGraphViewer.RemoveVertex(objectSetVertex);

            objectSetVertex = MakeObjectSetVertex(subGroupObjects);

            AddVertex(objectSetVertex, 0, 0);
            MainGraphViewer.VerticesMoveAnimationCompleted += MainGraphViewer_VerticesMoveAnimationCompleted;
        }

        private Vertex MakeObjectSetVertex(IEnumerable<Entities.KWObject> subGroupObjects)
        {
            string typeUri = ontology.InferGroupType(subGroupObjects?.Select(kwo => kwo.TypeURI).Distinct());

            List<Vertex> subGroupVertices = new List<Vertex>();
            foreach (var item in subGroupObjects)
            {
                Vertex relatedSubVertex = Vertex.VertexFactory(OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(item.TypeURI));
                subGroupVertices.Add(relatedSubVertex);
            }

            Vertex vertex = Vertex.VertexFactory(Properties.Resources.Source_Set, subGroupVertices);
            vertex.SetIconUri(OntologyIconProvider.GetTypeIconPath(typeUri).ToString());
            return vertex;
        }

        private void AddVertex(Vertex vertex, double xPos, double yPos)
        {
            if (MainGraphViewer.IsInitialized && MainGraphViewer.IsViewerInitialized)
            {
                MainGraphViewer.AddVertex(vertex, xPos, yPos);
                MainGraphViewer.AnimateVerticesAdd(new Vertex[] { vertex });
            }
        }

        private void MainGraphViewer_VerticesMoveAnimationCompleted(object sender, EventArgs e)
        {
            MainGraphViewer.VerticesMoveAnimationCompleted -= MainGraphViewer_VerticesMoveAnimationCompleted;
            MainGraphViewer.ZoomToFill();
        }

        private void ViewModel_CustomSearchAroundModelChanged(object sender, EventArgs e)
        {
            if (ViewModel.CustomSearchAroundModel != null)
            {
                ViewModel.CustomSearchAroundModel.ObjectCollectionChanged -= CustomSearchAroundModel_ObjectCollectionChanged;
                ViewModel.CustomSearchAroundModel.ObjectCollectionChanged += CustomSearchAroundModel_ObjectCollectionChanged;
            }

            ShowTargetObjectsAndLinks();
        }

        private void CustomSearchAroundModel_ObjectCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowTargetObjectsAndLinks();
        }

        private void ShowTargetObjectsAndLinks()
        {
            RemoveAllVertex();

            if (ViewModel?.CustomSearchAroundModel?.ObjectCollection == null ||
                ViewModel.CustomSearchAroundModel.ObjectCollection.Count == 0)
                return;

            int i = 0;
            double location = Math.Ceiling((double)(ViewModel.CustomSearchAroundModel.ObjectCollection.Count(o => !o.IsEvent) / 2)) - 1;

            foreach (CSAObject cSAObject in ViewModel.CustomSearchAroundModel.ObjectCollection.Where(o => !o.IsEvent))
            {
                Vertex vertex = Vertex.VertexFactory(cSAObject.Title);
                vertex.SetIconUri(OntologyIconProvider.GetTypeIconPath(cSAObject.TypeUri).ToString());
                Binding binding = new Binding(nameof(cSAObject.IsSelected))
                {
                    Source = cSAObject,
                    Mode = BindingMode.TwoWay,
                };
                vertex.RelatedVertexControl.SetBinding(VertexControl.IsSelectedProperty, binding);
                vertex.RelatedVertexControl.IsSelectedChanged += RelatedVertexControl_IsSelectedChanged;
                vertex.Tag = cSAObject;

                verticesDictionary.Add(cSAObject, vertex);

                AddVertex(vertex, MainGraphViewer.ActualWidth - 100, (i - location) * 130);
                AddEdge(cSAObject.RelatedLink);
                i++;
            }

            ViewModel.CustomSearchAroundModel.ObjectCollection.LastOrDefault().IsSelected = true;

            MainGraphViewer.VerticesMoveAnimationCompleted += MainGraphViewer_VerticesMoveAnimationCompleted;
        }

        private void RelatedVertexControl_IsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is VertexControl vertexControl && vertexControl.Vertex is Vertex vertex)
            {
                var relatedEdges = MainGraphViewer.GetEdgesRelatedTo(vertex);

                if ((bool)e.NewValue)
                {                    
                    if (!relatedEdges.FirstOrDefault().RelatedEdgeControl.IsSelected)
                    {
                        MainGraphViewer.DeselectAllEdges();
                        MainGraphViewer.SelectEdge(relatedEdges.Where(x => !(x.Tag is CSAEventBaseLink)));
                    }
                }
                else
                {
                    MainGraphViewer.DeselectEdges(relatedEdges.Where(x => !(x.Tag is CSAEventBaseLink)));
                }
                        
            }
        }

        private void AddEdge(CSALink cSALink)
        {
            Edge edge = new Edge(objectSetVertex, verticesDictionary[cSALink.RelatedObject], EdgeDirection.Bidirectional,
                cSALink.Title, OntologyIconProvider.GetTypeIconPath(cSALink.TypeUri))
            {
                Tag = cSALink
            };

            if (cSALink is CSAEventBaseLink eventBaseLink)
            {
                Binding binding = new Binding(nameof(eventBaseLink.EventObject.IsSelected))
                {
                    Source = eventBaseLink.EventObject,
                    Mode = BindingMode.TwoWay,
                };

                edge.RelatedEdgeControl.SetBinding(EdgeControl.IsSelectedProperty, binding);
            }

            edge.RelatedEdgeControl.IsSelectedChanged += RelatedEdgeControl_IsSelectedChanged;

            MainGraphViewer.AddEdge(edge);
        }

        private void RelatedEdgeControl_IsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is EdgeControl edgeControl && edgeControl.Edge is Edge edge)
            {
                if (edge.Tag is CSAEventBaseLink eventLink)
                {
                    if (edgeControl.IsSelected)
                    {                        
                        MainGraphViewer.SelectEdge(new List<Edge>() { edge });
                    }
                    else
                    {
                        MainGraphViewer.DeselectEdges(new List<Edge>() { edge });
                    }

                    eventLink.EventObject.IsSelected = edgeControl.IsSelected;
                    
                    return;
                }

                edge.Target.IsSelected = edgeControl.IsSelected;
            }
        }

        private void RemoveAllVertex()
        {
            foreach (Vertex vertex in verticesDictionary.Values)
            {
                MainGraphViewer.RemoveVertex(vertex);
            }

            verticesDictionary.Clear();
            MainGraphViewer.ZoomToCenterContent();
        }
    }
}
