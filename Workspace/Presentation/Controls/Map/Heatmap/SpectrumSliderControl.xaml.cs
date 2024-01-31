using System;
using System.Collections.Generic;
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

namespace GPAS.Workspace.Presentation.Controls.Map.Heatmap
{
    /// <summary>
    /// Interaction logic for SpectrumSliderControl.xaml
    /// </summary>
    public partial class SpectrumSliderControl : UserControl
    {
        public SpectrumSliderControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        Label lblSpectrum = null;
        Label lblTick = null;
        Label lblMinimum = null;
        Label lblMaximum = null;

        private void SetTickPosition()
        {
            if (lblSpectrum == null)
                return;

            double d = Maximum - Minimum;
            double w = lblSpectrum.ActualWidth;
            double a = (d == 0) ? 0 : w / d;
            lblTick.Margin = new Thickness((Value - Minimum) * a, lblTick.Margin.Top, lblTick.Margin.Right, lblTick.Margin.Bottom);
        }

        private void SetBindingMinimum()
        {
            if (ValueMode)
            {
                Binding bindingMin = new Binding("Minimum");
                bindingMin.Source = this;
                bindingMin.Mode = BindingMode.TwoWay;
                lblMinimum.SetBinding(Label.ContentProperty, bindingMin);
            }
            else
            {
                lblMinimum.ClearValue(Label.ContentProperty);
                lblMinimum.Content = MinimumText;
            }
        }

        private void SetBindingMaximum()
        {
            if (ValueMode)
            {
                Binding bindingMax = new Binding("Maximum");
                bindingMax.Source = this;
                bindingMax.Mode = BindingMode.TwoWay;
                lblMaximum.SetBinding(Label.ContentProperty, bindingMax);
            }
            else
            {
                lblMaximum.ClearValue(Label.ContentProperty);
                lblMaximum.Content = MaximumText;
            }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SpectrumSliderControl), new PropertyMetadata(0.0, OnSetValueChanged));

        private static void OnSetValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpectrumSliderControl spectrumSliderControl = d as SpectrumSliderControl;
            spectrumSliderControl.OnSetValueChanged(e);
        }

        private void OnSetValueChanged(DependencyPropertyChangedEventArgs e)
        {
            double val = (double)e.NewValue;
            if (val < Minimum)
                Value = Minimum;
            else if (val > Maximum)
                Value = Maximum;
            else
                SetTickPosition();
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SpectrumSliderControl), new PropertyMetadata(0.0, OnSetMinimumChanged));

        private static void OnSetMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpectrumSliderControl spectrumSliderControl = d as SpectrumSliderControl;
            spectrumSliderControl.OnSetMinimumChanged(e);
        }

        private void OnSetMinimumChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Minimum > Maximum)
                Maximum = Minimum;
            else
            {
                if (Value < Minimum)
                    Value = Minimum;
                else
                    SetTickPosition();
            }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SpectrumSliderControl), new PropertyMetadata(0.0, OnSetMaximumChanged));

        private static void OnSetMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpectrumSliderControl spectrumSliderControl = d as SpectrumSliderControl;
            spectrumSliderControl.OnSetMaximumChanged(e); 
        }

        private void OnSetMaximumChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Minimum > Maximum)
                Minimum = Maximum;
            else
            {
                if (Value > Maximum)
                    Value = Maximum;
                else
                    SetTickPosition();
            }
        }

        public Brush ColorSpectrum
        {
            get { return (Brush)GetValue(ColorSpectrumProperty); }
            set { SetValue(ColorSpectrumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorSpectrum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorSpectrumProperty =
            DependencyProperty.Register("ColorSpectrum", typeof(Brush), typeof(SpectrumSliderControl), new PropertyMetadata(Brushes.Transparent));

        public Visibility TickVisibility
        {
            get { return (Visibility)GetValue(TickVisibilityProperty); }
            set { SetValue(TickVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TickVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TickVisibilityProperty =
            DependencyProperty.Register("TickVisibility", typeof(Visibility), typeof(SpectrumSliderControl), new PropertyMetadata(Visibility.Visible));

        public String MinimumText
        {
            get { return (String)GetValue(MinimumTextProperty); }
            set { SetValue(MinimumTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumTextProperty =
            DependencyProperty.Register("MinimumText", typeof(String), typeof(SpectrumSliderControl), new PropertyMetadata("Minimum"));

        public String MaximumText
        {
            get { return (String)GetValue(MaximumTextProperty); }
            set { SetValue(MaximumTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumTextProperty =
            DependencyProperty.Register("MaximumText", typeof(String), typeof(SpectrumSliderControl), new PropertyMetadata("Maximum"));


        public String ScaleText
        {
            get { return (String)GetValue(ScaleTextProperty); }
            set { SetValue(ScaleTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScaleText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScaleTextProperty =
            DependencyProperty.Register("ScaleText", typeof(String), typeof(SpectrumSliderControl), new PropertyMetadata("Scale"));

        /// <summary>
        /// در صورت true بودن مقدار عددی Minimum و Maximum به نمایش در می آید و در صورت false بودن مقدار MinimumText و MaximumText به نمایش در می آید.
        /// </summary>
        public Boolean ValueMode
        {
            get { return (Boolean)GetValue(ValueModeProperty); }
            set { SetValue(ValueModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueModeProperty =
            DependencyProperty.Register("ValueMode", typeof(Boolean), typeof(SpectrumSliderControl), new PropertyMetadata(true, OnSetValueModeChanged));

        private static void OnSetValueModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SpectrumSliderControl spectrumSliderControl = d as SpectrumSliderControl;
            spectrumSliderControl.OnSetValueModeChanged(e);
        }

        private void OnSetValueModeChanged(DependencyPropertyChangedEventArgs e)
        {
            bool val = (bool)e.NewValue;
            if (lblMaximum == null || lblMinimum == null)
                return;

            SetBindingMinimum();
            SetBindingMaximum();
        }

        private void lblSpectrum_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetTickPosition();
        }

        private void lblSpectrum_Initialized(object sender, EventArgs e)
        {
            lblSpectrum = sender as Label;
        }

        private void lblTick_Initialized(object sender, EventArgs e)
        {
            lblTick = sender as Label;
        }

        private void lblMinimum_Initialized(object sender, EventArgs e)
        {
            lblMinimum = sender as Label;
            SetBindingMinimum();
        }

        private void lblMaximum_Initialized(object sender, EventArgs e)
        {
            lblMaximum = sender as Label;
            SetBindingMaximum();
        }

        private void txbValue_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock txbValue = sender as TextBlock;
            txbValue.Margin = new Thickness(txbValue.ActualWidth / -2, 0, 0, 0);
        }
    }
}
