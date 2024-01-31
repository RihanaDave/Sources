using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for TabularMappingFirstStepUserControl.xaml
    /// </summary>
    public partial class TabularMappingFirstStepUserControl
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public TabularMappingFirstStepUserControl()
        {
            InitializeComponent();
        }

        private void DeleteButtonOnClick(object sender, RoutedEventArgs e)
        {
            DeleteFromRecentMap((SavedMapModel)((Button)sender).DataContext);
        }

        private void RecentMapListViewOnMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoadFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
        }

        private void RecentMapListViewOnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                DeleteFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoadFromRecentMap((SavedMapModel)((ListViewItem)sender).DataContext);
            }
        }

        #region Events

        public event EventHandler<EventArgs> NextStep;

        protected void OnNextStep()
        {
            NextStep?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Events Handler

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            SaveMap();
        }

        private void LoadMapButtonOnClick(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        /// <summary>
        /// از افزودن فایل‌های غیر معتبر به کنترل هنگام
        /// کشیدن و انداختن جلوگیری می‌شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainGridOnDragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                if (e.Data.GetData(DataFormats.FileDrop, true) is string[] fileNames)
                {
                    if (fileNames.Length > 1 || fileNames.Any(filename => Path.GetExtension(filename)?.ToLower() != ".imm"))
                    {
                        dropEnabled = false;
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }

            if (dropEnabled)
                return;

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void MainGridOnDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] selectedFilesPath))
                return;

            LoadMap(selectedFilesPath[0]);
        }

        private void NextStepButtonOnClick(object sender, RoutedEventArgs e)
        {
            OnNextStep();
        }

        #endregion       
    }
}
