using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class EmlDirectory : MaterialBase
    {
        public string DirectoryJobSharePath
        {
            get;
            set;
        }

        public EmlDirectory()
        {            
            DirectoryJobSharePath = string.Empty;
        }
    }
}
