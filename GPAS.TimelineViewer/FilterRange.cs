using GPAS.TimelineViewer.EventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.TimelineViewer
{
    public class FilterRange : DependencyObject
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;

        protected void OnRangeChanged(RangeChangedEventArgs args)
        {
            RangeChanged?.Invoke(this, args);
        }


        public DateTime From
        {
            get { return (DateTime)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(DateTime), typeof(FilterRange), new PropertyMetadata(new DateTime(), OnSetFromChanged));

        private static void OnSetFromChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterRange).OnSetFromChanged(e);
        }

        private void OnSetFromChanged(DependencyPropertyChangedEventArgs e)
        {
            OnRangeChanged(new RangeChangedEventArgs((DateTime)e.NewValue, To, (DateTime)e.OldValue, To));
        }

        public DateTime To
        {
            get { return (DateTime)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for To.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(DateTime), typeof(FilterRange), new PropertyMetadata(new DateTime(), OnSetToChanged));

        private static void OnSetToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterRange).OnSetToChanged(e);
        }
        private void OnSetToChanged(DependencyPropertyChangedEventArgs e)
        {
            OnRangeChanged(new RangeChangedEventArgs(From, (DateTime)e.NewValue, From, (DateTime)e.OldValue));
        }

        public TimeSpan Duration
        {
            get
            {
                return To - From;
            }
        }

        public Range ConvertToRange()
        {
            return new Range()
            {
                From = From,
                To = To,
            };
        }
    }
}
