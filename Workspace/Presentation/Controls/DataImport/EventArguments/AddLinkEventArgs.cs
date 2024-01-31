using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.EventArguments
{
    public class AddLinkEventArgs : EventArgs
    {
        public ObjectMapModel SourceObject { get; }
        public ObjectMapModel TargetObject { get; }

        public AddLinkEventArgs(ObjectMapModel sourceObject, ObjectMapModel targetObject)
        {
            SourceObject = sourceObject ?? throw new ArgumentNullException(nameof(sourceObject));
            TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
        }
    }
}
