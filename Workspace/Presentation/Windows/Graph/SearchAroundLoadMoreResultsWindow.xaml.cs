using System;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public sealed partial class SearchAroundLoadMoreResultsWindow : Window
    {
        #region Properties

        public int PrimitiveNumber { get; set; }
        public int SecondaryNumber { get; set; }
        public int ThirdNumber { get; set; }
        public int MaximumNumber { get; set; }
        public int ResultWindow = 0;

        #endregion

        #region Methods

        public SearchAroundLoadMoreResultsWindow()
        {
            InitializeComponent();
        }

        public void Init(int primitiveNumber, int secondaryNumber, int thirdNumber, int maximumNumber)
        {
            PrimitiveNumber = primitiveNumber;
            SecondaryNumber = secondaryNumber;
            ThirdNumber = thirdNumber;
            MaximumNumber = maximumNumber;
            textBlockMessage.Text = String.Format(@"For each searched object, a maximum of {0} results are loaded and there are more results to display.",
                                                    primitiveNumber);
            SecondaryNumberButton.Content = String.Format(@"Yes! Load up to {0} results.", secondaryNumber);
            ThirdNumberButton.Content = String.Format(@"Yes! Load up to {0} results.", thirdNumber);
            MaximumNumberButton.Content = String.Format(@"Yes! Load all results. (Up to {0} results)", maximumNumber);

            ResultEvent += SearchAroundLoadMoreResultsWindow_Result;
        }

        private void PrimitiveNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new SearchAroundLoadMoreResultsWindowResultEventArgs(PrimitiveNumber));
        }

        private void SecondaryNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new SearchAroundLoadMoreResultsWindowResultEventArgs(SecondaryNumber));
        }

        private void ThirdNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new SearchAroundLoadMoreResultsWindowResultEventArgs(ThirdNumber));
        }

        private void MaximumNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new SearchAroundLoadMoreResultsWindowResultEventArgs(MaximumNumber));
        }

        private void SearchAroundLoadMoreResultsWindow_Result(object sender, SearchAroundLoadMoreResultsWindowResultEventArgs e)
        {
            ResultWindow = e.Result;
            Close();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
            // Begin dragging the window
            DragMove();
        }

        #endregion

        #region Events

        private event EventHandler<SearchAroundLoadMoreResultsWindowResultEventArgs> ResultEvent;

        #endregion
    }
}
