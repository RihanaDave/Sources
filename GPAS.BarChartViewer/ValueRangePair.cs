using System;
using System.Windows;

namespace GPAS.BarChartViewer
{
    public class ValueRangePair : Range
    {
        const double MinimumValue = ((double)decimal.MinValue / 2) + (19791209299969);
        const double MaximumValue = ((double)decimal.MaxValue / 2) - (19791209299969);

        public ValueRangePair()
        {
            RangeChanged += ValueRangePair_RangeChanged;
        }

        private void ValueRangePair_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            GenerateLabel();
        }

        private void GenerateLabel()
        {
            Label = "[" + Start + ", " + End + ")\n" + VerticalAxisLabel + ": " + Value;
        }

        public string Label { get; set; }

        public string VerticalAxisLabel
        {
            get { return (string)GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalAxisLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalAxisLabelProperty =
            DependencyProperty.Register("VerticalAxisLabel", typeof(string), typeof(ValueRangePair), new PropertyMetadata("", OnSetVerticalAxisLabelChanged));

        private static void OnSetVerticalAxisLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ValueRangePair).OnSetVerticalAxisLabelChanged(e);
        }

        private void OnSetVerticalAxisLabelChanged(DependencyPropertyChangedEventArgs e)
        {
            GenerateLabel();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ValueRangePair), new PropertyMetadata(0.0, OnSetValueChanged));

        private static void OnSetValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ValueRangePair).OnSetValueChanged(e);
        }

        private void OnSetValueChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Value < MinimumValue)
            {
                if (e.OldValue != null)
                    Value = (double)e.OldValue < MinimumValue ? MinimumValue : (double)e.OldValue;
                else
                    Value = 0;
                throw new IndexOutOfRangeException("Value can't be less than " + MinimumValue);
            }

            if (Value > MaximumValue)
            {
                if (e.OldValue != null)
                    Value = (double)e.OldValue > MaximumValue ? MaximumValue : (double)e.OldValue;
                else
                    Value = 0;
                throw new IndexOutOfRangeException("Value can't be greater than " + MaximumValue);
            }

            GenerateLabel();
        }
    }
}
