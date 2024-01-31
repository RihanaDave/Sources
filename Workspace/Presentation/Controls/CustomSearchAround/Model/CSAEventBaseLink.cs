using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public class CSAEventBaseLink : CSALink
    {
        CSAObject eventObject = null;
        
        public CSAObject EventObject
        {
            get => eventObject;
            set
            {
                if (SetValue(ref eventObject, value))
                {
                    OnScenarioChanged();
                }
            }
        }
    }
}
