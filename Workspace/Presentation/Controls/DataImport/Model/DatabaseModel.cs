using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class DatabaseModel : BaseModel, ISelectable, ICheckable
    {
        #region Properties

        string fullPath = string.Empty;
        public string FullPath
        {
            get => fullPath;
            set
            {
                SetValue(ref fullPath, value);
            }
        }

        ObservableCollection<SQLServerDataSourceModel> tablesAndViewsCollection = null;
        public ObservableCollection<SQLServerDataSourceModel> TablesAndViewsCollection
        {
            get => tablesAndViewsCollection;
            set
            {
                ObservableCollection<SQLServerDataSourceModel> oldValue = TablesAndViewsCollection;

                if (SetValue(ref tablesAndViewsCollection, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= TablesAndViewsCollection_CollectionChanged;
                    }

                    if (TablesAndViewsCollection == null)
                    {
                        TablesAndViewsCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        TablesAndViewsCollection.CollectionChanged -= TablesAndViewsCollection_CollectionChanged;
                        TablesAndViewsCollection.CollectionChanged += TablesAndViewsCollection_CollectionChanged;

                        if (oldValue == null)
                        {
                            TablesAndViewsCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, TablesAndViewsCollection));
                        }
                        else
                        {
                            TablesAndViewsCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, TablesAndViewsCollection, oldValue));
                        }
                    }
                }
            }
        }

        bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (SetValue(ref isSelected, value))
                {
                    if (IsSelected)
                        OnSelected();
                    else
                        OnDeselected();
                }
            }
        }

        ObservableCollection<SQLServerDataSourceModel> selectedTablesAndViews = null;
        public ObservableCollection<SQLServerDataSourceModel> SelectedTablesAndViews
        {
            get => selectedTablesAndViews;
            set
            {
                ObservableCollection<SQLServerDataSourceModel> oldValue = SelectedTablesAndViews;
                if (SetValue(ref selectedTablesAndViews, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= SelectedTablesAndViews_CollectionChanged;
                    }
                    if (SelectedTablesAndViews == null)
                    {
                        SelectedTablesAndViews_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedTablesAndViews.CollectionChanged -= SelectedTablesAndViews_CollectionChanged;
                        SelectedTablesAndViews.CollectionChanged += SelectedTablesAndViews_CollectionChanged;

                        if (oldValue == null)
                        {
                            SelectedTablesAndViews_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedTablesAndViews));
                        }
                        else
                        {
                            SelectedTablesAndViews_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SelectedTablesAndViews, oldValue));
                        }
                    }
                }
            }
        }

        SQLServerDataSourceModel selectedTableOrView = null;
        public SQLServerDataSourceModel SelectedTableOrView
        {
            get => selectedTableOrView;
            set => SetValue(ref selectedTableOrView, value);
        }

        bool? isChecked = false;
        public bool? IsChecked
        {
            get => isChecked;
            set
            {
                if (SetValue(ref isChecked, value))
                {
                    AfterIsCheckedChanged(value);

                    OnIsCheckedChanged();
                }
            }
        }

        #endregion

        #region Methods

        public DatabaseModel()
        {
            TablesAndViewsCollection = new ObservableCollection<SQLServerDataSourceModel>();
        }

        private void TablesAndViewsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                RemoveDataSourceFromSelectedDataSources(e.OldItems.OfType<SQLServerDataSourceModel>());

                foreach (SQLServerDataSourceModel dataSource in e.OldItems)
                {
                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.IsCheckedChanged -= DataSource_IsCheckedChanged;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                AddItemToSelectedTablesAndViews(e.NewItems.OfType<SQLServerDataSourceModel>().Where(ds => ds.IsSelected));

                foreach (SQLServerDataSourceModel dataSource in e.NewItems)
                {
                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Selected += DataSource_Selected;

                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.Deselected += DataSource_Deselected;

                    dataSource.IsCheckedChanged -= DataSource_IsCheckedChanged;
                    dataSource.IsCheckedChanged += DataSource_IsCheckedChanged;
                }
            }

            if (TablesAndViewsCollection == null || TablesAndViewsCollection.Count == 0)
            {
                SelectedTablesAndViews?.Clear();
            }

            OnTablesAndViewsCollectionChanged(e);
        }

        private void DataSource_IsCheckedChanged(object sender, EventArgs e)
        {
            SetIsChecked();
            OnCheckedTablesAndViewsChanged();
        }

        private void DataSource_Deselected(object sender, EventArgs e)
        {
            RemoveDataSourceFromSelectedDataSources(new List<SQLServerDataSourceModel>() { (SQLServerDataSourceModel)sender });
        }

        private void RemoveDataSourceFromSelectedDataSources(IEnumerable<SQLServerDataSourceModel> dataSources)
        {
            if (SelectedTablesAndViews == null) return;

            SelectedTablesAndViews = new ObservableCollection<SQLServerDataSourceModel>(
                SelectedTablesAndViews.Except(dataSources)
            );
        }

        private void DataSource_Selected(object sender, EventArgs e)
        {
            AddItemToSelectedTablesAndViews(new List<SQLServerDataSourceModel>() { (SQLServerDataSourceModel)sender });
        }

        private void AddItemToSelectedTablesAndViews(IEnumerable<SQLServerDataSourceModel> dataSources)
        {
            if (dataSources == null) return;

            if (SelectedTablesAndViews == null)
                SelectedTablesAndViews = new ObservableCollection<SQLServerDataSourceModel>(dataSources);
            else
                SelectedTablesAndViews = new ObservableCollection<SQLServerDataSourceModel>(
                    SelectedTablesAndViews.Concat(dataSources)
                );
        }

        private void SelectedTablesAndViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedTableOrView = SelectedTablesAndViews?.LastOrDefault();
        }

        private void SetIsChecked()
        {
            if (TablesAndViewsCollection == null || TablesAndViewsCollection.Count == 0)
                IsChecked = false;
            else if (TablesAndViewsCollection.All(tav => tav.IsChecked == true))
                IsChecked = true;
            else if (TablesAndViewsCollection.All(tav => tav.IsChecked == false))
                IsChecked = false;
            else
                IsChecked = null;
        }

        private void AfterIsCheckedChanged(bool? isChecked)
        {
            if (IsChecked != null)
                foreach (SQLServerDataSourceModel dataSource in TablesAndViewsCollection)
                {
                    dataSource.IsChecked = isChecked;
                }
        }

        #endregion

        #region Events

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Deselected;
        protected void OnDeselected()
        {
            Deselected?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler TablesAndViewsCollectionChanged;
        protected void OnTablesAndViewsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            TablesAndViewsCollectionChanged?.Invoke(this, e);
        }

        public event EventHandler IsCheckedChanged;
        protected void OnIsCheckedChanged()
        {
            IsCheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CheckedTablesAndViewsChanged;
        protected void OnCheckedTablesAndViewsChanged()
        {
            CheckedTablesAndViewsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
