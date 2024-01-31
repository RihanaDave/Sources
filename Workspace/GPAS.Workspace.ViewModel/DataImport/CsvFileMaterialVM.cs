using GPAS.DataImport.Material.SemiStructured;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class CsvFileMaterialVM : SemiStructuredMaterialVM
    {
        public CsvFileMaterialVM(bool isSelected, string title, string csvFilePath, DataTable preview)
            : base(isSelected, title, preview)
        {
            CsvFilePath = csvFilePath;
        }
        public string CsvFilePath { get; set; }        
    }
}
