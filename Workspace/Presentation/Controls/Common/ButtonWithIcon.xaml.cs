using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.Common
{
    /// <summary>
    /// Interaction logic for ButtonWithIcon.xaml
    /// </summary>
    public partial class ButtonWithIcon
    {
        public ButtonWithIcon()
        {
            InitializeComponent();
        }

        public PackIconKind Icon
        {
            get => (PackIconKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
            typeof(PackIconKind), typeof(ButtonWithIcon), new PropertyMetadata(null));

        public Dock IconPosition
        {
            get => (Dock)GetValue(IconPositionProperty);
            set => SetValue(IconPositionProperty, value);
        }

        public static readonly DependencyProperty IconPositionProperty = DependencyProperty.Register(nameof(IconPosition),
            typeof(Dock), typeof(ButtonWithIcon), new PropertyMetadata(Dock.Left));

        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
        }

        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(nameof(IconHeight),
            typeof(double), typeof(ButtonWithIcon), new PropertyMetadata(20.0));

        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(nameof(IconWidth),
            typeof(double), typeof(ButtonWithIcon), new PropertyMetadata(20.0));
        
        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(nameof(IconMargin),
            typeof(Thickness), typeof(ButtonWithIcon), new PropertyMetadata(new Thickness(0,0,0,0)));

        public Thickness ContentMargin
        {
            get => (Thickness)GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin),
            typeof(Thickness), typeof(ButtonWithIcon), new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        public double ContentFontSize
        {
            get => (double)GetValue(ContentFontSizeProperty);
            set => SetValue(ContentFontSizeProperty, value);
        }

        public static readonly DependencyProperty ContentFontSizeProperty = DependencyProperty.Register(nameof(ContentFontSize),
            typeof(double), typeof(ButtonWithIcon), new PropertyMetadata(14.0));
    }
}
