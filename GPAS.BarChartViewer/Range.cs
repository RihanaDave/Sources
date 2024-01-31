using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.BarChartViewer
{
    public class Range : DependencyObject
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;

        public void OnRangeChanged(RangeChangedEventArgs args)
        {
            RangeChanged?.Invoke(this, args);
        }

        public double Start
        {
            get { return (double)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Start.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(double), typeof(Range), new PropertyMetadata(0.0, OnSetStartChanged));

        private static void OnSetStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Range).OnSetStartChanged(e);
        }

        private void OnSetStartChanged(DependencyPropertyChangedEventArgs e)
        {
            OnRangeChanged(new RangeChangedEventArgs((double)e.NewValue, End, (double)e.OldValue, End));
        }

        public double End
        {
            get { return (double)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        // Using a DependencyProperty as the backing store for End.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(double), typeof(Range), new PropertyMetadata(0.0, OnSetEndChanged));

        private static void OnSetEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Range).OnSetEndChanged(e);
        }

        private void OnSetEndChanged(DependencyPropertyChangedEventArgs e)
        {
            OnRangeChanged(new RangeChangedEventArgs(Start, (double)e.NewValue, Start, (double)e.OldValue));
        }
    }
}
