using GPAS.Workspace.Entities.KWLinks;
using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class RelationshipMapModel : MapElement
    {
        #region Properties

        string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        ObjectMapModel source;
        public ObjectMapModel Source
        {
            get => source;
            set
            {
                if (SetValue(ref source, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        ObjectMapModel target;
        public ObjectMapModel Target
        {
            get => target;
            set
            {
                if (SetValue(ref target, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        LinkDirection direction;
        public LinkDirection Direction
        {
            get => direction;
            set
            {
                if (SetValue(ref direction, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        string description = string.Empty;
        public string Description
        {
            get => description;
            set
            {
                if (SetValue(ref description, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set
            {
                if (SetValue(ref typeUri, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        string iconPath;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        MapModel ownerMap;
        [XmlIgnore]
        public MapModel OwnerMap
        {
            get => ownerMap;
            set
            {
                if (SetValue(ref ownerMap, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        public RelationshipMapModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        public bool Equals(RelationshipMapModel otherRelationship)
        {
            return Id.Equals(otherRelationship.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        #region Event

        #endregion
    }
}
