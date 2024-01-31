using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for SelectorCsvSepratorWindow.xaml
    /// </summary>
    public partial class SelectorCsvSepratorWindow : Window
    {
        private char PreSelectSeprator { get; set; }
        private char SelectSeprator { get; set; }
        private bool IsCanceled { get; set; } = true;


        public CsvDataSourceModel DataSource
        {
            get { return (CsvDataSourceModel)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register(nameof(DataSource), typeof(CsvDataSourceModel), typeof(SelectorCsvSepratorWindow),
                new PropertyMetadata(null));


        public SelectorCsvSepratorWindow(DataImportViewModel context)
        {
            InitializeComponent();
            if (context.SelectedDataSource is CsvDataSourceModel)
            {
               DataSource = context.SelectedDataSource as CsvDataSourceModel;
            }
            PreSelectSeprator = DataSource.Separator;
            SelectSeprator = DataSource.Separator;
            RefreshSelectedSeparator();
        }

        private void RefreshSelectedSeparator()
        {
            switch (DataSource.Separator)
            {
                case ',':
                    SeparatorCombobox.SelectedItem = ColonSeparatorComboBoxItem;
                    SelectSeprator = ',';
                    break;
                case ';':
                    SeparatorCombobox.SelectedItem = SemicolonSeparatorComboBoxItem;
                    SelectSeprator = ';';
                    break;
                case '\u001B':
                    SeparatorCombobox.SelectedItem = EscSeparatorComboBoxItem;
                    SelectSeprator = '\u001B';
                    break;
                case '\t':
                    SeparatorCombobox.SelectedItem = TabSeparatorComboBoxItem;
                    SelectSeprator = '\t';
                    break;
                default:
                    SeparatorCombobox.SelectedItem = CustomSeparatorComboBoxItem;
                    CustomTextBox.Text = DataSource.Separator.ToString();
                    break;
            }
        }

        private void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomTextBox.Text!=string.Empty)
            {
                DataSource.Separator = SelectSeprator=CustomTextBox.Text[0];
            }
          
        }


        private void SeparatorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBoxItem)SeparatorCombobox.SelectedItem).Content)
            {
                case ",":
                    SeparatorCombobox.SelectedItem = ColonSeparatorComboBoxItem.Content;
                    SelectSeprator = ',';
                    break;
                case ";":
                    SeparatorCombobox.SelectedItem = SemicolonSeparatorComboBoxItem;
                    SelectSeprator = ';';
                    break;
                case "Esc":
                    SeparatorCombobox.SelectedItem = EscSeparatorComboBoxItem;
                    SelectSeprator = '\u001B';
                    break;
                case "Tab":
                    SeparatorCombobox.SelectedItem = TabSeparatorComboBoxItem;
                    SelectSeprator = '\t';
                    break;
                default:
                    SeparatorCombobox.SelectedItem = CustomSeparatorComboBoxItem;                           
                    break;
            }
            DataSource.Separator = SelectSeprator;
            CustomTextBox.Text = string.Empty;
        }

        private void SelectButon_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsCanceled = true;
            Close();
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
    }
}
