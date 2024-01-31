using GPAS.AccessControl;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.ETLProvider;
using GPAS.Workspace.Presentation.Controls.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public class DataImportViewModel : BaseViewModel
    {
        #region Properties

        private MemoryStream copiedMap;
        private MemoryStream copiedPermission;
        private readonly ETLProvider etlProvider = new ETLProvider();

        bool canImportAll = false;
        public bool CanImportAll
        {
            get => canImportAll;
            set => SetValue(ref canImportAll, value);
        }

        bool canImportAllSelected = false;
        public bool CanImportAllSelected
        {
            get => canImportAllSelected;
            set => SetValue(ref canImportAllSelected, value);
        }

        ObservableCollection<string> currentUserGroupNameCollection = new ObservableCollection<string>();
        public ObservableCollection<string> CurrentUserGroupNameCollection
        {
            get => currentUserGroupNameCollection;
            set
            {
                SetValue(ref currentUserGroupNameCollection, value);
            }
        }

        private bool isMapCopied;
        public bool IsMapCopied
        {
            get => isMapCopied;
            set => SetValue(ref isMapCopied, value);
        }

        private bool isPermissionCopied;
        public bool IsPermissionCopied
        {
            get => isPermissionCopied;
            set => SetValue(ref isPermissionCopied, value);
        }

        private SortOrder sortOrder;
        public SortOrder SortOrder
        {
            get => sortOrder;
            set
            {
                if (SetValue(ref sortOrder, value))
                {
                    SortDataSourceCollection();
                }
            }

        }

        private SortPriority sortPriority;
        public SortPriority SortPriority
        {
            get => sortPriority;
            set
            {
                if (SetValue(ref sortPriority, value))
                {
                    SortDataSourceCollection();
                }
            }
        }

        ObservableCollection<IDataSource> dataSourceCollection;
        public ObservableCollection<IDataSource> DataSourceCollection
        {
            get => dataSourceCollection;
            set
            {
                ObservableCollection<IDataSource> oldVal = DataSourceCollection;
                if (SetValue(ref dataSourceCollection, value))
                {
                    if (DataSourceCollection == null)
                    {
                        DataSourceCollectionOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        DataSourceCollection.CollectionChanged -= DataSourceCollectionOnCollectionChanged;
                        DataSourceCollection.CollectionChanged += DataSourceCollectionOnCollectionChanged;
                        if (oldVal == null)
                        {
                            DataSourceCollectionOnCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, DataSourceCollection));
                        }
                        else
                        {
                            oldVal.CollectionChanged -= DataSourceCollectionOnCollectionChanged;
                            DataSourceCollectionOnCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, DataSourceCollection, oldVal));
                        }
                    }
                }
            }
        }

        ObservableCollection<IDataSource> sortedDataSourceCollection;
        public ObservableCollection<IDataSource> SortedDataSourceCollection
        {
            get => sortedDataSourceCollection;
            set
            {
                SetValue(ref sortedDataSourceCollection, value);
            }
        }


        ObservableCollection<string> defectionMessageCollection;
        public ObservableCollection<string> DefectionMessageCollection
        {
            get => defectionMessageCollection;
            set
            {
                SetValue(ref defectionMessageCollection, value);
            }
        }

        List<DatabaseModel> databasesList = null;
        public IReadOnlyCollection<DatabaseModel> DatabasesList
        {
            get => databasesList?.AsReadOnly();
        }

        #endregion

        #region Methods

        public DataImportViewModel()
        {
            SelectedDataSourceCollection = new ObservableCollection<IDataSource>();
            DataSourceCollection = new ObservableCollection<IDataSource>();

            SetCurrentUserGroupNameCollection();

            SortPriority = SortPriority.AddedTime;
            SortOrder = SortOrder.Ascending;
            etlProvider.ChunkUploaded += EtlProvider_ChunkUploaded;
            App.LogReader.DataSourceImportingStateChanged += LogReader_DataSourceImportingStateChanged;
        }

        private void LogReader_DataSourceImportingStateChanged(object sender, Logic.LogReader.DataSourceImportingStateEventArgs e)
        {
            Console.WriteLine("logReader Id: " + e.DataSourceId);
            IDataSource dataSource = DataSourceCollection.FirstOrDefault(ds => ds.Id == e.DataSourceId);
            if (dataSource == null)
                return;

            App.MainWindow.Dispatcher.Invoke(() => {
                switch (e.Message.ToLower())
                {
                    case "ready":
                    case "transforming":
                        dataSource.ImportStatus = DataSourceImportStatus.Transforming;
                        break;
                    case "publishing":
                        dataSource.ImportStatus = DataSourceImportStatus.Publishing;
                        break;
                    case "finished":
                        dataSource.ImportStatus = DataSourceImportStatus.Completed;
                        break;
                    case "pause":
                        dataSource.ImportStatus = DataSourceImportStatus.Pause;
                        break;
                    case "stop":
                        dataSource.ImportStatus = DataSourceImportStatus.Stop;
                        break;
                    case "error":
                    default:
                        dataSource.ImportStatus = DataSourceImportStatus.Failure;
                        break;
                }
            });
        }

        private void EtlProvider_ChunkUploaded(object sender, ChunkUploadedEventArgs e)
        {
            if (StreamDataSourcePair.ContainsKey(e.Stream))
                StreamDataSourcePair[e.Stream].ProgressValue++;
        }

        private async void SetCurrentUserGroupNameCollection()
        {
            IEnumerable<string> groupNames = await GetGroupsOfCurrentUser();
            CurrentUserGroupNameCollection = new ObservableCollection<string>(groupNames);
        }

        public async Task<IEnumerable<string>> GetGroupsOfCurrentUser()
        {
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            UserAccountControlProvider authentication = new UserAccountControlProvider();

            IEnumerable<string> groupNames = null;
            await Task.Run(() =>
            {
                groupNames = groupManagement.GetGroupsOfUser(authentication.GetLoggedInUserName()).GetAwaiter().GetResult();
            });

            return groupNames;
        }

        private void DataSourceCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                RemoveDataSourceFromSelectedDataSourceCollection(e.OldItems.OfType<IDataSource>());

                foreach (IDataSource dataSource in e.OldItems)
                {
                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.IsValidChanged -= DataSource_IsValidChanged;
                    dataSource.AclChanged -= DataSource_AclChanged;
                    dataSource.MapChanged -= DataSource_MapChanged;
                    dataSource.Map.IsValidChanged -= DataSourceMap_IsValidChanged;

                    if (dataSource is GroupDataSourceModel groupDataSource)
                    {
                        groupDataSource.UngroupRequested -= GroupDataSourceOnUngroupRequested;
                    }
                }
            }

            if (e.NewItems?.Count > 0)
            {
                IEnumerable<IDataSource> dataSources = e.NewItems.OfType<IDataSource>();
                AddDataSourceToSelectedDataSourceCollection(dataSources.Where(ds => ds.IsSelected));
                RemoveDataSourceFromSelectedDataSourceCollection(dataSources.Where(ds => !ds.IsSelected));

                foreach (IDataSource dataSource in e.NewItems)
                {
                    dataSource.Selected -= DataSource_Selected;
                    dataSource.Selected += DataSource_Selected;

                    dataSource.Deselected -= DataSource_Deselected;
                    dataSource.Deselected += DataSource_Deselected;

                    dataSource.IsValidChanged -= DataSource_IsValidChanged;
                    dataSource.IsValidChanged += DataSource_IsValidChanged;

                    dataSource.AclChanged -= DataSource_AclChanged;
                    dataSource.AclChanged += DataSource_AclChanged;

                    dataSource.MapChanged -= DataSource_MapChanged;
                    dataSource.MapChanged += DataSource_MapChanged;

                    dataSource.Map.IsValidChanged -= DataSourceMap_IsValidChanged;
                    dataSource.Map.IsValidChanged += DataSourceMap_IsValidChanged;

                    dataSource.ImportStatusChanged -= DataSource_ImportStatusChanged;
                    dataSource.ImportStatusChanged += DataSource_ImportStatusChanged;

                    if (dataSource is GroupDataSourceModel groupDataSource)
                    {
                        groupDataSource.UngroupRequested -= GroupDataSourceOnUngroupRequested;
                        groupDataSource.UngroupRequested += GroupDataSourceOnUngroupRequested;
                    }
                }
            }

            if (DataSourceCollection == null || DataSourceCollection.Count == 0)
            {
                SelectedDataSourceCollection?.Clear();
            }

            SortDataSourceCollection();
            SetCanImportAll();
        }

        private void DataSource_ImportStatusChanged(object sender, EventArgs e)
        {
            SetCanImportAll();
            SetCanImportAllSelected();
        }

        private void DataSource_AclChanged(object sender, EventArgs e)
        {
            SetCanImportAll();
            SetCanImportAllSelected();
        }

        private void DataSource_IsValidChanged(object sender, EventArgs e)
        {
            SetCanImportAll();
            SetCanImportAllSelected();
        }

        private void GroupDataSourceOnUngroupRequested(object sender, UngroupDataSourceEventArgs e)
        {
            if (sender is GroupDataSourceModel groupDataSource)
            {
                if (groupDataSource.ParentDataSource == null)
                {
                    foreach (IDataSource dataSource in e.UngroupedDataSources)
                    {
                        if (dataSource.Map.IsEmpty())
                            DataImportUtility.CreateDefultMapForEmlDataSource(dataSource);

                        DataSourceCollection.Add(dataSource);
                    }
                }
            }
        }

        private void DataSource_MapChanged(object sender, EventArgs e)
        {
            SetCanImportAll();
            SetCanImportAllSelected();
        }

        private void DataSourceMap_IsValidChanged(object sender, EventArgs e)
        {
            SetCanImportAll();
            SetCanImportAllSelected();
        }

        private void SetCanImportAll()
        {
            CanImportAll = CanImportDataSourceCollection(DataSourceCollection);
        }

        private void SetCanImportAllSelected()
        {
            CanImportAllSelected = CanImportDataSourceCollection(SelectedDataSourceCollection);
        }

        private bool CanImportDataSourceCollection(Collection<IDataSource> dataSources)
        {
            return dataSources != null && dataSources.Count > 0 &&
                dataSources.All(ds => ds != null && ds.IsValid && ds.Map != null && ds.Map.IsValid && ds.Acl != null) &&
                dataSources.All(ds => ds?.ImportStatus == DataSourceImportStatus.Ready);
        }

        private void DataSource_Deselected(object sender, EventArgs e)
        {
            RemoveDataSourceFromSelectedDataSourceCollection(new List<IDataSource>() { (IDataSource)sender });
        }

        private void RemoveDataSourceFromSelectedDataSourceCollection(IEnumerable<IDataSource> dataSources)
        {
            if (SelectedDataSourceCollection == null)
                return;

            SelectedDataSourceCollection = new ObservableCollection<IDataSource>(
                SelectedDataSourceCollection.Except(dataSources)
            );
        }

        private void DataSource_Selected(object sender, EventArgs e)
        {
            AddDataSourceToSelectedDataSourceCollection(new List<IDataSource>() { (IDataSource)sender });
        }

        private void AddDataSourceToSelectedDataSourceCollection(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null) return;

            if (SelectedDataSourceCollection == null)
                SelectedDataSourceCollection = new ObservableCollection<IDataSource>(dataSources);
            else
                SelectedDataSourceCollection = new ObservableCollection<IDataSource>(
                    SelectedDataSourceCollection.Concat(dataSources)
                );
        }

        private IDataSource selectedDataSource;

        public IDataSource SelectedDataSource
        {
            get => selectedDataSource;
            set => SetValue(ref selectedDataSource, value);
        }

        private ObservableCollection<IDataSource> selectedDataSourceCollection;
        public ObservableCollection<IDataSource> SelectedDataSourceCollection
        {
            get => selectedDataSourceCollection;
            set
            {
                ObservableCollection<IDataSource> oldValue = SelectedDataSourceCollection;
                if (SetValue(ref selectedDataSourceCollection, value))
                {
                    if (SelectedDataSourceCollection == null)
                    {
                        SelectedDataSourceCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedDataSourceCollection.CollectionChanged -= SelectedDataSourceCollection_CollectionChanged;
                        SelectedDataSourceCollection.CollectionChanged += SelectedDataSourceCollection_CollectionChanged;
                        if (oldValue == null)
                        {
                            SelectedDataSourceCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedDataSourceCollection));
                        }
                        else
                        {
                            oldValue.CollectionChanged -= SelectedDataSourceCollection_CollectionChanged;
                            SelectedDataSourceCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SelectedDataSourceCollection, oldValue));
                        }
                    }
                }
            }
        }

        private void SelectedDataSourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedDataSource = SelectedDataSourceCollection?.LastOrDefault();
            SetCanImportAllSelected();
        }

        public async Task AddDataSource(string[] filePath)
        {
            List<IDataSource> dataSources = new List<IDataSource>();
            await Task.Run(() =>
            {
                dataSources = DataImportUtility.SpecifyDataSourceType(filePath).ToList();
            });

            foreach (IDataSource dataSource in dataSources)
            {
                DataSourceCollection.Add(dataSource);
            }
        }

        public void RemoveSelectedDataSources()
        {
            RemoveDataSources(SelectedDataSourceCollection);
        }

        public void RemoveDataSources(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null || DataSourceCollection == null)
                return;

            DataSourceCollection = new ObservableCollection<IDataSource>(DataSourceCollection.Except(dataSources));
        }

        private void SortDataSourceCollection()
        {
            IEnumerable<IDataSource> dataSources = DataSourceCollection;

            switch (SortPriority)
            {
                case SortPriority.Name:
                    dataSources = SortOrder == SortOrder.Descending ?
                        dataSources.OrderByDescending(x => x.Title) : dataSources.OrderBy(x => x.Title);
                    break;
                case SortPriority.Type:
                    dataSources = SortOrder == SortOrder.Descending ?
                        dataSources.OrderByDescending(x => x.Type) : dataSources.OrderBy(x => x.Type);
                    break;
                case SortPriority.AddedTime:
                default:
                    dataSources = SortOrder == SortOrder.Descending ?
                        dataSources.OrderByDescending(x => x.AddedTime) : dataSources.OrderBy(x => x.AddedTime);
                    break;
            }

            SortedDataSourceCollection = new ObservableCollection<IDataSource>(dataSources);
        }

        public void MakeEmlDataSourcesGroup()
        {
            if (SelectedDataSourceCollection.Count == 0)
                return;

            MakeEmlDataSourcesGroup(SelectedDataSourceCollection);
        }

        private void MakeEmlDataSourcesGroup(IEnumerable<IDataSource> dataSources)
        {
            if (!CanGroupDataSources(dataSources))
                return;

            TabularGroupDataSourceModel emlGroupDataSource = new TabularGroupDataSourceModel()
            {
                Title = "EML group"
            };

            emlGroupDataSource.AddRangeDataSources(dataSources);
            DataImportUtility.CreateDefultMapForEmlDataSource(emlGroupDataSource);

            DataSourceCollection.Add(emlGroupDataSource);
            RemoveDataSources(dataSources);
        }

        public bool AddDataSourcesToTabularGroup(List<IDataSource> dataSources, TabularGroupDataSourceModel group)
        {
            bool canAddAllDataSources = false;
            IEnumerable<ITabularDataSource> validDataSources = DataImportUtility.GetMatchDataSourcesWithTabularDataSource(dataSources, group);
            List<EmlDataSourceModel> emlDataSources = validDataSources.OfType<EmlDataSourceModel>().ToList();

            if (emlDataSources.Count.Equals(dataSources.Count))
            {
                canAddAllDataSources = true;
            }

            group.AddRangeDataSources(emlDataSources);
            RemoveDataSources(emlDataSources);

            return canAddAllDataSources;
        }

        private bool CanGroupDataSources(IEnumerable<IDataSource> dataSources)
        {
            return dataSources.All(dataSource => dataSource is EmlDataSourceModel);
        }

        public void CopyTheMapOfSelectedDataSource()
        {
            copiedMap = new MemoryStream();
            SelectedDataSource.Map.Copy(copiedMap);
            IsMapCopied = true;
        }

        public void CopyThePermissionOfSelectedDataSource()
        {
            copiedPermission = new MemoryStream();
            SelectedDataSource.Acl.Copy(copiedPermission);
            IsPermissionCopied = true;
        }

        public string PasteMapForSelectedDataSources()
        {
            StringBuilder message = new StringBuilder();
            IEnumerable<IDataSource> readyImportStatusDataSources = SelectedDataSourceCollection.Where(ds => ds.ImportStatus == DataSourceImportStatus.Ready);
            IEnumerable<IDataSource> notReadyImportStatusDataSources = SelectedDataSourceCollection.Except(readyImportStatusDataSources);

            foreach (IDataSource dataSource in readyImportStatusDataSources)
            {
                MapModel newMap = Utility.Utility.DeSerialize<MapModel>(copiedMap);
                copiedMap.Position = 0;

                OperationResultModel loadResult = DataImportUtility.LoadMap(dataSource, newMap);

                if (!loadResult.IsOk)
                {
                    message.Append(loadResult.ErrorMessage + Environment.NewLine);
                }
            }
            if (notReadyImportStatusDataSources != null && notReadyImportStatusDataSources.Count() > 0)
            {
                IEnumerable<string> notReadyDSTitles = notReadyImportStatusDataSources.Select(ds => ds.Title);
                string titles = string.Join(", ", notReadyDSTitles);

                message.Append($"The mapping of data sources {titles} cannot be changed.{Environment.NewLine}");
            }

            return message.ToString();
        }

        public string PastePermissionForSelectedDataSources()
        {
            StringBuilder message = new StringBuilder();
            IEnumerable<IDataSource> readyImportStatusDataSources = SelectedDataSourceCollection.Where(ds => ds.ImportStatus == DataSourceImportStatus.Ready);
            IEnumerable<IDataSource> notReadyImportStatusDataSources = SelectedDataSourceCollection.Except(readyImportStatusDataSources);

            foreach (IDataSource dataSource in readyImportStatusDataSources)
            {
                ACLModel newPermission = Utility.Utility.DeSerialize<ACLModel>(copiedPermission);
                copiedPermission.Position = 0;
                DataImportUtility.LoadPermission(dataSource, newPermission);
            }

            if (notReadyImportStatusDataSources != null && notReadyImportStatusDataSources.Count() > 0)
            {
                IEnumerable<string> notReadyDSTitles = notReadyImportStatusDataSources.Select(ds => ds.Title);
                string titles = string.Join(", ", notReadyDSTitles);

                message.Append($"The permission of data sources {titles} cannot be changed.{Environment.NewLine}");
            }
            return message.ToString();
        }

        public async Task Import(IEnumerable<IDataSource> dataSources)
        {
            if (dataSources == null)
                return;

            IEnumerable<UnstructuredDataSourceModel> unstructuredDataSources = dataSources.OfType<UnstructuredDataSourceModel>();

            foreach (UnstructuredDataSourceModel dataSource in unstructuredDataSources)
            {
                await ImportUnstructuredDataSource(dataSource);
            }

            IEnumerable<ITabularDataSource> tabularDatasources = dataSources.OfType<ITabularDataSource>();

            foreach (ITabularDataSource dataSource in tabularDatasources)
            {
                await ImportTabularDataSource(dataSource);
            }
        }

        private bool CanImportDataSource(IDataSource dataSource)
        {
            return dataSource != null && dataSource.Map != null && dataSource.Map.IsValid;
        }

        Dictionary<Stream, IDataSource> StreamDataSourcePair = new Dictionary<Stream, IDataSource>();

        private async Task ImportTabularDataSource(ITabularDataSource dataSource)
        {
            if (!CanImportDataSource(dataSource))
                return;

            dataSource.ImportStatus = DataSourceImportStatus.Importing;
            dataSource.IsIndeterminateProgress = false;

            try
            {
                ACL acl = DataImportUtility.ConvertACLModelToAccessControlACL(dataSource.Acl);

                Logic.DataImport.PublishAdaptor publishAdaptor = new Logic.DataImport.PublishAdaptor();
                if (dataSource is SingleDataSourceModel csvDataSource)
                {
                    dataSource.Id = publishAdaptor.GetNewDataSourceID();
                    Console.WriteLine("Workspace Id: " + dataSource.Id);

                    MemoryStream mapStream = new MemoryStream();
                    dataSource.Map.Copy(mapStream);
                    Stream aclStream = acl.ToJsonStream();
                    string fileLocalPath = csvDataSource.FileInfo.FullPath;
                    Stream dsStream = File.OpenRead(fileLocalPath);
                    StreamDataSourcePair.Add(mapStream, dataSource);
                    StreamDataSourcePair.Add(aclStream, dataSource);
                    StreamDataSourcePair.Add(dsStream, dataSource);

                    dataSource.MaximumProgress = etlProvider.GetTotalNumberOfChunks(mapStream) +
                        etlProvider.GetTotalNumberOfChunks(aclStream) + etlProvider.GetTotalNumberOfChunks(dsStream);

                    string datasourceName = $"datasource.{csvDataSource.FileInfo.Extension.ToLower()}";

                    await Task.Run(async () =>
                    {
                        await etlProvider.UploadFile(dsStream, $"{dataSource.Id}/{datasourceName}");
                        await etlProvider.UploadFile(aclStream, $"{dataSource.Id}/acl.json");
                        await etlProvider.UploadFile(mapStream, $"{dataSource.Id}/mapping.xml");
                    });

                    dataSource.IsIndeterminateProgress = true;
                    dataSource.ProgressValue = 0;
                    dataSource.ImportStatus = DataSourceImportStatus.InQueue;

                    mapStream.Close();
                    aclStream.Close();
                    dsStream.Close();
                    StreamDataSourcePair.Remove(mapStream);
                    StreamDataSourcePair.Remove(aclStream);
                    StreamDataSourcePair.Remove(dsStream);

                    await Task.Run(() =>
                    {
                        etlProvider.CallStartImport(dataSource.Id, datasourceName);
                    });
                }
            }
            catch(Exception ex)
            {
                dataSource.ImportStatus = DataSourceImportStatus.Failure;
                throw;
            }
            finally
            {
                dataSource.IsIndeterminateProgress = true;
                dataSource.ProgressValue = 0;
            }
        }

        private async Task ImportUnstructuredDataSource(UnstructuredDataSourceModel dataSource)
        {
            if (!CanImportDataSource(dataSource))
                return;

            dataSource.ImportStatus = DataSourceImportStatus.Importing;

            ACL acl = DataImportUtility.ConvertACLModelToAccessControlACL(dataSource.Acl);
        }

        public async Task AddTablesAndViewsToDataSources(IEnumerable<SQLServerDataSourceModel> tablesAndViews)
        {
            foreach (SQLServerDataSourceModel dataSource in tablesAndViews)
            {
                if (dataSource.IsValid)
                {
                    IDataSource copyDS = await DataImportUtility.CopyDataSourceAsync(dataSource);
                    DataSourceCollection.Add(copyDS);
                }
            }
        }

        public void SetDatabasesList(IEnumerable<DatabaseModel> databases)
        {
            databasesList = new List<DatabaseModel>(databases);
        }

        public void Reset()
        {
            DataSourceCollection.Clear();
            SortPriority = SortPriority.AddedTime;
            SortOrder = SortOrder.Ascending;
            SetCurrentUserGroupNameCollection();
            copiedMap = null;
            copiedPermission = null;
            IsMapCopied = false;
            IsPermissionCopied = false;
        }

        public void SelectAllDataSources()
        {
            foreach (IDataSource dataSource in DataSourceCollection)
            {
                dataSource.IsSelected = true;
            }
        }

        public void DeselectAllDataSources()
        {
            foreach (IDataSource dataSource in DataSourceCollection)
            {
                dataSource.IsSelected = false;
            }
        }       

        #endregion

        #region Events

        #endregion

        #region Commands

        #endregion
    }
}
