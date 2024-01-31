using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.PropertiesValidation.Geo;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    /// <summary>
    /// Interaction logic for PropertyFilterSearchControl.xaml
    /// </summary>
    public partial class PropertyFilterSearchControl : BaseFilterSearchControl
    {
        #region Dependencies

        public string PropertyTypeUri
        {
            get { return (string)GetValue(PropertyTypeUriProperty); }
            set { SetValue(PropertyTypeUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyTypeUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyTypeUriProperty =
            DependencyProperty.Register(nameof(PropertyTypeUri), typeof(string), typeof(PropertyFilterSearchControl),
                new PropertyMetadata(string.Empty, OnSetPropertyTypeUriChanged));

        private static void OnSetPropertyTypeUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyFilterSearchControl)d).OnSetPropertyTypeUriChanged(e);
        }

        private void OnSetPropertyTypeUriChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            SetPropertyValueCriteria();
            OnPropertyTypeUriChanged();
        }

        public PropertyCriteriaOperatorValuePair OperatorValuePair
        {
            get { return (PropertyCriteriaOperatorValuePair)GetValue(OperatorValuePairProperty); }
            set { SetValue(OperatorValuePairProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OperatorValuePair.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OperatorValuePairProperty =
            DependencyProperty.Register(nameof(OperatorValuePair), typeof(PropertyCriteriaOperatorValuePair), typeof(PropertyFilterSearchControl),
                new PropertyMetadata(new EmptyPropertyCriteriaOperatorValuePair(), OnSetOperatorValuePairChanged));

        private static void OnSetOperatorValuePairChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyFilterSearchControl)d).OnSetOperatorValuePairChanged(e);
        }

        private void OnSetOperatorValuePairChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            SetPropertyValueCriteria();
            OnOperatorValuePairChanged();
        }

        protected BaseDataTypes SelectedPropertyDataType
        {
            get { return (BaseDataTypes)GetValue(SelectedPropertyDataTypeProperty); }
            set { SetValue(SelectedPropertyDataTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedPropertyDataType.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty SelectedPropertyDataTypeProperty =
            DependencyProperty.Register(nameof(SelectedPropertyDataType), typeof(BaseDataTypes), typeof(PropertyFilterSearchControl),
                new PropertyMetadata(BaseDataTypes.None, OnSetSelectedPropertyDataTypeChanged));

        private static void OnSetSelectedPropertyDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyFilterSearchControl)d).OnSetSelectedPropertyDataTypeChanged(e);
        }

        private void OnSetSelectedPropertyDataTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            PropertyValueTemplatesControl.PrepareControl(SelectedPropertyDataType);
            SetOperatorValuePair();
        }


        public RelationalOperator RelationalOperator
        {
            get { return (RelationalOperator)GetValue(RelationalOperatorProperty); }
            set { SetValue(RelationalOperatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RelationalOperator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RelationalOperatorProperty =
            DependencyProperty.Register(nameof(RelationalOperator), typeof(RelationalOperator), typeof(PropertyFilterSearchControl),
                new PropertyMetadata(RelationalOperator.Equals, OnSetRelationalOperatorChanged));

        private static void OnSetRelationalOperatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyFilterSearchControl)d).OnSetRelationalOperatorChanged(e);
        }

        private void OnSetRelationalOperatorChanged(DependencyPropertyChangedEventArgs e)
        {
            SetPropertyValueCriteria();
            OnRelationalOperatorChanged();
        }

        public string CriteriaValue
        {
            get { return (string)GetValue(CriteriaValueProperty); }
            set { SetValue(CriteriaValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriteriaValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriteriaValueProperty =
            DependencyProperty.Register(nameof(CriteriaValue), typeof(string), typeof(PropertyFilterSearchControl),
                new PropertyMetadata(string.Empty, OnSetCriteriaValueChanged));

        private static void OnSetCriteriaValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyFilterSearchControl)d).OnSetCriteriaValueChanged(e);
        }

        private void OnSetCriteriaValueChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            SetPropertyValueCriteria();
            OnCriteriaValueChanged();
        }

        #endregion

        #region Events

        public event EventHandler PropertyTypeUriChanged;
        private void OnPropertyTypeUriChanged()
        {
            PropertyTypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OperatorValuePairChanged;
        private void OnOperatorValuePairChanged()
        {
            OperatorValuePairChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RelationalOperatorChanged;
        protected void OnRelationalOperatorChanged()
        {
            RelationalOperatorChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CriteriaValueChanged;
        protected void OnCriteriaValueChanged()
        {
            CriteriaValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Variables

        private PropertyValueCriteria propertyValueCriteria = null;

        #endregion

        #region Methodes

        public PropertyFilterSearchControl()
        {
            InitializeComponent();
            Init();
        }

        protected override void SetValidationStatus()
        {
            if (!(string.IsNullOrEmpty(PropertyTypeUri) && string.IsNullOrWhiteSpace(OperatorValuePair.GetInvarientValue())))
            {
                ValidationMessage = PropertyValueTemplatesControl.ValidationMessage;
                ValidationStatus = PropertyValueTemplatesControl.ValidationStatus;
            }
            else
            {
                ValidationMessage = Properties.Resources.Select_a_property_type_from_list_;
                ValidationStatus = ValidationStatus.Invalid;
            }
        }

        protected override void AfterSetCriteriaBase()
        {
            propertyValueCriteria = CriteriaBase as PropertyValueCriteria;
        }

        private void Init()
        {
            DataContext = this;
            List<string> ontologyObjectTypes = GetOntologyObjectTypes();

            propertyTypePickerControl.ExceptDataTypeCollection.Clear();
            propertyTypePickerControl.ExceptDataTypeCollection.Add(BaseDataTypes.GeoTime);

            if (CriteriaBase == null)
            {
                CriteriaBase = new PropertyValueCriteria();
            }
        }

        private List<string> GetOntologyObjectTypes()
        {
            List<string> ontologyObjectTypes = new List<string>();
            ObservableCollection<OntologyNode> objectTypesHierarchy = OntologyProvider.GetOntology().GetOntologyObjectsHierarchy();

            foreach (OntologyNode objectTypeNode in objectTypesHierarchy)
            {
                if (!ontologyObjectTypes.Contains(objectTypeNode.TypeUri))
                {
                    ontologyObjectTypes.Add(objectTypeNode.TypeUri);
                }

                AppendNodeInTree(objectTypeNode, ontologyObjectTypes);
            }

            return ontologyObjectTypes;
        }

        private void AppendNodeInTree(OntologyNode ontologyNodeToAppend, List<string> ontologyObjectTypes)
        {
            if (!ontologyNodeToAppend.IsLeaf)
            {
                foreach (OntologyNode item in ontologyNodeToAppend.Children)
                {
                    if (!ontologyObjectTypes.Contains(item.TypeUri))
                    {
                        ontologyObjectTypes.Add(item.TypeUri);
                    }

                    AppendNodeInTree(item, ontologyObjectTypes);
                }
            }
        }

        private void SetOperatorValuePair()
        {
            switch (SelectedPropertyDataType)
            {
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    OperatorValuePair = new LongPropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.Boolean:
                    OperatorValuePair = new BooleanPropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.DateTime:
                    OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.String:
                    OperatorValuePair = new StringPropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.Double:
                    OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.GeoPoint:
                    OperatorValuePair = new GeoPointPropertyCriteriaOperatorValuePair();
                    break;
                case BaseDataTypes.HdfsURI:
                case BaseDataTypes.None:
                case BaseDataTypes.GeoTime:
                default:
                    OperatorValuePair = new EmptyPropertyCriteriaOperatorValuePair();
                    break;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked();
        }

        private void PropertyTypePickerControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (propertyTypePickerControl.SelectedItem == null)
            {
                SelectedPropertyDataType = BaseDataTypes.None;
                PropertyTypeUri = string.Empty;
            }
            else
            {
                SelectedPropertyDataType = propertyTypePickerControl.SelectedItem.BaseDataType;
                PropertyTypeUri = propertyTypePickerControl.SelectedItem.TypeUri;
                RelationalOperatorComboBox.SelectedIndex = 0;
            }
        }

        private void PropertyValueTemplatesControl_PropertyValueChanged(object sender, EventArgs e)
        {
            CriteriaValue = PropertyValueTemplatesControl.PropertyValue;
        }

        private void RelationalOperatorComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(RelationalOperatorComboBox.SelectedItem == null)
            {
                return;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals("="))
            {
                RelationalOperator = RelationalOperator.Equals;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals(">"))
            {
                RelationalOperator = RelationalOperator.GreaterThan;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals("<"))
            {
                RelationalOperator = RelationalOperator.LessThan;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals(">="))
            {
                RelationalOperator = RelationalOperator.GreaterThanOrEquals;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals("<="))
            {
                RelationalOperator = RelationalOperator.LessThanOrEquals;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals("!="))
            {
                RelationalOperator = RelationalOperator.NotEquals;
            }
            else if (RelationalOperatorComboBox.SelectedItem.Equals("Like"))
            {
                RelationalOperator = RelationalOperator.Like;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void SetPropertyValueCriteria()
        {
            if (ValidationStatus == ValidationStatus.Invalid)
                return;

            int valueRelationalOperator = (int)RelationalOperator;
            if (OperatorValuePair is LongPropertyCriteriaOperatorValuePair)
            {
                long longVal = 0;
                long.TryParse(PropertyValueTemplatesControl.PropertyValue, out longVal);
                (OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaValue = longVal;
                (OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaOperator = (LongPropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }
            else if (OperatorValuePair is FloatPropertyCriteriaOperatorValuePair)
            {
                float floatVal = 0;
                float.TryParse(PropertyValueTemplatesControl.PropertyValue, out floatVal);
                (OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).CriteriaValue = floatVal;
                (OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).CriteriaOperator = (FloatPropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }
            else if (OperatorValuePair is StringPropertyCriteriaOperatorValuePair)
            {
                (OperatorValuePair as StringPropertyCriteriaOperatorValuePair).CriteriaValue = PropertyValueTemplatesControl.PropertyValue;
                (OperatorValuePair as StringPropertyCriteriaOperatorValuePair).CriteriaOperator = (StringPropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }
            else if (OperatorValuePair is DateTimePropertyCriteriaOperatorValuePair)
            {
                DateTime dateTimeVal = DateTime.MinValue;
                DateTime.TryParse(PropertyValueTemplatesControl.PropertyValue, out dateTimeVal);
                (OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair).CriteriaValue = dateTimeVal;
                (OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair).CriteriaOperator = (DateTimePropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }
            else if (OperatorValuePair is BooleanPropertyCriteriaOperatorValuePair)
            {
                bool boolVal;
                bool.TryParse(PropertyValueTemplatesControl.PropertyValue, out boolVal);
                (OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair).CriteriaValue = boolVal;
                (OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair).CriteriaOperator = (BooleanPropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }
            else if (OperatorValuePair is GeoPointPropertyCriteriaOperatorValuePair geoPointPropertyCriteriaOperatorValuePair)
            {
                geoPointPropertyCriteriaOperatorValuePair.CriteriaValue = 
                    GeoCircle.GeoCircleEntityRawData(PropertyValueTemplatesControl.PropertyValue);
                geoPointPropertyCriteriaOperatorValuePair.CriteriaOperator =
                    (GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator)valueRelationalOperator;
            }

            propertyValueCriteria.OperatorValuePair = OperatorValuePair;
            propertyValueCriteria.PropertyTypeUri = PropertyTypeUri;
        }

        private void PropertyValueTemplatesControl_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        private void PropertyValueTemplatesControl_CheckValidation(object sender, PropertyValueTemplates.ValidationResultEventArgs e)
        {
            ValidationMessage = Properties.Resources.InValid;
            ValidationStatus = e.ValidationResult;
        }

        #endregion        
    }
}
