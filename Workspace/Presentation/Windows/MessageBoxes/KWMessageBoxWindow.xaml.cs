using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.MessageBoxes
{
    /// <summary>
    /// Interaction logic for OkWindow.xaml
    /// </summary>
    public partial class KWMessageBoxWindow : Window
    {
        public MessageBoxResult Result { get; set; }

        protected MessageBoxButton ButtonType
        {
            get { return (MessageBoxButton)GetValue(ButtonTypeProperty); }
            set { SetValue(ButtonTypeProperty, value); }
        }

        protected static readonly DependencyProperty ButtonTypeProperty =
            DependencyProperty.Register(nameof(ButtonType), typeof(MessageBoxButton), typeof(KWMessageBoxWindow),
                new PropertyMetadata(MessageBoxButton.OK));

        protected MessageBoxImage IconType
        {
            get { return (MessageBoxImage)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        protected static readonly DependencyProperty IconTypeProperty =
            DependencyProperty.Register(nameof(IconType), typeof(MessageBoxImage), typeof(KWMessageBoxWindow),
                new PropertyMetadata(MessageBoxImage.Information));

        public RelayCommand EscapeCommand { get; set; }

        private void EscapeCommandMethod(object obj)
        {
            switch (ButtonType)
            {
                case MessageBoxButton.OK:
                    Result = MessageBoxResult.OK;
                    Close();
                    break;
                case MessageBoxButton.YesNoCancel:
                case MessageBoxButton.OKCancel:
                    Result = MessageBoxResult.Cancel;
                    Close();
                    break;
                case MessageBoxButton.YesNo:
                    break;
                default:
                    break;
            }            
        }

        public KWMessageBoxWindow(string message) :
            this(null, message, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
        { }

        public KWMessageBoxWindow(string message, MessageBoxButton button) :
            this(null, message, button, MessageBoxImage.Information, MessageBoxResult.Yes)
        { }

        public KWMessageBoxWindow(string message, MessageBoxButton button, MessageBoxImage icon) :
            this(null, message, button, icon, MessageBoxResult.Yes)
        { }

        public KWMessageBoxWindow(string message, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) :
            this(null, message, button, icon, defaultResult)
        { }

        public KWMessageBoxWindow(Window owner, string message) :
            this(owner, message, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK)
        { }

        public KWMessageBoxWindow(Window owner, string message, MessageBoxButton button) :
            this(owner, message, button, MessageBoxImage.Information, MessageBoxResult.OK)
        { }

        public KWMessageBoxWindow(Window owner, string message, MessageBoxButton button, MessageBoxImage icon) :
           this(owner, message, button, icon, MessageBoxResult.OK)
        { }

        public KWMessageBoxWindow(Window owner, string message, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            ButtonType = button;
            IconType = icon;
            SetIconVisibility();
            SetDefaultButon(defaultResult);

            if (owner == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            EscapeCommand = new RelayCommand(EscapeCommandMethod);
        }

        private void SetIconVisibility()
        {
            Icon.Visibility = IconType == MessageBoxImage.None ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetDefaultButon(MessageBoxResult defaultResult)
        {
            switch (defaultResult)
            {
                case MessageBoxResult.OK:
                    if (ButtonType == MessageBoxButton.OK || ButtonType == MessageBoxButton.OKCancel)
                        OkButton.Focus();
                    break;
                case MessageBoxResult.Cancel:
                    if (ButtonType == MessageBoxButton.YesNoCancel || ButtonType == MessageBoxButton.OKCancel)
                        CancelButton.Focus();
                    break;
                case MessageBoxResult.Yes:
                    if (ButtonType == MessageBoxButton.YesNo || ButtonType == MessageBoxButton.YesNoCancel)
                        YesButton.Focus();                        
                    else
                        OkButton.Focus();
                    break;
                case MessageBoxResult.No:
                    if (ButtonType == MessageBoxButton.YesNo || ButtonType == MessageBoxButton.YesNoCancel)
                        NoButton.Focus();                        
                    else
                        CancelButton.Focus();
                    break;
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void OkButtonOnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void CancelButtonOnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void NoButtonOnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void YesButtonOnClick(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }
    }
}