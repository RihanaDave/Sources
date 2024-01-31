using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for RegularExpressionWindow.xaml
    /// </summary>
    public partial class RegularExpressionWindow 
    {
        /// <summary>
        /// مقدار انتخاب شده 
        /// </summary>
        public ValueMapModel PropertyValue
        {
            get => (ValueMapModel)GetValue(PropertyValueProperty);
            set => SetValue(PropertyValueProperty, value);
        }

        public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register(nameof(PropertyValue),
            typeof(ValueMapModel), typeof(RegularExpressionWindow), null);

        public RegularExpressionWindow(ValueMapModel propertyValueModel)
        {
            InitializeComponent();
            PropertyValue = propertyValueModel;
        }

        /// <summary>
        /// حرکت دادن پنجره با ماوس
        /// </summary>
        /// <param name="sender"/>
        /// <param name="e"/>
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }
        
        private void DoneButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
