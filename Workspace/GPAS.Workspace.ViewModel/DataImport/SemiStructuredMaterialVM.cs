using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class SemiStructuredMaterialVM : MaterialBaseVM
    {
        public SemiStructuredMaterialVM() { }
        public SemiStructuredMaterialVM(bool isSelected, string title, DataTable preview)
            : base(isSelected, title)
        {
            Preview = preview;
        }
        public DataTable Preview { get; set; }
    }
}
