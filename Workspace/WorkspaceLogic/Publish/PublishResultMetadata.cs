namespace GPAS.Workspace.Logic.Publish
{
    public class PublishResultMetadata
    {
        public bool HorizonServerSynchronized { get; set; }
        public bool SearchServerSynchronized { get; set; }

        public PublishResultMetadata(bool horizonServerSynchronized, bool searchServerSynchronized)
        {
            HorizonServerSynchronized = horizonServerSynchronized;
            SearchServerSynchronized = searchServerSynchronized;
        }

        public string GetMessage()
        {
            if (HorizonServerSynchronized && SearchServerSynchronized)
                return "Publish completed";
            else
                return string.Format
                    ("Publish completed without {0}{1}{2} server synchronization"
                    , (!HorizonServerSynchronized) ? "Horizon" : ""
                    , (!HorizonServerSynchronized && !SearchServerSynchronized) ? " and " : ""
                    , (!SearchServerSynchronized) ? "Search" : "");
        }
    }
}