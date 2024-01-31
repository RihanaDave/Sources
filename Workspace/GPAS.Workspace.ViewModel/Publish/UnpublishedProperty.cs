using GPAS.Workspace.Entities;
using System.ComponentModel;

namespace GPAS.Workspace.ViewModel.Publish
{
    public class UnpublishedProperty 
    {
        public string UnpublishedPropertyType { get; set; }

        public string UnpublishedPropertyValue { get; set; }

        public UnpublishConceptChangeType ChangeType { get; set; }
        public KWProperty relatedKWProperty { get; set; }
    }
}
