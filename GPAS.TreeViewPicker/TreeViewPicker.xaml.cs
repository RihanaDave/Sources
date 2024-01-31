using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.TreeViewPicker
{
    /// <summary>
    /// Interaction logic for TreeViewPicker.xaml
    /// </summary>
    public partial class TreeViewPicker : UserControl
    {
        #region Dependencies

        protected ObservableCollection<TreeViewPickerItem> Items
        {
            get { return (ObservableCollection<TreeViewPickerItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<TreeViewPickerItem>), typeof(TreeViewPicker),
                new PropertyMetadata(null, OnSetItemsChanged));

        private static void OnSetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetItemsChanged(e);
        }

        private void OnSetItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Items == null)
            {
                Items = new ObservableCollection<TreeViewPickerItem>();
                return;
            }

            Items.CollectionChanged -= Items_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            OnItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }


        public ObservableCollection<TreeViewPickerItem> ItemsSource
        {
            get { return (ObservableCollection<TreeViewPickerItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<TreeViewPickerItem>), typeof(TreeViewPicker),
                new PropertyMetadata(null, OnSetItemsSourceChanged));

        private static void OnSetItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetItemsSourceChanged(e);
        }

        private void OnSetItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ItemsSource == null)
            {
                Items?.Clear();
                return;
            }

            ItemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
            ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;

            if (Items == null)
            {
                Items = new ObservableCollection<TreeViewPickerItem>(ItemsSource);
                Items.CollectionChanged -= Items_CollectionChanged;
                Items.CollectionChanged += Items_CollectionChanged;
            }

            ResetItems();
        }

        public TreeViewPickerItem SelectedItem
        {
            get { return (TreeViewPickerItem)GetValue(SelectedItemProperty); }
            protected set
            {
                if (SelectedItem == value)
                    OnSelectedItemReselected(new DependencyPropertyChangedEventArgs(SelectedItemProperty, SelectedItem, SelectedItem));

                SetValue(SelectedItemProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(TreeViewPickerItem), typeof(TreeViewPicker),
                new PropertyMetadata(null, OnSetSelectedItemChanged));

        private static void OnSetSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetSelectedItemChanged(e);
        }

        private void OnSetSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                TreeViewPickerItem oldSelectedItem = e.OldValue as TreeViewPickerItem;
                oldSelectedItem.IsSelected = false;
            }

            if (SelectedItem != null)
            {
                SelectedItem.IsSelected = true;
                Collapse(null);
            }

            OnSelectedItemChanged(e);
        }


        public TreeViewPickerItem CandidateItemForSelect
        {
            get { return (TreeViewPickerItem)GetValue(CandidateItemForSelectProperty); }
            set { SetValue(CandidateItemForSelectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CandidateItemForSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CandidateItemForSelectProperty =
            DependencyProperty.Register(nameof(CandidateItemForSelect), typeof(TreeViewPickerItem), typeof(TreeViewPicker),
                new PropertyMetadata(null, OnSetCandidateItemForSelectChanged));

        private static void OnSetCandidateItemForSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetCandidateItemForSelectChanged(e);
        }

        private void OnSetCandidateItemForSelectChanged(DependencyPropertyChangedEventArgs e)
        {
            OnCandidateItemForSelectChanged(e);
        }

        public string CriteriaSearch
        {
            get { return (string)GetValue(CriteriaSearchProperty); }
            set { SetValue(CriteriaSearchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriteriaSearch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriteriaSearchProperty =
            DependencyProperty.Register(nameof(CriteriaSearch), typeof(string), typeof(TreeViewPicker),
                new PropertyMetadata(string.Empty, OnSetCriteriaSearchChanged));

        private static void OnSetCriteriaSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetCriteriaSearchChanged(e);
        }

        private void OnSetCriteriaSearchChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplyCriteriaSearch();

            CandidateItemForSelectFirstItemMatch();
            if (!string.IsNullOrEmpty(CriteriaSearch))
                Expand();

            OnCriteriaSearchChanged(e);
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpanded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(TreeViewPicker),
                new PropertyMetadata(false, OnSetIsExpandedChanged));

        private static void OnSetIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetIsExpandedChanged(e);
        }

        private void OnSetIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsExpanded)
            {
                CriteriaSearch = string.Empty;
                if (SelectedItem != null)
                    SelectedItem.IsCandidateForSelect = true;

                SearchBoxGetFocus();
            }
            else
            {
                HeaderToggleButtonGetFocus();
            }

            OnIsExpandedChanged(e);
        }

        public string HeaderDefaultTitle
        {
            get { return (string)GetValue(HeaderDefaultTitleProperty); }
            set { SetValue(HeaderDefaultTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChooseItemTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderDefaultTitleProperty =
            DependencyProperty.Register(nameof(HeaderDefaultTitle), typeof(string), typeof(TreeViewPicker),
                new PropertyMetadata("Select or type an item", OnSetHeaderDefaultTitleChanged));

        private static void OnSetHeaderDefaultTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetHeaderDefaultTitleChanged(e);
        }

        private void OnSetHeaderDefaultTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            OnHeaderDefaultTitleChanged(e);
        }


        public BitmapSource HeaderDefaultIcon
        {
            get { return (BitmapSource)GetValue(HeaderDefaultIconProperty); }
            set { SetValue(HeaderDefaultIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderDefaultIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderDefaultIconProperty =
            DependencyProperty.Register(nameof(HeaderDefaultIcon), typeof(BitmapSource), typeof(TreeViewPicker),
                new PropertyMetadata(null, OnSetHeaderDefaultIconChanged));

        private static void OnSetHeaderDefaultIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetHeaderDefaultIconChanged(e);
        }

        private void OnSetHeaderDefaultIconChanged(DependencyPropertyChangedEventArgs e)
        {
            OnHeaderDefaultIconChanged(e);
        }



        public string SearchBoxPlaceHolder
        {
            get { return (string)GetValue(SearchBoxPlaceHolderProperty); }
            set { SetValue(SearchBoxPlaceHolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchBoxPlaceHolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchBoxPlaceHolderProperty =
            DependencyProperty.Register(nameof(SearchBoxPlaceHolder), typeof(string), typeof(TreeViewPicker),
                new PropertyMetadata("Search...", OnSetSearchBoxPlaceHolderChanged));

        private static void OnSetSearchBoxPlaceHolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetSearchBoxPlaceHolderChanged(e);
        }

        private void OnSetSearchBoxPlaceHolderChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSearchBoxPlaceHolderChanged(e);
        }


        public bool ShowExpanderButton
        {
            get { return (bool)GetValue(ShowExpanderButtonProperty); }
            set { SetValue(ShowExpanderButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowExpanderButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowExpanderButtonProperty =
            DependencyProperty.Register(nameof(ShowExpanderButton), typeof(bool), typeof(TreeViewPicker),
                new PropertyMetadata(true, OnSetShowExpanderButtonChanged));

        private static void OnSetShowExpanderButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetShowExpanderButtonChanged(e);
        }

        private void OnSetShowExpanderButtonChanged(DependencyPropertyChangedEventArgs e)
        {
            OnShowExpanderButtonChanged(e);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TreeViewPicker),
                new PropertyMetadata(string.Empty, OnSetTextChanged));

        private static void OnSetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectItem(Text);
            OnTextChanged(e);
        }


        public DisplayMode DisplayMode
        {
            get { return (DisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(nameof(DisplayMode), typeof(DisplayMode), typeof(TreeViewPicker),
                new PropertyMetadata(DisplayMode.DropDown, OnSetDisplayModeChanged));

        private static void OnSetDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TreeViewPicker)d).OnSetDisplayModeChanged(e);
        }

        private void OnSetDisplayModeChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (DisplayMode)
            {
                case DisplayMode.List:
                    FindListUIControls();
                    break;
                case DisplayMode.DropDown:
                default:
                    FindDropDownUIControls();
                    break;
            }
        }



        public bool DisplayCheckIconForSelectableItems
        {
            get { return (bool)GetValue(DisplayCheckIconForSelectableItemsProperty); }
            set { SetValue(DisplayCheckIconForSelectableItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayCheckIconForSelectableItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayCheckIconForSelectableItemsProperty =
            DependencyProperty.Register(nameof(DisplayCheckIconForSelectableItems), typeof(bool), typeof(TreeViewPicker),
                new PropertyMetadata(true));



        #endregion

        #region Methodes

        public TreeViewPicker()
        {
            InitializeComponent();

            InitCommands();
            InitProperties();
        }

        private void InitCommands()
        {
            CollapseCommand = new RelayCommand(Collapse);
            ExpandCommand = new RelayCommand(Expand);
            RemoveSelectedItemCommand = new RelayCommand(RemoveSelectedItem);
            SelectItemCommand = new RelayCommand(SelectItem);
            TreeViewGetFocusCommand = new RelayCommand(TreeViewGetFocus);
            SearchBoxGetFocusIfFirstItemIsSelectedCommand = new RelayCommand(SearchBoxGetFocusIfFirstItemIsSelected);
        }

        private void InitProperties()
        {
            ItemsSource = new ObservableCollection<TreeViewPickerItem>();
        }

        private void SelectItemIfPossible(TreeViewPickerItem item)
        {
            if (item == null || item.IsSelectable)//possible
            {
                SelectedItem = item;
            }

            if (item != null)
            {
                if (item.IsSelectable)
                    item.IsExpanded = true;
                else
                    item.IsExpanded = !item.IsExpanded;
            }
        }

        public void SelectItem(string title)
        {
            SelectItemIfPossible(FindItem(Items, title));
        }

        public void DeselectItem()
        {
            SelectItemIfPossible((TreeViewPickerItem)null);
        }

        private TreeViewPickerItem FindItem(ObservableCollection<TreeViewPickerItem> items, string title)
        {
            TreeViewPickerItem treeViewPickerItem = null;
            foreach (var item in items)
            {
                if (treeViewPickerItem != null)
                    break;

                if (item.Title.Equals(title))
                {
                    treeViewPickerItem = item;
                    break;
                }

                treeViewPickerItem = FindItem(item.Children, title);
            }

            return treeViewPickerItem;
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetItems();
        }

        bool safeItemChange = false;
        private void ResetItems()
        {
            safeItemChange = true;

            Items.Clear();
            foreach (var item in ItemsSource)
            {
                Items.Add(item);
            }

            safeItemChange = false;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ItemsSource?.Count > 0 && !safeItemChange)
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsSource instead.");

            RefreshUI();

            OnItemsChanged(e);
        }

        private void RefreshUI()
        {
            IsExpanded = true;
            IsExpanded = false;
        }

        private void Expand()
        {
            IsExpanded = true;
        }

        private void CandidateItemForSelectFirstItemMatch()
        {
            foreach (var item in Items)
            {
                if (item.CandidateItemForSelectFirstItemMatch(CriteriaSearch))
                    return;
            }

            RemoveCandidateItemForSelect();
        }

        private void RemoveCandidateItemForSelect()
        {
            if (CandidateItemForSelect != null)
                CandidateItemForSelect.IsCandidateForSelect = false;
        }

        private void ApplyCriteriaSearch()
        {
            foreach (var item in Items)
            {
                item.ApplyCriteriaSearch(CriteriaSearch);
            }
        }

        private void FindListUIControls()
        {
            GetTreeView();
            GetSearchTextBox();
        }

        private void FindDropDownUIControls()
        {
            FindPopUp();
            if (ContentPopup == null)
                return;

            GetTreeView();
            GetSearchTextBox();
        }

        TextBox searchTextBox = null;
        private void SearchBoxGetFocus()
        {
            if (DisplayMode == DisplayMode.DropDown)
            {
                FindPopUp();
                if (ContentPopup == null)
                    return;
            }

            if (searchTextBox == null)
                GetSearchTextBox();

            if (searchTextBox != null)
                searchTextBox.Focus();
        }

        private void GetSearchTextBox()
        {
            if (DisplayMode == DisplayMode.List)
                searchTextBox = GetFrameworkElementByName<TextBox>(this, "SearchTextBox");
            else
                searchTextBox = GetFrameworkElementByName<TextBox>(ContentPopup.Child as FrameworkElement, "SearchTextBox");
        }

        Popup ContentPopup = null;
        private void FindPopUp()
        {
            if (ContentPopup == null)
                ContentPopup = GetFrameworkElementByName<Popup>(this, "ContentPopup");
        }

        TreeView treeView = null;

        private void GetTreeView()
        {
            if (DisplayMode == DisplayMode.List)
                treeView = GetFrameworkElementByName<TreeView>(this, "treeView");
            else
                treeView = GetFrameworkElementByName<TreeView>(ContentPopup.Child as FrameworkElement, "treeView");
        }

        private void TreeViewGetFocus()
        {
            if (DisplayMode == DisplayMode.DropDown)
            {
                FindPopUp();
                if (ContentPopup == null)
                    return;
            }

            if (treeView == null)
                GetTreeView();

            if (treeView != null)
            {
                TreeViewItem selectedTreeViewItem = GetSelectedTreeViewItem(treeView);
                if (selectedTreeViewItem == null)
                    treeView.Focus();
                else
                    selectedTreeViewItem.Focus();
            }
        }

        ToggleButton headerToggleButton = null;

        private void FindHeaderToggleButton()
        {
            if (headerToggleButton == null)
                headerToggleButton = GetFrameworkElementByName<ToggleButton>(this, "HeaderToggleButton");
        }

        private void HeaderToggleButtonGetFocus()
        {
            FindHeaderToggleButton();

            if (headerToggleButton != null)
                headerToggleButton.Focus();
        }

        private TreeViewItem GetSelectedTreeViewItem(FrameworkElement referenceElement)
        {
            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;

                if (child != null && child.GetType() == typeof(TreeViewItem) && (child as TreeViewItem).IsSelected)
                {
                    break;
                }

                if (child != null)
                {
                    child = GetSelectedTreeViewItem(child);

                    if (child != null && child.GetType() == typeof(TreeViewItem) && (child as TreeViewItem).IsSelected)
                    {
                        break;
                    }
                }
            }

            return child as TreeViewItem;
        }

        private T GetFrameworkElementByName<T>(FrameworkElement referenceElement, string name) where T : FrameworkElement
        {
            if (referenceElement == null)
                return null;

            FrameworkElement child = null;

            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;

                if (child != null && child.GetType() == typeof(T) && child.Name.Equals(name))
                {
                    break;
                }

                if (child != null)
                {
                    child = GetFrameworkElementByName<T>(child, name);

                    if (child != null && child.GetType() == typeof(T) && child.Name.Equals(name))
                    {
                        break;
                    }
                }
            }

            return child as T;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewPickerItem item = e.NewValue as TreeViewPickerItem;
            if (item == null || item.IsSelectable)
                CandidateItemForSelect = item;
        }

        private void ContentPopup_Opened(object sender, EventArgs e)
        {
            SearchBoxGetFocus();
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TreeViewPickerItem treeViewPickerItem = (TreeViewPickerItem)(sender as FrameworkElement).DataContext;
                SelectItemIfPossible(treeViewPickerItem);

                e.Handled = true;
            }
        }

        private void HeaderToggleButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HandleExpandingPopUp();
        }

        private void ExpanderToggleButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HandleExpandingPopUp();
        }

        private void HandleExpandingPopUp()
        {
            FindHeaderToggleButton();
            if (headerToggleButton != null)
                if (headerToggleButton.IsChecked != IsExpanded)
                {
                    headerToggleButton.IsChecked = false;
                }
        }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler ItemsChanged;
        protected void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged?.Invoke(this, e);
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

        public event DependencyPropertyChangedEventHandler CandidateItemForSelectChanged;
        protected void OnCandidateItemForSelectChanged(DependencyPropertyChangedEventArgs e)
        {
            CandidateItemForSelectChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler CriteriaSearchChanged;
        protected void OnCriteriaSearchChanged(DependencyPropertyChangedEventArgs e)
        {
            CriteriaSearchChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler IsExpandedChanged;
        protected void OnIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            IsExpandedChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler HeaderDefaultTitleChanged;
        protected void OnHeaderDefaultTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            HeaderDefaultTitleChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler HeaderDefaultIconChanged;
        protected void OnHeaderDefaultIconChanged(DependencyPropertyChangedEventArgs e)
        {
            HeaderDefaultIconChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler SearchBoxPlaceHolderChanged;
        protected void OnSearchBoxPlaceHolderChanged(DependencyPropertyChangedEventArgs e)
        {
            SearchBoxPlaceHolderChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ShowExpanderButtonChanged;
        protected void OnShowExpanderButtonChanged(DependencyPropertyChangedEventArgs e)
        {
            ShowExpanderButtonChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TextChanged;
        protected void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        #endregion

        #region Command

        public RelayCommand CollapseCommand { get; set; }

        private void Collapse(object parameter)
        {
            IsExpanded = false;
        }

        public RelayCommand ExpandCommand { get; set; }

        private void Expand(object obj)
        {
            Expand();
        }

        public RelayCommand RemoveSelectedItemCommand { get; set; }
        private void RemoveSelectedItem(object obj)
        {
            SelectItemIfPossible((TreeViewPickerItem)null);
        }

        public RelayCommand SelectItemCommand { get; set; }
        private void SelectItem(object obj)
        {
            GetTreeView();

            TreeViewPickerItem selectedItem = (TreeViewPickerItem)treeView?.SelectedItem;

            SelectItemIfPossible(selectedItem);
        }

        public RelayCommand TreeViewGetFocusCommand { get; set; }
        private void TreeViewGetFocus(object obj)
        {
            TreeViewGetFocus();
        }

        public RelayCommand SearchBoxGetFocusIfFirstItemIsSelectedCommand { get; set; }
        private void SearchBoxGetFocusIfFirstItemIsSelected(object obj)
        {
            SearchBoxGetFocus();
        }

        #endregion
    }
}
