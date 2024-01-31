using System;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public partial class UnmergeMoreLinksWindow : Window
    {
        #region Properties

        public int PrimitiveNumber { get; set; }
        public int SecondaryNumber { get; set; }
        public int ThirdNumber { get; set; }
        public int MaximumNumber { get; set; }
        public int ResultWindow = 0;

        #endregion

        #region Methods 

        public UnmergeMoreLinksWindow()
        {
            InitializeComponent();           
        }

        public void Init(int primitiveNumber, int secondaryNumber, int thirdNumber, int maximumNumber)
        {
            PrimitiveNumber = primitiveNumber;
            SecondaryNumber = secondaryNumber;
            ThirdNumber = thirdNumber;
            MaximumNumber = maximumNumber;
            textBlockMessage.Text = String.Format(@"{0} links have been unmerged from links between two objects and there are more links to unmerge.",
                                                    primitiveNumber);
            SecondaryNumberButton.Content = String.Format(@"Yes! Unmerge up to {0} links.", secondaryNumber);
            ThirdNumberButton.Content = String.Format(@"Yes! Unmerge up to {0} links.", thirdNumber);
            MaximumNumberButton.Content = String.Format(@"Yes! Unmerge all links (Up to {0} links)", maximumNumber);
            ResultWindow = 0;

            ResultEvent += UnmergeMoreLinksWindow_ResultEvent;
        }

        private void UnmergeMoreLinksWindow_ResultEvent(object sender, UnmergeMoreLinksWindowEventArgs e)
        {
            ResultWindow = e.Result;
            Close();
        }

        private void PrimitiveNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new UnmergeMoreLinksWindowEventArgs(PrimitiveNumber));
        }

        private void SecondaryNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new UnmergeMoreLinksWindowEventArgs(SecondaryNumber));
        }

        private void ThirdNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new UnmergeMoreLinksWindowEventArgs(ThirdNumber));
        }

        private void MaximumNumberButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent.Invoke(this, new UnmergeMoreLinksWindowEventArgs(MaximumNumber));
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        #endregion

        #region Events 

        private event EventHandler<UnmergeMoreLinksWindowEventArgs> ResultEvent;

        #endregion
    }
}
