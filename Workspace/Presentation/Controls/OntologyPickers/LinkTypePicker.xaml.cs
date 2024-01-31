using GPAS.Ontology;
using GPAS.TreeViewPicker;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.OntologyPickers.Converters;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.OntologyPickers
{
    /// <summary>
    /// این کنترل وظیفه نمایش لینک های موجود در آنتولوژی را بصورت درختی برعهده دارد.
    /// </summary>
    public partial class LinkTypePicker : PresentationControl
    {
        #region Dependencies


        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(LinkTypePicker),
                new PropertyMetadata(false, OnSetIsExpandedChanged));

        private static void OnSetIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetIsExpandedChanged(e);
        }

        private void OnSetIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            OnIsExpandedChanged(e);
        }



        /// <summary>
        /// با این مشخصه لیستی از TypeUri های اشیاء را می گیرد و تنها لینک هایی در درخت آنتولوژی مشاهده می‌شود که مبدا آن‌ها در این لیست باشد.
        /// در صورتی که این لیست خالی باشد درخت انتولوژی نیز خالی است.
        /// در صورتی که برای این مشخصه چند گزینه وارد شده باشد کنترل به مشخصه TargetTypeUri توجه نکرده و تنها لینک هایی را نمایش می دهد که
        /// مبدا آنها در لیست موجود است
        /// </summary>
        public ObservableCollection<string> SourceTypeCollection
        {
            get { return (ObservableCollection<string>)GetValue(SourceTypeCollectionProperty); }
            set { SetValue(SourceTypeCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SourceTypeCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceTypeCollectionProperty =
            DependencyProperty.Register(nameof(SourceTypeCollection), typeof(ObservableCollection<string>), typeof(LinkTypePicker),
                new PropertyMetadata(null, OnSetSourceTypeCollectionChanged));

        private static void OnSetSourceTypeCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetSourceTypeCollectionChanged(e);
        }

        private void OnSetSourceTypeCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SourceTypeCollection != null)
            {
                SourceTypeCollection.CollectionChanged -= SourceTypeCollection_CollectionChanged;
                SourceTypeCollection.CollectionChanged += SourceTypeCollection_CollectionChanged;
            }

            if (e.OldValue == null)
                OnSourceTypeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewValue));
            else
                OnSourceTypeCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        /// <summary>
        /// این مشخصه TypeUri شئ مقصد لینک را دربرمیگیرد.
        /// در صورتی که برای لیست مبدا چند گزینه وارد شده باشد کنترل بدون توجه به مقدار این مشخه تنها لینک هایی را نمایش میدهد که
        /// مبدا آنها در لیست موجود است
        /// </summary>
        public string TargetTypeUri
        {
            get { return (string)GetValue(TargetTypeUriProperty); }
            set { SetValue(TargetTypeUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TargetTypeUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetTypeUriProperty =
            DependencyProperty.Register(nameof(TargetTypeUri), typeof(string), typeof(LinkTypePicker),
                new PropertyMetadata(null, OnSetTargetTypeUriChanged));

        private static void OnSetTargetTypeUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetTargetTypeUriChanged(e);
        }

        private void OnSetTargetTypeUriChanged(DependencyPropertyChangedEventArgs e)
        {
            CreateOntologyTree();

            OnTargetTypeUriChanged(e);
        }



        /// <summary>
        /// TypeUri تمام لینک هایی که قرارنیست در درخت هستان شناسی نمایش داده شوند به این لیست افزوده می‌شوند.
        /// </summary>
        public ObservableCollection<string> ExceptTypeUriCollection
        {
            get { return (ObservableCollection<string>)GetValue(ExceptTypeUriCollectionProperty); }
            set { SetValue(ExceptTypeUriCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExceptTypeUriCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptTypeUriCollectionProperty =
            DependencyProperty.Register(nameof(ExceptTypeUriCollection), typeof(ObservableCollection<string>), typeof(LinkTypePicker),
                new PropertyMetadata(null, OnSetExceptTypeUriCollectionChanged));

        private static void OnSetExceptTypeUriCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetExceptTypeUriCollectionChanged(e);
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



        protected ObservableCollection<TreeViewPickerItem> ItemCollection
        {
            get { return (ObservableCollection<TreeViewPickerItem>)GetValue(ItemCollectionProperty); }
            set { SetValue(ItemCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemCollection.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ItemCollectionProperty =
            DependencyProperty.Register(nameof(ItemCollection), typeof(ObservableCollection<TreeViewPickerItem>), typeof(LinkTypePicker),
                new PropertyMetadata(null, OnSetItemCollectionChanged));

        private static void OnSetItemCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetItemCollectionChanged(e);
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



        public OntologyNode SelectedItem
        {
            get { return (OntologyNode)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(OntologyNode), typeof(LinkTypePicker),
                new PropertyMetadata(null, OnSetSelectedItemChanged));

        private static void OnSetSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetSelectedItemChanged(e);
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
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LinkTypePicker),
                new PropertyMetadata(string.Empty, OnSetTextChanged));

        private static void OnSetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LinkTypePicker)d).OnSetTextChanged(e);
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
            DependencyProperty.Register(nameof(DisplayMode), typeof(DisplayMode), typeof(LinkTypePicker), new PropertyMetadata(DisplayMode.DropDown));


        public bool DisplayCheckIconForSelectableItems
        {
            get { return (bool)GetValue(DisplayCheckIconForSelectableItemsProperty); }
            set { SetValue(DisplayCheckIconForSelectableItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayCheckIconForSelectableItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayCheckIconForSelectableItemsProperty =
            DependencyProperty.Register(nameof(DisplayCheckIconForSelectableItems), typeof(bool), typeof(LinkTypePicker), 
                new PropertyMetadata(true));


        #endregion

        #region Variables

        private Ontology.Ontology currentOntology = null;

        private OntologyNode eventRootNode = null;

        private OntologyNode relationshipRootNode = null;

        #endregion

        #region Methodes

        public LinkTypePicker()
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
            SourceTypeCollection = new ObservableCollection<string>();
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

        private OntologyNode GetEventLinksTree()
        {
            if (currentOntology == null)
                return null;

            if (SourceTypeCollection == null || SourceTypeCollection.Count == 0)
            {
                return null;
            }

            return currentOntology.GetOntologyEventsHierarchy();
        }

        private OntologyNode GetRelationshipLinksTree()
        {
            if (currentOntology == null)
                return null;

            if (SourceTypeCollection == null || SourceTypeCollection.Count == 0)
            {
                return null;
            }

            if (SourceTypeCollection?.Count == 1 && !string.IsNullOrEmpty(TargetTypeUri))
            {
                //todo با توجه به حذف خاصیت گروه بندی اشیا پارامتر سوم همیشه false است.
                //در صورت فعال شدن گروه بندی این بخش باید تکمیل گردد
                return currentOntology.GetValidRelationshipsHierarchy(SourceTypeCollection[0], TargetTypeUri, false);
            }
            else
            {
                return currentOntology.GetAllValidRelationshipsHierarchyForDomainSet(SourceTypeCollection.ToList());
            }
        }

        private void CreateOntologyTree()
        {
            if (currentOntology == null)
                return;

            ItemCollection.Clear();

            CreateEventSubTree();
            CreateRelationshipSubTree();
        }
        
        private void CreateEventSubTree()
        {
            eventRootNode = GetEventLinksTree();

            TreeViewPickerItem eventRootItem = null;
            if (eventRootNode?.Children?.Count > 0)
            {
                var events = CreateHierarchyObjects(new ObservableCollection<OntologyNode>() { eventRootNode });
                if (events?.Count > 0)
                    eventRootItem = events[0];
            }
            else if (eventRootItem == null)
            {
                eventRootItem = new TreeViewPickerItem()
                {
                    Title = Properties.Resources.No_Event,
                    IsSelectable = false,
                };
            }

            ItemCollection.Add(eventRootItem);
        }

        private void CreateRelationshipSubTree()
        {
            relationshipRootNode = GetRelationshipLinksTree();

            TreeViewPickerItem relationshipRootItem = null;
            if (relationshipRootNode?.Children?.Count > 0)
            {
                var rels = CreateHierarchyObjects(new ObservableCollection<OntologyNode>() { relationshipRootNode });
                if (rels?.Count > 0)
                    relationshipRootItem = rels[0];
            }
            else if (relationshipRootItem == null)
            {
                relationshipRootItem = new TreeViewPickerItem()
                {
                    Title = Properties.Resources.No_Relationship,
                    IsSelectable = false
                };
            }

            ItemCollection.Add(relationshipRootItem);
        }

        private ObservableCollection<TreeViewPickerItem> CreateHierarchyObjects(ObservableCollection<OntologyNode> nodes)
        {
            ObservableCollection<TreeViewPickerItem> items = new ObservableCollection<TreeViewPickerItem>();

            try
            {
                if (nodes?.Count > 0)
                {
                    foreach (OntologyNode node in nodes)
                    {
                        if (ExceptTypeUriCollection != null && ExceptTypeUriCollection.Contains(node.TypeUri))
                            continue;

                        items.Add(new TreeViewPickerItem
                        {
                            Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(node.TypeUri)),
                            Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(node.TypeUri),
                            Children = CreateHierarchyObjects(node.Children),
                            Tag = node,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Properties.Resources.Event_Relationship_Types_Hierarchy_Or_Icons_Loading_Failed, ex.Message));
            }

            return items;
        }

        private void SourceTypeCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnSourceTypeCollectionChanged(e);
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

        public event NotifyCollectionChangedEventHandler SourceTypeCollectionChanged;
        protected void OnSourceTypeCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            SourceTypeCollectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TargetTypeUriChanged;
        protected void OnTargetTypeUriChanged(DependencyPropertyChangedEventArgs e)
        {
            TargetTypeUriChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ExceptTypeUriCollectionChanged;
        protected void OnExceptTypeUriCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CreateOntologyTree();

            ExceptTypeUriCollectionChanged?.Invoke(this, e);
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
