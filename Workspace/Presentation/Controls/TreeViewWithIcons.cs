using System.Collections.ObjectModel;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls
{
    public class TreeViewWithIcons
    {
        public TreeViewWithIcons()
        {
            TreeNodes = new ObservableCollection<TreeViewWithIcons>();
        }

        public string OntologyHeaderText { get; set; }

        public string OntologyUri { get; set; }

        public ImageSource OntologyIconSource { get; set; }

        public ObservableCollection<TreeViewWithIcons> TreeNodes { get; set; }
    }
}
