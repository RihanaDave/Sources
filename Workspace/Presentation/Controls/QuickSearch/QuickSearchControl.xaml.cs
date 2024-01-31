using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// منطق تعامل با QuickSearchControl.xaml
    /// </summary>
    public partial class QuickSearchControl
    {
        #region متغیرهای سراسری
        private ResourceManager resourceManager = new ResourceManager(typeof(Properties.Resources));
        private DispatcherTimer dispatcherTimer;
        #endregion

        #region مدیریت رخداد
        /// <summary>
        /// رخداد «شروع شدن جستجوی سریع» 
        /// </summary>
        public event EventHandler<EventArgs> QuickSearchStarted;

        /// <summary>
        /// عملکرد مدیریت رخداد «تکمیل شدن جستجوی سریع» 
        /// </summary>
        public void OnQuickSearchStarted()
        {
            ApplySearchTimeArrearance();
            QuickSearchStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// کلاس آرگومان های مورد نیاز جهت فراخوانی رخداد «تکمیل شدن جستجوی سریع»
        /// </summary>
        public class QuickSearchCompletedEventAgrs : EventArgs
        {
            public QuickSearchCompletedEventAgrs(Exception occuredExceptionDuringSearch)
            {
                SearchResults = new List<KWObject>();
                occuredException = occuredExceptionDuringSearch ?? throw new ArgumentNullException(nameof(occuredExceptionDuringSearch));
            }

            public QuickSearchCompletedEventAgrs(List<KWObject> results)
            {
                SearchResults = results ?? throw new ArgumentNullException(nameof(results));
                occuredException = null;
            }

            public List<KWObject> SearchResults
            {
                get;
            }

            public Exception occuredException
            {
                get;
            }

            public bool IsCompletedWithException()
            { return occuredException != null; }
        }

        /// <summary>
        /// رخداد «تکمیل شدن جستجوی سریع» 
        /// </summary>
        public event EventHandler<QuickSearchCompletedEventAgrs> QuickSearchCompleted;

        /// <summary>
        /// عملگر صدور رخداد «تکمیل شدن جستجوی سریع» در زمان اجرای موفقیت آمیز جستجو
        /// </summary>
        protected virtual void OnQuickSearchCompleted(List<KWObject> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            ApplyIdelTimeArrearance();
            QuickSearchCompleted?.Invoke(this, new QuickSearchCompletedEventAgrs(results));
            SearchKeywordTextBox.Focus();
            SearchKeywordTextBox.SelectAll();
        }
        /// <summary>
        /// عملگر صدور رخداد «تکمیل شدن جستجوی سریع» در زمان مواجهه جستجو با استثناء
        /// </summary>
        protected virtual void OnQuickSearchCompleted(Exception occueredExceptionDuringSearch)
        {
            if (occueredExceptionDuringSearch == null)
                throw new ArgumentNullException(nameof(occueredExceptionDuringSearch));

            ApplyIdelTimeArrearance();
            QuickSearchCompleted?.Invoke(this, new QuickSearchCompletedEventAgrs(occueredExceptionDuringSearch));
            SearchKeywordTextBox.Focus();
            SearchKeywordTextBox.SelectAll();
        }

        //////////////////////////
        /// <summary>
        /// ارگومان های صدور رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش
        /// </summary>
        public class QuickSearchResultChoosenEventArgs
        {
            /// <summary>
            /// سازنده کلاس
            /// </summary>
            /// <param name="choosenResult"></param>
            public QuickSearchResultChoosenEventArgs(KWObject choosenResult)
            {
                ChoosenResult = choosenResult ?? throw new ArgumentNullException(nameof(choosenResult));
            }
            /// <summary>
            /// نتیجه برگزیده شده
            /// </summary>
            public KWObject ChoosenResult
            {
                get;
            }
        }

        /// <summary>
        /// رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش»؛
        /// این اتفاق عموما با دوبار کلیک روی نتیجه اتفاق می افتد
        /// </summary>
        public event EventHandler<QuickSearchResultChoosenEventArgs> QuickSearchResultChoosen;

        /// <summary>
        /// عملکرد مدیریت رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش»
        /// </summary>
        protected virtual void OnQuickSearchResultChoosen(KWObject choosenResult)
        {
            if (choosenResult == null)
                throw new ArgumentNullException(nameof(choosenResult));

            QuickSearchResultChoosen?.Invoke(this, new QuickSearchResultChoosenEventArgs(choosenResult));
        }
        #endregion

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        #region توابع
        public QuickSearchControl()
        {
            InitializeComponent();
            Init();
            UpdateAppearanceAccordingEnteredKeyword();
            ApplyIdelTimeArrearance();
            SearchKeywordTextBox.Focus();
        }

        private void Init()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 6);
        }

        private void UpdateAppearanceAccordingEnteredKeyword()
        {
            if (!IsEnteredKeywordValid())
            {
                SearchButton.IsEnabled = false;
                StatusTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            StatusTextBlock.Text = "";
            StatusTextBlock.Visibility = Visibility.Visible;

            if (!IsEnteredKeywordMaximumLengthValid())
            {
                SearchButton.IsEnabled = false;
                StatusTextBlock.Text = resourceManager.GetString("Entered_Keyword_Is_Too_Long");
                return;
            }
            if (!IsEnteredKeywordMinimumLengthValid())
            {
                SearchButton.IsEnabled = false;
                StatusTextBlock.Text = resourceManager.GetString("Entered_Keyword_Is_Too_Short");
                return;
            }

            SearchButton.IsEnabled = true;
            StatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private bool IsEnteredKeywordMinimumLengthValid()
        {
            return SearchKeywordTextBox.Text.Length >= 0;
        }

        private bool IsEnteredKeywordMaximumLengthValid()
        {
            return SearchKeywordTextBox.Text.Length <= 1000;
        }

        private bool IsEnteredKeywordValid()
        {
            return !string.IsNullOrWhiteSpace(SearchKeywordTextBox.Text);
        }

        /// <summary>
        /// عملکرد جستجو براساس مقدار وارد شده
        /// </summary>
        private async void SearchByEnteredKeyword()
        {
            WaitProgressBar.Visibility = Visibility.Visible;
            SearchButton.IsEnabled = false;
            SearchKeywordTextBox.IsEnabled = false;

            // صدور رخداد شروع جستجو سریع
            OnQuickSearchStarted();
            IEnumerable<KWObject> results;

            try
            {
                results = await Search();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                StatusTextBlock.Text = resourceManager.GetString("Search_Failed");
                dispatcherTimer.Start();
                // صدور رخداد تکمیل جستجو سریع با برخورد با استثناء
                OnQuickSearchCompleted(ex);
                return;
            }

            // صدور رخداد تکمیل شدن موفق جستجو سریع
            OnQuickSearchCompleted(results.ToList());
            ResultViewer.ShowResults(results.ToList());
            ResultPopup.IsOpen = true;
        }

        private async Task<IEnumerable<KWObject>> Search()
        {
            IEnumerable<KWObject> results = null;
            string searchText = SearchKeywordTextBox.Text;

            await Task.Run(() =>
            {
                results = SearchProvider.QuickSearchAsync(searchText, Properties.Settings.Default.QuickSearchUnpublishedResultMaxCount);
            });

            return results;
        }

        private void ApplySearchTimeArrearance()
        {
            SearchKeywordTextBox.IsReadOnly = true;
            SearchButton.Visibility = Visibility.Hidden;
        }

        private void ApplyIdelTimeArrearance()
        {
            SearchKeywordTextBox.IsReadOnly = false;
            SearchButton.Visibility = Visibility.Visible;

            WaitProgressBar.Visibility = Visibility.Collapsed;
            SearchButton.IsEnabled = true;
            SearchKeywordTextBox.IsEnabled = true;
        }

#pragma warning disable CS0108 // 'QuickSearchControl.Focus()' hides inherited member 'UIElement.Focus()'. Use the new keyword if hiding was intended.
        public void Focus()
#pragma warning restore CS0108 // 'QuickSearchControl.Focus()' hides inherited member 'UIElement.Focus()'. Use the new keyword if hiding was intended.
        {
            SearchKeywordTextBox.Focus();
            SearchKeywordTextBox?.SelectAll();
        }

        public void CloseResultPopup()
        {
            ResultPopup.IsOpen = false;
        }

        public void Reset()
        {
            SearchKeywordTextBox.Text = string.Empty;
        }

        #endregion

        #region مدیریت رخدادگردانها
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            StatusTextBlock.Visibility = Visibility.Collapsed;

            //Disable the timer
            dispatcherTimer.IsEnabled = false;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEnteredKeywordValid()
                    && IsEnteredKeywordMinimumLengthValid()
                    && IsEnteredKeywordMaximumLengthValid())
            {
                SearchByEnteredKeyword();
            }
        }
        private void SearchKeywordTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAppearanceAccordingEnteredKeyword();
        }

        private void SearchKeywordTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
                    && IsEnteredKeywordValid()
                    && IsEnteredKeywordMinimumLengthValid()
                    && IsEnteredKeywordMaximumLengthValid())
            {
                SearchByEnteredKeyword();
            }
        }

        private void ResultViewer_QuickSearchResultChoosen(object sender, QuickSearchResultViewerControl.QuickSearchResultChoosenEventArgs e)
        {
            OnQuickSearchResultChoosen(e.ChoosenResult);
        }
        #endregion

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnteredKeywordValid()
                   && IsEnteredKeywordMinimumLengthValid()
                   && IsEnteredKeywordMaximumLengthValid())
            {
                SearchByEnteredKeyword();
            }
        }
    }
}
