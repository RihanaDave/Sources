using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace GPAS.JSTagCloudViewer
{
    public partial class JsTagCloudViewer
    {
        /// <summary>
        /// مسیر فایل
        /// Html
        /// </summary>
        private readonly string jsPath = $"{AppDomain.CurrentDomain.BaseDirectory}Resources\\JavaScript\\TagCloud\\index.html";
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public ThemeType Theme
        {
            get { return (ThemeType)GetValue(ThemeProperty); }
            set { SetValue(ThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Theme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.Register(nameof(Theme), typeof(ThemeType), typeof(JsTagCloudViewer),
                new PropertyMetadata(ThemeType.Light, OnSetThemeChanged));

        private static void OnSetThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((JsTagCloudViewer)d).OnSetThemeChanged(e);
        }

        private void OnSetThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            SetBackground();
        }

        private void SetBackground()
        {
            string background = (Theme == ThemeType.Light) ? "#FFFFFF" : "#000000";

            try
            {
                ExecScript("setBackground", new object[] { background });
                dispatcherTimer.Stop();
                WebBrowser.Opacity = 1;
            }
            catch
            {
                dispatcherTimer.Start();
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            SetBackground();
        }



        /// <summary>
        /// مجموعه عبارات که در ابر کلمات قرار است نمایش داده شود
        /// </summary>
        public ObservableCollection<KeyPhrase> KeyPhrasesCollection
        {
            get => (ObservableCollection<KeyPhrase>)GetValue(KeyPhrasesCollectionProperty);
            set => SetValue(KeyPhrasesCollectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyPhrasesCollectionProperty =
            DependencyProperty.Register(nameof(KeyPhrasesCollection), typeof(ObservableCollection<KeyPhrase>),
                typeof(JsTagCloudViewer), new PropertyMetadata(null, OnSetKeyPhrasesCollectionChanged));

        private static void OnSetKeyPhrasesCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((JsTagCloudViewer)d).OnSetKeyPhrasesCollectionChanged(e);
        }

        private void OnSetKeyPhrasesCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (KeyPhrasesCollection != null)
            {
                ShowTagCloud();

                KeyPhrasesCollection.CollectionChanged -= KeyPhrasesOnCollectionChanged;
                KeyPhrasesCollection.CollectionChanged += KeyPhrasesOnCollectionChanged;
            }

            OnKeyPhrasesCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewValue, e.OldValue));
        }

        private void KeyPhrasesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ShowTagCloud();
        }

        /// <summary>
        /// رویداد تغییر محتوای مجموعه عبارات 
        /// </summary>
        public event NotifyCollectionChangedEventHandler KeyPhrasesCollectionChanged;
        private void OnKeyPhrasesCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            KeyPhrasesCollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public JsTagCloudViewer()
        {
            InitializeComponent();
            SetBackground();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            WebBrowser.Navigate(jsPath);
        }

        /// <summary>
        /// آماده‌سازی ابر کلمات برای نمایش
        /// </summary>
        private void ShowTagCloud()
        {
            SetTags(KeyPhrasesCollection.ToJson(), CalculateMaxFontSize(KeyPhrasesCollection.ToList()));
            ReloadTagCloud();
        }

        /// <summary>
        /// محاسبه بزرگترین انداز قلم برای نمایش کلمات طبق
        /// میانگین طول کل کلمات
        /// </summary>
        /// <param name="keyPhrases">مجموعه کلمات</param>
        /// <returns></returns>
        private int CalculateMaxFontSize(List<KeyPhrase> keyPhrases)
        {
            int avg = keyPhrases.Count > 0 ? keyPhrases.Sum(phrase => phrase.key.Length) / keyPhrases.Count : 0;

            switch (avg)
            {
                case int c when (c >= 1 && c < 7):
                    return 100;
                case int c when (c >= 7 && c < 13):
                    return 50;
                case int c when (c >= 13 && c < 19):
                    return 20;
                case int c when (c >= 19):
                    return 15;
                default:
                    return 100;
            }
        }

        /// <summary>
        /// گرفتن عکس از صفحه کنترل مرورگر
        /// </summary>
        /// <returns></returns>
        public Bitmap TakeScreenShot()
        {
            var topLeftCorner = WebBrowser.PointToScreen(new System.Windows.Point(0, 0));
            var topLeftGdiPoint = new System.Drawing.Point((int)topLeftCorner.X, (int)topLeftCorner.Y);
            var size = new System.Drawing.Size((int)WebBrowser.ActualWidth, (int)WebBrowser.ActualHeight);

            var screenShot = new Bitmap((int)WebBrowser.ActualWidth, (int)WebBrowser.ActualHeight);

            using (var graphics = Graphics.FromImage(screenShot))
            {
                graphics.CopyFromScreen(topLeftGdiPoint, new System.Drawing.Point(),
                    size, CopyPixelOperation.SourceCopy);
            }

            return screenShot;
        }

        /// <summary>
        /// مقدار دهی لیست کلمات برای  نمایش
        /// </summary>
        /// <param name="jsonKeyPhrases">لیست کلمات به صورت جیسون</param>
        /// <param name="maxFontSize">بزرگترین اندازه قلم</param>
        private void SetTags(string jsonKeyPhrases, int maxFontSize)
        {
            ExecScript("setTags", new object[] { jsonKeyPhrases, maxFontSize });
        }

        /// <summary>
        /// تازه‌سازی صفحه مرورگر
        /// </summary>
        private void ReloadTagCloud()
        {
            ExecScript("update");
            WebBrowser.Opacity = 1;
        }

        /// <summary>
        /// فراخوانی تابع‌های فایل جاوا اسکریپت برای اجرا
        /// </summary>
        /// <param name="methodName">نام تابع</param>
        private void ExecScript(string methodName)
        {
            if (WebBrowser.IsInitialized)
                WebBrowser.InvokeScript(methodName);
        }

        /// <summary>
        ///  فراخوانی تابع‌های فایل جاوا اسکریپت برای اجرا
        /// </summary>
        /// <param name="methodName">نام تابع</param>
        /// <param name="parameters">پارامتر‌های ارسالی</param>
        private void ExecScript(string methodName, object[] parameters)
        {
            if (WebBrowser.IsInitialized)
                WebBrowser.InvokeScript(methodName, parameters);
        }
    }
}
