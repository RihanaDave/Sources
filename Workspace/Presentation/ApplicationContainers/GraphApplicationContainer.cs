using GPAS.FilterSearch;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Helpers;
using GPAS.Workspace.Presentation.Helpers.Filter;
using GPAS.Workspace.Presentation.Helpers.Graph;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Utility;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class GraphApplicationContainer : ApplicationContainerBase
    {
        public static int count = 0;
        public GraphApplicationContainer()
        {
            InitiateContainer();
        }

        public event EventHandler<SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        public event EventHandler<DocumentCreationRequestSubmitedEventAgrs> DocumentCreationRequestSubmited;

        protected virtual void OnDocumentCreationRequestSubmited(string filePath)
        {
            DocumentCreationRequestSubmited?.Invoke(this, new DocumentCreationRequestSubmitedEventAgrs(filePath));
        }

        private FilterHelper filterHelper;
        private HistogramHelper histogramHelper;
        private FlowsHelper flowsHelper;
        private TimelineHelper timelineHelper;

        ObjectsSelectionObserver selectionsSourcedByGraphApplicationObserver = new ObjectsSelectionObserver();
        ObjectsShowingObserver showingByTimelineHelperObserver = new ObjectsShowingObserver();
        ObjectsFilteringObserver filterObserver = new ObjectsFilteringObserver();
        Query filterHelperQuery = new Query();
        Query timelineQuery = new Query();

        private void InitiateContainer()
        {
            // ایجاد ابزارک‌های مخصوص گراف
            filterHelper = new FilterHelper();
            helpers.Add(filterHelper);
            histogramHelper = new HistogramHelper();
            helpers.Add(histogramHelper);
            flowsHelper = new FlowsHelper();
            flowsHelper.Init(masterApplication.graphControl);
            helpers.Add(flowsHelper);
            timelineHelper = new TimelineHelper();
            helpers.Add(timelineHelper);

            // ایجاد ناظرهای مخصوص گراف
            var showingByFilterSearchObserver = new ObjectsShowingObserver();
            var selectionsSourcedByFilterHelperObserver = new ObjectsSelectionObserver();
            var selectionsSourcedByHistogramHelperObserver = new ObjectsSelectionObserver();
            var selectionsSourcedByTimelineHelperObserver = new ObjectsSelectionObserver();

            filterObserver.AddListener(masterApplication);
            showingByFilterSearchObserver.AddListener(masterApplication);
            showingByTimelineHelperObserver.AddListener(timelineHelper);

            selectionsSourcedByFilterHelperObserver.AddListener(masterApplication);
            selectionsSourcedByHistogramHelperObserver.AddListener(masterApplication);
            selectionsSourcedByTimelineHelperObserver.AddListener(masterApplication);
            selectionsSourcedByGraphApplicationObserver.AddListener(histogramHelper);
            selectionsSourcedByGraphApplicationObserver.AddListener(timelineHelper);
            // ناظر = Obsever
            // تنظیم رخدادگران‌های ابزارک فیلتر برای تعامل با ناظر

            filterHelper.FilterApplied += (sender, e) =>
            {
                filterHelperQuery = e.Query;
                ApplyFilter(new List<Query>() { filterHelperQuery, timelineQuery });
            };

            filterHelper.FilterCleared += (sender, e) =>
            {
                filterHelperQuery = new Query();
                ApplyFilter(new List<Query>() { filterHelperQuery, timelineQuery });
            };

            filterHelper.FilterSearchCompleted += (sender, e) =>
            {
                try
                {
                    BaseWaitingControl.Message = Properties.Resources.Apply_Search_Results;
                    BaseWaitingControl.TaskIncrement();

                    ObjectsShowingArgs objectsShowingArgs = new ObjectsShowingArgs()
                    {
                        ObjectsToShow = e.Result
                    };

                    showingByFilterSearchObserver.ReportAction(objectsShowingArgs);
                    BaseWaitingControl.TaskDecrement();
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Show_Filter_Search_Result_On_Graph, ex.Message),
                         MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            filterHelper.FilterAppliedAsSelection += async (sender, e) =>
            {
                try
                {
                    BaseWaitingControl.Message = Properties.Resources.Performing_Selection_By_Filter_Criteria;
                    BaseWaitingControl.TaskIncrement();

                    selectionsSourcedByFilterHelperObserver.ReportAction
                        (new ObjectsSelectionChangedArgs(await Logic.FilterProvider.ApplyFilterOnAsync(masterApplication.graphControl.GetShowingObjects(), e.Query)));
                    BaseWaitingControl.TaskDecrement();
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Perform_Selection_By_Filter_Criteria, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            // تنظیم رخدادگران ابزارک هیستوگرام برای پاس دادن صحیح اشیا انتخاب شده
            histogramHelper.SelectionChanged += (sender, e) =>
            {
                try
                {
                    selectionsSourcedByGraphApplicationObserver.RemoveListener(histogramHelper);
                    selectionsSourcedByHistogramHelperObserver.ReportAction(new ObjectsSelectionChangedArgs(e.SelectedObjects));
                    selectionsSourcedByGraphApplicationObserver.AddListener(histogramHelper);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            histogramHelper.SnapshotTaken += HistogramHelper_SnapshotTaken;

            timelineHelper.ObjectsSelectionRequested += (sender, e) =>
            {
                try
                {
                    selectionsSourcedByGraphApplicationObserver.RemoveListener(histogramHelper);
                    selectionsSourcedByGraphApplicationObserver.RemoveListener(timelineHelper);

                    selectionsSourcedByTimelineHelperObserver.ReportAction(new ObjectsSelectionChangedArgs(e.Objects));

                    selectionsSourcedByGraphApplicationObserver.AddListener(timelineHelper);
                    selectionsSourcedByGraphApplicationObserver.AddListener(histogramHelper);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            timelineHelper.FilterQueryChanged += (sender, e) =>
            {
                timelineQuery = (Query)e.NewValue;
                ApplyFilter(new List<Query>() { filterHelperQuery, timelineQuery });
            };

            timelineHelper.SnapshotRequested -= TimelineHelper_SnapshotRequested;
            timelineHelper.SnapshotRequested += TimelineHelper_SnapshotRequested;

            // تنظیم ناظر اتنخاب اشیا برای گوش دادن به رخداد انتخاب اشیا روی گراف
            masterApplication.ObjectsSelectionChanged += (sender, e) =>
            {
                var temp = new ObjectsSelectionChangedArgs(e.CurrentlySelectedObjects);
                try
                {
                    selectionsSourcedByGraphApplicationObserver.ReportAction(temp);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            masterApplication.ObjectsAdded -= MasterApplication_ObjectsAdded;
            masterApplication.ObjectsAdded += MasterApplication_ObjectsAdded;

            masterApplication.ObjectsRemoved -= MasterApplication_ObjectsRemoved;
            masterApplication.ObjectsRemoved += MasterApplication_ObjectsRemoved;

            masterApplication.ObjectsPropertiesChanged -= MasterApplication_ObjectsPropertiesChanged;
            masterApplication.ObjectsPropertiesChanged += MasterApplication_ObjectsPropertiesChanged;

            masterApplication.SnapshotRequested -= MasterApplication_SnapshotRequested;
            masterApplication.SnapshotRequested += MasterApplication_SnapshotRequested;

            masterApplication.DocumentCreationRequestSubmited -= MasterApplication_DocumentCreationRequestSubmited1;
            masterApplication.DocumentCreationRequestSubmited += MasterApplication_DocumentCreationRequestSubmited1;

            RightHelperTabs_SelectionChanged += GraphApplicationContainer_RightHelperTabs_SelectionChanged;
            BottomHelperTabs_SelectionChanged += GraphApplicationContainer_BottomHelperTabs_SelectionChanged;

            // نمایش ابزارک‌ها
            AddHelper(filterHelper, HelperPosition.Right);
            AddHelper(histogramHelper, HelperPosition.Right);
            AddHelper(flowsHelper, HelperPosition.Left);
            AddHelper(timelineHelper, HelperPosition.Bottom);

        }

        private void MasterApplication_DocumentCreationRequestSubmited1(object sender, DocumentCreationRequestSubmitedEventAgrs e)
        {
            OnDocumentCreationRequestSubmited(e.FilePath);
        }

        private void HistogramHelper_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        protected override void RightIsExpandedPropertyChanged()
        {
            if (RightIsExpanded)
            {
                AddListenerToGraphSelectionsObserver(histogramHelper);
            }
            else
            {
                RemoveListenerFromGraphSelectionsObserver(histogramHelper);
            }

        }

        protected override void BottomIsExpandedPropertyChanged()
        {
            if (BottomIsExpanded)
            {
                AddListenerToGraphSelectionsObserver(timelineHelper);
            }
            else
            {
                RemoveListenerFromGraphSelectionsObserver(timelineHelper);
            }
        }

        /// <summary>
        /// لیستی از کوئری ها را گرفته آنها را به یک کوئری واحد تبدیل کرده و براساس این کوئری اشیاء مشاهده شده روی گراف را فیلتر می کند.
        /// عملگر شرطی بین کوئری های به هم متصل شده Any یا همان OR می باشد.
        /// </summary>
        /// <param name="queries">لیست کوئری ها</param>
        private void ApplyFilter(List<Query> queries)
        {
            Query query = new Query();
            query.CriteriasSet.SetOperator = BooleanOperator.Any;
            foreach (var q in queries)
            {
                if (!q.CriteriasSet.IsEmpty())
                {
                    query.CriteriasSet.Criterias.Add(new ContainerCriteria()
                    {
                        CriteriaSet = q.CriteriasSet,
                    });
                }
            }

            ApplyFilter(query);
        }

        /// <summary>
        /// براساس کوئری ارسال شده اشیاء مشاهده شده روی گراف را فیلتر می کند.
        /// </summary>
        /// <param name="query">کوئری</param>
        private void ApplyFilter(Query query)
        {
            try
            {
                filterObserver.ReportAction(new ObjectsFilteringArgs() { FilterToApply = query });
            }
            catch (Exception ex)
            {
                BaseWaitingControl.TaskDecrement();
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Filter, ex.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private void MasterApplication_ObjectsPropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetTimelineHelper();
        }

        private void MasterApplication_ObjectsRemoved(object sender, EventArgs e)
        {
            ResetTimelineHelper();
        }

        private void MasterApplication_ObjectsAdded(object sender, EventArgs e)
        {
            ResetTimelineHelper();
        }

        private void ResetTimelineHelper()
        {
            try
            {
                BaseWaitingControl.Message = "Reset Timeline";
                BaseWaitingControl.TaskIncrement();

                ObjectsShowingArgs objectsShowingArgs = new ObjectsShowingArgs()
                {
                    ObjectsToShow = masterApplication.graphControl.GetShowingObjects()
                };

                showingByTimelineHelperObserver.ReportAction(objectsShowingArgs);
                BaseWaitingControl.TaskDecrement();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format("Reset timeline is not successful.", ex.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
            finally
            {
                BaseWaitingControl.TaskDecrement();
            }
        }

        private void GraphApplicationContainer_BottomHelperTabs_SelectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TimelineHelper)
                AddListenerToGraphSelectionsObserver(timelineHelper);
            else
                RemoveListenerFromGraphSelectionsObserver(timelineHelper);
        }

        private void GraphApplicationContainer_RightHelperTabs_SelectionChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is HistogramHelper)
                AddListenerToGraphSelectionsObserver(histogramHelper);
            else
                RemoveListenerFromGraphSelectionsObserver(histogramHelper);
        }

        private void TimelineHelper_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void MasterApplication_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void HistogramHelper_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        internal async Task<GraphApplicationStatus> GetGraphApplicationStatus()
        {
            GraphApplicationStatus graphAppStatus = await masterApplication.GetGraphApplicationStatus();
            return graphAppStatus;
        }

        internal async Task SetGraphApplicationStatus(GraphApplicationStatus graphAppStatus)
        {
            await masterApplication.SetGraphApplicationStatus(graphAppStatus);
        }

        private void RemoveListenerFromGraphSelectionsObserver(IObjectsSelectableListener listener)
        {
            selectionsSourcedByGraphApplicationObserver.RemoveListener(listener);
        }

        private void AddListenerToGraphSelectionsObserver(IObjectsSelectableListener listener)
        {
            selectionsSourcedByGraphApplicationObserver.AddListener(listener);
            try
            {
                selectionsSourcedByGraphApplicationObserver.ReportAction(
                    new ObjectsSelectionChangedArgs(((masterApplication as GraphApplication).graphControl).GetSelectedObjects()));
            }
            catch (Exception ex)
            {
                BaseWaitingControl.TaskDecrement();
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                   MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private List<PresentationHelper> helpers = new List<PresentationHelper>();
        public override List<PresentationHelper> Helpers
        {
            get
            {
                return helpers;
            }
        }

        public override BitmapImage Icon
        {
            get
            {
                ResourceDictionary iconsResource = new ResourceDictionary();
                iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
                return iconsResource["GraphApplicationIcon"] as BitmapImage;
            }
        }
        public override BitmapImage SelectedArrow
        {
            get
            {
                ResourceDictionary iconsResource = new ResourceDictionary();
                iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
                return iconsResource["SelectedArrowIcon"] as BitmapImage;
            }
        }

        private GraphApplication masterApplication = new GraphApplication();
        public override PresentationApplication MasterApplication
        {
            get
            {
                return masterApplication;
            }
        }

        public override Type MasterApplicationType
        {
            get
            {
                return typeof(GraphApplication);
            }
        }

        public override string Title
        {
            get
            {
                return Properties.Resources.Graph_Application;
            }
        }

        public async override void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {
            if (commandShortCutKey == SupportedShortCutKey.Ctrl_L)
                ShowHelper(filterHelper);
            else if (commandShortCutKey == SupportedShortCutKey.Esc)
            {
                (MasterApplication as GraphApplication).HideRightClickMenuItem();
                (MasterApplication as GraphApplication).HideCreateNewObjectPopup();

            }
            else if (commandShortCutKey == SupportedShortCutKey.Del)
            {
                (MasterApplication as GraphApplication).RemoveSelectedObjectsAndLinksFromGraph();
            }
            else if (commandShortCutKey == SupportedShortCutKey.Shift_Del)
            {
                await (MasterApplication as GraphApplication).DeleteSelectedLinksFromCache();
                await (MasterApplication as GraphApplication).DeleteSelectedObjectsFromCache();
            }
            else if (commandShortCutKey == SupportedShortCutKey.Ctrl_D)
            {
                (MasterApplication as GraphApplication).HardDeleteSelectedLinks();
                (MasterApplication as GraphApplication).HardDeleteSelectedObjects();
            }
            else
                base.PerformCommandForShortcutKey(commandShortCutKey);
        }

        public byte[] TakeImageOfGraph()
        {
            return masterApplication.TakeImageOfGraph();
        }

        public BitmapImage TakeBitMapImageOfGraph()
        {
            return masterApplication.TakeBitmapImageOfGraph();
        }
    }
}