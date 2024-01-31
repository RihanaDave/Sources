using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventArguments;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    /// <summary>
    /// Interaction logic for ContainerCriteriaControl.xaml
    /// </summary>
    [ContentProperty(nameof(CriteriaItems))]  // Prior to C# 6.0, replace nameof(CriteriaItems) with "CriteriaItems"
    public partial class ContainerFilterSearchControl : BaseFilterSearchControl
    {
        #region Dependencies

        public bool ShowRemoveButton
        {
            get { return (bool)GetValue(ShowRemoveButtonProperty); }
            set { SetValue(ShowRemoveButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowRemoveButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowRemoveButtonProperty =
            DependencyProperty.Register(nameof(ShowRemoveButton), typeof(bool), typeof(ContainerFilterSearchControl), new PropertyMetadata(true));


        public BooleanOperator SelectedBooleanOperator
        {
            get { return (BooleanOperator)GetValue(SelectedBooleanOperatorProperty); }
            set { SetValue(SelectedBooleanOperatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBooleanOperator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBooleanOperatorProperty =
            DependencyProperty.Register(nameof(SelectedBooleanOperator), typeof(BooleanOperator), typeof(ContainerFilterSearchControl),
                new PropertyMetadata(BooleanOperator.All, OnSetSelectedBooleanOperatorChanged));

        private static void OnSetSelectedBooleanOperatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerFilterSearchControl)d).OnSetSelectedBooleanOperatorChanged(e);
        }

        private void OnSetSelectedBooleanOperatorChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSelectedBooleanOperatorChanged();
        }

        public ObservableCollection<CriteriaBase> CriteriaItems
        {
            get { return (ObservableCollection<CriteriaBase>)GetValue(CriteriaItemsProperty); }
            set { SetValue(CriteriaItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Criterias.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriteriaItemsProperty =
            DependencyProperty.Register(nameof(CriteriaItems), typeof(ObservableCollection<CriteriaBase>), typeof(ContainerFilterSearchControl),
                new PropertyMetadata(null, OnSetCriteriaItemsChanged));

        private static void OnSetCriteriaItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerFilterSearchControl)d).OnSetCriteriaItemsChanged(e);
        }

        private void OnSetCriteriaItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            CriteriaItems.CollectionChanged -= CriteriaItems_CollectionChanged;
            CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;

            CriteriaItemsPrimitiveInit((IList)e.NewValue, (IList)e.OldValue, NotifyCollectionChangedAction.Reset);
        }

        protected ObservableCollection<BaseFilterSearchControl> CriteriaControlItems
        {
            get { return (ObservableCollection<BaseFilterSearchControl>)GetValue(CriteriaControlItemsProperty); }
            set { SetValue(CriteriaControlItemsProperty, value); }
        }

        protected static readonly DependencyProperty CriteriaControlItemsProperty =
            DependencyProperty.Register(nameof(CriteriaControlItems), typeof(ObservableCollection<BaseFilterSearchControl>), typeof(ContainerFilterSearchControl),
                new PropertyMetadata(null, OnSetCriteriaControlItemsChanged));

        private static void OnSetCriteriaControlItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerFilterSearchControl)d).OnSetCriteriaControlItemsChanged(e);
        }

        private void OnSetCriteriaControlItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            CriteriaControlItems.CollectionChanged -= CriteriaControlItems_CollectionChanged;
            CriteriaControlItems.CollectionChanged += CriteriaControlItems_CollectionChanged;

            CriteriaControlItemsPrimitiveInit((IList)e.NewValue, (IList)e.OldValue, NotifyCollectionChangedAction.Reset);
        }

        public BaseFilterSearchControl SelectedCriteriaItem
        {
            get { return (BaseFilterSearchControl)GetValue(SelectedCriteriaItemProperty); }
            set { SetValue(SelectedCriteriaItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedCriteriaItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCriteriaItemProperty =
            DependencyProperty.Register(nameof(SelectedCriteriaItem), typeof(BaseFilterSearchControl), typeof(ContainerFilterSearchControl),
                new PropertyMetadata(null, OnSetSelectedCriteriaItemChanged));

        private static void OnSetSelectedCriteriaItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerFilterSearchControl)d).OnSetSelectedCriteriaItemChanged(e);
        }

        private void OnSetSelectedCriteriaItemChanged(DependencyPropertyChangedEventArgs e)
        {
            SetSelectedItemForParentControl();

            OnSelectedCriteriaItemChanged();
        }

        #endregion

        #region events
        public event EventHandler<BooleanOperatorEventArgs> SelectedBooleanOperatorChanged;
        protected virtual void OnSelectedBooleanOperatorChanged()
        {
            SelectedBooleanOperatorChanged?.Invoke(this, new BooleanOperatorEventArgs(SelectedBooleanOperator));
        }

        public event EventHandler<CriteriaItemsChangedEventArgs> CriteriaItemsChanged;
        protected void OnCriteriaItemsChanged(CriteriaItemsChangedEventArgs args)
        {
            CriteriaItemsChanged?.Invoke(this, args);
        }

        public event EventHandler SelectedCriteriaItemChanged;
        protected void OnSelectedCriteriaItemChanged()
        {
            SelectedCriteriaItemChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Variables

        ContainerCriteria containerCriteria;

        #endregion

        #region Methodes

        public ContainerFilterSearchControl()
        {
            InitializeComponent();
            Init();
        }

        public void RemoveAllItems()
        {
            while (CriteriaItems.Count > 0)
            {
                CriteriaItems.RemoveAt(0);
            }
        }

        protected override void AfterSetCriteriaBase()
        {
            containerCriteria = CriteriaBase as ContainerCriteria;
            containerCriteria.CriteriaSet.Criterias = CriteriaItems;

            Binding SelectedBooleanOperatorBinding = new Binding(nameof(containerCriteria.CriteriaSet.SetOperator));
            SelectedBooleanOperatorBinding.Source = containerCriteria.CriteriaSet;
            SelectedBooleanOperatorBinding.Mode = BindingMode.TwoWay;
            SetBinding(SelectedBooleanOperatorProperty, SelectedBooleanOperatorBinding);
        }

        protected override void SetValidationStatus()
        {
            if (CriteriaControlItems?.Count > 0)
            {
                foreach (var item in CriteriaControlItems)
                {
                    if (item.ValidationStatus == ValidationStatus.Invalid)
                    {
                        ValidationStatus = ValidationStatus.Invalid;
                        return;
                    }
                }

                ValidationStatus = ValidationStatus.Valid;
            }
            else
            {
                ValidationStatus = ValidationStatus.Invalid;
            }
        }

        internal void SetSelectedCriteiraItem(BaseFilterSearchControl selectedCriteriaControl)
        {
            ContainerFilterSearchControl root = GetRoot(this);
            DeSelectAllCriteriaItems(root, selectedCriteriaControl);
            SelectedCriteriaItem = selectedCriteriaControl;
        }

        private void Init()
        {
            DataContext = this;

            CriteriaControlItems = new ObservableCollection<BaseFilterSearchControl>();
            CriteriaControlItems.CollectionChanged -= CriteriaControlItems_CollectionChanged;
            CriteriaControlItems.CollectionChanged += CriteriaControlItems_CollectionChanged;

            CriteriaItems = new ObservableCollection<CriteriaBase>();
            CriteriaItems.CollectionChanged -= CriteriaItems_CollectionChanged;
            CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;

            if (CriteriaBase == null)
            {
                CriteriaBase = new ContainerCriteria();
            }
        }

        private void DeSelectAllCriteriaItems(ContainerFilterSearchControl parent, BaseFilterSearchControl exceptCriteriaControl)
        {
            if (parent == null)
                return;

            if (parent != exceptCriteriaControl)
                parent.IsSelected = false;

            if (parent.CriteriaControlItems?.Count > 0)
            {
                foreach (var item in parent.CriteriaControlItems)
                {
                    if (item is ContainerFilterSearchControl)
                    {
                        DeSelectAllCriteriaItems(item as ContainerFilterSearchControl, exceptCriteriaControl);
                    }
                    else
                    {
                        if (item != exceptCriteriaControl)
                            item.IsSelected = false;
                    }
                }
            }
        }

        private void CriteriaControl_ValidationStatusChanged(object sender, ValidationStatusChangedEventArgs e)
        {
            SetValidationStatus();
        }

        private void BooleanOperaorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedBooleanOperator = (BooleanOperator)BooleanOperaorCombobox.SelectedIndex;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked();
        }

        private void CriteriaItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CriteriaItemsPrimitiveInit(e.NewItems, e.OldItems, e.Action);
        }

        private void CriteriaItemsPrimitiveInit(IList newItems, IList oldItems, NotifyCollectionChangedAction action)
        {
            if (action == NotifyCollectionChangedAction.Reset)
            {
                CriteriaControlItems.Clear();
            }

            if (oldItems?.Count > 0)
            {
                foreach (var item in oldItems)
                {
                    if (item is CriteriaBase)
                    {
                        CriteriaBase criteria = (CriteriaBase)item;
                        var criteriaControl = GetCriteriaControlByCriteriaBase(criteria);
                        if (criteriaControl != null)
                            CriteriaControlItems.Remove(criteriaControl);
                    }
                }
            }

            if (newItems?.Count > 0)
            {
                foreach (var item in newItems)
                {
                    if (item is CriteriaBase)
                    {
                        BaseFilterSearchControl criteriaControl = null;
                        if (item is KeywordCriteria)
                            criteriaControl = new KeywordFilterSearchControl();
                        else if (item is ContainerCriteria)
                            criteriaControl = new ContainerFilterSearchControl();
                        else if (item is DateRangeCriteria)
                            criteriaControl = new DateFilterSearchControl();
                        else if (item is ObjectTypeCriteria)
                            criteriaControl = new ObjectTypeFilterSearchControl();
                        else if (item is PropertyValueCriteria)
                            criteriaControl = new PropertyFilterSearchControl();
                        else
                            throw new NotSupportedException();

                        criteriaControl.CriteriaBase = item as CriteriaBase;
                        CriteriaControlItems.Add(criteriaControl);
                    }
                }
            }

            OnCriteriaItemsChanged(new CriteriaItemsChangedEventArgs(oldItems, newItems));
        }

        private BaseFilterSearchControl GetCriteriaControlByCriteriaBase(CriteriaBase criteria)
        {
            foreach (BaseFilterSearchControl item in CriteriaControlItems)
            {
                if (item.CriteriaBase == criteria)
                    return item;
            }

            return null;
        }

        private void CriteriaControlItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CriteriaControlItemsPrimitiveInit(e.NewItems, e.OldItems, e.Action);
        }

        private void CriteriaControlItemsPrimitiveInit(IList newItems, IList oldItems, NotifyCollectionChangedAction action)
        {
            if (action == NotifyCollectionChangedAction.Reset)
            {
                ContainerStackPanel.Children.Clear();
            }

            if (oldItems?.Count > 0)
            {
                foreach (var item in oldItems)
                {
                    if (item is BaseFilterSearchControl)
                    {
                        BaseFilterSearchControl criteriaControl = (BaseFilterSearchControl)item;
                        ContainerStackPanel.Children.Remove(criteriaControl);
                        criteriaControl.ParentContainerControl = null;

                        criteriaControl.ValidationStatusChanged -= CriteriaControl_ValidationStatusChanged;
                    }
                }
            }

            if (newItems?.Count > 0)
            {
                foreach (var item in newItems)
                {
                    if (item is BaseFilterSearchControl)
                    {
                        BaseFilterSearchControl criteriaControl = (BaseFilterSearchControl)item;
                        ContainerStackPanel.Children.Add(criteriaControl);
                        criteriaControl.ParentContainerControl = this;
                        criteriaControl.IsSelected = true;

                        criteriaControl.ValidationStatusChanged -= CriteriaControl_ValidationStatusChanged;
                        criteriaControl.ValidationStatusChanged += CriteriaControl_ValidationStatusChanged;
                    }
                }
            }

            SetValidationStatus();
        }

        private void SetSelectedItemForParentControl()
        {
            if (ParentContainerControl != null)
                ParentContainerControl.SelectedCriteriaItem = SelectedCriteriaItem;
        }

        public void Reset()
        {
            RemoveAllItems();
            BooleanOperaorCombobox.SelectedIndex = 0;
            IsSelected = true;
        }

        #endregion
    }
}
