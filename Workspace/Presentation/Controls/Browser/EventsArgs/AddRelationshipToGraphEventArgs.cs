using GPAS.Workspace.Entities;
using System;

namespace GPAS.Workspace.Presentation.Controls.Browser.EventsArgs
{
    public class AddRelationshipToGraphEventArgs
    {
        public AddRelationshipToGraphEventArgs(RelationshipBasedKWLink relationshipToShow)
        {
            RelationshipToShow = relationshipToShow ?? throw new ArgumentNullException(nameof(relationshipToShow));
        }

        public RelationshipBasedKWLink RelationshipToShow
        {
            get;
        }
    }
}
