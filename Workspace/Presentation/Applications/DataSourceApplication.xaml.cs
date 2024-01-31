using GPAS.AccessControl;
using GPAS.Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// Interaction logic for DataSourceApplication.xaml
    /// </summary>
    public partial class DataSourceApplication
    {
        #region متغیرهای سراسری
        public ObservableCollection<ShowingDataSources> ShowingDataSourcesList { get; set; }
        public int NumberOfDataSourcesPerType = 10;
        ResourceDictionary iconsResource = null;
        #endregion
        public DataSourceApplication()
        {
            InitializeComponent();
            iconsResource = new ResourceDictionary();
            iconsResource.Source = new Uri("../Resources/Icons.xaml", UriKind.Relative);
            DataContext = this;
            ShowingDataSourcesList = new ObservableCollection<ShowingDataSources>();
        }

        #region توابع
        public async Task LoadAllDataSources()
        {
            try
            {
                ShowingDataSourcesList = new ObservableCollection<ShowingDataSources>();
                DataSourceWaitingControl.Message = Properties.Resources.Loading_All_Data_Sources;
                DataSourceWaitingControl.TaskIncrement();
                Logic.DataSourceProvider dataSourceProvider = new Logic.DataSourceProvider();
                DataSourceInfo[] retrievedDataSources = await dataSourceProvider.GetAllDataSourcesAsync(searchTextBox.Text);
                ShowingDataSourcesList = GenerateShowingDataSources(retrievedDataSources);
                dataSourcesTreeview.ItemsSource = ShowingDataSourcesList;
            }
            catch (Exception)
            {
               
            }
            finally
            {
                DataSourceWaitingControl.TaskDecrement();
            }     
        }

        private ObservableCollection<ShowingDataSources> GenerateShowingDataSources(DataSourceInfo[] retrievedDataSources)
        {
            ObservableCollection<ShowingDataSources> result = new ObservableCollection<ShowingDataSources>();
            Dictionary<long, ObservableCollection<ShowingDataSource>> sortedDataSourcesByType = new Dictionary<long, ObservableCollection<ShowingDataSource>>();
            foreach (DataSourceInfo currentDataSource in retrievedDataSources)
            {
                if (sortedDataSourcesByType.ContainsKey(currentDataSource.Type))
                {
                    if (!sortedDataSourcesByType[currentDataSource.Type].Where(ds => ds.relatedDataSourceInfo.Id == currentDataSource.Id).Any()
                            && sortedDataSourcesByType[currentDataSource.Type].Count < NumberOfDataSourcesPerType)
                    {
                        ShowingDataSource showingDataSource = new ShowingDataSource()
                        {
                            Name = currentDataSource.Name,
                            CreatedBy = currentDataSource.CreatedBy,
                            CreatedTime = currentDataSource.CreatedTime,
                            Description = currentDataSource.Description,
                            ShowMoreHyperlink = false,
                            relatedDataSourceInfo = currentDataSource
                        };
                        SetShowingDataSourceIcon((DataSourceType)currentDataSource.Type, ref showingDataSource);
                        sortedDataSourcesByType[currentDataSource.Type].Add(showingDataSource);
                    }
                }
                else
                {
                    ObservableCollection<ShowingDataSource> showingDataSourceOfSpecificType = new ObservableCollection<ShowingDataSource>();
                    ShowingDataSource showingDataSource = new ShowingDataSource()
                    {
                        Name = currentDataSource.Name,
                        CreatedBy = currentDataSource.CreatedBy,
                        CreatedTime = currentDataSource.CreatedTime,
                        Description = currentDataSource.Description,
                        ShowMoreHyperlink = false,
                        relatedDataSourceInfo = currentDataSource
                    };
                    SetShowingDataSourceIcon((DataSourceType)currentDataSource.Type, ref showingDataSource);
                    showingDataSourceOfSpecificType.Add(showingDataSource);
                    sortedDataSourcesByType.Add(currentDataSource.Type, showingDataSourceOfSpecificType);
                }
            }

            foreach (long currentDataSourceType in sortedDataSourcesByType.Keys)
            {

                result.Add(new ShowingDataSources()
                {
                    DataSourceType = (DataSourceType)currentDataSourceType,
                    DataSources = sortedDataSourcesByType[currentDataSourceType],
                    IsExpanded = true
                });
            }

            return result;
        }
        private async Task DownloadFileFromServer(DataSourceInfo dataSourceInfo)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.OverwritePrompt = true;
            saveFileDialog.FileName = dataSourceInfo.Name;
            string[] filePathSplit = dataSourceInfo.Name.Split('/');
            string[] fileExec = filePathSplit[filePathSplit.Length - 1].Split('.');
            string filterName = fileExec[fileExec.Length - 1].ToUpper();
            string filter = filterName + " Files" + "|*." + fileExec[fileExec.Length - 1];
            saveFileDialog.Filter = filter;
            string defaultExec = fileExec[fileExec.Length - 1];
            saveFileDialog.DefaultExt = defaultExec;
            saveFileDialog.ValidateNames = true;
            if (saveFileDialog.ShowDialog().Value)
            {
                try
                {
                    DataSourceWaitingControl.Message = Properties.Resources.Downloading;
                    DataSourceWaitingControl.TaskIncrement();
                    //await Logic.DataSourceProvider.DownloadDataSourceAsync(dataSourceInfo.Id, saveFileDialog.FileName);
                    await Logic.DataSourceProvider.DownloadDataSourceByNameAsync(dataSourceInfo.Name, saveFileDialog.FileName);
                    DataSourceWaitingControl.TaskDecrement();
                    KWMessageBox.Show(Properties.Resources.Save_successfull
                   , MessageBoxButton.OK
                   , MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    DataSourceWaitingControl.TaskDecrement();
                    ExceptionHandler exceptionHandler = new ExceptionHandler();
                    exceptionHandler.WriteErrorLog(ex);
                    KWMessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message));
                }
            }
        }


        private void GenerateShowingDataSource(DataSourceInfo[] retrievedDataSources,
            ref ShowingDataSources relatedShowingDataSources)
        {
            if (relatedShowingDataSources.DataSources.Count != 0)
            {
                relatedShowingDataSources.DataSources.RemoveAt(relatedShowingDataSources.DataSources.Count - 1);
            }
            for (int i = 0; i < retrievedDataSources.Length; i++)
            {
                if (i < NumberOfDataSourcesPerType)
                {
                    ShowingDataSource showingDataSource = new ShowingDataSource()
                    {
                        Name = retrievedDataSources[i].Name,
                        CreatedTime = retrievedDataSources[i].CreatedTime,
                        CreatedBy = retrievedDataSources[i].CreatedBy,
                        Description = retrievedDataSources[i].Description,
                        ShowMoreHyperlink = false,
                        IsExpanded = false,
                        relatedDataSourceInfo = retrievedDataSources[i],
                        relatedShowingDataSources = relatedShowingDataSources
                    };
                    SetShowingDataSourceIcon((DataSourceType)retrievedDataSources[i].Type, ref showingDataSource);
                    relatedShowingDataSources.DataSources.Add(showingDataSource);
                }
            }


            if (retrievedDataSources.Length > NumberOfDataSourcesPerType)
            {
                relatedShowingDataSources.DataSources.Add(new ShowingDataSource()
                {
                    ShowMoreHyperlink = true,
                    Name = string.Empty,
                    CreatedBy = string.Empty,
                    CreatedTime = string.Empty,
                    Description = string.Empty,
                    IsExpanded = false,
                    relatedShowingDataSources = relatedShowingDataSources
                });
            }
        }


        public void SetShowingDataSourceIcon(DataSourceType dataSourceType, ref ShowingDataSource showingDataSource)
        {
            switch (dataSourceType)
            {
                case DataSourceType.ManuallyEntered:
                    showingDataSource.Icon = iconsResource["ManualEntedredDataSource"] as BitmapImage;
                    break;
                case DataSourceType.Document:
                    showingDataSource.Icon = iconsResource["DocumentDataSourceIcon"] as BitmapImage;
                    showingDataSource.DownloadIcon = iconsResource["DownloadDataSourceIcon"] as BitmapImage;
                    break;
                case DataSourceType.Graph:
                    showingDataSource.Icon = iconsResource["GraphDataSourceIcon"] as BitmapImage;
                    break;
                case DataSourceType.CsvFile:
                    showingDataSource.Icon = iconsResource["CsvFileImage"] as BitmapImage;
                    showingDataSource.DownloadIcon = iconsResource["DownloadDataSourceIcon"] as BitmapImage;
                    break;
                case DataSourceType.AttachedDatabaseTable:
                    showingDataSource.Icon = iconsResource["AddedDatabaseIcon"] as BitmapImage;
                    break;
                case DataSourceType.DataLakeSearchResult:
                    break;
                case DataSourceType.ExcelSheet:
                    showingDataSource.Icon = iconsResource["ExcelSheetImage"] as BitmapImage;
                    showingDataSource.DownloadIcon = iconsResource["DownloadDataSourceIcon"] as BitmapImage;
                    break;
                case DataSourceType.AccessTable:
                    showingDataSource.Icon = iconsResource["AccessTableImage"] as BitmapImage;
                    showingDataSource.DownloadIcon = iconsResource["DownloadDataSourceIcon"] as BitmapImage;
                    break;
                default:
                    break;
            }
        }

        public override void Reset()
        {
            searchTextBox.Text = string.Empty;
        }

        #endregion

        #region مدیریت رخدادگردانها
        private async void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            searchTextBox.Text = string.Empty;
            //SearchMagnifierTextBlock.Visibility = Visibility.Visible;
            await LoadAllDataSources();
        }

        private void searchTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(searchTextBox.Text) ||
            //    string.IsNullOrEmpty(searchTextBox.Text))
            //{
            //  //  SearchMagnifierTextBlock.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //   // SearchMagnifierTextBlock.Visibility = Visibility.Collapsed;
            //}
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(searchTextBox.Text) ||
            //    string.IsNullOrEmpty(searchTextBox.Text))
            //{
            //   // SearchMagnifierTextBlock.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //  //  SearchMagnifierTextBlock.Visibility = Visibility.Collapsed;
            //}
            //  BtnSearch.IsDefault = true;
        }

        private async void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null && (tvi.DataContext is ShowingDataSources))
            {
                ShowingDataSources relatedShowingDataSources = (tvi.DataContext as ShowingDataSources);
                DataSourceWaitingControl.Message = string.Format(Properties.Resources.Loading_Data_Sources, relatedShowingDataSources.DataSourceType);
                DataSourceWaitingControl.TaskIncrement();
                Logic.DataSourceProvider dataSourceProvider = new Logic.DataSourceProvider();
                DataSourceInfo[] retrievedDataSourceInfos = await dataSourceProvider.GetDataSourcesAsync(relatedShowingDataSources.DataSourceType, relatedShowingDataSources.NumberOfMoreRequested, searchTextBox.Text);
                relatedShowingDataSources.DataSources.Clear();
                GenerateShowingDataSource(retrievedDataSourceInfos, ref relatedShowingDataSources);
                DataSourceWaitingControl.TaskDecrement();
            }
        }

        private void dataSourcesTreeview_Collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null && (tvi.DataContext is ShowingDataSources))
            {
                ShowingDataSources relatedShowingDataSources = (tvi.DataContext as ShowingDataSources);
                relatedShowingDataSources.NumberOfMoreRequested = 0;
            }
        }

        private async void MoreHyperlink_Click(object sender, RoutedEventArgs e)
        {
            ShowingDataSource relatedShowingDataSource = (sender as System.Windows.Documents.Hyperlink).DataContext as ShowingDataSource;
            ShowingDataSources relatedShowingDataSources = relatedShowingDataSource.relatedShowingDataSources;
            relatedShowingDataSource.relatedShowingDataSources.NumberOfMoreRequested++;
            DataSourceWaitingControl.Message = Properties.Resources.Loading_More_Data_Sources;
            DataSourceWaitingControl.TaskIncrement();
            Logic.DataSourceProvider dataSourceProvider = new Logic.DataSourceProvider();
            DataSourceInfo[] retrievedDataSourceInfos = await dataSourceProvider.GetDataSourcesAsync(relatedShowingDataSource.relatedShowingDataSources.DataSourceType, relatedShowingDataSource.relatedShowingDataSources.NumberOfMoreRequested, searchTextBox.Text);
            GenerateShowingDataSource(retrievedDataSourceInfos, ref relatedShowingDataSources);
            DataSourceWaitingControl.TaskDecrement();
        }


        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        private async void LoadAll()
        {
            await LoadAllDataSources();
        }

        #endregion

        private async void DownloadIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowingDataSource selectedDataSource = ((sender as MaterialDesignThemes.Wpf.PackIcon)?.DataContext as ShowingDataSource);
            await DownloadFileFromServer(selectedDataSource.relatedDataSourceInfo);
        }

        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            BtnSearch.IsDefault = false;
        }

        private void searchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            BtnSearch.IsDefault = true;
        }

        private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadAll();
            }
        }
    }

    public class InverseBooleanToVisibillityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((bool)value == true)
                return Visibility.Collapsed;
            else return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }

}
