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
    /// Interaction logic for BranchItem.xaml
    /// </summary>
    public partial class BranchItem : UserControl
    {
        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(BranchItem), new PropertyMetadata(null));

       
        public BranchItem()
        {
            InitializeComponent();
            DataContext = this;
           
        }
        
        public event EventHandler<RecomputeEventArgs> Recompute;
        public event EventHandler<DependencyPropertyChangedEventArgs> AllowDragDropChanged;

        public List<BranchItem> BranchesItems { get; set; }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(BranchItem), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(BranchItem), new PropertyMetadata(""));

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(BranchItem), new PropertyMetadata(null));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(BranchItem), new PropertyMetadata(false));

        public bool IsInActiveSequence
        {
            get { return (bool)GetValue(IsInActiveSequenceProperty); }
            set { SetValue(IsInActiveSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInActiveSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInActiveSequenceProperty =
            DependencyProperty.Register("IsInActiveSequence", typeof(bool), typeof(BranchItem), new PropertyMetadata(false));

        public int VerticalDept
        {
            get { return (int)GetValue(VerticalDeptProperty); }
            set { SetValue(VerticalDeptProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalDept.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalDeptProperty =
            DependencyProperty.Register("VerticalDept", typeof(int), typeof(BranchItem), new PropertyMetadata(0));

        public int HorizontalDept
        {
            get { return (int)GetValue(HorizontalDeptProperty); }
            set { SetValue(HorizontalDeptProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalDept.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalDeptProperty =
            DependencyProperty.Register("HorizontalDept", typeof(int), typeof(BranchItem), new PropertyMetadata(0));

        public bool IsDerivedObject
        {
            get { return (bool)GetValue(IsDerivedObjectProperty); }
            set { SetValue(IsDerivedObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDerivedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDerivedObjectProperty =
            DependencyProperty.Register("IsDerivedObject", typeof(bool), typeof(BranchItem), new PropertyMetadata(true));

        
        public BranchItem ParentItem
        {
            get { return (BranchItem)GetValue(ParentItemProperty); }
            set { SetValue(ParentItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParentItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentItemProperty =
            DependencyProperty.Register("ParentItem", typeof(BranchItem), typeof(BranchItem), new PropertyMetadata(null, OnSetParentItemChanged));

        private static void OnSetParentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bi = d as BranchItem;
            bi.OnSetParentItemChanged(e);
        }

        private void OnSetParentItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ParentItem?.BranchesItems == null)
                ParentItem.BranchesItems = new List<BranchItem>();

            ParentItem?.BranchesItems?.Add(this);
        }

        public bool AllowDragDrop
        {
            get { return (bool)GetValue(AllowDragDropProperty); }
            set { SetValue(AllowDragDropProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowDragDrop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowDragDropProperty =
            DependencyProperty.Register("AllowDragDrop", typeof(bool), typeof(BranchItem), new PropertyMetadata(false, OnSetAllowDragDropChanged));

        private static void OnSetAllowDragDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BranchItem).OnSetAllowDragDropChanged(e);
        }

        private void OnSetAllowDragDropChanged(DependencyPropertyChangedEventArgs e)
        {
            AllowDragDropChanged?.Invoke(this, new DependencyPropertyChangedEventArgs(e.Property, e.OldValue, e.NewValue));
        }

        public ObjectBase ObjectBase
        {
            get { return (ObjectBase)GetValue(ObjectBaseProperty); }
            set { SetValue(ObjectBaseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectBase.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectBaseProperty =
            DependencyProperty.Register("ObjectBase", typeof(ObjectBase), typeof(BranchItem), new PropertyMetadata(null, OnSetObjectBaseChanged));

        private static void OnSetObjectBaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bi = d as BranchItem;
            bi.OnSetObjectBaseChanged(e);
        }       


        private void OnSetObjectBaseChanged(DependencyPropertyChangedEventArgs e)
        {
            SetBinding(TitleProperty, new Binding("Title") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(DescriptionProperty, new Binding("Description") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(IconProperty, new Binding("Icon") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(IsInActiveSequenceProperty, new Binding("IsInActiveSequence") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(IsActiveProperty, new Binding("IsActive") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(HorizontalDeptProperty, new Binding("HorizontalDept") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(VerticalDeptProperty, new Binding("VerticalDept") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(TagProperty, new Binding("Tag") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(IsDerivedObjectProperty, new Binding("IsDerivedObject") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });
            SetBinding(AllowDragDropProperty, new Binding("AllowDragDrop") { Source = this.ObjectBase, Mode = BindingMode.TwoWay });

            this.ObjectBase.Recompute -= ObjectBase_Recompute;
            this.ObjectBase.Recompute += ObjectBase_Recompute;
        }

        private void ObjectBase_Recompute(object sender, RecomputeEventArgs e)
        {
            Recompute?.Invoke(this, e);
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (!IsActive)
                    ObjectBase.OnGetSequence();
        }

        private void menuSelectThisSet_Click(object sender, RoutedEventArgs e)
        {
            if (!IsActive)
                ObjectBase.OnGetSequence();
        }

        private void menuRecomputeSetContents_Click(object sender, RoutedEventArgs e)
        {
            ObjectBase?.OnRecompute();
        }
    }

    

    public class IsInActiveSequenceToBackgroundColorBranchItemConverter : IMultiValueConverter
    {
       PaletteHelper paletteHelper = new PaletteHelper();
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
                  IBaseTheme baseTheme = null;

            if ((ThemeApplication)values[1]==ThemeApplication.Dark)
            {
                baseTheme = new MaterialDesignDarkTheme();
                theme.SetBaseTheme(baseTheme);
            }
            else
            {
                baseTheme = new MaterialDesignLightTheme();
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToReverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;

            return bool.Parse(value.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}
