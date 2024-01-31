using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Helpers.Filter.EventsArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Helpers.Filter
{
    public class FilterHelperVM : PresentationHelperViewModel
    {
        #region Properties

        private bool showAllComponent = true;
        public bool ShowAllComponent
        {
            get { return showAllComponent; }
            set { SetValue(ref showAllComponent, value); }
        }

        private CriteriaSet criteriaSet = new CriteriaSet();
        public CriteriaSet CriteriaSet
        {
            get { return criteriaSet; }
            set { SetValue(ref criteriaSet, value); }
        }

        private int countOfResult = 10;
        public int CountOfResult
        {
            get { return countOfResult; }
            set { SetValue(ref countOfResult, value); }
        }

        private ValidationStatus validationStatus = ValidationStatus.Invalid;
        public ValidationStatus ValidationStatus
        {
            get { return validationStatus; }
            set { SetValue(ref validationStatus, value); }
        }

        private BooleanOperator selectedBooleanOperator;
        public BooleanOperator SelectedBooleanOperator
        {
            get { return selectedBooleanOperator; }
            set { SetValue(ref selectedBooleanOperator, value); }
        }

        public RelayCommand SearchCommand { get; set; }
        public RelayCommand ApllyAsFilterCommand { get; set; }
        public RelayCommand ClearFilterCommand { get; set; }
        public RelayCommand SelectMatchingCommand { get; set; }

        #endregion

        #region Public Methodes

        public FilterHelperVM()
        {
            SearchCommand = new RelayCommand(Search, CanExcuteSearch);
            ApllyAsFilterCommand = new RelayCommand(ApllyAsFilter, CanExcuteApllyAsFilter);
            SelectMatchingCommand = new RelayCommand(SelectMatching, CanExcuteSelectMatching);
            ClearFilterCommand = new RelayCommand(ClearFilter);
        }

        public void ClearFilter()
        {
            OnFilterCleared();
        }

        #endregion

        #region Events

        public event EventHandler<FilterSearchCompletedEventArgs> FilterSearchCompleted;
        protected void OnFilterSearchCompleted(FilterSearchCompletedEventArgs args)
        {
            FilterSearchCompleted?.Invoke(this, args);
        }

        public event EventHandler<FilterAppliedEventArgs> FilterApplied;
        protected void OnFilterApplied(FilterAppliedEventArgs args)
        {
            FilterApplied?.Invoke(this, args);
        }

        public event EventHandler<FilterAppliedEventArgs> FilterAppliedAsSelection;
        protected void OnFilterAppliedAsSelection(FilterAppliedEventArgs args)
        {
            FilterAppliedAsSelection?.Invoke(this, args);
        }

        public event EventHandler FilterCleared;
        protected void OnFilterCleared()
        {
            FilterCleared?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methodes

        private async void Search(object parameter)
        {
            try
            {
                Query query = new Query()
                {
                    CriteriasSet = CriteriaSet
                };

                WaitingMessage = Properties.Resources.Performing_FilterSearch_;
                ShowWaiting = true;
                IEnumerable<KWObject> filterSearchResults = await PerformFilterSearch(query, CountOfResult);
                ShowWaiting = false;
                OnFilterSearchCompleted(new FilterSearchCompletedEventArgs(filterSearchResults));
            }
            catch (Exception ex)
            {
                Exception exception = new Exception(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message), ex);
                UnhandledExceptionEventArgs args = new UnhandledExceptionEventArgs(exception, false);
                OnUnhandledException(args);
            }
            finally
            {
                ShowWaiting = false;
            }
        }

        private async Task<IEnumerable<KWObject>> PerformFilterSearch(Query query, int count)
        {
            return await Logic.Search.FilterSearch.PerformFilterSearchAsync(query, count.ToString());
        }

        private bool CanExcuteSearch(object parameter)
        {
            return ValidationStatus != ValidationStatus.Invalid;
        }

        private void ApllyAsFilter(object parameter)
        {
            Query query = new Query()
            {
                CriteriasSet = CriteriaSet
            };

            OnFilterApplied(new FilterAppliedEventArgs(query));
        }

        private bool CanExcuteApllyAsFilter(object parameter)
        {
            return ValidationStatus != ValidationStatus.Invalid;
        }

        private void SelectMatching(object parameter)
        {
            Query query = new Query()
            {
                CriteriasSet = CriteriaSet
            };

            OnFilterAppliedAsSelection(new FilterAppliedEventArgs(query));
        }

        private bool CanExcuteSelectMatching(object parameter)
        {
            return ValidationStatus != ValidationStatus.Invalid;
        }

        private void ClearFilter(object parameter)
        {
            ClearFilter();
        }

        #endregion
    }
}
