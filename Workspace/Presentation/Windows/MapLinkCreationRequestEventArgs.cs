using GPAS.DataImport.DataMapping.SemiStructured;
using System;

namespace GPAS.Workspace.Presentation.Windows
{
    public class MapLinkCreationRequestEventArgs : EventArgs
    {
        public MapLinkCreationRequestEventArgs(string description, string linkType,
            RelationshipBaseLinkMappingRelationDirection linkDirection, object source, object target)
        {
            this.description = description ?? throw new ArgumentNullException(nameof(description));
            this.linkType = linkType ?? throw new ArgumentNullException(nameof(linkType));
            this.source = (ObjectMapping)source ?? throw new ArgumentNullException(nameof(source));
            this.target = (ObjectMapping)target ?? throw new ArgumentNullException(nameof(target));
            this.linkDirection = linkDirection;
        }

        private string description;
        public string Description => description;

        private string linkType;
        public string LinkType => linkType;

        private RelationshipBaseLinkMappingRelationDirection linkDirection;
        public RelationshipBaseLinkMappingRelationDirection LinkDirection => linkDirection;

        private ObjectMapping source;
        public ObjectMapping Source => source;

        private ObjectMapping target;
        public ObjectMapping Target => target;
    }
}
