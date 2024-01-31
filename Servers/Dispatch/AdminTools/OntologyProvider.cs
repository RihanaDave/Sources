using System;
using System.IO;

namespace GPAS.Dispatch.AdminTools
{
    public class OntologyProvider
    {
        OntologyProvider()
        {

        }

        public static string GetUserFriendlyNameOfOntologyTypeNode(string nodeType)
        {
            if (OntologyLoader.OntologyLoader.GetOntology().IsDocument(nodeType) &&
                !nodeType.Equals(OntologyLoader.OntologyLoader.GetOntology().GetDocumentTypeURI()))
                return string.Format("سند {0}", nodeType);

            return nodeType.Replace('_', ' ');
        }

        public static Uri GetTypeIconPath(string typeURI)
        {
            if (string.IsNullOrWhiteSpace(typeURI))
                throw new ArgumentException("Invalid Argument (Null, Empty or Whitespace)", nameof(typeURI));

            var currentOntology = OntologyLoader.OntologyLoader.GetOntology();
            if (!(currentOntology.IsEntity(typeURI) || currentOntology.IsEvent(typeURI) ||
                currentOntology.IsDocument(typeURI) || currentOntology.IsRelationship(typeURI)))
            {
                typeURI = currentOntology.GetAllConceptsTypeURI();
            }            

            string iconPath = Path.Combine(OntologyLoader.OntologyLoader.GetOntologyIconsPath(), typeURI + ".png");
            string typeToGetItsIcon = typeURI;

            while (!File.Exists(iconPath))
            {
                try
                {
                    typeToGetItsIcon = currentOntology.GetParent(typeToGetItsIcon);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format("Unable to get parent type for the type '{0}'.\r\nMessage: {1}",
                        typeURI, ex.Message)); // SPARQL Query Or Ontology Configuration Is Wrong
                }

                if (string.IsNullOrWhiteSpace(typeToGetItsIcon))
                    throw new ArgumentException(string.Format("Invalid parent returned for type '{0}'.", typeURI));

                iconPath = Path.Combine(OntologyLoader.OntologyLoader.GetOntologyIconsPath(), typeToGetItsIcon + ".png");
            }

            return new Uri(iconPath, UriKind.RelativeOrAbsolute);
        }
    }
}
