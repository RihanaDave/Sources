using System.Collections.Generic;

namespace GPAS.Workspace.Entities
{
    public interface IObjectContainerKWLink
    {
        bool ContainsObject(KWObject objectToCheck);
        IEnumerable<KWObject> GetContainedObjects();
    }
}