namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class SchemaField
    {
        public bool IsIndexed { get; internal set; }
        public bool IsMultiValue { get; internal set; }
        public bool IsStored { get; internal set; }
        public string Name { get; internal set; }
        public string Type { get; internal set; }
        public bool DocValues { get; internal set; }
    }
}