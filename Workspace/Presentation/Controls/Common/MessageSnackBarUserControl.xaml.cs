using System;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace GPAS.Workspace.Presentation.Controls.Common
{
    /// <summary>
    /// Interaction logic for MessageSnackBarUserControl.xaml
    /// </summary>
    public partial class MessageSnackBarUserControl
    {        
        private readonly DispatcherTimer closeSnackBar;

        public MessageSnackBarUserControl()
        {
            InitializeComponent();
            closeSnackBar = new DispatcherTimer();
            closeSnackBar.Interval = TimeSpan.FromSeconds(3);
            closeSnackBar.Tick += CloseSnackBarOnTick;
        }

        private void CloseSnackBarOnTick(object sender, EventArgs e)
        {
            MainSnackbar.IsActive = false;
            closeSnackBar.Stop();
        }

        public void Show()
        {
            MainSnackbar.IsActive = true;

            if (AutoDeactivate)
                closeSnackBar.Start();
        }

        public void Hide()
        {
            MainSnackbar.IsActive = false;
            closeSnackBar.Stop();
        }

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
            typeof(PackIconKind), typeof(MessageSnackBarUserControl),
            new PropertyMetadata(PackIconKind.WarningOctagon));

        public Brush IconForeground
        {
            get => (Brush)GetValue(IconForegroundProperty);
            set => SetValue(IconForegroundProperty, value);
        }

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(nameof(IconForeground),
            typeof(Brush), typeof(MessageSnackBarUserControl), new PropertyMetadata());

        public string Message
        {

            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message),
            typeof(string), typeof(MessageSnackBarUserControl), new PropertyMetadata(null));

        public Visibility DetailsButtonVisibility
        {
            get => (Visibility)GetValue(DetailsButtonVisibilityProperty);
            set => SetValue(DetailsButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty DetailsButtonVisibilityProperty = DependencyProperty.Register(nameof(DetailsButtonVisibility),
            typeof(Visibility), typeof(MessageSnackBarUserControl), new PropertyMetadata(Visibility.Collapsed));

        public bool AutoDeactivate
        {
            get => (bool)GetValue(AutoDeactivateProperty);
            set => SetValue(AutoDeactivateProperty, value);
        }

        public static readonly DependencyProperty AutoDeactivateProperty = DependencyProperty.Register(nameof(AutoDeactivate),
            typeof(bool), typeof(MessageSnackBarUserControl), new PropertyMetadata(true));

        #region Events

        public event EventHandler<EventArgs> ShowDetailsClicked;

        protected void OnShowDetailsClicked()
        {
            ShowDetailsClicked?.Invoke(this, new EventArgs());
        }

        #endregion

        private void ShowDetailsButtonOnClick(object sender, RoutedEventArgs e)
        {
            MainSnackbar.IsActive = false;
            OnShowDetailsClicked();
        }

        private void CloseSnackBarButtonOnClick(object sender, RoutedEventArgs e)
        {
            MainSnackbar.IsActive = false;
        }
    }
}
