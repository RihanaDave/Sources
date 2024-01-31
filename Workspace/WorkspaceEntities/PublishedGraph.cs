using System;

namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاس نگهداری از اطلاعات گراف ذخیره شده
    /// </summary>
    public class PublishedGraph
    {
        /// <summary>
        /// شناسه گراف را براساس مقدار آن در مخزن داده ها برمی گرداند یا مقداردهی می کند!
        /// </summary>
        public virtual int ID
        {
            get;
            set;
        }
        /// <summary>
        /// عنوان گراف را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public virtual string GraphTitle
        {
            get;
            set;
        }
        /// <summary>
        /// توضیحات گراف را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public virtual string GraphDescription
        {
            get;
            set;
        }
        /// <summary>
        /// تعداد اشیاء موجود در گراف را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public virtual int NodesCount
        {
            get;
            set;
        }
        /// <summary>
        /// زمان ذخیره ی گراف را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public virtual DateTime CreatedTime
        {
            get;
            set;
        }

        /// <summary>
        /// نام گروهی که گراف در آن می باشد را مقداردهی یا برمی گرداند.
        /// </summary>
        public virtual string GroupCategory
        {
            get;
            set;
        }
    }
}
