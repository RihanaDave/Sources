using System;
using System.Collections.Generic;
using System.IO;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class CsvFileMaterial : MaterialBase
    {
        public CsvFileMaterial()
        {
            Separator = ',';
            FileJobSharePath = string.Empty;
        }

        public string FileJobSharePath
        {
            get;
            set;
        }

        public char Separator
        {
            get;
            set;
        }
    }
}
