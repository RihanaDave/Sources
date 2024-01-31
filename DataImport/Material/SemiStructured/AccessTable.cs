using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class AccessTable : MaterialBase
    {
        public AccessTable()
        {
            FileJobSharePath = string.Empty;
            TableName = string.Empty;
        }

        public string FileJobSharePath
        {
            get;
            set;
        }

        public string TableName
        {
            get;
            set;
        }
    }
}
