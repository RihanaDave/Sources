using GPAS.Workspace.ViewModel.ObjectExplorer;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.FormulaPanel
{
    /// <summary>
    /// Interaction logic for FormulaItem.xaml
    /// </summary>
    public partial class FormulaItem 
    {
        public FormulaItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly RoutedEvent SelectedEvent =
           EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble,
           typeof(RoutedEventHandler), typeof(FormulaItem));

        public event RoutedEventHandler Selected {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }

        public static readonly RoutedEvent DeselectedEvent =
           EventManager.RegisterRoutedEvent("deselected", RoutingStrategy.Bubble,
           typeof(RoutedEventHandler), typeof(FormulaItem));


        public event RoutedEventHandler Deselected
        {
            add { AddHandler(DeselectedEvent, value); }
            remove { RemoveHandler(DeselectedEvent, value); }
        }

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(FormulaItem), new PropertyMetadata(new BitmapImage()));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FormulaItem), new PropertyMetadata("Title"));

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubTitleProperty =
            DependencyProperty.Register("SubTitle", typeof(string), typeof(FormulaItem), new PropertyMetadata("SubTitle"));

        public string ObjectsCountText
        {
            get { return (string)GetValue(ObjectsCountTextProperty); }
            set { SetValue(ObjectsCountTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsCountText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsCountTextProperty =
            DependencyProperty.Register("ObjectsCountText", typeof(string), typeof(FormulaItem), new PropertyMetadata("ObjectsCount"));

        public ElementLocationOfOfSequence ElementLocation
        {
            get { return (ElementLocationOfOfSequence)GetValue(ElementLocationProperty); }
            set { SetValue(ElementLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ElementLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ElementLocationProperty =
            DependencyProperty.Register("ElementLocation", typeof(ElementLocationOfOfSequence), typeof(FormulaItem), new PropertyMetadata(ElementLocationOfOfSequence.Center));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(FormulaItem), new PropertyMetadata(false, OnSetIsSelectedChanged));

        private static void OnSetIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fp = d as FormulaItem;
            fp.OnSetIsSelectedChanged(e);
        }

        private void OnSetIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            //ObjectSet.IsActiveSet = (bool)e.NewValue;

            if ((bool)e.NewValue)
            {
                RoutedEventArgs args = new RoutedEventArgs(FormulaItem.SelectedEvent);
                RaiseEvent(args);
            }
            else
            {
                RoutedEventArgs args = new RoutedEventArgs(FormulaItem.DeselectedEvent);
                RaiseEvent(args);
            }
        }

        public ObjectSetBase ObjectSet
        {
            get { return (ObjectSetBase)GetValue(ObjectSetProperty); }
            set { SetValue(ObjectSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectSetProperty =
            DependencyProperty.Register("ObjectSet", typeof(ObjectSetBase), typeof(FormulaItem), new PropertyMetadata(null, OnSetObjectSetChanged));

        private static void OnSetObjectSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fi = d as FormulaItem;
            fi.OnSetObjectSetChanged(e);
        }

        private void OnSetObjectSetChanged(DependencyPropertyChangedEventArgs e)
        {
            var item = (ObjectSetBase) e.NewValue;
            SetBinding(TitleProperty, new Binding("Title") { Source = ObjectSet, Mode = BindingMode.TwoWay });
            SetBinding(SubTitleProperty, new Binding("SubTitle") { Source = ObjectSet, Mode = BindingMode.TwoWay  });
            SetBinding(ObjectsCountTextProperty, new Binding("ObjectsCount") { Source = ObjectSet, Mode = BindingMode.TwoWay,
                Converter = new ObjectsCountToObjectsCountTextConverter() });
            SetBinding(IconProperty, new Binding("Icon") { Source = ObjectSet, Mode = BindingMode.TwoWay });
            SetBinding(IsSelectedProperty, new Binding("IsActiveSet") { Source = ObjectSet, Mode = BindingMode.TwoWay });
            item.RecomputeSet += Item_RecomputeSet;
        }

        private void Item_RecomputeSet(object sender, RecomputeSetEventArgs e)
        {
            if (e.IsSuccesed)
                IsSelected = true;
        }

        private void menuSelectThisSet_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSelected)
                IsSelected = true;
        }

        private void menuRecomputeSetContents_Click(object sender, RoutedEventArgs e)
        {
            ObjectSet?.OnRecomputeSet();
        }

        private void PresentationControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                if (!IsSelected)
                    IsSelected = true;
        }
    }

    public enum ElementLocationOfOfSequence
    {
        First, Center, Last
    }   

    public class ElementLocationOfOfSequenceToPointsCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var center = new PointCollection() {
                new System.Windows.Point(0,0),
                new System.Windows.Point(240,0),
                new System.Windows.Point(270,45),
                new System.Windows.Point(240,90),
                new System.Windows.Point(0,90),
                new System.Windows.Point(30,45),
            };

            if (value is ElementLocationOfOfSequence)
            {
                if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.First)
                {
                    return new PointCollection() {
                        new System.Windows.Point(0,0),
                        new System.Windows.Point(210,0),
                        new System.Windows.Point(240,45),
                        new System.Windows.Point(210,90),
                        new System.Windows.Point(0,90),
                    };
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Last)
                {
                    return new PointCollection() {
                        new System.Windows.Point(0,0),
                        new System.Windows.Point(240,0),
                        new System.Windows.Point(240,90),
                        new System.Windows.Point(0,90),
                        new System.Windows.Point(30,45),
                    };
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Center)
                {
                    return center;
                }
            }

            return center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ElementLocationOfOfSequenceToMarginLeftControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ElementLocationOfOfSequence)
            {
                if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.First)
                {
                    return new Thickness(0, 0, 0, 0);
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Last)
                {
                    return new Thickness(-30, 0, 0, 0);
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Center)
                {
                    return new Thickness(-30, 0, 0, 0);
                }
            }

            return new Thickness(-30, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ElementLocationOfOfSequenceToMarginLeftGridIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ElementLocationOfOfSequence)
            {
                if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.First)
                {
                    return new Thickness(0, 0, 0, 0);
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Last)
                {
                    return new Thickness(30, 0, 0, 0);
                }
                else if ((ElementLocationOfOfSequence)value == ElementLocationOfOfSequence.Center)
                {
                    return new Thickness(30, 0, 0, 0);
                }
            }

            return new Thickness(30, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IconWidthToMaxWidthContentPanelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 200 - double.Parse(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

    internal class ObjectsCountToObjectsCountTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() + " " + Properties.Resources.objects;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
