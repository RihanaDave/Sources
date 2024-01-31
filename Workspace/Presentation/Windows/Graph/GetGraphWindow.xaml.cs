using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Windows
{
    /// <summary>
    /// منطق تعامل با GetGraphWindow.xaml
    /// </summary>
    public partial class GetGraphWindow
    {
        public GetGraphWindow()
        {
            InitializeComponent();
        }

        private async void PresentationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWaitingControl.Message = Properties.Resources.Retriving_Published_Graphs_Information;
                MainWaitingControl.TaskIncrement();
                await SetPublishedGraphsList();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Unable_To_Load_Published_Graph}\n\n{ex.Message}", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        /// <summary>
        /// لیست گراف های ذخیره شده را از سرور می گیرد و آنها را نمایش میدهد و اولین آیتم را انتخاب مب کند.
        /// </summary>
        /// <returns></returns>
        private async Task SetPublishedGraphsList()
        {
            // اطلاعات گراف های ذخیره شده که براساس زمان دسته بندی شده اند را در رابط کاربری نمایش می دهد.
            List<PublishedGraph> graphDictionary = await GraphRepositoryManager.RetrieveGraphsListAsync();
            BindingPublishedGraphListToListBox(graphDictionary);

            if (publishedGraphsList.Items.Count > 0)
            {
                publishedGraphsList.SelectedIndex = 0;
                publishedGraphsList.Focus();
            }
        }

        /// <summary>
        /// یک لیست از گراف های ذخیره شده از سرور دریافت می کند و در لیست باکس نمایش می دهد
        /// </summary>
        public void BindingPublishedGraphListToListBox(IEnumerable<PublishedGraph> graphsList)
        {
            ObservableCollection<PublishedGraph> graphlistCollection = new ObservableCollection<PublishedGraph>();

            foreach (var item in graphsList)
            {
                graphlistCollection.Add(item);
            }

            ICollectionView listBoxCollection = CollectionViewSource.GetDefaultView(graphlistCollection);
            listBoxCollection.GroupDescriptions.Add(new PropertyGroupDescription("CategoryGroup"));

            publishedGraphsList.ItemsSource = listBoxCollection;
            DisplayListManagment(listBoxCollection);
        }

        private void DisplayListManagment(ICollectionView listBoxCollection)
        {
            if (listBoxCollection.Groups.Count > 0)
            {
                publishedGraphsList.Visibility = Visibility.Visible;
                NoItemsTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                publishedGraphsList.Visibility = Visibility.Collapsed;
                NoItemsTextBlock.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// تصویر گراف انتخاب شده را از سرور دریافت می کند و نمایش می دهد.
        /// </summary>
        /// <param name="selectedGraph"></param>
        /// <returns></returns>
        private async Task<BitmapImage> CreateImageOfSelectedGraphAsync(PublishedGraph selectedGraph)
        {
            if (selectedGraph == null)
                throw new ArgumentNullException("selectedGraph");

            if (selectedGraph.ID < 0)
                throw new ArgumentOutOfRangeException("selectedGraph");

            try
            {
                byte[] graphBytes = await GraphRepositoryManager.RetrieveGraphImageAsync(selectedGraph.ID);
                MemoryStream ms = new MemoryStream(graphBytes);
                ms.Write(graphBytes, 0, graphBytes.Length);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        private int selectedGraphId = -1;

        private void btnAddGraph_Click(object sender, RoutedEventArgs e)
        {
            if (selectedGraphId == -1)
                return;

            RetriveGraph();
        }

        /// <summary>
        /// رخداد انتخاب گراف را برای نمایش محتویات گراف در اپلیکیشن گراف فراخوانی می کند
        /// </summary>
        private async void RetriveGraph()
        {
            try
            {
                MainWaitingControl.Message = "Loading...";
                MainWaitingControl.TaskIncrement();

                SelectedPublishedGraph = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(selectedGraphId);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Invalid_Server_Response}\n\n{ex.Message}");
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
                Close();
            }
        }

        public GraphArrangment SelectedPublishedGraph;

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void publishedGraphsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox)?.SelectedItem is PublishedGraph publishedGraph)
                ShowGraphInformation(publishedGraph);
        }

        /// <summary>
        /// اطلاعات مربوط به گراف انتخاب شده را برگذاری می کند و نمایش می دهد.
        /// </summary>
        /// <param name="selectedGraph"></param>
        private async void ShowGraphInformation(PublishedGraph selectedGraph)
        {
            try
            {
                MainWaitingControl.Message = "Loading...";
                MainWaitingControl.TaskIncrement();
                selectedGraphId = selectedGraph.ID;
                imgGraphPreview.Source = await CreateImageOfSelectedGraphAsync(selectedGraph);
                btnAddGraph.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Unable_To_Load_Published_Graphs_Inforamtion}\n\n{ex.Message}");
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        private void publishedGraphsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (selectedGraphId == -1)
                return;
            if (e == null)
                return;

            if (e.ChangedButton == MouseButton.Left)
            {
                RetriveGraph();
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }
    }
}
