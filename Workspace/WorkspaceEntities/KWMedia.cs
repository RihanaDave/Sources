using System;

namespace GPAS.Workspace.Entities
{
    public class KWMedia
    {

        public KWMedia()
        {
        }

        public virtual long ID
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual MediaPathContent MediaUri
        {
            get;
            set;
        }

        public virtual KWObject Owner
        {
            get;
            set;
        }
        public long? DataSourceId
        {
            get;
            set;
        }

        public EventHandler<EventArgs> Deleted;
        public void OnDeleted()
        {
            if (Deleted != null)
                Deleted(this, EventArgs.Empty);
        }
    }
}
