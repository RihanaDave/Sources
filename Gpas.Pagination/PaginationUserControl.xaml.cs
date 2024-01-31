using Gpas.Pagination.Events;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Gpas.Pagination
{
    /// <summary>
    /// Interaction logic for PaginationUserControl.xaml
    /// </summary>
    public partial class PaginationUserControl : UserControl
    {
        public PaginationUserControl()
        {
            InitializeComponent();

        }
        #region DependencyProperty  


        public long TotalNumberCollection
        {
            get { return (long)GetValue(TotalNumberCollectionProperty); }
            set { SetValue(TotalNumberCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalNumberCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalNumberCollectionProperty =
            DependencyProperty.Register(nameof(TotalNumberCollection), typeof(long), typeof(PaginationUserControl),
                new PropertyMetadata((long)1, OnSetTotalNumberCollectionChanged));



        private static void OnSetTotalNumberCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaginationUserControl)d).OnSetTotalNumberCollectionChanged(e);
        }

        private void OnSetTotalNumberCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            SetTotalPageCount();
            SetEnableButtons();
        }

        public long TotalPageCount
        {
            get { return (long)GetValue(TotalPageCountProperty); }
            protected set { SetValue(TotalPageCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalPageCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalPageCountProperty =
            DependencyProperty.Register(nameof(TotalPageCount), typeof(long), typeof(PaginationUserControl),
                new PropertyMetadata((long)1));


        public int ItemPerPage
        {
            get { return (int)GetValue(ItemPerPageProperty); }
            set { SetValue(ItemPerPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemPerPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemPerPageProperty =
            DependencyProperty.Register(nameof(ItemPerPage), typeof(int), typeof(PaginationUserControl), new PropertyMetadata(10, OnSetItemPerPageChanged));
        private static void OnSetItemPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaginationUserControl)d).OnSetItemPerPageChanged(e);
        }

        private void OnSetItemPerPageChanged(DependencyPropertyChangedEventArgs e)
        {
            SetTotalPageCount();
            long pn = (long)Math.Floor(SelectedIndex / (double)ItemPerPage);
            if (pn < 0)
                pn = 0;
            if (pn == PageNumber)
            {
                PageNumber = pn;
                SetEnableButtons();
            }
            else
            {
                PageNumber = pn;
            }

            OnItemPerPageChanged(new PaginationEventHandler(PageNumber, ItemPerPage));
        }

        public long PageNumber
        {
            get { return (long)GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register(nameof(PageNumber), typeof(long), typeof(PaginationUserControl),
                new PropertyMetadata((long)0, OnSetPageNumberChanged));

        private static void OnSetPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaginationUserControl)d).OnSetPageNumberChanged(e);
        }

        private void OnSetPageNumberChanged(DependencyPropertyChangedEventArgs e)
        {
            SetEnableButtons();
        }

        private void SetEnableButtons()
        {
            IsEnableFirstPage = IsEnablePrevPage = PageNumber > 0;
            IsEnableNextPage = IsEnableLastPage = PageNumber < TotalPageCount - 1;

            OnPageNumberChanged(new PaginationEventHandler(PageNumber, ItemPerPage));
        }

        public object SelectedObject
        {
            get { return (object)GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PaginationUserControl),
                new PropertyMetadata(null));



        public long SelectedIndex
        {
            get { return (long)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(long), typeof(PaginationUserControl),
                new PropertyMetadata((long)-1));




        protected bool IsEnableNextPage
        {
            get { return (bool)GetValue(IsEnableNextPageProperty); }
            set { SetValue(IsEnableNextPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for .  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnableNextPageProperty =
            DependencyProperty.Register(nameof(IsEnableNextPage), typeof(bool), typeof(PaginationUserControl),
                new PropertyMetadata(false));

        protected bool IsEnablePrevPage
        {
            get { return (bool)GetValue(IsEnablePrevPageProperty); }
            set { SetValue(IsEnablePrevPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnablePrevPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnablePrevPageProperty =
            DependencyProperty.Register(nameof(IsEnablePrevPage), typeof(bool), typeof(PaginationUserControl),
                new PropertyMetadata(false));


        protected bool IsEnableFirstPage
        {
            get { return (bool)GetValue(IsEnableFirstPageProperty); }
            set { SetValue(IsEnableFirstPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnableFirstPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnableFirstPageProperty =
            DependencyProperty.Register(nameof(IsEnableFirstPage), typeof(bool), typeof(PaginationUserControl),
                new PropertyMetadata(false));


        protected bool IsEnableLastPage
        {
            get { return (bool)GetValue(IsEnableLastPageProperty); }
            set { SetValue(IsEnableLastPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEnableLastPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnableLastPageProperty =
            DependencyProperty.Register(nameof(IsEnableLastPage), typeof(bool), typeof(PaginationUserControl),
                new PropertyMetadata(false));

        #endregion

        #region Events           


        public event EventHandler<PaginationEventHandler> PageNumberChanged;
        protected void OnPageNumberChanged(PaginationEventHandler e)
        {
            PageNumberChanged?.Invoke(this, e);
        }

        public event EventHandler<PaginationEventHandler> ItemPerPageChanged;
        protected void OnItemPerPageChanged(PaginationEventHandler e)
        {
            ItemPerPageChanged?.Invoke(this, e);
        }

        #endregion


        private void SetTotalPageCount()
        {
            if (TotalNumberCollection == 0)
                TotalPageCount = 1;
            else
                TotalPageCount = (long)Math.Ceiling((double)TotalNumberCollection / (double)ItemPerPage);

            if (PageNumber >= TotalPageCount)
                PageNumber = TotalPageCount - 1;
        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber = 0;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber--;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber++;
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            PageNumber = TotalPageCount - 1;
        }

    }
}
