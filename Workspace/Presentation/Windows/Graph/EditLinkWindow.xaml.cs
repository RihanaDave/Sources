using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    /// <summary>
    /// Interaction logic for EditLinkWindow.xaml
    /// </summary>
    public partial class EditLinkWindow
    {
        private readonly LinkViewModel linkViewModel;

        public event EventHandler<UpdateLinksEventArgs> UpdateAllLinks;

        private void OnUpdateAllLinks(List<KWLink> deletedLinks, List<KWLink> newLinks)
        {
            UpdateAllLinks?.Invoke(this, new UpdateLinksEventArgs(deletedLinks, newLinks));
        }

        public EditLinkWindow(List<KWLink> links)
        {
            InitializeComponent();
            linkViewModel = (LinkViewModel)DataContext;
            linkViewModel.PrepareLinkToEdit(links);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            await UpdateLinks();
            linkViewModel.PrepareLinkToEdit(new List<KWLink>(linkViewModel.PublishedAndNewLinks));
        }

        private async void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            await UpdateLinks();
            Close();
        }

        private async Task UpdateLinks()
        {
            try
            {
                MainWaitingControl.TaskIncrement();
                var deletedLinks = await linkViewModel.DeleteLinksForUpdate();
                var newLinks = await linkViewModel.CreateLinksForUpdate();
                OnUpdateAllLinks(deletedLinks, newLinks);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show
                (string.Format(Properties.Resources.Unable_To_Remove_Links_From_The_Graph, ex.Message)
                    , MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        private void MainBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void UnpublishedListScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void PublishedListScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void EventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            UnpublishedLinksListView.UnselectAll();
        }

        private void UnpublishedEventSetter_OnHandler(object sender, RoutedEventArgs e)
        {
            PublishedLinksListView.UnselectAll();
        }

        private void CreateLinkUserControl_OnLinkValidation(object sender, bool e)
        {
            OkButton.IsEnabled = e;
            linkViewModel.ApplyButtonIsEnable = linkViewModel.ApplyButtonIsEnable && e;
        }
    }
}
