using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Histogram
{
    public class HistogramItem : INotifyPropertyChanged
    {
        #region Properties

        private int currentNumberLimitationToShow;

        private int numberLimitationToShow;
        public int NumberLimitationToShow
        {
            get => numberLimitationToShow;
            set
            {
                if (SetValue(ref numberLimitationToShow, value))
                {
                    currentNumberLimitationToShow = NumberLimitationToShow;
                }
            }
        }

        private long totaleItemsCount = 0;
        public long TotaleItemsCount
        {
            get => totaleItemsCount;
            set
            {
                if (SetValue(ref totaleItemsCount, value))
                {
                }
            }
        }

        private bool hasUnloadedItems = false;
        public bool HasUnloadedItems
        {
            get => hasUnloadedItems;
            set
            {
                if (SetValue(ref hasUnloadedItems, value))
                {
                }
            }
        }

        private BitmapSource icon;
        public BitmapSource Icon
        {
            get => icon;
            set => SetValue(ref icon, value);
        }

        private SortOrder sortOrder;
        public SortOrder SortOrder
        {
            get => sortOrder;
            set
            {
                SetValue(ref sortOrder, value);
                RecalculateItemsToShow();
                SetItemsSortOrder();
            }
        }

        private SortPriority sortPriority;
        public SortPriority SortPriority
        {
            get => sortPriority;
            set
            {
                SetValue(ref sortPriority, value);
                RecalculateItemsToShow();
                SetItemsSortPriority();
            }
        }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (SetValue(ref title, value))
                {
                    OnTitleChanged();
                }
            }
        }

        private ObservableCollection<HistogramItem> items;
        public ObservableCollection<HistogramItem> Items
        {
            get => items;
            set
            {
                ObservableCollection<HistogramItem> oldValue = items;
                if (SetValue(ref items, value))
                {
                    if (items == null)
                    {
                        ItemsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Items.CollectionChanged -= ItemsCollectionChanged;
                        Items.CollectionChanged += ItemsCollectionChanged;

                        if (oldValue == null)
                            ItemsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Items));
                        else
                            ItemsCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Items, oldValue));
                    }
                }
            }
        }

        private ObservableCollection<HistogramItem> sortedItems;
        public ObservableCollection<HistogramItem> SortedItems
        {
            get => sortedItems;
            set => SetValue(ref sortedItems, value);
        }

        private ObservableCollection<HistogramItem> itemsToShow;
        public ObservableCollection<HistogramItem> ItemsToShow
        {
            get => itemsToShow;
            set
            {
                ObservableCollection<HistogramItem> oldValue = ItemsToShow;
                if (SetValue(ref itemsToShow, value))
                {
                    if (ItemsToShow == null)
                    {
                        SelectedItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SelectedItems.CollectionChanged -= SelectedItemsOnCollectionChanged;
                        SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;

                        if (oldValue == null)
                            SelectedItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ItemsToShow));
                        else
                            SelectedItemsOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ItemsToShow, oldValue));
                    }
                }
            }
        }

        private HistogramItem parent;
        public HistogramItem Parent
        {
            get => parent;
            set
            {
                if (SetValue(ref parent, value))
                {
                    if (Parent != null && !Parent.Items.Contains(this))
                        Parent.Items.Add(this);
                }
            }
        }

        ObservableCollection<HistogramItem> selectedItems = new ObservableCollection<HistogramItem>();
        public ObservableCollection<HistogramItem> SelectedItems
        {
            get => selectedItems;
            set
            {
                if (SetValue(ref selectedItems, value))
                {
                    OnSelectedItemsChanged();
                }
            }
        }

        private object relatedElement;
        public object RelatedElement
        {
            get => relatedElement;
            set => SetValue(ref relatedElement, value);
        }

        private long value;
        public long Value
        {
            get => value;
            set
            {
                if (SetValue(ref this.value, value))
                {
                    OnValueChanged();
                }
            }
        }

        private long maxValue;
        public long MaxValue
        {
            get => maxValue;
            set
            {
                if (SetValue(ref maxValue, value))
                {
                    RecalculateItemsToShow();
                }
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (SetValue(ref isSelected, value))
                {
                    if (IsSelected)
                    {
                        OnPreSelectedItem(this);
                        OnSelected();
                    }
                    else
                    {
                        OnDeselected();
                    }

                    ChangeSelectionAllItems(ItemsToShow, IsSelected);
                }
            }
        }

        private bool canShowAll = true;
        public bool CanShowAll
        {
            get => canShowAll;
            set => SetValue(ref canShowAll, value);
        }

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                bool oldValue = isExpanded;
                if (SetValue(ref isExpanded, value))
                {
                    OnIsExpandedChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isExpanded));
                }
            }
        }

        private bool isLeaf = true;
        public bool IsLeaf
        {
            get => isLeaf;
            set => SetValue(ref isLeaf, value);
        }

        private bool usePrimaryColor = false;
        public bool UsePrimaryColor
        {
            get => usePrimaryColor;
            set
            {
                if (SetValue(ref usePrimaryColor, value))
                {
                }
            }
        }

        #endregion

        #region Methodes

        public HistogramItem()
        {
            Items = new ObservableCollection<HistogramItem>();
            ItemsToShow = new ObservableCollection<HistogramItem>();

            NumberLimitationToShow = 10;
        }

        private void ChangeSelectionAllItems(ObservableCollection<HistogramItem> itemsCollection, bool selectionStatus)
        {
            foreach (HistogramItem item in itemsCollection)
            {
                item.IsSelected = selectionStatus;

                if (item.ItemsToShow.Count > 0)
                {
                    ChangeSelectionAllItems(item.ItemsToShow, selectionStatus);
                }
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnSelectedItemsChanged();
        }

        private bool SetValue<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (property != null)
            {
                if (property.Equals(newValue)) return false;
            }

            property = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HistogramItem item in e.OldItems)
                {
                    item.Parent = null;
                    item.Selected -= ItemOnSelected;
                    item.Deselected -= ItemOnDeselected;
                    item.ValueChanged -= ItemOnValueChanged;
                    item.SelectedItemsChanged -= ItemOnSelectedItemsChanged;
                    item.PreSelectedItem -= OnPreSelectedItem;
                    item.TitleChanged -= Item_TitleChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (HistogramItem item in e.NewItems)
                {
                    item.Parent = this;
                    item.SortOrder = SortOrder;
                    item.SortPriority = SortPriority;

                    item.Selected -= ItemOnSelected;
                    item.Selected += ItemOnSelected;

                    item.Deselected -= ItemOnDeselected;
                    item.Deselected += ItemOnDeselected;

                    item.ValueChanged -= ItemOnValueChanged;
                    item.ValueChanged += ItemOnValueChanged;

                    item.SelectedItemsChanged -= ItemOnSelectedItemsChanged;
                    item.SelectedItemsChanged += ItemOnSelectedItemsChanged;

                    item.PreSelectedItem -= OnPreSelectedItem;
                    item.PreSelectedItem += OnPreSelectedItem;

                    item.TitleChanged -= Item_TitleChanged;
                    item.TitleChanged += Item_TitleChanged;
                }
            }

            IsLeaf = Items.Count == 0;

            if (Items.Count > TotaleItemsCount)
            {
                TotaleItemsCount = Items.Count;
            }

            RecalculateMaxValue();
            RecalculateItemsToShow();
            OnItemsChanged(e);
        }

        private void Item_TitleChanged(object sender, EventArgs e)
        {
            RecalculateItemsToShow();
        }

        private void ItemOnValueChanged(object sender, EventArgs e)
        {
            RecalculateMaxValue();
            RecalculateItemsToShow();
        }

        private void OnPreSelectedItem(object sender, ChangeChildrenToShowCountEventArgs e)
        {
            OnPreSelectedItem(e.SelectedItem);
        }

        private void ItemOnSelectedItemsChanged(object sender, EventArgs e)
        {
            SelectedItems = new ObservableCollection<HistogramItem>(GetAllSelectedItems());
        }

        private void ItemOnSelected(object sender, EventArgs e)
        {
            SelectedItems = new ObservableCollection<HistogramItem>(ItemsToShow.Where(x => x.IsSelected));
        }

        private void ItemOnDeselected(object sender, EventArgs e)
        {
            SelectedItems = new ObservableCollection<HistogramItem>(ItemsToShow.Where(x => x.IsSelected));
        }

        private IEnumerable<HistogramItem> GetAllSelectedItems()
        {
            IEnumerable<HistogramItem> selectedItemsList = ItemsToShow.SelectMany(item => item.SelectedItems);
            return selectedItemsList.Where(item => item.IsLeaf);
        }

        private void RecalculateItemsToShow()
        {
            if (ItemsToShow == null)
                return;

            List<HistogramItem> sortedCollection = ReOrderItemsCollection();

            if (ItemsToShow.Count != 0)
                ItemsToShow.Clear();

            if (sortedCollection.Count >= currentNumberLimitationToShow)
            {
                for (int i = 0; i < currentNumberLimitationToShow; i++)
                {
                    ItemsToShow.Add(sortedCollection[i]);
                }
            }
            else
            {
                foreach (HistogramItem item in sortedCollection)
                {
                    ItemsToShow.Add(item);
                }
            }
        }

        private List<HistogramItem> ReOrderItemsCollection()
        {
            List<HistogramItem> itemsList = Items.ToList();

            switch (SortPriority)
            {
                case SortPriority.Value:
                    itemsList = SortOrder == SortOrder.Descending
                        ? itemsList.OrderByDescending(x => x.Value).ToList()
                        : itemsList.OrderBy(x => x.Value).ToList();
                    break;
                case SortPriority.Title:
                    itemsList = SortOrder == SortOrder.Descending
                        ? itemsList.OrderByDescending(x => x.Title).ToList()
                        : itemsList.OrderBy(x => x.Title).ToList();
                    break;
            }

            SortedItems = new ObservableCollection<HistogramItem>(itemsList);

            return itemsList;
        }

        private void RecalculateMaxValue()
        {
            if (Items?.Count > 0)
                MaxValue = Items.Max(x => x.Value);
            else
                MaxValue = 0;
        }

        private void SetItemsSortOrder()
        {
            if (Items.Count != 0)
            {
                foreach (HistogramItem item in Items)
                {
                    item.SortOrder = SortOrder;
                }
            }
        }

        private void SetItemsSortPriority()
        {
            if (Items.Count != 0)
            {
                foreach (HistogramItem item in Items)
                {
                    item.SortPriority = SortPriority;
                }
            }
        }

        public void ShowAllItems()
        {
            if (ItemsToShow.Count != 0)
                ItemsToShow.Clear();

            foreach (HistogramItem item in SortedItems)
            {
                item.IsSelected = item.Parent.IsSelected;
                ItemsToShow.Add(item);
            }

            currentNumberLimitationToShow = SortedItems.Count;
        }

        public void ShowMoreItems()
        {
            int newCount = ItemsToShow.Count + NumberLimitationToShow;

            if (SortedItems.Count >= newCount)
            {
                for (int i = ItemsToShow.Count; i < newCount; i++)
                {
                    SortedItems[i].IsSelected = SortedItems[i].Parent.IsSelected;
                    ItemsToShow.Add(SortedItems[i]);
                }
            }
            else
            {
                for (int i = ItemsToShow.Count; i < SortedItems.Count; i++)
                {
                    SortedItems[i].IsSelected = SortedItems[i].Parent.IsSelected;
                    ItemsToShow.Add(SortedItems[i]);
                }                
            }

            currentNumberLimitationToShow = newCount;
        }

        public void ShowLessItems()
        {
            int newCount;
            int oldCount = ItemsToShow.Count - 1;
            int mod = ItemsToShow.Count % NumberLimitationToShow;

            if (mod == 0)
            {
                newCount = ItemsToShow.Count - NumberLimitationToShow;
            }
            else
            {
                newCount = ItemsToShow.Count - mod;
            }

            if (newCount < 0)
                return;

            if (newCount == 0)
            {
                newCount = NumberLimitationToShow;
            }

            for (int i = oldCount; i >= newCount; i--)
            {
                ItemsToShow.RemoveAt(i);
            }

            currentNumberLimitationToShow = newCount;
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler SelectedItemsChanged;
        protected void OnSelectedItemsChanged()
        {
            SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler ItemsChanged;
        private void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ItemsChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsExpandedChanged;
        protected void OnIsExpandedChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsExpandedChanged?.Invoke(this, e);
        }

        public event EventHandler<ChangeChildrenToShowCountEventArgs> PreSelectedItem;
        protected void OnPreSelectedItem(HistogramItem selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }

            PreSelectedItem?.Invoke(this, new ChangeChildrenToShowCountEventArgs(selectedItem));
        }

        public event EventHandler ValueChanged;
        protected void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Deselected;
        protected void OnDeselected()
        {
            Deselected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TitleChanged;
        protected void OnTitleChanged()
        {
            TitleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
