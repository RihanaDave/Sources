using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class DataLakeSearchResultMaterial : MaterialBase
    {
        public DataLakeSearchResultMaterial()
        { }

        public string[] CachedSearchResultAsCSV
        {
            get;
            set;
        }
    }
}
