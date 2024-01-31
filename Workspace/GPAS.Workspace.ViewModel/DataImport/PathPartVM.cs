using GPAS.DataImport.DataMapping;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class PathPartVM
    {
        public string Text { get; set; }
        public PathPartTypeMappingItem Type { get; set; }
        public int DirectoryIndex { get; set; }
        public PathPartDirectionMappingItem Direction { get; set; }
        public bool IsSelected { get; set; }
    }
}
