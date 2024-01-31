using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for GeoSpecialFormatWindow.xaml
    /// </summary>
    public partial class GeoSpecialFormatWindow : Window
    {
        #region Propertes 


        public GeoPointPropertyMapModel GeoPointPropertyMap
        {
            get { return (GeoPointPropertyMapModel)GetValue(GeoPointPropertyMapProperty); }
            set { SetValue(GeoPointPropertyMapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GeoPointPropertyMap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeoPointPropertyMapProperty =
            DependencyProperty.Register(nameof(GeoPointPropertyMap), typeof(GeoPointPropertyMapModel), typeof(GeoSpecialFormatWindow),
                new PropertyMetadata(null));


        #endregion

        #region Methods
        public GeoSpecialFormatWindow(GeoPointPropertyMapModel geoPointPropertyMap)
        {
            InitializeComponent();
            GeoPointPropertyMap = geoPointPropertyMap;
            if (GeoPointPropertyMap.SampleValueValidationStatus == null)
            {
                PropertiesValidation.ValidationProperty validationProperty = new PropertiesValidation.ValidationProperty()
                {
                    Status = PropertiesValidation.ValidationStatus.Invalid

                };
                GeoPointPropertyMap.SampleValueValidationStatus = validationProperty;
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void ToggleButtonOnChecked(object sender, RoutedEventArgs e)
        {
            GeoPointPropertyMap.Format = (GeoTimeFormat)((RadioButton)sender).Tag;
        }

        private void OkButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
