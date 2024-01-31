using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public abstract class DataSourceModel : BaseModel, IDataSource
    {
        #region Properties

        long id;
        public long Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (SetValue(ref title, value))
                {
                    AfterTitleChanged();
                }
            }
        }

        string type = string.Empty;
        public string Type
        {
            get => type;
            set => SetValue(ref type, value);
        }

        BitmapSource largeIcon;
        public BitmapSource LargeIcon
        {
            get => largeIcon;
            set => SetValue(ref largeIcon, value);
        }

        BitmapSource smallIcon;
        public BitmapSource SmallIcon
        {
            get => smallIcon;
            set => SetValue(ref smallIcon, value);
        }

        private DateTime addedTime;
        public DateTime AddedTime
        {
            get => addedTime;
            set => SetValue(ref addedTime, value);
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

        bool? isChecked = false;
        public bool? IsChecked
        {
            get => isChecked;
            set
            {
                if (SetValue(ref isChecked, value))
                {
                    OnIsCheckedChanged();
                }
            }
        }

        bool isValid = false;
        public bool IsValid
        {
            get => isValid;
            set
            {
                if (SetValue(ref isValid, value))
                {
                    OnIsValidChanged();
                }
            }
        }

        MapModel map;
        public MapModel Map
        {
            get => map;
            set
            {
                if (SetValue(ref map, value))
                {
                    if (Map != null)
                    {
                        Map.OwnerDataSource = this;
                        Map.ScenarioChanged -= Map_ScenarioChanged;
                        Map.ScenarioChanged += Map_ScenarioChanged;
                        Map.ScenarioChanged -= Map_IsValidChanged;
                        Map.IsValidChanged += Map_IsValidChanged;
                    }

                    if (!(ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Importing ||
                        ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.InQueue ||
                        ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Transforming ||
                        ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Publishing ))
                        ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;

                    OnMapChanged();
                }
            }
        }

        ObservableCollection<DataSourceFieldModel> fieldCollection;
        public ObservableCollection<DataSourceFieldModel> FieldCollection
        {
            get => fieldCollection;
            set
            {
                ObservableCollection<DataSourceFieldModel> oldVal = FieldCollection;
                if (SetValue(ref fieldCollection, value))
                {
                    if (oldVal != null)
                        oldVal.CollectionChanged -= FieldCollection_CollectionChanged;

                    if (FieldCollection == null)
                    {
                        FieldCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        FieldCollection.CollectionChanged -= FieldCollection_CollectionChanged;
                        FieldCollection.CollectionChanged += FieldCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            FieldCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, FieldCollection));
                        }
                        else
                        {
                            FieldCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, FieldCollection, oldVal));
                        }
                    }
                }
            }
        }

        ObservableCollection<MetaDataItemModel> metaDataCollection;
        public ObservableCollection<MetaDataItemModel> MetaDataCollection
        {
            get => metaDataCollection;
            set
            {
                ObservableCollection<MetaDataItemModel> oldVal = metaDataCollection;
                if (SetValue(ref metaDataCollection, value))
                {
                    if (oldVal != null)
                        oldVal.CollectionChanged -= MetaDataCollection_CollectionChanged;

                    if (MetaDataCollection == null)
                    {
                        MetaDataCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        MetaDataCollection.CollectionChanged -= MetaDataCollection_CollectionChanged;
                        MetaDataCollection.CollectionChanged += MetaDataCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            MetaDataCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, MetaDataCollection));
                        }
                        else
                        {
                            MetaDataCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, MetaDataCollection, oldVal));
                        }
                    }
                }
            }
        }

        DataSourceImportStatus importingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;
        public DataSourceImportStatus ImportingObjectCollectionGenerationStatus
        {
            get => importingObjectCollectionGenerationStatus;
            set
            {
                if (SetValue(ref importingObjectCollectionGenerationStatus, value))
                {
                    OnIsCompletedGenerationImportingObjectCollectionChanged();
                }
            }
        }

        ObservableCollection<ImportingObject> importingObjectCollection;
        public ObservableCollection<ImportingObject> ImportingObjectCollection
        {
            get => importingObjectCollection;
            set
            {
                ObservableCollection<ImportingObject> oldVal = ImportingObjectCollection;
                if (SetValue(ref importingObjectCollection, value))
                {
                    if (oldVal != null)
                        oldVal.CollectionChanged -= ImportingObjectCollection_CollectionChanged;

                    if (ImportingObjectCollection == null)
                    {
                        ImportingObjectCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ImportingObjectCollection.CollectionChanged -= ImportingObjectCollection_CollectionChanged;
                        ImportingObjectCollection.CollectionChanged += ImportingObjectCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            ImportingObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ImportingObjectCollection));
                        }
                        else
                        {
                            ImportingObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ImportingObjectCollection, oldVal));
                        }
                    }
                }
            }
        }

        ObservableCollection<ImportingRelationship> importingRelationshipCollection;
        public ObservableCollection<ImportingRelationship> ImportingRelationshipCollection
        {
            get => importingRelationshipCollection;
            set
            {
                ObservableCollection<ImportingRelationship> oldVal = ImportingRelationshipCollection;
                if (SetValue(ref importingRelationshipCollection, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= ImportingRelationshipCollection_CollectionChanged;
                    }

                    if (ImportingRelationshipCollection == null)
                    {
                        ImportingRelationshipCollection_CollectionChanged(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ImportingRelationshipCollection.CollectionChanged -= ImportingRelationshipCollection_CollectionChanged;
                        ImportingRelationshipCollection.CollectionChanged += ImportingRelationshipCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            ImportingRelationshipCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ImportingRelationshipCollection));
                        }
                        else
                        {
                            ImportingRelationshipCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ImportingRelationshipCollection, oldVal));
                        }
                    }
                }
            }
        }

        ACLModel acl;
        public ACLModel Acl
        {
            get => acl;
            set
            {
                if (SetValue(ref acl, value))
                {
                    if (acl != null)
                    {
                        Acl.OwnerDataSource = this;
                    }

                    OnAclChanged();
                }

            }
        }

        DataSourceImportStatus importStatus = DataSourceImportStatus.Ready;
        public DataSourceImportStatus ImportStatus
        {
            get => importStatus;
            set
            {
                if (SetValue(ref importStatus, value))
                {
                    OnImportStatusChanged();
                }
            }
        }

        GroupDataSourceModel parentDataSource;
        public GroupDataSourceModel ParentDataSource
        {
            get => parentDataSource;
            set
            {
                if (SetValue(ref parentDataSource, value))
                {
                    if (ParentDataSource != null && !ParentDataSource.DataSourceCollection.Contains(this))
                        ParentDataSource.DataSourceCollection.Add(this);

                    DefectionMessageCollection = PrepareDefections();
                }
            }
        }

        ObservableCollection<DefectionModel> defectionMessageCollection;
        public ObservableCollection<DefectionModel> DefectionMessageCollection
        {
            get => defectionMessageCollection;
            set
            {
                if (SetValue(ref defectionMessageCollection, value))
                {

                }
            }
        }

        ObservableCollection<long> importedObjectsId = null;
        public ObservableCollection<long> ImportedObjectsId
        {
            get => importedObjectsId;
            set => SetValue(ref importedObjectsId, value);
        }

        double maximumProgress = 100;
        public double MaximumProgress
        {
            get => maximumProgress;
            set
            {
                SetValue(ref maximumProgress, value);
            }
        }

        double progressValue = 0;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                SetValue(ref progressValue, value);
            }
        }

        bool isIndeterminateProgress = true;
        public bool IsIndeterminateProgress
        {
            get => isIndeterminateProgress;
            set
            {
                SetValue(ref isIndeterminateProgress, value);
            }
        }

        #endregion

        #region Methods

        public DataSourceModel()
        {
            DefectionMessageCollection = new ObservableCollection<DefectionModel>();
            FieldCollection = new ObservableCollection<DataSourceFieldModel>();
            MetaDataCollection = new ObservableCollection<MetaDataItemModel>();
            ImportingObjectCollection = new ObservableCollection<ImportingObject>();
            ImportingRelationshipCollection = new ObservableCollection<ImportingRelationship>();
            ImportedObjectsId = new ObservableCollection<long>();
            AddedTime = DateTime.Now;
            AddConstField();
            Map = new MapModel(this);
            SetDefaultACL();
            IsValid = GetValidation();
            DefectionMessageCollection = PrepareDefections();
        }

        protected virtual void AfterTitleChanged()
        {

        }

        private void SetDefaultACL()
        {
            AccessControl.ACL importAcl = UserAccountControlProvider.ImportACL;

            Acl = new ACLModel()
            {
                Classification = new ClassificationModel()
                {
                    Identifier = importAcl.Classification,
                }
            };

            foreach (AccessControl.ACI permission in importAcl.Permissions)
            {
                Acl.Permissions.Add(new ACIModel
                {
                    GroupName = permission.GroupName,
                    AccessLevel = permission.AccessLevel
                });
            }
        }

        public virtual void RegeneratePreview()
        {
            throw new NotImplementedException();
        }

        private void Map_ScenarioChanged(object sender, EventArgs e)
        {
            //DefectionMessageCollection=PrepareDefections();
            if (!(ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Importing ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.InQueue ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Transforming ||
                ImportingObjectCollectionGenerationStatus == DataSourceImportStatus.Publishing ))
                ImportingObjectCollectionGenerationStatus = DataSourceImportStatus.Ready;
        }

        private void Map_IsValidChanged(object sender, EventArgs e)
        {
            DefectionMessageCollection = PrepareDefections();
        }

        private void FieldCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (DataSourceFieldModel oldItem in e.OldItems)
                {
                    oldItem.OwnerDataSource = null;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (DataSourceFieldModel newItem in e.NewItems)
                {
                    newItem.OwnerDataSource = this;
                }
            }

            UpdateMapValueFields();
            OnFieldCollectionChanged(e);
        }

        private void UpdateMapValueFields()
        {
            IEnumerable<ValueMapModel> allValues = Map?.ObjectCollection?.SelectMany(o => o.GetAllProperties())?.
                   SelectMany(p => p?.GetAllValues());

            if (allValues == null)
                return;

            foreach (ValueMapModel value in allValues)
            {
                DataSourceFieldModel newField = FieldCollection.
                    FirstOrDefault(f => f?.Type == value?.Field?.Type && f?.Title == value?.Field?.Title);

                if (newField != null)
                    value.Field = newField;
            }
        }

        private void MetaDataCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetMetaDataFieldCollection();

            OnMetaDataCollectionChanged(e);
        }

        private void ImportingObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (ImportingObject importingObject in e.OldItems)
                {

                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (ImportingObject importingObject in e.NewItems)
                {
                    importingObject.OwnerDataSource = this;
                }
            }

            if (ImportingObjectCollection == null || ImportingObjectCollection.Count == 0)
            {

            }

            OnImportingObjectCollectionChanged(e);
        }

        private void ImportingRelationshipCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {

            }

            if (e.NewItems?.Count > 0)
            {
                foreach (ImportingRelationship importingRelationship in e.NewItems)
                {
                    importingRelationship.OwnerDataSource = this;
                }
            }

            if (ImportingRelationshipCollection == null || ImportingRelationshipCollection.Count == 0)
            {

            }

            OnImportingRelationshipCollectionChanged(e);
        }

        protected virtual void ResetMetaDataFieldCollection()
        {
            RemoveMetaDataFields();
            AddMetaDataFields();
        }

        private void AddMetaDataFields()
        {
            if (MetaDataCollection == null)
                return;

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
                FieldCollection.Concat(MetaDataCollection.Select(md => new MetaDataFieldModel() { Title = md.Title, RelatedMetaDataItem = md }))
            );
        }

        private void RemoveMetaDataFields()
        {
            if (FieldCollection == null)
                return;

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
                FieldCollection.Except(FieldCollection.OfType<MetaDataFieldModel>())
            );
        }

        protected virtual void ResetMetaDataCollection()
        {
            ResetDataSourceMetaDataCollection();
        }

        protected void ResetDataSourceMetaDataCollection()
        {
            RemoveDataSourceMetaDataCollection();
            AddDataSourceMetaDataCollection();
        }

        private void RemoveDataSourceMetaDataCollection()
        {
            if (MetaDataCollection == null)
                return;

            MetaDataCollection = new ObservableCollection<MetaDataItemModel>(
                MetaDataCollection.Except(MetaDataCollection.Where(md => md.Type == MetaDataType.DataSource))
            );
        }

        private void AddDataSourceMetaDataCollection()
        {
            IEnumerable<MetaDataItemModel> dataSourceMetaData = GetDataSourceMetaData().OfType<MetaDataItemModel>();
            if (MetaDataCollection == null)
                MetaDataCollection = new ObservableCollection<MetaDataItemModel>(dataSourceMetaData.OrderBy(md => md.Type).ThenBy(md => md.Title));
            else
                MetaDataCollection = new ObservableCollection<MetaDataItemModel>(
                    MetaDataCollection.Concat(dataSourceMetaData).OrderBy(md => md.Type).ThenBy(md => md.Title)
                );
        }

        protected virtual IEnumerable<MetaDataItemModel> GetDataSourceMetaData()
        {
            return new List<MetaDataItemModel>();
        }

        protected virtual object RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            return metaDataItem.Value;
        }

        public async Task RecalculateMetaDataItemAsync(MetaDataItemModel metaDataItem)
        {
            object value = null;

            await Task.Run(() =>
            {
                value = RecalculateMetaDataItem(metaDataItem);
            });

            metaDataItem.Value = value;
            metaDataItem.NeedsRecalculation = false;
        }

        protected virtual void ResetFieldCollection()
        {
            FieldCollection.Clear();
            AddConstField();
        }

        private void AddConstField()
        {
            FieldCollection.Add(new ConstFieldModel());
        }

        public virtual bool CanImportWorkSpaceSide()
        {
            return false;
        }

        protected virtual bool GetValidation()
        {
            return false;
        }

        protected virtual ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defection = new ObservableCollection<DefectionModel>();
            if (!Map.IsValid && ParentDataSource == null)
            {
                defection.Add(new DefectionModel
                {
                    Message = "Mapping is not valid",
                    DefectionType = DefectionType.NotValidMap
                });
            }
            return defection;
        }

        protected virtual bool CanGeneratePreview()
        {
            return false;
        }

        protected virtual void SetIcon()
        {

        }

        #endregion

        #region Event

        public event EventHandler PreviewChanged;
        protected void OnPreviewChanged()
        {
            PreviewChanged?.Invoke(this, EventArgs.Empty);
        }

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

        public event EventHandler MapChanged;
        protected void OnMapChanged()
        {
            MapChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler AclChanged;
        protected void OnAclChanged()
        {
            AclChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler FieldCollectionChanged;
        protected void OnFieldCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            FieldCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler MetaDataCollectionChanged;
        protected void OnMetaDataCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            MetaDataCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ImportingObjectCollectionChanged;
        protected void OnImportingObjectCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ImportingObjectCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ImportingRelationshipCollectionChanged;
        protected void OnImportingRelationshipCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ImportingRelationshipCollectionChanged?.Invoke(this, e);
        }

        public event EventHandler ImportStatusChanged;
        protected void OnImportStatusChanged()
        {
            ImportStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsCheckedChanged;
        protected void OnIsCheckedChanged()
        {
            IsCheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsCompletedGenerationImportingObjectCollectionChanged;
        protected void OnIsCompletedGenerationImportingObjectCollectionChanged()
        {
            IsCompletedGenerationImportingObjectCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
