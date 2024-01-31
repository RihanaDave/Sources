using System.IO;

namespace GPAS.OntologyLoader
{
    public interface IOntologyBuilder
    {
        Stream DownloadOntologyStream();

        Stream DownloadOntologyPackIconStream();
    }
}
