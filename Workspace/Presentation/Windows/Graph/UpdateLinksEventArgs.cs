using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public class UpdateLinksEventArgs
    {
        public List<KWLink> NewLinks { get; set; }
        public List<KWLink> LinksToDelete { get; set; }

        public UpdateLinksEventArgs(List<KWLink> linksToDelete, List<KWLink> newLinks)
        {
            NewLinks = newLinks ?? throw new ArgumentNullException(nameof(newLinks));
            LinksToDelete = linksToDelete ?? throw new ArgumentNullException(nameof(linksToDelete));
        }
    }
}
