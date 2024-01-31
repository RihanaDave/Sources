using GPAS.Workspace.Logic;
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
using System.Windows.Shapes;

namespace GPAS.Workspace.Presentation.Windows.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for PropertyMatchedValueFilterWindow.xaml
    /// </summary>
    public partial class PropertyMatchedValueFilterWindow : Window
    {
        #region Property

        public string SearchValue { get; set; }
        public string TypeUri { get; set; }

        public bool IsCancel { get; set; }


        public string TitlePropertySelected
        {
            get { return (string)GetValue(TitlePropertySelectedProperty); }
            set { SetValue(TitlePropertySelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitlePropertySelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitlePropertySelectedProperty =
            DependencyProperty.Register(nameof(TitlePropertySelected), typeof(string),
                typeof(PropertyMatchedValueFilterWindow), new PropertyMetadata(string.Empty));


        #endregion

        #region Method
        public PropertyMatchedValueFilterWindow(string title, string typeUri)
        {
            InitializeComponent();
            TitlePropertySelected = title;
            TypeUri = typeUri;
            init();
        }

        private void init()
        {
            PropertyValueTemplatesControl.PrepareControl(OntologyProvider.GetBaseDataTypeOfProperty(TypeUri));
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }
        #endregion

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SearchValue = PropertyValueTemplatesControl.GetPropertyValue();
            IsCancel = false;
            Close();            
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCancel = true;
            Close();
        }
        

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!(e.OriginalSource is System.Windows.Controls.Primitives.CalendarDayButton))
                {
                    SearchValue = PropertyValueTemplatesControl.GetPropertyValue();
                    IsCancel = false;
                    Close();
                }
            }
        }
    }
}