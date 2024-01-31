using System;
using System.Windows.Controls;
using GPAS.Workspace.Presentation.Windows;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// این کنترل انتزاعی به عنوان پدر تمام کاربردهای لایه ارائه در نظر گرفته شده
    /// </summary>
    public abstract class PresentationApplication : UserControl
    {
        /// <summary>
        /// سازنده
        /// </summary>
        public PresentationApplication()
            : base()
        { }

        public virtual void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {

        }

        public virtual void Reset() { }
    }
}
