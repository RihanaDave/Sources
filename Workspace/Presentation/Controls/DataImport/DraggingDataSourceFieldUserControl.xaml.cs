using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for DraggingDataSourceFieldUserControl.xaml
    /// </summary>
    public partial class DraggingDataSourceFieldUserControl 
    {
        public DraggingDataSourceFieldUserControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(DraggingDataSourceFieldUserControl),
                new PropertyMetadata(null));
    }
}
