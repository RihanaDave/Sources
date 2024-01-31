using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Helpers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using GPAS.Workspace.Presentation.Controls.ObjectExplorer;
using GPAS.Workspace.Presentation.Utility;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public class ObjectExplorerApplicationContainer : ApplicationContainerBase
    {
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

        public ObjectExplorerApplicationContainer()
        {
            branchingHistoryHelper = new BranchingHistoryHelper();
            branchingHistoryHelper.VerticalAlignment = VerticalAlignment.Stretch;
            helpers.Add(branchingHistoryHelper);
            AddHelper(branchingHistoryHelper, HelperPosition.Top);

            objectExplorerApplication.mainControl.ClearAll += MainControl_ClearAll;
            objectExplorerApplication.mainControl.BeginExecutingStatisticalQuery += MainControlOnBeginExecutingStatisticalQuery;
            objectExplorerApplication.mainControl.EndExecutingStatisticalQuery += MainControlOnEndExecutingStatisticalQuery;

            objectExplorerApplication.ObjectSetBaseAdded += ObjectExplorerApplication_ObjectSetBaseAdded;
            objectExplorerApplication.SnapshotRequested += ObjectExplorerApplication_SnapshotRequested;
            objectExplorerApplication.SnapshotTaken += ObjectExplorerApplication_SnapshotTaken;

            branchingHistoryHelper.SelectionChanged += BranchingHistoryHelper_SelectionChanged;
            branchingHistoryHelper.RecomputeRequested += BranchingHistoryHelper_RecomputeRequested;
            branchingHistoryHelper.ItemDragedAndDroped += BranchingHistoryHelper_ItemDragedAndDroped;
        }        

        private void MainControlOnBeginExecutingStatisticalQuery(object sender, BeginExecutingStatisticalQueryEventArgs e)
        {
            BaseWaitingControl.Message = e.WaitingMessage;
            BaseWaitingControl.TaskIncrement();
        }

        private void MainControlOnEndExecutingStatisticalQuery(object sender, EventArgs e)
        {
            BaseWaitingControl.TaskDecrement();
        }        

        private void MainControl_ClearAll(object sender, EventArgs e)
        {
            branchingHistoryHelper.ClearAll();
        }

        private async void BranchingHistoryHelper_ItemDragedAndDroped(object sender, Controls.ObjectExplorer.BranchingHistoryControl.ItemDragedAndDropedEventArgs e)
        {
            await objectExplorerApplication.mainControl.PerformSetAgebraOperation(e.First, e.Second, e.Position);
        }

        private async void BranchingHistoryHelper_RecomputeRequested(object sender, BranchingHistoryHelper.RecomputeRequestedArgs e)
        {
            await objectExplorerApplication.mainControl.RecomputingStatisticalQuery(e.ObjectSetBase);
        }

        private void BranchingHistoryHelper_SelectionChanged(object sender, BranchingHistoryHelper.SelectionChangedArgs e)
        {
            objectExplorerApplication.mainControl.UpdateFormulaPanel(e.SelectedObjectSetBases);
        }

        private void ObjectExplorerApplication_ObjectSetBaseAdded(object sender, ObjectExplorerApplication.AddedObjectSetBasesArgs e)
        {
            branchingHistoryHelper.ShowAddedObjectSetBases(e.AddedObjectSetBase);
        }

        private void ObjectExplorerApplication_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void ObjectExplorerApplication_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        private ObjectExplorerApplication objectExplorerApplication = new ObjectExplorerApplication();
        private List<PresentationHelper> helpers = new List<PresentationHelper>();
        private BranchingHistoryHelper branchingHistoryHelper;

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
                return iconsResource["ObjectExplorerApplicationIcon"] as BitmapImage;
            }
        }

        public override PresentationApplication MasterApplication
        {
            get
            {
                return objectExplorerApplication;
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
                return Properties.Resources.Object_Explorer_Application;
            }
        }
    }
}
