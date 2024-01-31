using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public abstract class GroupDataSourceModel : DataSourceModel, IGroupDataSource
    {
        #region Properties

        ObservableCollection<IDataSource> dataSourceCollection;
        public ObservableCollection<IDataSource> DataSourceCollection
        {
            get => dataSourceCollection;
            set
            {
                ObservableCollection<IDataSource> oldVal = DataSourceCollection;
                if (SetValue(ref dataSourceCollection, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= DataSourceCollection_CollectionChanged;
                    }
                    if (DataSourceCollection == null)
                    {
                        DataSourceCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        DataSourceCollection.CollectionChanged -= DataSourceCollection_CollectionChanged;
                        DataSourceCollection.CollectionChanged += DataSourceCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            DataSourceCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, DataSourceCollection));
                        }
                        else
                        {
                            DataSourceCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, DataSourceCollection, oldVal));
                        }
                    }
                }
            }
        }

        ObservableCollection<IDataSource> selectedDataSources;
        public ObservableCollection<IDataSource> SelectedDataSources
        {
            get => selectedDataSources;
            set
            {
                ObservableCollection<IDataSource> oldVal = SelectedDataSources;
                if (SetValue(ref selectedDataSources, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= SelectedDataSources_CollectionChanged;
                    }
                    if (SelectedDataSources == null)
                    {
                        SelectedDataSources_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedDataSources.CollectionChanged -= SelectedDataSources_CollectionChanged;
                        SelectedDataSources.CollectionChanged += SelectedDataSources_CollectionChanged;

                        if (oldVal == null)
                        {
                            SelectedDataSources_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedDataSources));
                        }
                        else
                        {
                            SelectedDataSources_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SelectedDataSources, oldVal));
                        }
                    }
                }
            }
        }

        IDataSource selectedDataSource;
        public IDataSource SelectedDataSource
        {
            get => selectedDataSource;
            set => SetValue(ref selectedDataSource, value);
        }

        MetaDataItemModel dataSourcesCountMetaDataItem = null;
        public MetaDataItemModel DataSourcesCountMetaDataItem
        {
            get => dataSourcesCountMetaDataItem;
            protected set => SetValue(ref dataSourcesCountMetaDataItem, value);
        }

        #endregion

        #region Methods

        public GroupDataSourceModel()
        {
            Type = "Group";
            SetIcon();

            DataSourceCollection = new ObservableCollection<IDataSource>();

            DataSourcesCountMetaDataItem = new MetaDataItemModel()
            {
                OwnerDataSource = this,
                Title = "Number of data sources",
                Value = -1,
                NeedsRecalculation = true,
                Type = MetaDataType.DataSource,
            };
        }

        protected override void SetIcon()
        {
            LargeIcon = SmallIcon = Utility.Utility.GetIconResource("Group");
        }

        private void SelectedDataSources_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedDataSource = SelectedDataSources?.LastOrDefault();
        }

        private void DataSourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                RemoveDataSourceFromSelectedDataSources(e.OldItems.OfType<IDataSource>());

                foreach (IDataSource dataSource in e.OldItems)
                {
                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.IsValidChanged -= DataSource_IsValidChanged;
                    dataSource.ParentDataSource = null;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                IEnumerable<IDataSource> addedDataSources = e.NewItems.OfType<IDataSource>();
                AddDataSourceToSelectedDataSources(addedDataSources.Where(ds => ds.IsSelected));

                foreach (IDataSource dataSource in addedDataSources)
                {
                    if (this is GroupDataSourceModel group)
                        dataSource.ParentDataSource = group;
                    else
                        dataSource.ParentDataSource = null;

                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Selected += DataSource_Selected;

                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.Deselected += DataSource_Deselected;

                    dataSource.IsValidChanged -= DataSource_IsValidChanged;
                    dataSource.IsValidChanged += DataSource_IsValidChanged;
                }
            }

            if (DataSourceCollection == null || DataSourceCollection.Count == 0)
            {
                SelectedDataSources?.Clear();
            }

            IsValid = GetValidation();
            DefectionMessageCollection = PrepareDefections();
            ResetDataSourceMetaDataCollection();
            RegeneratePreview();

            if (!(ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Importing ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.InQueue ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Publishing ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Transforming ))
                ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;

            OnDataSourceCollectionChanged(e);
        }

        private void DataSource_IsValidChanged(object sender, EventArgs e)
        {
            IsValid = GetValidation();
            DefectionMessageCollection = PrepareDefections();
        }

        private void DataSource_Deselected(object sender, EventArgs e)
        {
            RemoveDataSourceFromSelectedDataSources(new List<IDataSource>() { (IDataSource)sender });
        }

        private void RemoveDataSourceFromSelectedDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (SelectedDataSources == null) return;

            SelectedDataSources = new ObservableCollection<IDataSource>(
                SelectedDataSources.Except(dataSources)
            );
        }

        private void DataSource_Selected(object sender, EventArgs e)
        {
            AddDataSourceToSelectedDataSources(new List<IDataSource>() { (IDataSource)sender });
        }

        private void AddDataSourceToSelectedDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null) return;

            if (SelectedDataSources == null)
                SelectedDataSources = new ObservableCollection<IDataSource>(dataSources);
            else
                SelectedDataSources = new ObservableCollection<IDataSource>(
                    SelectedDataSources.Concat(dataSources)
                );
        }

        public void AddRangeDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return;

            DataSourceCollection = DataSourceCollection == null
                ? new ObservableCollection<IDataSource>(dataSources)
                : new ObservableCollection<IDataSource>(DataSourceCollection.Union(dataSources));
        }

        public void RemoveRangeDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return;

            if (DataSourceCollection != null)
            {
                DataSourceCollection = new ObservableCollection<IDataSource>(DataSourceCollection.Except(dataSources));
            }
        }

        public void UngroupDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return;

            List<IDataSource> dataSourcesList = new List<IDataSource>(dataSources);

            RemoveRangeDataSources(dataSources);
            if (ParentDataSource != null)
                MoveDataSources(dataSourcesList, ParentDataSource);

            OnUngroupRequested(new UngroupDataSourceEventArgs(this, dataSourcesList));
        }

        public void MoveDataSources(IEnumerable<IDataSource> dataSources, GroupDataSourceModel target)
        {
            if (dataSources == null || target == null)
                return;

            if (target is ITabularDataSource tabularTarget)
            {
                var validDataSourcesList = DataImportUtility.GetMatchDataSourcesWithTabularDataSource(dataSources, tabularTarget);
                var dataSourcesList = new List<ITabularDataSource>(validDataSourcesList);

                RemoveRangeDataSources(dataSourcesList);
                target.AddRangeDataSources(validDataSourcesList);
            }
        }

        protected override IEnumerable<MetaDataItemModel> GetDataSourceMetaData()
        {
            var result = base.GetDataSourceMetaData().ToList();
            result.Add(CreateNumberOfDataSourcesMetaData());

            return result;
        }

        private MetaDataItemModel CreateNumberOfDataSourcesMetaData()
        {
            if (DataSourcesCountMetaDataItem != null)
            {
                DataSourcesCountMetaDataItem.NeedsRecalculation = false;
                DataSourcesCountMetaDataItem.Value = DataSourceCollection == null ? 0 :
                    DataSourceCollection.OfType<SingleDataSourceModel>()?.LongCount() +
                    DataSourceCollection.OfType<GroupDataSourceModel>().Sum(gds => (long)gds.DataSourcesCountMetaDataItem.Value);
            }

            return DataSourcesCountMetaDataItem;
        }

        public void SelectAllDataSources()
        {
            foreach (var dataSource in DataSourceCollection)
            {
                dataSource.IsSelected = true;
            }
        }

        public void DeselectAllDataSources()
        {
            foreach (var dataSource in DataSourceCollection)
            {
                dataSource.IsSelected = false;
            }
        }

        public long GetAllDataSourcesFileSize()
        {
            if (DataSourceCollection == null)
                return 0;

            long sum = 0;

            foreach (ISingleDataSource dataSource in GetAllSingleDataSources(DataSourceCollection))
            {
                sum += dataSource.FileInfo?.Size == null ? 0 : dataSource.FileInfo.Size;
            }

            return sum;
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defections = base.PrepareDefections();
            if (DataSourceCollection == null || DataSourceCollection.Count == 0)
            {
                defections.Add(new DefectionModel
                {
                    Message = "There is no data source in the group",
                    DefectionType = DefectionType.NotValidGroup
                });
                return defections;
            }

            if (!DataSourceCollection.All(ds => ds.IsValid))
            {
                defections.Add(new DefectionModel
                {
                    Message = "One or more data sources in the group are not valid",
                    DefectionType = DefectionType.NotValidGroup
                });
            }
            //if (HasDuplicateFileInDataSources(DataSourceCollection))
            //{
            //    defections.Add(new DefectionModel
            //    {
            //        Message = "One or more data sources in this group are duplicates",
            //        DefectionType = DefectionType.NotValidGroup
            //    });
            //}
            return defections;
        }

        protected override bool GetValidation()
        {
            return DataSourceCollection != null && DataSourceCollection.Count > 0 && DataSourceCollection.All(ds => ds.IsValid);
                //&& !HasDuplicateFileInDataSources(DataSourceCollection);
        }

        private bool HasDuplicateFileInDataSources(IEnumerable<IDataSource> dataSources)
        {
            List<string> allFilePath = GetAllSingleDataSources(dataSources).Select(sds => sds?.FileInfo?.FullPath).ToList();
            return allFilePath.Count != allFilePath.Distinct().Count();
        }

        private IEnumerable<ISingleDataSource> GetAllSingleDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null || dataSources.Count() == 0)
                return new List<ISingleDataSource>();

            List<ISingleDataSource> singleDataSources = dataSources.OfType<ISingleDataSource>().ToList();
            singleDataSources.AddRange(GetAllSingleDataSources(dataSources.OfType<IGroupDataSource>().SelectMany(gds => gds.DataSourceCollection)));
            return singleDataSources;
        }

        protected override bool CanGeneratePreview()
        {
            return DataSourceCollection != null && DataSourceCollection.Count > 0;
        }

        #endregion

        #region Event

        public event NotifyCollectionChangedEventHandler DataSourceCollectionChanged;
        protected void OnDataSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            DataSourceCollectionChanged?.Invoke(this, e);
        }

        public event EventHandler<UngroupDataSourceEventArgs> UngroupRequested;
        protected void OnUngroupRequested(UngroupDataSourceEventArgs e)
        {
            UngroupRequested?.Invoke(this, e);
        }

        #endregion
    }
}
