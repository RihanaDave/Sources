using System.Windows;
using System.Windows.Media;

namespace GPAS.HistogramViewer
{
    /// <summary>
    /// تنظیمات ظاهر کنترل های هیستوگرام؛
    /// </summary>
    /// <remarks>این کلاس ایستا برای تجمیع اعمال تنظیمات ظاهر کنرتل های روی هیستوگرام طراحی شده است</remarks>
    internal static class Appearance
    {
        /// <summary>
        /// تنظیمات ظاهری نمایش گروه های ویژگی در هیستوگرام
        /// </summary>
        internal static class Groups
        {
            private static Brush rowBackground = new SolidColorBrush(Colors.CornflowerBlue);
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف گروه را برمی گرداند
            /// </summary>
            internal static Brush RowBackground
            {
                get { return Groups.rowBackground; }
            }

            private static Brush rowForeground = new SolidColorBrush(Colors.White);
            /// <summary>
            /// برس رنگ آمیزی رو زمینه ردیف گروه را برمی گرداند
            /// </summary>
            internal static Brush RowForeground
            {
                get { return Groups.rowForeground; }
            }

            private static double rowFontSize = 18;
            /// <summary>
            /// اندازه فونت ردیف گروه را برمی گرداند
            /// </summary>
            internal static double RowFontSize
            {
                get { return Groups.rowFontSize; }
            }

            private static FontWeight rowFontWeight = FontWeights.Bold;
            /// <summary>
            /// وزن فونت ردیف گروه را برمی گرداند
            /// </summary>
            internal static FontWeight RowFontWeight
            {
                get { return Groups.rowFontWeight; }
            }

            private static int histogramRowHeight = 40;
            /// <summary>
            /// ارتفاع ردیف گروه را (به پیکسل) برمی گرداند
            /// </summary>
            internal static int HistogramRowHeight
            {
                get { return Groups.histogramRowHeight; }
            }
        }
        /// <summary>
        /// تنظیمات ظاهری نمایش ویژگی ها در هیستوگرام
        /// </summary>
        internal static class Properties
        {
            private static Brush rowBackground = new SolidColorBrush(Colors.White);
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف عنوان ویژگی را برمی گرداند
            /// </summary>
            internal static Brush RowBackground
            {
                get { return Properties.rowBackground; }
            }

            private static Brush rowForeground = new SolidColorBrush(Colors.Black);
            /// <summary>
            /// برس رنگ آمیزی رو زمینه ردیف عنوان ویژگی را برمی گرداند
            /// </summary>
            internal static Brush RowForeground
            {
                get { return Properties.rowForeground; }
            }

            private static double rowFontSize = 14;
            /// <summary>
            /// اندازه فونت ردیف عنوان ویژگی را برمی گرداند
            /// </summary>
            internal static double RowFontSize
            {
                get { return Properties.rowFontSize; }
            }

            private static FontWeight rowFontWeight = FontWeights.Bold;
            /// <summary>
            /// وزن فونت ردیف عنوان ویژگی را برمی گرداند
            /// </summary>
            internal static FontWeight RowFontWeight
            {
                get { return Properties.rowFontWeight; }
            }

            private static int histogramRowHeight = 30;
            /// <summary>
            /// ارتفاع ردیف عنوان ویژگی را (به پیکسل) برمی گرداند
            /// </summary>
            internal static int HistogramRowHeight
            {
                get { return Properties.histogramRowHeight; }
            }
        }
        /// <summary>
        /// تنظیمات ظاهری نمایش مقدار ویژگی ها (زوج «مقدار-تعداد») در
        /// هیستوگرام
        /// </summary>
        internal static class PropertyValue
        {
            private static Brush oddRowBackground = new SolidColorBrush(Colors.White);//LightCyan
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف مقدار ویژگی، زمانی که در جایگاه فرد باشد را برمی گرداند
            /// </summary>
            internal static Brush OddRowBackground
            {
                get { return PropertyValue.oddRowBackground; }
            }

            private static Brush evenRowBackground = new SolidColorBrush(Colors.White);
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف مقدار ویژگی، زمانی که در جایگاه زوج باشد را برمی گرداند
            /// </summary>
            internal static Brush EvenRowBackground
            {
                get { return PropertyValue.evenRowBackground; }
            }

            private static Brush pointedRowBackground = new SolidColorBrush(Colors.GhostWhite);//Cyan
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف مقدار ویژگی، زمانی که در نشانگر موس روی آن باشد را برمی گرداند
            /// </summary>
            internal static Brush PointedRowBackground
            {
                get { return PropertyValue.pointedRowBackground; }
            }

            private static Brush selectedRowBackground = new SolidColorBrush(Colors.Gold);//CadetBlue
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف مقدار ویژگی، زمانی که انتخاب شده باشد را برمی گرداند
            /// </summary>
            internal static Brush SelectedRowBackground
            {
                get { return PropertyValue.selectedRowBackground; }
            }

            private static Brush dataBarValueBrushOfOversizedValues = new SolidColorBrush(Colors.PaleVioletRed);
            /// <summary>
            /// برس رنگ آمیزی نوار داده ردیف مقدار ویژگی، زمانی که از اندازه بیشینه بزرگتر باشد را برمی گرداند
            /// </summary>
            internal static Brush DataBarValueBrushOfOversizedValues
            {
                get { return PropertyValue.dataBarValueBrushOfOversizedValues; }
            }

            private static Brush rowForeground = new SolidColorBrush(Colors.Black);
            /// <summary>
            /// برس رنگ آمیزی رو زمینه ردیف مقدار ویژگی، را برمی گرداند
            /// </summary>
            internal static Brush RowForeground
            {
                get { return PropertyValue.rowForeground; }
            }

            private static double rowFontSize = 11;
            /// <summary>
            /// اندازه فونت ردیف مقدار ویژگی را برمی گرداند
            /// </summary>
            internal static double RowFontSize
            {
                get { return PropertyValue.rowFontSize; }
            }

            private static FontWeight rowFontWeight = FontWeights.Normal;
            /// <summary>
            /// وزن فونت ردیف مقدار ویژگی را برمی گرداند
            /// </summary>
            internal static FontWeight RowFontWeight
            {
                get { return PropertyValue.rowFontWeight; }
            }

            private static int histogramRowHeight = 24;
            /// <summary>
            /// ارتفاع ردیف مقدار ویژگی را به پیکسل برمی گرداند
            /// </summary>
            internal static int HistogramRowHeight
            {
                get { return PropertyValue.histogramRowHeight; }
            }
        }
        /// <summary>
        /// تنظیمات ظاهری گسترش/جمع کردن گره های «مقدار-تعداد»
        /// زیرمجموعه هر ویژگی در هیستوگرام
        /// </summary>
        internal static class ExpandHistogramItems
        {
            private static Brush rowBackground = new SolidColorBrush(Colors.White);
            /// <summary>
            /// برس رنگ آمیزی پس زمینه ردیف عنوان ویژگی را برمی گرداند
            /// </summary>
            internal static Brush RowBackground
            {
                get { return ExpandHistogramItems.rowBackground; }
            }

            private static int defaultNumberOfShowingValuesForEachProperty = 7;
            /// <summary>
            /// تعداد پیش فرض «مقدار-تعداد»هایی که برای هر ویژگی نمایش 
            /// داده می شود
            /// </summary>
            /// <remarks>
            /// در صورتی که تعداد کل «مقدار-ویژگی»ها کمتر از این باشد
            /// همه نمایش داده خواهند شد
            /// </remarks>
            public static int DefaultNumberOfShowingValuesForEachProperty
            {
                get { return defaultNumberOfShowingValuesForEachProperty; }
                //set { defaultNumberOfShowingValuesForEachProperty = value; }
            }

            private static int stepOfIncreasingNumberOfShowingVAluesForProperty = 20;
            /// <summary>
            /// گام گسترش یا جمع کردن «مقدار-تعداد»های گره
            /// </summary>
            /// <remarks>
            /// در هر بار درخواست کاربر برای نمایش تعداد بیشتر یا کمتری
            /// از «مقدار-تعداد»ها، گام افزایش یا کاهش براساس این مقدار
            /// انجام خواهد شد
            /// </remarks>
            public static int StepOfIncreasingNumberOfShowingVAluesForProperty
            {
                get { return stepOfIncreasingNumberOfShowingVAluesForProperty; }
            }
        }
    }
}