using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using GPAS.Workspace.Presentation.Controls.Sesrch.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.View
{
    /// <summary>
    /// Interaction logic for BasicSearchControl.xaml
    /// </summary>
    public partial class BasicSearchControl : UserControl
    {
        #region Dependencies
        public bool DisplayAdvance
        {
            get { return (bool)GetValue(DisplayAdvanceProperty); }
            set { SetValue(DisplayAdvanceProperty, value); }
        }
       
        public static readonly DependencyProperty DisplayAdvanceProperty =
            DependencyProperty.Register(nameof(DisplayAdvance), typeof(bool), typeof(BasicSearchControl), new PropertyMetadata(false));


        public SearchState State
        {
            get { return (SearchState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(SearchState), typeof(BasicSearchControl), new PropertyMetadata(SearchState.All));

        #endregion

        #region Methods

        public BasicSearchControl()
        {
            InitializeComponent();
        }

        private void TextSearchHomeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TextSearchHomeTextBox.Text != string.Empty)
                {
                    OnSearchAllRequest();
                }
            }
        }

        private void AdvanceSearchControl_BackRequest(object sender, EventArgs e)
        {
            DisplayAdvance = false;
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

        private void AdvancedSearchButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAdvance = true;
        }

        #endregion

        #region Event

        /// <summary>
        /// رویداد جستوجو
        /// </summary>
        public event EventHandler<EventArgs> SearchAllRequest;
        protected void OnSearchAllRequest()
        {
            SearchAllRequest?.Invoke(this, new EventArgs());
        }

        #endregion

        private void AllSearch_Click(object sender, EventArgs e)
        {
            //if (TextSearchHomeTextBox.Text != string.Empty)
            //{
            OnSearchAllRequest();
            //}
        }

        private void AllSearchTextSearchButtonClick(object sender, RoutedEventArgs e)
        {
            if (TextSearchHomeTextBox.Text != string.Empty)
            {
                OnSearchAllRequest();
            }
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TextSearchHomeTextBox.Text != string.Empty)
            {
                OnSearchAllRequest();
            }
        }
    }
}
