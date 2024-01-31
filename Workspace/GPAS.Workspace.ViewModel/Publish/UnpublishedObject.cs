using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.ViewModel.Publish
{
    public class UnpublishedObject
    {
        public UnpublishedObject()
        {
            UnpublishedProperties = new List<UnpublishedProperty>();
            UnpublishedMedias = new List<UnpublishedMedia>();
            UnpublishedLinks = new List<UnpublishedLink>();
        }

        public string DisplayName { get; set; }

        public string TypeURI { get; set; }

        public UnpublishConceptChangeType ChangeType { get; set; }
        public KWObject RelatedKWObject { get; set; }
        public List<UnpublishedProperty> UnpublishedProperties { get; set; }
        public List<UnpublishedMedia> UnpublishedMedias { get; set; }
        public List<UnpublishedLink> UnpublishedLinks { get; set; }

    }
}
