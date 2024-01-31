namespace GPAS.DataImport.DataMapping
{
    public class PathPart
    {
        public string Text { get; set; }
        public PathPartTypeMappingItem Type { get; set; }
        public int DirectoryIndex { get; set; }
        public PathPartDirectionMappingItem Direction { get; set; }
    }
}
