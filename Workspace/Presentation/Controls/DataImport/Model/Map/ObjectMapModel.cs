using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [XmlInclude(typeof(DocumentMapModel))]
    [Serializable]
    public class ObjectMapModel : MapElement
    {
        #region Properties

        string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set
            {
                if (SetValue(ref typeUri, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (SetValue(ref title, value))
                {
                    PrepareWarnings();
                }
            }
        }

        string iconPath = null;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        bool isResolvable = true;
        public bool IsResolvable
        {
            get => isResolvable;
            set => SetValue(ref isResolvable, value);
        }

        bool displayNameChangeable = true;
        public bool DisplayNameChangeable
        {
            get => displayNameChangeable;
            set => SetValue(ref displayNameChangeable, value);
        }

        ResolutionPolicy resolutionPolicy = ResolutionPolicy.SetNew;
        public ResolutionPolicy ResolutionPolicy
        {
            get => resolutionPolicy;
            set
            {
                if (SetValue(ref resolutionPolicy, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        bool resolutionPolicyChangeable = false;
        [XmlIgnore]
        public bool ResolutionPolicyChangeable
        {
            get => resolutionPolicyChangeable;
            set => SetValue(ref resolutionPolicyChangeable, value);
        }

        ObservableCollection<PropertyMapModel> properties = new ObservableCollection<PropertyMapModel>();
        public ObservableCollection<PropertyMapModel> Properties
        {
            get => properties;
            set
            {
                ObservableCollection<PropertyMapModel> oldVal = Properties;
                if (SetValue(ref properties, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= Properties_CollectionChanged;
                    }
                    if (Properties == null)
                    {
                        Properties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Properties.CollectionChanged -= Properties_CollectionChanged;
                        Properties.CollectionChanged += Properties_CollectionChanged;

                        if (oldVal == null)
                        {
                            Properties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Properties));
                        }
                        else
                        {
                            Properties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Properties, oldVal));
                        }
                    }
                }
            }
        }

        PropertyMapModel displayNameProperty = null;
        [XmlIgnore]
        public PropertyMapModel DisplayNameProperty
        {
            get => displayNameProperty;
            set
            {
                PreDisplayNamePropertyChanged(value);

                if (SetValue(ref displayNameProperty, value))
                {
                    SetValidation();
                    PrepareWarnings();
                    OnScenarioChanged();
                }
            }
        }

        PropertyMapModel selectedProperty = null;
        [XmlIgnore]
        public PropertyMapModel SelectedProperty
        {
            get => selectedProperty;
            set
            {
                PreSelectedPropertyChanged(value);

                SetValue(ref selectedProperty, value);
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

        ObservableCollection<string> usedTypeUriProperties = new ObservableCollection<string>();
        [XmlIgnore]
        public ObservableCollection<string> UsedTypeUriProperties
        {
            get => usedTypeUriProperties;
            set
            {
                ObservableCollection<string> oldVal = UsedTypeUriProperties;
                if (SetValue(ref usedTypeUriProperties, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= UsedTypeUriProperties_CollectionChanged;
                    }
                    if (UsedTypeUriProperties == null)
                    {
                        UsedTypeUriProperties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        UsedTypeUriProperties.CollectionChanged -= UsedTypeUriProperties_CollectionChanged;
                        UsedTypeUriProperties.CollectionChanged += UsedTypeUriProperties_CollectionChanged;

                        if (oldVal == null)
                        {
                            UsedTypeUriProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, UsedTypeUriProperties));
                        }
                        else
                        {
                            UsedTypeUriProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, UsedTypeUriProperties, oldVal));
                        }
                    }
                }
            }
        }

        MapModel ownerMap = null;
        [XmlIgnore]
        public MapModel OwnerMap
        {
            get => ownerMap;
            set
            {
                if (SetValue(ref ownerMap, value))
                {
                    AfterOwnerMapChanged();
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        public ObjectMapModel()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new ObservableCollection<PropertyMapModel>();
        }

        private void UsedTypeUriProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        protected virtual void AfterOwnerMapChanged()
        {

        }

        private void PreDisplayNamePropertyChanged(PropertyMapModel displayNameProperty)
        {
            foreach (PropertyMapModel property in Properties)
            {
                if (!property.Equals(displayNameProperty))
                    property.IsDisplayName = false;
            }
        }

        private void PreSelectedPropertyChanged(PropertyMapModel selectedProperty)
        {
            foreach (PropertyMapModel property in Properties)
            {
                if (!property.Equals(selectedProperty))
                    property.IsSelected = false;
            }
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PropertyMapModel property in e.NewItems)
                {
                    property.OwnerObject = this;

                    property.Selected -= Property_Selected;
                    property.Selected += Property_Selected;

                    property.Deselected -= Property_Deselected;
                    property.Deselected += Property_Deselected;

                    property.WarningCollectionChanged -= PropertyWarningCollectionChanged;
                    property.WarningCollectionChanged += PropertyWarningCollectionChanged;

                    property.IsDisplayNameChanged -= Property_IsDisplayNameChanged;
                    property.IsDisplayNameChanged += Property_IsDisplayNameChanged;

                    property.IsValidChanged -= Property_IsValidChanged;
                    property.IsValidChanged += Property_IsValidChanged;

                    property.ScenarioChanged -= Property_ScenarioChanged;
                    property.ScenarioChanged += Property_ScenarioChanged;

                    property.HasResolutionChanged -= Property_HasResolutionChanged;
                    property.HasResolutionChanged += Property_HasResolutionChanged;

                    property.TypeUriChanged -= Property_TypeUriChanged;
                    property.TypeUriChanged += Property_TypeUriChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (PropertyMapModel property in e.OldItems)
                {
                    property.IsSelected = false;
                    property.IsDisplayName = false;

                    property.Selected -= Property_Selected;
                    property.Deselected -= Property_Deselected;
                    property.WarningCollectionChanged -= PropertyWarningCollectionChanged;
                    property.IsDisplayNameChanged -= Property_IsDisplayNameChanged;
                    property.IsValidChanged -= Property_IsValidChanged;
                    property.ScenarioChanged -= Property_ScenarioChanged;
                    property.HasResolutionChanged -= Property_HasResolutionChanged;
                    property.TypeUriChanged -= Property_TypeUriChanged;
                }
            }

            if (Properties == null || Properties.Count == 0)
            {
                SelectedProperty = null;
                DisplayNameProperty = null;
            }

            SetUsedTypeUriProperties();
            SetResolutionPolicyChangeable();
            PrepareWarnings();
            SetValidation();
            OnPropertiesChanged(e);
        }

        private void Property_TypeUriChanged(object sender, EventArgs e)
        {
            SetUsedTypeUriProperties();
            PrepareWarnings();
            SetValidation();
        }

        private void SetUsedTypeUriProperties()
        {
            if (Properties == null || Properties.Count == 0)
                UsedTypeUriProperties = new ObservableCollection<string>();
            else
                UsedTypeUriProperties = new ObservableCollection<string>(Properties.Select(p => p.TypeUri).Distinct());
        }

        private void Property_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void Property_HasResolutionChanged(object sender, EventArgs e)
        {
            SetResolutionPolicyChangeable();
            PrepareWarnings();
            SetValidation();
        }

        protected void SetResolutionPolicyChangeable()
        {
            ResolutionPolicyChangeable = Properties?.Count > 0 && Properties.Any(p => p != null && p.HasResolution);
        }

        public virtual void PrepareWarnings()
        {
            if (WarningCollection.Count != 0)
                WarningCollection.Clear();

            if (DisplayNameProperty == null)
            {
                WarningCollection.Add(new MapWarningModel
                {
                    Message = string.Format(Presentation.Properties.Resources.String_MapObjectHasNoDisplayName, Title),
                    WarningType = MapWarningType.NotSetDisplayName,
                    RelatedElement = this
                });
            }

            if (Properties.Count == 0)
            {
                WarningCollection.Add(new MapWarningModel
                {
                    Message = string.Format(Presentation.Properties.Resources.String_MapObjectHasNoAnyProperty, Title),
                    WarningType = MapWarningType.NoProperties,
                    RelatedElement = this
                });
            }

            List<IGrouping<string, PropertyMapModel>> groupByTypeUri = Properties.GroupBy(p => p.TypeUri).ToList();
            foreach (var group in groupByTypeUri)
            {
                int propertiesCount = group.Count();
                if (propertiesCount > 1)
                {
                    WarningCollection.Add(new MapWarningModel
                    {
                        Message = string.Format(Presentation.Properties.Resources.Object_Has_Number_Properties_Of_Property, Title, propertiesCount, group.FirstOrDefault()?.Title),
                        WarningType = MapWarningType.HasDuplicateProperties,
                        RelatedElement = this
                    });
                }
            }

            foreach (PropertyMapModel property in Properties)
            {
                foreach (MapWarningModel warningModel in property.WarningCollection)
                {
                    WarningCollection.Add(warningModel);
                }
            }
        }

        private void PropertyWarningCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PrepareWarnings();
        }

        private void Property_IsDisplayNameChanged(object sender, EventArgs e)
        {
            PropertyMapModel propertyMapModel = (PropertyMapModel)sender;
            if (propertyMapModel.IsDisplayName)
            {
                DisplayNameProperty = propertyMapModel;
            }
            else
            {
                DisplayNameProperty = Properties.FirstOrDefault(p => p.IsDisplayName);
            }
        }

        private void Property_IsValidChanged(object sender, EventArgs e)
        {
            SetValidation();
        }

        private void Property_Selected(object sender, EventArgs e)
        {
            SelectedProperty = (PropertyMapModel)sender;
        }

        private void Property_Deselected(object sender, EventArgs e)
        {
            SelectedProperty = Properties.FirstOrDefault(p => p.IsSelected);
        }

        public bool Equals(ObjectMapModel otherObject)
        {
            return Id.Equals(otherObject.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        protected void SetValidation()
        {
            IsValid = DisplayNameProperty != null && Properties?.Count > 0 && Properties.All(p => p.IsValid) &&
                !Properties.GroupBy(p => p.TypeUri).Any(g => g.Count() > 1);
        }

        /// <summary>
        /// این متد لیستی از تمامی ویژگی های یک شی را بر می گرداند.
        /// مزیت استفاده از آن برای اشیا از نوع سند است که ویژگی Path مربوط به سند را نیز بر می‌گرداند.
        /// </summary>
        /// <returns>Properties + Path for documents</returns>
        public virtual IEnumerable<PropertyMapModel> GetAllProperties()
        {
            return Properties;
        }

        #endregion

        #region Event

        public event NotifyCollectionChangedEventHandler PropertiesChanged;
        protected void OnPropertiesChanged(NotifyCollectionChangedEventArgs e)
        {
            PropertiesChanged?.Invoke(this, e);
            OnScenarioChanged();
        }

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
