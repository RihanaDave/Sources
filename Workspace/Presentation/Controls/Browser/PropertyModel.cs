using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class PropertyModel : BaseModel
    {
        private long id;
        public long Id
        {
            get => id;
            set
            {
                SetValue(ref id, value);
            }
        }

        private string typeUri;
        public string TypeUri
        {
            get => typeUri;
            set
            {
                SetValue(ref typeUri, value);
            }
        }

        private string propertyValue;
        public string PropertyValue
        {
            get => propertyValue;
            set
            {
                SetValue(ref propertyValue, value);
            }
        }

        private string userFriendlyName;
        public string UserFriendlyName
        {
            get => userFriendlyName;
            set
            {
                SetValue(ref userFriendlyName, value);
            }
        }

        private bool isUnpublished;
        public bool IsUnpublished
        {
            get => isUnpublished;
            set
            {
                SetValue(ref isUnpublished, value);
            }
        }

        private Brush background;
        public Brush Background
        {
            get => background;
            set
            {
                SetValue(ref background, value);
            }
        }

        private object tag;
        public object Tag
        {
            get => tag;
            set
            {
                SetValue(ref tag, value);
            }
        }
    }
}
