using GPAS.Ontology;
using GPAS.TreeViewPicker;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.OntologyPickers.Converters;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.OntologyPickers
{
    /// <summary>
    /// این کنترل وظیفه نمایش مشخصه های موجود در آنتولوژی را بصورت درختی برعهده دارد.
    /// </summary>
    public partial class PropertyTypePicker : PresentationControl
    {
        #region Dependencies

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(PropertyTypePicker),
                new PropertyMetadata(false, OnSetIsExpandedChanged));

        private static void OnSetIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetIsExpandedChanged(e);
        }

        private void OnSetIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            OnIsExpandedChanged(e);
        }

        /// <summary>
        /// لیستی از اشیاء را میگیرد که قصد داریم مشخصه های مربوط به آنها را نمایش دهیم
        /// اگر خالی باشد، همه مشخصه‌های همه اشیاء موجود در آنتولوژی را نمایش می دهد
        /// </summary>
        public ObservableCollection<string> ObjectTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(ObjectTypeUriCollectionProperty); }
            set { SetValue(ObjectTypeUriCollectionProperty, value); }
        }


        // Using a DependencyProperty as the backing store for ObjectTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(ObjectTypeUriCollection), typeof(ObservableCollection<string>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OsSetObjectTypeUriCollectionChanged));

        private static void OsSetObjectTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OsSetObjectTypeUriCollectionChanged(e);
        }

        private void OsSetObjectTypeUriCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ObjectTypeUriCollection != null)
            {
                ObjectTypeUriCollection.CollectionChanged -= ObjectTypeUriCollection_CollectionChanged;
                ObjectTypeUriCollection.CollectionChanged += ObjectTypeUriCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnObjectTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnObjectTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));

        }

        /// <summary>
        /// تنها مشخصه هایی به نمایش در می آیند که TypeUri‌آنها در این لیست وجود دارد. 
        /// </summary>
        public ObservableCollection<string> PropertyTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(PropertyTypeUriCollectionProperty); }
            set { SetValue(PropertyTypeUriCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(PropertyTypeUriCollection), typeof(ObservableCollection<string>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetPropertyTypeUriCollectionChanged));

        private static void OnSetPropertyTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetPropertyTypeUriCollectionChanged(e);
        }

        private void OnSetPropertyTypeUriCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (PropertyTypeUriCollection != null)
            {
                PropertyTypeUriCollection.CollectionChanged -= PropertyTypeUriCollection_CollectionChanged;
                PropertyTypeUriCollection.CollectionChanged += PropertyTypeUriCollection_CollectionChanged;
            }

            if (e.OldValue == null)
            {
                OnPropertyTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, PropertyTypeUriCollection));
            }
            else
            {
                OnPropertyTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, PropertyTypeUriCollection, e.OldValue));
            }
        }



        /// <summary>
        /// TypeUri مشخصه هایی را که قصد نمایش آن‌ها را نداریم را در اینجا وارد می کنیم.
        /// </summary>
        public ObservableCollection<string> ExceptTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(ExceptTypeUriCollectionProperty); }
            set { SetValue(ExceptTypeUriCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExceptTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(ExceptTypeUriCollection), typeof(ObservableCollection<string>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetExceptTypeUriCollectionChanged));

        private static void OnSetExceptTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetExceptTypeUriCollectionChanged(e);
        }

        private void OnSetExceptTypeUriCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ExceptTypeUriCollection != null)
            {
                ExceptTypeUriCollection.CollectionChanged -= ExceptTypeUriCollection_CollectionChanged;
                ExceptTypeUriCollection.CollectionChanged += ExceptTypeUriCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnExceptTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnExceptTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        /// <summary>
        /// تنها مشخصه هایی را نمایش می دهد که نوع داده های آن ها در این مجموعه وجود داشته باشد
        /// اگر این مجموعه خالی باشد کلیه مشخصه ها را با هر نوع داده ای نمایش می دهد
        /// </summary>
        public ObservableCollection<BaseDataTypes> DataTypeFilterCollection
        {
            get { return (ObservableCollection<BaseDataTypes>)GetValue(DataTypeFilterCollectionProperty); }
            set { SetValue(DataTypeFilterCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataTypeFilterCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataTypeFilterCollectionProperty =
            DependencyProperty.Register(nameof(DataTypeFilterCollection), typeof(ObservableCollection<BaseDataTypes>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetDataTypeFilterCollectionChanged));

        private static void OnSetDataTypeFilterCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetDataTypeFilterCollectionChanged(e);
        }

        private void OnSetDataTypeFilterCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DataTypeFilterCollection != null)
            {
                DataTypeFilterCollection.CollectionChanged -= DataTypeFilterCollection_CollectionChanged;
                DataTypeFilterCollection.CollectionChanged += DataTypeFilterCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnDataTypeFilterCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnDataTypeFilterCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }

        /// <summary>
        /// مشخصه هایی که نوع داده آنها در این مجموعه وجود دارد را نمایش نمی دهد 
        /// </summary>
        public ObservableCollection<BaseDataTypes> ExceptDataTypeCollection
        {
            get { return (ObservableCollection<BaseDataTypes>)GetValue(ExceptDataTypeCollectionProperty); }
            set { SetValue(ExceptDataTypeCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExceptDataTypeCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptDataTypeCollectionProperty =
            DependencyProperty.Register(nameof(ExceptDataTypeCollection), typeof(ObservableCollection<BaseDataTypes>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetExceptDataTypeCollectionChanged));

        private static void OnSetExceptDataTypeCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetExceptDataTypeCollectionChanged(e);
        }

        private void OnSetExceptDataTypeCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ExceptDataTypeCollection != null)
            {
                ExceptDataTypeCollection.CollectionChanged -= ExceptDataTypeCollection_CollectionChanged;
                ExceptDataTypeCollection.CollectionChanged += ExceptDataTypeCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnExceptDataTypeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnExceptDataTypeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        public ObservableCollection<TreeViewPickerItem> ItemCollection
        {
            get { return (ObservableCollection<TreeViewPickerItem>)GetValue(ItemCollectionProperty); }
            set { SetValue(ItemCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemCollectionProperty =
            DependencyProperty.Register(nameof(ItemCollection), typeof(ObservableCollection<TreeViewPickerItem>), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetItemCollectionChanged));

        private static void OnSetItemCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetItemCollectionChanged(e);
        }

        private void OnSetItemCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ItemCollection != null)
            {
                ItemCollection.CollectionChanged -= ItemCollection_CollectionChanged;
                ItemCollection.CollectionChanged += ItemCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnItemCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnItemCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        public PropertyNode SelectedItem
        {
            get { return (PropertyNode)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(PropertyNode), typeof(PropertyTypePicker),
                new PropertyMetadata(null, OnSetSelectedItemChanged));

        private static void OnSetSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetSelectedItemChanged(e);
        }

        private void OnSetSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedItem == null)
            {
                MainTreeViewPicker.DeselectItem();
            }

            OnSelectedItemChanged(e);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PropertyTypePicker),
                new PropertyMetadata(string.Empty, OnSetTextChanged));

        private static void OnSetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyTypePicker)d).OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            OnTextChanged(e);
        }

        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(DisplayMode), typeof(PropertyTypePicker), new PropertyMetadata(DisplayMode.DropDown));



        public bool DisplayCheckIconForSelectableItems
        {
            get { return (bool)GetValue(DisplayCheckIconForSelectableItemsProperty); }
            set { SetValue(DisplayCheckIconForSelectableItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayCheckIconForSelectableItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayCheckIconForSelectableItemsProperty =
            DependencyProperty.Register(nameof(DisplayCheckIconForSelectableItems), typeof(bool), typeof(PropertyTypePicker),
                new PropertyMetadata(true));



        #endregion

        #region Variables

        private Ontology.Ontology currentOntology = null;

        #endregion

        #region Methodes

        public PropertyTypePicker()
        {
            InitializeComponent();
            InitProperties();
            BindProperties();
            GetOntology();
            CreateOntologyTree();
        }

        private void GetOntology()
        {
            try
            {
                currentOntology = OntologyProvider.GetOntology();
            }
            catch
            {
                // ignored
            }
        }

        private void InitProperties()
        {
            ItemCollection = new ObservableCollection<TreeViewPickerItem>();
            ObjectTypeUriCollection = new ObservableCollection<string>();
            ExceptTypeUriCollection = new ObservableCollection<string>();
            DataTypeFilterCollection = new ObservableCollection<BaseDataTypes>();
            ExceptDataTypeCollection = new ObservableCollection<BaseDataTypes>();
        }

        private void BindProperties()
        {
            Binding selectedItemBinding = new Binding(nameof(SelectedItem));
            selectedItemBinding.Source = this;
            selectedItemBinding.Mode = BindingMode.OneWayToSource;
            selectedItemBinding.Converter = new TreeViewPickerItemTagPropertyToOntologyNodeConverter();
            MainTreeViewPicker.SetBinding(TreeViewPicker.TreeViewPicker.SelectedItemProperty, selectedItemBinding);
        }

        public void RemoveSelectedItem()
        {
            MainTreeViewPicker.DeselectItem();
        }

        public void SelectItem(string typeUri)
        {
            if (string.IsNullOrEmpty(typeUri) || string.IsNullOrWhiteSpace(typeUri))
                RemoveSelectedItem();
            else
                MainTreeViewPicker.SelectItem(OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri));
        }

        private void ObjectTypeUriCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnObjectTypeUriCollectionChanged(e);
        }

        private void PropertyTypeUriCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyTypeUriCollectionChanged(e);
        }

        private void ExceptTypeUriCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnExceptTypeUriCollectionChanged(e);
        }

        private void DataTypeFilterCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDataTypeFilterCollectionChanged(e);
        }

        private void ExceptDataTypeCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnExceptDataTypeCollectionChanged(e);
        }

        private void ItemCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnItemCollectionChanged(e);
        }

        private void CreateOntologyTree()
        {
            if (currentOntology == null)
                return;

            ObservableCollection<OntologyNode> treeProperty = new ObservableCollection<OntologyNode>();
            if (ObjectTypeUriCollection?.Count > 0)
            {
                treeProperty = currentOntology.GetHierarchyPropertiesOfObjects(ObjectTypeUriCollection.ToList());
            }
            else
            {
                treeProperty = currentOntology.GetHierarchyPropertiesOfObject(string.Empty);
            }

            ItemCollection.Clear();

            ObservableCollection<TreeViewPickerItem> hierarchy = CreateHierarchyObjects(treeProperty);

            if (hierarchy?.Count > 0)
            {
                AddItems(hierarchy);
            }
            else
            {
                AddEmptyItem();
            }
        }

        private void AddItems(ObservableCollection<TreeViewPickerItem> items)
        {
            foreach (var item in items)
            {
                ItemCollection.Add(item);
            }
        }

        private void AddEmptyItem()
        {
            ItemCollection.Add(new TreeViewPickerItem()
            {
                Title = Properties.Resources.No_Property,
                IsSelectable = false
            });
        }

        private ObservableCollection<TreeViewPickerItem> CreateHierarchyObjects(ObservableCollection<OntologyNode> nodes)
        {
            ObservableCollection<TreeViewPickerItem> items = new ObservableCollection<TreeViewPickerItem>();

            try
            {
                if (nodes.Count > 0)
                {
                    foreach (PropertyNode node in nodes)
                    {
                        if (PropertyTypeUriCollection?.Count > 0 && !PropertyTypeUriCollectionContainNodeWithChildren(node))
                            continue;

                        if (IsExceptNode(node))
                            continue;

                        bool isSelectable = node.IsLeaf;
                        if (PropertyTypeUriCollection?.Count > 0)
                            isSelectable = node.IsLeaf && PropertyTypeUriCollection.Contains(node.TypeUri);

                        items.Add(new TreeViewPickerItem
                        {
                            Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(node.TypeUri)),
                            Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(node.TypeUri),
                            Children = CreateHierarchyObjects(node.Children),
                            IsSelectable = isSelectable,
                            Tag = node,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Properties.Resources.Object_Types_Hierarchy_Or_Icons_Loading_Failed, ex.Message));
            }

            return items;
        }

        private bool PropertyTypeUriCollectionContainNodeWithChildren(OntologyNode node)
        {
            if (PropertyTypeUriCollection == null)
                return false;

            if (PropertyTypeUriCollection.Contains(node.TypeUri))
                return true;

            return PropertyTypeUriCollection.Intersect(node.GetAllChildren().Select(c => c.TypeUri)).Count() > 0;
        }

        private bool IsExceptNode(PropertyNode node)
        {
            return (node.IsLeaf && DataTypeFilterCollection?.Count > 0 && !DataTypeFilterCollection.Contains(node.BaseDataType)) ||
                (node.IsLeaf && ExceptDataTypeCollection != null && ExceptDataTypeCollection.Contains(node.BaseDataType)) ||
                (ExceptTypeUriCollection != null && ExceptTypeUriCollection.Contains(node.TypeUri));
        }

        private void MainTreeViewPicker_SelectedItemReselected(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnSelectedItemReselected(new DependencyPropertyChangedEventArgs(SelectedItemProperty, SelectedItem, SelectedItem));
        }

        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler IsExpandedChanged;
        protected void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            IsExpandedChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ObjectTypeUriCollectionChanged;
        protected void OnObjectTypeUriCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            ObjectTypeUriCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler PropertyTypeUriCollectionChanged;
        protected void OnPropertyTypeUriCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            PropertyTypeUriCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ExceptTypeUriCollectionChanged;
        protected void OnExceptTypeUriCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            ExceptTypeUriCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler DataTypeFilterCollectionChanged;
        protected void OnDataTypeFilterCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            DataTypeFilterCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ExceptDataTypeCollectionChanged;
        protected void OnExceptDataTypeCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            ExceptDataTypeCollectionChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ItemCollectionChanged;
        protected void OnItemCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemCollectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler SelectedItemChanged;
        protected void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectedItemChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler SelectedItemReselected;
        protected void OnSelectedItemReselected(DependencyPropertyChangedEventArgs e)
        {
            SelectedItemReselected?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TextChanged;
        protected void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        #endregion
    }
}
