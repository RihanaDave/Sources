using GPAS.Workspace.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Observers
{
    public interface IObjectsShowableListener
    {
        Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow);
    }
}