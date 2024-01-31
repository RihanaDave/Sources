using GPAS.FilterSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Controls.Timeline;
using GPAS.Workspace.Presentation.Observers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers
{
    /// <summary>
    /// Interaction logic for TimelineHelper.xaml
    /// </summary>
    public partial class TimelineHelper : PresentationHelper, IObjectsSelectableListener, IObjectsShowableListener
    {
        #region Dependencies

        /// <summary>
        /// مجموعه ای از اشیاء که قصد داریم ویژگی های زمانی آن ها را روی خط زمان‌ مشاهده نماییم.
        /// </summary>
        public ObservableCollection<KWObject> ObjectCollection
        {
            get { return (ObservableCollection<KWObject>)GetValue(ObjectCollectionProperty); }
            set { SetValue(ObjectCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectCollectionProperty =
            DependencyProperty.Register(nameof(ObjectCollection), typeof(ObservableCollection<KWObject>), typeof(TimelineHelper),
                new PropertyMetadata(null, OnSetObjectCollectionChanged));

        private static void OnSetObjectCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineHelper)d).OnSetObjectCollectionChanged(e);
        }

        private void OnSetObjectCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ObjectCollection != null)
            {
                ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
                ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;
            }

            OnObjectCollectionChanged(e);
        }


        #endregion

        #region Methodes

        public TimelineHelper()
        {
            InitializeComponent();

            InitProperties();
            BindProperties();
        }

        private void InitProperties()
        {
            ObjectCollection = new ObservableCollection<KWObject>();
            ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
            ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;
        }

        private void BindProperties()
        {
            BindObjectCollectionProperty();
        }

        private void BindObjectCollectionProperty()
        {
            Binding ObjectCollectionBinding = new Binding(nameof(ObjectCollection));
            ObjectCollectionBinding.Source = this;
            ObjectCollectionBinding.Mode = BindingMode.TwoWay;
            timelineControl.SetBinding(TimelineControl.ObjectCollectionProperty, ObjectCollectionBinding);
        }

        private void ObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<KWObject> oldItems = new ObservableCollection<KWObject>(ObjectCollection);
            if (e.NewItems != null)
                oldItems = new ObservableCollection<KWObject>(ObjectCollection.Except(e.NewItems.OfType<KWObject>()));

            OnObjectCollectionChanged(new DependencyPropertyChangedEventArgs(ObjectCollectionProperty, oldItems, ObjectCollection));
        }

        private void TimelineControl_SelectedRangesChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnSelectedRangesChanged(e);
        }

        private void TimelineControl_ObjectsSelectionRequested(object sender, ObjectsSelectionRequestEventArgs e)
        {
            OnObjectsSelectionRequested(e);
        }

        private void TimelineControl_FilterWindowsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnFilterWindowsChanged(e);
        }

        private void TimelineControl_FilterQueryChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnFilterQueryChanged(e);
        }

        private void TimelineControl_CheckedPropertyTypeUriListChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnCheckedPropertyTypeUriListChanged(e);
        }

        private void TimelineControl_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            timelineControl.HighlightBarsRelatedObjects(objectsToSelect); //only highlight
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            ObjectCollection = new ObservableCollection<KWObject>(objectsToShow);
        }

        public override void Reset()
        {
            timelineControl.Reset();
        }

        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler SelectedRangesChanged;
        protected void OnSelectedRangesChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectedRangesChanged?.Invoke(this, e);
        }

        public event EventHandler<ObjectsSelectionRequestEventArgs> ObjectsSelectionRequested;
        protected void OnObjectsSelectionRequested(ObjectsSelectionRequestEventArgs e)
        {
            ObjectsSelectionRequested?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler FilterWindowsChanged;
        protected void OnFilterWindowsChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterWindowsChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler FilterQueryChanged;
        protected void OnFilterQueryChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterQueryChanged?.Invoke(this, e);
        }


        public event DependencyPropertyChangedEventHandler CheckedPropertyTypeUriListChanged;
        protected void OnCheckedPropertyTypeUriListChanged(DependencyPropertyChangedEventArgs e)
        {
            CheckedPropertyTypeUriListChanged?.Invoke(this, e);
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs e)
        {
            SnapshotRequested?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ObjectCollectionChanged;
        protected void OnObjectCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            ObjectCollectionChanged?.Invoke(this, e);
        }

        #endregion
    }
}
