using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpas.Pagination.Events
{
    public class PaginationEventHandler
    {
        public PaginationEventHandler(long pageNumber, int itemPerPage)
        {
            PageNumber = pageNumber;
            ItemPerPage = itemPerPage;
            FromItem = (pageNumber * itemPerPage);
            ToItem = (FromItem + itemPerPage);
        }

        public long PageNumber { get; protected set; }
        public int ItemPerPage { get; protected set; }
        public long FromItem { get; }
        public long ToItem { get; }
    }
}
