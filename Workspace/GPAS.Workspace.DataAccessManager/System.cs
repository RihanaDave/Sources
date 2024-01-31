namespace GPAS.Workspace.DataAccessManager
{
    public class System
    {
        private static Ontology.Ontology currentOntology = null;
        private System() { }

        public static void Init(Ontology.Ontology ontology)
        {
            currentOntology = ontology;
        }

        internal static Ontology.Ontology GetOntology()
        {
            return currentOntology;
        }
    }
}
