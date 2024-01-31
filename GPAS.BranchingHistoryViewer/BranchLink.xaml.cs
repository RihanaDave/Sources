using GPAS.BranchingHistoryViewer.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.BranchingHistoryViewer
{
    /// <summary>
    /// Interaction logic for BranchLink.xaml
    /// </summary>
    public partial class BranchLink : UserControl
    {

        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(BranchLink), new PropertyMetadata(null));

        public BranchLink()
        {
            InitializeComponent();
            DataContext = this;
        }

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(BranchLink), new PropertyMetadata(null));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(BranchLink), new PropertyMetadata(""));

        public bool IsInActiveSequence
        {
            get { return (bool)GetValue(IsInActiveSequenceProperty); }
            set { SetValue(IsInActiveSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInActiveSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInActiveSequenceProperty =
            DependencyProperty.Register("IsInActiveSequence", typeof(bool), typeof(BranchLink), new PropertyMetadata(false));

        public bool IsContingency
        {
            get { return (bool)GetValue(IsContingencyProperty); }
            set { SetValue(IsContingencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContingency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContingencyProperty =
            DependencyProperty.Register("IsContingency", typeof(bool), typeof(BranchLink), new PropertyMetadata(false, OnSetIsContingencyChanged));

        private static void OnSetIsContingencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bl = d as BranchLink;
            bl.OnSetIsContingencyChanged(e);
        }

        private void OnSetIsContingencyChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsDirect
        {
            get { return (bool)GetValue(IsDirectProperty); }
            set { SetValue(IsDirectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirectProperty =
            DependencyProperty.Register("IsDirect", typeof(bool), typeof(BranchLink), new PropertyMetadata(true, OnSetIsDirectChanged));

        private static void OnSetIsDirectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bl = d as BranchLink;
            bl.OnSetIsDirectChanged(e);
        }

        private void OnSetIsDirectChanged(DependencyPropertyChangedEventArgs e)
        {
            RegenrateShape();
        }

        public void RegenrateShape()
        {
            var l = (Resources["link"] as ConnectionLink);
            l.From = Link.From;
            l.To = Link.To;
        }

        public ConnectionLink Link
        {
            get { return (ConnectionLink)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Link.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(ConnectionLink), typeof(BranchLink), new PropertyMetadata(null, OnSetLinkChanged));

        private static void OnSetLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var l = d as BranchLink;
            l.OnSetLinkChanged(e);
        }

        private void OnSetLinkChanged(DependencyPropertyChangedEventArgs e)
        {
            SetBinding(IconProperty, new Binding("Icon") { Source = Link, Mode = BindingMode.TwoWay });
            SetBinding(DescriptionProperty, new Binding("Description") { Source = Link, Mode = BindingMode.TwoWay });
            SetBinding(IsInActiveSequenceProperty, new Binding("IsInActiveSequence") { Source = Link.To, Mode = BindingMode.TwoWay });
            SetBinding(IsDirectProperty, new Binding("IsDirect") { Source = Link, Mode = BindingMode.TwoWay });
        }
    }

    public class IsContingencyToOpacityBranchLinkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return .3;
                }
                else
                {
                    return 1;
                }
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DeptFromToPolygonPointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection points = new PointCollection();
            points.Add(new Point(0, 10));
            points.Add(new Point(35, 10));
            points.Add(new Point(35, 0));
            points.Add(new Point(80, 45));
            points.Add(new Point(35, 90));
            points.Add(new Point(35, 80));
            points.Add(new Point(0, 80));

            if (value is bool)
            {
                if ((bool)value)
                {
                    return points;
                }
                else
                {
                    ConnectionLink link = parameter as ConnectionLink;
                    var dif = link.To.VerticalDept - link.From.VerticalDept;

                    points = new PointCollection();
                    double h = (dif - 1) * -100;
                    points.Add(new Point(-70, h - 10));
                    points.Add(new Point(-10, h - 10));
                    points.Add(new Point(-10, 10));
                    points.Add(new Point(35, 10));
                    points.Add(new Point(35, 0));
                    points.Add(new Point(80, 45));
                    points.Add(new Point(35, 90));
                    points.Add(new Point(35, 80));
                    points.Add(new Point(-70, 80));

                    return points;
                }
            }

            return points;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsInActiveSequenceToBackgroundColorBranchLinkConverter : IMultiValueConverter
    {
        readonly PaletteHelper paletteHelper = new PaletteHelper();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ITheme theme = paletteHelper.GetTheme();
            LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
            myLinearGradientBrush.StartPoint = new Point(0.5, 0);
            myLinearGradientBrush.EndPoint = new Point(0.5, 1);
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(theme.PrimaryMid.Color, 0.0));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(theme.PrimaryDark.Color, 0.5));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(theme.PrimaryMid.Color, 1));

            if ((ThemeApplication)values[1] == ThemeApplication.Dark)
            {
            var  baseTheme = new MaterialDesignDarkTheme();
                theme.SetBaseTheme(baseTheme);               
            }
            else
            {
              var  baseTheme = new MaterialDesignLightTheme();
                theme.SetBaseTheme(baseTheme);
                theme.Paper = (Color)ColorConverter.ConvertFromString("#EFEFEF");
                theme.CardBackground = (Color)ColorConverter.ConvertFromString("#DFDFDF");

            }
            if (values[0] is bool)
            {
                if ((bool)values[0])
                {
                    return myLinearGradientBrush;
                }
                else
                {
                    return new SolidColorBrush(theme.CardBackground);
                }
            }
            return new SolidColorBrush(theme.CardBackground);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsContingencyToTooltipVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
