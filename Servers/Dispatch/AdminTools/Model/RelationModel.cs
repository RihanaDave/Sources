using System.Collections.Generic;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class RelationModel : BaseModel
    {
        private long id;
        public long Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private string label;
        public string Label
        {
            get => label;
            set
            {
                label = value;
                OnPropertyChanged();
            }
        }

        private long sourceId;
        public long SourceId
        {
            get => sourceId;
            set
            {
                sourceId = value;
                OnPropertyChanged();
            }
        }

        private long targetId;
        public long TargetId
        {
            get => targetId;
            set
            {
                targetId = value;
                OnPropertyChanged();
            }
        }

        private long dataSourceId;
        public long DataSourceId
        {
            get => dataSourceId;
            set
            {
                dataSourceId = value;
                OnPropertyChanged();
            }
        }

        private bool searchIndexed;
        public bool SearchIndexed
        {
            get => searchIndexed;
            set
            {
                searchIndexed = value;
                OnPropertyChanged();
            }
        }

        private bool horizonIndexed;
        public bool HorizonIndexed
        {
            get => horizonIndexed;
            set
            {
                horizonIndexed = value;
                OnPropertyChanged();
            }
        }
    }
}
