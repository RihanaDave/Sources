using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls
{
    internal class EventBasedLinksPerEnds
    {
        internal KWObject End1;
        internal KWObject End2;
        internal List<EventBasedKWLink> Links;
        
        internal static List<EventBasedLinksPerEnds> SeperateLinksByTheirEnds(IEnumerable<EventBasedKWLink> selectedEventBasedLinks)
        {
            var result = new List<EventBasedLinksPerEnds>();
            foreach (var link in selectedEventBasedLinks)
            {
                EventBasedLinksPerEnds relatedResultItem = null;
                foreach (var resultItem in result)
                {
                    if ((resultItem.End1.Equals(link.Source) && resultItem.End2.Equals(link.Target))
                        || (resultItem.End1.Equals(link.Target) && resultItem.End2.Equals(link.Source)))
                    {
                        relatedResultItem = resultItem;
                        break;
                    }
                }
                if (relatedResultItem == null)
                {
                    relatedResultItem = new EventBasedLinksPerEnds()
                    {
                        End1 = link.Source,
                        End2 = link.Target,
                        Links = new List<EventBasedKWLink>()
                    };
                    result.Add(relatedResultItem);
                }
                relatedResultItem.Links.Add(link);
            }
            return result;
        }
    }
}