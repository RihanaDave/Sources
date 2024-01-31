using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.ETLProvider
{
  public  class DirectoryContent
    {
        public DirectoryContentType ContentType { set; get; }
        
        public string UriAddress { set; get; }

        public string DisplayName { set; get; }
    }
}
