using GPAS.Workspace.Entities.KWLinks;
using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class ImportingRelationship : BaseModel
    {
        #region Properties

        long id = 0;
        public long Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set => SetValue(ref typeUri, value);
        }

        string description = string.Empty;
        public string Description
        {
            get => description;
            set => SetValue(ref description, value);
        }

        LinkDirection direction = LinkDirection.Bidirectional;
        public LinkDirection Direction
        {
            get => direction;
            set => SetValue(ref direction, value);
        }

        DateTime? timeBegin = null;
        public DateTime? TimeBegin
        {
            get => timeBegin;
            set => SetValue(ref timeBegin, value);
        }

        DateTime? timeEnd = null;
        public DateTime? TimeEnd
        {
            get => timeEnd;
            set => SetValue(ref timeEnd, value);
        }

        private ImportingObject source;
        public ImportingObject Source
        {
            get => source;
            set => SetValue(ref source, value);
        }

        private ImportingObject target;
        public ImportingObject Target
        {
            get => target;
            set => SetValue(ref target, value);
        }

        IDataSource ownerDataSource;
        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set => SetValue(ref ownerDataSource, value);
        }

        #endregion

        #region Methods

        public ImportingRelationship()
        {
            Id = Guid.NewGuid().GetHashCode();
        }

        #endregion
    }
}
