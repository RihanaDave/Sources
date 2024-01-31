using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAccountsControl.Windows.ACL
{
    public class TreeViewItemBaseModel : BaseModel
    {
        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged();
            }
        }

        public string ClassificationIdentifier { get; set; }
    }
}
