using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.RightClickMenu
{
    public class RightClickMenuItem : HeaderedItemsControl
    {
        public event RoutedEventHandler Click;

        public static readonly DependencyProperty SubMenuSectorProperty;
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty MenuIconProperty;

        [Bindable(true)]
        public double SubMenuSector
        {
            get
            {
                return (double)GetValue(SubMenuSectorProperty);
            }
            set
            {
                SetValue(SubMenuSectorProperty, value);
            }
        }

        [Bindable(true)]
        public BitmapImage Icon
        {
            get
            {
                return (BitmapImage)GetValue(MenuIconProperty);
            }
            set
            {
                SetValue(MenuIconProperty, value);
            }
        }

        [Bindable(true)]
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        double _size;

        static RightClickMenuItem()
        {
            SubMenuSectorProperty = DependencyProperty.Register("SubMenuSector", typeof(double), typeof(RightClickMenuItem), new FrameworkPropertyMetadata(120.0));
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(RightClickMenuItem), new FrameworkPropertyMetadata(null));
            MenuIconProperty = DependencyProperty.Register("IconUri", typeof(BitmapImage), typeof(RightClickMenu));
        }

        public double CalculateSize(double s, double d)
        {
            // size of current level 
            double ss = s + d;

            foreach (UIElement i in Items)
            {
                ss = Math.Max(ss, (i as RightClickMenuItem).CalculateSize(s + d, d));
            }

            _size = ss;

            return _size;
        }

        protected override Size MeasureOverride(Size availablesize)
        {
            foreach (UIElement i in Items)
            {
                i.Measure(availablesize);
            }

            return new Size(_size, _size);
        }

        protected override Size ArrangeOverride(Size finalsize)
        {
            return finalsize;
        }

        public void OnClick()
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(Header);
            }

            if (Click != null)
            {
                Click(this, new RoutedEventArgs());
            }
        }
    }
}
