using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
   public class DefectionModel:BaseModel
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

        private DefectionType defectionType;
        public DefectionType DefectionType
        {
            get => defectionType;
            set => SetValue(ref defectionType, value);
        }
    }
}
