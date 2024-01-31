using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.PeripheralTools.All2CsvConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CommaSeparatorSignIndicator = ",";
        private const string SemicolonSeparatorSignIndicator = ";";
        private const string TabSeparatorSignIndicator = "(Tab)";
        private const string SpaceSeparatorSignIndicator = "(Space)";
        private const string CustomSeparatorIndicator = "(Custom separator)";

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.DispatcherUnhandledException += (sender, e) =>
            {
                MessageBox.Show(string.Format("Exception occured;\r\r{0}", e.Exception.Message), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Stop);
                e.Handled = true;
            };

            DisableAppearance();

            LoadPluginsList();
            SetBaseSeparators();
            try
            {
                OneFileToOneCsvSourceImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.UnknownFileIcon);
                OneFileToOneCsvTargetImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.CsvFileIcon);
                MultipleFilesToOneCsvSourceImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.UnknownFilesFolderIcon);
                MultipleFilesToOneCsvTargetImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.CsvFileIcon);
                FilesDirectoryToCsvDirectorySourceImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.UnknownFilesFolderIcon);
                FilesDirectoryToCsvDirectoryTargetImage.Source = IconProvider.ConvertBitmapToBitmapSource(Properties.Resources.CsvFilesFolderIcon);
            }
            catch
            { }

            ApplyEnteredCustomSeparatorValidationAppearance();
        }

        private void SetBaseSeparators()
        {
            ComboBoxItem commaSeparatorItem = new ComboBoxItem();
            commaSeparatorItem.Content = CommaSeparatorSignIndicator;
            commaSeparatorItem.Selected += (sender, e) =>
               ApplyNonCustomSeparatorSelectionAppearance();
            SeparatorsCombobox.Items.Add(commaSeparatorItem);

            ComboBoxItem semicolonSeparatorItem = new ComboBoxItem();
            semicolonSeparatorItem.Content = SemicolonSeparatorSignIndicator;
            semicolonSeparatorItem.Selected += (sender, e) =>
               ApplyNonCustomSeparatorSelectionAppearance();
            SeparatorsCombobox.Items.Add(semicolonSeparatorItem);

            ComboBoxItem tabSeparatorItem = new ComboBoxItem();
            tabSeparatorItem.Content = TabSeparatorSignIndicator;
            tabSeparatorItem.Selected += (sender, e) =>
               ApplyNonCustomSeparatorSelectionAppearance();
            SeparatorsCombobox.Items.Add(tabSeparatorItem);

            ComboBoxItem spaceSeparatorItem = new ComboBoxItem();
            spaceSeparatorItem.Content = SpaceSeparatorSignIndicator;
            spaceSeparatorItem.Selected += (sender, e) =>
               ApplyNonCustomSeparatorSelectionAppearance();
            SeparatorsCombobox.Items.Add(spaceSeparatorItem);

            ComboBoxItem customSeparatorItem = new ComboBoxItem();
            customSeparatorItem.Content = CustomSeparatorIndicator;
            customSeparatorItem.Selected += (sender, e) =>
                ApplyCustomSeparatorSelectionAppearance();
            SeparatorsCombobox.Items.Add(customSeparatorItem);

            SeparatorsCombobox.SelectedIndex = 0;
        }

        private void ApplyCustomSeparatorSelectionAppearance()
        {
            CustomSeparatorTextbox.Text = "";
            CustomSeparatorTextbox.Visibility = Visibility.Visible;
            CustomSeparatorTextbox.Focus();
        }

        private void ApplyNonCustomSeparatorSelectionAppearance()
        {
            CustomSeparatorTextbox.Text = "";
            CustomSeparatorTextbox.Visibility = Visibility.Hidden;
        }

        private void LoadPluginsList()
        {
            PluginsCombobox.Items.Clear();

            if (Directory.Exists("." + Properties.Settings.Default.PluginsFolderRelativePath))
                foreach (var item in Directory.GetFiles("." + Properties.Settings.Default.PluginsFolderRelativePath))
                {
                    if (!Path.GetExtension(item).Equals(Properties.Settings.Default.PluginFilesExtension, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    PluginsCombobox.Items.Add(Path.GetFileNameWithoutExtension(item));
                }

            if (PluginsCombobox.Items.Count > 0)
                EnableAppearance();
            else
            {
                DisableAppearance();
                PluginsCombobox.Items.Add("(No Plug-in detected)");
            }

            PluginsCombobox.SelectedIndex = 0;
        }

        private void DisableAppearance()
        {
            PluginsCombobox.IsEnabled =
                SeparatorsCombobox.IsEnabled =
                CustomSeparatorTextbox.IsEnabled =
                OneFileToOneCsvButton.IsEnabled =
                MultipleFilesToOneCsvButton.IsEnabled =
                FilesDirectoryToCsvDirectoryButton.IsEnabled = false;

        }

        private void EnableAppearance()
        {
            PluginsCombobox.IsEnabled =
                SeparatorsCombobox.IsEnabled =
                CustomSeparatorTextbox.IsEnabled =
                OneFileToOneCsvButton.IsEnabled =
                MultipleFilesToOneCsvButton.IsEnabled =
                FilesDirectoryToCsvDirectoryButton.IsEnabled = true;
        }

        private void CustomSeparatorTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomSeparatorTextbox.Text.Length > 1)
                CustomSeparatorTextbox.Text = CustomSeparatorTextbox.Text.Substring(CustomSeparatorTextbox.Text.Length - 1);
            CustomSeparatorTextbox.SelectAll();
            ApplyEnteredCustomSeparatorValidationAppearance();
        }

        private void ApplyEnteredCustomSeparatorValidationAppearance()
        {
            if (IsEnteredCustomSeparatorValid())
                CustomSeparatorTextbox.Background = Brushes.LightGreen;
            else
                CustomSeparatorTextbox.Background = Brushes.LightPink;
        }

        private bool IsEnteredCustomSeparatorValid()
        {
            char enteredCustomChar;
            if (!char.TryParse(CustomSeparatorTextbox.Text, out enteredCustomChar))
                return false;
            return true;
        }

        private void OneFileToOneCsvButton_Click(object sender, RoutedEventArgs e)
        {
            OneFileToOneCsvConversion();
        }

        private string lastSelectedDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private void StoreLastSelectedDirectory(string selectedPath)
        {
            if (File.Exists(selectedPath))
                lastSelectedDirectory = Path.GetFullPath(selectedPath).Substring(0, selectedPath.LastIndexOf(Path.GetFileName(selectedPath)));
            else if (Directory.Exists(selectedPath))
                lastSelectedDirectory = selectedPath;
            else
                try
                {
                    string directoryPartOfPath = Path.GetFullPath(selectedPath).Substring(0, selectedPath.LastIndexOf(Path.GetFileName(selectedPath)));
                    if (Directory.Exists(directoryPartOfPath))
                        lastSelectedDirectory = directoryPartOfPath;
                }
                catch
                {
                    lastSelectedDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
        }

        private void OneFileToOneCsvConversion()
        {
            // اعتبارسنجی مقادیر تعیین‌شده توسط کاربر
            if (!IsUserDeterminedOptionsValid())
                return;

            // انتخاب فایل مبدا
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.CheckFileExists = true;
            openDialog.InitialDirectory = lastSelectedDirectory;
            openDialog.Filter = "All files (*.*)|*.*";
            openDialog.Multiselect = false;
            openDialog.Title = "Select source file to Convert";
            openDialog.ValidateNames = true;
            if (!openDialog.ShowDialog().Value)
                // تبدیل توسط کاربر لغو شده
                return;
            StoreLastSelectedDirectory(openDialog.FileName);

            // انتخاب مسیر برای فایل مقصد
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.CheckPathExists = true;
            saveDialog.InitialDirectory = lastSelectedDirectory;
            saveDialog.Filter = "CSV files (*.csv)|*.csv";
            saveDialog.OverwritePrompt = true;
            saveDialog.Title = "Determine target path & file (CSV)";
            saveDialog.ValidateNames = true;
            if (!saveDialog.ShowDialog().Value)
                // تبدیل توسط کاربر لغو شده
                return;
            StoreLastSelectedDirectory(saveDialog.FileName);

            ExecuteSelectedPluginForOneFileToOneCsvConversion
                (GetSelectedSeparatorRelatedCharacter(), openDialog.FileName, saveDialog.FileName);
        }

        private bool IsUserDeterminedOptionsValid()
        {
            bool optionsAreValid = true;
            if (SeparatorsCombobox.Text == CustomSeparatorIndicator
                && !IsEnteredCustomSeparatorValid())
            {
                CustomSeparatorTextbox.Focus();
                optionsAreValid = false;
            }
            return optionsAreValid;
        }

        private char GetSelectedSeparatorRelatedCharacter()
        {
            if (SeparatorsCombobox.Text == CommaSeparatorSignIndicator)
                return ',';
            if (SeparatorsCombobox.Text == SemicolonSeparatorSignIndicator)
                return ';';
            if (SeparatorsCombobox.Text == TabSeparatorSignIndicator)
                return '\t';
            if (SeparatorsCombobox.Text == SpaceSeparatorSignIndicator)
                return ' ';
            if (SeparatorsCombobox.Text == CustomSeparatorIndicator)
            {
                char customSeparator;
                if (char.TryParse(CustomSeparatorTextbox.Text, out customSeparator))
                    return customSeparator;
                else
                    throw new NotSupportedException("Invalid separator character entered");
            }
            throw new ApplicationException("Undetermined separator selected");
        }

        private void MultipleFilesToOneCsvButton_Click(object sender, RoutedEventArgs e)
        {
            MultipleFilesToOneCsvConversion();
        }

        private void MultipleFilesToOneCsvConversion()
        {
            // اعتبارسنجی مقادیر تعیین‌شده توسط کاربر
            if (!IsUserDeterminedOptionsValid())
                return;

            // انتخاب پوشه حاوی فایل‌های مبدا
            System.Windows.Forms.FolderBrowserDialog openDialog = new System.Windows.Forms.FolderBrowserDialog();
            openDialog.RootFolder = Environment.SpecialFolder.Desktop;
            openDialog.SelectedPath = lastSelectedDirectory;
            if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                // تبدیل توسط کاربر لغو شده
                return;
            if(!Directory.Exists(openDialog.SelectedPath))
            {
                MessageBox.Show("Invalid/Inaccessable folder path; Try again!");
                return;
            }
            string[] inputFiles = Directory.GetFiles(openDialog.SelectedPath);
            if (inputFiles.Length == 0)
            {
                MessageBox.Show("Selected folder is empty and cannot used as source path; Try again!");
                return;
            }
            StoreLastSelectedDirectory(openDialog.SelectedPath);

            // انتخاب مسیر برای فایل مقصد
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.CheckPathExists = true;
            saveDialog.Filter = "CSV files (*.csv)|*.csv";
            saveDialog.InitialDirectory = lastSelectedDirectory;
            saveDialog.OverwritePrompt = true;
            saveDialog.Title = "Determine target path & file (CSV)";
            saveDialog.ValidateNames = true;
            if (!saveDialog.ShowDialog().Value)
                // تبدیل توسط کاربر لغو شده
                return;
            StoreLastSelectedDirectory(saveDialog.FileName);

            ExecuteSelectedPluginForMultipleFilesToOneCsvConversion
                (GetSelectedSeparatorRelatedCharacter(), inputFiles, saveDialog.FileName);
        }

        private void ExecuteSelectedPluginForOneFileToOneCsvConversion(char separator, string sourceFilePath, string targetFilePath)
        {
            string arguments = string.Format("\"{0}\" \"{1}\" \"{2}\""
                , separator.ToString()
                , sourceFilePath
                , targetFilePath);

            ExecuteSelectedPluginWithArgument(arguments);
        }

        private void ExecuteSelectedPluginForMultipleFilesToOneCsvConversion(char separator, string[] inputFiles, string outputFile)
        {
            // آماده‌سازی آرگومان‌های ورودی اجرای افزونه
            string arguments = string.Format("\"{0}\"", separator.ToString());
            foreach (var item in inputFiles)
                arguments += string.Format(" \"{0}\"", item);
            arguments += string.Format(" \"{0}\"", outputFile);

            ExecuteSelectedPluginWithArgument(arguments);
        }
        private void ExecuteSelectedPluginWithArgument(string argumentToExecutePluginWith)
        {
            ProcessStartInfo pluginStartInfo = new ProcessStartInfo();
            pluginStartInfo.FileName = string.Format("{0}{1}{2}"
                , Environment.CurrentDirectory + Properties.Settings.Default.PluginsFolderRelativePath
                , PluginsCombobox.Text
                , Properties.Settings.Default.PluginFilesExtension);
            pluginStartInfo.Arguments = argumentToExecutePluginWith
                // اصلاح ترتیب‌های کراکتری به وضعیت قابل قبول برای اجرای پروسه
                .Replace("\"\\\"", "\"\\\\\"")
                .Replace("\"\"\"", "\"\\\"\"");
            // اجرای افزونه تبدیل
            Process pluginProcess = Process.Start(pluginStartInfo);
            pluginProcess.WaitForExit();
        }

        private void FilesDirectoryToCsvDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            FilesDirectoryToCsvDirectoryConversion();
        }

        private void FilesDirectoryToCsvDirectoryConversion()
        {
            // اعتبارسنجی مقادیر تعیین‌شده توسط کاربر
            if (!IsUserDeterminedOptionsValid())
                return;

            // انتخاب پوشه حاوی فایل‌های مبدا
            System.Windows.Forms.FolderBrowserDialog openDialog = new System.Windows.Forms.FolderBrowserDialog();
            openDialog.RootFolder = Environment.SpecialFolder.Desktop;
            openDialog.SelectedPath = lastSelectedDirectory;
            if (openDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                // تبدیل توسط کاربر لغو شده
                return;
            if (!Directory.Exists(openDialog.SelectedPath))
            {
                MessageBox.Show("Invalid/Inaccessable folder path; Try again!");
                return;
            }
            string[] inputFiles = Directory.GetFiles(openDialog.SelectedPath);
            if (inputFiles.Length == 0)
            {
                MessageBox.Show("Selected folder is empty and cannot used as source path; Try again!");
                return;
            }
            StoreLastSelectedDirectory(openDialog.SelectedPath);

            // انتخاب مسیر برای فایل مقصد
            System.Windows.Forms.FolderBrowserDialog saveDialog = new System.Windows.Forms.FolderBrowserDialog();
            saveDialog.RootFolder = Environment.SpecialFolder.Desktop;
            saveDialog.SelectedPath = lastSelectedDirectory;
            if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                // تبدیل توسط کاربر لغو شده
                return;
            if (!Directory.Exists(saveDialog.SelectedPath))
            {
                MessageBox.Show("Invalid/Inaccessable folder path; Try again!");
                return;
            }
            StoreLastSelectedDirectory(saveDialog.SelectedPath);

            foreach (var currentIputFile in inputFiles)
            {
                string relatedOutputFile = string.Format("{0}{1}{2}"
                        , saveDialog.SelectedPath
                        , Path.DirectorySeparatorChar
                        , Path.GetFileName(Path.ChangeExtension(currentIputFile, "csv")));
                ExecuteSelectedPluginForOneFileToOneCsvConversion
                    (GetSelectedSeparatorRelatedCharacter(), currentIputFile, relatedOutputFile);
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpButton.ContextMenu.IsOpen = true;
        }

        private void UserManualMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string userManualDocFullPath = Environment.CurrentDirectory + Properties.Settings.Default.UserManualDocumentRelativePath;
            if (!File.Exists(userManualDocFullPath))
                return;
            Process.Start(userManualDocFullPath);
        }

        private void PluginDevelopManualMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string pluginsDevelopManualDocFullPath = Environment.CurrentDirectory + Properties.Settings.Default.PluginsDevelopMaunalDocumentRelativePath;
            if (!File.Exists(pluginsDevelopManualDocFullPath))
                return;
            Process.Start(pluginsDevelopManualDocFullPath);
        }
    }
}