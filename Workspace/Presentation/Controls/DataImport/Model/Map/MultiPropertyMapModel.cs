using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [XmlInclude(typeof(GeoTimePropertyMapModel))]
    [XmlInclude(typeof(GeoPointPropertyMapModel))]
    [Serializable]
    public class MultiPropertyMapModel : PropertyMapModel
    {
        #region Properties

        ObservableCollection<PropertyMapModel> innerProperties = new ObservableCollection<PropertyMapModel>();
        public ObservableCollection<PropertyMapModel> InnerProperties
        {
            get => innerProperties;
            set
            {
                ObservableCollection<PropertyMapModel> oldVal = InnerProperties;
                if (SetValue(ref innerProperties, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= InnerProperties_CollectionChanged;
                    }
                    if (InnerProperties == null)
                    {
                        InnerProperties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        InnerProperties.CollectionChanged -= InnerProperties_CollectionChanged;
                        InnerProperties.CollectionChanged += InnerProperties_CollectionChanged;

                        if (oldVal == null)
                        {
                            InnerProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, InnerProperties));
                        }
                        else
                        {
                            InnerProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, InnerProperties, oldVal));
                        }
                    }
                }
            }
        }

        PropertyMapModel selectedInnerProperty;
        [XmlIgnore]
        public PropertyMapModel SelectedInnerProperty
        {
            get => selectedInnerProperty;
            set
            {
                PreSelectedInnerPropertyChanged(value);

                if (SetValue(ref selectedInnerProperty, value))
                {
                    OnSelectedInnerPropertyChanged();
                }
            }
        }

        ObservableCollection<SinglePropertyMapModel> leafProperties = new ObservableCollection<SinglePropertyMapModel>();
        [XmlIgnore]
        public ObservableCollection<SinglePropertyMapModel> LeafProperties
        {
            get => leafProperties;
            set
            {
                ObservableCollection<SinglePropertyMapModel> oldVal = LeafProperties;
                if (SetValue(ref leafProperties, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= LeafProperties_CollectionChanged;
                    }
                    if (LeafProperties == null)
                    {
                        LeafProperties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        LeafProperties.CollectionChanged -= LeafProperties_CollectionChanged;
                        LeafProperties.CollectionChanged += LeafProperties_CollectionChanged;

                        if (oldVal == null)
                        {
                            LeafProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, LeafProperties));
                        }
                        else
                        {
                            LeafProperties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, LeafProperties, oldVal));
                        }
                    }
                }
            }
        }

        SinglePropertyMapModel selectedLeafProperty;
        [XmlIgnore]
        public SinglePropertyMapModel SelectedLeafProperty
        {
            get => selectedLeafProperty;
            set
            {
                PreSelectedLeafPropertyChanged(value);

                if (SetValue(ref selectedLeafProperty, value))
                {
                    OnSelectedLeafPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        public MultiPropertyMapModel()
        {
            IsResolvable = false;
            InnerProperties.CollectionChanged += InnerProperties_CollectionChanged;
            LeafProperties.CollectionChanged += LeafProperties_CollectionChanged;
        }

        private void InnerProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PropertyMapModel InnerProperty in e.OldItems)
                {
                    InnerProperty.Selected -= InnerProperty_Selected;
                    InnerProperty.SampleValueChanged -= InnerProperty_SampleValueChanged;
                    InnerProperty.WarningCollectionChanged -= InnerPropertyOnWarningCollectionChanged;
                    InnerProperty.IsValidChanged -= InnerProperty_IsValidChanged;
                    InnerProperty.ScenarioChanged -= InnerProperty_ScenarioChanged;
                    InnerProperty.SampleValueValidationStatusChanged -= InnerProperty_SampleValueValidationStatusChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (PropertyMapModel InnerProperty in e.NewItems)
                {
                    InnerProperty.ParentProperty = this;
                    InnerProperty.OwnerObject = OwnerObject;
                    InnerProperty.IsResolvable = false;

                    InnerProperty.Selected -= InnerProperty_Selected;
                    InnerProperty.Selected += InnerProperty_Selected;

                    InnerProperty.SampleValueChanged -= InnerProperty_SampleValueChanged;
                    InnerProperty.SampleValueChanged += InnerProperty_SampleValueChanged;

                    InnerProperty.WarningCollectionChanged -= InnerPropertyOnWarningCollectionChanged;
                    InnerProperty.WarningCollectionChanged += InnerPropertyOnWarningCollectionChanged;

                    InnerProperty.IsValidChanged -= InnerProperty_IsValidChanged;
                    InnerProperty.IsValidChanged += InnerProperty_IsValidChanged;

                    InnerProperty.ScenarioChanged -= InnerProperty_ScenarioChanged;
                    InnerProperty.ScenarioChanged += InnerProperty_ScenarioChanged;

                    InnerProperty.SampleValueValidationStatusChanged -= InnerProperty_SampleValueValidationStatusChanged;
                    InnerProperty.SampleValueValidationStatusChanged += InnerProperty_SampleValueValidationStatusChanged;
                }
            }

            PrepareWarnings();
            LeafProperties = new ObservableCollection<SinglePropertyMapModel>(GetLeafs(this));
            SetValidation();
            SetSampleValue();
            SetValidationForSampleValue();
            AfterInnerPropertiesChanged();
        }

        private void LeafProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PropertyMapModel leafProperty in e.OldItems)
                {
                    leafProperty.Selected -= LeafProperty_Selected;
                }
            }

            if (e.NewItems != null)
            {
                foreach (PropertyMapModel leafProperty in e.NewItems)
                {
                    leafProperty.Selected -= LeafProperty_Selected;
                    leafProperty.Selected += LeafProperty_Selected;
                }
            }
        }

        protected virtual void AfterInnerPropertiesChanged()
        {

        }

        private void InnerProperty_SampleValueValidationStatusChanged(object sender, EventArgs e)
        {
            SetValidationForSampleValue();
        }

        private void InnerProperty_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void InnerPropertyOnWarningCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PrepareWarnings();
        }

        private void InnerProperty_IsValidChanged(object sender, EventArgs e)
        {
            SetValidation();
        }

        private void InnerProperty_SampleValueChanged(object sender, EventArgs e)
        {
            SetSampleValue();
        }

        private void InnerProperty_Selected(object sender, EventArgs e)
        {
            SelectedInnerProperty = (PropertyMapModel)sender;
        }

        private void LeafProperty_Selected(object sender, EventArgs e)
        {
            SelectedLeafProperty = (SinglePropertyMapModel)sender;
        }

        private void PreSelectedInnerPropertyChanged(PropertyMapModel selectedProperty)
        {
            foreach (PropertyMapModel innerProperty in InnerProperties)
            {
                if (!innerProperty.Equals(selectedProperty))
                {
                    innerProperty.IsSelected = false;
                }
            }
        }

        private void PreSelectedLeafPropertyChanged(SinglePropertyMapModel selectedProperty)
        {
            foreach (SinglePropertyMapModel leafProperty in LeafProperties)
            {
                if (!leafProperty.Equals(selectedProperty))
                {
                    leafProperty.IsSelected = false;
                }
            }
        }

        protected override void PrepareWarnings()
        {
            if (WarningCollection.Count != 0)
                WarningCollection.Clear();

            foreach (PropertyMapModel innerProperty in InnerProperties)
            {
                foreach (MapWarningModel warningModel in innerProperty.WarningCollection)
                {
                    WarningCollection.Add(warningModel);
                }
            }
        }

        protected override void SetOwnerObjectForInnerProperties()
        {
            foreach (PropertyMapModel innerProperty in InnerProperties)
            {
                innerProperty.OwnerObject = OwnerObject;
            }
        }

        protected override void SetSampleValue()
        {
            if (InnerProperties?.Count > 0)
                SampleValue = ToString();
        }

        protected override void SetValidation()
        {
            IsValid = InnerProperties?.Count > 0 && InnerProperties.All(ip => ip.IsValid);
        }

        protected override void AfterSelected()
        {
            base.AfterSelected();
            if (SelectedInnerProperty == null && InnerProperties?.Count > 0)
                InnerProperties.FirstOrDefault().IsSelected = true;
        }

        public override IEnumerable<ValueMapModel> GetAllValues()
        {
            return InnerProperties?.SelectMany(ip => ip?.GetAllValues());
        }

        public override string ToString()
        {
            List<SinglePropertyMapModel> leafs = GetLeafs(this);
            List<string> lineValues = new List<string>();

            foreach (SinglePropertyMapModel leaf in leafs)
            {
                lineValues.Add(leaf.GetLeafTitle() + leaf.Title + " : " + leaf.SampleValue);
            }
            return string.Join(Environment.NewLine, lineValues);
        }

        public static List<SinglePropertyMapModel> GetLeafs(MultiPropertyMapModel multiProperty)
        {
            if (multiProperty?.InnerProperties == null)
                return null;

            List<SinglePropertyMapModel> leafs = null;
            GetLeafs(multiProperty.InnerProperties, ref leafs);
            return leafs;
        }

        private static void GetLeafs(ObservableCollection<PropertyMapModel> innerProperties, ref List<SinglePropertyMapModel> leafs)
        {
            if (innerProperties != null)
            {
                if (leafs == null)
                    leafs = new List<SinglePropertyMapModel>();

                foreach (PropertyMapModel innerProp in innerProperties)
                {
                    if (innerProp is MultiPropertyMapModel multiProperty)
                        GetLeafs(multiProperty.InnerProperties, ref leafs);
                    else if (innerProp is SinglePropertyMapModel singleProperty)
                        leafs.Add(singleProperty);
                }
            }
        }

        #endregion

        #region Events  

        public event EventHandler SelectedInnerPropertyChanged;
        protected void OnSelectedInnerPropertyChanged()
        {
            SelectedInnerPropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SelectedLeafPropertyChanged;
        protected void OnSelectedLeafPropertyChanged()
        {
            SelectedLeafPropertyChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
