using GPAS.Workspace.Entities;
using System.ComponentModel;

namespace GPAS.Workspace.ViewModel.Publish
{
    public class UnpublishedMedia 
    {
        public string UnpublishedMediaFilePath { get; set; }

        public UnpublishConceptChangeType ChangeType { get; set; }
        public KWMedia relatedKWMedia { get; set; }
    }
}
