using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public class CSAObject : BaseModel, ICSAElement, ISelectable
    {
        #region Properties

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set
            {
                if (SetValue(ref typeUri, value))
                {
                    OnTypeUriChanged();
                    OnScenarioChanged();
                }
            }
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        string iconPath = null;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        

        CSALink relatedLink = null;

        [XmlIgnore]
        public CSALink RelatedLink
        {
            get => relatedLink;
            set
            {
                if (SetValue(ref relatedLink, value))
                {
                    if (RelatedLink != null && !IsEvent)
                    {
                        RelatedLink.RelatedObject = this;

                        RelatedLink.ScenarioChanged -= RelatedLink_ScenarioChanged;
                        RelatedLink.ScenarioChanged += RelatedLink_ScenarioChanged;
                    }

                    OnScenarioChanged();
                }
            }
        }

        ObservableCollection<CSAProperty> properties = null;
        public ObservableCollection<CSAProperty> Properties
        {
            get => properties;
            set
            {
                ObservableCollection<CSAProperty> oldValue = Properties;
                if (SetValue(ref properties, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= Properties_CollectionChanged;
                    }

                    if (Properties == null)
                    {
                        Properties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Properties.CollectionChanged -= Properties_CollectionChanged;
                        Properties.CollectionChanged += Properties_CollectionChanged;

                        if (oldValue == null)
                        {
                            Properties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Properties));
                        }
                        else
                        {
                            Properties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Properties, oldValue));
                        }
                    }
                }
            }
        }

        CSAProperty selectedProperty = null;

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

        bool isEvent = false;
        public bool IsEvent
        {
            get => isEvent;
            set
            {
                SetValue(ref isEvent, value);
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

        [XmlIgnore]
        public CSAProperty SelectedProperty
        {
            get => selectedProperty;
            set
            {
                PreSelectedPropertyChanged(value);

                SetValue(ref selectedProperty, value);
            }
        }

        bool isSelected = false;

        [XmlIgnore]
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

        CustomSearchAroundModel ownerCSAModel = null;

        [XmlIgnore]
        public CustomSearchAroundModel OwnerCSAModel
        {
            get => ownerCSAModel;
            set => SetValue(ref ownerCSAModel, value);
        }

        #endregion

        #region Methods

        public CSAObject()
        {
            Properties = new ObservableCollection<CSAProperty>();
            Defections = new ObservableCollection<WarningModel>();
        }

        private void PreSelectedPropertyChanged(CSAProperty selectedProperty)
        {
            foreach (CSAProperty cSAProperty in Properties)
            {
                if (!cSAProperty.Equals(selectedProperty))
                    cSAProperty.IsSelected = false;
            }
        }

        private void RelatedLink_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (CSAProperty cSAProperty in e.OldItems)
                {
                    cSAProperty.Selected -= CSAProperty_Selected;
                    cSAProperty.Deselected -= CSAProperty_Deselected;
                    cSAProperty.IsValidChanged -= CSAProperty_IsValidChanged;
                    cSAProperty.ScenarioChanged -= CSAProperty_ScenarioChanged;

                    cSAProperty.OwnerObject = null;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (CSAProperty cSAProperty in e.NewItems)
                {
                    cSAProperty.OwnerObject = this;

                    cSAProperty.Selected -= CSAProperty_Selected;
                    cSAProperty.Selected += CSAProperty_Selected;

                    cSAProperty.Deselected -= CSAProperty_Deselected;
                    cSAProperty.Deselected += CSAProperty_Deselected;

                    cSAProperty.IsValidChanged -= CSAProperty_IsValidChanged;
                    cSAProperty.IsValidChanged += CSAProperty_IsValidChanged;

                    cSAProperty.ScenarioChanged -= CSAProperty_ScenarioChanged;
                    cSAProperty.ScenarioChanged += CSAProperty_ScenarioChanged;
                }
            }

            if (Properties == null || Properties.Count == 0)
                SelectedProperty = null;

            SetValidation();
            OnScenarioChanged();
        }

        private void CSAProperty_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void CSAProperty_IsValidChanged(object sender, EventArgs e)
        {
            SetValidation();
        }

        protected virtual void SetValidation()
        {
            IsValid = Properties == null || Properties.Count == 0 || Properties.All(p => p.IsValid);
        }

        private void CSAProperty_Deselected(object sender, EventArgs e)
        {
            SelectedProperty = Properties?.FirstOrDefault(p => p.IsSelected);
        }

        private void CSAProperty_Selected(object sender, EventArgs e)
        {
            SelectedProperty = (CSAProperty)sender;
        }

        private void Defections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDefectionsChanged(e);
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

        public event EventHandler TypeUriChanged;
        protected void OnTypeUriChanged()
        {
            TypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler DefectionsChanged;
        protected void OnDefectionsChanged(NotifyCollectionChangedEventArgs e)
        {
            DefectionsChanged?.Invoke(this, e);
        }

        #endregion
    }
}
