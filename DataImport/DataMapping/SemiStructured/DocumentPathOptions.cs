using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class DocumentPathOptions
    {
        public DocumentPathOptions()
        {
            SingleFile = true;
            FolderContent = true;
            SubFoldersContent = true;
        }

        public bool SingleFile { get; set; }
        public bool FolderContent { get; set; }
        public bool SubFoldersContent { get; set; }
    }
}
