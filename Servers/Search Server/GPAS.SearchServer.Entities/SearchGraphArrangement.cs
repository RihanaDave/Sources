using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
public    class SearchGraphArrangement
    {
        public string Title { set; get; }

        public string Description { set; get; }

        public long Id { set; get; }

        public string TimeCreated { set; get; }

        public byte[] GraphImage { set; get; }

        public byte[] GraphArrangementXML { set; get; }

        public int NodesCount { set; get; }

        public long DataSourceID { set; get; }
    }
}
