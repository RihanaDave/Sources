using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Presentation.Applications
{
    public class ObjectToBrowsModel : BaseModel
    {
        private KWObject myObject;
        public KWObject MyObject
        {
            get => myObject;
            set
            {
                SetValue(ref myObject, value);
            }
        }
    }
}
