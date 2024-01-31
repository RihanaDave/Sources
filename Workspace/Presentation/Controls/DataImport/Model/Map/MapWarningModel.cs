using MaterialDesignThemes.Wpf;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public class MapWarningModel : BaseModel
    {
        private PackIconKind icon = PackIconKind.Dangerous;
        public PackIconKind Icon
        {
            get => icon;
            set => SetValue(ref icon, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set => SetValue(ref message, value);
        }

        private MapWarningType warningType;
        public MapWarningType WarningType
        {
            get => warningType;
            set => SetValue(ref warningType, value);
        }

        private MapElement relatedElement;
        public MapElement RelatedElement
        {
            get => relatedElement;
            set => SetValue(ref relatedElement, value);
        }
    }
}
