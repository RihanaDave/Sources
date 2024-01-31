using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class AccessTableMaterialVM : SemiStructuredMaterialVM
    {
        public AccessTableMaterialVM(bool isSelected, string title, string accessFilePath, string tableName,
            DataTable preview)
            : base(isSelected, title, preview)
        {
            AccessLocalFilePath = accessFilePath;
            TableName = tableName;
        }

        public string AccessLocalFilePath { get; set; }
        public string TableName { get; set; }

    }
}

