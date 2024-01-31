using GPAS.FilterSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Utility;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// Interaction logic for ObjectExplorerApplication.xaml
    /// </summary>
    public partial class ObjectExplorerApplication : IObjectsShowableListener
    {

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
        public event EventHandler<AddedObjectSetBasesArgs> ObjectSetBaseAdded;
        public void OnObjectSetBaseAdded(ObjectSetBase addedObjectSetBase)
        {
            if (ObjectSetBaseAdded != null)
                ObjectSetBaseAdded(this, new AddedObjectSetBasesArgs(addedObjectSetBase));
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

        public ObjectExplorerApplication()
        {
            InitializeComponent();
        }

        public Query CurrentFilter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task ApplyFilter(ObjectsFilteringArgs filterToApply)
        {
            throw new NotImplementedException();
        }

        public void ClearFilter()
        {
            throw new NotImplementedException();
        }

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            throw new NotImplementedException();
        }

        public Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
        {
            throw new NotImplementedException();
        }

        internal void Init()
        {
            
        }

        public void ClearObjectExplorerApplication()
        {
            mainControl.ClearObjectExplorerApplication();
        }

        private void MainControl_ShowOnGraphRequested(object sender, Controls.ObjectExplorer.MainControl.ShowOnGraphRequestedEventArgs e)
        {
            OnShowOnGraphRequested(e.ObjectRequestedToShowOnGraph);
        }

        private void MainControl_ShowOnMapRequested(object sender, Controls.ObjectExplorer.MainControl.ShowOnMapRequestedEventArgs e)
        {
            OnShowOnMapRequested(e.ObjectRequestedToShowOnMap);
        }

        private void mainControl_ObjectSetBaseAdded(object sender, Controls.ObjectExplorer.MainControl.AddedObjectSetBasesArgs e)
        {
            OnObjectSetBaseAdded(e.AddedObjectSetBase);
        }

        private void mainControl_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void mainControl_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        public override void Reset()
        {
            mainControl.Reset();
        }
    }
}
