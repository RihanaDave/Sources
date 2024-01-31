using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    /// <summary>
    /// جزئیات حضور یک رابطه روی گراف
    /// به ازای هر رابطه‌ی در حال نمایش روی گراف، یک نمونه از این کلاس ایجاد خواهد شد.
    /// </summary>
    internal class RelationshipShowMetadata
    {
        public RelationshipShowMetadata()
        {
            IsCached = false;
            HostEdges = new HashSet<EdgeMetadata>();
        }

        /// <summary>
        /// اگر کنترل از میانگیری رابطه در محیط کاربری مطمئن باشد، این ویژگی مقدار درست دارد
        /// </summary>
        public bool IsCached { get; set; }
        /// <summary>
        /// یال‌هایی که رابطه‌ی متناظر با این نمونه از کلاس را میزبانی می‌کنند
        /// </summary>
        public HashSet<EdgeMetadata> HostEdges { get; set; }
    }
}
