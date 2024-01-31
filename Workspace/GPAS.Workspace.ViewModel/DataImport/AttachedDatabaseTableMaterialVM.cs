using GPAS.DataImport.Material.SemiStructured;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class AttachedDatabaseTableMaterialVM : SemiStructuredMaterialVM
    {
        public AttachedDatabaseTableMaterialVM(bool isSelected, string title, DataTable preview)
        : base(isSelected, title, preview)
        {
            
        }        
    }
}
