using Gpas.Pagination.Events;
using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using GPAS.Workspace.Presentation.Controls.Sesrch.ViewModel;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.View
{
    /// <summary>
    /// Interaction logic for SearchResultUserControl.xaml
    /// </summary>
    public partial class SearchResultUserControl : UserControl
    {
        public SearchResultUserControl()
        {
            InitializeComponent();
        }

        #region Events

        public event EventHandler SortEvent;
        protected void OnSortEvent()
        {
            SortEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SortByAscOrDescEvent;
        protected void OnSortByAscOrDescEvent()
        {
            SortByAscOrDescEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<long> ShowObjectOnBrowser;

        protected void OnShowObjectOnBrowser(long objrctId)
        {
            ShowObjectOnBrowser?.Invoke(this, objrctId);
        }

        #endregion

        #region Dependency
        public bool DisplayAdvance
        {
            get { return (bool)GetValue(DisplayAdvanceProperty); }
            set { SetValue(DisplayAdvanceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayAdvance.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayAdvanceProperty =
            DependencyProperty.Register(nameof(DisplayAdvance), typeof(bool), typeof(SearchResultUserControl), new PropertyMetadata(false));

        public SearchState State
        {
            get { return (SearchState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(SearchState), typeof(SearchResultUserControl), new PropertyMetadata(SearchState.All));

        #endregion

        private void TextSearchHomeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TextSearchHomeTextBox.Text != string.Empty)
                {
                    OnSortEvent();
                }
            }
        }

        private void AdvancedSearchButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAdvance = true;
        }

        private void SortButtonAcsOrDes_Click(object sender, RoutedEventArgs e)
        {
            OnSortByAscOrDescEvent();
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextSearchHomeTextBox.Text != string.Empty)
            {
                DisplayAdvance = false;
                OnSortEvent();
            }
        }

        private void AdvanceSearchControl_BackRequest(object sender, EventArgs e)
        {
            DisplayAdvance = false;           
        }

        private void AllSearch_Click(object sender, EventArgs e)
        {
            DisplayAdvance = false;
            OnSortEvent();
        }
      
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

      
        public event EventHandler<PaginationEventHandler> ItemPerPageChanged;
        protected void OnItemPerPageChanged(PaginationEventHandler e)
        {
            ItemPerPageChanged?.Invoke(this, e);
        }
           public event EventHandler<PaginationEventHandler> PageNumberChanged;

        protected void OnPageNumberChanged(PaginationEventHandler e)
        {
            PageNumberChanged?.Invoke(this, e);
        }

        private void PaginationUserControl_PageNumberChange(object sender, PaginationEventHandler e)
        {
            OnPageNumberChanged(e);
        }

        private void PaginationUserControl_ItemPerPageChanged(object sender, PaginationEventHandler e)
        {
            OnItemPerPageChanged(e);
        }

        private void AllSearch_Click(object sender, RoutedEventArgs e)
        {
            if (TextSearchHomeTextBox.Text != string.Empty)
            {
                OnSortEvent();
            }
        }

        private void AllSearchButton_Click(object sender, RoutedEventArgs e)
        {
            State = SearchState.All;
            ((SearchViewModel)DataContext).SearchState = SearchState.All;
        }

        private void DocSearchButton_Click(object sender, RoutedEventArgs e)
        {
            State = SearchState.TextDoc;
            ((SearchViewModel)DataContext).SearchState = SearchState.TextDoc;
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TextSearchHomeTextBox.Text != string.Empty)
            {
                OnSortEvent();
            }
        }

        private void FileSizeFromTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9]+");
            e.Handled = reg.IsMatch(e.Text);
        }

        private void FileSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "0";
            }
        }

        private void OpenWithViewerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnShowObjectOnBrowser(((SearchResultModel)ResultList.SelectedItem).Id);
        }
    }
}