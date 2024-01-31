using GPAS.Histogram;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.HistogramViewer
{
    public partial class Histogram : UserControl
    {
        #region اعلان های انتظار
        /// <summary>
        /// اشاره گری به منویی که میزبان کنترل برای آن طراحی کرده
        /// </summary>
        /// <remarks>
        /// از این متغیر زمانی استفاده می شود که کنترل به حالت انتظار
        /// می رود؛ از آنجا که در طول مدتی که کنترل در حالت انتظار
        /// است به میزبان کنترل اجازه استفاده از منو را نمی دهیم، در
        /// طول این مدت مقدار منو را برای کنترل نال می کنیم و منویی
        /// که میزبان تعریف کرده را موقتا در این متغیر نگه داری
        /// می کنیم
        /// </remarks>
        private ContextMenu relatedMasterSetContextMenu = null;

        /// <summary>
        /// نمایش اعلان انتظار به صورت تعیین نشده
        /// </summary>
        public void ShowWaiting(string custumPrompt = "", bool enableCanceling = true)
        {
            // غیرفعال کردن (کانتکس)منوی مربوطه برای جلوگیری از رویداد
            // ناخواسته، با نال کردن مقدار منوی کنترل و نگهداری موقت
            // منویی که میزبان تعریف کرده در متغیر محلی
            if (ContextMenu != null)
            {
                relatedMasterSetContextMenu = ContextMenu;
                ContextMenu = null;
            }
            // قرار دادن نوار وضعیت در حالت تعیین نشده
            prgWaiting.IsIndeterminate = true;
            // آشکارسازی نوار پیشرفت
            prgWaiting.Visibility = Visibility.Visible;

            PromptInternalStatusOnStatusbar = false;

            // نمایش پیام وضعیت متناسب
            HistogramStatusTextBlock.Text = (custumPrompt == "")
                ? Properties.Resources.WaitingPrompts_UndeterminedWaitingPrompt : custumPrompt;

            if (enableCanceling)
                ResetHistogramUIToDefaultStatus();
            else
                DisableCancelHistogramRequestButton();
        }

        /// <summary>
        /// نمایش اعلان انتظار به صورت نوار پیشرفت
        /// </summary>
        /// <remarks>
        /// در این حالت پیام متنی اعلان به صورت خودکار ایجاد می شود که
        /// میزان پیشرفت عملیات را نشان می دهد
        /// این پیام در نوار وضعیت هیستوگرام نمایش داده می شود
        /// </remarks>
        /// <param name="CurrentItemIndex">اندیس موردی که در حال حاضر در حال پردازش است را نشان می دهد</param>
        /// <param name="TotalItemsCount">تعداد کل مواردی که باید مورد پردازش قرار گیرند را نشان می دهد</param>
        public void ShowWaiting(int CurrentItemIndex, int TotalItemsCount)
        {
            // فراخوانی همپوشان عملگر، با تعریف یک پیام خودکار که
            // براساس مقادیر وارد شده نشاندهنده میزان پیشرفت است
            ShowWaiting(CurrentItemIndex, TotalItemsCount, string.Format(Properties.Resources.WaitingPrompts_DeterminedWaitingPrompt, CurrentItemIndex.ToString(), TotalItemsCount.ToString()));
        }

        /// <summary>
        /// نمایش اعلان انتظار به صورت نوار پیشرفت با پیام سفارشی
        /// </summary>
        /// <param name="CurrentItemIndex">اندیس موردی که در حال حاضر در حال پردازش است را نشان می دهد</param>
        /// <param name="TotalItemsCount">تعداد کل مواردی که باید مورد پردازش قرار گیرند را نشان می دهد</param>
        /// <param name="WaitingMessage">پیام دلخواهی که می خواهید همراه با اعلان در نوار وضعیت هیستوگرام نمایش داده شود</param>
        public void ShowWaiting(int CurrentItemIndex, int TotalItemsCount, string WaitingMessage, bool enableCanceling = true)
        {
            // غیرفعال کردن (کانتکس)منوی مربوطه برای جلوگیری از رویداد
            // ناخواسته، با نال کردن مقدار منوی کنترل و نگهداری موقت
            // منویی که میزبان تعریف کرده در متغیر محلی
            if (ContextMenu != null)
            {
                relatedMasterSetContextMenu = ContextMenu;
                ContextMenu = null;
            }
            // خارج کردن نوار وضعیت از حالت تعیین نشده
            prgWaiting.IsIndeterminate = false;
            // تعیین مقادیر بیشینه و مقدار کنونی نوار پیشرفت
            prgWaiting.Maximum = TotalItemsCount;
            prgWaiting.Value = CurrentItemIndex;
            // آشکارسازی نوار پیشرفت
            prgWaiting.Visibility = Visibility.Visible;
            // نمایش پیام وضعیت براساس ورودی این عملگر
            string progressPrompt = string.Format("{0}/{1}", CurrentItemIndex.ToString(), TotalItemsCount.ToString());
            HistogramStatusTextBlock.Text = (WaitingMessage == "")
                ? progressPrompt
                : string.Format("{0} [{1}]", WaitingMessage, progressPrompt);
            if (enableCanceling)
                ResetHistogramUIToDefaultStatus();
            else
                DisableCancelHistogramRequestButton();
        }
        /// <summary>
        /// مخفی نمودن اعلان انتظار
        /// </summary>
        public void HideWaiting(string readyPrompt = "", bool disableCanceling = true)
        {
            // خارج کردن نوار وضعیت از حالت تعیین نشده
            prgWaiting.IsIndeterminate = false;
            // مخفی کردن نوار پیشرفت
            prgWaiting.Visibility = System.Windows.Visibility.Hidden;
            // نمایش پیام وضعیت آمادگی کنترل
            HistogramStatusTextBlock.Text = (readyPrompt == "")
                ? Properties.Resources.ReadyPrompts_SimpleReadyPrompt : readyPrompt;

            // فعال سازی مجدد (کانتکس) منوی کنترل که برای جلوگیری از
            // رویدادی ناخواسته، در زمان شروع اعلان انتظار غیرفعال
            // شده بود؛ برای این کار مقدار منوی تعریف شده توسط میزبان
            // که موقتا در متغیر محلی ذخیره شده بود، به عنوان منوی
            // کنترل در نظر گرفته می شود
            if (relatedMasterSetContextMenu != null)
            {
                ContextMenu = relatedMasterSetContextMenu;
                relatedMasterSetContextMenu = null;
            }

            if (disableCanceling)
                DisableCancelHistogramRequestButton();
        }

        public void DisableCancelHistogramRequestButton()
        {
            CancelHistogramRequestButton.IsEnabled = false;
        }

        /// <summary>
        /// نشان می دهد که آیا هیستوگرام در حالت منتظر بمانید است یا
        /// خیر
        /// </summary>
        public bool IsInWaitingState
        {
            get { return prgWaiting.IsEnabled; }
        }
        #endregion      

        /// <summary>
        /// پیام وضعیت در حال نمایش هیستوگرام را برمیگرداند
        /// </summary>
        public string StatusMessage
        {
            get { return HistogramStatusTextBlock.Text; }
            set { HistogramStatusTextBlock.Text = value; }
        }

        public Histogram()
        {
            InitializeComponent();
            DataContext = this;

            // پیام وضعیت آمادگی کنترل هیستوگرام
            HistogramStatusTextBlock.Text = Properties.Resources.ReadyPrompts_SimpleReadyPrompt;

            // مخفی کردن اعلان "منتظر بمانید"
            HideWaiting();
        }

        public bool PromptInternalStatusOnStatusbar
        {
            get;
            set;
        }

        public void Clear()
        {
            MainHistogram.ClearItems();
            NoItemToShowGrid.Visibility = Visibility.Visible;

            // نمایش پیام وضعیت مربوطه
            if (PromptInternalStatusOnStatusbar)
                HistogramStatusTextBlock.Text = Properties.Resources.ReadyPrompts_HistogramCleared;
        }

        public void CollapselblHistogramStatus()
        {
            HistogramStatusTextBlock.Visibility = Visibility.Collapsed;
        }

        public void CollapseCancelHistogramRequestButton()
        {
            CancelHistogramRequestButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// رخدادگردان خالی رخداد تغییر گره های انتاب شده این هیستوگرام
        /// </summary>
        /// <remarks>
        /// این رخدادگردان برای جلوگیری از خطای عدم پیاده سازی رخداد تعریف شده است
        /// </remarks>
        private void SelectionChanged_EmptyEventHandler(Histogram sender, EventArgs e)
        { }

        /// <summary>
        /// رخدادگردان کلیک راست روی کنترل
        /// </summary>
        /// <remarks>
        /// این رخداد گردان در صورتی که کنترل در حالت انتظر باشد و
        /// میزبان کنترل نیز منویی برای آن تعریف کرده باشد (که در
        /// این حالت کنترل آن را از دسترس خارج کرده است) پیام وضعیتی
        /// را مبنی بر عدم آمادگی کنترل اعلام می کند
        /// </remarks>
        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (PromptInternalStatusOnStatusbar && IsInWaitingState && relatedMasterSetContextMenu != null)
                HistogramStatusTextBlock.Text = Properties.Resources.WaitingPrompts_UnableToResponseWhileWorkNotCompleted;
        }

        public event EventHandler<EventArgs> CancelHistogramRequestButtonClicked;

        protected virtual void OnCancelHistogramRequestButtonClicked()
        {
            // صدور رخداد مربوطه در صورت نیاز
            if (CancelHistogramRequestButtonClicked != null)
                CancelHistogramRequestButtonClicked(this, EventArgs.Empty);
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        public event EventHandler<List<HistogramItem>> SelectionChanged;
        private void OnSelectionChanged(List<HistogramItem> selectedItems)
        {
            SelectionChanged?.Invoke(this, selectedItems);
        }

        private void CancelHistogramRequestButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeHistogramUIToCancelStatus();
            OnCancelHistogramRequestButtonClicked();
        }

        public void Reset()
        {
            MainHistogram.Reset();
        }

        public void ResetHistogramUIToDefaultStatus()
        {
            CancelHistogramRequestButton.IsEnabled = true;
            prgWaiting.Visibility = Visibility.Visible;
        }

        public void ChangeHistogramUIToCancelStatus()
        {
            HistogramStatusTextBlock.Text = Properties.Resources.Histogram_Request_Canceled_By_User;
            CancelHistogramRequestButton.IsEnabled = false;
            prgWaiting.Visibility = Visibility.Hidden;
        }

        public void FillMainHistogram(IHistogramFillingPropertiesGroup newItems)
        {
            if (newItems == null)
                throw new ArgumentNullException(nameof(newItems));

            NoItemToShowGrid.Visibility = Visibility.Collapsed;

            // نمایش پیام وضعیت مربوطه
            if (PromptInternalStatusOnStatusbar)
                HistogramStatusTextBlock.Text = Properties.Resources.ReadyPrompts_AGroupAdded;

            ObservableCollection<HistogramItem> items = new ObservableCollection<HistogramItem>();

            HistogramItem newItem = MakeNewHistogramItem(newItems);

            items.Add(newItem);

            MainHistogram.Items = items;

            // نمایش پیام وضعیت افزودن گروه (ها) به هیستوگرام
            if (PromptInternalStatusOnStatusbar)
                HistogramStatusTextBlock.Text = string.Format(Properties.Resources.ReadyPrompts_MoreThanOneGroupAdded, items.Count);
        }

        public void UpdateHistogramItems(IHistogramFillingPropertiesGroup newItems)
        {
            List<HistogramItem> oldItems = MainHistogram.Items.ToList();

            ObservableCollection<HistogramItem> items = new ObservableCollection<HistogramItem>(oldItems);

            HistogramItem newItem = MakeNewHistogramItem(newItems);

            newItems.NewSubItemAdded += (sender, e) =>
            {
                AddNewItemToParent(newItem, e.AddedSubItems);
            };

            items.Add(newItem);

            MainHistogram.Items = items;
        }

        private HistogramItem MakeNewHistogramItem(IHistogramFillingPropertiesGroup newItems)
        {
            HistogramItem level1 = new HistogramItem
            {
                Title = newItems.HistogramTitle,
                UsePrimaryColor = true
            };

            foreach (IHistogramFillingProperty item in newItems.HistogramSubItems)
            {
                HistogramItem level2 = new HistogramItem
                {
                    Title = item.HistogramTitle
                };

                foreach (IHistogramFillingValueCountPair innerItem in item.HistgramValueCounts)
                {
                    HistogramItem level3 = new HistogramItem
                    {
                        Title = innerItem.HistogramValue,
                        Value = innerItem.HistogramCountForValue,
                        RelatedElement = innerItem
                    };

                    level2.Items.Add(level3);
                }

                level1.Items.Add(level2);
            }

            return level1;
        }

        private void AddNewItemToParent(HistogramItem parent, IEnumerable<IHistogramFillingProperty> currentUserProperty)
        {
            foreach (IHistogramFillingProperty item in currentUserProperty)
            {
                HistogramItem level2 = new HistogramItem
                {
                    Title = item.HistogramTitle
                };

                foreach (IHistogramFillingValueCountPair innerItem in item.HistgramValueCounts)
                {
                    HistogramItem level3 = new HistogramItem
                    {
                        Title = innerItem.HistogramValue,
                        Value = innerItem.HistogramCountForValue,
                        RelatedElement = innerItem
                    };

                    level2.Items.Add(level3);
                }

                parent.Items.Add(level2);
            }
        }

        private void MainHistogram_SelectedItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnSelectionChanged(MainHistogram.SelectedItems.ToList());
        }

        private void MainHistogram_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }
    }
}