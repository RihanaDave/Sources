using System;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class ExcelSheet : MaterialBase
    {
        public ExcelSheet()
        {
            FileJobSharePath = string.Empty;
            SheetName = string.Empty;
        }

        public string FileJobSharePath
        {
            get;
            set;
        }

        public string SheetName
        {
            get;
            set;
        }
    }
}
