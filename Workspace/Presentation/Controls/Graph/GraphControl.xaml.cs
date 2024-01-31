using GPAS.FilterSearch;
using GPAS.Graph.GraphViewer;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Graph.GraphViewer.LayoutAlgorithms;
using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Observers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class GraphControl : IObjectsFilterableListener
    {
        private int maximumShowingObjects = -1;
        private int MaximumShowingObjects
        {
            get
            {
                if (maximumShowingObjects != -1)
                {
                    return maximumShowingObjects;
                }
                else
                {
                    if (int.TryParse(ConfigurationManager.AppSettings["GraphApplication_MaximumShowingObjects"], out maximumShowingObjects))
                        return maximumShowingObjects;
                    else
                        throw new ConfigurationErrorsException("Unable to load app config 'GraphApplication_MaximumShowingObjects'"); ;
                }
            }
        }

        private int countOfUnmergeLinksWarning = -1;
        private int CountOfUnmergeLinksWarning
        {
            get
            {
                if (countOfUnmergeLinksWarning != -1)
                {
                    return countOfUnmergeLinksWarning;
                }
                else
                {
                    if (int.TryParse(ConfigurationManager.AppSettings["GraphApplication_CountOfUnmergeLinksWarning"], out countOfUnmergeLinksWarning))
                        return countOfUnmergeLinksWarning;
                    else
                        throw new ConfigurationErrorsException("Unable to load app config 'GraphApplication_CountOfUnmergeLinksWarning'"); ;
                }
            }
        }

        public bool IsControlInitialized { get { return graphviewerMain.IsViewerInitialized; } }

        #region مدیریت رخداد
        /// <summary>
        /// رخداد تغییر در اشیا انتخاب شده ی روی گراف
        /// </summary>
        public event EventHandler<EventArgs> ObjectsSelectionChanged;
        /// <summary>
        /// عملگر صدور رخداد تغییر در اشیا انتخاب شده
        /// </summary>
        protected async virtual void OnObjectsSelectionChanged()
        {
            // در صورت معرفی رخدادگردان برای رخداد مربوطه،
            if (ObjectsSelectionChanged != null)
            {
                // رخداد با آرگومان خالی صادر می شود
                ObjectsSelectionChanged(this, EventArgs.Empty);

                if (FlowsSourceObjects == Graph.Flows.SourceObjects.SelectedObjects)
                    await ResetFlowsIfShown();
            }
        }
        /// <summary>
        /// رخداد تغییر در لینک های انتخاب شده ی روی گراف
        /// </summary>
        public event EventHandler<EventArgs> EdgesSelectionChanged;
        /// <summary>
        /// عملگر صدور رخداد تغییر در لینک های انتخاب شده
        /// </summary>
        protected async virtual void OnEdgesSelectionChanged()
        {
            EdgesSelectionChanged?.Invoke(this, EventArgs.Empty);

            if (FlowsSourceObjects == Graph.Flows.SourceObjects.SelectedObjects)
                await ResetFlowsIfShown();
        }

        /// <summary>
        /// رخداد «خاتمه ی جمع شدن انیمیشنی گروه»؛
        /// این رخداد براساس گروهی که در خواست جمع شدن آن داده شده، صادر می‌شود، در نتیجه در صورت جمع‌شدن تودرتوی گروه‌ها، این رخداد فقط برای ریشه صادر خواهد شد.
        /// این رخداد برای جمع‌شدن‌هایی صادر می‌شود که به صورت انیمیشنی انجام می‌گیرند
        /// </summary>
        public event EventHandler<Graph.AnimatingCollapseGroupCompletedEventArgs> AnimatingCollapseGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی جمع شدن گروه»
        /// </summary>
        protected virtual void OnAnimatingCollapseGroupCompleted(GroupMasterKWObject collapseRootObject)
        {
            if (collapseRootObject == null)
                throw new ArgumentNullException(nameof(collapseRootObject));
            // صدور رخداد مربوطه در صورت نیاز
            if (AnimatingCollapseGroupCompleted != null)
                AnimatingCollapseGroupCompleted(this, new Graph.AnimatingCollapseGroupCompletedEventArgs(collapseRootObject));
        }

        /// <summary>
        /// رخداد «خاتمه ی باز شدن انیمیشنی گروه»؛
        /// این رخداد براساس گروهی که درخواست باز شدن آن داده شده، صادر می‌شود، در نتیجه در صورت جمع‌باز شدن تودرتوی گروه‌ها، این رخداد فقط برای ریشه صادر خواهد شد.
        /// این رخداد برای باز‌شدن‌هایی صادر می‌شود که به صورت انیمیشنی انجام می‌گیرند
        /// </summary>
        public event EventHandler<Graph.AnimatingExpandGroupCompletedEventArgs> AnimatingExpandGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی باز شدن گروه»
        /// </summary>
        protected virtual void OnAnimatingExpandGroupCompleted(GroupMasterKWObject expandRootObject)
        {
            if (expandRootObject == null)
                throw new ArgumentNullException(nameof(expandRootObject));
            // صدور رخداد مربوطه در صورت نیاز
            if (AnimatingExpandGroupCompleted != null)
                AnimatingExpandGroupCompleted(this, new Graph.AnimatingExpandGroupCompletedEventArgs(expandRootObject));
        }

        /// <summary>
        /// رخداد «کلیک راست بر روی گراف»؛        
        /// </summary>
        public event EventHandler<GraphContentRightClickEventArgs> GraphContentRightClick;
        /// <summary>
        /// عملگر صدور رخداد «کلیک راست بر روی گراف»
        /// </summary>
        protected virtual void OnGraphContentRightClick(Point clickPoint)
        {
            if (clickPoint == null)
                throw new ArgumentNullException(nameof(clickPoint));
            // صدور رخداد مربوطه در صورت نیاز
            if (GraphContentRightClick != null)
                GraphContentRightClick(this, new GraphContentRightClickEventArgs(clickPoint));
        }

        public event EventHandler<DoubleClickedVertexEventArgs> ObjectDoubleClicked;

        private void OnObjectDoubleClicked(KWObject doubleClickedObject)
        {
            if (doubleClickedObject == null)
                throw new ArgumentNullException(nameof(doubleClickedObject));

            if (ObjectDoubleClicked != null)
                ObjectDoubleClicked(this, new DoubleClickedVertexEventArgs(doubleClickedObject));
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler ControlInitializationCompleted;
        protected void OnControlInitializationCompleted()
        {
            if (ControlInitializationCompleted != null)
                ControlInitializationCompleted.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ObjectsAdded;
        protected async Task OnObjectsAdded()
        {
            if (!unmergingLinks)
                await ResetFlowsIfShown();

            ApplyFilterOnAddedObjects();

            if (!hasChangedObjectsMapping)
                return;

            hasChangedObjectsMapping = false;

            ObjectsAdded?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ObjectsRemoved;
        protected async Task OnObjectsRemoved()
        {
            await ResetFlowsIfShown();

            if (!hasChangedObjectsMapping)
                return;

            hasChangedObjectsMapping = false;

            ObjectsRemoved?.Invoke(this, EventArgs.Empty);
        }

        protected async Task OnLinksAdded()
        {
            await ResetFlowsIfShown();
        }
        protected async Task OnLinksRemoved()
        {
            await ResetFlowsIfShown();
        }
        protected async Task OnObjectsFilterChanged()
        {
            await ResetFlowsIfShown();
        }

        #endregion

        /// <summary>
        /// سازنده کنترل
        /// </summary>
        public GraphControl()
        {
            InitializeComponent();
            DataContext = this;

            graphviewerMain.VerticesSelectionChanged += graphviewerMain_VerticesSelectionChanged;
            graphviewerMain.EdgesSelectionChanged += graphviewerMain_EdgesSelectionChanged;
            graphviewerMain.VertexRemoved += graphviewerMain_VertexRemoved;
            graphviewerMain.EdgeRemoved += graphviewerMain_EdgeRemoved;
            graphviewerMain.GraphCleared += graphviewerMain_GraphCleared;
        }

        internal GraphViewer Viewer { get { return graphviewerMain; } }

        private async void graphviewerMain_GraphCleared(object sender, EventArgs e)
        {
            ClearMappings();

            await OnObjectsRemoved();
        }

        private void ClearMappings()
        {
            foreach (var item in ObjectsMapping.Keys)
                RemoveBindingsOfObject(item);
            try
            {
                TotalEdges.Clear();
                ClearObjectsMapping();
            }
            catch
            {
                KWMessageBox.Show(Properties.Resources.Unable_To_Clear_Graph_Mappings);
            }
        }

        private void ClearObjectsMapping()
        {
            if (ObjectsMapping.Count > 0)
            {
                ObjectsMapping.Clear();
                hasChangedObjectsMapping = true;
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async void graphviewerMain_VertexRemoved(object sender, VertexRemovedEventArgs e)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            RemoveObjectMapping(e.RemovedVertex);
        }

        private void RemoveObjectMapping(Vertex vertexToRemoveMapping)
        {
            if (vertexToRemoveMapping == null)
                throw new ArgumentNullException(nameof(vertexToRemoveMapping));

            try
            {
                var relatedObject = ObjectsMapping.Single
                    (checkingMapping => checkingMapping.Value == vertexToRemoveMapping).Key;
                // حذف نگاشت شئ به گرهی که قراراست از گراف حذف شود؛
                // حذف این نگاشت منحصرا از این طریق صورت می گیرد

                RemoveFromObjectsMapping(relatedObject);
                RemoveBindingsOfObject(relatedObject);
            }
            catch (InvalidOperationException)
            {
                KWMessageBox.Show(Properties.Resources.Unable_To_Reach_Unique_Mapping_For_The_Vertex);
            }
            catch
            {
                KWMessageBox.Show(Properties.Resources.Unable_To_Remove_Object_Mapping);
            }
        }

        private void RemoveFromObjectsMapping(KWObject key)
        {
            if (ObjectsMapping.Remove(key))
                hasChangedObjectsMapping = true;
        }

        private void RemoveBindingsOfObject(KWObject relatedObject)
        {
            if (relatedObject.DisplayName != null)
            {
                relatedObject.DisplayName.ValueChanged -= ApplyShowingObjectDisplayNameChange;
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async void graphviewerMain_EdgeRemoved(object sender, EdgeRemovedEventArgs e)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            RemoveLinkMapping(e.RemovedEdge);
        }

        private void RemoveLinkMapping(Edge edgeToRemoveMapping)
        {
            if (edgeToRemoveMapping == null)
                throw new ArgumentNullException(nameof(edgeToRemoveMapping));

            // حذف نگاشت لینک به یالی که قراراست از گراف حذف شود؛
            // حذف این نگاشت منحصرا از این طریق صورت می گیرد
        }

        //private void DeBindDeletedChangeToLink(KWLink c)
        //{
        //    if (c is RelationshipBasedKWLink)
        //    {
        //        ((c as RelationshipBasedKWLink).Relationship).Deleted -= GraphControl_Deleted;
        //    }
        //    else if (c is EventBasedKWLink)
        //    {
        //        ((c as EventBasedKWLink)).FirstRelationship.Deleted -= GraphControl_Deleted;
        //        ((c as EventBasedKWLink)).SecondRelationship.Deleted -= GraphControl_Deleted;
        //    }
        //    else if (c is PropertyBasedKWLink)
        //    {
        //        return;
        //    }
        //}

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        /// <summary>
        /// رخدادگردان تغییر در گره های در حالت انتخاب روی نمایشگر گراف
        /// </summary>
        private async void graphviewerMain_VerticesSelectionChanged(object sender, EventArgs e)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            // رخداد تغییر در اشیا انتخاب شده را فراخوانی می کند
            OnObjectsSelectionChanged();
        }


#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async void graphviewerMain_EdgesSelectionChanged(object sender, EventArgs e)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            OnEdgesSelectionChanged();
        }

        /// <summary>
        /// دیکشنری نگاشت اشیا در حال نمایش به گره های مربوطه در نمایش دهنده گراف
        /// </summary>
        private Dictionary<KWObject, Vertex> ObjectsMapping = new Dictionary<KWObject, Vertex>();
        bool hasChangedObjectsMapping = false;

        /// <summary>
        /// اشیا انتخاب شده در گراف را برمی گرداند
        /// </summary>
        public IEnumerable<KWObject> GetSelectedObjects()
        {
            return ObjectsMapping
                .Where(map => map.Value.IsSelected)
                .Select(map => map.Key);
        }

        public KWObject GetRelatedObject(Vertex vertex)
        {
            return ObjectsMapping.Single(mapping => mapping.Value.Equals(vertex)).Key;
        }

        public RenderTargetBitmap TakeImageOfGraph()
        {
            return graphviewerMain.TakeImageOfGraphCurrentShowingArea();
        }

        public void TakeSnapshot()
        {
            UIElement uIElement = graphviewerMain.GetUIElementForSnapshot(true);

            OnSnapshotRequested(new Windows.SnapshotRequestEventArgs(uIElement, $"Graph Application {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}"));
        }

        /// <summary>
        /// عکس گرفته شده را به آرایه ای از بایتها تبدیل می کند.
        /// </summary>
        /// <returns>آرایه بایت</returns>
        public byte[] RenderTargetBitmapToByteArrangement(RenderTargetBitmap graphImageToPreview)
        {
            //عکس گرفته شده را به png تبدیل می کند.
            BitmapEncoder encoder = new PngBitmapEncoder();
            BitmapFrame frame = BitmapFrame.Create(graphImageToPreview);
            encoder.Frames.Add(frame);

            //عکس را در یک فایل موقت ذخیره می کند و بایتهای آنرا می خواند.
            string tempFileName = Path.GetTempFileName();

            FileStream fs = File.Open(tempFileName, FileMode.Create);
            encoder.Save(fs);
            fs.Close();
            byte[] graphImageArrangement = File.ReadAllBytes(tempFileName);
            File.Delete(tempFileName);

            return graphImageArrangement;
        }

        public BitmapImage ConvertRenderTargetBitmapToBitmapImage(RenderTargetBitmap renderTargetBitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public bool IsAnyLinkSelected()
        {
            return TotalEdges
                .Any(metadata => graphviewerMain.IsSelectedEdge(metadata.RelatedEdge));
        }
        public bool IsAnyUnmergableLinkSelected()
        {
            return TotalEdges
                .Any(metadata => metadata.IsUnmergable());
        }

        public List<KWLink> GetAllLinks()
        {
            List<KWLink> selectedLinks = new List<KWLink>();
            foreach (EdgeMetadata metadata in TotalEdges)
            {
                selectedLinks.AddRange(metadata.RelationshipBasedLinks);
                selectedLinks.AddRange(metadata.EventBasedLinks);
                selectedLinks.AddRange(metadata.PropertyBasedLinks);
                selectedLinks.Add(metadata.GetNotLoadedRelationshipBasedKWLink());
                selectedLinks.Add(metadata.GetNotLoadedEventBasedKWLink());
            }
            return selectedLinks;
        }

        public List<KWLink> GetSelectedLinks()
        {
            List<KWLink> selectedLinks = new List<KWLink>();
            foreach (EdgeMetadata metadata
                in TotalEdges.Where(metadata => graphviewerMain.IsSelectedEdge(metadata.RelatedEdge)))
            {
                selectedLinks.AddRange(metadata.RelationshipBasedLinks);
                selectedLinks.AddRange(metadata.EventBasedLinks);
                selectedLinks.AddRange(metadata.PropertyBasedLinks);
                selectedLinks.Add(metadata.GetNotLoadedRelationshipBasedKWLink());
                selectedLinks.Add(metadata.GetNotLoadedEventBasedKWLink());
            }
            return selectedLinks;
        }
        /// <summary>
        /// تعداد لینک‌های متناظر با یال‌های انتخاب شده را با احتساب اجزای لینک‌های بارگذاری نشده برمی‌گرداند
        /// </summary>
        /// <returns></returns>
        public int GetSelectedLinksCount()
        {
            int counter = 0;
            foreach (EdgeMetadata metadata
                in TotalEdges.Where(metadata => graphviewerMain.IsSelectedEdge(metadata.RelatedEdge)))
            {
                counter += metadata.GetAllLinksCount();
            }
            return counter;
        }

        public List<KWLink> GetRelatedLinks(List<KWObject> objects)
        {
            HashSet<KWObject> objectsHashSet = new HashSet<KWObject>(objects);
            return TotalEdges
                .Where(edgeMetadata => objectsHashSet.Contains(edgeMetadata.From) || objectsHashSet.Contains(edgeMetadata.To))
                .SelectMany(edgeMetadata => edgeMetadata.GetAllLinks())
                .ToList();
        }

        public void SelectAllObjects()
        {
            graphviewerMain.SelectAllVertices();
        }
        public void DeselectAllObjects()
        {
            graphviewerMain.DeselectAllVertices();
        }
        public void SelectOrphansObjects()
        {
            graphviewerMain.SelectOrphansVertices();
        }

        public void InvertSelectionObjects()
        {
            graphviewerMain.InvertSelectionVertices();
        }
        public void ExpandNodeSelectiontoNextLinkedLevel()
        {
            graphviewerMain.ExpandNodeSelectiontoNextLinkedLevel();
        }
        public void SetNodeSelectiontoNextLinkedLevel()
        {
            graphviewerMain.SetNodeSelectiontoNextLinkedLevel();
        }
        public void InvertSelectionLinks()
        {
            graphviewerMain.InvertSelectionEdges();
        }
        public void SelectAllLinks()
        {
            graphviewerMain.SelectAllEdges();
        }
        public void DeselectAllLinks()
        {
            graphviewerMain.DeselectAllEdges();
        }
        public void SelectOuterLinksOfSelectedObject()
        {
            graphviewerMain.SelectOuterEdgesOfSelectedVertices();
        }
        public void SelectOuterObjectsOfSelectedObject()
        {
            graphviewerMain.SelectOuterObjectsOfSelectedVertices();
        }

        public void SelectInnerLinksOfSelectedObject()
        {
            graphviewerMain.SelectInnerEdgesOfSelectedVertices();
        }

        public void SelectInnerObjectsOfSelectedObject()
        {
            graphviewerMain.SelectInnerObjectsOfSelectedVertices();
        }
        public void SelectLinksBetweenSelectedObject()
        {
            graphviewerMain.SelectLinksBetweenSelectedVertex();
        }

        public void SelectAllLinksOfSelectedObject()
        {
            graphviewerMain.SelectAllEdgesOfSelectedVertices();
        }
        public void DeselectObjects(IEnumerable<KWObject> objectsToDeselect)
        {
            IEnumerable<Vertex> selectedVertices = GetRelatedVertices(objectsToDeselect);
            graphviewerMain.DeselectVertices(selectedVertices);
        }

        private Point GetGraphCurrentViewCenterPosition()
        {
            return graphviewerMain.GetCurrentViewCenterPosition();
        }

        public async Task UnmergeNotLoadedEventBasedMasterLinks(List<NotLoadedEventBasedKWLink> selectedNotLoadedEventBasedLinks)
        {
            if (selectedNotLoadedEventBasedLinks.Count == 0)
                return;

            var loadingInnerRelationships = new List<EventBasedResultInnerRelationships>(selectedNotLoadedEventBasedLinks.Count);
            foreach (var item in selectedNotLoadedEventBasedLinks)
            {
                loadingInnerRelationships.Add(item.IntermediaryLinksRelationshipIDs.First());
            }

            Dictionary<long, RelationshipBasedKWLink> retrieveResultsPerRelID
                = (await LinkManager.RetrieveRelationshipBaseLinksAsync
                    (loadingInnerRelationships.Select(n => n.FirstRelationshipID)
                    .Concat(loadingInnerRelationships.Select(n => n.SecondRelationshipID))
                    .Distinct().ToList()))
                    .ToDictionary(r => r.Relationship.ID);

            IEnumerable<EventBasedKWLink> expandingLinks = loadingInnerRelationships.Select
                (n => LinkManager.GetEventBaseKWLinkFromLinkInnerRelationships
                        (retrieveResultsPerRelID[n.FirstRelationshipID], retrieveResultsPerRelID[n.SecondRelationshipID]));

            try
            {
                List<ObjectShowMetadata> showMetadata
                    = GetObjectShowMetadatas
                        (expandingLinks.SelectMany(l => new KWObject[] { l.Source, l.Target }));
                // ترکیب شوند
                ShowObjects(showMetadata);
                ShowLinks(expandingLinks);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<ObjectShowMetadata> GetObjectShowMetadatas(IEnumerable<KWObject> objectsToShow)
        {
            List<ObjectShowMetadata> result = new List<ObjectShowMetadata>();
            foreach (KWObject obj in objectsToShow)
            {
                result.Add(new ObjectShowMetadata()
                {
                    ObjectToShow = obj
                });
            }
            return result;
        }

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            graphviewerMain.DeselectAllVertices();
            List<Vertex> verticesToSelect = new List<Vertex>();
            foreach (var item in objectsToSelect)
            {
                Vertex relatedVertex;
                if ((relatedVertex = GetRelatedVertex(item)) != null)
                    verticesToSelect.Add(relatedVertex);
            }
            graphviewerMain.SelectVertices(verticesToSelect);
        }

        public IEnumerable<NotLoadedEventBasedKWLink> GetNotLoadedEventBasedMasterLinksFor(RelationshipBasedKWLink link)
        {
            return TotalEdges
                .Select(m => m.GetNotLoadedEventBasedKWLink())
                .Where(showingLink => showingLink.ContainsLink(link));
        }

        Dictionary<long, RelationshipShowMetadata> RelationshipsMapping = new Dictionary<long, RelationshipShowMetadata>();
        HashSet<EdgeMetadata> TotalEdges = new HashSet<EdgeMetadata>();

        /// <summary>
        /// Get the single Edge between two Objects (Vertices) if exist or Create it if not exist
        /// </summary>
        /// <returns>Given/Created Edge</returns>
        private EdgeMetadata GetEdgeBetweenObjects(KWObject from, KWObject to)
        {
            EdgeMetadata newEdge = new EdgeMetadata(from, to);
            EdgeMetadata existEdge = new EdgeMetadata(from, to);
            if (TotalEdges.TryGetValue(newEdge, out existEdge))
            {
                return existEdge;
            }
            else
            {
                TotalEdges.Add(newEdge);
                return newEdge;
            }
        }

        private void RemoveEdgeIfIsEmptyOrPendToShowIfIsNotEmpty(EdgeMetadata e)
        {
            if (e.IsEmpty())
                RemoveEdge(e);
            else
                PendEdgeToShow(e);
        }
        private void RemoveEdgeIfIsEmpty(EdgeMetadata e)
        {
            if (e.IsEmpty())
                RemoveEdge(e);
        }
        private void RemoveEdge(EdgeMetadata edgeMetadata)
        {
            if (PendingEdgesToShow.Contains(edgeMetadata))
                PendingEdgesToShow.Remove(edgeMetadata);
            if (PendingEdgesToSelect.Contains(edgeMetadata))
                PendingEdgesToSelect.Remove(edgeMetadata);
            if (edgeMetadata.IsShownOnGraph)
                graphviewerMain.RemoveEdge(edgeMetadata.RelatedEdge);
            if (TotalEdges.Contains(edgeMetadata))
                TotalEdges.Remove(edgeMetadata);
            UpdateRelationshipMappingAccordingToEdgeRemove(edgeMetadata);
        }
        private void RemoveEdges(List<EdgeMetadata> edgesToRemove)
        {
            foreach (EdgeMetadata edgeMetadata in edgesToRemove)
            {
                RemoveEdge(edgeMetadata);
            }
        }

        List<KWLink> PendingLinksToShow = new List<KWLink>();
        HashSet<EdgeMetadata> PendingEdgesToShow = new HashSet<EdgeMetadata>();
        HashSet<EdgeMetadata> PendingEdgesToSelect = new HashSet<EdgeMetadata>();
        HashSet<EdgeMetadata> PendingEdgesToUpdateWithoutSelection = new HashSet<EdgeMetadata>();

        /// <summary>
        /// نمایش لینک‌ها روی گراف
        /// </summary>
        /// <remarks>
        /// در پیاده‌سازی این تابع سعی شده ابتدا تغییرات در ساختمان‌داده‌ی متناظر لینک‌ها و اشیاء
        /// به طور کامل اعمال شود، سپس تغییرات به صورت یکجا در نمایش‌دهنده‌ی گراف اعمال شوند
        /// </remarks>
        public void ShowLinks(IEnumerable<KWLink> linksToShow)
        {
            if (linksToShow == null)
                throw new ArgumentNullException(nameof(linksToShow));
            if (!linksToShow.Any())
                return;
            try
            {
                PendLinksToShow(linksToShow);
                ShowPendentVertices();
                ShowPendentEdges();
                graphviewerMain.DeselectAllVertices();
                SelectPendentEdges();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnLinksAdded();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        private void ShowPendentEdges()
        {
            if (PendingEdgesToShow.Count == 0)
                return;
            try
            {
                graphviewerMain.AddEdgeRange(PendingEdgesToShow.Select(m => m.RelatedEdge));
                foreach (EdgeMetadata edgeMetadataToAdd in PendingEdgesToShow)
                {
                    edgeMetadataToAdd.IsShownOnGraph = true;
                    if (edgeMetadataToAdd.To is GroupMasterKWObject
                     && edgeMetadataToAdd.RelationshipBasedLinks.Any(l => l.TypeURI == OntologyProvider.GetOntology().DefaultGroupRelationshipType()))
                    {
                        (GetRelatedVertex(edgeMetadataToAdd.To) as GroupMasterVertex).AddSubGroup(GetRelatedVertex(edgeMetadataToAdd.From), graphviewerMain);
                    }
                }
                //graphviewerMain.UpdateLayout();
            }
            finally
            {
                PendingEdgesToShow.Clear();
            }
        }

        private void SelectPendentEdges()
        {
            if (PendingEdgesToSelect.Count == 0)
                return;
            try
            {
                graphviewerMain.DeselectAllEdges();
                graphviewerMain.SelectEdge(PendingEdgesToSelect.Select(m => m.RelatedEdge));
            }
            finally
            {
                PendingEdgesToSelect.Clear();
            }
        }

        private void PendLinksToShow(IEnumerable<KWLink> linksToShow)
        {
            PendingEdgesToUpdateWithoutSelection.Clear();
            try
            {
                PendingLinksToShow.AddRange(linksToShow);
                while (PendingLinksToShow.Count > 0)
                {
                    KWLink linkToShow = PendingLinksToShow[0];
                    PendingLinksToShow.Remove(linkToShow);

                    if (GetRelatedVertex(linkToShow.Source) == null)
                        throw new InvalidOperationException(Properties.Resources.Source_Object_Of_The_Link_May_Be_Added_To_Graph);
                    if (GetRelatedVertex(linkToShow.Target) == null)
                        throw new InvalidOperationException(Properties.Resources.Target_Object_Of_The_Link_May_Be_Added_To_Graph);

                    EdgeMetadata edgeMetadata = GetEdgeBetweenObjects(linkToShow.Source, linkToShow.Target);
                    if (linkToShow is RelationshipBasedKWLink)
                    {
                        RelationshipShowMetadata relMetadata;
                        long relID = ((RelationshipBasedKWLink)linkToShow).Relationship.ID;
                        if (RelationshipsMapping.TryGetValue(relID, out relMetadata))
                        {
                            ApplyAddNotLoadedRelBasedLinkMappingChanges
                                ((RelationshipBasedKWLink)linkToShow, relMetadata, edgeMetadata);
                            foreach (EdgeMetadata hostMetadata in relMetadata.HostEdges)
                            {
                                PendEdgeToShow(hostMetadata);
                            }
                        }
                        else
                        {
                            edgeMetadata.RelationshipBasedLinks.Add((RelationshipBasedKWLink)linkToShow);
                            relMetadata = AddNewRelationshipMapping(relID, edgeMetadata);
                            PendEdgeToShow(edgeMetadata);
                        }
                    }
                    else if (linkToShow is EventBasedKWLink)
                    {
                        EventBasedKWLink evLinkToShow = (EventBasedKWLink)linkToShow;
                        if (IsObjectShowing(evLinkToShow.IntermediaryEvent))
                        {
                            if (edgeMetadata.EventBasedLinks.Contains(evLinkToShow))
                                edgeMetadata.EventBasedLinks.Remove(evLinkToShow);
                            var innerLinks = LinkManager.ConvertEventBaseKWLinkToRelationshipBasedKWLink(evLinkToShow);
                            UpdateRelationshipMappingAccordingToEdgeChanges(innerLinks.Item1.Relationship.ID, edgeMetadata);
                            PendingLinksToShow.Add(innerLinks.Item1);
                            UpdateRelationshipMappingAccordingToEdgeChanges(innerLinks.Item2.Relationship.ID, edgeMetadata);
                            PendingLinksToShow.Add(innerLinks.Item2);
                            if (edgeMetadata.IsEmpty())
                                RemoveEdge(edgeMetadata);
                            else
                                PendingEdgesToUpdateWithoutSelection.Add(edgeMetadata);
                        }
                        else
                        {
                            if (edgeMetadata.EventBasedLinks.Contains(evLinkToShow))
                            {
                                PendEdgeToSelect(edgeMetadata);
                            }
                            else
                            {
                                ApplyAddEventBasedLinkMappingChanges
                                    (evLinkToShow, edgeMetadata);
                                if (TotalEdges.Contains(edgeMetadata))
                                {
                                    if (!edgeMetadata.EventBasedLinks.Contains(evLinkToShow))
                                        edgeMetadata.EventBasedLinks.Add(evLinkToShow);
                                    PendEdgeToShow(edgeMetadata);
                                }
                            }
                        }
                    }
                    else if (linkToShow is NotLoadedRelationshipBasedKWLink)
                    {
                        NotLoadedRelationshipBasedKWLink nlrLinkToShow = (NotLoadedRelationshipBasedKWLink)linkToShow;
                        if (nlrLinkToShow.IntermediaryRelationshipIDs.Count > 0)
                        {
                            foreach (long innerRelID in nlrLinkToShow.IntermediaryRelationshipIDs)
                            {
                                if (edgeMetadata.RelationshipBasedLinks.Any(l => l.Relationship.ID.Equals(innerRelID)))
                                {
                                    PendEdgeToSelect(edgeMetadata);
                                    continue;
                                }
                                if (!edgeMetadata.NLRelationshipBasedLinkIDs.Contains(innerRelID))
                                    edgeMetadata.NLRelationshipBasedLinkIDs.Add(innerRelID);
                                RelationshipShowMetadata innerRelMetadata;
                                if (RelationshipsMapping.TryGetValue(innerRelID, out innerRelMetadata))
                                {
                                    if (!innerRelMetadata.HostEdges.Contains(edgeMetadata))
                                        innerRelMetadata.HostEdges.Add(edgeMetadata);
                                    PendEdgeRangeToSelect(innerRelMetadata.HostEdges);
                                }
                                else
                                {
                                    innerRelMetadata = AddNewRelationshipMapping(innerRelID, edgeMetadata, false);
                                }
                            }
                            RemoveEdgeIfIsEmptyOrPendToShowIfIsNotEmpty(edgeMetadata);
                        }
                    }
                    else if (linkToShow is NotLoadedEventBasedKWLink)
                    {
                        NotLoadedEventBasedKWLink nleLinkToShow = (NotLoadedEventBasedKWLink)linkToShow;
                        if (nleLinkToShow.IntermediaryLinksRelationshipIDs.Count > 0)
                        {
                            foreach (EventBasedResultInnerRelationships innerRelIDPair in nleLinkToShow.IntermediaryLinksRelationshipIDs)
                            {
                                RelationshipShowMetadata innerRel1Metadata;
                                bool innerRel1Exist = RelationshipsMapping.TryGetValue(innerRelIDPair.FirstRelationshipID, out innerRel1Metadata);
                                RelationshipShowMetadata innerRel2Metadata;
                                bool innerRel2Exist = RelationshipsMapping.TryGetValue(innerRelIDPair.SecondRelationshipID, out innerRel2Metadata);

                                if (innerRel1Exist && innerRel1Metadata.IsCached
                                 && innerRel2Exist && innerRel2Metadata.IsCached)
                                {
                                    PendEdgeRangeToShow(innerRel1Metadata.HostEdges);
                                    PendEdgeRangeToShow(innerRel2Metadata.HostEdges);
                                }
                                else if ((!innerRel1Exist || !innerRel1Metadata.IsCached)
                                      && (!innerRel2Exist || !innerRel2Metadata.IsCached))
                                {
                                    edgeMetadata.AddNewNLEventBasedLinkInnerIDPairIfNotExist(innerRelIDPair);
                                    if (!innerRel1Exist)
                                    {
                                        AddNewRelationshipMapping(innerRelIDPair.FirstRelationshipID, edgeMetadata, false);
                                    }
                                    else
                                    {
                                        if (!innerRel1Metadata.HostEdges.Contains(edgeMetadata))
                                            innerRel1Metadata.HostEdges.Add(edgeMetadata);
                                    }
                                    if (!innerRel2Exist)
                                    {
                                        AddNewRelationshipMapping(innerRelIDPair.SecondRelationshipID, edgeMetadata, false);
                                    }
                                    else
                                    {
                                        if (!innerRel2Metadata.HostEdges.Contains(edgeMetadata))
                                            innerRel2Metadata.HostEdges.Add(edgeMetadata);
                                    }
                                    PendingEdgesToUpdateWithoutSelection.Add(edgeMetadata);
                                }
                                else // = One Rel is cached, and other one is not cached
                                {
                                    RelationshipBasedKWLink innerLoadedRelLink;
                                    if (innerRel1Exist && innerRel1Metadata.IsCached)
                                        innerLoadedRelLink = LinkManager.GetCachedRelationshipBasedKWLink(innerRelIDPair.FirstRelationshipID);
                                    else if (innerRel2Exist && innerRel2Metadata.IsCached)
                                        innerLoadedRelLink = LinkManager.GetCachedRelationshipBasedKWLink(innerRelIDPair.SecondRelationshipID);
                                    else
                                        throw new InvalidOperationException();
                                    AddNotLoadedSideOfPairToNlrList(edgeMetadata, innerLoadedRelLink, innerRelIDPair);
                                }
                            }
                            RemoveEdgeIfIsEmptyOrPendToShowIfIsNotEmpty(edgeMetadata);
                        }
                    }
                    else if (linkToShow is PropertyBasedKWLink)
                    {
                        edgeMetadata.PropertyBasedLinks.Add((PropertyBasedKWLink)linkToShow);
                        PendEdgeToShow(edgeMetadata);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                EdgeMetadata[] PendingEdgesToShowArray = PendingEdgesToShow.ToArray();
                for (int i = 0; i < PendingEdgesToShowArray.Length; i++)
                {
                    EdgeMetadata edgeMetadataToAdd = PendingEdgesToShowArray[i];
                    if (edgeMetadataToAdd.IsEmpty())
                    {
                        RemoveEdge(edgeMetadataToAdd);
                        continue;
                    }
                    if (!edgeMetadataToAdd.IsShownOnGraph)
                    {
                        Edge edge = GraphControlEdgeManager.EdgeFactory(edgeMetadataToAdd, this);
                        AddEdgeMapping(edgeMetadataToAdd, edge);
                    }
                    else
                    {
                        PendingEdgesToShow.Remove(edgeMetadataToAdd);
                        if (!PendingEdgesToUpdateWithoutSelection.Contains(edgeMetadataToAdd))
                            PendingEdgesToUpdateWithoutSelection.Add(edgeMetadataToAdd);
                    }
                    edgeMetadataToAdd.UpdateLayout();
                }
                foreach (EdgeMetadata edgeMetadata in PendingEdgesToUpdateWithoutSelection)
                {
                    if (edgeMetadata.IsEmpty())
                    {
                        RemoveEdge(edgeMetadata);
                        continue;
                    }
                    edgeMetadata.UpdateLayout();
                }
            }
            catch
            {
                PendingLinksToShow.Clear();
                PendingEdgesToSelect.Clear();
                PendingEdgesToUpdateWithoutSelection.Clear();
                throw;
            }
        }

        private void AddEdgeMapping(EdgeMetadata edgeMetadataToAdd, Edge edge)
        {
            edgeMetadataToAdd.RelatedEdge = edge;
            //totalEdges.Add(edgeMetadataToAdd); => Added at creation time!
            edge.Tag = edgeMetadataToAdd;
        }

        private RelationshipShowMetadata AddNewRelationshipMapping(long relID, EdgeMetadata hostEdgeMetadata, bool isRelCached = true)
        {
            RelationshipShowMetadata relMetadata = new RelationshipShowMetadata();
            relMetadata.IsCached = isRelCached;
            relMetadata.HostEdges.Add(hostEdgeMetadata);
            RelationshipsMapping.Add(relID, relMetadata);
            return relMetadata;
        }

        private void ApplyAddEventBasedLinkMappingChanges(EventBasedKWLink linkToShow, EdgeMetadata edgeMetadata)
        {
            RelationshipShowMetadata rel1Metadata;
            RelationshipsMapping.TryGetValue(linkToShow.FirstRelationship.ID, out rel1Metadata);
            RelationshipShowMetadata rel2Metadata;
            RelationshipsMapping.TryGetValue(linkToShow.SecondRelationship.ID, out rel2Metadata);

            if (rel1Metadata != null && rel2Metadata != null)
            {
                ApplyAddNotLoadedEventBasedLinkMappingChanges
                    (linkToShow, rel1Metadata, rel2Metadata, edgeMetadata);
            }
            else
            {
                if (rel1Metadata != null)
                {
                    ApplyAddNotLoadedEventBasedLinkSubRelationshipMappingChanges
                        (linkToShow.FirstRelationship, rel1Metadata, edgeMetadata);
                }
                if (rel2Metadata != null)
                {
                    ApplyAddNotLoadedEventBasedLinkSubRelationshipMappingChanges
                        (linkToShow.SecondRelationship, rel2Metadata, edgeMetadata);
                }
            }

            if (rel1Metadata == null)
                rel1Metadata = AddNewRelationshipMapping(linkToShow.FirstRelationship.ID, edgeMetadata);
            if (rel2Metadata == null)
                rel2Metadata = AddNewRelationshipMapping(linkToShow.SecondRelationship.ID, edgeMetadata);
        }

        private void PendEdgeRangeToShow(IEnumerable<EdgeMetadata> edgesMetadata)
        {
            foreach (EdgeMetadata metadata in edgesMetadata)
            {
                PendEdgeToShow(metadata);
            }
        }
        private void PendEdgeToShow(EdgeMetadata edgeMetadata)
        {
            if (!PendingEdgesToShow.Contains(edgeMetadata))
                PendingEdgesToShow.Add(edgeMetadata);
            PendEdgeToSelect(edgeMetadata);
        }

        private void PendEdgeRangeToSelect(IEnumerable<EdgeMetadata> edgesMetadata)
        {
            foreach (EdgeMetadata metadata in edgesMetadata)
            {
                PendEdgeToSelect(metadata);
            }
        }
        private void PendEdgeToSelect(EdgeMetadata hostMetadata)
        {
            if (!PendingEdgesToSelect.Contains(hostMetadata))
                PendingEdgesToSelect.Add(hostMetadata);
        }

        private void ApplyAddNotLoadedEventBasedLinkSubRelationshipMappingChanges
            (KWRelationship subRelationship, RelationshipShowMetadata relMetadata
            , EdgeMetadata edgeMetadata)
        {
            if (!relMetadata.HostEdges.Contains(edgeMetadata))
                relMetadata.HostEdges.Add(edgeMetadata);
            RelationshipBasedKWLink subRelationshipEquivalentLink = LinkManager.GetRelationshipBasedKWLink(subRelationship);
            EdgeMetadata[] hostEdgesArray = relMetadata.HostEdges.ToArray();
            for (int i = 0; i < hostEdgesArray.Length; i++)
            {
                EdgeMetadata hostEdge = hostEdgesArray[i];
                if (hostEdge.NLRelationshipBasedLinkIDs.Contains(subRelationship.ID)
                    && !hostEdge.RelationshipBasedLinks.Contains(subRelationshipEquivalentLink))
                {
                    hostEdge.RelationshipBasedLinks.Add(subRelationshipEquivalentLink);
                    hostEdge.NLRelationshipBasedLinkIDs.Remove(subRelationship.ID);
                    relMetadata.IsCached = true;
                }
                // شناسایی لینک‌های مبتنی بر رخداد لود نشده‌ای که رابطه‌ی مورد بررسی فقط یک طرف
                // آن را تشکیل می‌دهد؛ تضمین این قضیه از فراخوانی تابع
                // ApplyAddNotLoadedEventBasedLinkMappingChanges
                // فراهم می‌شود
                HashSet<EventBasedResultInnerRelationships> hostEdgeRelatedNLEs;
                if (hostEdge.TryGetNLEventBasedLinkInnerIDPairsThatContainsRel(subRelationship.ID, out hostEdgeRelatedNLEs)
                    && IsObjectShowing(subRelationshipEquivalentLink.Source)
                    && IsObjectShowing(subRelationshipEquivalentLink.Target))
                {
                    var relatedNLEsArray = hostEdgeRelatedNLEs.ToArray();
                    for (int j = 0; j < relatedNLEsArray.Length; j++)
                    {
                        EventBasedResultInnerRelationships nle = relatedNLEsArray[j];
                        hostEdge.RemoveNLEventBasedLinkInnerIDPairIfExist(nle);
                        AddNotLoadedSideOfPairToNlrList(hostEdge, subRelationshipEquivalentLink, nle);
                    }
                    relMetadata.IsCached = true;
                }
                PendingEdgesToUpdateWithoutSelection.Add(edgeMetadata);
            }
        }

        private void ApplyAddNotLoadedEventBasedLinkMappingChanges
            (EventBasedKWLink linkToAdd, RelationshipShowMetadata firstRelMetadata
            , RelationshipShowMetadata secondRelMetadata, EdgeMetadata edgeMetadata)
        {
            if (!firstRelMetadata.HostEdges.Contains(edgeMetadata))
                firstRelMetadata.HostEdges.Add(edgeMetadata);
            if (!secondRelMetadata.HostEdges.Contains(edgeMetadata))
                secondRelMetadata.HostEdges.Add(edgeMetadata);

            bool relationshipsAtleastAppearedInOneEventLink = false;

            ApplyAddLoadedEventBasedLinkSubRelMappingChanges(linkToAdd, firstRelMetadata, ref relationshipsAtleastAppearedInOneEventLink);
            ApplyAddLoadedEventBasedLinkSubRelMappingChanges(linkToAdd, secondRelMetadata, ref relationshipsAtleastAppearedInOneEventLink);

            if (relationshipsAtleastAppearedInOneEventLink)
            {
                firstRelMetadata.IsCached = true;
                secondRelMetadata.IsCached = true;
            }

            ApplyAddNotLoadedEventBasedLinkSubRelationshipMappingChanges(linkToAdd.FirstRelationship, firstRelMetadata, edgeMetadata);
            ApplyAddNotLoadedEventBasedLinkSubRelationshipMappingChanges(linkToAdd.SecondRelationship, secondRelMetadata, edgeMetadata);

            RemoveEdgeIfIsEmpty(edgeMetadata);
        }

        private void ApplyAddLoadedEventBasedLinkSubRelMappingChanges(EventBasedKWLink linkToAdd, RelationshipShowMetadata subRelMetadata, ref bool relationshipsAtleastAppearedInOneEventLink)
        {
            EdgeMetadata[] subRelHostEdgesArray = subRelMetadata.HostEdges.ToArray();
            for (int i = 0; i < subRelHostEdgesArray.Length; i++)
            {
                EdgeMetadata hostEdge = subRelHostEdgesArray[i];
                EventBasedResultInnerRelationships hostEdgeRelatedNLE;
                if (hostEdge.TryGetNLEventBasedLinkInnerIDsPairByInnerRelIDs(linkToAdd.FirstRelationship.ID, linkToAdd.SecondRelationship.ID, out hostEdgeRelatedNLE))
                {
                    hostEdge.RemoveNLEventBasedLinkInnerIDPairIfExist(hostEdgeRelatedNLE);
                    hostEdge.EventBasedLinks.Add(linkToAdd);
                    relationshipsAtleastAppearedInOneEventLink = true;
                    PendingEdgesToUpdateWithoutSelection.Add(hostEdge);
                }
            }
        }

        private void ApplyAddNotLoadedRelBasedLinkMappingChanges
            (RelationshipBasedKWLink notLoadedLinkToShow, RelationshipShowMetadata relMetadata
            , EdgeMetadata edgeMetadata)
        {
            if (!relMetadata.HostEdges.Contains(edgeMetadata))
                relMetadata.HostEdges.Add(edgeMetadata);
            EdgeMetadata[] hostEdgesArray = relMetadata.HostEdges.ToArray();
            for (int i = 0; i < hostEdgesArray.Length; i++)
            {
                EdgeMetadata hostEdge = hostEdgesArray[i];

                if (!hostEdge.RelationshipBasedLinks.Contains(notLoadedLinkToShow)
                    && hostEdge.IsEdgeEndsMatchWith(notLoadedLinkToShow))
                {
                    hostEdge.RelationshipBasedLinks.Add(notLoadedLinkToShow);
                }
                if (hostEdge.NLRelationshipBasedLinkIDs.Contains(notLoadedLinkToShow.Relationship.ID))
                {
                    hostEdge.NLRelationshipBasedLinkIDs.Remove(notLoadedLinkToShow.Relationship.ID);
                }
                HashSet<EventBasedResultInnerRelationships> hostEdgeRelatedNLEs;
                if (hostEdge.TryGetNLEventBasedLinkInnerIDPairsThatContainsRel(notLoadedLinkToShow.Relationship.ID, out hostEdgeRelatedNLEs))
                {
                    var relatedNLEsArray = hostEdgeRelatedNLEs.ToArray();
                    for (int j = 0; j < relatedNLEsArray.Length; j++)
                    {
                        EventBasedResultInnerRelationships nle = relatedNLEsArray[j];
                        hostEdge.RemoveNLEventBasedLinkInnerIDPairIfExist(nle);
                        AddNotLoadedSideOfPairToNlrList(hostEdge, notLoadedLinkToShow, nle);
                    }
                }
                if (!hostEdge.RelationshipBasedLinks.Contains(notLoadedLinkToShow))
                {
                    relMetadata.HostEdges.Remove(hostEdge);
                    RemoveEdgeIfIsEmpty(hostEdge);
                }
                UpdateRelationshipMappingAccordingToEdgeChanges(notLoadedLinkToShow.Relationship.ID, hostEdge);
                PendingEdgesToUpdateWithoutSelection.Add(hostEdge);
            }
            relMetadata.IsCached = true;
        }

        private void UpdateRelationshipMappingAccordingToEdgeChanges(long relID, EdgeMetadata hostEdge)
        {
            if (!hostEdge.ContainsRelationship(relID))
            {
                RemoveEdgeFromRelationshipHostEdges(relID, hostEdge);
            }
        }

        private void RemoveEdgeFromRelationshipHostEdges(long relID, EdgeMetadata hostEdge)
        {
            if (RelationshipsMapping.ContainsKey(relID))
            {
                RelationshipShowMetadata relShowMetadata = RelationshipsMapping[relID];
                if (relShowMetadata.HostEdges.Contains(hostEdge))
                {
                    relShowMetadata.HostEdges.Remove(hostEdge);
                    if (relShowMetadata.HostEdges.Count == 0)
                        RelationshipsMapping.Remove(relID);
                    PendingEdgesToUpdateWithoutSelection.Add(hostEdge);
                }
            }
        }

        private void UpdateRelationshipMappingAccordingToEdgeRemove(EdgeMetadata hostEdge)
        {
            foreach (long relID in hostEdge.RelationshipBasedLinks.Select(l => l.Relationship.ID))
            {
                RemoveEdgeFromRelationshipHostEdges(relID, hostEdge);
            }
            foreach (EventBasedKWLink evLink in hostEdge.EventBasedLinks)
            {
                RemoveEdgeFromRelationshipHostEdges(evLink.FirstRelationship.ID, hostEdge);
                RemoveEdgeFromRelationshipHostEdges(evLink.SecondRelationship.ID, hostEdge);
            }
            foreach (long relID in hostEdge.NLRelationshipBasedLinkIDs)
            {
                RemoveEdgeFromRelationshipHostEdges(relID, hostEdge);
            }
            foreach (EventBasedResultInnerRelationships relPair in hostEdge.GetNLEventBasedLinkInnerIdPairs())
            {
                RemoveEdgeFromRelationshipHostEdges(relPair.FirstRelationshipID, hostEdge);
                RemoveEdgeFromRelationshipHostEdges(relPair.SecondRelationshipID, hostEdge);
            }
        }

        private void AddNotLoadedSideOfPairToNlrList
            (EdgeMetadata sourceEdge, RelationshipBasedKWLink loadedLink
            , EventBasedResultInnerRelationships nlePair)
        {
            // مبدا و مقصد، سرهای غیرمشترک خواهد بود
            KWObject newNlrSource;
            KWObject newNlrTarget;
            KWObject probablyNotShownObject;
            if (loadedLink.Source.Equals(sourceEdge.From))
            {
                newNlrSource = probablyNotShownObject = loadedLink.Target;
                newNlrTarget = sourceEdge.To;
            }
            else if (loadedLink.Target.Equals(sourceEdge.From))
            {
                newNlrSource = sourceEdge.To;
                newNlrTarget = probablyNotShownObject = loadedLink.Source;
            }
            else
            {
                newNlrSource = sourceEdge.From;
                newNlrTarget = probablyNotShownObject = loadedLink.Source;
            }

            if (!IsObjectShowing(probablyNotShownObject))
            {
                return;
            }

            if (nlePair.FirstRelationshipID != loadedLink.Relationship.ID)
            {
                NotLoadedRelationshipBasedKWLink nlr = new NotLoadedRelationshipBasedKWLink()
                {
                    IntermediaryRelationshipIDs = new HashSet<long>(),
                    Source = newNlrSource,
                    Target = newNlrTarget
                };
                nlr.IntermediaryRelationshipIDs.Add(nlePair.FirstRelationshipID);
                PendingLinksToShow.Add(nlr);
            }
            else if (nlePair.SecondRelationshipID != loadedLink.Relationship.ID)
            {
                NotLoadedRelationshipBasedKWLink nlr = new NotLoadedRelationshipBasedKWLink()
                {
                    IntermediaryRelationshipIDs = new HashSet<long>(),
                    Source = newNlrSource,
                    Target = newNlrTarget
                };
                nlr.IntermediaryRelationshipIDs.Add(nlePair.SecondRelationshipID);
                PendingLinksToShow.Add(nlr);
            }
        }

        internal bool IsObjectShowing(KWObject objectToCheck)
        {
            return ObjectsMapping.ContainsKey(objectToCheck);
        }

        //private void BindDeletedChangeToLink(KWLink item)
        //{
        //    if (item is RelationshipBasedKWLink)
        //    {
        //        ((item as RelationshipBasedKWLink).Relationship).Deleted += GraphControl_Deleted;
        //    }
        //    else if (item is EventBasedKWLink)
        //    {
        //        ((item as EventBasedKWLink)).FirstRelationship.Deleted += GraphControl_Deleted;
        //        ((item as EventBasedKWLink)).SecondRelationship.Deleted += GraphControl_Deleted;
        //    }
        //    else if (item is PropertyBasedKWLink)
        //    {
        //        return;
        //    }
        //}

        //private void GraphControl_Deleted(object sender, EventArgs e)
        //{

        //}

        /// <summary>
        /// حذف اشیا از گراف
        /// </summary>
        public void RemoveObjects(List<KWObject> objectsToRemoveFromGraph)
        {
            if (objectsToRemoveFromGraph == null)
                throw new ArgumentNullException(nameof(objectsToRemoveFromGraph));
            // مجموعه ورودی خالی نیاز به انجام هیچ کاری ندارد
            if (objectsToRemoveFromGraph.Count == 0)
                return;

            DeselectObjects(objectsToRemoveFromGraph);

            // اگر بین اشیائی که قرار است حذف شوند، شئی باشد که در حالت جمع شده
            // باشد، می‌بایست اشیا جمع شده نیز حذف شوند؛
            // برای این کار می‌بایست ابتدا گروه جمع شده تا آخرین سطح (بدون پویانمایی) باز
            // و نیز اشیا زیرگروه حذف شوند

            List<KWObject> additionalObjectsToRemoveFromGraph = new List<KWObject>();
            foreach (var item in objectsToRemoveFromGraph)
            {
                Vertex relatedVertex = GetRelatedVertex(item);
                if (relatedVertex == null)
                    // در صورتی که گره متناظری برای این شئ وجود نداشته باشد، با این شئ کاری نخواهیم داشت
                    continue;
                // در صورت جمع بودن...
                // و زیرگروه‌ها (در تمام سطوح) به لیست ورودی افزوده می‌شوند
                if (item is GroupMasterKWObject && graphviewerMain.IsGroupInCollapsedViewing(relatedVertex as GroupMasterVertex))
                {
                    // گروه را تا آخرین سطح باز و
                    graphviewerMain.ExpandGroupCascadely(relatedVertex as GroupMasterVertex);
                    // زیرگروه‌ها برای افزودن به لیست حذف، ثبت می‌شوند
                    additionalObjectsToRemoveFromGraph.AddRange(GetGrandSubGroups(item as GroupMasterKWObject));
                }
            }
            objectsToRemoveFromGraph.AddRange(additionalObjectsToRemoveFromGraph);

            // از آنجایی که حذف یک شئ می تواند بر حذف دیگر اشیا تاثیر گذارد، پس از حذف هر
            // شئ، اشیا باقیمانده برای حذف را براساس وجودشان روی گراف در نظر می‌گیریم

            IEnumerable<KWObject> remainingObjectsToRemove = null;
            Vertex lastVertexToRemove = null;
            while ((remainingObjectsToRemove = objectsToRemoveFromGraph
                    .Where(o => GetRelatedVertex(o) != null))
                .Any())
            {
                KWObject item = remainingObjectsToRemove.First();
                // یافتن گره متناظر با شی در حال پیمایش
                Vertex relatedVertex = GetRelatedVertex(item);
                if (relatedVertex == null)
                    // در صورتی که گره متناظری برای این شئ روی گراف وجود نداشته باشد، با این شئ کاری نخواهیم داشت
                    continue;
                if (relatedVertex == lastVertexToRemove)
                    throw new Exception(string.Format(Properties.Resources.Unable_To_Remove_Object_Form_Graph_ID_Display_Name, item.ID, item.GetObjectLabel()));
                else
                    lastVertexToRemove = relatedVertex;

                List<EdgeMetadata> relatedEdges = TotalEdges.Where(m => m.From.Equals(item) || m.To.Equals(item)).ToList();
                RemoveEdges(relatedEdges);

                // حذف گره متناظر با شئ، از گراف
                graphviewerMain.RemoveVertex(relatedVertex);
                // نکته: حذف نگاشت گره از طریق رخداد (حذف) بازگشتی از نمایشگر گراف صورت می‌گیرد
                // و اینجا نیازی به کار اضافه ندارد
                // (نگاشت گره/یال برای مدیریت جمع و باز شدن گره‌ها انجام می‌شود)
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        public void ClearGraph()
        {
            ClearObjectsMapping();
            PendingEdgesToSelect = new HashSet<EdgeMetadata>();
            PendingEdgesToShow = new HashSet<EdgeMetadata>();
            PendingEdgesToUpdateWithoutSelection = new HashSet<EdgeMetadata>();
            PendingLinksToShow = new List<KWLink>();
            PendingVerticesToShow = new List<VertexAddMetadata>();
            RelationshipsMapping = new Dictionary<long, RelationshipShowMetadata>();
            TotalAffectedVertices = new List<Vertex>();
            TotalEdges = new HashSet<EdgeMetadata>();
            graphviewerMain.ClearGraph();
        }

        public void RemoveSelectedLinks()
        {
            List<EdgeMetadata> selectedEdgeMetadatas
                = Viewer.GetSelectedEdges()
                    .Select(e => (EdgeMetadata)e.Tag)
                    .ToList();
            if (selectedEdgeMetadatas.Count == 0)
                return;

            foreach (EdgeMetadata edgeMetadata in selectedEdgeMetadatas)
            {
                // اگر لینکی می‌خواهیم حذف کنیم بین دو شئ جمع شده باشد، گره مبدا/مقصد جمع شده
                // نگهداری می‌شود و نیز گره جمع شده برای حذف لینک، موقتا (بدون پویانمایی) باز
                // می‌شود؛
                // پس از حذف لینک، گره باز شده (بدون پویانمایی) به حالت جمع شده برمی‌گردد تا
                // کاربر غاقلگیر نشود 

                Vertex sourceVertex = GetRelatedVertex(edgeMetadata.From);
                GroupMasterVertex sourceGroupMasterTemproryExpanded = null;
                if (graphviewerMain.IsVertexCurrentlyCollapsedByAGroup(sourceVertex))
                {
                    sourceGroupMasterTemproryExpanded = graphviewerMain.GetVertexMostTopCollapsedGroup(sourceVertex);
                    graphviewerMain.ExpandGroupCascadelyToExpandVertex(sourceGroupMasterTemproryExpanded, sourceVertex);
                }
                Vertex targetVertex = GetRelatedVertex(edgeMetadata.To);
                GroupMasterVertex targetGroupMasterTemproryExpanded = null;
                if (graphviewerMain.IsVertexCurrentlyCollapsedByAGroup(targetVertex))
                {
                    targetGroupMasterTemproryExpanded = graphviewerMain.GetVertexMostTopCollapsedGroup(targetVertex);
                    graphviewerMain.ExpandGroupCascadelyToExpandVertex(targetGroupMasterTemproryExpanded, targetVertex);
                }

                foreach (KWLink relLink in edgeMetadata.GetAllLinks())
                {
                    // اگر رابطه نشاندهنده یک عضویت باشد، رابطه از زیرگروه‌های شئ میزبان گروه حذف می‌شود
                    if (relLink.Target is GroupMasterKWObject && relLink.TypeURI == OntologyProvider.GetOntology().DefaultGroupRelationshipType())
                    {
                        (GetRelatedVertex(relLink.Target) as GroupMasterVertex).RemoveSubGroup(GetRelatedVertex(relLink.Source), graphviewerMain);
                    }
                }
                RemoveEdge(edgeMetadata);

                // درصورت باز شدن موقت گره‌های مبدا یا مقصد، آن را به حالت اولیه خود برمی‌گردانیم
                if (sourceGroupMasterTemproryExpanded != null)
                    graphviewerMain.CollapseGroup(sourceGroupMasterTemproryExpanded, false);
                if (targetGroupMasterTemproryExpanded != null)
                    graphviewerMain.CollapseGroup(targetGroupMasterTemproryExpanded, false);

                // نکته: حذف نگاشت یال از طریق رخداد (حذف) بازگشتی از نمایشگر گراف صورت می‌گیرد
                // و اینجا نیازی به کار اضافه ندارد
                // (نگاشت گره/یال برای مدیریت جمع و باز شدن گره‌ها انجام می‌شود)
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnLinksRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        public void RemoveLinks(List<KWLink> links)
        {
            if (links.Count == 0)
                return;

            var selectedEdgesMetadata = Viewer.GetSelectedEdges().Select(e => (EdgeMetadata)e.Tag).ToList();
            var edgesToRemove = new List<EdgeMetadata>();

            foreach (var link in links)
            {
                if (link is EventBasedKWLink)
                {
                    edgesToRemove.AddRange(from selectedLink in selectedEdgesMetadata
                                           from basedLink in selectedLink.EventBasedLinks
                                           where basedLink.FirstRelationship.ID == ((EventBasedKWLink)link).FirstRelationship.ID &&
                                                 basedLink.SecondRelationship.ID == ((EventBasedKWLink)link).SecondRelationship.ID
                                           select selectedLink);
                }
                else if (link is RelationshipBasedKWLink)
                {
                    edgesToRemove.AddRange(from selectedLink in selectedEdgesMetadata
                                           from basedLink in selectedLink.RelationshipBasedLinks
                                           where basedLink.Relationship.ID == ((RelationshipBasedKWLink)link).Relationship.ID
                                           select selectedLink);
                }
            }

            if (edgesToRemove.Count == 0)
                return;

            foreach (EdgeMetadata edgeMetadata in edgesToRemove)
            {
                // اگر لینکی می‌خواهیم حذف کنیم بین دو شئ جمع شده باشد، گره مبدا/مقصد جمع شده
                // نگهداری می‌شود و نیز گره جمع شده برای حذف لینک، موقتا (بدون پویانمایی) باز
                // می‌شود؛
                // پس از حذف لینک، گره باز شده (بدون پویانمایی) به حالت جمع شده برمی‌گردد تا
                // کاربر غاقلگیر نشود 

                Vertex sourceVertex = GetRelatedVertex(edgeMetadata.From);
                GroupMasterVertex sourceGroupMasterTemproryExpanded = null;
                if (graphviewerMain.IsVertexCurrentlyCollapsedByAGroup(sourceVertex))
                {
                    sourceGroupMasterTemproryExpanded = graphviewerMain.GetVertexMostTopCollapsedGroup(sourceVertex);
                    graphviewerMain.ExpandGroupCascadelyToExpandVertex(sourceGroupMasterTemproryExpanded, sourceVertex);
                }
                Vertex targetVertex = GetRelatedVertex(edgeMetadata.To);
                GroupMasterVertex targetGroupMasterTemproryExpanded = null;
                if (graphviewerMain.IsVertexCurrentlyCollapsedByAGroup(targetVertex))
                {
                    targetGroupMasterTemproryExpanded = graphviewerMain.GetVertexMostTopCollapsedGroup(targetVertex);
                    graphviewerMain.ExpandGroupCascadelyToExpandVertex(targetGroupMasterTemproryExpanded, targetVertex);
                }

                foreach (KWLink relLink in edgeMetadata.GetAllLinks())
                {
                    // اگر رابطه نشاندهنده یک عضویت باشد، رابطه از زیرگروه‌های شئ میزبان گروه حذف می‌شود
                    if (relLink.Target is GroupMasterKWObject && relLink.TypeURI ==
                        OntologyProvider.GetOntology().DefaultGroupRelationshipType())
                    {
                        (GetRelatedVertex(relLink.Target) as GroupMasterVertex)?.RemoveSubGroup(
                            GetRelatedVertex(relLink.Source), graphviewerMain);
                    }
                }

                RemoveEdge(edgeMetadata);

                // درصورت باز شدن موقت گره‌های مبدا یا مقصد، آن را به حالت اولیه خود برمی‌گردانیم
                if (sourceGroupMasterTemproryExpanded != null)
                    graphviewerMain.CollapseGroup(sourceGroupMasterTemproryExpanded, false);
                if (targetGroupMasterTemproryExpanded != null)
                    graphviewerMain.CollapseGroup(targetGroupMasterTemproryExpanded, false);

                // نکته: حذف نگاشت یال از طریق رخداد (حذف) بازگشتی از نمایشگر گراف صورت می‌گیرد
                // و اینجا نیازی به کار اضافه ندارد
                // (نگاشت گره/یال برای مدیریت جمع و باز شدن گره‌ها انجام می‌شود)
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnLinksRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        /// <summary>
        /// گره متناسب با یک شی در حال نمایش در گراف را برمی گرداند
        /// </summary>
        /// <returns>در صورتی که شی روی گراف در حال نمایش نباشد، مقدار «نال» را بر می گرداند</returns>
        internal Vertex GetRelatedVertex(KWObject objectToGetItsVertex)
        {
            if (objectToGetItsVertex == null)
                throw new ArgumentNullException(nameof(objectToGetItsVertex));

            Vertex mappedVertex;
            // تلاش برای برگرداندن گره متناظر شی ورودی
            if (ObjectsMapping.TryGetValue(objectToGetItsVertex, out mappedVertex))
            {
                return mappedVertex;
            }
            else
            {
                // TODO: فنی - اعمال راهکاری بهتر از برگرداندن نال
                return null;
            }
        }

        /// <summary>
        /// بازچینش گره های انتخاب شده گراف
        /// </summary>
        /// <param name="LayoutToApplyVertices">الگوریتمی که چینش گره های گراف براساس آن انجام خواهد شد</param>
        public void RelayoutSelectedVertices(LayoutAlgorithmTypeEnum LayoutToApplyVertices)
        {
            if (!graphviewerMain.GetSelectedVertices().Any())
                return;
            RelayoutVertices(LayoutToApplyVertices, graphviewerMain.GetSelectedVertices());
        }

        internal IEnumerable<Vertex> GetRelatedVertices(IEnumerable<KWObject> objects)
        {
            return objects
                .Select(obj => GetRelatedVertex(obj))
                .Where(v => v != null);
        }

        /// <summary>
        /// بازچینش گره‌های روی گراف
        /// </summary>
        /// <param name="LayoutToApplyVertices">الگوریتمی که چینش گره های گراف براساس آن انجام خواهد شد</param>
        public void RelayoutVertices(LayoutAlgorithmTypeEnum LayoutToApplyVertices, IEnumerable<Vertex> verticesToRelayout, bool applyWithAnimation = true)
        {
            if (verticesToRelayout == null)
                throw new ArgumentNullException(nameof(verticesToRelayout));

            graphviewerMain.RelayoutVertices(verticesToRelayout, LayoutToApplyVertices, null, true, applyWithAnimation);
        }

        public void ShowObjectsAround(KWObject centeralObject, IEnumerable<KWObject> aroundObjects)
        {
            if (centeralObject == null)
                throw new ArgumentNullException(nameof(centeralObject));
            if (aroundObjects == null)
                throw new ArgumentNullException(nameof(aroundObjects));

            Vertex centeralVertex = GetRelatedVertex(centeralObject);
            if (centeralVertex == null)
            {
                throw new ArgumentException("Currently no vertex shown on graph for the centeral object");
            }
            Point centeralPosition = graphviewerMain.GetVertexPosition(centeralVertex);

            List<ObjectShowMetadata> notShownObjects = new List<ObjectShowMetadata>();
            foreach (KWObject obj in aroundObjects)
            {
                Vertex relatedVertex = GetRelatedVertex(obj);
                if (relatedVertex == null)
                {
                    notShownObjects.Add(new ObjectShowMetadata()
                    {
                        ObjectToShow = obj,
                        NonDefaultPositionX = centeralPosition.X,
                        NonDefaultPositionY = centeralPosition.Y
                    });
                }
            }
            ShowObjects(notShownObjects);

            List<Vertex> aroundVertices = new List<Vertex>();
            foreach (KWObject obj in aroundObjects)
            {
                aroundVertices.Add(GetRelatedVertex(obj));
            }
            RelayoutVerticesAroundTheirCurrentPosition(aroundVertices);
        }

        private readonly double MinimumDistanceFromSearchedObject = 400;

        private void RelayoutVerticesAroundTheirCurrentPosition(List<Vertex> verticesToRelayout)
        {
            if (verticesToRelayout == null)
                throw new ArgumentNullException(nameof(verticesToRelayout));

            if (verticesToRelayout.Count == 0)
            {
                return;
            }
            else if (verticesToRelayout.Count == 1)
            {
                var vertexNewPositionDic = new Dictionary<Vertex, Point>(1);
                Point vertexPosition = graphviewerMain.GetVertexPosition(verticesToRelayout[0]);
                vertexNewPositionDic.Add(verticesToRelayout[0], new Point
                    (vertexPosition.X + MinimumDistanceFromSearchedObject / 3
                    , vertexPosition.Y - MinimumDistanceFromSearchedObject));
                graphviewerMain.AnimateVerticesMove(vertexNewPositionDic);
            }
            else
            {
                AlgorithmFactory algFactory = new AlgorithmFactory();
                var parameters = algFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Circle) as CircleLayoutParameters;
                parameters.VerticesMinimumDistanceFromCenter = MinimumDistanceFromSearchedObject;
                graphviewerMain.RelayoutVertices(verticesToRelayout, LayoutAlgorithmTypeEnum.Circle, parameters, true, true);
            }
        }

        /// <summary>
        /// اشیا در حال نمایش روی گراف (همه اشیا) را برمی گرداند
        /// </summary>
        public ICollection<KWObject> GetShowingObjects()
        {
            return ObjectsMapping.Keys;
        }

        public bool IsAnyLinkShown()
        {
            return TotalEdges.Any(m => m.IsShownOnGraph && !m.IsEmpty());
        }

        /// <summary>
        /// لینک‌های در حال نمایش روی گراف (همه لینک‌ها) را برمی گرداند
        /// </summary>
        public IEnumerable<KWLink> GetShowingLinks()
        {
            return TotalEdges
                .Where(m => m.IsShownOnGraph)
                .SelectMany(m => m.GetAllLinks());
        }

        public IEnumerable<EventBasedKWLink> GetShowingEventBasedLinks()
        {
            return TotalEdges
                .Where(m => m.IsShownOnGraph)
                .SelectMany(m => m.EventBasedLinks);
        }

        public IEnumerable<RelationshipBasedKWLink> GetShowingRelationshipBasedLinks()
        {
            return TotalEdges
                .Where(m => m.IsShownOnGraph)
                .SelectMany(m => m.RelationshipBasedLinks);
        }

        /// <summary>
        /// جمع کردن گره های گروه بندی شده
        /// </summary>
        /// <param name="groupMasterObjectToCollapse">شئ گروهی که میخواهیم جمع شود</param>
        internal void CollapseGroup(GroupMasterKWObject groupMasterObjectToCollapse)
        {
            if (groupMasterObjectToCollapse == null)
                throw new ArgumentNullException(nameof(groupMasterObjectToCollapse));

            graphviewerMain.AnimatingCollapseGroupCompleted += (sender, e) =>
            {
                OnAnimatingCollapseGroupCompleted(ObjectsMapping.Single(mapping => mapping.Value == e.CollapseRootVertex).Key as GroupMasterKWObject);
            };
            graphviewerMain.CollapseGroup((GroupMasterVertex)GetRelatedVertex(groupMasterObjectToCollapse));
        }

        /// <summary>
        /// باز شدن (عکس جمع شدن) گره های گروه بندی شده
        /// </summary>
        /// <param name="groupMasterObjectToExpand">شئ گروهی که می خواهیم باز شود</param>
        internal void ExpandGroup(GroupMasterKWObject groupMasterObjectToExpand)
        {
            if (groupMasterObjectToExpand == null)
                throw new ArgumentNullException(nameof(groupMasterObjectToExpand));

            graphviewerMain.AnimatingExpandGroupCompleted += (sender, e) =>
            {
                OnAnimatingExpandGroupCompleted(ObjectsMapping.Single(mapping => mapping.Value == e.ExpandRootVertex).Key as GroupMasterKWObject);
            };
            graphviewerMain.ExpandGroup((GroupMasterVertex)GetRelatedVertex(groupMasterObjectToExpand));
        }

        protected List<KWObject> GetGrandSubGroups(GroupMasterKWObject groupMaster)
        {
            if (groupMaster == null)
                throw new ArgumentNullException(nameof(groupMaster));

            IEnumerable<KWObject> nextLevelSubGroups = groupMaster.GetSubGroupObjects();
            List<KWObject> result = new List<KWObject>();
            // افزودن زیرگروه‌های تودرتوی زیرگروه هایی که خودشان میزبان گروه اند، به لیست نتیجه
            foreach (GroupMasterKWObject subGroup in nextLevelSubGroups.Where(o => o is GroupMasterKWObject))
            {
                result.AddRange(GetGrandSubGroups(subGroup));
            }
            // افزودن همه زیرگروه ها به لیست نتیجه
            result.AddRange(nextLevelSubGroups);

            return result;
        }

        /// <summary>
        /// جمع بودن (کامل) یک گروه را نشان می دهد
        /// </summary>
        /// <returns>
        /// در صورت جمع بودن مقدار صحیح و در غیراینصورت مقدار غلط را برمیگرداند
        /// (در صورتی که گروه در حال جمع شدن باشد، مقدار غلط برگردانده خواهد شد)
        /// </returns>
        public bool IsGroupInCollapsedViewing(GroupMasterKWObject groupMasterObjectToCheck)
        {
            if (groupMasterObjectToCheck == null)
                throw new ArgumentNullException(nameof(groupMasterObjectToCheck));
            return graphviewerMain.IsGroupInCollapsedViewing((GroupMasterVertex)GetRelatedVertex(groupMasterObjectToCheck));
        }
        /// <summary>
        /// باز بودن (کامل) یک گروه را نشان می دهد
        /// </summary>
        /// <returns>
        /// در صورت باز بودن مقدار صحیح و در غیراینصورت مقدار غلط را برمیگرداند
        /// (در صورتی که گروه در حال باز شدن باشد، مقدار غلط برگردانده خواهد شد)
        /// </returns>
        public bool IsGroupInExpandedViewing(GroupMasterKWObject groupMasterObjectToCheck)
        {
            if (groupMasterObjectToCheck == null)
                throw new ArgumentNullException(nameof(groupMasterObjectToCheck));
            return graphviewerMain.IsGroupInExpandedViewing((GroupMasterVertex)GetRelatedVertex(groupMasterObjectToCheck));
        }

        /// <summary>
        /// اشیا مرکزی می‌بایست از قبل روی گراف باشند
        /// </summary>
        /// <param name="linksPerCenteralObjects"></param>
        internal void ShowLinksAround(Dictionary<KWObject, List<KWLink>> linksPerCenteralObjects)
        {
            List<ObjectShowMetadata> objectsShowMetadata = new List<ObjectShowMetadata>();
            List<KWLink> linksToShow = new List<KWLink>(linksPerCenteralObjects.Count);
            // اشیائی که می‌بایست بازچینش شوند؛ شامل اشیا مرکزی و اشیائی که به تازگی به گراف افزوده می‌شوند
            foreach (var centeralObject in linksPerCenteralObjects.Keys)
            {
                Vertex centeralObjectVertex = null;
                if ((centeralObjectVertex = GetRelatedVertex(centeralObject)) == null)
                {
                    throw new ArgumentException(Properties.Resources.Atleast_one_centeral_object_was_not_previously_on_graph, nameof(linksPerCenteralObjects));
                }
                Point centeralObjectVertexPosition = graphviewerMain.GetVertexPosition(centeralObjectVertex);

                // اطمینان از وجود مبدا و مقصد تمام لینک‌های مربوط به این شئ مرکزی
                foreach (var currentLink in linksPerCenteralObjects[centeralObject])
                {
                    if (GetRelatedVertex(currentLink.Source) == null)
                    {
                        objectsShowMetadata.Add(new ObjectShowMetadata()
                        {
                            ObjectToShow = currentLink.Source,
                            NonDefaultPositionX = centeralObjectVertexPosition.X,
                            NonDefaultPositionY = centeralObjectVertexPosition.Y
                        });
                    }
                    if (GetRelatedVertex(currentLink.Target) == null)
                    {
                        objectsShowMetadata.Add(new ObjectShowMetadata()
                        {
                            ObjectToShow = currentLink.Target,
                            NonDefaultPositionX = centeralObjectVertexPosition.X,
                            NonDefaultPositionY = centeralObjectVertexPosition.Y
                        });
                    }
                }
                // نمایش لینک‌های مربوط به این شئ مرکزی روی گراف
                linksToShow.AddRange(linksPerCenteralObjects[centeralObject]);
            }
            ShowObjects(objectsShowMetadata);
            ShowLinks(linksToShow);

            List<Vertex> verticesToRelayout = new List<Vertex>();
            foreach (ObjectShowMetadata showMetadata in objectsShowMetadata)
            {
                Vertex newlyAddedVertex = GetRelatedVertex(showMetadata.ObjectToShow);
                verticesToRelayout.Add(newlyAddedVertex);
            }
            if (verticesToRelayout.Any())
            {
                graphviewerMain.UpdateLayout();
                // اعمال بازچینش خودکار به گره‌های مرکزی و نیز گره‌های تازه اضافه شده به گراف
                RelayoutVerticesAroundTheirCurrentPosition(verticesToRelayout);
            }

            // تنظیم وضعیت انتخاب گره‌ها - انتخاب همه‌ی گره‌ها
            graphviewerMain.DeselectAllEdges();
            graphviewerMain.DeselectAllVertices();
            List<Vertex> vertexToSelect = new List<Vertex>();
            foreach (var linksPerAnObject in linksPerCenteralObjects.Values)
            {
                foreach (var item in linksPerAnObject)
                {
                    vertexToSelect.Add(GetRelatedVertex(item.Source));
                    vertexToSelect.Add(GetRelatedVertex(item.Target));
                }
            }
            graphviewerMain.SelectVertices(vertexToSelect);

            List<Vertex> vertexToDeSelect = new List<Vertex>(linksPerCenteralObjects.Keys.Count);
            foreach (var item in linksPerCenteralObjects.Keys)
            {
                vertexToDeSelect.Add(GetRelatedVertex(item));
            }

            graphviewerMain.DeselectVertices(vertexToDeSelect);
        }

        private void AddObjectMapping(KWObject objectToShow, Vertex itemRelatedVertex)
        {
            AddToObjectsMapping(objectToShow, itemRelatedVertex);
            BindObjectChangesToObject(objectToShow);
        }

        private void AddToObjectsMapping(KWObject key, Vertex value)
        {
            ObjectsMapping.Add(key, value);
            hasChangedObjectsMapping = true;
        }

        List<VertexAddMetadata> PendingVerticesToShow = new List<VertexAddMetadata>();
        List<Vertex> TotalAffectedVertices = new List<Vertex>();

        /// <summary>
        /// نمایش اشیاء روی گراف
        /// </summary>
        /// <remarks>
        /// در پیاده‌سازی این تابع سعی شده ابتدا تغییرات در ساختمان‌داده‌ی متناظر اشیاء
        /// به طور کامل اعمال شود، سپس تغییرات به صورت یکجا در نمایش‌دهنده‌ی گراف اعمال شوند
        /// </remarks>
        public void ShowObjects(List<ObjectShowMetadata> objectsToShow)
        {
            if (objectsToShow == null)
                throw new ArgumentNullException(nameof(objectsToShow));
            if (objectsToShow.Count == 0)
                return;
            if (objectsToShow.Count + ObjectsMapping.Count > MaximumShowingObjects)
                throw new ArgumentOutOfRangeException(nameof(objectsToShow), Properties.Resources.Unable_To_Show_More_Object_On_Graph);

            List<KWObject> objectsToRemoveAfterResolve = GetAllObjectsToRemoveAfterResolve(objectsToShow, ObjectsMapping.Keys);
            var objectsBeforeChange = GetShowingObjects();

            List<ObjectShowMetadata> internalResolvedObjectsToShow = 
                GetAllObjectsToShowAfterResolve(objectsToShow, objectsToShow.Select(o => o.ObjectToShow));
            List<ObjectShowMetadata> objectsToShowAfterResolve =
                GetAllObjectsToShowAfterResolve(internalResolvedObjectsToShow, ObjectsMapping.Keys);

            try
            {
                foreach (ObjectShowMetadata metadata in objectsToShowAfterResolve)
                {
                    PendVertexToShow(metadata);
                    if (metadata.ObjectToShow is GroupMasterKWObject)
                    {
                        if (metadata.ForceShowSubGroups)
                        {
                            AppendSubGroupsVerticesToLists((GroupMasterKWObject)metadata.ObjectToShow);
                            PendLinksToShow(((GroupMasterKWObject)metadata.ObjectToShow).GroupLinks
                                .Where(l => !IsLinkShowing(l)));
                        }
                        else
                            PendLinksToShow(((GroupMasterKWObject)metadata.ObjectToShow).GroupLinks
                                .Where(l => !IsLinkShowing(l) && GetRelatedVertex(l.Source) != null));
                    }

                    if (OntologyProvider.GetOntology().IsEvent(metadata.ObjectToShow.TypeURI))
                    {
                        List<KWLink> eventBasedLinksWithNewlyAddedIntermediaryEvent = new List<KWLink>();
                        foreach (EventBasedKWLink eventBasedLink in GetShowingEventBasedLinks())
                        {
                            if (eventBasedLink.IntermediaryEvent.Equals(metadata.ObjectToShow))
                            {
                                eventBasedLinksWithNewlyAddedIntermediaryEvent.Add(eventBasedLink);
                            }
                        }
                        if (eventBasedLinksWithNewlyAddedIntermediaryEvent.Count > 0)
                        {
                            PendLinksToShow(eventBasedLinksWithNewlyAddedIntermediaryEvent);
                        }
                    }
                }

                RemoveObjects(objectsToRemoveAfterResolve);
            }
            catch (Exception ex)
            {
                PendingVerticesToShow.Clear();
                TotalAffectedVertices.Clear();
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message);
            }

            ShowPendentVertices();
            SelectAffectedVertices();
            SelectPendentEdges();
            OnObjectsAdded();
        }

        private List<ObjectShowMetadata> GetAllObjectsToShowAfterResolve(List<ObjectShowMetadata> objectsToShow, IEnumerable<KWObject> showingObjects)
        {
            List<ObjectShowMetadata> objectsToShowAfterResolve = new List<ObjectShowMetadata>();
            IEnumerable<ObjectShowMetadata> slaveObjects = objectsToShow.Where(o => o.ObjectToShow.IsSlave); //اشیایی که اسلیو هستند
            IEnumerable<ObjectShowMetadata> noSlaveObjects = objectsToShow.Except(slaveObjects); // اشیایی که یا مستر هستند یا کلا ادغام نشده اند (همان غیر اسلیوها)
            objectsToShowAfterResolve.AddRange(noSlaveObjects); // اشیا غیر اسلیو را به لیست اضافه می کند

            if (objectsToShowAfterResolve.Count == objectsToShow.Count)
                return objectsToShowAfterResolve;
            //اگر تعداد اشیا افزوده شده برابر تعداد اشیا ورودی بود یعنی اینکه هیچ کاندید ادغامی وجود ندارد و در واقع همان ورودی برگردانده می شود

            foreach (ObjectShowMetadata slaveObject in slaveObjects)
            {
                KWObject masterShowingObject =
                    showingObjects.FirstOrDefault(k => slaveObject.ObjectToShow.MasterId == k.MasterId && k.IsMaster);
                //از لیست اشیا در حال نمایش درون گراف شئ مستر اسلیو موردنظر را پیدا می کند.

                if (masterShowingObject != null)
                {
                    //در صورتی که مستری برای اسلیو یافته بود شئ مستر را برای نمایش جایگزین می کند و در غیر این صورت اسلیو ورودی را نمایش می دهد.
                    slaveObject.ObjectToShow = masterShowingObject;
                }
                objectsToShowAfterResolve.Add(slaveObject);
            }

            return objectsToShowAfterResolve;
        }

        private List<KWObject> GetAllObjectsToRemoveAfterResolve(List<ObjectShowMetadata> objectsToShow, IEnumerable<KWObject> showingObjects)
        {
            List<KWObject> objectsToRemoveAfterResolve = new List<KWObject>();
            foreach (ObjectShowMetadata objectToShow in objectsToShow)
            {
                IEnumerable<KWObject> slaveShowingObjects =
                    showingObjects.Where(k =>
                    objectToShow.ObjectToShow.MasterId == k.MasterId && k.IsSlave && objectToShow.ObjectToShow.ID != k.ID).ToList();

                objectsToRemoveAfterResolve.AddRange(slaveShowingObjects);
                //از لیست اشیا در حال نمایش درون گراف اشیا اسلیو متناظر را پیدا کرده و به لیست حذفیات می افزاید.
            }

            return objectsToRemoveAfterResolve;
        }

        private bool IsLinkShowing(RelationshipBasedKWLink l)
        {
            return RelationshipsMapping.ContainsKey(l.Relationship.ID);
        }

        private void ShowPendentVertices()
        {
            if (PendingVerticesToShow.Count == 0)
                return;
            try
            {
                graphviewerMain.AddVertices(PendingVerticesToShow);
                ShowPendentEdges();
                graphviewerMain.UpdateLayout();

                IEnumerable<Vertex> addedVertices = PendingVerticesToShow.Select(m => m.VertexToAdd);
                ArrangeAddedVertices(addedVertices);
                graphviewerMain.AnimateVerticesAdd(addedVertices);
                graphviewerMain.ZoomOutToCoverVertices(addedVertices);
            }
            finally
            {
                PendingVerticesToShow.Clear();
            }
        }

        private void SelectAffectedVertices()
        {
            if (TotalAffectedVertices.Count == 0)
                return;
            try
            {
                graphviewerMain.DeselectAllVertices();
                graphviewerMain.SelectVertices(TotalAffectedVertices);
                graphviewerMain.ZoomOutToCoverVertices(TotalAffectedVertices);
            }
            finally
            {
                TotalAffectedVertices.Clear();
            }
        }

        /// <summary>
        /// Append Object's related vertex to lists
        /// </summary>
        private void PendVertexToShow(ObjectShowMetadata metadata)
        {
            if (metadata.ObjectToShow == null)
                return;
            Vertex vertex = null;
            if (TryGetRelatedVertex(metadata.ObjectToShow, out vertex))
            {
                if (graphviewerMain.IsVertexCurrentlyCollapsedByAGroup(vertex))
                    graphviewerMain.ExpandGroupCascadelyToExpandVertex(graphviewerMain.GetVertexMostTopCollapsedGroup(vertex), vertex);
                if (!metadata.NonDefaultPositionX.Equals(double.NaN) && !metadata.NonDefaultPositionY.Equals(double.NaN))
                    graphviewerMain.SetVertexPosition(vertex, metadata.NonDefaultPositionX, metadata.NonDefaultPositionY);
            }
            else
            {
                if (metadata.ObjectToShow is GroupMasterKWObject
                    && (metadata.NonDefaultPositionX == double.NaN || metadata.NonDefaultPositionY == double.NaN))
                {
                    UpdateGroupMasterPositionByItsShownSubGroups(ref metadata);
                }
                vertex = GraphControlVertexManager.VertexFactory(metadata.ObjectToShow, this);
                AddObjectMapping(metadata.ObjectToShow, vertex);
                PendingVerticesToShow.Add(new VertexAddMetadata()
                {
                    VertexToAdd = vertex,
                    PositionX = metadata.NonDefaultPositionX,
                    PositionY = metadata.NonDefaultPositionY
                });
            }
            graphviewerMain.SetVertexVisiblity(vertex, metadata.NewVisiblity);
            TotalAffectedVertices.Add(vertex);
        }

        private void AppendSubGroupsVerticesToLists(GroupMasterKWObject groupMasterObj)
        {
            foreach (KWObject subGroup in groupMasterObj.GetSubGroupObjects())
            {
                Vertex tempV = null;
                if (!TryGetRelatedVertex(subGroup, out tempV))
                {
                    continue;
                }
                ObjectShowMetadata subGroupShowMetadata = new ObjectShowMetadata() { ObjectToShow = subGroup };
                PendVertexToShow(subGroupShowMetadata);
            }
        }

        private bool TryGetRelatedVertex(KWObject obj, out Vertex vertex)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return ObjectsMapping.TryGetValue(obj, out vertex);
        }

        private void UpdateGroupMasterPositionByItsShownSubGroups(ref ObjectShowMetadata metadata)
        {
            if (!(metadata.ObjectToShow is GroupMasterKWObject))
                throw new ArgumentException(nameof(metadata.ObjectToShow));

            bool isSubGroupsShownOnGraph = true;
            foreach (var subgroupObject in ((GroupMasterKWObject)metadata.ObjectToShow).GetSubGroupObjects())
            {
                if (GetRelatedVertex(subgroupObject) == null)
                {
                    isSubGroupsShownOnGraph = false;
                    break;
                }
            }
            // در صورت داشتن زیرگروه و روی گراف بودن همه زیرمجموعه ها،
            if (((GroupMasterKWObject)metadata.ObjectToShow).GetSubGroupObjectsCount > 1 && isSubGroupsShownOnGraph)
            {
                // موقعیت گره براساس زیرگروه های آن تعیین می شوند
                IEnumerable<KWObject> subgroupObjects = ((GroupMasterKWObject)metadata.ObjectToShow).GetSubGroupObjects();
                Vertex firstSubgroupVertex = GetRelatedVertex(subgroupObjects.First());
                double subverticesMinimumX = graphviewerMain.GetVertexPosition(firstSubgroupVertex).X;
                double subverticesMinimumY = graphviewerMain.GetVertexPosition(firstSubgroupVertex).Y;
                double subverticesMaximumX = graphviewerMain.GetVertexPosition(firstSubgroupVertex).X;
                double subverticesMaximumY = graphviewerMain.GetVertexPosition(firstSubgroupVertex).Y;
                foreach (var subgroupObject in subgroupObjects)
                {
                    Point currentSubgroupVertexPosition = graphviewerMain.GetVertexPosition(GetRelatedVertex(subgroupObject));
                    if (subverticesMinimumX > currentSubgroupVertexPosition.X)
                        subverticesMinimumX = currentSubgroupVertexPosition.X;
                    else
                        if (subverticesMaximumX < currentSubgroupVertexPosition.X)
                        subverticesMaximumX = currentSubgroupVertexPosition.X;
                    if (subverticesMinimumY > currentSubgroupVertexPosition.Y)
                        subverticesMinimumY = currentSubgroupVertexPosition.Y;
                    else
                        if (subverticesMaximumY < currentSubgroupVertexPosition.Y)
                        subverticesMaximumY = currentSubgroupVertexPosition.Y;
                }
                metadata.NonDefaultPositionX = (subverticesMaximumX + subverticesMinimumX) / 2;
                metadata.NonDefaultPositionY = (subverticesMaximumY + subverticesMinimumY) / 2;
            }
        }

        private void BindObjectChangesToObject(KWObject objectToShow)
        {
            if (objectToShow.DisplayName != null)
            {
                objectToShow.DisplayName.ValueChanged += ApplyShowingObjectDisplayNameChange;
            }
        }
        private void ApplyDeletingObject(object sender, EventArgs e)
        {
            List<KWObject> objectsToRemove = new List<KWObject>();
            objectsToRemove.Add(sender as KWObject);
            RemoveObjects(objectsToRemove);
        }
        private void ApplyShowingObjectDisplayNameChange(object sender, EventArgs e)
        {
            if (!(sender is KWProperty))
                return;
            var changingObject = (sender as KWProperty).Owner;
            var changingObjectVertex = GetRelatedVertex(changingObject);
            changingObjectVertex.Text = changingObject.GetObjectLabel();
        }

        private void graphviewerMain_VertexDoubleClicked(object sender, VertexDoubleClickEventArgs e)
        {
            KWObject relatedObject = GetRelatedObject(e.DoubleClickedVertexControl.Vertex as Vertex);
            OnObjectDoubleClicked(relatedObject);
        }

        private void graphviewerMain_EdgeRightClick(object sender, EventArgs e)
        {
            Point rightClickPoint = Mouse.GetPosition(this);
            OnGraphContentRightClick(rightClickPoint);
        }

        private void graphviewerMain_VertexRightClick(object sender, EventArgs e)
        {
            Point rightClickPoint = Mouse.GetPosition(this);
            OnGraphContentRightClick(rightClickPoint);
        }
        /// <summary>
        /// آخرین فیلتر اعمال شده روی گراف را برمی گرداند
        /// </summary>
        /// <returns>درصورت پاک شدن فیلتر و یا اینکه قبلا فیلتری اعمال نشده باشد، این متغیر نگهدارنده فیلتر خالی خواهد بود</returns>
        public Query CurrentFilter
        {
            get;
            private set;
        }

        private async void ApplyFilterOnAddedObjects()
        {
            if (CurrentFilter != null && !CurrentFilter.IsEmpty())
                await FreezeObjectsByCriteriaAsync(CurrentFilter);
        }

        /// <summary>
        /// یک فیلتر را به گره‌های در حال نمایش روی گراف اعمال می‌کند
        /// </summary>
        public async Task ApplyFilter(ObjectsFilteringArgs filterToApply)
        {
            await FreezeObjectsByCriteriaAsync(filterToApply.FilterToApply);
            CurrentFilter = filterToApply.FilterToApply;

            await OnObjectsFilterChanged();
        }
        /// <summary>
        /// اشیائی که شرایط معیارهای فیلتر را احراز نکنند، منجمد می‌کند
        /// </summary>
        private async Task FreezeObjectsByCriteriaAsync(Query filterToApply)
        {
            DefreezeAllVertices();

            if (filterToApply.IsEmpty())
            {
                return;
            }

            IEnumerable<KWObject> allShowingObjects = GetShowingObjects();
            IEnumerable<KWObject> unsatisfiedObjects = allShowingObjects.Except(await FilterProvider.ApplyFilterOnAsync(allShowingObjects, filterToApply));
            // منجمد کردن گره‌هایی که شرایط را احراز نمی‌کنند
            graphviewerMain.FreezeVertices(unsatisfiedObjects.Select(o => GetRelatedVertex(o)));
        }
        /// <summary>
        /// همه‌ی اشیا در حال نمایش روی گراف را از حالت انجماد خارج می‌کند
        /// </summary>
        private void DefreezeAllVertices()
        {
            graphviewerMain.DefreezeAllVertices();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void graphviewerMain_ViewerInitializationCompleted(object sender, EventArgs e)
        {
            OnControlInitializationCompleted();
        }
    }
}
