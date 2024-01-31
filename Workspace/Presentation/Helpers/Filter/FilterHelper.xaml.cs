using GPAS.FilterSearch;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias;
using GPAS.Workspace.Presentation.Helpers.Filter.EventsArgs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Filter
{
    public partial class FilterHelper : PresentationHelper
    {
        #region Dependencies

        public bool ShowAllComponent
        {
            get { return (bool)GetValue(ShowAllComponentProperty); }
            set { SetValue(ShowAllComponentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowAllComponent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowAllComponentProperty =
            DependencyProperty.Register(nameof(ShowAllComponent), typeof(bool), typeof(FilterHelper), new PropertyMetadata(true));

        private ContainerFilterSearchControl SelectedContainerCriteriaControl;

        #endregion

        #region Events

        public event EventHandler<FilterAppliedEventArgs> FilterApplied;
        protected void OnFilterApplied(FilterAppliedEventArgs args)
        {
            FilterApplied?.Invoke(this, args);
        }

        public event EventHandler FilterCleared;
        protected void OnFilterCleared()
        {
            FilterCleared?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<FilterSearchCompletedEventArgs> FilterSearchCompleted;

        protected void OnFilterSearchCompleted(FilterSearchCompletedEventArgs args)
        {
            FilterSearchCompleted?.Invoke(this, args);
        }

        public event EventHandler<FilterAppliedEventArgs> FilterAppliedAsSelection;
        protected void OnFilterAppliedAsSelection(FilterAppliedEventArgs args)
        {
            FilterAppliedAsSelection?.Invoke(this, args);
        }

        #endregion

        #region Private Variables

        private FilterHelperVM FilterHelperVM;

        #endregion

        #region Public Methodes

        public FilterHelper()
        {
            InitializeComponent();
            Init();
        }

        public override void Reset()
        {
            if (FilterHelperVM != null)
            {
                FilterHelperVM.CountOfResult = 10;
                FilterHelperVM.ClearFilter();
            }

            ClearAll();
        }

        #endregion

        #region Private Methodes
        private void Init()
        {
            SelectedContainerCriteriaControl = MainCriteriaControl;

            BindProperties();
        }

        private void BindProperties()
        {
            FilterHelperVM = (FilterHelperVM)DataContext;

            FilterHelperVM.FilterSearchCompleted -= FilterHelperVM_SearchCompleted;
            FilterHelperVM.FilterSearchCompleted += FilterHelperVM_SearchCompleted;

            FilterHelperVM.FilterApplied -= FilterHelperVM_FilterApplied;
            FilterHelperVM.FilterApplied += FilterHelperVM_FilterApplied;

            FilterHelperVM.FilterCleared -= FilterHelperVM_FilterCleared;
            FilterHelperVM.FilterCleared += FilterHelperVM_FilterCleared;

            FilterHelperVM.FilterAppliedAsSelection -= FilterHelperVM_FilterAppliedAsSelection;
            FilterHelperVM.FilterAppliedAsSelection += FilterHelperVM_FilterAppliedAsSelection;

            FilterHelperVM.UnhandledException -= FilterHelperVM_UnhandledException;
            FilterHelperVM.UnhandledException += FilterHelperVM_UnhandledException;

            Binding ShowAllComponentBinding = new Binding(nameof(ShowAllComponent));
            ShowAllComponentBinding.Source = FilterHelperVM;
            ShowAllComponentBinding.Mode = BindingMode.TwoWay;
            SetBinding(ShowAllComponentProperty, ShowAllComponentBinding);

            Binding ValidationStatusBinding = new Binding(nameof(FilterHelperVM.ValidationStatus));
            ValidationStatusBinding.Source = FilterHelperVM;
            ValidationStatusBinding.Mode = BindingMode.TwoWay;
            MainCriteriaControl.SetBinding(ContainerFilterSearchControl.ValidationStatusProperty, ValidationStatusBinding);

            Binding SelectedBooleanOperatorBinding = new Binding(nameof(FilterHelperVM.CriteriaSet.SetOperator));
            SelectedBooleanOperatorBinding.Source = FilterHelperVM.CriteriaSet;
            SelectedBooleanOperatorBinding.Mode = BindingMode.TwoWay;
            MainCriteriaControl.SetBinding(ContainerFilterSearchControl.SelectedBooleanOperatorProperty, SelectedBooleanOperatorBinding);

            Binding CriteriasBinding = new Binding(nameof(FilterHelperVM.CriteriaSet.Criterias));
            CriteriasBinding.Source = FilterHelperVM.CriteriaSet;
            CriteriasBinding.Mode = BindingMode.TwoWay;
            MainCriteriaControl.SetBinding(ContainerFilterSearchControl.CriteriaItemsProperty, CriteriasBinding);
        }

        private void FilterHelperVM_FilterCleared(object sender, EventArgs e)
        {
            OnFilterCleared();
        }

        private void FilterHelperVM_FilterAppliedAsSelection(object sender, FilterAppliedEventArgs e)
        {
            OnFilterAppliedAsSelection(e);
        }

        private void FilterHelperVM_FilterApplied(object sender, FilterAppliedEventArgs e)
        {
            OnFilterApplied(e);
        }

        private void FilterHelperVM_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void FilterHelperVM_SearchCompleted(object sender, FilterSearchCompletedEventArgs e)
        {
            OnFilterSearchCompleted(e);

            if (e.Result.Count() == 0)
            {
                ShowNotification(Properties.Resources.There_is_no_result_for_this_query, Properties.Resources.Filter_Search);
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            MainCriteriaControl.Reset();
            SelectedContainerCriteriaControl = MainCriteriaControl;
        }

        private void MainCriteriaControl_SelectedCriteriaItemChanged(object sender, EventArgs e)
        {
            if (MainCriteriaControl.SelectedCriteriaItem is ContainerFilterSearchControl)
            {
                SelectedContainerCriteriaControl = MainCriteriaControl.SelectedCriteriaItem as ContainerFilterSearchControl;
            }
            else
            {
                if (MainCriteriaControl.SelectedCriteriaItem.ParentContainerControl == null)
                {
                    SelectedContainerCriteriaControl = MainCriteriaControl;
                }
                else
                {
                    SelectedContainerCriteriaControl = MainCriteriaControl.SelectedCriteriaItem.ParentContainerControl;
                }
            }
        }

        #endregion

        private void KeywordFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContainerCriteriaControl == null)
                SelectedContainerCriteriaControl = MainCriteriaControl;

            SelectedContainerCriteriaControl.CriteriaItems.Add(new KeywordCriteria());
        }

        private void PropertyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContainerCriteriaControl == null)
                SelectedContainerCriteriaControl = MainCriteriaControl;

            SelectedContainerCriteriaControl.CriteriaItems.Add(new PropertyValueCriteria());
        }

        private void ObjectTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContainerCriteriaControl == null)
                SelectedContainerCriteriaControl = MainCriteriaControl;

            SelectedContainerCriteriaControl.CriteriaItems.Add(new ObjectTypeCriteria());
        }

        private void DateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContainerCriteriaControl == null)
                SelectedContainerCriteriaControl = MainCriteriaControl;

            SelectedContainerCriteriaControl.CriteriaItems.Add(new DateRangeCriteria());
        }

        private void NestedFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedContainerCriteriaControl == null)
                SelectedContainerCriteriaControl = MainCriteriaControl;

            SelectedContainerCriteriaControl.CriteriaItems.Add(new ContainerCriteria());
        }
    }
}