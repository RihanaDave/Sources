namespace GPAS.Workspace.Presentation.Controls.Graph.Flows
{
    public class DataSource
    {
        public DataSourceType SourceType { get; set; }
        public string SourceConceptTypeUri { get; set; }
        public override int GetHashCode()
        {
            return SourceConceptTypeUri.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DataSource))
                return false;
            else
                return SourceConceptTypeUri.Equals(((DataSource)obj).SourceConceptTypeUri);
        }
    }
}