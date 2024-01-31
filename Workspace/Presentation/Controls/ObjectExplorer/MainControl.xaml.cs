using GPAS.Histogram;
using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Model;
using GPAS.Workspace.Presentation.Windows.ObjectExplorer;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet.StartingObjectSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl
    {
        #region رخدادها
        public class AddedObjectSetBasesArgs
        {
            public AddedObjectSetBasesArgs(ObjectSetBase addedObjectSetBase)
            {
                AddedObjectSetBase = addedObjectSetBase;
            }

            public ObjectSetBase AddedObjectSetBase
            {
                private set;
                get;
            }
        }

        public event EventHandler<BeginExecutingStatisticalQueryEventArgs> BeginExecutingStatisticalQuery;

        private void OnBeginExecutingStatisticalQuery(string message)
        {
            BeginExecutingStatisticalQuery?.Invoke(this, new BeginExecutingStatisticalQueryEventArgs(message));
        }

        public event EventHandler<EventArgs> EndExecutingStatisticalQuery;

        private void OnEndExecutingStatisticalQuery()
        {
            EndExecutingStatisticalQuery?.Invoke(this, new EventArgs());
        }

        internal async Task PerformSetAgebraOperation(ObjectSetBase first, ObjectSetBase second, Point position)
        {
            if (first == null || second == null || first == second ||
                first is StartingObjectSetBase || second is StartingObjectSetBase ||
                first.ObjectsCount == 0 || second.ObjectsCount == 0)//دراپ های نامعتبر
                return;

            var algebraResult = SetAlgebraWindow.Show(position,this);

            if (!algebraResult.IsDialogCanceled)
            {
                try
                {
                    OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                    ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                    DerivedObjectSet derivedObjectSet = await objectExplorerModel.Explore(first, second, algebraResult.UserChoice);
                    histogramControl.ShowPreviewStatistics(derivedObjectSet.Preview);
                    visualizationPanel.PreviewStatistics = derivedObjectSet.Preview;
                    formulaPanel.AddItems(new List<ObjectSetBase> { derivedObjectSet });
                    OnObjectSetBaseAdded(derivedObjectSet);
                }
                catch (Exception ex)
                {
                    ExceptionHandler exceptionHandler = new ExceptionHandler();
                    exceptionHandler.WriteErrorLog(ex);
                    KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    OnEndExecutingStatisticalQuery();
                }
            }
        }

        public event EventHandler<AddedObjectSetBasesArgs> ObjectSetBaseAdded;

        private void OnObjectSetBaseAdded(ObjectSetBase addedObjectSetBase)
        {
            ObjectSetBaseAdded?.Invoke(this, new AddedObjectSetBasesArgs(addedObjectSetBase));
        }

        public class ShowOnMapRequestedEventArgs
        {
            public ShowOnMapRequestedEventArgs(List<KWObject> objectRequestedToShowOnMap)
            {
                ObjectRequestedToShowOnMap = objectRequestedToShowOnMap;
            }
            public List<KWObject> ObjectRequestedToShowOnMap
            {
                get;
                private set;
            }
        }
        public event EventHandler<ShowOnMapRequestedEventArgs> ShowOnMapRequested;
        protected void OnShowOnMapRequested(List<KWObject> objectRequestedToShowOnMap)
        {
            if (objectRequestedToShowOnMap == null)
                throw new ArgumentNullException("objectRequestedToShowOnMap");

            if (ShowOnMapRequested != null)
            {
                ShowOnMapRequested(this, new ShowOnMapRequestedEventArgs(objectRequestedToShowOnMap));
            }
        }

        public class ShowOnGraphRequestedEventArgs
        {
            public ShowOnGraphRequestedEventArgs(List<KWObject> objectRequestedToShowOnGraph)
            {
                ObjectRequestedToShowOnGraph = objectRequestedToShowOnGraph;
            }
            public List<KWObject> ObjectRequestedToShowOnGraph
            {
                get;
                private set;
            }
        }
        public event EventHandler<ShowOnGraphRequestedEventArgs> ShowOnGraphRequested;
        protected void OnShowOnGraphRequested(List<KWObject> objectRequestedToShowOnGraph)
        {
            if (objectRequestedToShowOnGraph == null)
                throw new ArgumentNullException("objectRequestedToShowOnGraph");

            if (ShowOnGraphRequested != null)
            {
                ShowOnGraphRequested(this, new ShowOnGraphRequestedEventArgs(objectRequestedToShowOnGraph));
            }
        }

        public event EventHandler<EventArgs> ClearAll;

        public void OnClearAll()
        {
            ClearAll?.Invoke(this, new EventArgs());
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler<Utility.TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(Utility.TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        #endregion

        #region متغیرهای سراسری
        ResourceDictionary iconsResource = new ResourceDictionary();

        #endregion

        public MainControl()
        {
            InitializeComponent();
            Init();
        }

        #region توابع
        private void Init()
        {
            iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
        }

        internal void UpdateFormulaPanel(ObjectSetBase selectedObjectSetBases)
        {
            List<ObjectSetBase> sequence = GetSequence(selectedObjectSetBases);

            formulaPanel.ClearItems();
            formulaPanel.AddItems(sequence);
        }

        public List<ObjectSetBase> GetSequence(ObjectSetBase objectSetBase)
        {
            if (objectSetBase == null)
                throw new ArgumentNullException(nameof(objectSetBase));

            List<ObjectSetBase> sequence = new List<ObjectSetBase>();
            sequence.Add(objectSetBase);

            if (objectSetBase is DerivedObjectSet)
            {
                var par = (objectSetBase as DerivedObjectSet).ParentSet;
                while (true)
                {
                    if (par is StartingObjectSetBase)
                    {
                        sequence.Add(par);
                        break;
                    }

                    sequence.Add(par);
                    par = ((DerivedObjectSet)par).ParentSet;
                }
            }
            sequence.Reverse();
            return sequence;
        }

        private void ChangeMainControlSizeToFullScreenState()
        {
            Step2Grid.RowDefinitions[0].Height = new GridLength(0);
            Step2Grid.ColumnDefinitions[1].Width = new GridLength(0);
            Step2Grid.ColumnDefinitions[1].MinWidth = 0;
        }

        private void ChangeMainControlSizeToNormalState()
        {
            Step2Grid.RowDefinitions[0].Height = GridLength.Auto;
            Step2Grid.ColumnDefinitions[1].Width = GridLength.Auto;
            Step2Grid.ColumnDefinitions[1].MinWidth = 500;
        }

        public async Task RecomputingStatisticalQuery(ObjectSetBase selectedObjectSetBase)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Recomputing_statistics_by_executing_statistical_query);
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                await objectExplorerModel.RecomputeStatistics(selectedObjectSetBase);
                histogramControl.ShowPreviewStatistics(selectedObjectSetBase.Preview);
                visualizationPanel.PreviewStatistics = selectedObjectSetBase.Preview;
                visualizationPanel.HideAllControl();
                await visualizationPanel.ShowActiveToolOfSelectedObjectSetBase(selectedObjectSetBase);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private ObjectSetBase GetActiveObjectSet()
        {
            return formulaPanel.Items.Where(o => o.IsActiveSet).First();
        }

        private async void ExplorAllObject()
        {
            try
            {
                AllObjects firstObjectSetBase = new AllObjects
                {
                    Icon = iconsResource["ObjectExplorerApplicationIcon"] as BitmapImage,
                    IsInActiveSetSequence = true
                };

                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                await objectExplorerModel.RecomputeStatistics(firstObjectSetBase);
                histogramControl.ShowPreviewStatistics(firstObjectSetBase.Preview);

                visualizationPanel.Init(firstObjectSetBase);
                visualizationPanel.PreviewStatistics = firstObjectSetBase.Preview;
                formulaPanel.AddItems(new List<ObjectSetBase> { firstObjectSetBase });
                OnObjectSetBaseAdded(firstObjectSetBase);

                Step1Grid.Visibility = Visibility.Collapsed;
                Step2Grid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        public void ClearObjectExplorerApplication()
        {
            OnClearAll();
            ExplorAllObject();
        }

        public void Reset()
        {
            OnClearAll();
            Step1Grid.Visibility = Visibility.Visible;
            Step2Grid.Visibility = Visibility.Hidden;
            visualizationPanel.Reset();
            histogramControl.Reset();
        }
        #endregion

        #region رخدادگردانها
        private void btnExplorAllObject_Click(object sender, RoutedEventArgs e)
        {
            ExplorAllObject();
        }

        private async void histogramControl_DrillDownRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticsEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                DerivedObjectSet derivedObjectSet =
                    await objectExplorerModel.Explore(formulaPanel.Items.Where(o => o.IsActiveSet).First(),
                        e.PreviewStatistics);
                histogramControl.ShowPreviewStatistics(derivedObjectSet.Preview);
                visualizationPanel.PreviewStatistics = derivedObjectSet.Preview;

                formulaPanel.AddItems(new ObjectSetBase[] { derivedObjectSet });
                OnObjectSetBaseAdded(derivedObjectSet);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message,  MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void histogramControl_ShowRetrievedObjectsOnGraphRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticsEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<ObjectSetBase> objectSets = formulaPanel.Items.ToList();
                ObjectSetBase activeObjectSet = objectSets.Where(o => o.IsActiveSet).First();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PreviewStatistics);
                OnShowOnGraphRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message,  MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void histogramControl_ShowRetrievedObjectsOnMapRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticsEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<ObjectSetBase> objectSets = formulaPanel.Items.ToList();
                ObjectSetBase activeObjectBase = objectSets.Where(o => o.IsActiveSet).First();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectBase, e.PreviewStatistics);

                OnShowOnMapRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void formulaPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ObjectSetBase> selectedObjectSetBase = new List<ObjectSetBase>();
            foreach (ObjectSetBase item in e.AddedItems)
            {
                selectedObjectSetBase.Add(item);
            }

            var preview = selectedObjectSetBase.First().Preview;
            if (preview != null)
            {
                try
                {
                    histogramControl.ShowPreviewStatistics(preview);
                    await visualizationPanel.ShowActiveToolOfSelectedObjectSetBase(selectedObjectSetBase.First());
                    visualizationPanel.PreviewStatistics = preview;
                }
                catch (Exception ex)
                {
                    ExceptionHandler exceptionHandler = new ExceptionHandler();
                    exceptionHandler.WriteErrorLog(ex);
                    KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void formulaPanel_RecomputeRequested(object sender, FormulaPanel.FormulaPanel.RecomputeEventArgs e)
        {
            await RecomputingStatisticalQuery(e.SelectedObjectSetBase);
        }

        private void visualizationPanel_ScreenSizeChanged(object sender, VisualizationPanel.ChangeScreenSizeEventArgs e)
        {
            if (e.ScreenStatus == ScreenStatus.normal)
            {
                ChangeMainControlSizeToNormalState();

            }
            else if (e.ScreenStatus == ScreenStatus.fullScreen)
            {
                ChangeMainControlSizeToFullScreenState();
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        private async void visualizationPanel_LinkTypeHistogramRequested(object sender, EventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                await visualizationPanel.ShowLinkTypeHistogramControl(GetActiveObjectSet());
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_ShowRetrievedLinkedObjectsOnGraphRequested(object sender, VisualizationPanel.ChangePreviewStatisticsSelectionEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<ObjectSetBase> objectSets = formulaPanel.Items.ToList();
                ObjectSetBase activeObjectSet = objectSets.Where(o => o.IsActiveSet).First();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PreviewStatistics);
                OnShowOnGraphRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void histogramControl_PropertyValueFilterRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticEventArgs e)
        {
            try
            {
                PropertyMatchedValueFilterWindow propertyMatchedValueFilterWindow =
                    new PropertyMatchedValueFilterWindow(e.PreviewStatistic.Title, e.PreviewStatistic.TypeURI)
                    {
                        Owner = Window.GetWindow(this)
                    };

                propertyMatchedValueFilterWindow.ShowDialog();
                if (propertyMatchedValueFilterWindow.IsCancel)
                    return;


                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                ObjectSetBase activeObjectSet = GetActiveObjectSet();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                DerivedObjectSet derivedObjectSet
                    = await objectExplorerModel.Explore
                        (activeObjectSet, e.PreviewStatistic.TypeURI, propertyMatchedValueFilterWindow.SearchValue);

                formulaPanel.AddItems(new[] { derivedObjectSet });
                OnObjectSetBaseAdded(derivedObjectSet);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void histogramControl_HistogramThisValueRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                await visualizationPanel.ShowPropertyValueHistogramControl(GetActiveObjectSet(), e.PreviewStatistic);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void histogramControl_BarChartThisValueRequested(object sender, Histograms.HistogramControl.SelectedPreviewStatisticEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                await visualizationPanel.RetrievePropertyBarValuesStatisticsBarChartControl(GetActiveObjectSet(),
                    e.PreviewStatistic);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_DrillDownRequested(object sender, VisualizationPanel.ChangeSelectionEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                ObjectSetBase activeObjectSet = GetActiveObjectSet();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                DerivedObjectSet derivedObjectSet
                    = await objectExplorerModel.Explore
                        (activeObjectSet, e.PropertyValueStatistic);

                formulaPanel.AddItems(new[] { derivedObjectSet });
                OnObjectSetBaseAdded(derivedObjectSet);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_BarChartDrillDownRequested(object sender, Charts.BarChartControlDrillDownRequestEventArgs e)
        {
            try
            {
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                ObjectSetBase activeObjectSet = GetActiveObjectSet();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                DerivedObjectSet derivedObjectSet =
                    await objectExplorerModel.Explore(activeObjectSet, e.PropertyValueRangeDrillDown);

                formulaPanel.AddItems(new[] { derivedObjectSet });
                OnObjectSetBaseAdded(derivedObjectSet);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_BarChartShowRetrievedObjectsOnGraphRequested(object sender, Charts.BarChartControlDrillDownRequestEventArgs e)
        {
            try
            {
                ObjectSetBase activeObjectSet = GetActiveObjectSet();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PropertyValueRangeDrillDown);
                OnShowOnGraphRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_BarChartShowRetrievedObjectsOnMapRequested(object sender, Charts.BarChartControlDrillDownRequestEventArgs e)
        {
            try
            {
                ObjectSetBase activeObjectSet = GetActiveObjectSet();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PropertyValueRangeDrillDown);
                OnShowOnMapRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_ShowRetrievedPropertyObjectsOnGraphRequested(object sender, VisualizationPanel.ChangeSelectionEventArgs e)
        {
            try
            {
                List<ObjectSetBase> objectSets = formulaPanel.Items.ToList();
                ObjectSetBase activeObjectSet = objectSets.Where(o => o.IsActiveSet).First();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PropertyValueStatistic);
                OnShowOnGraphRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private async void visualizationPanel_ShowRetrievedPropertyObjectsOnMapRequested(object sender, VisualizationPanel.ChangeSelectionEventArgs e)
        {
            try
            {
                List<ObjectSetBase> objectSets = formulaPanel.Items.ToList();
                ObjectSetBase activeObjectSet = objectSets.Where(o => o.IsActiveSet).First();
                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
                OnBeginExecutingStatisticalQuery(Properties.Resources.Executing_statistical_query);
                List<KWObject> retrieveObjects =
                    await objectExplorerModel.RetrieveExploredObjects(activeObjectSet, e.PropertyValueStatistic);
                OnShowOnMapRequested(retrieveObjects);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                OnEndExecutingStatisticalQuery();
            }
        }

        private void btnResetObjectExplorer_Click(object sender, RoutedEventArgs e)
        {
            ClearObjectExplorerApplication();
        }

        private void visualizationPanel_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        #endregion        

        private void MarkHistogram_OnItemMouseLeftButtonDown(object sender, ChangeChildrenToShowCountEventArgs e)
        {

        }

        private void MarkHistogram_OnSnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {

        }

        private void visualizationPanel_SnapshotTaken(object sender, Utility.TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        private void histogramControl_SnapshotTaken(object sender, Utility.TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }
    }
}
