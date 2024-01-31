using System.Windows;

namespace GPAS.MapViewer
{
    /// <summary>
    /// Interaction logic for SimpleMarkerShape.xaml
    /// </summary>
    public partial class SimpleMarkerShape 
    {
        public SimpleMarkerShape()
        {
            InitializeComponent();
        }

        public object RelatedObject
        {
            get { return GetValue(RelatedObjectProperty); }
            set { SetValue(RelatedObjectProperty, value); }
        }
        public static readonly DependencyProperty RelatedObjectProperty =
            DependencyProperty.Register("RelatedObject", typeof(object), typeof(SimpleMarkerShape));

        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(SimpleMarkerShape));
        public bool IsHighlighted
        {
            get
            {
                return (bool)this.GetValue(IsHighlightedProperty);
            }
            set
            {
                this.SetValue(IsHighlightedProperty, value);
            }
        }
    }
}
