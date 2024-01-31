using GPAS.OntologyLoader;
using System.IO;

namespace GPAS.Dispatch.Logic
{
    public class DispatchOntologyDownLoader : IOntologyBuilder
    {
        public Stream DownloadOntologyPackIconStream()
        {
            DispatchFileProvider fileProvider = new DispatchFileProvider();
            return fileProvider.GetIconPack();
        }

        public Stream DownloadOntologyStream()
        {
            DispatchFileProvider fileProvider = new DispatchFileProvider();
            return fileProvider.GetOntology();
        }
    }
}
