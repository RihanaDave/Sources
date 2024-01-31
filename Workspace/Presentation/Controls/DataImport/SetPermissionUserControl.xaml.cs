using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for SetPermissionUserControl.xaml
    /// </summary>
    public partial class SetPermissionUserControl
    {
        public SetPermissionUserControl()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((ClassificationModel)((ListViewItem)sender).DataContext).IsSelected = true;
        }

        private void GroupsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GroupsDataGrid.UnselectAll();
        }
    }
}
