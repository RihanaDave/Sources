using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// این کنترل انتزاعی به عنوان پدر تمام کنترل های لایه ارائه در نظر گرفته شده
    /// </summary>
    public abstract partial class PresentationControl : UserControl
    {
        /// <summary>
        /// سازنده
        /// </summary>
        public PresentationControl()
            : base()
        { }
    }
}
