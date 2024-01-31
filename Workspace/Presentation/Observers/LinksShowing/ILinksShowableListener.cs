using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Observers
{
    public interface ILinksShowableListener
    {
        Task ShowLinks(IEnumerable<KWLink> linksToShow);
    }
}