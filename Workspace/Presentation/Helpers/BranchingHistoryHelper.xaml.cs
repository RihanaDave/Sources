using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using System;

namespace GPAS.Workspace.Presentation.Helpers
{
    /// <summary>
    /// Interaction logic for BranchingHistoryHelper.xaml
    /// </summary>
    public partial class BranchingHistoryHelper
    {
        public class SelectionChangedArgs
        {
            public SelectionChangedArgs(ObjectSetBase selectedObjectSetBases)
            {
                SelectedObjectSetBases = selectedObjectSetBases;
            }

            public ObjectSetBase SelectedObjectSetBases
            {
                private set;
                get;
            }
        }
        public event EventHandler<SelectionChangedArgs> SelectionChanged;
        public void OnSelectionChanged(ObjectSetBase selectedObjectBase)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangedArgs(selectedObjectBase));
        }

        public class RecomputeRequestedArgs
        {
            public RecomputeRequestedArgs(ObjectSetBase objectSetBase)
            {
                ObjectSetBase = objectSetBase;
            }

            public ObjectSetBase ObjectSetBase
            {
                private set;
                get;
            }
        }
        public event EventHandler<RecomputeRequestedArgs> RecomputeRequested;
        public void OnRecomputeRequested(ObjectSetBase objectSetBase)
        {
            if (RecomputeRequested != null)
                RecomputeRequested(this, new RecomputeRequestedArgs(objectSetBase));
        }

        public event EventHandler<Controls.ObjectExplorer.BranchingHistoryControl.ItemDragedAndDropedEventArgs> ItemDragedAndDroped;

        private void OnItemDragedAndDroped(Controls.ObjectExplorer.BranchingHistoryControl.ItemDragedAndDropedEventArgs args)
        {
            ItemDragedAndDroped?.Invoke(this, args);
        }

        public BranchingHistoryHelper()
        {
            InitializeComponent();
        }

        public void ShowAddedObjectSetBases(ObjectSetBase addedObjectSetBase)
        {
            branchingHistoryControl.AddITems(addedObjectSetBase);
        }

        private void branchingHistoryControl_SelectionChanged(object sender, Controls.ObjectExplorer.BranchingHistoryControl.SelectionChangedArgs e)
        {
            OnSelectionChanged(e.SelectedObjectSetBases);
            
        }

        private void branchingHistoryControl_RecomputeRequested(object sender, Controls.ObjectExplorer.BranchingHistoryControl.RecomputeRequestedArgs e)
        {
            OnRecomputeRequested(e.SelectedObjectSetBases);
        }

        private void branchingHistoryControl_ItemDragedAndDroped(object sender, Controls.ObjectExplorer.BranchingHistoryControl.ItemDragedAndDropedEventArgs e)
        {
            OnItemDragedAndDroped(e);
        }

        public void ClearAll()
        {
            branchingHistoryControl.ClearAll();
        }
    }
}
