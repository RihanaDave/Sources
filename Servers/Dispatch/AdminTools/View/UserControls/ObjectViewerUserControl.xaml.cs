using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for ObjectViewerUserControl.xaml
    /// </summary>
    public partial class ObjectViewerUserControl
    {
        public ObjectViewerUserControl()
        {
            InitializeComponent();
        }

        private object lastRelationSelected;
        public event EventHandler<long> SearchById;
        public event EventHandler<object> ShowPermissionsEvent;
        public event EventHandler<long> SelectIdToShow;

        private void OnSearchById(long id)
        {
            SearchById?.Invoke(this, id);
        }

        private void OnShowPermissionsEvent(object selectedItem)
        {
            ShowPermissionsEvent?.Invoke(this, selectedItem);
        }

        private void OnSelectIdToShow(long objectId)
        {
            SelectIdToShow?.Invoke(this, objectId);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(SearchTextBox.Text));
        }

        private void ShowPermissions_Click(object sender, RoutedEventArgs e)
        {
            OnShowPermissionsEvent(PropertiesDataGrid.SelectedItem);
        }

        private void ShowRelationPermissions_Click(object sender, RoutedEventArgs e)
        {
            OnShowPermissionsEvent(lastRelationSelected);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) OnSearchById(long.Parse(SearchTextBox.Text));
        }

        private void RelatedEntitiesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastRelationSelected = e.AddedItems[0];
        }

        private void RelatedEventsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastRelationSelected = e.AddedItems[0];
        }

        private void RelatedDocumentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lastRelationSelected = e.AddedItems[0];
        }

        private void EntitiesSourceIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void EntitiesTargetIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void EventsSourceIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void EventsTargetIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void DocumentsTargetIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void DocumentsSourceIdHyperlink_Click(object sender, RoutedEventArgs e)
        {
            OnSearchById(long.Parse(((TextBlock)((InlineUIContainer)((Hyperlink)sender).Inlines.First()).Child).Text));
        }

        private void ChangeScrollViewerLine(MouseWheelEventArgs mouseWheelEventArgs)
        {
            if (mouseWheelEventArgs.Delta > 0)
            {
                RelationScrollViewer.LineUp();
            }
            else if (mouseWheelEventArgs.Delta < 0)
            {
                RelationScrollViewer.LineDown();
            }
        }

        private void RelatedEntitiesDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeScrollViewerLine(e);
        }

        private void RelatedEventsDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeScrollViewerLine(e);
        }

        private void RelatedDocumentsDataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeScrollViewerLine(e);
        }

        private void IdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnSelectIdToShow((long)e.AddedItems[0]);
        }
    }
}