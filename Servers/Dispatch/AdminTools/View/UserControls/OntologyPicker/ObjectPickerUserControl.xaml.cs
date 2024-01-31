using GPAS.Dispatch.AdminTools.View.Converters;
using GPAS.Ontology;
using GPAS.TreeViewPicker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GPAS.Dispatch.AdminTools.View.UserControls.OntologyPicker
{
    public partial class ObjectPickerUserControl : UserControl
    {
        #region Dependencies

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ObjectPickerUserControl),
                new PropertyMetadata(false, OnSetIsExpandedChanged));

        private static void OnSetIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetIsExpandedChanged(e);
        }

        private void OnSetIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            OnIsExpandedChanged(e);
        }

        /// <summary>
        /// تنها اشیایی در درخت هستان شناسی نمایش داده می شوند که در این لیست باشند. اگر این لیست خالی باشد همه اشیا نمایش داده خواهند شد.
        /// </summary>
        public ObservableCollection<string> ObjectTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(ObjectTypeUriCollectionProperty); }
            set { SetValue(ObjectTypeUriCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(ObjectTypeUriCollection), typeof(ObservableCollection<string>), typeof(ObjectPickerUserControl),
                new PropertyMetadata(null, OnSetObjectTypeUriCollectionChanged));

        private static void OnSetObjectTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetObjectTypeUriCollectionChanged(e);
        }

        private void OnSetObjectTypeUriCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ObjectTypeUriCollection != null)
            {
                ObjectTypeUriCollection.CollectionChanged -= ObjectTypeUriCollection_CollectionChanged;
                ObjectTypeUriCollection.CollectionChanged += ObjectTypeUriCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnObjectTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ObjectTypeUriCollection));
            else
                OnObjectTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ObjectTypeUriCollection, e.OldValue));
        }


        /// <summary>
        /// تمام گزینه های که قرارنیست در درخت هستان شناسی نمایش داده شوند به این لیست افزوده می‌شوند.
        /// </summary>
        public ObservableCollection<string> ExceptTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(ExceptTypeUriCollectionProperty); }
            set { SetValue(ExceptTypeUriCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExceptTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(ExceptTypeUriCollection), typeof(ObservableCollection<string>), typeof(ObjectPickerUserControl),
                new PropertyMetadata(null, OnSetExceptTypeUriCollectionChanged));

        private static void OnSetExceptTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetExceptTypeUriCollectionChanged(e);
        }

        private void OnSetExceptTypeUriCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ExceptTypeUriCollection != null)
            {
                ExceptTypeUriCollection.CollectionChanged -= ExceptTypeUriCollection_CollectionChanged;
                ExceptTypeUriCollection.CollectionChanged += ExceptTypeUriCollection_CollectionChanged;
            }

            GetDefautExceptTypeUriList();

            if (e.OldValue == null)
                OnExceptTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnExceptTypeUriCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        /// <summary>
        /// لیستی از اشیاء را می گیرد که با یک لینک ارتباط دارند.
        /// در درخت آنتتولوژی اشیائی را نمایش می دهد که این لینک را دارا هستند.
        /// اگر این گزینه خالی باشد یا اینکه شئ یا لینکی را نداشته باشد همه اشیاء نمایش داده خواهند شد. 
        /// </summary>
        /// <param name="item1">لینک</param>
        /// <param name="item2">لیستی از اشیاء</param>
        public Tuple<string, List<string>> ObjectsRelatedLikeType
        {
            get { return (Tuple<string, List<string>>)GetValue(ObjectsRelatedLikeTypeProperty); }
            set { SetValue(ObjectsRelatedLikeTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsRelatedLikeType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsRelatedLikeTypeProperty =
            DependencyProperty.Register(nameof(ObjectsRelatedLikeType), typeof(Tuple<string, List<string>>), typeof(ObjectPickerUserControl),
                new PropertyMetadata(null, OnSetObjectsRelatedLikeTypeChanged));

        private static void OnSetObjectsRelatedLikeTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetObjectsRelatedLikeTypeChanged(e);
        }

        private void OnSetObjectsRelatedLikeTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            OnObjectsRelatedLikeTypeChanged(e);
        }


        /// <summary>
        /// درخت آنتولوژی زا شامل می شود.
        /// </summary>
        protected ObservableCollection<TreeViewPickerItem> ItemCollection
        {
            get { return (ObservableCollection<TreeViewPickerItem>)GetValue(ItemCollectionProperty); }
            set { SetValue(ItemCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemCollection.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ItemCollectionProperty =
            DependencyProperty.Register(nameof(ItemCollection), typeof(ObservableCollection<TreeViewPickerItem>), typeof(ObjectPickerUserControl),
                new PropertyMetadata(null, OnSetItemCollectionChanged));

        private static void OnSetItemCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetItemCollectionChanged(e);
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



        /// <summary>
        /// گزینه انتخاب شده را برمیگرداند.
        /// </summary>
        public OntologyNode SelectedItem
        {
            get { return (OntologyNode)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(OntologyNode), typeof(ObjectPickerUserControl),
                new PropertyMetadata(null, OnSetSelectedItemChanged));

        private static void OnSetSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetSelectedItemChanged(e);
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
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ObjectPickerUserControl),
                new PropertyMetadata(string.Empty, OnSetTextChanged));

        private static void OnSetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPickerUserControl)d).OnSetTextChanged(e);
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
            DependencyProperty.Register(nameof(DisplayMode), typeof(DisplayMode), typeof(ObjectPickerUserControl), new PropertyMetadata(DisplayMode.DropDown));


        #endregion

        #region Variables

        private Ontology.Ontology currentOntology = null;

        private ObservableCollection<OntologyNode> treeAllObject;

        private List<string> defaultExceptTypeUriList;

        #endregion

        #region Methodes

        public ObjectPickerUserControl()
        {
            InitializeComponent();
            InitProperties();
            BindProperties();
            GetOntology();
            treeAllObject = GetAllObjectsTree();
            GetDefautExceptTypeUriList();
            CreateOntologyTree();
        }

        private void GetOntology()
        {
            try
            {
                currentOntology = OntologyLoader.OntologyLoader.GetOntology();
            }
            catch
            {
                // ignored
            }
        }

        private void InitProperties()
        {
            ItemCollection = new ObservableCollection<TreeViewPickerItem>();
            ExceptTypeUriCollection = new ObservableCollection<string>();
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


        private ObservableCollection<OntologyNode> GetAllObjectsTree()
        {
            if (currentOntology == null)
                return new ObservableCollection<OntologyNode>();

            return currentOntology?.GetOntologyObjectsHierarchy();
        }

        private ObservableCollection<OntologyNode> GetObjectsWithRelatedLink(string linkType, List<string> objectsUri)
        {
            if (currentOntology == null)
                return new ObservableCollection<OntologyNode>();

            return currentOntology?.GetEntitiesTreeForASpecificDomainAndLink(objectsUri, linkType);
        }

        private void CreateOntologyTree()
        {
            if (currentOntology == null)
                return;

            ObservableCollection<OntologyNode> treeObject;

            if (IsObjectsWithRelatedLink())
            {
                treeObject = GetObjectsWithRelatedLink(ObjectsRelatedLikeType.Item1, ObjectsRelatedLikeType.Item2);
            }
            else
            {
                treeObject = GetAllObjectsTree();
            }

            ItemCollection.Clear();

            ObservableCollection<TreeViewPickerItem> hierarchy = CreateHierarchyObjects(treeObject);

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
                Title = "No object",
                IsSelectable = false
            });
        }

        private bool IsObjectsWithRelatedLink()
        {
            return ObjectsRelatedLikeType != null &&
                !string.IsNullOrEmpty(ObjectsRelatedLikeType.Item1) &&
                ObjectsRelatedLikeType.Item2 != null &&
                ObjectsRelatedLikeType.Item2.Count > 0;
        }

        private void GetDefautExceptTypeUriList()
        {
            defaultExceptTypeUriList = new List<string>();
            defaultExceptTypeUriList.AddRange(GetAllDocumentChildren());
        }

        private List<string> GetAllDocumentChildren()
        {
            //لیست تمام اشیا از نوع سند را برمیگرداند، منظور خود استاد است نه فرزندان آن ها
            List<OntologyNode> documents = new List<OntologyNode>();
            if (treeAllObject == null)
                treeAllObject = GetAllObjectsTree();

            GetDocumentList(treeAllObject, documents);

            //لیست تمام فرزندان اسناد را برمیگرداند
            List<string> docChildren = new List<string>();
            foreach (var doc in documents)
            {
                docChildren.AddRange(doc.GetAllChildren().Select(c => c.TypeUri).Distinct());
            }

            //فرزندان اسناد را به لیست گزینه های نمایش داده نشده در هستان شناسی می افزاید
            return new List<string>(docChildren);
        }

        private void GetDocumentList(IList<OntologyNode> nodes, IList<OntologyNode> documents)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            if (documents == null)
                documents = new List<OntologyNode>();

            foreach (var node in nodes)
            {
                if (currentOntology.IsDocument(node.TypeUri))
                {
                    documents.Add(node);
                }
                else
                {
                    GetDocumentList(node.Children, documents);
                }
            }
        }

        private ObservableCollection<TreeViewPickerItem> CreateHierarchyObjects(ObservableCollection<OntologyNode> nodes)
        {
            ObservableCollection<TreeViewPickerItem> items = new ObservableCollection<TreeViewPickerItem>();
            List<string> allExceptTypeUri = GetAllExceptTypeUri();

            try
            {
                if (nodes.Count > 0)
                {
                    foreach (OntologyNode node in nodes)
                    {
                        if (ObjectTypeUriCollection?.Count > 0 && !ObjectTypeUriCollectionContainNodeWithChildren(node))
                            continue;

                        if (allExceptTypeUri != null && allExceptTypeUri.Contains(node.TypeUri))
                            continue;

                        bool isSelectable = true;
                        if (ObjectTypeUriCollection?.Count > 0)
                            isSelectable = ObjectTypeUriCollection.Contains(node.TypeUri);

                        items.Add(new TreeViewPickerItem
                        {
                            Icon = new BitmapImage(OntologyProvider.GetTypeIconPath(node.TypeUri)),
                            Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(node.TypeUri),
                            Children = CreateHierarchyObjects(node.Children),
                            IsSelectable = isSelectable,
                            Tag = node,
                        });
                    }
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException("Object types hierarchy or icons loading failed");
            }

            return items;
        }

        private bool ObjectTypeUriCollectionContainNodeWithChildren(OntologyNode node)
        {
            if (ObjectTypeUriCollection == null)
                return false;

            if (ObjectTypeUriCollection.Contains(node.TypeUri))
                return true;

            return ObjectTypeUriCollection.Intersect(node.GetAllChildren().Select(c => c.TypeUri)).Count() > 0;
        }

        private List<string> GetAllExceptTypeUri()
        {
            if (defaultExceptTypeUriList != null && ExceptTypeUriCollection != null)
                return defaultExceptTypeUriList.Concat(ExceptTypeUriCollection).ToList();
            else if (defaultExceptTypeUriList != null)
                return defaultExceptTypeUriList;
            else if (ExceptTypeUriCollection != null)
                return ExceptTypeUriCollection.ToList();
            else
                return new List<string>();
        }

        private void ObjectTypeUriCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnObjectTypeUriCollectionChanged(e);
        }

        private void ExceptTypeUriCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnExceptTypeUriCollectionChanged(e);
        }

        private void ItemCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnItemCollectionChanged(e);
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

        public event NotifyCollectionChangedEventHandler ExceptTypeUriCollectionChanged;
        protected void OnExceptTypeUriCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            ExceptTypeUriCollectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ObjectsRelatedLikeTypeChanged;
        protected void OnObjectsRelatedLikeTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            CreateOntologyTree();

            ObjectsRelatedLikeTypeChanged?.Invoke(this, e);
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
