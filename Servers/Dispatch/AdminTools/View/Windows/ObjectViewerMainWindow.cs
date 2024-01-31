using System;
using System.Windows;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class MainWindow
    {
        private async void SearchById(long id, string typeUri)
        {
            try
            {
                ObjectViewerUserControl.SearchTextBox.Text = id.ToString();
                BeforeRequest(ObjectViewerUserControl);
                await objectViewerViewModel.SearchById(id, typeUri);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                AfterRequest(ObjectViewerUserControl);
            }
        }

        private void ShowPermissions(object selectedItem)
        {
            PermissionsWindow permissionsWindow = new PermissionsWindow(selectedItem);
            permissionsWindow.ShowDialog();
        }

        private void ObjectViewerUserControl_SearchById(object sender, long e)
        {
            SearchById(e, string.Empty);
        }

        private void ObjectViewerUserControl_ShowPermissionsEvent(object sender, object e)
        {
            ShowPermissions(e);
        }

        private void ObjectViewerUserControl_SelectIdToShow(object sender, long e)
        {
            SearchById(e, string.Empty);
        }
    }
}
