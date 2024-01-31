using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Observers
{
    public class LinksShowingObserver
        : ObserverBase<ILinksShowableListener, LinksShowingArgs>
    {
        protected override void WakeupListeners(IEnumerable<ILinksShowableListener> listener, LinksShowingArgs arguments)
        {
            List<Task> tasks = new List<Task>();
            foreach (ILinksShowableListener item in listener)
                 tasks.Add(item.ShowLinks((arguments as LinksShowingArgs).LinksToShow));
            Task.WhenAll(tasks);
        }
    }
}
