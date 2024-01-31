using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [XmlInclude(typeof(DateTimePropertyMapModel))]
    [Serializable]
    public class SinglePropertyMapModel : PropertyMapModel
    {
        #region Properties

        private ObservableCollection<ValueMapModel> valueCollection = new ObservableCollection<ValueMapModel>();
        public ObservableCollection<ValueMapModel> ValueCollection
        {
            get => valueCollection;
            set
            {
                ObservableCollection<ValueMapModel> oldVal = ValueCollection;
                if (SetValue(ref valueCollection, value))
                {
                    if(oldVal != null)
                        oldVal.CollectionChanged -= ValueCollection_CollectionChanged;

                    if (ValueCollection == null)
                    {
                        ValueCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ValueCollection.CollectionChanged -= ValueCollection_CollectionChanged;
                        ValueCollection.CollectionChanged += ValueCollection_CollectionChanged;

                        if(oldVal == null)
                        {
                            ValueCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ValueCollection));
                        }
                        else
                        {
                            ValueCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ValueCollection, oldVal));
                        }
                    }
                }
            }
        }

        ValueMapModel selectedValue;
        [XmlIgnore]
        public ValueMapModel SelectedValue
        {
            get => selectedValue;
            set
            {
                PreSelectedValueChanged(value);

                SetValue(ref selectedValue, value);
            }
        }

        #endregion

        #region Methods

        public SinglePropertyMapModel()
        {
            ValueCollection = new ObservableCollection<ValueMapModel>();
        }

        private void ValueCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
                foreach (ValueMapModel value in e.NewItems)
                {
                    value.OwnerProperty = this;

                    value.Selected -= Value_Selected;
                    value.Selected += Value_Selected;

                    value.SampleValueChanged -= Value_SampleValueChanged;
                    value.SampleValueChanged += Value_SampleValueChanged;

                    value.ScenarioChanged -= Value_ScenarioChanged;
                    value.ScenarioChanged += Value_ScenarioChanged;
                }

            if (e.OldItems?.Count > 0)
                foreach (ValueMapModel value in e.OldItems)
                {
                    value.Selected -= Value_Selected;
                    value.SampleValueChanged -= Value_SampleValueChanged;
                    value.ScenarioChanged -= Value_ScenarioChanged;
                }

            PrepareWarnings();
            SetValidation();
            SetSampleValue();
            OnScenarioChanged();
        }

        private void Value_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        private void Value_SampleValueChanged(object sender, EventArgs e)
        {
            ValueMapModel valueMapModel = (ValueMapModel)sender;
            if (valueMapModel.Field is GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField.ConstFieldModel)
                OnScenarioChanged();

            SetSampleValue();
        }

        protected override void SetOwnerObjectForInnerProperties()
        {

        }

        protected override void SetSampleValue()
        {
            string tempValue = string.Empty;

            if (ValueCollection?.Count > 0)
                foreach (ValueMapModel val in ValueCollection)
                {
                    tempValue += val.SampleValue;
                }

            SampleValue = tempValue;
        }

        protected override void SetValidation()
        {
            IsValid = ValueCollection?.Count > 0;
        }

        protected override void PrepareWarnings()
        {
            if (string.IsNullOrEmpty(Title) || OwnerObject == null)
                return;

            if (ValueCollection?.Count == 0)
            {
                if (WarningCollection.Count == 0)
                {
                    string warningMessage = ParentProperty == null ?
                        string.Format(Properties.Resources.String_MapPropertyHasNoValue, Title, OwnerObject.Title) :
                        string.Format(Properties.Resources.String_SubMapPropertyHasNoValue, Title, GetAncestorsTitle(), OwnerObject.Title);

                    WarningCollection.Add(new MapWarningModel
                    {
                        Message = warningMessage,
                        WarningType = MapWarningType.NotSetValue,
                        RelatedElement = this
                    });
                }
            }
            else
            {
                if (WarningCollection.Count != 0)
                    WarningCollection.Clear();
            }
        }

        private void Value_Selected(object sender, EventArgs e)
        {
            SelectedValue = (ValueMapModel)sender;
        }

        private void PreSelectedValueChanged(ValueMapModel selectedValueMapModel)
        {
            foreach (ValueMapModel value in ValueCollection)
            {
                value.IsSelected = value.Equals(selectedValueMapModel);
            }
        }

        public override IEnumerable<ValueMapModel> GetAllValues()
        {
            return ValueCollection;
        }

        public string GetLeafTitle()
        {
            string title = string.Empty;
            List<string> ancestorsTitle = GetAncestors().Select(anc => anc.Title).ToList();
            ancestorsTitle.RemoveAt(0);

            foreach (string ancTitle in ancestorsTitle)
            {
                title += ancTitle + "->";
            }

            return title;
        }

        #endregion
    }
}
