using GPAS.FilterSearch;
using GPAS.Logger;
using GPAS.RightClickMenu;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.Presentation.Controls;
using GPAS.Workspace.Presentation.Controls.CustomSearchAround;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Windows;
using GPAS.Workspace.Presentation.Windows.CustomSearchAround;
using GPAS.Workspace.Presentation.Windows.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GPAS.Workspace.Presentation.Applications.EventsArgs;
using GPAS.Workspace.Presentation.Observers.ObjectsRemoving;
using GPAS.Workspace.Presentation.Observers.Properties;
using System.Collections.Specialized;
using MaterialDesignThemes.Wpf;
using GPAS.Ontology;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// منطق تعامل برای GraphControlApplication.xaml
    /// </summary>
    public partial class GraphApplication : IObjectsFilterableListener, IObjectsShowableListener,
        ILinksShowableListener, IObjectsSelectableListener, IObjectsRemovableListener, IPropertiesChangeableListener
    {
        ResourceDictionary iconsResource;

        #region مدیریت رخدادها
        /// <summary>
        /// کلاس آرگومان(های) فراخوانی رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        public class BrowseRequestedEventArgs
        {
            /// <summary>
            /// سازنده کلاس
            /// </summary>
            /// <param name="objectsRequestedForBrowse"></param>
            public BrowseRequestedEventArgs(IEnumerable<KWObject> objectsRequestedForBrowse)
            {
                if (objectsRequestedForBrowse == null)
                    throw new ArgumentNullException(nameof(objectsRequestedForBrowse));

                ObjectsToBrowse = objectsRequestedForBrowse;

            }


            /// <summary>
            /// اشیائی که درخواست نمایش (براوز) برای آن ها داده شده است
            /// </summary>
            public IEnumerable<KWObject> ObjectsToBrowse
            {
                get;
            }
        }

        /// <summary>
        /// رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        public event EventHandler<BrowseRequestedEventArgs> BrowseRequested;

        /// <summary>
        /// عملکرد مدیریت صدور رخداد رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        /// <param name="objectsRequestedForBrowse"></param>
        protected void OnBrowseRequested(IEnumerable<KWObject> objectsRequestedForBrowse)
        {
            if (objectsRequestedForBrowse == null)
                throw new ArgumentNullException(nameof(objectsRequestedForBrowse));

            BrowseRequested?.Invoke(this, new BrowseRequestedEventArgs(objectsRequestedForBrowse));
        }

        public class ShowOnMapRequestedEventArgs
        {
            public ShowOnMapRequestedEventArgs(IEnumerable<KWObject> objectsRequestedToShowOnMap)
            {
                ObjectsRequestedToShowOnMap = objectsRequestedToShowOnMap;
            }

            public IEnumerable<KWObject> ObjectsRequestedToShowOnMap
            {
                get;
            }
        }

        public event EventHandler<ShowOnMapRequestedEventArgs> ShowOnMapRequested;

        protected void OnShowOnMapRequested(IEnumerable<KWObject> objectsRequestedToShowOnMap)
        {
            if (objectsRequestedToShowOnMap == null)
                throw new ArgumentNullException(nameof(objectsRequestedToShowOnMap));

            ShowOnMapRequested?.Invoke(this, new ShowOnMapRequestedEventArgs(objectsRequestedToShowOnMap));
        }

        public class ObjectsSelectionChangedArgs
        {
            public ObjectsSelectionChangedArgs(IEnumerable<KWObject> currentlySelectedObjects)
            {
                CurrentlySelectedObjects = currentlySelectedObjects;
            }

            public IEnumerable<KWObject> CurrentlySelectedObjects
            {
                get;
            }
        }

        public event EventHandler<ObjectsSelectionChangedArgs> ObjectsSelectionChanged;

        protected void OnObjectsSelectionChanged(IEnumerable<KWObject> currentlySelectedObjects)
        {
            if (currentlySelectedObjects == null)
                throw new ArgumentNullException(nameof(currentlySelectedObjects));

            ObjectsSelectionChanged?.Invoke(this, new ObjectsSelectionChangedArgs(currentlySelectedObjects));
        }

        public event EventHandler ObjectsAdded;
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        protected async Task OnObjectsAdded()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            ObjectsAdded?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ObjectsRemoved;
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        protected async Task OnObjectsRemoved()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            ObjectsRemoved?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<SnapshotRequestEventArgs> SnapshotRequested;

        public void OnSnapshotRequested(SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler<RemoveObjectFromMapRequestEventArgs> RemoveObjectFromMapRequest;

        private void OnRemoveObjectFromMapRequest(List<KWObject> objectsToRemove)
        {
            if (objectsToRemove == null)
            {
                throw new ArgumentNullException(nameof(objectsToRemove));
            }

            RemoveObjectFromMapRequest?.Invoke(this, new RemoveObjectFromMapRequestEventArgs(objectsToRemove));
        }

        public event EventHandler<DocumentCreationRequestSubmitedEventAgrs> DocumentCreationRequestSubmited;

        protected virtual void OnDocumentCreationRequestSubmited(string filePath)
        {
            DocumentCreationRequestSubmited?.Invoke(this, new DocumentCreationRequestSubmitedEventAgrs(filePath));
        }

        #endregion

        // در نوشتن کد این کلاس، سعی شده خود-مستندی کد حفظ شود
        // و عناوین، توضیح کافی از فرایند در حال انجام را به خواننده ارائه دهند

        public GraphApplication()
        {
            try
            {
                // آماده ساز اولیه اجزای پنجره
                InitializeComponent();
                Init();
                objectCreationControl.DocumentsImported += ObjectCreationControl_UnstructuredImportButtonClicked;
                // مقداردهی منوی قالب های چینش گره های روی گراف
                if (btnRelayoutSelectedNodes.ContextMenu != null)
                {
                    foreach (Graph.GraphViewer.LayoutAlgorithms.LayoutAlgorithmTypeEnum item in EnumUtitlities.GetEnumElements<Graph.GraphViewer.LayoutAlgorithms.LayoutAlgorithmTypeEnum>())
                    {
                        MenuItem mniItem = new MenuItem();
                        mniItem.Click += mniRelayoutSelectedNodesItem_Click;
                        mniItem.Header = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(item);
                        mniItem.Tag = item;
                        btnRelayoutSelectedNodes.ContextMenu.Items.Add(mniItem);
                    }
                }

                RefreshToolboxButtonsEnablity();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Main_Window_Initialization_Error, ex.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Init()
        {
            iconsResource = new ResourceDictionary { Source = new Uri("/Resources/Icons.xaml", UriKind.Relative) };
            App.WorkspaceInitializationCompleted += App_WorkspaceInitializationCompleted;
        }

        private void App_WorkspaceInitializationCompleted(object sender, EventArgs e)
        {
            App.MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;
        }
        internal PaletteHelper paletteHelper = new PaletteHelper();
        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            selectContextMenu.Background = new SolidColorBrush(theme.Paper);
            selectContextMenu.Foreground = new SolidColorBrush(theme.Body);
        }

        private async void ObjectCreationControl_UnstructuredImportButtonClicked(object sender, ObjectCreationControl.DocumentsImportedEventArgs e)
        {
            await ShowObjectsAsync(e.GeneratedDocuments);
        }

        private void graphControl_rightClicked(object sender, GraphContentRightClickEventArgs e)
        {
            OpenRightClickMenu(e.ClickPoint);
        }

        private List<ObjectShowMetadata> GetObjectsDefaultConfigedShowMetadata(IEnumerable<KWObject> ObjectsToShow)
        {
            List<ObjectShowMetadata> objectShowMetadatas = new List<ObjectShowMetadata>();
            foreach (KWObject obj in ObjectsToShow)
            {
                objectShowMetadatas.Add(new ObjectShowMetadata()
                {
                    ObjectToShow = obj
                });
            }
            return objectShowMetadatas;
        }

        public void HideCreateNewObjectPopup()
        {
            newObjectPopup.StaysOpen = false;
            newObjectPopup.IsOpen = false;
        }

        private async Task ShowCustomSearchAroundResultsOnGraph(KWCustomSearchAroundResult searchResult)
        {
            if (searchResult.IsResultsCountMoreThanThreshold)
                ShowSearchAroundResultsResultsCountMoreThanThresholdNotification();
            ShowCustomSearchAroundLoadedResultsOnGraph(searchResult);
            await ShowLoadMoreResultsPrompt(searchResult.RalationshipBasedResult, searchResult.EventBasedResult);
        }

        private async Task ShowLoadMoreResultsPrompt(List<RelationshipBasedResultsPerSearchedObjects> ralationshipBasedResult)
        {
            bool isAnyNotLoadedResultRemained
                = ralationshipBasedResult.Any(r => r.NotLoadedResults.Any());
            if (isAnyNotLoadedResultRemained)
            {
                int currentlyLoadedResultsCount = Logic.Search.SearchAround.LoadingDefaultBatchSize;
                int recommandedLoadMoreNResultsCount = 100;
                int recommandedLoad500ResultsCount = 500;
                int maximummLoadableResultsCount = Logic.Search.SearchAround.TotalResultsThreshould;


                SearchAroundLoadMoreResultsWindow searchAroundLoadMoreResultsWindow = new SearchAroundLoadMoreResultsWindow()
                {
                    Owner = Window.GetWindow(this)
                };
                searchAroundLoadMoreResultsWindow.Init(currentlyLoadedResultsCount,
                    recommandedLoadMoreNResultsCount,
                    recommandedLoad500ResultsCount,
                    maximummLoadableResultsCount);
                searchAroundLoadMoreResultsWindow.ShowDialog();

                try
                {
                    WaitingControl.Message = Properties.Resources.Prepare_SearchAround_Results_;
                    WaitingControl.TaskIncrement();
                    await LoadAndShowNotLoadedResults(ralationshipBasedResult, searchAroundLoadMoreResultsWindow.ResultWindow);
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }
            }
        }

        private async Task LoadAndShowNotLoadedResults
            (List<RelationshipBasedResultsPerSearchedObjects> ralationshipBasedResult, int loadLimit = -1)
        {
            int countOfRelationshipBasedsToLoad = ralationshipBasedResult.Count;
            if (loadLimit != -1)
            {
                if (ralationshipBasedResult.Count > loadLimit)
                {
                    countOfRelationshipBasedsToLoad = loadLimit;
                }
            }

            List<RelationshipBasedResultsPerSearchedObjects> tempLoadingRelResults
                = await CloneRelationshipResultsAndMoveFromNotLoadedToLoaded
                    (ralationshipBasedResult, countOfRelationshipBasedsToLoad);

            Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
            AddSearchAroundRelationshipBasedResultsToShowDictionary(tempLoadingRelResults, ref linksPerSearchedObjects);
            graphControl.ShowLinksAround(linksPerSearchedObjects);
        }

        private async Task LoadAndAppendMoreResultsByUserChoice(
            List<EventBasedResultsPerSearchedObjects> eventBasedResult)
        {
            bool isAnyNotLoadedResultRemained = eventBasedResult.Any(r => r.NotLoadedResults.Any());
            if (isAnyNotLoadedResultRemained)
            {
                int currentlyLoadedResultsCount = Logic.Search.SearchAround.LoadingDefaultBatchSize;
                int recommandedLoadMoreNResultsCount = 100;
                int recommandedLoad500ResultsCount = 500;
                int maximummLoadableResultsCount = Logic.Search.SearchAround.TotalResultsThreshould;


                SearchAroundLoadMoreResultsWindow searchAroundLoadMoreResultsWindow = new SearchAroundLoadMoreResultsWindow()
                {
                    Owner = Window.GetWindow(this)
                };
                searchAroundLoadMoreResultsWindow.Init(currentlyLoadedResultsCount,
                    recommandedLoadMoreNResultsCount,
                    recommandedLoad500ResultsCount,
                    maximummLoadableResultsCount);
                searchAroundLoadMoreResultsWindow.ShowDialog();

                try
                {
                    WaitingControl.Message = Properties.Resources.Prepare_SearchAround_Results_;
                    WaitingControl.TaskIncrement();
                    eventBasedResult = await LoadNotLoadedResults(eventBasedResult,
                       searchAroundLoadMoreResultsWindow.ResultWindow - currentlyLoadedResultsCount);
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }
            }
        }

        private async Task<List<EventBasedResultsPerSearchedObjects>> LoadNotLoadedResults
            (List<EventBasedResultsPerSearchedObjects> eventBasedResult, int loadLimit = -1)
        {
            int countOfEventBasedsToLoad = loadLimit;
            //if (loadLimit != -1)
            //{
            //    //if (eventBasedResult.Count > loadLimit)
            //    //{
            //        CountOfEventBasedsToLoad = loadLimit;
            //    //}
            //}

            List<EventBasedResultsPerSearchedObjects> tempLoadingEvResults
                = await CloneEventResultsAndMoveFromNotLoadedToLoaded(eventBasedResult, countOfEventBasedsToLoad);

            return tempLoadingEvResults;
        }

        private async Task ShowLoadMoreResultsPrompt
            (List<RelationshipBasedResultsPerSearchedObjects> ralationshipBasedResult
            , List<EventBasedResultsPerSearchedObjects> eventBasedResult)
        {
            bool isAnyNotLoadedResultRemained
                = ralationshipBasedResult.Any(r => r.NotLoadedResults.Any())
                    || eventBasedResult.Any(r => r.NotLoadedResults.Any());
            if (isAnyNotLoadedResultRemained)
            {
                int currentlyLoadedResultsCount = Logic.Search.SearchAround.LoadingDefaultBatchSize;
                int recommandedLoadMoreNResultsCount = 100;
                int recommandedLoad500ResultsCount = 500;
                int maximummLoadableResultsCount = Logic.Search.SearchAround.TotalResultsThreshould;



                SearchAroundLoadMoreResultsWindow searchAroundLoadMoreResultsWindow = new SearchAroundLoadMoreResultsWindow()
                {
                    Owner = Window.GetWindow(this)
                };
                searchAroundLoadMoreResultsWindow.Init(currentlyLoadedResultsCount,
                    recommandedLoadMoreNResultsCount,
                    recommandedLoad500ResultsCount,
                    maximummLoadableResultsCount);
                searchAroundLoadMoreResultsWindow.ShowDialog();

                try
                {
                    WaitingControl.Message = Properties.Resources.Prepare_SearchAround_Results_;
                    WaitingControl.TaskIncrement();
                    await LoadAndShowNotLoadedResults(ralationshipBasedResult, eventBasedResult, searchAroundLoadMoreResultsWindow.ResultWindow);
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }
            }
        }
        private async Task LoadAndShowNotLoadedResults
            (List<RelationshipBasedResultsPerSearchedObjects> ralationshipBasedResult
            , List<EventBasedResultsPerSearchedObjects> eventBasedResult, int loadLimit = -1)
        {
            int countOfRelationshipBasedsToLoad;
            int countOfEventBasedsToLoad;

            //if (loadLimit != -1)
            //{
            //    if (ralationshipBasedResult.Count + eventBasedResult.Count > loadLimit)
            //    {
            //        if (ralationshipBasedResult.Count <= loadLimit / 2)
            //        {
            //            CountOfEventBasedsToLoad = loadLimit - CountOfRelationshipBasedsToLoad;
            //        }
            //        else if (eventBasedResult.Count <= loadLimit / 2)
            //        {
            //            CountOfRelationshipBasedsToLoad = loadLimit - CountOfEventBasedsToLoad;
            //        }
            //        else
            //        {
            //            CountOfRelationshipBasedsToLoad = loadLimit / 2;
            //            CountOfEventBasedsToLoad = loadLimit - CountOfRelationshipBasedsToLoad;
            //        }
            //    }
            //}

            if (loadLimit == -1)
            {
                countOfRelationshipBasedsToLoad = -1;
                countOfEventBasedsToLoad = -1;
            }
            else
            {
                countOfRelationshipBasedsToLoad = loadLimit;
                countOfEventBasedsToLoad = loadLimit;
            }

            List<RelationshipBasedResultsPerSearchedObjects> tempLoadingRelResults
                = await CloneRelationshipResultsAndMoveFromNotLoadedToLoaded
                    (ralationshipBasedResult, countOfRelationshipBasedsToLoad);

            List<EventBasedResultsPerSearchedObjects> tempLoadingEvResults
                = await CloneEventResultsAndMoveFromNotLoadedToLoaded
                    (eventBasedResult, countOfEventBasedsToLoad);

            Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
            AddSearchAroundRelationshipBasedResultsToShowDictionary(tempLoadingRelResults, ref linksPerSearchedObjects);
            AddSearchAroundEventBasedResultsToShowDictionary(tempLoadingEvResults, ref linksPerSearchedObjects);

            graphControl.ShowLinksAround(linksPerSearchedObjects);
        }

        private async Task<List<RelationshipBasedResultsPerSearchedObjects>> CloneRelationshipResultsAndMoveFromNotLoadedToLoaded
            (List<RelationshipBasedResultsPerSearchedObjects> ralationshipBasedResult, int CountOfRelationshipBasedsToLoad)
        {
            List<RelationshipBasedResultsPerSearchedObjects> result = new List<RelationshipBasedResultsPerSearchedObjects>();

            foreach (RelationshipBasedResultsPerSearchedObjects currentResult in ralationshipBasedResult)
            {
                IEnumerable<long> loadingObjectsIDs
                        = currentResult.NotLoadedResults.Select(nr => nr.TargetObjectID);
                Dictionary<long, KWObject> loadingTargetObjectsPerID
                    = (await ObjectManager.GetObjectsById(loadingObjectsIDs))
                        .ToDictionary(o => o.ID);

                RelationshipBasedResultsPerSearchedObjects tempRelationPerSelectedObj = new RelationshipBasedResultsPerSearchedObjects
                {
                    SearchedObject = currentResult.SearchedObject
                };

                List<RelationshipBasedLoadedTargetResult> newLoaded = new List<RelationshipBasedLoadedTargetResult>();
                List<RelationshipBasedNotLoadedResult> newNotLoaded = new List<RelationshipBasedNotLoadedResult>();

                if (CountOfRelationshipBasedsToLoad == -1)
                {
                    foreach (RelationshipBasedNotLoadedResult currentNotLoadedResult in currentResult.NotLoadedResults)
                    {
                        newLoaded.Add(new RelationshipBasedLoadedTargetResult()
                        {
                            RelationshipIDs = currentNotLoadedResult.RelationshipIDs,
                            TargetObject = loadingTargetObjectsPerID[currentNotLoadedResult.TargetObjectID]
                        });
                    }
                }
                else
                {
                    int newLoadedCount = currentResult.LoadedResults.Count + CountOfRelationshipBasedsToLoad;
                    newLoaded = currentResult.LoadedResults;

                    foreach (RelationshipBasedNotLoadedResult currentNotLoadedResult in currentResult.NotLoadedResults)
                    {
                        if (newLoaded.Count < newLoadedCount)
                        {
                            newLoaded.Add(new RelationshipBasedLoadedTargetResult()
                            {
                                RelationshipIDs = currentNotLoadedResult.RelationshipIDs,
                                TargetObject = loadingTargetObjectsPerID[currentNotLoadedResult.TargetObjectID]
                            });
                        }
                        else
                        {
                            newNotLoaded.Add(currentNotLoadedResult);
                        }
                    }
                }

                tempRelationPerSelectedObj.LoadedResults = newLoaded;
                tempRelationPerSelectedObj.NotLoadedResults = newNotLoaded;
                result.Add(tempRelationPerSelectedObj);
            }

            return result;
        }

        private async Task<List<EventBasedResultsPerSearchedObjects>> CloneEventResultsAndMoveFromNotLoadedToLoaded
            (List<EventBasedResultsPerSearchedObjects> eventBasedResult, int CountOfEventBasedsToLoad)
        {
            List<EventBasedResultsPerSearchedObjects> result = new List<EventBasedResultsPerSearchedObjects>();

            foreach (EventBasedResultsPerSearchedObjects currentResult in eventBasedResult)
            {
                IEnumerable<long> loadingObjectsIDs
                        = currentResult.NotLoadedResults.Select(nr => nr.TargetObjectID);
                Dictionary<long, KWObject> loadingTargetObjectsPerID
                    = (await ObjectManager.GetObjectsById(loadingObjectsIDs))
                        .ToDictionary(o => o.ID);

                EventBasedResultsPerSearchedObjects tempEventPerSelectedObj = new EventBasedResultsPerSearchedObjects
                {
                    SearchedObject = currentResult.SearchedObject
                };

                List<EventBasedLoadedTargetResult> newLoaded = new List<EventBasedLoadedTargetResult>();
                List<EventBasedNotLoadedResult> newNotLoaded = new List<EventBasedNotLoadedResult>();

                if (CountOfEventBasedsToLoad == -1)
                {
                    foreach (EventBasedNotLoadedResult currentNotLoadedResult in currentResult.NotLoadedResults)
                    {
                        newLoaded.Add(new EventBasedLoadedTargetResult()
                        {
                            InnerRelationshipIDs = currentNotLoadedResult.InnerRelationships,
                            TargetObject = loadingTargetObjectsPerID[currentNotLoadedResult.TargetObjectID]
                        });
                    }
                }
                else
                {
                    int newLoadedCount = currentResult.LoadedResults.Count + CountOfEventBasedsToLoad;
                    newLoaded = currentResult.LoadedResults;

                    foreach (EventBasedNotLoadedResult currentNotLoadedResult in currentResult.NotLoadedResults)
                    {
                        if (newLoaded.Count < newLoadedCount)
                        {
                            newLoaded.Add(new EventBasedLoadedTargetResult()
                            {
                                InnerRelationshipIDs = currentNotLoadedResult.InnerRelationships,
                                TargetObject = loadingTargetObjectsPerID[currentNotLoadedResult.TargetObjectID]
                            });
                        }
                        else
                        {
                            newNotLoaded.Add(currentNotLoadedResult);
                        }
                    }
                }

                tempEventPerSelectedObj.LoadedResults = newLoaded;
                tempEventPerSelectedObj.NotLoadedResults = newNotLoaded;
                result.Add(tempEventPerSelectedObj);
            }

            return result;
        }

        private async Task<List<PropertyBasedResultsPerSearchedProperty>> ClonePropertyResultsAndMoveFromNotLoadedToLoaded
            (List<PropertyBasedResultsPerSearchedProperty> propertyBasedResult, int countOfPropertyBasedsToLoad)
        {
            List<long> loadingPropertyIDs
                = new List<long>(propertyBasedResult
                    .Take(countOfPropertyBasedsToLoad)
                    .SelectMany(r => r.NotLoadedResultPropertyIDs));
            Dictionary<long, KWProperty> loadingTargetPropertiesPerID
                = (await PropertyManager.RetriveProeprtiesByIdAsync(loadingPropertyIDs))
                    .ToDictionary(o => o.ID);
            List<PropertyBasedResultsPerSearchedProperty> tempLoadingPropResults
                = new List<PropertyBasedResultsPerSearchedProperty>();
            for (int i = 0; i < countOfPropertyBasedsToLoad; i++)
            {
                PropertyBasedResultsPerSearchedProperty basePropResult = propertyBasedResult[i];
                PropertyBasedResultsPerSearchedProperty newResult = (new PropertyBasedResultsPerSearchedProperty()
                {
                    SearchedProperty = basePropResult.SearchedProperty,
                    LoadedResults = new List<PropertyBasedKWLink>(basePropResult.NotLoadedResultPropertyIDs.Count)
                });
                foreach (long item in basePropResult.NotLoadedResultPropertyIDs)
                {
                    if (loadingTargetPropertiesPerID[item].Owner.Equals(basePropResult.SearchedProperty.Owner))
                    {
                        continue;
                    }
                    newResult.LoadedResults.Add
                        (LinkManager.GetPropertyBasedKWLinkFromLinkInnerProperties
                            (basePropResult.SearchedProperty, loadingTargetPropertiesPerID[item]));
                }
                tempLoadingPropResults.Add(newResult);
            }
            return tempLoadingPropResults;
        }

        private async Task ShowLoadMoreResultsPrompt
            (List<PropertyBasedResultsPerSearchedProperty> propertyBasedResult)
        {
            bool isAnyNotLoadedResultRemained
                = propertyBasedResult.Any(r => r.NotLoadedResultPropertyIDs.Any());
            if (isAnyNotLoadedResultRemained)
            {
                int currentlyLoadedResultsCount = Logic.Search.SearchAround.LoadingDefaultBatchSize;
                int recommandedLoadMoreNResultsCount = 100;
                int recommandedLoad500ResultsCount = 500;
                int maximummLoadableResultsCount = Logic.Search.SearchAround.TotalResultsThreshould;


                SearchAroundLoadMoreResultsWindow searchAroundLoadMoreResultsWindow = new SearchAroundLoadMoreResultsWindow()
                {
                    Owner = Window.GetWindow(this)
                };
                searchAroundLoadMoreResultsWindow.Init(currentlyLoadedResultsCount,
                    recommandedLoadMoreNResultsCount,
                    recommandedLoad500ResultsCount,
                    maximummLoadableResultsCount);
                searchAroundLoadMoreResultsWindow.ShowDialog();
                try
                {
                    WaitingControl.Message = Properties.Resources.Prepare_SearchAround_Results_;
                    WaitingControl.TaskIncrement();
                    await LoadAndShowNotLoadedResults(propertyBasedResult, searchAroundLoadMoreResultsWindow.ResultWindow);
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }
            }
        }

        private async Task LoadAndShowNotLoadedResults
            (List<PropertyBasedResultsPerSearchedProperty> propertyBasedResult, int loadLimit = -1)
        {
            int countOfPropertyBasedsToLoad = propertyBasedResult.Count;
            if (loadLimit != -1)
            {
                if (propertyBasedResult.Count > loadLimit)
                {
                    countOfPropertyBasedsToLoad = loadLimit;
                }
            }

            List<PropertyBasedResultsPerSearchedProperty> tempLoadingPropResults
                = await ClonePropertyResultsAndMoveFromNotLoadedToLoaded
                    (propertyBasedResult, countOfPropertyBasedsToLoad);

            Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
            AddSearchAroundPropertyBasedResultsToShowDictionary(tempLoadingPropResults, ref linksPerSearchedObjects);
            graphControl.ShowLinksAround(linksPerSearchedObjects);
        }

        private void ShowCustomSearchAroundLoadedResultsOnGraph(KWCustomSearchAroundResult searchResult)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Prepare_SearchAround_Results_;
                WaitingControl.TaskIncrement();

                Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
                AddSearchAroundRelationshipBasedResultsToShowDictionary(searchResult.RalationshipBasedResult, ref linksPerSearchedObjects);
                AddSearchAroundEventBasedResultsToShowDictionary(searchResult.EventBasedResult, ref linksPerSearchedObjects);

                graphControl.ShowLinksAround(linksPerSearchedObjects);
            }
            finally
            {
                RefreshToolboxButtonsEnablity();
                WaitingControl.TaskDecrement();
            }
        }

        private void AddSearchAroundEventBasedResultsToShowDictionary
            (List<EventBasedResultsPerSearchedObjects> results
            , ref Dictionary<KWObject, List<KWLink>> linksToShowPerSearchedObjects)
        {
            foreach (EventBasedResultsPerSearchedObjects eventResult in results)
            {
                foreach (EventBasedLoadedTargetResult loadedResult in eventResult.LoadedResults)
                {
                    KWLink newLink = LinkManager.GetNotLoadedLinkFormEventBasedLinks
                        (eventResult.SearchedObject, loadedResult.TargetObject, loadedResult.InnerRelationshipIDs);
                    if (!linksToShowPerSearchedObjects.ContainsKey(eventResult.SearchedObject))
                    {
                        linksToShowPerSearchedObjects.Add(eventResult.SearchedObject, new List<KWLink>());
                    }
                    linksToShowPerSearchedObjects[eventResult.SearchedObject].Add(newLink);
                }
            }
        }

        private void AddSearchAroundRelationshipBasedResultsToShowDictionary
            (List<RelationshipBasedResultsPerSearchedObjects> results
            , ref Dictionary<KWObject, List<KWLink>> linksToShowPerSearchedObjects)
        {
            //Dictionary<KWObject, List<long>> linksToLoad = new Dictionary<KWObject, List<long>>();
            foreach (RelationshipBasedResultsPerSearchedObjects relResult in results)
            {
                foreach (RelationshipBasedLoadedTargetResult loadedResult in relResult.LoadedResults)
                {
                    KWLink newLink = LinkManager.GetNotLoadedLinkFormRelationships
                        (relResult.SearchedObject, loadedResult.TargetObject, loadedResult.RelationshipIDs);
                    if (!linksToShowPerSearchedObjects.ContainsKey(relResult.SearchedObject))
                    {
                        linksToShowPerSearchedObjects.Add(relResult.SearchedObject, new List<KWLink>());
                    }
                    linksToShowPerSearchedObjects[relResult.SearchedObject].Add(newLink);
                }
                //// در حال حاظر به خاطر عدم وجود معادلی برای نمایش لینک‌های مبتنی بر
                //// رابطه، تمام روابط بین دو شی یا بارگذاری می‌شوند یا نمی‌شوند
                //HashSet<long> loadedTargetIDs = new HashSet<long>();
                //foreach (RelationshipBasedLoadedTargetResult loadedRel in relResult.LoadedResults)
                //{
                //    if (loadedRel.Source.Equals(relResult.SearchedObject))
                //        loadedTargetIDs.Add(loadedRel.Target.ID);
                //    else if (loadedRel.Target.Equals(relResult.SearchedObject))
                //        loadedTargetIDs.Add(loadedRel.Source.ID);
                //    else
                //        throw new InvalidOperationException();
                //}

                //foreach (RelationshipBasedNotLoadedResult notLoadedRel in relResult.NotLoadedResults)
                //{
                //    if (loadedTargetIDs.Contains(notLoadedRel.TargetObjectID))
                //    {
                //        if (!linksToLoad.ContainsKey(relResult.SearchedObject))
                //        {
                //            linksToLoad.Add(relResult.SearchedObject, new List<long>());
                //        }
                //        linksToLoad[relResult.SearchedObject].Add(notLoadedRel.RelationshipID);
                //    }
                //}
            }

            //// فراخوانی برای افزودن بهینه‌ی نتایج به میانگیر
            //await LinkManager.RetriveRelationshipBaseLinksAsync(linksToLoad.Values.SelectMany(i => i).ToList());
            //foreach (var rtlItem in linksToLoad)
            //{
            //    linksToShowPerSearchedObjects[rtlItem.Key].AddRange(await LinkManager.RetriveRelationshipBaseLinksAsync(rtlItem.Value));
            //}
        }

        private void AddSearchAroundPropertyBasedResultsToShowDictionary
            (List<PropertyBasedResultsPerSearchedProperty> results
            , ref Dictionary<KWObject, List<KWLink>> linksToShowPerSearchedObjects)
        {
            foreach (PropertyBasedResultsPerSearchedProperty propResult in results)
            {
                foreach (PropertyBasedKWLink loadedResult in propResult.LoadedResults)
                {
                    if (!linksToShowPerSearchedObjects.ContainsKey(propResult.SearchedProperty.Owner))
                    {
                        linksToShowPerSearchedObjects.Add(propResult.SearchedProperty.Owner, new List<KWLink>());
                    }
                    linksToShowPerSearchedObjects[propResult.SearchedProperty.Owner].Add(loadedResult);
                }
            }
        }

        private void ShowSearchAroundResultsResultsCountMoreThanThresholdNotification()
        {
            ShowNotification(Properties.Resources.Search_Around,
                Properties.Resources.Search_Around_Result_More_Than_Threshold_);
        }

        private readonly Notifications.Wpf.NotificationManager NotificationManager = new Notifications.Wpf.NotificationManager();
        private void ShowNotification(string title, string message)
        {
            Notifications.Wpf.NotificationContent content = new Notifications.Wpf.NotificationContent
            {
                Title = title,
                Message = message
            };
            NotificationManager.Show(content, "WindowArea", TimeSpan.FromSeconds(7));
        }

        private static void HandleSearchAroundErrorException(Exception ex)
        {
            KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            ExceptionHandler exReporter = new ExceptionHandler();
            exReporter.WriteErrorLog(ex);
        }

        private void CreateLinksObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Merge);
            rightMenuItem.SubMenuSector = 120;

            MergeProvider mergeProvider = new MergeProvider(graphControl, graphControl.GetSelectedObjects());

            if (mergeProvider.CanMergeObjects)
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    HideRightClickMenuItem();
                    mergeProvider.MergeObjects();
                };

                rightMenuItem.Icon = iconsResource["RightClickObjectsMergeImage"] as BitmapImage;
                rightMenuItem.IsEnabled = true;
            }
            else
            {
                rightMenuItem.Icon = iconsResource["RightClickObjectsMergeImage_Disable"] as BitmapImage;
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
            }

            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private async Task ShowRelatedDocuments(IEnumerable<KWObject> objectsToSearchAround)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_SearchAround_;
                WaitingControl.TaskIncrement();
                // انجام جستجو و آماده‌سازی نتیجه آن
                RelationshipBasedResult searchResult = await Logic.Search.SearchAround.GetRelatedDocuments(objectsToSearchAround);
                // نمایش نتیجه جستجو
                if (searchResult.IsResultsCountMoreThanThreshold)
                    ShowSearchAroundResultsResultsCountMoreThanThresholdNotification();
                Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
                AddSearchAroundRelationshipBasedResultsToShowDictionary(searchResult.Results, ref linksPerSearchedObjects);
                graphControl.ShowLinksAround(linksPerSearchedObjects);

                await ShowLoadMoreResultsPrompt(searchResult.Results);
            }
            finally
            {
                RefreshToolboxButtonsEnablity();
                WaitingControl.TaskDecrement();
            }
        }

        private bool CanSearchAroundRelatedDocuments()
        {
            return graphControl.GetSelectedObjects().Any();
        }

        private GetGraphWindow getGraphWindow;

        #region رخدادگردان های رخدادهای پنجره و اجزای آن
        private void graphControl_ObjectsSelectionChanged(object sender, EventArgs e)
        {
            RefreshToolboxButtonsEnablity();
            OnObjectsSelectionChanged(graphControl.GetSelectedObjects());
        }

        private void GraphControl_ObjectsAdded(object sender, EventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsAdded();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        private void GraphControl_ObjectsRemoved(object sender, EventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        private void graphControl_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void btnCreatObject_Click(object sender, RoutedEventArgs e)
        {
            ShowNewObjectPopup();
        }

        public void ShowNewObjectPopup()
        {
            newObjectPopup.IsOpen = true;
        }

        private void btnCreateLink_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateNewLinkAccordingToGraphSelectedObjects())
                return;
            List<KWObject> graphSelectedObjects = graphControl.GetSelectedObjects().ToList();
            ShowCreateNewLinkWindows(graphSelectedObjects[0], graphSelectedObjects[1]);
        }

        private void btnRemoveObject_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRemoveObjectAccordingToGraphSelectedObjects())
                return;
            RemoveObjectFromGraph(graphControl.GetSelectedObjects());
        }

        private void btnRelayoutSelectedNodes_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRelayoutSelectedNodes())
                return;
            OpenRelayoutSelectedNodesMenu();
        }

        private void mniRelayoutSelectedNodesItem_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRelayoutSelectedNodes())
                return;
            RelayoutSelectedObjects((Graph.GraphViewer.LayoutAlgorithms.LayoutAlgorithmTypeEnum)((MenuItem)e.Source).Tag);
        }

        private void btnCollapseGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCollapseGroup())
                return;
            if (AreSelectedObjectsCountEquals(1))
                CollapseGroupOnGraph((GroupMasterKWObject)graphControl.GetSelectedObjects().First());
        }

        private void btnExpandGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!CanExpandGroup())
                return;
            if (AreSelectedObjectsCountEquals(1))
                ExpandGroupOnGraph((GroupMasterKWObject)graphControl.GetSelectedObjects().First());
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedNodesMenu();
        }

        private void btnSnapshot_Click(object sender, RoutedEventArgs e)
        {
            graphControl.TakeSnapshot();
        }

        private void MItemSelectAllObjects_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.SelectAllObjects(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MItemDeselectAllObjects_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.DeselectAllObjects(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemSelectOrphansNodes_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.SelectOrphansObjects(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemInvertSelectionObjects_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.InvertSelectionObjects(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemExpandNodeSelectiontoNextLinkedLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.ExpandNodeSelectiontoNextLinkedLevel(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MItemSetNodeSelectiontoNextLinkedLevel_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.SetNodeSelectiontoNextLinkedLevel(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemInvertSelectionLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.InvertSelectionLinks(); }
            catch (Exception ex)
            { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void MItemSelectAllLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.SelectAllLinks(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemDeselectAllLinks_Click(object sender, RoutedEventArgs e)
        {
            try
            { graphControl.DeselectAllLinks(); }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void MItemSelectOuterLinksOfSelectedObject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                graphControl.DeselectAllLinks();
                graphControl.SelectOuterLinksOfSelectedObject();
            }
            catch (Exception ex)
            {
                {
                    KWMessageBox.Show(ex.Message,

                      MessageBoxButton.OK,
                      MessageBoxImage.Error);
                }
            }
        }

        private void MItemSelectInnerLinksOfSelectedObject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                graphControl.DeselectAllLinks();
                graphControl.SelectInnerLinksOfSelectedObject();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MItemSelectAllLinksOfSelectedObject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                graphControl.DeselectAllLinks();
                graphControl.SelectAllLinksOfSelectedObject();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnBrowseSelectedObjects_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            if (!CanBrowseSelectedObjects())
                return;
            BrowseSelectedObjects();
        }

        private void btnShowSelectedObjectsOnMap_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            if (!CanShowSelectedObjectsOnMap())
                return;
            ShowSelectedObjectsOnMap();
        }

        private async void btnPublisGraph_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            if (!CanPublishGraph())
                return;
            await PromptUserToPublishGraph();
        }

        private async void btnGetGraph_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            await PromptUserToGetGraph();
        }
        #endregion

        #region عملکردهای اجزا و زیرپنجره های پنجره اصلی

        private void RefreshToolboxButtonsEnablity()
        {
            //btnCreatObject.IsEnabled = true;
            btnCreateLink.IsEnabled = CanCreateNewLinkAccordingToGraphSelectedObjects();
            btnRemoveObject.IsEnabled = CanRemoveObjectAccordingToGraphSelectedObjects();
            btnRelayoutSelectedNodes.IsEnabled = CanRelayoutSelectedNodes();
            //btnMakeGroup.IsEnabled = CanMakeGroup();
            //btnCollapseGroup.IsEnabled = CanCollapseGroup();
            //btnExpandGroup.IsEnabled = CanExpandGroup();
            btnSelect.IsEnabled = CanSelectAccordingToGraph();
            SelectAllObjectsMenuItem.IsEnabled = CanSelectAllObjects();
            DeselectAllObjectsMenuItem.IsEnabled = CanDeselectAllObjects();
            SelectAllLinksMenuItem.IsEnabled = CanSelectAllLinks();
            DeselectAllLinksMenuItem.IsEnabled = CanDeselectAllLinks();
            SelectOuterLinksMenuItem.IsEnabled = CanSelectOuterLinks();
            SelectInerLinksMenuItem.IsEnabled = CanSelectInnerLinks();
            SelectAllLinksOfObjectMenuItem.IsEnabled = CanSelectAllLinksOfObject();
            InvertSelectionObjectsMenuItem.IsEnabled = CanInvertSelectionObjects();
            InvertSelectionLinksMenuItem.IsEnabled = CanInvertLinks();
            btnBrowseSelectedObjects.IsEnabled = CanBrowseSelectedObjects();
            btnShowSelectedObjectsOnMap.IsEnabled = CanShowSelectedObjectsOnMap();
            //btnGetGraph.IsEnabled = true;
            btnShareGraph.IsEnabled = CanPublishGraph();
            btnCreatNewGraph.IsEnabled = CanPublishGraph();
            //btnOntology.IsEnabled = true;
            //SearchAroundSamePropertiesButton.IsEnabled = CanSearchAroundSameProperties();
            //SearchAroundRelatedObjectsByMediatorEventsButton.IsEnabled = CanSearchAroundRelatedObjectsByMediatorEvents();
            //SearchAroundRelatedEntitiesButton.IsEnabled = CanSearchAroundRelatedEntities();
        }

        private bool CanSearchAroundRelatedObjectsByMediatorEvents()
        {
            return graphControl.GetSelectedObjects().Any();
        }

        private bool CanSearchAroundRelatedEntities()
        {
            return graphControl.GetSelectedObjects().Any();
        }

        private bool CanSearchAroundSameProperties()
        {
            return graphControl.GetSelectedObjects().Any();
        }

        private bool CanDoneCustomSearchAround()
        {
            return graphControl.GetSelectedObjects().Any();
        }

        private bool CanCreateNewLinkAccordingToGraphSelectedObjects()
        {
            try
            {
                return AreSelectedObjectsCountEquals(2);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowCreateNewLinkWindows(KWObject sourceObject, KWObject targetObject)
        {
            try
            {
                CreateLinkWindow createLinkWindow = new CreateLinkWindow(sourceObject, targetObject);
                createLinkWindow.NewLinkCreated += CreateLinkWindowOnNewLinkCreated;
                createLinkWindow.ShowDialog();

                //LinkCreationWindow lcwTemp = new LinkCreationWindow();
                //lcwTemp.Init(sourceObject, targetObject);
                //lcwTemp.NewLinkCreated += (sender, e) =>
                //    {
                //        try
                //        {
                //            graphControl.ShowLinks(new[] { e.CreatedLink });
                //            RefreshToolboxButtonsEnablity();
                //        }
                //        catch (Exception ex)
                //        {
                //            KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Show_Link_Between_On_The_Graph, e.CreatedLink.Text, e.CreatedLink.Source.GetObjectLabel(), e.CreatedLink.Source.ID, e.CreatedLink.Target.GetObjectLabel(), e.CreatedLink.Target.ID, ex.Message), Properties.Resources.Show_Link, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //        }

                //    };
                //lcwTemp.ShowDialog();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Show_Create_New_Link_Window, ex.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreateLinkWindowOnNewLinkCreated(object sender, object e)
        {
            try
            {
                graphControl.ShowLinks(new[] { (KWLink)e });
                RefreshToolboxButtonsEnablity();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(
                    string.Format(Properties.Resources.Unable_To_Show_Link_Between_On_The_Graph, ((KWLink)e).Text,
                        ((KWLink)e).Source.GetObjectLabel(), ((KWLink)e).Source.ID,
                        ((KWLink)e).Target.GetObjectLabel(), ((KWLink)e).Target.ID, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool CanRemoveObjectAccordingToGraphSelectedObjects()
        {
            try
            {
                return graphControl.GetSelectedObjects().Any();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanRelayoutSelectedNodes()
        {
            try
            {
                return AreSelectedObjectsCountGreaterThan(1);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool AreSelectedObjectsCountGreaterThan(int minimumCount)
        {
            if (minimumCount == 0)
            {
                return graphControl.GetSelectedObjects().Any();
            }
            else
            {
                // با توجه به احتمال سنگین بودن تابع
                // Count
                // برای یک نوع شمارشی، شمارش تا زمان رسیدن به نتیجه‌ی صحیح ادامه می‌یابد
                int counter = 0;
                foreach (KWObject item in graphControl.GetSelectedObjects())
                {
                    counter++;
                    if (counter > minimumCount)
                        return true;
                }
                return false;
            }
        }

        private bool AreSelectedObjectsCountEquals(int checkingCount)
        {
            if (checkingCount == 0)
            {
                return !graphControl.GetSelectedObjects().Any();
            }
            else
            {
                // با توجه به احتمال سنگین بودن تابع
                // Count
                // برای یک نوع شمارشی، شمارش تا زمان رسیدن به نتیجه‌ی صحیح ادامه می‌یابد
                int counter = 0;
                foreach (KWObject item in graphControl.GetSelectedObjects())
                {
                    counter++;
                    if (counter > checkingCount)
                        return false;
                }
                if (counter == checkingCount)
                    return true;
                else
                    return false;
            }
        }

        private void OpenRelayoutSelectedNodesMenu()
        {
            try
            {
                if (btnRelayoutSelectedNodes.ContextMenu == null)
                    throw new NullReferenceException(Properties.Resources.Relayout_Context_Menu_Not_Presented);
                btnRelayoutSelectedNodes.ContextMenu.IsOpen = true;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private void RelayoutSelectedObjects(Graph.GraphViewer.LayoutAlgorithms.LayoutAlgorithmTypeEnum layoutAlgorithmToApply)
        {
            try
            {
                graphControl.RelayoutSelectedVertices(layoutAlgorithmToApply);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool CanSelectAccordingToGraph()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectOrphansAccordingToGraph()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectAllObjects()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanDeselectAllObjects()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanInvertSelectionObjects()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectAllLinks()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanDeselectAllLinks()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectOuterLinks()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectInnerLinks()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectAllLinksOfObject()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanInvertLinks()
        {
            try
            {
                return graphControl.IsAnyLinkShown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanMakeGroup()
        {
            try
            {
                return AreSelectedObjectsCountGreaterThan(1);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanCollapseGroup()
        {
            try
            {
                return
                  AreSelectedObjectsCountEquals(1)
                  && graphControl.GetSelectedObjects().First() is GroupMasterKWObject
                  && graphControl.IsGroupInExpandedViewing(graphControl.GetSelectedObjects().First() as GroupMasterKWObject);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanExpandGroup()
        {
            try
            {
                return
                  AreSelectedObjectsCountEquals(1)
                  && graphControl.GetSelectedObjects().First() is GroupMasterKWObject
                  && graphControl.IsGroupInCollapsedViewing(graphControl.GetSelectedObjects().First() as GroupMasterKWObject);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private void BrowseSelectedObjects()
        {
            try
            {
                OnBrowseRequested(graphControl.GetSelectedObjects());
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private bool CanBrowseSelectedObjects()
        {
            try
            {
                return graphControl.GetSelectedObjects().Any();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowSelectedObjectsOnMap()
        {
            try
            {
                OnShowOnMapRequested(graphControl.GetSelectedObjects());
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                  MessageBoxButton.OK,
                  MessageBoxImage.Error);
            }
        }

        private bool CanShowSelectedObjectsOnMap()
        {
            try
            {
                return graphControl.GetSelectedObjects().Any();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanSelectLinksBetweenSelectedObjects()
        {
            try
            {
                return AreSelectedObjectsCountGreaterThan(1);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanPublishGraph()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool CanShowPeropertiesTagCloud()
        {
            try
            {
                return graphControl.GetShowingObjects().Count > 0;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        internal bool HasGraphAnythingToPublish()
        {
            try
            {
                return CanPublishGraph();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task ShowPublishedGraphOnGraphAsync(GraphArrangment graphArrangment)
        {
            await graphControl.ShowGraphByArrangmentAsync(graphArrangment);
        }

        #endregion

        #region عملکردهای پنجره اصلی

        private void RemoveObjectFromGraph(IEnumerable<KWObject> objectsToRemoveFromGraph)
        {
            try
            {
                if (KWMessageBox.Show(string.Format(Properties.Resources.Are_You_Sure_Want_To_Remove_Objects_From_Graph, objectsToRemoveFromGraph.Count().ToString()),
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    graphControl.RemoveObjects(objectsToRemoveFromGraph.ToList());
                    RefreshToolboxButtonsEnablity();
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Remove_Objects_From_The_Graph, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void CollapseGroupOnGraph(GroupMasterKWObject groupMasterObjectToCollapse)
        {
            try
            {
                graphControl.AnimatingCollapseGroupCompleted += graphControl_AnimatingCollapseGroupCompleted;
                graphControl.CollapseGroup(groupMasterObjectToCollapse);
                RefreshToolboxButtonsEnablity();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Collapse_Group_Vertex, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        void graphControl_AnimatingCollapseGroupCompleted(object sender, AnimatingCollapseGroupCompletedEventArgs e)
        {
            graphControl.AnimatingCollapseGroupCompleted -= graphControl_AnimatingCollapseGroupCompleted;
            RefreshToolboxButtonsEnablity();
        }

        private void ExpandGroupOnGraph(GroupMasterKWObject groupMasterObjectToExpand)
        {
            try
            {
                graphControl.AnimatingExpandGroupCompleted += graphControl_AnimatingExpandGroupCompleted;
                graphControl.ExpandGroup(groupMasterObjectToExpand);
                RefreshToolboxButtonsEnablity();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Expand_Group_Vertex, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        void graphControl_AnimatingExpandGroupCompleted(object sender, AnimatingExpandGroupCompletedEventArgs e)
        {
            graphControl.AnimatingExpandGroupCompleted -= graphControl_AnimatingExpandGroupCompleted;
            RefreshToolboxButtonsEnablity();
        }

        private void OpenSelectedNodesMenu()
        {
            try
            { btnSelect.ContextMenu.IsOpen = true; }
            catch (Exception ex)
            { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        //private void ResolveSelectedObjects()
        //{
        //    try
        //    {
        //        var grProvider = new GlobalResolutionProvider();
        //        grProvider.ResolveObjects(graphControl.GetSelectedObjects());
        //    }
        //    catch (Exception ex)
        //    { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        //}

        #endregion       

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task ShowLinks(IEnumerable<KWLink> relationshipsToShow)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                ShowObjectsAsync(relationshipsToShow.SelectMany(l => new[] { l.Source, l.Target }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                graphControl.ShowLinks(relationshipsToShow);
            }
            catch (Exception ex)
            { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        internal async Task PromptUserToPublishGraph()
        {
            try
            {
                GraphArrangment arrangment = graphControl.GetGraphArrangment();
                List<long> arrangmentObjectIDs = GraphRepositoryManager.GetArrangmentObjectIDs(arrangment);
                List<long> arrangmentRelationshipIDs = await GraphRepositoryManager.GetArrangmentRelationshipIDsAsync(arrangment);
                if (!GraphRepositoryManager.HasGraphAnyUnpublishedConcepts(arrangmentObjectIDs, arrangmentRelationshipIDs))
                {
                    ShareGraphWindow shareGraphWindow = new ShareGraphWindow(arrangment)
                    {
                        Owner = Window.GetWindow(this)
                    };

                    shareGraphWindow.ShowDialog();
                }
                else
                {
                    throw new Exception("Your graph has unpublished concepts, please publish these concepts before you want to publish your graph.");
                }

            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Share_Graph, ex.Message),
                     MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally

            { }
        }

        private void ClearGraph()
        {
            graphControl.ClearGraph();
        }

        private async Task PromptUserToGetGraph()
        {
            try
            {
                getGraphWindow = new GetGraphWindow()
                {
                    Owner = Window.GetWindow(this)
                };

                bool? windowResult = getGraphWindow.ShowDialog();
                if (windowResult.HasValue && windowResult.Value == true
                    && getGraphWindow.SelectedPublishedGraph != null)
                {
                    WaitingControl.Message = Properties.Resources.Showing_Published_Graph;
                    WaitingControl.TaskIncrement();
                    await ShowPublishedGraphOnGraphAsync(getGraphWindow.SelectedPublishedGraph);
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitingControl.TaskDecrement();
                RefreshToolboxButtonsEnablity();
            }
        }

        private async void SearchAroundRelatedEntitiesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ShowRelatedEntities(graphControl.GetSelectedObjects());
            }
            catch (Exception ex)
            {
                HandleSearchAroundErrorException(ex);
            }
        }

        private async Task ShowRelatedEntities(IEnumerable<KWObject> objectsToSearchAround)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_SearchAround_;
                WaitingControl.TaskIncrement();
                // انجام جستجو و آماده‌سازی نتیجه آن
                RelationshipBasedResult searchResult = await Logic.Search.SearchAround.GetRelatedEntities(objectsToSearchAround);
                // نمایش نتیجه جستجو
                if (searchResult.IsResultsCountMoreThanThreshold)
                    ShowSearchAroundResultsResultsCountMoreThanThresholdNotification();
                Dictionary<KWObject, List<KWLink>> linksPerSearchedObjects = new Dictionary<KWObject, List<KWLink>>();
                AddSearchAroundRelationshipBasedResultsToShowDictionary(searchResult.Results, ref linksPerSearchedObjects);
                graphControl.ShowLinksAround(linksPerSearchedObjects);

                await ShowLoadMoreResultsPrompt(searchResult.Results);
            }
            finally
            {
                RefreshToolboxButtonsEnablity();
                WaitingControl.TaskDecrement();
            }
        }

        private async void SearchAroundSamePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ShowObjectsWithSameProperty(graphControl.GetSelectedObjects());
            }
            catch (Exception ex)
            {
                HandleSearchAroundErrorException(ex);
            }
        }

        private async Task ShowObjectsWithSameProperty(IEnumerable<KWObject> objectsToSearchAround)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_SearchAround_;
                WaitingControl.TaskIncrement();
                // انجام جستجو و آماده‌سازی نتیجه آن
                List<KWProperty> searchProperties = await GetSamePropertySearchablePropertiesForObjects(objectsToSearchAround);
                PropertyBasedResult searchResult = await Logic.Search.SearchAround.GetObjectsWithSameProperty(searchProperties);
                // نمایش نتیجه جستجو
                if (searchResult.IsResultsCountMoreThanThreshold)
                    ShowSearchAroundResultsResultsCountMoreThanThresholdNotification();
                Dictionary<KWObject, List<KWLink>> resultsToShow = new Dictionary<KWObject, List<KWLink>>();
                AddSearchAroundPropertyBasedResultsToShowDictionary(searchResult.Results, ref resultsToShow);
                graphControl.ShowLinksAround(resultsToShow);

                await ShowLoadMoreResultsPrompt(searchResult.Results);
            }
            finally
            {
                RefreshToolboxButtonsEnablity();
                WaitingControl.TaskDecrement();
            }
        }

        private async Task<List<KWProperty>> GetSamePropertySearchablePropertiesForObjects(IEnumerable<KWObject> objectsToSearchAround)
        {
            List<KWProperty> searchBaseProperties = new List<KWProperty>();
            foreach (KWObject searchedObject in objectsToSearchAround.Distinct())
            {
                // دریافت آخرین وضعیت تمام ویژگی‌های شیء
                List<KWProperty> totalPropertiesOfSearchedObject = (await PropertyManager.GetPropertiesOfObjectAsync(searchedObject)).ToList();
                // Prepare search 'Base Properties' (specify needed types and Distinct properties)
                foreach (KWProperty property in totalPropertiesOfSearchedObject)
                {
                    BaseDataTypes dataType = OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI);
                    if (dataType == BaseDataTypes.GeoTime || dataType == BaseDataTypes.GeoPoint)
                        continue;
                    bool isAnyBasePropertyMatches = false;
                    foreach (KWProperty baseProperty in searchBaseProperties)
                    {
                        if (baseProperty.TypeURI.Equals(property.TypeURI)
                            && baseProperty.Value.Equals(property.Value)
                            && baseProperty.Owner.Equals(property.Owner))
                        {
                            isAnyBasePropertyMatches = true;
                            break;
                        }
                    }
                    if (!isAnyBasePropertyMatches)
                        searchBaseProperties.Add(property);
                }
            }
            return searchBaseProperties;
        }

        private async void SearchAroundRelatedObjectsByMediatorEventsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await ShowRelatedObjectsByMediatorEvents(graphControl.GetSelectedObjects());
            }
            catch (Exception ex)
            {
                HandleSearchAroundErrorException(ex);
            }
        }

        private async Task ShowRelatedObjectsByMediatorEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_SearchAround_;
                WaitingControl.TaskIncrement();
                // انجام جستجو و آماده‌سازی نتیجه آن
                EventBasedResult searchResult = await Logic.Search.SearchAround.GetRelatedObjectsByMediatorEvents(objectsToSearchAround);
                // نمایش نتیجه جستجو
                if (searchResult.IsResultsCountMoreThanThreshold)
                    ShowSearchAroundResultsResultsCountMoreThanThresholdNotification();
                List<EventBasedResultsPerSearchedObjects> loadedResults = searchResult.Results;

                await LoadAndAppendMoreResultsByUserChoice(searchResult.Results);

                Dictionary<KWObject, List<KWLink>> resultsToShow = new Dictionary<KWObject, List<KWLink>>();
                AddSearchAroundEventBasedResultsToShowDictionary(loadedResults, ref resultsToShow);
                graphControl.ShowLinksAround(resultsToShow);
            }
            finally
            {
                RefreshToolboxButtonsEnablity();
                WaitingControl.TaskDecrement();
            }
        }

        //private void graphControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    HideRightClickMenuItem();           
        //}

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            graphControl.SelectObjects(objectsToSelect);
        }

        public Query CurrentFilter
        {
            get { return graphControl.CurrentFilter; }
        }

        public async Task ApplyFilter(ObjectsFilteringArgs filterToApply)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Apply_Filter;
                WaitingControl.TaskIncrement();
                await graphControl.ApplyFilter(filterToApply);
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }
        }

        private void graphApplicationRightClickMenu_LostFocus(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
        }

        private void graphControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideRightClickMenuItem();
        }

        public override void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {
            switch (commandShortCutKey)
            {
                case SupportedShortCutKey.Ctrl_A:
                    try
                    { graphControl.SelectAllObjects(); }
                    catch (Exception ex)
                    { KWMessageBox.Show(ex.Message); }
                    break;
                case SupportedShortCutKey.Ctrl_S:
                    if (!CanPublishGraph())
                        return;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    PromptUserToPublishGraph();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    break;
                case SupportedShortCutKey.Ctrl_Shift_I:
                    try
                    { graphControl.InvertSelectionObjects(); }
                    catch (Exception ex)
                    { KWMessageBox.Show(ex.Message); }
                    break;
                case SupportedShortCutKey.Ctrl_N:
                    ShowNewObjectPopup();
                    break;
                case SupportedShortCutKey.Ctrl_B:
                    if (!CanBrowseSelectedObjects())
                        return;
                    BrowseSelectedObjects();
                    break;
                case SupportedShortCutKey.RightClickKey:
                    //var t = Keyboard.FocusedElement;
                    //Thickness rightClickMenuMargin = new Thickness(0);
                    Point p = new Point(0, 0);
                    OpenRightClickMenu(p);
                    //ShowRightClickMenu(rightClickMenuMargin);
                    //graphApplicationRightClickMenu.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void ObjectCreationControl_ObjectCreationRequestSubmited(object sender, ObjectCreationControl.ObjectCreationRequestSubmitedEventAgrs e)
        {
            // ایجاد شی براساس آرگومان های رخداد به وقوع پیوسته
            await CreateNewObject(e.Type, e.DisplayName);
        }

        private void objectCreationControl_DocumentCreationRequestSubmited(object sender, DocumentCreationRequestSubmitedEventAgrs e)
        {
            newObjectPopup.IsOpen = false;
            OnDocumentCreationRequestSubmited(e.FilePath);
        }

        /// <summary>
        /// ایجاد یک شی جدید
        /// </summary>
        private async Task CreateNewObject(Ontology.OntologyNode typeNode, string displayName)
        {
            WaitingControl.Message = Properties.Resources.Creating_New_Object_;
            WaitingControl.TaskIncrement();
            try
            {
                // تلاش برای ایجاد شی جدید از طریق لایه منطق
                KWObject objmgrNewObject = await ObjectManager.CreateNewObject(typeNode.TypeUri, displayName);
                // صدور رخداد مربوط به ایجاد شی جدید
                //OnNewObjectCreated(objmgrNewObject);
                graphControl.ShowObjects(GetObjectsDefaultConfigedShowMetadata(new[] { objmgrNewObject }));
                RefreshToolboxButtonsEnablity();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                // مخفی کردن اعلان عمومی انتظار
                WaitingControl.TaskDecrement();
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // مخفی کردن اعلان عمومی انتظار
            WaitingControl.TaskDecrement();
        }

        private async void btnCreatNewGraph_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            if (!CanPublishGraph())
                return;
            switch (KWMessageBox.Show(Properties.Resources.Do_you_want_to_publish_your_Graph_Click_Yes_to_publish_And_Clear_Graph,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes))
            {
                case MessageBoxResult.Yes:
                    await PromptUserToPublishGraph();
                    break;
                case MessageBoxResult.No:
                    ClearGraph();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
            RefreshToolboxButtonsEnablity();
        }

        private void graphControl_ObjectDoubleClicked(object sender, DoubleClickedVertexEventArgs e)
        {
            try
            {
                List<KWObject> doubleClickedObjects = new List<KWObject>();
                doubleClickedObjects.Add(e.DoubleClickedObject);
                OnBrowseRequested(doubleClickedObjects);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            try
            {
                List<ObjectShowMetadata> objectsToShowMetaData = GetObjectsDefaultConfigedShowMetadata(objectsToShow);
                graphControl.ShowObjects(objectsToShowMetaData);
                graphControl.SelectObjects(objectsToShowMetaData.Select(o => o.ObjectToShow));
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal async Task<GraphApplicationStatus> GetGraphApplicationStatus()
        {
            GraphArrangment arrangment = graphControl.GetGraphArrangment();
            List<long> arrangmentObjectIDs = GraphRepositoryManager.GetArrangmentObjectIDs(arrangment);
            List<long> arrangmentRelationshipIDs = await GraphRepositoryManager.GetArrangmentRelationshipIDsAsync(arrangment);
            string graphArrangmentString = GraphRepositoryManager.GetXmlStringForGraphArrangmentAsync(arrangment, arrangmentObjectIDs, arrangmentRelationshipIDs);
            GraphApplicationStatus graphAppStatus = new GraphApplicationStatus()
            {
                GraphArrangement = graphArrangmentString,
                SelectedObjectIds = graphControl.GetSelectedObjects()?.Select(o => o.ID).ToList()
            };
            return graphAppStatus;
        }

        internal async Task SetGraphApplicationStatus(GraphApplicationStatus graphAppStatus)
        {
            if (!string.IsNullOrEmpty(graphAppStatus.GraphArrangement) &&
                graphAppStatus.SelectedObjectIds != null)
            {
                StreamUtility strmUtil = new StreamUtility();
                GraphArrangment graphArrangment = await GraphRepositoryManager.GetGraphArrangmentFromXmlStream(strmUtil.GenerateStreamFromString(graphAppStatus.GraphArrangement));
                await graphControl.ShowGraphByArrangmentAsync(graphArrangment);
                graphControl.SelectObjects(await ObjectManager.GetObjectsById(graphAppStatus.SelectedObjectIds));
            }
        }

        public byte[] TakeImageOfGraph()
        {
            RenderTargetBitmap renderTargetBitmap = graphControl.TakeImageOfGraph();
            return graphControl.RenderTargetBitmapToByteArrangement(renderTargetBitmap);
        }

        public BitmapImage TakeBitmapImageOfGraph()
        {
            RenderTargetBitmap renderTargetBitmap = graphControl.TakeImageOfGraph();
            return graphControl.ConvertRenderTargetBitmapToBitmapImage(renderTargetBitmap);
        }

        public async Task DeleteSelectedLinksFromCache()
        {
            try
            {
                List<KWLink> selectedLinks = graphControl.GetSelectedLinks();
                if (KWMessageBox.Show(string.Format(Properties.Resources.Are_You_Sure_Want_To_Remove_Links_From_Cache, selectedLinks.Count.ToString()),
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    graphControl.RemoveSelectedLinks();
                    await LinkManager.DeleteLinksList(selectedLinks);
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show
                    (string.Format(Properties.Resources.Unable_To_Remove_Links_From_The_Graph, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void RemoveSelectedLinksFromGraph()
        {
            try
            {
                if (KWMessageBox.Show
                    (string.Format(Properties.Resources.Are_You_Sure_Want_To_Remove_Links_From_Graph, graphControl.GetSelectedLinksCount().ToString())
                    , MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    graphControl.RemoveSelectedLinks();
                    RefreshToolboxButtonsEnablity();
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show
                    (string.Format(Properties.Resources.Unable_To_Remove_Links_From_The_Graph, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        #region کلیک راست دایره ای
        private void OpenRightClickMenu(Point clickPoint)
        {
            if (IsStatusReadyToShowObjectRightClickMenu())
            {
                PrepareShowObjectRightClickMenu();
                ShowPreparedRightClickMenu(clickPoint);
            }
            else if (IsStatusReadyToShowLinkRightClickMenu())
            {
                PrepareShowLinkRightClickMenu();
                ShowPreparedRightClickMenu(clickPoint);
            }
        }

        private void ShowPreparedRightClickMenu(Point clickPoint)
        {
            graphApplicationRightClickMenu.FlowDirection = FlowDirection.LeftToRight;
            Storyboard rightClickStoryBoard = (Storyboard)TryFindResource("rightClickStoryBoard");
            rightClickStoryBoard.Begin();
            double xMargin = NormalizeX(clickPoint.X);
            double yMargin = NormalizeY(clickPoint.Y);
            graphApplicationRightClickMenu.setCurrentLevel();
            graphApplicationRightClickMenu.Margin = new Thickness(xMargin, yMargin, 0, 0);
            graphApplicationRightClickMenu.Visibility = Visibility.Visible;
            graphApplicationRightClickMenu.Focus();
        }

        private void PrepareShowLinkRightClickMenu()
        {
            ClearRightClickMenuContent();
            CreateRemoveLinkRightClickMenuItem();
            CreateEditLinkRightClickMenuItem();
            CreateUngroupLinkRightClickMenuItem();
            CreateUnmergeLinkRightClickMenuItem();
            CreatePublishLinkRightClickMenuItem();
            CreateViewLinkRightClickMenuItem();
        }

        private void ClearRightClickMenuContent()
        {
            graphApplicationRightClickMenu.ResetRightClickMenu();
            graphApplicationRightClickMenu.Items.Clear();
        }

        private void CreateViewLinkRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.View);
            rightMenuItem.SubMenuSector = 90;

            if (CanBrowseSelectedLinks())
            {
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickViewImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickViewImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        internal void HardDeleteSelectedObjects()
        {
            //TODO delete selected Object  throw new NotSupportedException();
        }

        internal void HardDeleteSelectedLinks()
        {
            // TODO   delete selected Links  throw new NotSupportedException();
        }

        private bool CanBrowseSelectedLinks()
        {
            return false;
        }

        private void CreatePublishLinkRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Publish);
            rightMenuItem.SubMenuSector = 90;

            if (CanPublishLink())
            {
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickPublishImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickPublishImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private bool CanPublishLink()
        {
            return false;
        }

        private void CreateUnmergeLinkRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Unmerge);
            rightMenuItem.SubMenuSector = 90;

            if (CanUnmergeLinkAccordingToGraphStatus())
            {
                rightMenuItem.Click += CreateUnmergeLinkRightClickMenuItem_Click;
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickUnMergeImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickUnMergeLinkImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private async void CreateUnmergeLinkRightClickMenuItem_Click(object sender, RoutedEventArgs e)
        {
            HideRightClickMenuItem();
            WaitingControl.Message = Properties.Resources.Preparing_Unmerge_;
            WaitingControl.TaskIncrement();
            try
            {
                await graphControl.UnmergeSelectedLinks();
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }
        }

        private bool CanUnmergeLinkAccordingToGraphStatus()
        {
            return graphControl.CanUnmergeSelectedLinks();
        }

        private void CreateUngroupLinkRightClickMenuItem()
        {
            //به علت غیر فعال کردن موقت قابلیت گروه بندی اشیا در گراف این قسمت از کد کامنت شده است.
            //پس از فعال شدن مجدد خاصیت گروه بندی کد های زیر باید فعال شوند.

            //RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Ungroup);
            //rightMenuItem.SubMenuSector = 90;

            //if (CanUnGroupLinkAccordingToGraphStatus())
            //{
            //    rightMenuItem.IsEnabled = true;
            //    rightMenuItem.Icon = iconsResource["RightClickUnGroupImage"] as BitmapImage;
            //}
            //else
            //{
            //    rightMenuItem.IsEnabled = false;
            //    rightMenuItem.Icon = iconsResource["RightClickUnGroupLinkImage_Disable"] as BitmapImage;
            //    rightMenuItem.Foreground = Brushes.Gray;
            //}
            //graphApplicationRightClickMenu.CriteriaItems.Add(rightMenuItem);
        }

        private bool CanUnGroupLinkAccordingToGraphStatus()
        {
            return false;
        }

        private void CreateEditLinkRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Edit);
            rightMenuItem.SubMenuSector = 90;

            if (CanEditLinkAccordingToGraphStatus())
            {
                rightMenuItem.Click += RightMenuItemOnClick;

                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickRditLinkImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickRditLinkImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private void RightMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            EditLinkWindow editLinkWindow = new EditLinkWindow(graphControl.GetSelectedLinks());
            editLinkWindow.UpdateAllLinks += EditLinkWindowOnUpdateAllLinks;
            HideRightClickMenuItem();
            editLinkWindow.ShowDialog();
        }

        private void EditLinkWindowOnUpdateAllLinks(object sender, UpdateLinksEventArgs e)
        {
            try
            {
                graphControl.RemoveLinks(e.LinksToDelete);
                graphControl.ShowLinks(e.NewLinks);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show
                (string.Format(Properties.Resources.Unable_To_Remove_Links_From_The_Graph, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool CanEditLinkAccordingToGraphStatus()
        {
            return true;
        }

        private void CreateRemoveLinkRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Remove);
            rightMenuItem.SubMenuSector = 90;

            AssignRemoveItemsToLinkRightClickMenu(rightMenuItem);

            if (CanRemoveLinkAccordingToGraphStatus())
            {
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickRemoveImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickRemoveImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private void AssignRemoveItemsToLinkRightClickMenu(RightClickMenuItem menuItem)
        {
            menuItem.Items.Clear();

            string MenuItemHeader = Properties.Resources.Remove;
            string MenuItemIconFile;
            RightClickMenuItem rightMenuItem = new RightClickMenuItem();

            rightMenuItem.Click += (sender, e) =>
            {
                RemoveSelectedLinksFromGraph();
                HideRightClickMenuItem();
            };
            if (CanRemoveLinkFromGraph())
            {
                rightMenuItem.IsEnabled = true;
                MenuItemIconFile = "RightClickRemoveFromGraphImage";
                MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
                MenuItemIconFile = "RightClickRemoveFromGraphImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem(); ;
            MenuItemHeader = Properties.Resources.Delete;

            if (LinkManager.CanDeleteLinksList(graphControl.GetSelectedLinks()))
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    await DeleteSelectedLinksFromCache();
                    HideRightClickMenuItem();
                };
                rightMenuItem.IsEnabled = true;
                MenuItemIconFile = "RightClickRemoveImage";
                MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                MenuItemIconFile = "RightClickRemoveImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
                rightMenuItem.Foreground = Brushes.Gray;
            }
            menuItem.Items.Add(rightMenuItem);

            //rightMenuItem = new RightClickMenuItem(); ;
            //MenuItemHeader = Properties.Resources.Hard_Delete;

            //if (CanHardDeleteLink())
            //{
            //    rightMenuItem.IsEnabled = true;
            //    MenuItemIconFile = "RightClickHardDeleteImage";
            //    MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
            //}
            //else
            //{
            //    rightMenuItem.IsEnabled = false;
            //    MenuItemIconFile = "RightClickRemoveImage_Disable";
            //    MakeRightClickMenuItem(rightMenuItem, MenuItemHeader, MenuItemIconFile);
            //    rightMenuItem.Foreground = Brushes.Gray;
            //}
            //menuItem.CriteriaItems.Add(rightMenuItem);
        }

        private bool CanHardDeleteLink()
        {
            return false;
        }

        public bool CanRemoveLinkFromGraph()
        {
            return true;
        }

        private bool CanRemoveLinkAccordingToGraphStatus()
        {
            return true;
        }

        private void AssignRemoveItemsToObjectRightClickMenu(RightClickMenuItem menuItem)
        {
            menuItem.Items.Clear();

            string menuItemHeader = Properties.Resources.Remove;
            string menuItemIconFile = null;
            RightClickMenuItem rightMenuItem = new RightClickMenuItem();

            if (CanRemoveLinkFromGraph())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    RemoveSelectedObjectsFromGraph();
                    HideRightClickMenuItem();
                };

                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickRemoveFromGraphImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
                menuItemIconFile = "RightClickRemoveFromGraphImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }

            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem(); ;
            menuItemHeader = Properties.Resources.Delete;

            if (ObjectManager.CanDeleteObjectsList(graphControl.GetSelectedObjects().ToList()))
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    await DeleteSelectedObjectsFromCache();
                    HideRightClickMenuItem();
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickRemoveImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.Foreground = Brushes.Gray;
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickRemoveImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            //rightMenuItem = new RightClickMenuItem(); ;
            //menuItemHeader = Properties.Resources.Hard_Delete;

            //if (CanHardDeleteLink())
            //{
            //    rightMenuItem.IsEnabled = true;
            //    menuItemIconFile = "RightClickHardDeleteImage";
            //    MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            //}
            //else
            //{
            //    rightMenuItem.IsEnabled = false;
            //    menuItemIconFile = "RightClickRemoveImage_Disable";
            //    MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            //    rightMenuItem.Foreground = Brushes.Gray;
            //}
            //menuItem.CriteriaItems.Add(rightMenuItem);
        }

        public void RemoveSelectedObjectsAndLinksFromGraph()
        {
            List<KWObject> objects = graphControl.GetSelectedObjects().ToList();
            if (objects.Count > 0)
            {
                if (graphControl.IsAnyLinkSelected())
                {
                    try
                    {
                        if (KWMessageBox.Show(string.Format(Properties.Resources.Are_You_Sure_Want_To_Remove_Objects_And_Links_From_Graph, objects.Count.ToString(), graphControl.GetSelectedLinksCount().ToString()),
                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            graphControl.RemoveSelectedLinks();
                            graphControl.RemoveObjects(objects);
                            RefreshToolboxButtonsEnablity();
                        }
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Remove_Objects_From_The_Graph, ex.Message),
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    }
                }
                else
                {
                    RemoveObjectFromGraph(objects);
                }
            }
            else
            {
                if (graphControl.IsAnyLinkSelected())
                {
                    RemoveSelectedLinksFromGraph();
                }
            }
        }

        public void RemoveSelectedObjectsFromGraph()
        {
            RemoveObjectFromGraph(graphControl.GetSelectedObjects());
        }

        public async Task DeleteSelectedObjectsFromCache()
        {
            try
            {
                if (KWMessageBox.Show(Properties.Resources.Are_You_Sure_Want_To_Remove_Objects_From_Cache
                    , MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    List<KWObject> objectLit = graphControl.GetSelectedObjects().ToList();
                    await ObjectManager.DeleteObjectsList(graphControl.GetSelectedObjects().ToList());
                    OnRemoveObjectFromMapRequest(objectLit);
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show
                    (string.Format(Properties.Resources.Unable_To_Remove_Objects_From_The_Cache, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool IsStatusReadyToShowLinkRightClickMenu()
        {
            try
            {
                return graphControl.IsAnyLinkSelected();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                   MessageBoxButton.OK,
                   MessageBoxImage.Exclamation);
                return false;
            }
        }

        private bool IsStatusReadyToShowObjectRightClickMenu()
        {
            try
            {
                return graphControl.GetSelectedObjects().Any();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,

                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return false;
            }
        }

        private void PrepareShowObjectRightClickMenu()
        {
            ClearRightClickMenuContent();
            CreateRemoveObjectRightClickMenuItem();
            CreateSearchAroundObjectRightClickMenuItem();
            CreateGroupObjectRightClickMenuItem();
            CreateSelectObjectRightClickMenuItem();
            CreateTagCloudRightClickMenuItem();
            CreatePublishObjectRightClickMenuItem();
            CreateLinksObjectRightClickMenuItem();
            CreateViewObjectRightClickMenuItem();
        }

        private void MakeRightClickMenuItem(RightClickMenuItem rightMenuItem, string menuItemHeader, string menuItemIconFile)
        {
            // TODO: Check the resource
            rightMenuItem.Header = menuItemHeader;
            rightMenuItem.FontSize = 10;
            rightMenuItem.BorderThickness = new Thickness(1);
            rightMenuItem.BorderBrush = this.BorderBrush;
            rightMenuItem.SubMenuSector = 90;
            rightMenuItem.Icon = iconsResource[menuItemIconFile] as BitmapImage;
        }

        private RightClickMenuItem GenerateRightClickMenuItem(string header)
        {
            RightClickMenuItem rightMenuItem = new RightClickMenuItem
            {
                Header = header,
                BorderThickness = new Thickness(1),
                BorderBrush = this.BorderBrush
            };

            return rightMenuItem;
        }

        private void CreateRemoveObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Remove);
            rightMenuItem.SubMenuSector = 90;
            AssignRemoveItemsToObjectRightClickMenu(rightMenuItem);

            if (CanRemoveObjectAccordingToGraphSelectedObjects())
            {
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickRemoveImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickRemoveImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private double NormalizeX(double xPosition)
        {
            double graphControlGridWidth = graphControlGrid.ActualWidth;
            double result = 0;
            if (xPosition > 250 && xPosition < (graphControlGridWidth - 250))
            {
                result = xPosition - 250;
            }
            else if (xPosition < 250)
            {
                result = 0;
            }
            else if (xPosition > (graphControlGridWidth - 250))
            {
                result = (graphControlGridWidth - 250 - 250);
            }
            return result;
        }

        private double NormalizeY(double yPosition)
        {
            double graphControlGridHeight = graphControlGrid.ActualHeight;
            double result = 0;
            if (yPosition > 250 && yPosition < (graphControlGridHeight - 250))
            {
                result = yPosition - 250;
            }
            else if (yPosition < 250)
            {
                result = 0;
            }
            else if (yPosition > (graphControlGridHeight - 250))
            {
                result = (graphControlGridHeight - 250 - 250);
            }
            return result;
        }

        private void CreateViewObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.View);
            rightMenuItem.SubMenuSector = 90;

            if (CanBrowseSelectedObjects())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    if (!CanBrowseSelectedObjects())
                        return;
                    BrowseSelectedObjects();
                    HideRightClickMenuItem();
                };

                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickViewImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickViewImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        public event EventHandler<EventArgs> PublishedSuccess;

        private void OnPublishedSuccess()
        {
            PublishedSuccess?.Invoke(this, new EventArgs());
        }

        private void CreatePublishObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Publish_Graph);
            rightMenuItem.SubMenuSector = 90;
            if (CanPublishGraph())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    UnpublishedConcepts unpublishConcepts = await UnpublishedChangesManager.GetSpecifiedUnpublishedChangesAsync(
                        graphControl.GetSelectedObjects().ToList(),
                        graphControl.GetSelectedLinks());
                    if (unpublishConcepts.unpublishedObjectChanges.Any() ||
                        unpublishConcepts.unpublishedPropertyChanges.Any() ||
                        unpublishConcepts.unpublishedMediaChanges.Any() ||
                        unpublishConcepts.unpublishedRelationshipChanges.Any()
                        )
                    {
                        PublishWindow publishWindow = new PublishWindow();
                        WaitingControl.Message = Properties.Resources.Publish_inprogress;
                        WaitingControl.TaskIncrement();
                        Tuple<List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>> unpublishedObjects =
                            await PendingChangesPublishManager.GetUnpublishedObjects(unpublishConcepts);
                        WaitingControl.TaskDecrement();
                        publishWindow.ShowUnpublishedObjects(unpublishedObjects);
                        publishWindow.ShowDialog();

                        if (publishWindow.Success)
                        {
                            OnPublishedSuccess();
                        }
                    }
                    else
                    {
                        KWMessageBox.Show(Properties.Resources.Synchronization_Is_Not_Required,
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    }
                    HideRightClickMenuItem();
                    WaitingControl.TaskDecrement();

                };
                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickPublishImage"] as BitmapImage;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickPublishImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }

            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private async Task<bool> CanShowNLPTools()
        {
            bool nlpServiceVisibilityStatus = await Logic.System.GetNLPServiceVisibilityStatus();
            return nlpServiceVisibilityStatus;
        }

        private void CreateTagCloudRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = new RightClickMenuItem { Header = Properties.Resources.Tag_Cloud };
            ResourceDictionary iconsResource = new ResourceDictionary
            {
                Source = new Uri("/Resources/Icons.xaml", UriKind.Relative)
            };

            rightMenuItem.BorderThickness = new Thickness(1);
            rightMenuItem.BorderBrush = this.BorderBrush;
            rightMenuItem.SubMenuSector = 90;

            if (CanShowPeropertiesTagCloud())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    try
                    {
                        WaitingControl.Message = Properties.Resources.Checking_Conditions_To_Open_Right_Click_Menu;
                        WaitingControl.TaskIncrement();
                        bool showNLPTools = await CanShowNLPTools();
                        WaitingControl.TaskDecrement();
                        if (showNLPTools)
                        {
                            TagCloudWindow tagCloudWindow = new TagCloudWindow()
                            {
                                Owner = Window.GetWindow(this)
                            };
                            tagCloudWindow.Init(graphControl.GetSelectedObjects().ToList());
                            tagCloudWindow.ShowOnGraphButtonClicked += TagCloudWindow_ShowOnGraphButtonClicked;
                            tagCloudWindow.ShowDialog();
                        }
                        else
                        {
                            KWMessageBox.Show(Properties.Resources.Service_is_not_available,

                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message,
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    finally
                    {
                        WaitingControl.TaskDecrement();
                        HideRightClickMenuItem();
                    }
                };

                rightMenuItem.IsEnabled = true;
                rightMenuItem.Icon = iconsResource["RightClickTagCloudImage"] as BitmapImage;

            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickTagCloudImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }

            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private void TagCloudWindow_ShowOnGraphButtonClicked(object sender, TagCloudWindow.ShowOnGraphButtonClickEventArgs e)
        {
            List<ObjectShowMetadata> objectShowMetadatas = GetObjectsDefaultConfigedShowMetadata(e.ObjectsToShowOnGraph);
            graphControl.ShowObjects(objectShowMetadatas);
        }

        private void CreateSelectObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Select);
            rightMenuItem.SubMenuSector = 120;
            AssignSelectItems(rightMenuItem);
            if (CanSelectAccordingToGraph())
            {
                rightMenuItem.Icon = iconsResource["RightClickSelectImage"] as BitmapImage;
                rightMenuItem.IsEnabled = true;
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Icon = iconsResource["RightClickSelectImage_Disable"] as BitmapImage;
                rightMenuItem.Foreground = Brushes.Gray;
            }
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        internal async Task<bool> HasAnyChangeInConceptsAsync()
        {
            return await UnpublishedChangesManager.IsAnyUnpublishedChangesAsync();
        }

        private void AssignSelectItems(RightClickMenuItem menuItem)
        {
            menuItem.Items.Clear();
            RightClickMenuItem rightMenuItem = new RightClickMenuItem();
            string menuItemHeader = Properties.Resources.Invert_Nodes;
            string menuItemIconFile;

            if (CanInvertSelectionObjects())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    try
                    {
                        graphControl.InvertSelectionObjects();
                        HideRightClickMenuItem();
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message,
                          MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSelectInvertNodesImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickSelectInvertNodesImage_Disable";
                rightMenuItem.Foreground = Brushes.Gray;
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Outgoing_Nodes;

            if (CanSelectOuterLinks())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    try
                    {
                        graphControl.SelectOuterObjectsOfSelectedObject();
                        HideRightClickMenuItem();
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message,

                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSelectOutgoingNodesImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickSelectOutgoingNodesImage_Disable";
                rightMenuItem.Foreground = Brushes.Gray;
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Incoming_Nodes;

            if (CanSelectInnerLinks())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    try
                    {
                        graphControl.SelectInnerObjectsOfSelectedObject();
                        HideRightClickMenuItem();
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message,
                          MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSelectIncomingNodesImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
                menuItemIconFile = "RightClickSelectIncomingNodesImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Links_Between;

            if (CanSelectLinksBetweenSelectedObjects())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    try
                    {
                        graphControl.SelectLinksBetweenSelectedObject();
                        HideRightClickMenuItem();
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message,

                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation);
                    }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSelectLinksBetweenImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
                menuItemIconFile = "RightClickSelectLinksBetweenImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);
        }

        public void HideRightClickMenuItem()
        {
            graphApplicationRightClickMenu.Visibility = Visibility.Hidden;
        }

        private void CreateGroupObjectRightClickMenuItem()
        {
            //به علت غیر فعال کردن موقت قابلیت گروه بندی اشیا در گراف این قسمت از کد کامنت شده است.
            //پس از فعال شدن مجدد خاصیت گروه بندی کد های زیر باید فعال شوند.

            //RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Grouping);
            //rightMenuItem.SubMenuSector = 90;
            //rightMenuItem.Icon = iconsResource["RightClickMakeGroupImage"] as BitmapImage;
            //AssignGroupItems(rightMenuItem);
            //graphApplicationRightClickMenu.CriteriaItems.Add(rightMenuItem);
        }

        private void CreateSearchAroundObjectRightClickMenuItem()
        {
            RightClickMenuItem rightMenuItem = GenerateRightClickMenuItem(Properties.Resources.Search_Around);
            rightMenuItem.SubMenuSector = 120;
            rightMenuItem.Icon = iconsResource["RightClickSearchAroundImage"] as BitmapImage;
            AssignSearchAroundItems(rightMenuItem);
            graphApplicationRightClickMenu.Items.Add(rightMenuItem);
        }

        private void AssignSearchAroundItems(RightClickMenuItem menuItem)
        {
            menuItem.Items.Clear();

            string menuItemHeader = Properties.Resources.Events;
            string menuItemIconFile;
            RightClickMenuItem rightMenuItem = new RightClickMenuItem();

            if (CanSearchAroundRelatedObjectsByMediatorEvents())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    HideRightClickMenuItem();
                    try
                    { await ShowRelatedObjectsByMediatorEvents(graphControl.GetSelectedObjects()); }
                    catch (Exception ex)
                    { HandleSearchAroundErrorException(ex); }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSearchAroundEventsImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                rightMenuItem.Foreground = Brushes.Gray;
                menuItemIconFile = "RightClickSearchAroundEventsImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Properties;

            if (CanSearchAroundSameProperties())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    HideRightClickMenuItem();
                    try
                    { await ShowObjectsWithSameProperty(graphControl.GetSelectedObjects()); }
                    catch (Exception ex)
                    { HandleSearchAroundErrorException(ex); }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSearchAroundPropertiesImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickSearchAroundPropertiesImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
                rightMenuItem.Foreground = Brushes.Gray;
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Linked_Entities;

            if (CanSearchAroundRelatedEntities())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    HideRightClickMenuItem();
                    try
                    { await ShowRelatedEntities(graphControl.GetSelectedObjects()); }
                    catch (Exception ex)
                    { HandleSearchAroundErrorException(ex); }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSearchAroundLinkedEntitiesImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickSearchAroundLinkedEntitiesImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
                rightMenuItem.Foreground = Brushes.Gray;
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.Linked_Document;

            if (CanSearchAroundRelatedDocuments())
            {
                rightMenuItem.Click += async (sender, e) =>
                {
                    HideRightClickMenuItem();
                    try
                    { await ShowRelatedDocuments(graphControl.GetSelectedObjects()); }
                    catch (Exception ex)
                    { HandleSearchAroundErrorException(ex); }
                };
                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickSearchAroundLinkedDocumentImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickSearchAroundLinkedDocumentImage_Disable";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
                rightMenuItem.Foreground = Brushes.Gray;
            }
            menuItem.Items.Add(rightMenuItem);

            rightMenuItem = new RightClickMenuItem();
            menuItemHeader = Properties.Resources.More;

            if (CanDoneCustomSearchAround())
            {
                rightMenuItem.Click += (sender, e) =>
                {
                    HideRightClickMenuItem();

                    CustomSearchAroundWindow customSearchAroundWindow = new CustomSearchAroundWindow(graphControl.GetSelectedObjects());
                    //customSearchAroundWindow.Init(graphControl.GetSelectedObjects().ToArray());
                    customSearchAroundWindow.Owner = Window.GetWindow(this);
                    customSearchAroundWindow.SearchRequest += CustomSearchAroundWindow_SearchRequest;
                    customSearchAroundWindow.ShowDialog();
                };

                rightMenuItem.IsEnabled = true;
                menuItemIconFile = "RightClickCustomSearchAroundImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
            }
            else
            {
                rightMenuItem.IsEnabled = false;
                menuItemIconFile = "RightClickCustomSearchAroundImage";
                MakeRightClickMenuItem(rightMenuItem, menuItemHeader, menuItemIconFile);
                rightMenuItem.Foreground = Brushes.Gray;
            }

            menuItem.Items.Add(rightMenuItem);
        }

        private async void CustomSearchAroundWindow_SearchRequest(object sender, CustomSearchAroundSearchRequestEventArgs e)
        {
            try
            {
                await ShowCustomSearchAroundResultsOnGraph(e.SearchAroundResult);
            }
            catch (Exception ex)
            {
                HandleSearchAroundErrorException(ex);
            }
        }

        #endregion

        public void RemoveObjects(IEnumerable<KWObject> objectsToRemove)
        {
            graphControl.RemoveObjects(objectsToRemove.ToList());
        }

        public event NotifyCollectionChangedEventHandler ObjectsPropertiesChanged;
        protected void OnObjectsPropertiesChanged(NotifyCollectionChangedEventArgs e)
        {
            ObjectsPropertiesChanged?.Invoke(this, e);
        }

        public void ChangeProperties(PropertiesChangedArgs args)
        {
            if (args == null)
                return;

            IEnumerable<KWProperty> allPropertiesChanged = GetAllPropertiesChanged(args);
            if (allPropertiesChanged == null)
                return;

            ICollection<KWObject> showingObjects = graphControl.GetShowingObjects();

            List<KWObject> ObjectsWhosePropertiesHaveChanged = allPropertiesChanged.Select(p => p.Owner).Intersect(showingObjects).ToList();

            if (ObjectsWhosePropertiesHaveChanged?.Count > 0)
            {
                IEnumerable<KWObject> added = new List<KWObject>();
                IEnumerable<KWObject> removed = new List<KWObject>();
                if (args.AddedProperties != null)
                    added = args.AddedProperties.Select(p => p.Owner);

                if (args.RemovedProperties != null)
                    removed = args.RemovedProperties.Select(p => p.Owner);

                OnObjectsPropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, added, removed));
            }
        }

        private IEnumerable<KWProperty> GetAllPropertiesChanged(PropertiesChangedArgs args)
        {
            if (args.AddedProperties == null && args.RemovedProperties == null)
                return null;
            else if (args.AddedProperties == null)
                return args.RemovedProperties;
            else if (args.RemovedProperties == null)
                return args.AddedProperties;
            else
                return args.AddedProperties.Union(args.RemovedProperties);
        }

        private void objectCreationControl_OpenPopupRequest(object sender, EventArgs e)
        {
            btnCreatObject.Focus();
            ShowNewObjectPopup();
        }

        public override void Reset()
        {
            objectCreationControl.Reset();
            ClearGraph();
        }
    }
}
