using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.ColorPickerViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ColorPickerViewer : UserControl
    {
        public ColorPickerViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Brush SelectedColor
        {
            get { return (Brush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Brush), typeof(ColorPickerViewer), new PropertyMetadata(null, OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ColorPickerViewer).OnSelectedColorChanged(e);
        }

        private void OnSelectedColorChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSelectedColorChanged();
        }

        public event EventHandler<EventArgs> SelectedColorChanged;
        protected void OnSelectedColorChanged()
        {
            SelectedColorChanged?.Invoke(this, new EventArgs());
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDropDownOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(ColorPickerViewer), new PropertyMetadata(false));

        public bool CustomColorIsActive
        {
            get { return (bool)GetValue(CustomColorIsActiveProperty); }
            set { SetValue(CustomColorIsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CustomColorIsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CustomColorIsActiveProperty =
            DependencyProperty.Register("CustomColorIsActive", typeof(bool), typeof(ColorPickerViewer), new PropertyMetadata(true));


        public event RoutedEventHandler Click;
        protected virtual void OnClick(RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void CreateCustomColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.IsDropDownOpen = false;
            Popup.IsOpen = true;
        }

        private void CancelColorEditorButton_Click(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
            ColorPicker.IsDropDownOpen = true;
        }

        private void GetColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPicker.SelectedColor = ColorEditor.SelectedColor;
            Popup.IsOpen = false;
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            OnClick(e);
        }
    }
}
