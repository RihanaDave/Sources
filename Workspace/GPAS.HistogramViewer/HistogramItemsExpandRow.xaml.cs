using System;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.HistogramViewer
{
    ///
    /// از این کنترل برای گسترش دادن و جمع کردن مجموعه داده های روی
    /// هیستوگرام استفاده می شود، آن زمان که تعدادشان از تعداد پیش فرض
    /// تعیین شده برای برای نمایش بیشتر باشد
    ///

    /// <summary>
    /// منطق تعامل با
    /// HistogramItemsExpandRow.xaml
    /// </summary>
    public partial class HistogramItemsExpandRow : UserControl
    {
        private RowDefinition relatedRow = null;
        /// <summary>
        /// شی "تعریف ردیف"ی که این کنترل گسترش روی آن قرار دارد را
        /// برمیگرداند؛ این شی تعریف ردیف مربوطه روی "گرید" هیستوگرام
        /// را نشان می دهد
        /// </summary>
        public RowDefinition RelatedRow
        {
            get { return relatedRow; }
        }

        private HistogramPropertyNode relatedProperty = null;
        /// <summary>
        /// شی "گره ویژگی هیستوگرام"ی که این کنترل گسترش مربوط به آن
        /// است و وظیفه گسترش و جمع کردن «مقدار-تعداد»های زیرمجموعه اش
        /// را برعهده دارد برمیگرداند
        /// </summary>
        internal HistogramPropertyNode RelatedProperty
        {
            get { return relatedProperty; }
        }

        /// <summary>
        /// تعداد گره های «مقدار-تعداد»ی که در هر لحظه در حال نمایش
        /// است را نگه می دارد
        /// </summary>
        private int currentlyShowingItemsCount = 0;

        /// <summary>
        /// سازنده کنترل
        /// </summary>
        /// <param name="RelatedRowDefinition">شی "تعریف ردیف"ی که این کنترل گسترش روی آن قرار دارد</param>
        /// <param name="RelatedPropertyNode">شی "گره ویژگی هیستوگرام"ی که این کنترل گسترش مربوط به آن است</param>
        /// <param name="CurrentlyShowingItemsCount">تعداد گره های «مقدار-تعداد»ی که در هر لحظه در حال نمایش است</param>
        internal HistogramItemsExpandRow(RowDefinition RelatedRowDefinition, HistogramPropertyNode RelatedPropertyNode, int CurrentlyShowingItemsCount)
        {
            // آماده سازی اجزا کنترل
            InitializeComponent();

            hypShowAll.Inlines.Add(Properties.Resources.ExpandRowCommands_ShowAll);
            hypLess.Inlines.Add(Properties.Resources.ExpandRowCommands_Less);
            hypMore.Inlines.Add(Properties.Resources.ExpandRowCommands_More);

            // آماده سازی رخداد گردان وقوع تغییر در تعداد موارد درحال
            // نمایش
            ShowingItemsCountChange += OnShowingItemsCountChange_EmptyImp;
            // مقداردهی اولیه متغیرهای محلی بر اساس ورودی های سازنده
            if (RelatedRowDefinition == null)
                throw new NullReferenceException(); 
            relatedRow = RelatedRowDefinition;
            if (RelatedPropertyNode == null)
                throw new NullReferenceException(); 
            relatedProperty = RelatedPropertyNode;
            if (CurrentlyShowingItemsCount < 0)
                throw new InvalidOperationException();
            currentlyShowingItemsCount = CurrentlyShowingItemsCount;
            // بازنشانی ظاهر کنترل
            refreshControlAppearance();
        }
        /// <summary>
        /// بازنشانی ظاهر کنترل بر اساس تنظیمات اعمال شده به آن
        /// </summary>
        private void refreshControlAppearance()
        {
            // تنظیم فعال بودن/نبودن لینک های روی کنترل بر اساس تعداد
            // موارد در حال نمایش
            hypShowAll.IsEnabled =
                hypMore.IsEnabled = currentlyShowingItemsCount < relatedProperty.ValueCounts.Count;
            hypLess.IsEnabled = currentlyShowingItemsCount > Appearance.ExpandHistogramItems.DefaultNumberOfShowingValuesForEachProperty;
        }
        /// <summary>
        /// رخداد گردان کلیک روی لینک نمایش همه
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hypShowAll_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyShowingItemsCount == relatedProperty.ValueCounts.Count)
                return;
            // صدور رخداد تغییر تعداد موارد در حال نمایش
            ShowingItemsCountChange.Invoke(this, new ShowingItemsCountChangeEventArgs(currentlyShowingItemsCount, relatedProperty.ValueCounts.Count));
            // تنظیم تعداد در حال نمایش به بیشینه که همان تعداد
            // گره های «مقدار-تعداد» زیر مجموعه گره ویژگی مربوطه است
            currentlyShowingItemsCount = relatedProperty.ValueCounts.Count;
            // بازنشانی ظاهر کنترل
            refreshControlAppearance();
        }
        /// <summary>
        /// رخداد گردان کلیک روی لینک نمایش تعداد کمتر
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hypLess_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyShowingItemsCount <= Appearance.ExpandHistogramItems.DefaultNumberOfShowingValuesForEachProperty)
                return;
            // بازتنظیم تعداد موارد در حال نمایش براساس تعداد در حال
            // نمایش کنونی
            if (currentlyShowingItemsCount <= Appearance.ExpandHistogramItems.DefaultNumberOfShowingValuesForEachProperty + Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty)
            {
                ShowingItemsCountChange.Invoke(this, new ShowingItemsCountChangeEventArgs(currentlyShowingItemsCount, Appearance.ExpandHistogramItems.DefaultNumberOfShowingValuesForEachProperty));
                currentlyShowingItemsCount = Appearance.ExpandHistogramItems.DefaultNumberOfShowingValuesForEachProperty;
            }
            else
            {
                ShowingItemsCountChange.Invoke(this, new ShowingItemsCountChangeEventArgs(currentlyShowingItemsCount, currentlyShowingItemsCount - Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty));
                currentlyShowingItemsCount = currentlyShowingItemsCount - Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty;
            }
            // بازنشانی ظاهر کنترل
            refreshControlAppearance();
        }
        /// <summary>
        /// رخداد گردان کلیک روی لینک نمایش تعداد بیشتر
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hypMore_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyShowingItemsCount >= relatedProperty.ValueCounts.Count)
                return;
            // بازتنظیم تعداد موارد در حال نمایش براساس تعداد در حال
            // نمایش کنونی
            if (currentlyShowingItemsCount >= relatedProperty.ValueCounts.Count - Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty)
            {
                ShowingItemsCountChange.Invoke(this, new ShowingItemsCountChangeEventArgs(currentlyShowingItemsCount, relatedProperty.ValueCounts.Count));
                currentlyShowingItemsCount = relatedProperty.ValueCounts.Count;
            }
            else
            {
                ShowingItemsCountChange.Invoke(this, new ShowingItemsCountChangeEventArgs(currentlyShowingItemsCount, currentlyShowingItemsCount + Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty));
                currentlyShowingItemsCount = currentlyShowingItemsCount + Appearance.ExpandHistogramItems.StepOfIncreasingNumberOfShowingVAluesForProperty;
            }
            // بازنشانی ظاهر کنترل
            refreshControlAppearance();
        }
        /// <summary>
        /// رخداد تغییر در تعداد موارد در حال نمایش
        /// </summary>
        internal event OnShowingItemsCountChange ShowingItemsCountChange;
        /// <summary>
        /// تعریف نماینده رخداد تغییر در تعداد موارد در حال نمایش
        /// </summary>
        internal delegate void OnShowingItemsCountChange(HistogramItemsExpandRow sender, ShowingItemsCountChangeEventArgs e);
        /// <summary>
        /// رخدادگردان خالی رخداد "تغییر در تعداد موارد در حال نمایش" جهت
        /// جلوگیری از بروز خطای عدم پیاده سازی
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowingItemsCountChange_EmptyImp(HistogramItemsExpandRow sender, ShowingItemsCountChangeEventArgs e)
        { }
        /// <summary>
        /// آرگومان رخداد تغییر در تعداد موراد درحال نمایش
        /// </summary>
        internal class ShowingItemsCountChangeEventArgs : EventArgs
        {
            int currentlyShowingItemsCount;
            /// <summary>
            /// تعداد مواردی که هم اکنون در حال نمایش اند را برمیگرداند
            /// </summary>
            public int CurrentlyShowingItemsCount
            {
                get { return currentlyShowingItemsCount; }
            }

            int newShowingItemsCount;
            /// <summary>
            /// تعداد جدید موارد برای نمایش را برمیگرداند
            /// </summary>
            public int NewShowingItemsCount
            {
                get { return newShowingItemsCount; }
            }
            /// <summary>
            /// سازنده آرگومان رخداد
            /// </summary>
            /// <param name="CurrentShowingItemsCount">تعداد مواردی که هم اکنون در حال نمایش اند</param>
            /// <param name="NewShowingItemsCount">تعداد جدید موارد برای نمایش</param>
            internal ShowingItemsCountChangeEventArgs(int CurrentShowingItemsCount, int NewShowingItemsCount)
            {
                // انتساب متغیرهای محلی براساس ورودی های سازنده
                currentlyShowingItemsCount = CurrentShowingItemsCount;
                newShowingItemsCount = NewShowingItemsCount;
            }
        }
    }
}
