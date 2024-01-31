using GPAS.Ontology;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls;
using GPAS.Workspace.Presentation.Controls.Map.Heatmap;
using GPAS.Workspace.Presentation.Controls.OntologyPickers;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Helpers.Map
{
    public class LayerShowReverseStateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LayerShowState)value)
            {
                case LayerShowState.Hidden:
                    return true;
                case LayerShowState.Shown:
                    return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return LayerShowState.Hidden;
                else
                    return LayerShowState.Shown;
            }
            return LayerShowState.Shown;
        }
    }

    public class LayerShowStateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LayerShowState)value)
            {
                case LayerShowState.Shown:
                    return true;
                case LayerShowState.Hidden:
                    return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return LayerShowState.Shown;
                else
                    return LayerShowState.Hidden;
            }
            return LayerShowState.Hidden;
        }
    }

    public class HeatmapTargetToTargetPointsRadioButtonGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((TargetPoints)value)
            {
                case TargetPoints.AllDataPoints:
                    return true;
                case TargetPoints.SelectedDataPoints:
                    (parameter as RadioButton).IsChecked = true;
                    return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return TargetPoints.AllDataPoints;
                else
                    return TargetPoints.SelectedDataPoints;
            }
            return TargetPoints.AllDataPoints;
        }
    }

    public class HeatmapPointsSourceToPointsValueSourceTypeGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((PointsValueSourceType)value)
            {
                case PointsValueSourceType.ObjectsCount:
                    return true;
                case PointsValueSourceType.ValueOfSelectedProperty:
                    (parameter as RadioButton).IsChecked = true;
                    return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return PointsValueSourceType.ObjectsCount;
                else
                    return PointsValueSourceType.ValueOfSelectedProperty;
            }
            return PointsValueSourceType.ObjectsCount;
        }
    }

    public class DensityRadiusInMetersToSliderDensityRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Math.Log10((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Math.Pow(10, (double)value);
        }
    }

    public class DensityRadiusInMetersToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Math.Pow(10, (double)value) / 1000;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val;
            double.TryParse(value.ToString(), out val);
            val *= 1000.0;
            val = val > 1000000 ? 1000000 : val;
            return Math.Log10(val);
        }
    }

    public class ArealUnitInSquareMetersToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((double)value / 1000000.0).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val;
            double.TryParse(value.ToString(), out val);
            return val * 1000000.0;
        }
    }

    internal class HeatmapWarningArealUnitNotMatchRadiusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Visibility)value)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    return false;
            }
            return false;
        }
    }

    /// <summary>
    /// Interaction logic for HeatmapHelper.xaml
    /// </summary>
    public partial class HeatmapHelper
    {
        public HeatmapHelper()
        {
            InitializeComponent();
            Reset();
        }

        MapControl BindingMapControl = null;
        List<Entities.KWObject> KWObjectList;

        public void Init(MapControl bindingMapControl)
        {
            if (BindingMapControl != null)
                throw new InvalidOperationException(Properties.Resources.Control_Is_Data_Initialized_Before);
            BindingMapControl = bindingMapControl;

            BindingMapControl.ObjectsSelectionChanged += BindingMapControl_ObjectsSelectionChanged;

            Binding HeatmapStatusBinding = new Binding("HeatmapStatus");
            HeatmapStatusBinding.Source = BindingMapControl;
            HeatmapStatusBinding.Mode = BindingMode.TwoWay;
            HeatmapStatusBinding.Converter = new LayerShowReverseStateToBooleanConverter();
            toggleHideShow.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, HeatmapStatusBinding);

            Binding HeatmapShowMapPointsAndLabelsBinding = new Binding("HeatmapShowMapPointsAndLabels");
            HeatmapShowMapPointsAndLabelsBinding.Source = BindingMapControl;
            HeatmapShowMapPointsAndLabelsBinding.Mode = BindingMode.TwoWay;
            HeatmapShowMapPointsAndLabelsBinding.Converter = new LayerShowReverseStateToBooleanConverter();
            checkBoxHideMapPointsAndLabeles.SetBinding(CheckBox.IsCheckedProperty, HeatmapShowMapPointsAndLabelsBinding);

            Binding HeatmapShowScaleOnMapBinding = new Binding("HeatmapShowScaleOnMap");
            HeatmapShowScaleOnMapBinding.Source = BindingMapControl;
            HeatmapShowScaleOnMapBinding.Mode = BindingMode.TwoWay;
            HeatmapShowScaleOnMapBinding.Converter = new LayerShowStateToBooleanConverter();
            checkBoxShowScaleOnMap.SetBinding(CheckBox.IsCheckedProperty, HeatmapShowScaleOnMapBinding);

            Binding HeatmapTargetBinding = new Binding("HeatmapTarget");
            HeatmapTargetBinding.Source = BindingMapControl;
            HeatmapTargetBinding.Mode = BindingMode.TwoWay;
            HeatmapTargetBinding.Converter = new HeatmapTargetToTargetPointsRadioButtonGroupConverter();
            HeatmapTargetBinding.ConverterParameter = rbtSelectedDataPoints;
            rbtAllDataPoints.SetBinding(RadioButton.IsCheckedProperty, HeatmapTargetBinding);

            Binding HeatmapPointsSourceBinding = new Binding("HeatmapPointsSource");
            HeatmapPointsSourceBinding.Source = BindingMapControl;
            HeatmapPointsSourceBinding.Mode = BindingMode.TwoWay;
            HeatmapPointsSourceBinding.Converter = new HeatmapPointsSourceToPointsValueSourceTypeGroupConverter();
            HeatmapPointsSourceBinding.ConverterParameter = rbtPointsValueSourceTypeSelectedProperty;
            rbtPointsValueSourceTypeCount.SetBinding(RadioButton.IsCheckedProperty, HeatmapPointsSourceBinding);

            Binding SliderHeatmapDensityRadiusInMetersBinding = new Binding("HeatmapDensityRadiusInMeters");
            SliderHeatmapDensityRadiusInMetersBinding.Source = BindingMapControl;
            SliderHeatmapDensityRadiusInMetersBinding.Mode = BindingMode.TwoWay;
            SliderHeatmapDensityRadiusInMetersBinding.Converter = new DensityRadiusInMetersToSliderDensityRadiusConverter();
            sliderDensityRadius.SetBinding(Slider.ValueProperty, SliderHeatmapDensityRadiusInMetersBinding);

            Binding HeatmapPointsCountBinding = new Binding("HeatmapPointsCount");
            HeatmapPointsCountBinding.Source = BindingMapControl;
            HeatmapPointsCountBinding.Mode = BindingMode.OneWay;
            HeatmapPointsCountBinding.NotifyOnTargetUpdated = true;
            textBlockPointsCount.SetBinding(TextBlock.TextProperty, HeatmapPointsCountBinding);
            textBlockPointsCount.TargetUpdated += TextBlockPointsCount_TargetUpdated;

            Binding HeatmapColorSpectrumBinding = new Binding("HeatmapColorSpectrum");
            HeatmapColorSpectrumBinding.Source = BindingMapControl;
            HeatmapColorSpectrumBinding.Mode = BindingMode.TwoWay;
            sscSpectrum.SetBinding(SpectrumSliderControl.ColorSpectrumProperty, HeatmapColorSpectrumBinding);
            ColorSpectrumComboBox.SelectedIndex = 0;

            Binding HeatmapOpacityBinding = new Binding("HeatmapOpacity");
            HeatmapOpacityBinding.Source = BindingMapControl;
            HeatmapOpacityBinding.Mode = BindingMode.TwoWay;
            sliderOpacity.SetBinding(Slider.ValueProperty, HeatmapOpacityBinding);

            Binding HeatmapArealUnitInSquareMetersBinding = new Binding("HeatmapArealUnitInSquareMeters");
            HeatmapArealUnitInSquareMetersBinding.Source = BindingMapControl;
            HeatmapArealUnitInSquareMetersBinding.Mode = BindingMode.TwoWay;
            HeatmapArealUnitInSquareMetersBinding.Converter = new ArealUnitInSquareMetersToStringConverter();
            textBoxArealUnitValue.SetBinding(TextBox.TextProperty, HeatmapArealUnitInSquareMetersBinding);

            Binding HeatmapWarningArealUnitNotMatchRadiusBinding = new Binding("HeatmapWarningArealUnitNotMatchRadius");
            HeatmapWarningArealUnitNotMatchRadiusBinding.Source = BindingMapControl;
            HeatmapWarningArealUnitNotMatchRadiusBinding.Mode = BindingMode.OneWay;
            HeatmapWarningArealUnitNotMatchRadiusBinding.Converter = new HeatmapWarningArealUnitNotMatchRadiusToVisibilityConverter();
            labelWarningArealUnitNotMatchRadiusMessage.SetBinding(Label.VisibilityProperty, HeatmapWarningArealUnitNotMatchRadiusBinding);

            Binding HeatmapPropertyBasedPointsSelectedTypeUriBinding = new Binding("HeatmapPropertyBasedPointsSelectedTypeUri");
            HeatmapPropertyBasedPointsSelectedTypeUriBinding.Source = BindingMapControl;
            HeatmapPropertyBasedPointsSelectedTypeUriBinding.Mode = BindingMode.OneWayToSource;
            HeatmapPropertyBasedPointsSelectedTypeUriBinding.Converter = new PropertyNodeToTypeUriConverter();
            PointsWeightPropertyTypePicker.SetBinding(PropertyTypePicker.SelectedItemProperty, HeatmapPropertyBasedPointsSelectedTypeUriBinding);

            Binding HeatmapMostDensityBinding = new Binding("HeatmapMostDensity");
            HeatmapMostDensityBinding.Source = BindingMapControl;
            HeatmapMostDensityBinding.Mode = BindingMode.TwoWay;
            sscSpectrum.SetBinding(SpectrumSliderControl.MaximumProperty, HeatmapMostDensityBinding);

            Binding HeatmapProgressPercentBinding = new Binding("HeatmapProgressPercent");
            HeatmapProgressPercentBinding.Source = BindingMapControl;
            HeatmapProgressPercentBinding.Mode = BindingMode.OneWay;
            prgLoadHeatmap.SetBinding(ProgressBar.ValueProperty, HeatmapProgressPercentBinding);

            Binding HeatmapHintedPixelBinding = new Binding("HeatmapHintedPixel");
            HeatmapHintedPixelBinding.Source = BindingMapControl;
            HeatmapHintedPixelBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(HeatmapHelper.HeatmapHintedPixelProperty, HeatmapHintedPixelBinding);
            sscSpectrum.ColorSpectrum = (((ColorSpectrumComboBox.SelectedItem as ComboBoxItem).Content as StackPanel).Children[0] as Border).Background;
        }

        private void BindingMapControl_ObjectsSelectionChanged(object sender, Controls.Map.ObjectsSelectionChangedArgs e)
        {
            if (rbtSelectedDataPoints.IsChecked == true)
                KWObjectList = BindingMapControl.GetSelectedObjects();
            else
                KWObjectList = BindingMapControl.GetShowingObjects();

            GetPropertiesForPointsWeightPropertyTypePicker(KWObjectList);
        }

        private void GetPropertiesForPointsWeightPropertyTypePicker(List<KWObject> selectedObjects)
        {
            BaseDataTypes dataType = BaseDataTypes.None;
            if (PointsWeightPropertyTypePicker.SelectedItem != null)
                dataType = PointsWeightPropertyTypePicker.SelectedItem.BaseDataType;

            if (selectedObjects != null)
            {
                List<string> typeURIList = (from so in selectedObjects select so.TypeURI).Distinct().ToList();
                PreparePropertyPicker(typeURIList);
            }
        }

        private void PreparePropertyPicker(IEnumerable<string> objectList)
        {
            PointsWeightPropertyTypePicker.ObjectTypeUriCollection = new ObservableCollection<string>(objectList);

            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            if (!PointsWeightPropertyTypePicker.ExceptTypeUriCollection.Contains(ontology.GetDefaultDisplayNamePropertyTypeUri()))
                PointsWeightPropertyTypePicker.ExceptTypeUriCollection.Add(ontology.GetDefaultDisplayNamePropertyTypeUri());

            PointsWeightPropertyTypePicker.DataTypeFilterCollection = new ObservableCollection<BaseDataTypes>()
            {
                BaseDataTypes.Double, BaseDataTypes.Int, BaseDataTypes.Long
            };
        }

        private void TextBlockPointsCount_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            sscSpectrum.ValueMode = double.Parse(textBlockPointsCount.Text) > 0;
            if (double.Parse(textBlockPointsCount.Text) == 0)
            {
                sscSpectrum.TickVisibility = Visibility.Collapsed;
            }

            SetPointsWeightPropertyTypePickerEnabled();
        }


        private void toggleHideShow_Checked(object sender, RoutedEventArgs e)
        {
            toggleHideShow.ToolTip = Properties.Resources.Show_Heatmap;
            stkpLegendInner.Visibility = Visibility.Collapsed;
            lblNoDataToGenerateHeatmap.Visibility = Visibility.Visible;
        }

        private void toggleHideShow_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleHideShow.ToolTip = Properties.Resources.Hide_Heatmap;
            stkpLegendInner.Visibility = Visibility.Visible;
            lblNoDataToGenerateHeatmap.Visibility = Visibility.Collapsed;
        }

        string lastCorrectArealUnit = "1";
        private void textBoxArealUnitValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            double au = 0;
            if (double.TryParse(textBoxArealUnitValue.Text, out au))
            {
                textBoxArealUnitValue.BorderBrush = new SolidColorBrush(theme.PrimaryMid.Color);
            }
            else
            {
                textBoxArealUnitValue.BorderBrush = Brushes.OrangeRed;
            }
        }

        private void textBoxArealUnitValue_LostFocus(object sender, RoutedEventArgs e)
        {
            double au = 0;
            if (double.TryParse(textBoxArealUnitValue.Text, out au))
            {
                lastCorrectArealUnit = textBoxArealUnitValue.Text;
            }
            else
            {
                textBoxArealUnitValue.Text = lastCorrectArealUnit;
            }
        }

        string lastCorrectRadius = "10";
        readonly PaletteHelper paletteHelper = new PaletteHelper();
        private void textBoxRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            double r = 0;
            if (double.TryParse(textBoxRadius.Text, out r))
            {
                textBoxRadius.BorderBrush = new SolidColorBrush(theme.PrimaryMid.Color);
            }
            else
            {
                textBoxRadius.BorderBrush = Brushes.OrangeRed;
            }
        }

        private void textBoxRadius_LostFocus(object sender, RoutedEventArgs e)
        {
            double r = 0;
            if (double.TryParse(textBoxRadius.Text, out r))
            {
                lastCorrectRadius = textBoxRadius.Text;
            }
            else
            {
                textBoxRadius.Text = lastCorrectRadius;
            }
        }

        private void prgLoadHeatmap_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (prgLoadHeatmap.Value == 0 || prgLoadHeatmap.Value == 100)
            {
                HeatMapTileLoadingBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeatMapTileLoadingBox.Visibility = Visibility.Visible;
            }
        }

        private void rbtPointsValueSourceTypeSelectedProperty_Checked(object sender, RoutedEventArgs e)
        {
            SetPointsWeightPropertyTypePickerEnabled();
            labelCorrectValueMessage.Visibility = Visibility.Visible;
        }

        private async void SetPointsWeightPropertyTypePickerEnabled()
        {
            long pc = 0;
            long.TryParse(textBlockPointsCount.Text, out pc);

            if (pc > 0 && rbtPointsValueSourceTypeSelectedProperty.IsChecked == true)
            {
                PointsWeightPropertyTypePicker.IsEnabled = true;
                await BindingMapControl.SetMarkersWeight();
            }
            else
            {
                PointsWeightPropertyTypePicker.IsEnabled = false;
            }
        }

        private void rbtPointsValueSourceTypeSelectedProperty_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPointsWeightPropertyTypePickerEnabled();
            labelCorrectValueMessage.Visibility = Visibility.Collapsed;
        }

        private void rbtSelectedDataPoints_Checked(object sender, RoutedEventArgs e)
        {
            KWObjectList = BindingMapControl?.GetSelectedObjects();
            if (PointsWeightPropertyTypePicker != null)
                GetPropertiesForPointsWeightPropertyTypePicker(KWObjectList);
        }

        private void rbtAllDataPoints_Checked(object sender, RoutedEventArgs e)
        {
            KWObjectList = BindingMapControl?.GetShowingObjects();
            if (PointsWeightPropertyTypePicker != null)
                GetPropertiesForPointsWeightPropertyTypePicker(KWObjectList);
        }


        /// <summary>
        /// مقدار عددی نقطه مشخص شده در تصویر هیت مپ 
        /// </summary>
        public double HeatmapHintedPixel
        {
            get { return (double)GetValue(HeatmapHintedPixelProperty); }
            set { SetValue(HeatmapHintedPixelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeatmapHintedPixel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeatmapHintedPixelProperty =
            DependencyProperty.Register("HeatmapHintedPixel", typeof(double), typeof(HeatmapHelper), new PropertyMetadata(0.0, OnSetHeatmapHintedPixelChanged));

        private static void OnSetHeatmapHintedPixelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeatmapHelper hh = d as HeatmapHelper;
            hh.OnSetHeatmapHintedPixelChanged(e);
        }

        private void OnSetHeatmapHintedPixelChanged(DependencyPropertyChangedEventArgs e)
        {
            double val = (double)e.NewValue;

            double rVal = Math.Round(val);

            if (rVal < 10)
            {
                sscSpectrum.Value = Math.Round(val, 10);
            }
            else if (rVal < 1000)
            {
                sscSpectrum.Value = Math.Round(val, 5);
            }
            else if (rVal < 100000)
            {
                sscSpectrum.Value = Math.Round(val, 2);
            }
            else
            {
                sscSpectrum.Value = Math.Round(val, 1);
            }

            if (val > 0)
            {
                sscSpectrum.TickVisibility = Visibility.Visible;
            }
            else
            {
                sscSpectrum.TickVisibility = Visibility.Collapsed;
            }
        }

        private void ColorSpectrumComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            sscSpectrum.ColorSpectrum = (((comboBox.SelectedItem as ComboBoxItem).Content as StackPanel).Children[0] as Border).Background;
        }

        public override void Reset()
        {
            BindingMapControl?.Reset();
            PointsWeightPropertyTypePicker.RemoveSelectedItem();
            ColorSpectrumComboBox.SelectedIndex = 0;
        }
    }
}
