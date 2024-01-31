using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GPAS.TagCloudViewer.Foundations
{
    public class VertexControl : GraphX.Controls.VertexControl
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="relatedVertex">گرهی که می خواهیم برای آن کنترل (نمایشی) بسازیم</param>
        /// <param name="groupVertexIsCollapsed">آیا گره میزبان گروه بندی به صورت جمع شده نمایش داده شود یا خیر</param>
        public VertexControl(Vertex relatedVertex, bool groupVertexIsCollapsed = false)
            : base(relatedVertex)
        {
            if (relatedVertex == null)
                throw new ArgumentNullException("relatedVertex");
            
            IsFrozen = false;
        }
        
        /// <summary>
        /// نشان می دهد که گره مربوط به این کنترل، در وضعیت منجمد هست یا خیر؛
        /// در وضعیت منجمد، گره قابل انتخاب و جابجایی نیست
        /// </summary>
        public bool IsFrozen
        {
            get { return (bool)GetValue(IsFrozenProperty); }
            set { SetValue(IsFrozenProperty, value); }
        }
        /// <summary>
        /// ویژگی استقلال نشاندهنده انجماد گره مربوط به این کنترل
        /// </summary>
        public static readonly DependencyProperty IsFrozenProperty =
            DependencyProperty.Register("IsFrozen", typeof(bool), typeof(VertexControl));
        
    }
}