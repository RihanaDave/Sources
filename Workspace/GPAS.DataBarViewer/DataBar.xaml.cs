using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.DataBarViewer
{
    /// <summary>
    /// منطق تعامل با
    /// DataBar.xaml
    /// </summary>
    public partial class DataBar : UserControl
    {
        /// <summary>
        /// سازنده
        /// </summary>
        public DataBar()
        {
            // آماده سازی اجزا کنترل
            InitializeComponent();
            // بازنشانی ظاهر کنترل
            refreshAppearance();
        }
        /// <summary>
        /// کمینه مقدار نمایشی را نگه می دارد
        /// </summary>
        private int minimum = 0;
        /// <summary>
        /// کمینه مقدار نمایشی را می گیرد یا برمیگرداند
        /// </summary>
        public int Minimum
        {
            get { return minimum; }
            set
            {
                // کمینه مقدار کوچکتر از صفر (در این نسخه کنترل) قابل پذیرش نیست
                if (value < 0)
                    return;
                // مقداردهی متغیر مربوطه
                minimum = value;
                // بازنشانی ظاهر کنترل
                refreshAppearance();
            }
        }
        /// <summary>
        /// بیشینه مقدار نمایشی را نگه می دارد
        /// </summary>
        private int maximum = 100;
        /// <summary>
        /// بیشینه مقدار نمایشی را می گیرد یا برمیگرداند
        /// </summary>
        public int Maximum
        {
            get { return maximum; }
            set
            {
                // بیشینه مقدار کوچکتر از کمینه، قابل پذیرش نیست
                if (value < minimum)
                    return;
                // مقداردهی متغیر مربوطه
                maximum = value;
                // بازنشانی ظاهر کنترل
                refreshAppearance();
            }
        }
        /// <summary>
        /// ظاهر کنترل را بر اساس تنظیماتش بازنشانی می کند
        /// </summary>
        private void refreshAppearance()
        {
            // در صورت بزرگ تر بودن مقدار از بیشینه قابل نمایش، ظاهر
            // بیش از اندازه اعمال می شود و نوار داده کاملا پر نمایش
            // داده می شود
            if (value > maximum)
            {
                grdBar.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                grdBar.ColumnDefinitions[1].Width = new GridLength(0);

                rectBar.Fill = oversizedBarBrush;
            }
            else
            {
                // در صورت کوچک تر بودن مقدار از کمینه قابل نمایش، نوار
                // داده کاملا خالی نمایش داده می شود
                if (value < minimum)
                {
                    grdBar.ColumnDefinitions[0].Width = new GridLength(0);
                    grdBar.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                }
                // در حالت عادی اندازه نوار داده را بر اساس نسبتش با
                // بیشینه و کمینه نمایشی تعیین می شود
                else
                {
                    grdBar.ColumnDefinitions[0].Width = new GridLength(this.value - this.minimum, GridUnitType.Star);
                    grdBar.ColumnDefinitions[1].Width = new GridLength(this.maximum - this.value, GridUnitType.Star);
                }
                // در دو حالت اخیر ظاهر (برس) نوار داده عادی خواهد بود
                rectBar.Fill = barBrush;
            }
        }
        /// <summary>
        /// مقدار داده برای نمایش را در خود نگه می دارد
        /// </summary>
        private int value = 20;
        /// <summary>
        /// مقدار داده برای نمایش را می گیرد یا برمیگرداند 
        /// </summary>
        public int Value
        {
            get { return this.value; }
            set
            {
                // متغیر مربوطه را مقداردهی کرده و این مقدار را روی
                // کنترل نمایش می دهد
                lblValueViewer.Content = (this.value = value).ToString();
                // بازنشانی ظاهر کنترل
                refreshAppearance();
            }
        }
        /// <summary>
        /// رنگ پیش فرض نوار داده در حالت مقدار بیش از اندازه را نگه می دارد
        /// </summary>
        private static Color defaultOversizedBarColor = Colors.PaleVioletRed;
        /// <summary>
        /// برس نوار داده در حالت بیش از اندازه را نگه می دارد
        /// </summary>
        private Brush oversizedBarBrush = new SolidColorBrush(defaultOversizedBarColor);
        /// <summary>
        /// برس نوار داده در حالت بیش از اندازه را می گیرد با برمیگرداند
        /// </summary>
        public Brush OversizedBarBrush
        {
            get { return oversizedBarBrush; }
            set
            {
                // مقداردهی متغیر مربوطه
                oversizedBarBrush = value;
                // بازنشانی ظاهر کنترل
                refreshAppearance();
            }
        }
        /// <summary>
        /// رنگ پیش فرض نوار داده در حالت مقدار عادی را نگه می دارد
        /// </summary>
        private static Color defaultBarColor = Colors.SkyBlue;
        /// <summary>
        /// برس نوار داده در حالت مقدار عادی را نگه می دارد
        /// </summary>
        private Brush barBrush = new SolidColorBrush(defaultBarColor);
        /// <summary>
        /// برس نوار داده در حالت مقدار عادی را می گیرد با برمیگرداند
        /// </summary>
        public Brush BarBrush
        {
            get { return barBrush; }
            set
            {
                // مقداردهی متغیر مربوطه
                barBrush = value;
                // بازنشانی ظاهر کنترل
                refreshAppearance();
            }
        }
    }
}