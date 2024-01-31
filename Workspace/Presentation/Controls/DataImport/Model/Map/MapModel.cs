using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class MapModel : MapElement
    {
        #region Properties

        ObservableCollection<ObjectMapModel> objectCollection = new ObservableCollection<ObjectMapModel>();
        public ObservableCollection<ObjectMapModel> ObjectCollection
        {
            get => objectCollection;
            set
            {
                ObservableCollection<ObjectMapModel> oldVal = ObjectCollection;
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

        ObservableCollection<ObjectMapModel> selectedObjects = null;

        [XmlIgnore]
        public ObservableCollection<ObjectMapModel> SelectedObjects
        {
            get => selectedObjects;
            set
            {
                ObservableCollection<ObjectMapModel> oldValue = SelectedObjects;
                if (SetValue(ref selectedObjects, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= SelectedObjects_CollectionChanged;
                    }
                    if (SelectedObjects == null)
                    {
                        SelectedObjects_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedObjects.CollectionChanged -= SelectedObjects_CollectionChanged;
                        SelectedObjects.CollectionChanged += SelectedObjects_CollectionChanged;

                        if (oldValue == null)
                        {
                            SelectedObjects_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedObjects));
                        }
                        else
                        {
                            SelectedObjects_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SelectedObjects, oldValue));
                        }
                    }
                }
            }
        }

        private ObjectMapModel selectedObject = null;

        [XmlIgnore]
        public ObjectMapModel SelectedObject
        {
            get => selectedObject;
            set
            {
                if (SetValue(ref selectedObject, value))
                {
                    OnSelectedObjectChanged();
                }
            }
        }

        ObservableCollection<RelationshipMapModel> relationshipCollection = new ObservableCollection<RelationshipMapModel>();
        public ObservableCollection<RelationshipMapModel> RelationshipCollection
        {
            get => relationshipCollection;
            set
            {
                ObservableCollection<RelationshipMapModel> oldVal = RelationshipCollection;
                if (SetValue(ref relationshipCollection, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= RelationshipCollection_CollectionChanged;
                    }
                    if (RelationshipCollection == null)
                    {
                        RelationshipCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        RelationshipCollection.CollectionChanged -= RelationshipCollection_CollectionChanged;
                        RelationshipCollection.CollectionChanged += RelationshipCollection_CollectionChanged;

                        if (oldVal == null)
                        {
                            RelationshipCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, RelationshipCollection));
                        }
                        else
                        {
                            RelationshipCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, RelationshipCollection, oldVal));
                        }
                    }
                }
            }
        }

        ObservableCollection<RelationshipMapModel> selectedRelationships = null;

        [XmlIgnore]
        public ObservableCollection<RelationshipMapModel> SelectedRelationships
        {
            get => selectedRelationships;
            set
            {
                ObservableCollection<RelationshipMapModel> oldValue = SelectedRelationships;

                if (SetValue(ref selectedRelationships, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= SelectedRelationships_CollectionChanged;
                    }
                    if (SelectedRelationships == null)
                    {
                        SelectedRelationships_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedRelationships.CollectionChanged -= SelectedRelationships_CollectionChanged;
                        SelectedRelationships.CollectionChanged += SelectedRelationships_CollectionChanged;

                        if (oldValue == null)
                        {
                            SelectedRelationships_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SelectedRelationships));
                        }
                        else
                        {
                            SelectedRelationships_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SelectedRelationships, oldValue));
                        }
                    }
                }
            }
        }

        RelationshipMapModel selectedRelationship = null;

        [XmlIgnore]
        public RelationshipMapModel SelectedRelationship
        {
            get => selectedRelationship;
            set
            {
                if (SetValue(ref selectedRelationship, value))
                {
                    OnSelectedRelationshipChanged();
                }
            }
        }

        private MapWarningModel selectedWarning;

        [XmlIgnore]
        public MapWarningModel SelectedWarning
        {
            get => selectedWarning;
            set
            {
                if (SetValue(ref selectedWarning, value))
                {
                    OnSelectedWarningChanged();
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


        //This property does not need to be serialized
        IDataSource ownerDataSource;
        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set
            {
                if (SetValue(ref ownerDataSource, value))
                {
                    if (OwnerDataSource != null)
                    {
                        OwnerDataSource.FieldCollectionChanged -= OwnerDataSource_FieldCollectionChanged;
                        OwnerDataSource.FieldCollectionChanged += OwnerDataSource_FieldCollectionChanged;
                    }
                }
            }
        }

        #endregion

        #region Methods

        public MapModel() : this(null) { }

        public MapModel(IDataSource owner)
        {
            OwnerDataSource = owner;
            ObjectCollection = new ObservableCollection<ObjectMapModel>();
            RelationshipCollection = new ObservableCollection<RelationshipMapModel>();
            PrepareWarnings();
        }

        private void SelectedObjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedObject = SelectedObjects?.LastOrDefault();
            OnSelectedObjectsChanged(e);
        }

        private void RelationshipCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
                foreach (RelationshipMapModel relationshipMap in e.OldItems)
                {
                    relationshipMap.Selected -= Relationship_Selected;
                    relationshipMap.Deselected -= Relationship_Deselected;
                    relationshipMap.ScenarioChanged -= RelationshipMap_ScenarioChanged;
                    RemoveRelationshipFromSelectedRelationships(relationshipMap);
                }

            if (e.NewItems?.Count > 0)
                foreach (RelationshipMapModel relationshipMap in e.NewItems)
                {
                    if (relationshipMap.IsSelected)
                        AddRelationshipToSelectedRelationships(relationshipMap);
                    else
                        RemoveRelationshipFromSelectedRelationships(relationshipMap);

                    relationshipMap.OwnerMap = this;

                    relationshipMap.Selected -= Relationship_Selected;
                    relationshipMap.Selected += Relationship_Selected;

                    relationshipMap.Deselected -= Relationship_Deselected;
                    relationshipMap.Deselected += Relationship_Deselected;

                    relationshipMap.ScenarioChanged -= RelationshipMap_ScenarioChanged;
                    relationshipMap.ScenarioChanged += RelationshipMap_ScenarioChanged;
                }

            if (RelationshipCollection == null || RelationshipCollection.Count == 0)
            {
                SelectedRelationships?.Clear();
            }

            OnRelationshipCollectionChanged(e);
        }

        public void SelectAllObjects()
        {
            foreach (ObjectMapModel objectMap in ObjectCollection)
            {
                objectMap.IsSelected = true;
            }
        }

        public void DeselectAllObjects()
        {
            foreach (ObjectMapModel objectMap in ObjectCollection)
            {
                objectMap.IsSelected = false;
            }
        }

        private void SelectedRelationships_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedRelationship = SelectedRelationships?.LastOrDefault();
            OnSelectedRelationshipsChanged(e);
        }

        private void RelationshipMap_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void Relationship_Deselected(object sender, EventArgs e)
        {
            RemoveRelationshipFromSelectedRelationships((RelationshipMapModel)sender);
        }

        private bool RemoveRelationshipFromSelectedRelationships(RelationshipMapModel relationshipMapMap)
        {
            if (SelectedRelationships == null)
                return false;

            return SelectedRelationships.Remove(relationshipMapMap);
        }

        private void Relationship_Selected(object sender, EventArgs e)
        {
            AddRelationshipToSelectedRelationships((RelationshipMapModel)sender);
        }

        private void AddRelationshipToSelectedRelationships(RelationshipMapModel relationshipMap)
        {
            if (relationshipMap == null) return;

            if (SelectedRelationships == null)
                SelectedRelationships = new ObservableCollection<RelationshipMapModel>() { relationshipMap };
            else
                SelectedRelationships.Add(relationshipMap);
        }

        private void ObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
                foreach (ObjectMapModel objectMap in e.OldItems)
                {
                    objectMap.IsSelected = false;
                    objectMap.Selected -= ObjectMap_Selected;
                    objectMap.Deselected -= ObjectMap_Deselected;
                    objectMap.WarningCollectionChanged -= ObjectMapOnWarningCollectionChanged;
                    objectMap.IsValidChanged -= ObjectMap_IsValidChanged;
                    objectMap.ScenarioChanged -= ObjectMap_ScenarioChanged;
                    RemoveObjectFromSelectedObjects(objectMap);
                }

            if (e.NewItems?.Count > 0)
                foreach (ObjectMapModel objectMap in e.NewItems)
                {
                    if (objectMap.IsSelected)
                        AddObjectToSelectedObjects(objectMap);
                    else
                        RemoveObjectFromSelectedObjects(objectMap);

                    objectMap.OwnerMap = this;
                    objectMap.Selected -= ObjectMap_Selected;
                    objectMap.Selected += ObjectMap_Selected;

                    objectMap.Deselected -= ObjectMap_Deselected;
                    objectMap.Deselected += ObjectMap_Deselected;

                    objectMap.WarningCollectionChanged -= ObjectMapOnWarningCollectionChanged;
                    objectMap.WarningCollectionChanged += ObjectMapOnWarningCollectionChanged;

                    objectMap.IsValidChanged -= ObjectMap_IsValidChanged;
                    objectMap.IsValidChanged += ObjectMap_IsValidChanged;

                    objectMap.ScenarioChanged -= ObjectMap_ScenarioChanged;
                    objectMap.ScenarioChanged += ObjectMap_ScenarioChanged;
                }

            if (ObjectCollection == null || ObjectCollection.Count == 0)
            {
                SelectedObjects?.Clear();
            }

            SetValidation();
            PrepareWarnings();
            OnObjectCollectionChanged(e);
        }

        private void ObjectMap_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void ObjectMapOnWarningCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PrepareWarnings();
        }

        protected virtual void PrepareWarnings()
        {
            if (WarningCollection.Count != 0)
                WarningCollection.Clear();

            if (ObjectCollection?.Count == 0)
            {
                if (WarningCollection.Count == 0)
                {
                    WarningCollection.Add(new MapWarningModel
                    {
                        Message = Properties.Resources.String_MapModelHasNoObject,
                        WarningType = MapWarningType.NoObjects,
                        RelatedElement = this
                    });
                }
            }
            else
            {
                foreach (var objectMapModel in ObjectCollection)
                {
                    foreach (var warningModel in objectMapModel.WarningCollection)
                    {
                        WarningCollection.Add(warningModel);
                    }
                }
            }
        }

        private void ObjectMap_IsValidChanged(object sender, EventArgs e)
        {
            SetValidation();
        }

        private void ObjectMap_Deselected(object sender, EventArgs e)
        {
            RemoveObjectFromSelectedObjects((ObjectMapModel)sender);
        }

        private bool RemoveObjectFromSelectedObjects(ObjectMapModel objectMap)
        {
            if (SelectedObjects == null)
                return false;

            return SelectedObjects.Remove(objectMap);
        }

        private void ObjectMap_Selected(object sender, EventArgs e)
        {
            AddObjectToSelectedObjects((ObjectMapModel)sender);
        }

        private void AddObjectToSelectedObjects(ObjectMapModel objectMap)
        {
            if (objectMap == null) return;

            if (SelectedObjects == null)
                SelectedObjects = new ObservableCollection<ObjectMapModel>() { objectMap };
            else
                SelectedObjects.Add(objectMap);
        }

        private void OwnerDataSource_FieldCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDataSourceFieldCollectionChanged(e);
        }

        private void SetValidation()
        {
            IsValid = ObjectCollection?.Count > 0 && ObjectCollection.All(o => o.IsValid);
        }

        public void SaveToFile(string filePath)
        {
            Utility.Utility.SerializeToFile(filePath, this);
        }

        public void Copy(MemoryStream memoryStream)
        {
            Utility.Utility.Serialize(memoryStream, this);
            memoryStream.Position = 0;
        }

        /// <summary>
        /// حذف تمام نگاشت
        /// </summary>
        public void ClearAllItems()
        {
            if (ObjectCollection.Count != 0)
                ObjectCollection.Clear();

            if (RelationshipCollection.Count != 0)
                RelationshipCollection.Clear();
        }

        public bool IsEmpty()
        {
            return ObjectCollection.Count == 0;
        }

        #endregion

        #region Events

        public event EventHandler SelectedObjectChanged;
        protected void OnSelectedObjectChanged()
        {
            SelectedObjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SelectedRelationshipChanged;
        protected void OnSelectedRelationshipChanged()
        {
            SelectedRelationshipChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SelectedWarningChanged;
        protected void OnSelectedWarningChanged()
        {
            SelectedWarningChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler DataSourceFieldCollectionChanged;
        protected void OnDataSourceFieldCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            DataSourceFieldCollectionChanged?.Invoke(this, e);
            OnScenarioChanged();
        }

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
            OnScenarioChanged();
        }

        public event NotifyCollectionChangedEventHandler ObjectCollectionChanged;
        protected void OnObjectCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ObjectCollectionChanged?.Invoke(this, e);
            OnScenarioChanged();
        }

        public event NotifyCollectionChangedEventHandler SelectedObjectsChanged;
        protected void OnSelectedObjectsChanged(NotifyCollectionChangedEventArgs e)
        {
            SelectedObjectsChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler RelationshipCollectionChanged;
        protected void OnRelationshipCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            RelationshipCollectionChanged?.Invoke(this, e);
            OnScenarioChanged();
        }

        public event NotifyCollectionChangedEventHandler SelectedRelationshipsChanged;
        protected void OnSelectedRelationshipsChanged(NotifyCollectionChangedEventArgs e)
        {
            SelectedRelationshipsChanged?.Invoke(this, e);
        }

        #endregion
    }
}
