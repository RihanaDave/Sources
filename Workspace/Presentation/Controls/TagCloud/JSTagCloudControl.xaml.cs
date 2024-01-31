using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Windows;
using GPAS.Workspace.Presentation.Windows;

namespace GPAS.Workspace.Presentation.Controls.TagCloud
{
    public partial class JsTagCloudControl
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public JsTagCloudControl()
        {
            InitializeComponent();
            MainWindow = App.MainWindow;
        }

        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainWindow.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(JsTagCloudControl),
                new PropertyMetadata(null, OnSetMainWindowChanged));


        private static void OnSetMainWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((JsTagCloudControl)d).OnSetMainWindowChanged(e);
        }

        private void OnSetMainWindowChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MainWindow != null)
            {
                MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
                MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;

                SetTheme(MainWindow?.CurrentTheme);
            }
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetTheme(MainWindow?.CurrentTheme);
        }

        private void SetTheme(ThemeApplication? themeApplication)
        {
            if (themeApplication == null)
            {
                JsTagCloudViewer.Theme = JSTagCloudViewer.ThemeType.Dark;
            }
            else
            {
                JsTagCloudViewer.Theme = themeApplication == ThemeApplication.Dark ?
                    JSTagCloudViewer.ThemeType.Dark :
                    JSTagCloudViewer.ThemeType.Light;
            }
        }


        /// <summary>
        /// مجموعه کلمات که روی ابر کلمات نمایش داده می‌شوند
        /// </summary>
        public ObservableCollection<KeyPhraseModel> KeyPhrasesCollection
        {
            get => (ObservableCollection<KeyPhraseModel>)GetValue(KeyPhrasesCollectionProperty);
            set => SetValue(KeyPhrasesCollectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyPhrasesCollectionProperty =
            DependencyProperty.Register(nameof(KeyPhrasesCollection), typeof(ObservableCollection<KeyPhraseModel>),
                typeof(JsTagCloudControl), new PropertyMetadata(null, OnSetKeyPhrasesCollectionChanged));

        private static void OnSetKeyPhrasesCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((JsTagCloudControl)d).OnSetKeyPhrasesCollectionChanged(e);
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
        /// رویداد تغییر محتوای مجموعه کلمات برای نمایش 
        /// </summary>
        public event NotifyCollectionChangedEventHandler KeyPhrasesCollectionChanged;
        private void OnKeyPhrasesCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            KeyPhrasesCollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// آماده‌سازی لیست کلمات برای نمایش
        /// </summary>
        private void ShowTagCloud()
        {
            var keyPhrases = new ObservableCollection<JSTagCloudViewer.KeyPhrase>();

            foreach (var currentKeyPhrase in KeyPhrasesCollection)
            {
                keyPhrases.Add(new JSTagCloudViewer.KeyPhrase
                {
                    key = currentKeyPhrase.Key,
                    value = currentKeyPhrase.Weight
                });
            }

            JsTagCloudViewer.KeyPhrasesCollection = keyPhrases;
        }

        /// <summary>
        /// ذخیره ابر کلمات به عنوان عکس
        /// </summary>
        public void SaveTagCloudImage()
        {
            var screenShot = JsTagCloudViewer.TakeScreenShot();

            if (screenShot == null)
                return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog()
            {
                DefaultExt = ".jpg",
                Filter = "JPG Image (*.jpg)|*.jpg"
            };
            var result = saveFileDialog.ShowDialog();
            if (result != true) return;

            try
            {
                screenShot.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow = GetMainWindow(Window.GetWindow(this));
        }

        private MainWindow GetMainWindow(Window window)
        {
            if (!(window is Window))
                return null;

            if (window is MainWindow mainWindow)
                return mainWindow;

            return GetMainWindow(window.Owner);
        }
    }
}
