using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.SearchAround.DataMapping;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.PropertyValueTemplates;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for PropertyValueControl.xaml
    /// </summary>
    public partial class PropertyValueControl
    {
        #region متغیرهای سراسری


        public bool SearchMode
        {
            get { return (bool)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchModeProperty =
            DependencyProperty.Register(nameof(SearchMode), typeof(bool), typeof(PropertyValueControl), new PropertyMetadata(false));



        private Ontology.Ontology currentOntology = OntologyProvider.GetOntology();

        public bool ShowNotEqualOperators { get; set; }

        public PropertyMapping RelatedPropertyMapping { get; set; }
        #endregion
        public PropertyValueControl()
        {
            InitializeComponent();
            RelatedPropertyMapping = new PropertyMapping(new OntologyTypeMappingItem(string.Empty), new ConstValueMappingItem(string.Empty));
        }

        #region توابع
        public void Init(ObjectMapping objectMapping)
        {
            if (!propertyTypePickerControl.ObjectTypeUriCollection.Contains(objectMapping.ObjectType.TypeUri))
            {
                propertyTypePickerControl.ObjectTypeUriCollection = new ObservableCollection<string>()
                {
                    objectMapping.ObjectType.TypeUri
                };
            }

            if (!propertyTypePickerControl.ExceptTypeUriCollection.Contains(currentOntology.GetDefaultDisplayNamePropertyTypeUri()))
                propertyTypePickerControl.ExceptTypeUriCollection.Add(currentOntology.GetDefaultDisplayNamePropertyTypeUri());

            if (!propertyTypePickerControl.ExceptDataTypeCollection.Contains(BaseDataTypes.GeoTime))
                propertyTypePickerControl.ExceptDataTypeCollection.Add(BaseDataTypes.GeoTime);
        }

        internal void SetPropertyValues(PropertyMapping currentProperty)
        {
            propertyTypePickerControl.SelectItem(currentProperty.PropertyType.TypeUri);

            PropertyValueTemplatesControl.SetPropertyValue
            (
                (currentProperty.Value as ConstValueMappingItem)?.ConstValue, currentProperty.PropertyType.TypeUri
            );

            SelectShowingComparator(currentProperty);
        }

        private void SelectShowingComparator(PropertyMapping currentProperty)
        {
            if (!(string.IsNullOrEmpty(currentProperty.PropertyType.TypeUri) || string.IsNullOrWhiteSpace(currentProperty.PropertyType.TypeUri)))
            {
                BaseDataTypes selectedPropertyType = currentOntology.GetBaseDataTypeOfProperty(currentProperty.PropertyType.TypeUri);

                switch (selectedPropertyType)
                {
                    case BaseDataTypes.Int:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            LongPropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as LongPropertyCriteriaOperatorValuePair).CriteriaOperator;
                            LongPropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.Boolean:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            BooleanPropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as BooleanPropertyCriteriaOperatorValuePair).CriteriaOperator;
                            BooleanPropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<BooleanPropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.DateTime:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            DateTimePropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as DateTimePropertyCriteriaOperatorValuePair).CriteriaOperator;
                            DateTimePropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<DateTimePropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.String:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            StringPropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as StringPropertyCriteriaOperatorValuePair).CriteriaOperator;
                            StringPropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<StringPropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.Double:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            FloatPropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as FloatPropertyCriteriaOperatorValuePair).CriteriaOperator;
                            FloatPropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<FloatPropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.HdfsURI:
                        break;
                    case BaseDataTypes.Long:
                        foreach (var currentComboBoxItem in propertyComparatorComboBox.Items)
                        {
                            LongPropertyCriteriaOperatorValuePair.RelationalOperator propertyOperator = (currentProperty.Comparator as LongPropertyCriteriaOperatorValuePair).CriteriaOperator;
                            LongPropertyCriteriaOperatorValuePair.RelationalOperator currentComboBoxItemOperator =
                                GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>((currentComboBoxItem as ComboBoxItem).Content.ToString());
                            if (propertyOperator.Equals(currentComboBoxItemOperator))
                            {
                                propertyComparatorComboBox.SelectedItem = currentComboBoxItem as ComboBoxItem;
                                break;
                            }
                        }
                        break;
                    case BaseDataTypes.None:
                    case BaseDataTypes.GeoTime:
                        break;
                }
            }
        }

        #endregion

        #region مدیریت رخدادها
        public event EventHandler<EventArgs> objectChanged;
        protected virtual void OnObjectChanged()
        {
            objectChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region مدیریت رخدادگردانها
        private void propertyComparatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPropertyCriteriaOperatorValuePair(true);
            OnObjectChanged();
        }

        private void SetPropertyCriteriaOperatorValuePair(bool validationResult)
        {
            if (propertyComparatorComboBox.SelectedItem != null)
            {
                string value = PropertyValueTemplatesControl.GetPropertyValue();
                string selectedComboBoxContent = ((ComboBoxItem)propertyComparatorComboBox.SelectedItem).Content.ToString();
                BaseDataTypes propertyBaseDataTypes =
                    OntologyProvider.GetBaseDataTypeOfProperty(RelatedPropertyMapping.PropertyType.TypeUri);


                switch (propertyBaseDataTypes)
                {
                    case BaseDataTypes.Boolean:
                        if (validationResult)
                        {
                            RelatedPropertyMapping.Comparator = new BooleanPropertyCriteriaOperatorValuePair
                            {
                                CriteriaValue = bool.Parse(value),
                                CriteriaOperator = GetValueFromDescription<BooleanPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        else
                        {
                            RelatedPropertyMapping.Comparator = new BooleanPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GetValueFromDescription<BooleanPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        break;

                    case BaseDataTypes.DateTime:
                        if (validationResult)
                        {
                            //DateTime dd = DateTime.ParseExact(value, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.CurrentCulture);
                            //string ff = ((DateTime)parsedValue).ToString(CultureInfo.InvariantCulture);
                            //DateTime fff = Convert.ToDateTime(ff, CultureInfo.InvariantCulture);

                            RelatedPropertyMapping.Comparator = new DateTimePropertyCriteriaOperatorValuePair
                            {
                                CriteriaValue = DateTime.Parse(value),
                                CriteriaOperator = GetValueFromDescription<DateTimePropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        else
                        {
                            RelatedPropertyMapping.Comparator = new DateTimePropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GetValueFromDescription<DateTimePropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        break;

                    case BaseDataTypes.String:
                        RelatedPropertyMapping.Comparator = new StringPropertyCriteriaOperatorValuePair
                        {
                            CriteriaValue = value,
                            CriteriaOperator = GetValueFromDescription<StringPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                        };
                        break;

                    case BaseDataTypes.Double:
                        if (validationResult)
                        {
                            RelatedPropertyMapping.Comparator = new FloatPropertyCriteriaOperatorValuePair
                            {
                                CriteriaValue = Convert.ToSingle(double.Parse(value)),
                                CriteriaOperator = GetValueFromDescription<FloatPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        else
                        {
                            RelatedPropertyMapping.Comparator = new FloatPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GetValueFromDescription<FloatPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        break;

                    case BaseDataTypes.Int:
                        if (validationResult)
                        {
                            RelatedPropertyMapping.Comparator = new LongPropertyCriteriaOperatorValuePair
                            {
                                CriteriaValue = int.Parse(value),
                                CriteriaOperator = GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        else
                        {
                            RelatedPropertyMapping.Comparator = new LongPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        break;

                    case BaseDataTypes.Long:
                        if (validationResult)
                        {
                            RelatedPropertyMapping.Comparator = new LongPropertyCriteriaOperatorValuePair
                            {
                                CriteriaValue = long.Parse(value),
                                CriteriaOperator = GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        else
                        {
                            RelatedPropertyMapping.Comparator = new LongPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GetValueFromDescription<LongPropertyCriteriaOperatorValuePair.RelationalOperator>(selectedComboBoxContent)
                            };
                        }
                        break;

                    case BaseDataTypes.None:
                    case BaseDataTypes.GeoTime:
                    case BaseDataTypes.HdfsURI:
                        break;
                }
            }
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }

        private void PropertyTypePickerControl_SelectedItemChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (propertyTypePickerControl.SelectedItem == null)
            {
                PropertyValueTemplatesControl.PrepareControl(BaseDataTypes.None);
                RelatedPropertyMapping.PropertyType.TypeUri = string.Empty;
                SetPropertyComparatorComboBoxValues(BaseDataTypes.None);
            }
            else
            {
                PropertyValueTemplatesControl.PrepareControl(propertyTypePickerControl.SelectedItem.BaseDataType);
                RelatedPropertyMapping.PropertyType.TypeUri = propertyTypePickerControl.SelectedItem.TypeUri;
                SetPropertyComparatorComboBoxValues(propertyTypePickerControl.SelectedItem.BaseDataType);
            }
        }

        private void SetPropertyComparatorComboBoxValues(BaseDataTypes selectedType)
        {
            propertyComparatorComboBox.Items.Clear();
            propertyComparatorComboBox.IsEnabled = true;
            switch (selectedType)
            {
                case BaseDataTypes.Boolean:
                    BooleanPropertyCriteriaOperatorValuePair booleanOperators = new BooleanPropertyCriteriaOperatorValuePair();
                    ComboBoxItem propertyTypeComboBoxItem = null;
                    foreach (BooleanPropertyCriteriaOperatorValuePair.RelationalOperator relationalOperator in Enum.GetValues(typeof(BooleanPropertyCriteriaOperatorValuePair.RelationalOperator)))
                    {
                        if (ShowNotEqualOperators)
                        {
                            propertyTypeComboBoxItem = new ComboBoxItem();
                            propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                            propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                        }
                        else
                        {
                            if (relationalOperator == BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals)
                            {
                                propertyTypeComboBoxItem = new ComboBoxItem();
                                propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                                propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                                break;
                            }
                        }
                    }

                    propertyComparatorComboBox.SelectedIndex = 0;
                    break;
                case BaseDataTypes.DateTime:
                    DateTimePropertyCriteriaOperatorValuePair dateTimeOperators = new DateTimePropertyCriteriaOperatorValuePair();
                    foreach (DateTimePropertyCriteriaOperatorValuePair.RelationalOperator relationalOperator in Enum.GetValues(typeof(DateTimePropertyCriteriaOperatorValuePair.RelationalOperator)))
                    {
                        if (ShowNotEqualOperators)
                        {
                            propertyTypeComboBoxItem = new ComboBoxItem();
                            propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                            propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                        }
                        else
                        {
                            if (relationalOperator == DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals)
                            {
                                propertyTypeComboBoxItem = new ComboBoxItem();
                                propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                                propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                                break;
                            }
                        }
                    }
                    propertyComparatorComboBox.SelectedIndex = 0;
                    break;
                case BaseDataTypes.Double:
                    FloatPropertyCriteriaOperatorValuePair floatOperators = new FloatPropertyCriteriaOperatorValuePair();
                    foreach (FloatPropertyCriteriaOperatorValuePair.RelationalOperator relationalOperator in Enum.GetValues(typeof(FloatPropertyCriteriaOperatorValuePair.RelationalOperator)))
                    {
                        if (ShowNotEqualOperators)
                        {
                            propertyTypeComboBoxItem = new ComboBoxItem();
                            propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                            propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                        }
                        else
                        {
                            if (relationalOperator == FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals)
                            {
                                propertyTypeComboBoxItem = new ComboBoxItem();
                                propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                                propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                                break;
                            }
                        }
                    }
                    propertyComparatorComboBox.SelectedIndex = 0;
                    break;

                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    LongPropertyCriteriaOperatorValuePair longOperators = new LongPropertyCriteriaOperatorValuePair();
                    foreach (LongPropertyCriteriaOperatorValuePair.RelationalOperator relationalOperator in Enum.GetValues(typeof(LongPropertyCriteriaOperatorValuePair.RelationalOperator)))
                    {
                        if (ShowNotEqualOperators)
                        {
                            propertyTypeComboBoxItem = new ComboBoxItem();
                            propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                            propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                        }
                        else
                        {
                            if (relationalOperator == LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals)
                            {
                                propertyTypeComboBoxItem = new ComboBoxItem();
                                propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                                propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                                break;
                            }
                        }
                    }
                    propertyComparatorComboBox.SelectedIndex = 0;

                    break;
                case BaseDataTypes.String:
                case BaseDataTypes.HdfsURI:
                    StringPropertyCriteriaOperatorValuePair stringOperators = new StringPropertyCriteriaOperatorValuePair();
                    foreach (StringPropertyCriteriaOperatorValuePair.RelationalOperator relationalOperator in Enum.GetValues(typeof(StringPropertyCriteriaOperatorValuePair.RelationalOperator)))
                    {
                        if (ShowNotEqualOperators)
                        {
                            propertyTypeComboBoxItem = new ComboBoxItem();
                            propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                            propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                        }
                        else
                        {
                            if (relationalOperator == StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals)
                            {
                                propertyTypeComboBoxItem = new ComboBoxItem();
                                propertyTypeComboBoxItem.Content = EnumUtitlities.GetUserFriendlyDescriptionOfAnEnumInstance(relationalOperator);
                                propertyComparatorComboBox.Items.Add(propertyTypeComboBoxItem);
                                break;
                            }
                        }
                    }
                    propertyComparatorComboBox.SelectedIndex = 0;

                    break;
                case BaseDataTypes.None:
                    propertyComparatorComboBox.IsEnabled = false;

                    break;
                default:

                    throw new ArgumentException(Properties.Resources.Unknown_Property_Type);
            }
        }

        private void PropertyValueTemplatesControl_CheckValidation(object sender, ValidationResultEventArgs e)
        {
            ((ConstValueMappingItem)RelatedPropertyMapping.Value).ConstValue = PropertyValueTemplatesControl.GetPropertyValue();
            SetPropertyCriteriaOperatorValuePair(e.ValidationResult == PropertiesValidation.ValidationStatus.Valid);
            OnObjectChanged();
        }

        #endregion
    }
}
