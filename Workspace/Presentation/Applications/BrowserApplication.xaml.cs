using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Applications.EventsArgs;
using GPAS.Workspace.Presentation.Controls.Browser;
using GPAS.Workspace.Presentation.Controls.Browser.EventsArgs;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Observers.ObjectsRemoving;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Applications
{
    public partial class BrowserApplication : INotifyPropertyChanged, IObjectsShowableListener, IObjectsRemovableListener
    {
        #region متغیرهای سراسری
        ObservableCollection<KWObject> objectCollection = new ObservableCollection<KWObject>();
        public ObservableCollection<KWObject> ObjectCollection
        {
            get => objectCollection;
            set
            {
                ObservableCollection<KWObject> oldVal = ObjectCollection;
                if (SetValue(ref objectCollection, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= ObjectCollection_CollectionChanged;
                    }
                    if (ObjectCollection == null)
                    {
                        ObjectCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
                        ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            ObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ObjectCollection));
                        }
                        else
                        {
                            ObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ObjectCollection, oldVal));
                        }
                    }
                }
            }
        }

        #endregion

        #region مدیریت رخداد        
        public class AddRelationshipToGraphEventArgs
        {
            public AddRelationshipToGraphEventArgs(RelationshipBasedKWLink relationshipToShow)
            {
                RelationshipToShow = relationshipToShow ?? throw new ArgumentNullException(nameof(relationshipToShow));
            }

            public RelationshipBasedKWLink RelationshipToShow
            {
                get;
            }
        }

        public event EventHandler<AddRelationshipToGraphEventArgs> AddRelationshipToGraph;

        private void OnAddRelationshipToGraph(RelationshipBasedKWLink relationshipToShow)
        {
            if (relationshipToShow == null)
                throw new ArgumentNullException(nameof(relationshipToShow));

            AddRelationshipToGraph?.Invoke(this, new AddRelationshipToGraphEventArgs(relationshipToShow));
        }

        public class ShowOnMapRequestedEventArgs
        {
            public ShowOnMapRequestedEventArgs(KWObject objectRequestedToShowOnMap)
            {
                ObjectRequestedToShowOnMap = objectRequestedToShowOnMap;
            }

            public KWObject ObjectRequestedToShowOnMap
            {
                get;
            }
        }

        public event EventHandler<ShowOnMapRequestedEventArgs> ShowOnMapRequested;

        protected void OnShowOnMapRequested(KWObject objectRequestedToShowOnMap)
        {
            if (objectRequestedToShowOnMap == null)
                throw new ArgumentNullException(nameof(objectRequestedToShowOnMap));

            ShowOnMapRequested?.Invoke(this, new ShowOnMapRequestedEventArgs(objectRequestedToShowOnMap));
        }

        public class LoadInImageAnalysisRequestedEventArgs
        {
            public LoadInImageAnalysisRequestedEventArgs(KWObject objectRequested)
            {
                ObjectRequested = objectRequested;
            }

            public KWObject ObjectRequested
            {
                get;
            }
        }

        public event EventHandler<LoadInImageAnalysisRequestedEventArgs> LoadInImageAnalysisRequested;

        protected void OnLoadInImageAnalysisRequested(KWObject objectRequestedToShowOnMap)
        {
            if (objectRequestedToShowOnMap == null)
                throw new ArgumentNullException(nameof(objectRequestedToShowOnMap));

            LoadInImageAnalysisRequested?.Invoke(this, new LoadInImageAnalysisRequestedEventArgs(objectRequestedToShowOnMap));
        }

        public event NotifyCollectionChangedEventHandler PropertiesChanged;

        private void OnPropertiesChanged(NotifyCollectionChangedEventArgs e)
        {
            PropertiesChanged?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #endregion

        #region توابع

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public BrowserApplication()
        {
            InitializeComponent();
            ObjectCollection = new ObservableCollection<KWObject>();
            DataContext = this;
        }

        private bool SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (property == null && value == null)
                return false;

            if (property != null)
            {
                if (property.Equals(value)) return false;
            }

            T oldValue = property;
            property = value;
            NotifyPropertyChanged(oldValue, value, propertyName);
            return true;
        }

        protected virtual void NotifyPropertyChanged<T>(T oldvalue, T newvalue, [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(this, new PropertyChangedExtendedEventArgs<T>(oldvalue, newvalue, propertyName));
        }

        private void ObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ObjectCollection?.Count > 0)
                NoObjectsGrid.Visibility = Visibility.Collapsed;
            else
                NoObjectsGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// آماده سازی و نمایش لیست اشیاء برای نمایش
        /// </summary>
        /// <param name="objectsToBrowse">لیست اشیاء</param>
        /// <returns></returns>
        public async Task BrowseObjects(IEnumerable<KWObject> objectsToBrowse)
        {
            var kwObjects = objectsToBrowse.ToList();

            IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;

            try
            {
                if (kwObjects.Count > 0)
                {
                    foreach (var item in kwObjects)
                    {
                        if (!ObjectCollection.Contains(item))
                        {
                            ObjectCollection.Add(item);

                            var browseControl = await GenerateBrowseControl(item);
                            ObjectsTabControl.Items.Add(browseControl);
                        }
                        FocusOnShowingObjectTab(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Logic.System.WriteExceptionLog(new Exception($"Error Browsing Object", ex));

                KWMessageBox.Show
                (
                    Properties.Resources.Unable_To_Load_Object_Information,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                IsEnabled = true;
            }
        }

        /// <summary>
        /// ایجاد کنترل نمایشگر اشیاء
        /// </summary>
        /// <param name="objectToShow">شی برای نمایش</param>
        /// <returns></returns>
        private async Task<BrowseControl> GenerateBrowseControl(KWObject objectToShow)
        {
            BrowseControl browseControl = new BrowseControl();

            await browseControl.Init(objectToShow);

            browseControl.AddRelationshipToGraph += ClosableTabBody_AddRelationshipToGraph;
            browseControl.ShowOnMapRequested += ClosableTabBody_ShowOnMapRequested;
            browseControl.LoadInImageAnalysisRequested += BrowseControl_LoadInImageAnalysisRequested;
            browseControl.PropertiesChanged += BrowseControl_PropertiesChanged;
            browseControl.ShowInBrowser += BrowseControlOnShowInBrowser;

            return browseControl;
        }

        private void BrowseControl_PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertiesChanged(e);
        }

        private async void BrowseControlOnShowInBrowser(object sender, KWObject e)
        {
            await BrowseObjects(new List<KWObject> { e });
        }

        /// <summary>
        ///انتخاب سربرگ متناسب با شیء مربوطه
        /// </summary>
        private void FocusOnShowingObjectTab(KWObject objectToSelect)
        {
            foreach (var item in ObjectsTabControl.Items)
            {
                if (((BrowseControl)item).ObjectToBrowse.ID == objectToSelect.ID)
                {
                    ObjectsTabControl.SelectedItem = item;
                }
            }
        }

        /// <summary>
        /// استخراج شناسه آیتم انتخاب شده
        /// </summary>
        /// <returns>شناسه آیتم انتخاب شده</returns>
        public long GetSelectedTabObjectId()
        {
            long selectedObjectId = -1;

            var selectedTabItem = ObjectsTabControl.SelectedItem;
            if (selectedTabItem != null && selectedTabItem is BrowseControl control)
            {
                selectedObjectId = control.ObjectToBrowse.ID;
            }

            return selectedObjectId;
        }

        public override void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {
            if (commandShortCutKey == SupportedShortCutKey.Ctrl_N)
            {
                // do nothing
            }
        }

        /// <summary>
        /// ذخیره وضعیت فعلی نمایشگر
        /// </summary>
        /// <returns>وضعیت جاری</returns>
        internal BrowserApplicationStatus GetBrowserApplicationStatus()
        {
            var browserAppStatus = new BrowserApplicationStatus
            {
                BrowsedObjectIds = ObjectCollection?.Select(o => o.ID).ToList(),
                OpenedObjectId = GetSelectedTabObjectId()
            };

            return browserAppStatus;
        }

        /// <summary>
        /// بازیابی وضعیت نمایشگر
        /// </summary>
        /// <param name="browserAppStatus">وضعیت جدید</param>
        /// <returns></returns>
        internal async Task SetBrowserApplicationStatus(BrowserApplicationStatus browserAppStatus)
        {
            if (ObjectsTabControl.Items.Count != 0)
                ObjectsTabControl.Items.Clear();

            if (ObjectCollection.Count != 0)
                ObjectCollection.Clear();

            if (browserAppStatus.OpenedObjectId > 0 &&
                browserAppStatus.BrowsedObjectIds != null)
            {
                KWObject savedSelectedObject = await ObjectManager.GetObjectById(browserAppStatus.OpenedObjectId);
                List<KWObject> savedShowingObjects = await ObjectManager.GetObjectsById(browserAppStatus.BrowsedObjectIds);

                await BrowseObjects(savedShowingObjects);
                FocusOnShowingObjectTab(savedSelectedObject);
            }
        }

        /// <summary>
        /// بروزرسانی تمام زبانه‌ها
        /// </summary>
        public void RefreshAllTabs()
        {
            foreach (var item in ObjectsTabControl.Items)
            {
                ((BrowseControl)item).RefreshView();
            }
        }

        #endregion

        #region مدیریت رخدادگردانها
        private void BrowseControl_LoadInImageAnalysisRequested(object sender, Controls.Browser.EventsArgs.LoadInImageAnalysisRequestedEventArgs e)
        {
            OnLoadInImageAnalysisRequested(e.ObjectRequested);
        }

        private void ClosableTabBody_ShowOnMapRequested(object sender, Controls.Browser.EventsArgs.ShowOnMapRequestedEventArgs e)
        {
            OnShowOnMapRequested(e.ObjectRequestedToShowOnMap);
        }

        public void RemoveObjects(IEnumerable<KWObject> objectsToRemove)
        {
            foreach (var kwObject in objectsToRemove)
            {
                CloseTab(kwObject.ID);
                ObjectCollection.Remove(kwObject);
            }
        }

        private void ClosableTabBody_AddRelationshipToGraph(object sender, Controls.Browser.EventsArgs.AddRelationshipToGraphEventArgs e)
        {
            OnAddRelationshipToGraph(e.RelationshipToShow);
        }

        public async Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
        {
            await BrowseObjects(objectsToShow);
        }

        private void CloseTabButton_OnClick(object sender, RoutedEventArgs e)
        {
            var objectId = ((BrowseControl)((Button)sender).Tag).ObjectToBrowse.ID;
            CloseTab(objectId);
        }

        private void CloseTab(long relatedObjectId)
        {
            var relatedTab = ObjectsTabControl.Items.Cast<object>().FirstOrDefault(tabControlItem =>
                ((BrowseControl)tabControlItem).ObjectToBrowse.ID == relatedObjectId);

            if (relatedTab != null)
            {
                ObjectsTabControl.Items.Remove(relatedTab);
            }

            var objectToRemove = ObjectCollection.FirstOrDefault(x => x.ID == relatedObjectId);

            if (objectToRemove != null)
                ObjectCollection.Remove(objectToRemove);
        }

        private void CloseAllTab(long exceptObjectId = 0)
        {
            if (exceptObjectId == 0)
            {
                ObjectsTabControl.Items.Clear();
                ObjectCollection.Clear();
            }
            else
            {
                var exceptTab = ObjectsTabControl.Items.Cast<object>().FirstOrDefault(tabControlItem =>
                    ((BrowseControl)tabControlItem).ObjectToBrowse.ID == exceptObjectId);
                ObjectsTabControl.Items.Clear();
                if (exceptTab != null)
                    ObjectsTabControl.Items.Add(exceptTab);

                var exceptObject = ObjectCollection.FirstOrDefault(x => x.ID == exceptObjectId);
                ObjectCollection.Clear();
                ObjectCollection.Add(exceptObject);
            }
        }

        private void MouseDown_OnHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
                return;

            var objectId = ((BrowseControl)((TabItem)sender).Content).ObjectToBrowse.ID;
            CloseTab(objectId);
        }

        private void CloseAll_OnClick(object sender, RoutedEventArgs e)
        {
            CloseAllTab();
        }

        private void CloseOtherTabs_OnClick(object sender, RoutedEventArgs e)
        {
            var thisObject = ((BrowseControl)((MenuItem)sender).DataContext).ObjectToBrowse;
            CloseAllTab(thisObject.ID);
            FocusOnShowingObjectTab(thisObject);
        }

        public override void Reset()
        {
            CloseAllTab();
        }

        #endregion
    }
}
