using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FtpServiceAccess
{
    public class DirectoryContentItem
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
    }
}
