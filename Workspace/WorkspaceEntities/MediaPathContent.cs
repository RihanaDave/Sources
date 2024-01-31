namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاسی برای نگهداری اطلاعات یک فایل یا دایرکتوری است.
    /// </summary>
    public class MediaPathContent
    {
        /// <summary>
        /// نوع فایل را برمی گراند یا مقداردهی می کند
        /// فایل یا دایرکتوری
        /// </summary>
        public virtual MediaPathContentType Type
        {
            get;
            set;
        }
        /// <summary>
        /// مسیر فایل یا دایرکتوری را برمی گراند یا مقداردهی می کند
        /// </summary>
        public virtual string UriAddress
        {
            get;
            set;
        }
        /// <summary>
        /// نام فایل یا دایرکتوری را برمی گراند یا مقداردهی می کند
        /// </summary>
        public virtual string DisplayName
        {
            get;
            set;
        }
    }
}