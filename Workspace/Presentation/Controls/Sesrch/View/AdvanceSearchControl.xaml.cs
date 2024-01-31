using GPAS.Workspace.Presentation.Controls.Sesrch.Validation;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.View
{
    /// <summary>
    /// Interaction logic for AdvanceSearchControl.xaml
    /// </summary>
    public partial class AdvanceSearchControl : UserControl
    {
        #region dependency     
        //public DateTime CreationDateOF
        //{
        //    get { return (DateTime)GetValue(DisplayCreationDateOF); }
        //    set { SetValue(DisplayCreationDateOF, value); }
        //}
        //public static readonly DependencyProperty DisplayCreationDateOF =
        //    DependencyProperty.Register("StartTimeCreate", typeof(DateTime), typeof(AdvanceSearchControl), new PropertyMetadata(DateTime.Now));

        #endregion
        public AdvanceSearchControl()
        {
            InitializeComponent();
        }

        public event EventHandler BackRequest;
        protected void OnBackRequest()
        {
            BackRequest?.Invoke(this, EventArgs.Empty);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
             OnBackRequest();
        }
        #region Event

        /// <summary>
        /// رویداد جستوجو
        /// </summary>
        public event EventHandler<EventArgs> SearchAllRequest;
        protected void OnSearchAllRequest()
        {
            SearchAllRequest?.Invoke(this, new EventArgs());
        }
        private void AllSearch_Click(object sender, RoutedEventArgs e)
        {
                OnSearchAllRequest();
        }
        #endregion

        #region Property


        #endregion

        private void FileSizeFromTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9]+");
            e.Handled = reg.IsMatch(e.Text);
        }
    }
}
