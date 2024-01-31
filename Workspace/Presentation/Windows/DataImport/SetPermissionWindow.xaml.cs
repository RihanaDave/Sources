using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for SetPermissionWindow.xaml
    /// </summary>
    public partial class SetPermissionWindow : Window
    {
        public SetPermissionWindow(ACLModel selectedDataSource)
        {
            InitializeComponent();
            ACLViewModel PermissionViewModel = new ACLViewModel(selectedDataSource);
            SetPermissionUserControl.DataContext = PermissionViewModel;
        }
    }
}
