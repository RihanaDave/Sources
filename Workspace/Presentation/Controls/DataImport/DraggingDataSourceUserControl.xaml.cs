using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public partial class DraggingDataSourceUserControl 
    {
        public ObservableCollection<BitmapSource> IconCollection
        {
            get => (ObservableCollection<BitmapSource>)GetValue(IconCollectionProperty);
            set => SetValue(IconCollectionProperty, value);
        }

        public static readonly DependencyProperty IconCollectionProperty =
            DependencyProperty.Register(nameof(IconCollection), typeof(ObservableCollection<BitmapSource>),
                typeof(DraggingDataSourceUserControl));


        public DraggingDataSourceUserControl()
        {
            InitializeComponent();
            IconCollection = new ObservableCollection<BitmapSource>();
        }
    }
}
