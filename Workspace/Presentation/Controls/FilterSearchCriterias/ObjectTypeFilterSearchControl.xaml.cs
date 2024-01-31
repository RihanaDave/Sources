using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    /// <summary>
    /// Interaction logic for ObjectTypeFilterSearchControl.xaml
    /// </summary>
    public partial class ObjectTypeFilterSearchControl : BaseFilterSearchControl
    {
        #region Dependencies

        public ObservableCollection<string> ObjectsTypeUri
        {
            get { return (ObservableCollection<string>)GetValue(ObjectsTypeUriProperty); }
            set { SetValue(ObjectsTypeUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsTypeUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsTypeUriProperty =
            DependencyProperty.Register(nameof(ObjectsTypeUri), typeof(ObservableCollection<string>), typeof(ObjectTypeFilterSearchControl),
                new PropertyMetadata(null, OnSetObjectsTypeUriChanged));

        private static void OnSetObjectsTypeUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectTypeFilterSearchControl)d).OnSetObjectsTypeUriChanged(e);
        }

        private void OnSetObjectsTypeUriChanged(DependencyPropertyChangedEventArgs e)
        {
            ObjectsTypeUri.CollectionChanged -= ObjectsTypeUri_CollectionChanged;
            ObjectsTypeUri.CollectionChanged += ObjectsTypeUri_CollectionChanged;

            OnObjectsTypeUriChanged(new CriteriaItemsChangedEventArgs((IList)e.OldValue, (IList)e.NewValue));
        }


        public bool? IncludeChildObjects
        {
            get { return (bool?)GetValue(IncludeChildObjectsProperty); }
            set { SetValue(IncludeChildObjectsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IncludeChildObjects.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IncludeChildObjectsProperty =
            DependencyProperty.Register(nameof(IncludeChildObjects), typeof(bool?), typeof(ObjectTypeFilterSearchControl),
                new PropertyMetadata(false, OnSetIncludeChildObjectsChanged));

        private static void OnSetIncludeChildObjectsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectTypeFilterSearchControl)d).OnSetIncludeChildObjectsChanged(e);
        }

        private void OnSetIncludeChildObjectsChanged(DependencyPropertyChangedEventArgs e)
        {
            SetObjectTypeCriteria();
            OnIncludeChildObjectsChanged();
        }


        #endregion

        #region Events

        public event EventHandler<CriteriaItemsChangedEventArgs> ObjectsTypeUriChanged;
        protected void OnObjectsTypeUriChanged(CriteriaItemsChangedEventArgs args)
        {
            ObjectsTypeUriChanged?.Invoke(this, args);
        }

        public event EventHandler IncludeChildObjectsChanged;
        protected void OnIncludeChildObjectsChanged()
        {
            IncludeChildObjectsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Variables

        private ObjectTypeCriteria objectTypeCriteria = null;

        #endregion

        #region Methodes

        public ObjectTypeFilterSearchControl()
        {
            InitializeComponent();
            Init();
        }

        protected override void SetValidationStatus()
        {
            if (objectTypeControl.SelectedItem == null)
            {
                ValidationStatus = ValidationStatus.Invalid;
                ValidationMessage = Properties.Resources.Select_an_object_type_from_list_;
            }
            else
            {
                ValidationStatus = ValidationStatus.Valid;
                ValidationMessage = string.Empty;
            }
        }

        protected override void AfterSetCriteriaBase()
        {
            objectTypeCriteria = CriteriaBase as ObjectTypeCriteria;
            objectTypeCriteria.ObjectsTypeUri = ObjectsTypeUri;

            Binding IncludeChildObjectsBinding = new Binding(nameof(IncludeChildObjects));
            IncludeChildObjectsBinding.Source = this;
            IncludeChildObjectsBinding.Mode = BindingMode.TwoWay;
            ChildCheckBox.SetBinding(CheckBox.IsCheckedProperty, IncludeChildObjectsBinding);
        }

        private void Init()
        {
            DataContext = this;

            SetValidationStatus();

            ObjectsTypeUri = new ObservableCollection<string>();
            ObjectsTypeUri.CollectionChanged -= ObjectsTypeUri_CollectionChanged;
            ObjectsTypeUri.CollectionChanged += ObjectsTypeUri_CollectionChanged;

            if (CriteriaBase == null)
            {
                CriteriaBase = new ObjectTypeCriteria();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked();
        }

        private void ObjectsTypeUri_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnObjectsTypeUriChanged(new CriteriaItemsChangedEventArgs(e.OldItems, e.NewItems));
        }

        private void ObjectTypeControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            SetObjectTypeCriteria();
        }

        private void SetObjectTypeCriteria()
        {
            if (ValidationStatus != ValidationStatus.Invalid)
            {
                ObjectsTypeUri.Clear();

                ObjectsTypeUri.Add(objectTypeControl.SelectedItem?.TypeUri);

                if (IncludeChildObjects == true)
                {
                    var childs = OntologyProvider.GetAllChilds(objectTypeControl.SelectedItem?.TypeUri);
                    foreach (var child in childs)
                    {
                        ObjectsTypeUri.Add(child);
                    }
                }
            }
        }

        private void ObjectTypeControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        private void ChildCheckBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        #endregion
    }
}
