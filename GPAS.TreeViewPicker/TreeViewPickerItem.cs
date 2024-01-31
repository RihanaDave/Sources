using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.TreeViewPicker
{
    public class TreeViewPickerItem : INotifyPropertyChanged
    {

        #region Properties
        
        private TreeViewPickerItem parent = null;
        public TreeViewPickerItem Parent
        {
            get { return parent; }
            set
            {
                TreeViewPickerItem oldValue = parent;
                if (SetValue(ref parent, value))
                {
                    if (Parent != null && !Parent.Children.Contains(this))
                        Parent.Children.Add(this);

                    OnParentItemChanged(new RoutedPropertyChangedEventArgs<TreeViewPickerItem>(oldValue, parent));
                }
            }
        }

        private ObservableCollection<TreeViewPickerItem> children = null;
        public ObservableCollection<TreeViewPickerItem> Children
        {
            get { return children; }
            set
            {
                ObservableCollection<TreeViewPickerItem> oldValue = children;
                if (SetValue(ref children, value))
                {
                    if (Children != null)
                    {
                        Children.CollectionChanged -= Children_CollectionChanged;
                        Children.CollectionChanged += Children_CollectionChanged;
                    }

                    RemoveParentForChildren(oldValue);
                    AddParentForChildren(Children);

                    if (oldValue is null)
                        OnChildrenChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children));
                    else
                        OnChildrenChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Children, oldValue));
                }
            }
        }

        private string title = string.Empty;
        public string Title
        {
            get { return title; }
            set
            {
                string oldValue = title;
                if (SetValue(ref title, value))
                {
                    OnTitleChanged(new RoutedPropertyChangedEventArgs<string>(oldValue, title));
                }
            }
        }

        private BitmapSource icon = null;
        public BitmapSource Icon
        {
            get { return icon; }
            set
            {
                var oldValue = icon;
                if (SetValue(ref icon, value))
                {
                    OnIconChanged(new RoutedPropertyChangedEventArgs<BitmapSource>(oldValue, icon));
                }
            }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                var oldValue = isSelected;
                if (SetValue(ref isSelected, value))
                {
                    if (IsSelected)
                        OnSelected();
                    else
                        OnUnSelected();

                    OnIsSelectedChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isSelected));
                }
            }
        }

        private bool isCandidateForSelect = false;
        public bool IsCandidateForSelect
        {
            get { return isCandidateForSelect; }
            set
            {
                var oldValue = isCandidateForSelect;
                if (SetValue(ref isCandidateForSelect, value))
                {
                    if (isCandidateForSelect)
                    {
                        ExpandAllParent(this);
                    }

                    OnIsCandidateForSelectChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isCandidateForSelect));
                }
            }
        }

        private bool isSelectable = true;
        public bool IsSelectable
        {
            get { return isSelectable; }
            set
            {
                var oldValue = isSelectable;
                if (SetValue(ref isSelectable, value))
                {
                    OnIsSelectableChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isSelectable));
                }
            }
        }

        private bool isMatchWithCriteriaSearch = true;
        public bool IsMatchWithCriteriaSearch
        {
            get { return isMatchWithCriteriaSearch; }
            set
            {
                var oldValue = isMatchWithCriteriaSearch;
                if (SetValue(ref isMatchWithCriteriaSearch, value))
                {
                    OnIsMatchWithCriteriaSearchChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isMatchWithCriteriaSearch));
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                var oldValue = isExpanded;
                if (SetValue(ref isExpanded, value))
                {
                    OnIsExpandedChanged(new RoutedPropertyChangedEventArgs<bool>(oldValue, isExpanded));
                }
            }
        }

        private object tag;
        public object Tag
        {
            get { return tag; }
            set
            {
                var oldValue = tag;
                if (SetValue(ref tag, value))
                {
                    OnTagChanged(new RoutedPropertyChangedEventArgs<object>(oldValue, tag));
                }
            }
        }

        public bool IsLeaf => Children == null || Children.Count == 0;

        public bool IsRoot => Parent == null;

        #endregion

        #region Methodes

        public TreeViewPickerItem()
        {
            Children = new ObservableCollection<TreeViewPickerItem>();
            Children.CollectionChanged -= Children_CollectionChanged;
            Children.CollectionChanged += Children_CollectionChanged;
        }

        public bool SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (property != null)
            {
                if (property.Equals(value)) return false;
            }

            property = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                RemoveParentForChildren(e.OldItems);
            }

            if (e.NewItems?.Count > 0)
            {
                AddParentForChildren(e.NewItems);
            }

            OnChildrenChanged(e);
        }

        private void AddParentForChildren(IList children)
        {
            if (children?.Count > 0)
            {
                foreach (TreeViewPickerItem child in children)
                {
                    child.Parent = this;
                }
            }
        }

        private void RemoveParentForChildren(IList children)
        {
            if (children?.Count > 0)
            {
                foreach (TreeViewPickerItem child in children)
                {
                    if (child.Parent.Equals(this))
                        child.Parent = null;
                }
            }
        }

        internal bool CandidateItemForSelectFirstItemMatch(string criteriaSearch)
        {
            TreeViewPickerItem finded = FindFirstSelectabletItemMatch(this, criteriaSearch);

            if (finded != null)
                finded.IsCandidateForSelect = true;

            return finded != null;
        }

        private TreeViewPickerItem FindFirstSelectabletItemMatch(TreeViewPickerItem item, string criteriaSearch)
        {
            TreeViewPickerItem finded = null;
            if (item.IsCriteriaSearchMatched(criteriaSearch))
            {
                finded = item;
            }
            else if(!string.IsNullOrEmpty(criteriaSearch))
            {
                foreach (var child in item.Children)
                {
                    finded = FindFirstSelectabletItemMatch(child, criteriaSearch);
                    if (finded != null)
                        break;
                }
            }

            return finded;
        }

        internal void ApplyCriteriaSearch(string criteriaSearch)
        {
            ApplyCriteriaSearch(criteriaSearch, new List<TreeViewPickerItem>());
        }

        private void ApplyCriteriaSearch(string criteriaSearch, List<TreeViewPickerItem> ancestors)
        {
            if (IsCriteriaSearchMatched(criteriaSearch))
            {
                MatchItemWithCriteriaSearch(criteriaSearch, ancestors);
            }
            else
            {
                IsMatchWithCriteriaSearch = false;
            }

            ManageAncestorListForMatchItemWithCriteriaSearch(criteriaSearch, ancestors);
        }

        private void ManageAncestorListForMatchItemWithCriteriaSearch(string criteriaSearch, List<TreeViewPickerItem> ancestors)
        {
            ancestors.Add(this);
            foreach (var child in Children)
            {
                child.ApplyCriteriaSearch(criteriaSearch, ancestors);
            }

            ancestors.RemoveAt(ancestors.Count - 1);
        }

        private void MatchItemWithCriteriaSearch(string criteriaSearch, List<TreeViewPickerItem> ancestors)
        {
            IsMatchWithCriteriaSearch = true;
            foreach (var ancestor in ancestors)
            {
                ancestor.IsMatchWithCriteriaSearch = true;
                ancestor.IsExpanded = !String.IsNullOrEmpty(criteriaSearch);
            }

            IsExpanded = false;
        }

        internal bool IsCriteriaSearchMatched(string criteriaSearch)
        {
            return String.IsNullOrEmpty(criteriaSearch) || Title.Contains(criteriaSearch);
        }

        internal void ExpandAllParent()
        {
            ExpandAllParent(this);
        }

        private void ExpandAllParent(TreeViewPickerItem treeViewPickerItem)
        {
            if (!treeViewPickerItem.IsRoot)
            {
                ExpandAllParent(treeViewPickerItem.Parent);
                treeViewPickerItem.Parent.IsExpanded = true;
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        public event RoutedPropertyChangedEventHandler<TreeViewPickerItem> ParentItemChanged;
        private void OnParentItemChanged(RoutedPropertyChangedEventArgs<TreeViewPickerItem> e)
        {
            ParentItemChanged?.Invoke(this, e);
        }

        public event NotifyCollectionChangedEventHandler ChildrenChanged;
        private void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            ChildrenChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<string> TitleChanged;
        protected void OnTitleChanged(RoutedPropertyChangedEventArgs<string> e)
        {
            TitleChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<BitmapSource> IconChanged;
        protected void OnIconChanged(RoutedPropertyChangedEventArgs<BitmapSource> e)
        {
            IconChanged?.Invoke(this, e);
        }

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler UnSelected;
        protected void OnUnSelected()
        {
            UnSelected?.Invoke(this, EventArgs.Empty);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsSelectedChanged;
        protected void OnIsSelectedChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsSelectedChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsCandidateForSelectChanged;
        protected void OnIsCandidateForSelectChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsCandidateForSelectChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsSelectableChanged;
        protected void OnIsSelectableChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsSelectableChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsMatchWithCriteriaSearchChanged;
        protected void OnIsMatchWithCriteriaSearchChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsMatchWithCriteriaSearchChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<bool> IsExpandedChanged;
        protected void OnIsExpandedChanged(RoutedPropertyChangedEventArgs<bool> e)
        {
            IsExpandedChanged?.Invoke(this, e);
        }

        public event RoutedPropertyChangedEventHandler<object> TagChanged;
        protected void OnTagChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            TagChanged?.Invoke(this, e);
        }

        #endregion
    }
}
