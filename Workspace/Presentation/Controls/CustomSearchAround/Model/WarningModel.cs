using MaterialDesignThemes.Wpf;
using System;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public class WarningModel : BaseModel, ISelectable
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

        private WarningType warningType;
        public WarningType WarningType
        {
            get => warningType;
            set => SetValue(ref warningType, value);
        }

        private ICSAElement relatedElement;
        public ICSAElement RelatedElement
        {
            get => relatedElement;
            set => SetValue(ref relatedElement, value);
        }

        bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (SetValue(ref isSelected, value))
                {
                    if (IsSelected)
                        OnSelected();
                    else
                        OnDeselected();
                }
            }
        }

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Deselected;
        protected void OnDeselected()
        {
            Deselected?.Invoke(this, EventArgs.Empty);
        }
    }
}
