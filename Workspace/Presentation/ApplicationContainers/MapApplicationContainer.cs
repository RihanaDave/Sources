using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using GPAS.Workspace.Presentation.Helpers.Filter;
using GPAS.Workspace.Presentation.Helpers.Map;
using GPAS.Workspace.Presentation.Controls.Timeline;
using GPAS.Workspace.Presentation.Observers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GPAS.FilterSearch;
using GPAS.Workspace.Presentation.Utility;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class MapApplicationContainer : ApplicationContainerBase
    {
        #region متغیرهای سراسری

        private MapApplication masterApplication = new MapApplication();

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        //-----------------------------Helpers----------------------------
        private FilterHelper filterHelper;
        private HistogramHelper histogramHelper;
        private TimelineHelper timelineHelper;
        private HeatmapHelper heatmapHelper;
        private OptionsHelper optionsHelper;

        //-----------------------------Observers----------------------------
        ObjectsShowingObserver showingByTimelineHelperObserver = new ObjectsShowingObserver();
        ObjectsSelectionObserver selectionsSourcedByMapApplicationObserver = new ObjectsSelectionObserver();
        ObjectsSelectionObserver selectionsSourcedByTimelineHelperObserver = new ObjectsSelectionObserver();
        ObjectsFilteringObserver filterObserver = new ObjectsFilteringObserver();
        Query filterHelperQuery = new Query();
        Query timelineQuery = new Query();
        #endregion

        public MapApplicationContainer()
        {
            InitiateContainer();
            histogramHelper = new HistogramHelper();
            helpers.Add(histogramHelper);

            optionsHelper = new OptionsHelper();
            optionsHelper.Init(masterApplication.mainMapControl);
            helpers.Add(optionsHelper);

            filterHelper = new FilterHelper();
            filterHelper.ShowAllComponent = false;
            helpers.Add(filterHelper);

            timelineHelper = new TimelineHelper();
            helpers.Add(timelineHelper);

            heatmapHelper = new HeatmapHelper();
            heatmapHelper.Init(masterApplication.mainMapControl);
            helpers.Add(heatmapHelper);

            var selectionsSourcedByHistogramHelperObserver = new ObjectsSelectionObserver();
            var selectionsSourcedByFilterHelperObserver = new ObjectsSelectionObserver();
            var showingByFilterSearchObserver = new ObjectsShowingObserver();

            selectionsSourcedByMapApplicationObserver.AddListener(histogramHelper);
            selectionsSourcedByMapApplicationObserver.AddListener(timelineHelper);
            showingByTimelineHelperObserver.AddListener(timelineHelper);

            filterObserver.AddListener(masterApplication);
            selectionsSourcedByHistogramHelperObserver.AddListener(masterApplication);
            selectionsSourcedByFilterHelperObserver.AddListener(masterApplication);
            showingByFilterSearchObserver.AddListener(masterApplication);
            selectionsSourcedByTimelineHelperObserver.AddListener(masterApplication);

            AddHelper(filterHelper, HelperPosition.Right);
            AddHelper(histogramHelper, HelperPosition.Right);
            AddHelper(timelineHelper, HelperPosition.Bottom);
            AddHelper(heatmapHelper, HelperPosition.Left);
            AddHelper(optionsHelper, HelperPosition.Right);

            // تنظیم رخدادگران ابزارک هیستوگرام برای پاس دادن صحیح اشیا انتخاب شده
            histogramHelper.SelectionChanged += (sender, e) =>
            {
                try
                {
                    selectionsSourcedByMapApplicationObserver.RemoveListener(histogramHelper);
                    selectionsSourcedByHistogramHelperObserver.ReportAction(new ObjectsSelectionChangedArgs(e.SelectedObjects));
                    selectionsSourcedByMapApplicationObserver.AddListener(histogramHelper);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            histogramHelper.SnapshotTaken += HistogramHelper_SnapshotTaken;

            // تنظیم ناظر اتنخاب اشیا برای گوش دادن به رخداد انتخاب اشیا روی نقشه
            masterApplication.ObjectsSelectionChanged += (sender, e) =>
            {
                try
                {
                    selectionsSourcedByMapApplicationObserver.ReportAction(new ObjectsSelectionChangedArgs(e.CurrentlySelectedObjects));
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            masterApplication.SnapshotRequested += MasterApplication_SnapshotRequested;

            filterHelper.FilterSearchCompleted += (sender, e) =>
            {
                try
                {
                    ObjectsShowingArgs objectsShowingArgs = new ObjectsShowingArgs()
                    {
                        ObjectsToShow = e.Result
                    };
                    showingByFilterSearchObserver.ReportAction(objectsShowingArgs);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();
                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Show_Filter_Search_Result_On_Graph, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };

            filterHelper.FilterApplied += (sender, e) =>
            {
                filterHelperQuery = e.Query;
                FilterApplied(new List<Query>() { filterHelperQuery, timelineQuery });
            };

            filterHelper.FilterCleared += (sender, e) =>
            {
                filterHelperQuery = new Query();
                FilterApplied(new List<Query>() { filterHelperQuery, timelineQuery });
            };

            timelineHelper.ObjectsSelectionRequested += (sender, e) =>
            {
                try
                {
                    selectionsSourcedByMapApplicationObserver.RemoveListener(histogramHelper);
                    selectionsSourcedByMapApplicationObserver.RemoveListener(timelineHelper);

                    selectionsSourcedByTimelineHelperObserver.ReportAction(new ObjectsSelectionChangedArgs(e.Objects));

                    selectionsSourcedByMapApplicationObserver.AddListener(timelineHelper);
                    selectionsSourcedByMapApplicationObserver.AddListener(histogramHelper);
                }
                catch (Exception ex)
                {
                    BaseWaitingControl.TaskDecrement();

                    KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Selection, ex.Message),
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            };

            timelineHelper.FilterQueryChanged += (sender, e) =>
            {
                timelineQuery = (Query)e.NewValue;
                FilterApplied(new List<Query>() { filterHelperQuery, timelineQuery });
            };


            timelineHelper.SnapshotRequested -= TimelineHelper_SnapshotRequested;
            timelineHelper.SnapshotRequested += TimelineHelper_SnapshotRequested;

            masterApplication.ObjectsAdded -= MasterApplication_ObjectsAdded;
            masterApplication.ObjectsAdded += MasterApplication_ObjectsAdded;

            masterApplication.ObjectsRemoved -= MasterApplication_ObjectsRemoved;
            masterApplication.ObjectsRemoved += MasterApplication_ObjectsRemoved;

            masterApplication.ObjectsPropertiesChanged -= MasterApplication_ObjectsPropertiesChanged;
            masterApplication.ObjectsPropertiesChanged += MasterApplication_ObjectsPropertiesChanged;
        }

        private void FilterApplied(List<Query> queries)
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

            FilterApplied(query);
        }

        private void FilterApplied(Query query)
        {
            try
            {
                filterObserver.ReportAction(new ObjectsFilteringArgs() { FilterToApply = query });
            }
            catch (Exception ex)
            {
                BaseWaitingControl.TaskDecrement();
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Apply_Filter, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void MasterApplication_ObjectsPropertiesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
                    ObjectsToShow = masterApplication.mainMapControl.GetShowingObjects()
                };

                showingByTimelineHelperObserver.ReportAction(objectsShowingArgs);
                BaseWaitingControl.TaskDecrement();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format("Reset timeline is not successful.", ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                BaseWaitingControl.TaskDecrement();
            }
        }

        private void TimelineHelper_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void MasterApplication_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void HistogramHelper_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        private void InitiateContainer()
        {
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
                return iconsResource["MapApplicationIcon"] as BitmapImage;
            }
        }

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
                return typeof(MapApplication);
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

        public override string Title
        {
            get
            {
                return Properties.Resources.Map_Application;
            }
        }

        internal MapApplicationStatus GetMapApplicationStatus()
        {
            MapApplicationStatus mapAppStatus = masterApplication.GetMapApplicationStatus();
            return mapAppStatus;
        }

        internal async Task SetMapApplicationStatus(MapApplicationStatus MapAppStatus)
        {
            await masterApplication.SetMapApplicationStatus(MapAppStatus);
        }
    }
}
