using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public class CustomSearchAroundModel : BaseModel, ICSAElement
    {
        #region Properties

        ObservableCollection<CSALink> linkCollection = new ObservableCollection<CSALink>();

        public ObservableCollection<CSALink> LinkCollection
        {
            get => linkCollection;
            set
            {
                SetValue(ref linkCollection, value);
            }
        }

        ObservableCollection<CSAObject> objectCollection = null;

        [XmlIgnore]
        public ObservableCollection<CSAObject> ObjectCollection
        {
            get => objectCollection;
            set
            {
                ObservableCollection<CSAObject> oldValue = ObjectCollection;
                if (SetValue(ref objectCollection, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= ObjectCollection_CollectionChanged;
                    }

                    if (ObjectCollection == null)
                    {
                        ObjectCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
                        ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;

                        if (oldValue == null)
                        {
                            ObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ObjectCollection));
                        }
                        else
                        {
                            ObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ObjectCollection, oldValue));
                        }
                    }
                }
            }
        }

        bool isValid = true;
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

        CSAObject selectedObject = null;

        [XmlIgnore]
        public CSAObject SelectedObject
        {
            get => selectedObject;
            set
            {
                PreSelectedObjectChanged(value);

                SetValue(ref selectedObject, value);
            }
        }

        ObservableCollection<WarningModel> defections = null;

        [XmlIgnore]
        public ObservableCollection<WarningModel> Defections
        {
            get => defections;
            set
            {
                ObservableCollection<WarningModel> oldValue = null;
                if (SetValue(ref defections, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= Defections_CollectionChanged;
                    }
                    if (Defections == null)
                    {
                        Defections_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Defections.CollectionChanged -= Defections_CollectionChanged;
                        Defections.CollectionChanged += Defections_CollectionChanged;

                        if (oldValue == null)
                        {
                            Defections_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Defections));
                        }
                        else
                        {
                            Defections_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Defections, oldValue));
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        public CustomSearchAroundModel()
        {
            Defections = new ObservableCollection<WarningModel>();
            ObjectCollection = new ObservableCollection<CSAObject>();
        }

        private void PreSelectedObjectChanged(CSAObject selectedObject)
        {
            foreach (CSAObject cSAObject in ObjectCollection)
            {
                if (!cSAObject.Equals(selectedObject))
                    cSAObject.IsSelected = false;
            }
        }

        private void ObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (CSAObject cSAObject in e.OldItems)
                {
                    cSAObject.Selected -= CSAObject_Selected;
                    cSAObject.Deselected -= CSAObject_Deselected;
                    cSAObject.IsValidChanged -= CSAObject_IsValidChanged;
                    cSAObject.ScenarioChanged -= CSAObject_ScenarioChanged;
                    cSAObject.OwnerCSAModel = null;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (CSAObject cSAObject in e.NewItems)
                {
                    cSAObject.OwnerCSAModel = this;

                    cSAObject.Selected -= CSAObject_Selected;
                    cSAObject.Selected += CSAObject_Selected;

                    cSAObject.Deselected -= CSAObject_Deselected;
                    cSAObject.Deselected += CSAObject_Deselected;

                    cSAObject.IsValidChanged -= CSAObject_IsValidChanged;
                    cSAObject.IsValidChanged += CSAObject_IsValidChanged;

                    cSAObject.ScenarioChanged -= CSAObject_ScenarioChanged;
                    cSAObject.ScenarioChanged += CSAObject_ScenarioChanged;
                }
            }

            if (ObjectCollection == null || ObjectCollection.Count == 0)
                SelectedObject = null;

            SetValidation();
            PrepareDefections();
            OnObjectCollectionChanged(e);
            OnScenarioChanged();
        }

        private void CSAObject_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void CSAObject_IsValidChanged(object sender, EventArgs e)
        {
            SetValidation();
            PrepareDefections();
        }

        protected virtual void SetValidation()
        {
            IsValid = ObjectCollection?.Count > 0 && ObjectCollection.All(o => o.IsValid);
        }

        protected virtual void PrepareDefections()
        {
            Defections = new ObservableCollection<WarningModel>();
            if (ObjectCollection == null || ObjectCollection.Count == 0)
            {
                Defections.Add(new WarningModel()
                {
                    Icon = MaterialDesignThemes.Wpf.PackIconKind.Dangerous,
                    WarningType = WarningType.NoObjects,
                    RelatedElement = this,
                    Message = Properties.Resources.String_MapModelHasNoObject
                });
            }
        }

        private void CSAObject_Deselected(object sender, EventArgs e)
        {
            SelectedObject = ObjectCollection?.FirstOrDefault(o => o.IsSelected);
        }

        private void CSAObject_Selected(object sender, EventArgs e)
        {
            SelectedObject = (CSAObject)sender;
        }

        private void Defections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDefectionsChanged(e);
        }

        #endregion

        #region Events

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ScenarioChanged;
        protected void OnScenarioChanged()
        {
            ScenarioChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler DefectionsChanged;
        protected void OnDefectionsChanged(NotifyCollectionChangedEventArgs e)
        {
            DefectionsChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ObjectCollectionChanged;
        protected void OnObjectCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ObjectCollectionChanged?.Invoke(this, e);
        }

        #endregion
    }
}
