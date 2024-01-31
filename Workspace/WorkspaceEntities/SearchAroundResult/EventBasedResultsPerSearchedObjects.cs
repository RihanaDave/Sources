using System.Collections.Generic;

namespace GPAS.Workspace.Entities.SearchAroundResult
{
    public class EventBasedResultsPerSearchedObjects
    {
        public EventBasedResultsPerSearchedObjects()
        {
            LoadedResults = new List<EventBasedLoadedTargetResult>();
            NotLoadedResults = new List<EventBasedNotLoadedResult>();
        }
        public KWObject SearchedObject { set; get; }

        public List<EventBasedLoadedTargetResult> LoadedResults { set; get; }

        public List<EventBasedNotLoadedResult> NotLoadedResults { set; get; }
    }
}