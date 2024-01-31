using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;

namespace GPAS.Histogram
{
    public partial class Histogram
    {
        #region Dependencies

        private HistogramItem clickedItem;

        /// <summary>
        /// نشان‌دهنده این است که آیا کلید کنترل فشار داده شده است یا نه
        /// </summary>
        protected bool IsCtrlPressed
        {
            get => (bool)GetValue(IsCtrlPressedProperty);
            set => SetValue(IsCtrlPressedProperty, value);
        }

        protected static readonly DependencyProperty IsCtrlPressedProperty =
            DependencyProperty.Register(nameof(IsCtrlPressed), typeof(bool), typeof(Histogram),
                new PropertyMetadata(false));

        /// <summary>
        /// نمایه هیستوگرام
        /// </summary>
        public BitmapSource Icon
        {
            get => (BitmapSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(BitmapSource), typeof(Histogram),
                new PropertyMetadata(null));

        /// <summary>
        /// عنوان هیستوگرام
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Histogram),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// توضیحات هیستوگرام
        /// </summary>
        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(Histogram),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// آیتم‌های انتخاب شده
        /// </summary>
        public ObservableCollection<HistogramItem> SelectedItems
        {
            get => (ObservableCollection<HistogramItem>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(ObservableCollection<HistogramItem>),
                typeof(Histogram), new PropertyMetadata(OnSetSelectedItemsChanged));

        /// <summary>
        /// آیتم‌های مورد نمایش
        /// </summary>
        public ObservableCollection<HistogramItem> Items
        {
            get => (ObservableCollection<HistogramItem>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<HistogramItem>),
                typeof(Histogram), new PropertyMetadata(null, OnSetItemsChanged));

        protected ObservableCollection<HistogramItem> SortedItems
        {
            get { return (ObservableCollection<HistogramItem>)GetValue(SortedItemsProperty); }
            set { SetValue(SortedItemsProperty, value); }
        }

        protected static readonly DependencyProperty SortedItemsProperty =
            DependencyProperty.Register(nameof(SortedItems), typeof(ObservableCollection<HistogramItem>), typeof(Histogram),
                new PropertyMetadata(null));

        public SortOrder SortOrder
        {
            get => (SortOrder)GetValue(SortOrderProperty);
            set => SetValue(SortOrderProperty, value);
        }

        public static readonly DependencyProperty SortOrderProperty =
            DependencyProperty.Register(nameof(SortOrder), typeof(SortOrder), typeof(Histogram),
                new PropertyMetadata(SortOrder.Descending, OnSetSortOrderChanged));

        public SortPriority SortPriority
        {
            get => (SortPriority)GetValue(SortPriorityProperty);
            set => SetValue(SortPriorityProperty, value);
        }

        public static readonly DependencyProperty SortPriorityProperty =
            DependencyProperty.Register(nameof(SortPriority), typeof(SortPriority), typeof(Histogram),
                new PropertyMetadata(SortPriority.Value, OnSortPriorityChanged));

        public long MaxValue
        {
            get { return (long)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(long), typeof(Histogram), new PropertyMetadata((long)0));

        public bool ShowSnapshotButton
        {
            get { return (bool)GetValue(ShowSnapshotButtonProperty); }
            set { SetValue(ShowSnapshotButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowSnapshotButtonProperty =
            DependencyProperty.Register(nameof(ShowSnapshotButton), typeof(bool), typeof(Histogram), new PropertyMetadata(true));

        public bool ShowSortButton
        {
            get { return (bool)GetValue(ShowSortButtonProperty); }
            set { SetValue(ShowSortButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowSortButtonProperty =
            DependencyProperty.Register(nameof(ShowSortButton), typeof(bool), typeof(Histogram), new PropertyMetadata(true));

        #endregion

        #region Methodes

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public Histogram()
        {
            InitializeComponent();

            Items = new ObservableCollection<HistogramItem>();

            CtrlPressedCommand = new DelegateCommand((obj) => IsCtrlPressed = true);
            CtrlUnpressedCommand = new DelegateCommand(ChangeCtrlPressedStatus);
        }

        private static void OnSetSortOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Histogram)d).OnSetSortOrderChanged(e);
        }

        private static void OnSortPriorityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Histogram)d).OnSortPriorityChanged(e);
        }

        private void OnSetSortOrderChanged(DependencyPropertyChangedEventArgs e)
        {
            foreach (HistogramItem item in Items)
            {
                if (item.Items.Count != 0)
                {
                    item.SortOrder = SortOrder;
                }
            }

            ReOrderItems();
        }

        private void OnSortPriorityChanged(DependencyPropertyChangedEventArgs e)
        {
            foreach (HistogramItem item in Items)
            {
                if (item.Items.Count != 0)
                {
                    item.SortPriority = SortPriority;
                }
            }

            ReOrderItems();
        }

        private void ReOrderItems()
        {
            IEnumerable<HistogramItem> result = new List<HistogramItem>();

            if (Items != null)
                switch (SortPriority)
                {
                    case SortPriority.Value:
                        result = SortOrder == SortOrder.Descending
                            ? Items.OrderByDescending(x => x.Value).ToList()
                            : Items.OrderBy(x => x.Value).ToList();
                        break;
                    case SortPriority.Title:
                        result = SortOrder == SortOrder.Descending
                            ? Items.OrderByDescending(x => x.Title).ToList()
                            : Items.OrderBy(x => x.Title).ToList();
                        break;
                }

            SortedItems = new ObservableCollection<HistogramItem>(result);
        }

        private void ChangeCtrlPressedStatus(object unpressedKey)
        {
            if (unpressedKey.Equals(Key.LeftCtrl) || unpressedKey.Equals(Key.RightCtrl))
            {
                IsCtrlPressed = false;
            }
        }

        private static void OnSetItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Histogram)d).OnSetItemsChanged(e);
        }

        private static void OnSetSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Histogram)d).OnSetSelectedItemsChanged(e);
        }

        private void OnSetItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Items == null)
            {
                ItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                Items.CollectionChanged -= ItemsOnCollectionChanged;
                Items.CollectionChanged += ItemsOnCollectionChanged;

                if (e.OldValue is ObservableCollection<HistogramItem> oldValue)
                    ItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Items, oldValue));
                else
                    ItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Items));
            }
        }

        private void ItemOnDeselected(object sender, EventArgs e)
        {
            RecalculateSelectedItems();
        }

        private void ItemOnSelected(object sender, EventArgs e)
        {
            RecalculateSelectedItems();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HistogramItem item in e.OldItems)
                {
                    item.PreSelectedItem -= ItemOnPreSelectedItem;
                    item.Selected -= ItemOnSelected;
                    item.Deselected -= ItemOnDeselected;
                    item.ItemsChanged -= ItemOnItemsChanged;
                    item.SelectedItemsChanged -= ItemOnSelectedItemChanged;
                    item.ValueChanged -= Item_ValueChanged;
                    item.TitleChanged -= Item_TitleChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (HistogramItem item in e.NewItems)
                {
                    item.SortOrder = SortOrder;
                    item.SortPriority = SortPriority;

                    item.PreSelectedItem -= ItemOnPreSelectedItem;
                    item.PreSelectedItem += ItemOnPreSelectedItem;

                    item.Selected -= ItemOnSelected;
                    item.Selected += ItemOnSelected;

                    item.Deselected -= ItemOnDeselected;
                    item.Deselected += ItemOnDeselected;

                    item.ItemsChanged -= ItemOnItemsChanged;
                    item.ItemsChanged += ItemOnItemsChanged;

                    item.SelectedItemsChanged -= ItemOnSelectedItemChanged;
                    item.SelectedItemsChanged += ItemOnSelectedItemChanged;

                    item.ValueChanged -= Item_ValueChanged;
                    item.ValueChanged += Item_ValueChanged;

                    item.TitleChanged -= Item_TitleChanged;
                    item.TitleChanged += Item_TitleChanged;
                }
            }

            if (Items == null || Items?.Count == 0)
            {
                if (SelectedItems != null && SelectedItems.Count != 0)
                    SelectedItems.Clear();
            }

            ReOrderItems();
            RecalculateMaxValue();
            OnItemsChanged(e);
        }

        private void RecalculateMaxValue()
        {
            if (Items?.Count > 0)
                MaxValue = Items.Max(x => x.Value);
            else
                MaxValue = 0;
        }

        private void Item_TitleChanged(object sender, EventArgs e)
        {
            if (SortPriority == SortPriority.Title)
                ReOrderItems();
        }

        private void Item_ValueChanged(object sender, EventArgs e)
        {
            RecalculateMaxValue();

            if (SortPriority == SortPriority.Value)
                ReOrderItems();
        }

        private void ItemOnSelectedItemChanged(object sender, EventArgs e)
        {
            RecalculateSelectedItems();
        }

        private void OnSetSelectedItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSelectedItemsChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }

        private void ItemOnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        private void RecalculateSelectedItems()
        {
            SelectedItems = new ObservableCollection<HistogramItem>(GetAllSelectedItems());
        }

        private IEnumerable<HistogramItem> GetAllSelectedItems()
        {
            IEnumerable<HistogramItem> selectedItems = Items.SelectMany(item => item.SelectedItems);
            return selectedItems.Where(item => item.IsLeaf);
        }

        private void ItemOnPreSelectedItem(object sender, ChangeChildrenToShowCountEventArgs e)
        {
            // SelectedItems = new ObservableCollection<MarkHistogramItem>(GetAllSelectedItems());

            if (IsCtrlPressed)
                return;

            if (e.SelectedItem.Parent != null)
            {
                if (e.SelectedItem.Parent.IsSelected)
                    if (e.SelectedItem != clickedItem)
                        return;
            }

            DeselectAllItemsExceptSpecificItem(e.SelectedItem, Items);

            //  SelectedItems = new ObservableCollection<MarkHistogramItem>(GetAllSelectedItems());
        }

        private void DeselectAllItemsExceptSpecificItem(HistogramItem newSelection,
            ObservableCollection<HistogramItem> itemCollection)
        {
            foreach (HistogramItem item in itemCollection)
            {
                //زیر مجموعه‌های خودش را لغو انتخاب نمی‌کند
                if (item.Equals(newSelection))
                    continue;

                item.IsSelected = false;

                if (item.ItemsToShow.Count > 0)
                {
                    DeselectAllItemsExceptSpecificItem(newSelection, item.ItemsToShow);
                }
            }
        }

        private void ShowAllHyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            ((HistogramItem)((Hyperlink)sender).DataContext).ShowAllItems();
            OnShowAllClicked((HistogramItem)((Hyperlink)sender).DataContext);
        }

        private void MoreHyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            ((HistogramItem)((Hyperlink)sender).DataContext).ShowMoreItems();

            if (((HistogramItem)((Hyperlink)sender).DataContext).ItemsToShow.Count ==
                ((HistogramItem)((Hyperlink)sender).DataContext).Items.Count)
            {
                OnShowMoreClicked((HistogramItem)((Hyperlink)sender).DataContext);
            }
        }

        private void LessHyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            ((HistogramItem)((Hyperlink)sender).DataContext).ShowLessItems();
            OnShowLessClicked((HistogramItem)((Hyperlink)sender).DataContext);
        }

        private void ItemsTreeViewItemOnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (((TreeViewItem)sender).DataContext as HistogramItem == clickedItem)
            {
                clickedItem.IsSelected = true;
                OnItemMouseRightButtonDown(((TreeViewItem)sender).DataContext as HistogramItem);
            }
        }

        private void SnapshotButtonOnClick(object sender, RoutedEventArgs e)
        {
            ScrollViewer.ScrollToHome();
            OnSnapshotTaken(TakeSnapshot(MainGrid));
        }

        /// <summary>
        /// در اینجا آیتمی که بر روی آن کلیک شده ذخیره می‌شود برای مدیریت وضعیت انتخاب آیتم‌ها
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsTreeViewItemOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            if (hitTestResult.VisualHit.GetVisualParent<CheckBox>() is CheckBox checkBox)
            {
                if ((HistogramItem)((TreeViewItem)sender).DataContext == (HistogramItem)checkBox.DataContext)
                {
                    clickedItem = (HistogramItem)checkBox.DataContext;
                }
            }
        }

        /// <summary>
        /// وقتی آیتمی لغو انتخاب می‌شود بررسی می‌شود آیا این آیتم همانی است که ذخیره شده بود یا نه
        /// اگر آن بود دوباره انتخاب می‌شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxOnUnchecked(object sender, RoutedEventArgs e)
        {

            if (IsCtrlPressed)
                return;

            if (clickedItem == ((CheckBox)sender).DataContext)
            {
                ((HistogramItem)((CheckBox)sender).DataContext).IsSelected = true;
            }
        }

        /// <summary>
        /// با انتخاب هر جایی از سطر در منوی مرتب‌ سازی
        /// آن گزینه انتخاب ‌می‌شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewPopUpMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            RadioButton radioButton = (RadioButton)((Grid)sender).Children[1];
            radioButton.IsChecked = true;
        }

        private PngBitmapEncoder TakeSnapshot(UIElement uIElement)
        {
            ActionGrid.Visibility = Visibility.Hidden;

            int width = (int)uIElement.RenderSize.Width;
            int height = (int)uIElement.RenderSize.Height;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(uIElement);

            PngBitmapEncoder pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            ActionGrid.Visibility = Visibility.Visible;

            return pngImage;
        }

        public void ClearAll()
        {
            Title = string.Empty;
            Description = string.Empty;
            Icon = null;
            Reset();
        }

        public void Reset()
        {
            SortPriority = SortPriority.Value;
            SortOrder = SortOrder.Descending;

            ClearItems();
        }

        public void ClearItems()
        {
            if (Items.Count != 0)
            {
                Items.Clear();
            }
        }

        #endregion

        #region Commands

        public DelegateCommand CtrlPressedCommand { get; set; }

        public DelegateCommand CtrlUnpressedCommand { get; set; }

        #endregion

        #region Events

        public event NotifyCollectionChangedEventHandler SelectedItemsChanged;
        private void OnSelectedItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            SelectedItemsChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ItemsChanged;
        private void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged?.Invoke(this, e);
        }

        public event EventHandler<ChangeChildrenToShowCountEventArgs> ItemMouseRightButtonDown;
        private void OnItemMouseRightButtonDown(HistogramItem item)
        {
            ItemMouseRightButtonDown?.Invoke(this, new ChangeChildrenToShowCountEventArgs(item));
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(PngBitmapEncoder image)
        {
            SnapshotTaken?.Invoke(this, new TakeSnapshotEventArgs(image, $"Histogram {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}"));
        }

        public event EventHandler<ChangeChildrenToShowCountEventArgs> ShowAllClicked;
        private void OnShowAllClicked(HistogramItem item)
        {
            ShowAllClicked?.Invoke(this, new ChangeChildrenToShowCountEventArgs(item));
        }

        public event EventHandler<ChangeChildrenToShowCountEventArgs> ShowMoreClicked;
        private void OnShowMoreClicked(HistogramItem item)
        {
            ShowMoreClicked?.Invoke(this, new ChangeChildrenToShowCountEventArgs(item));
        }

        public event EventHandler<ChangeChildrenToShowCountEventArgs> ShowLessClicked;
        private void OnShowLessClicked(HistogramItem item)
        {
            ShowLessClicked?.Invoke(this, new ChangeChildrenToShowCountEventArgs(item));
        }

        #endregion

        private void ItemsTreeView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                return;

            clickedItem = null;

            if (Items.Count != 0)
            {
                DeselectAllItems(Items);
            }
        }

        private void DeselectAllItems(ObservableCollection<HistogramItem> itemCollection)
        {
            foreach (HistogramItem item in itemCollection)
            {
                if (item.IsSelected)
                {
                    item.IsSelected = false;
                }

                if (item.ItemsToShow.Count > 0)
                {
                    DeselectAllItems(item.ItemsToShow);
                }
            }
        }

        private void ScrollViewerOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void SortPriorityRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SortPriority = (SortPriority)((RadioButton)sender).Tag;
        }

        private void SortOrderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SortOrder = (SortOrder)((RadioButton)sender).Tag;
        }

        private void SortByToolBar_OnOpened(object sender, RoutedEventArgs e)
        {
            if (SortPriority == SortPriority.Value)
            {
                ValueRadioButton.IsChecked = true;
            }
            else
            {
                TitleRadioButton.IsChecked = true;
            }

            if (SortOrder == SortOrder.Ascending)
            {
                AscendingRadioButton.IsChecked = true;
            }
            else
            {
                DescendingRadioButton.IsChecked = true;
            }
        }
    }
}
