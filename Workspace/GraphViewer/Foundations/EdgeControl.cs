using GraphX.Controls;
using System;
using System.Windows;
using System.Windows.Media;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class EdgeControl : GraphX.Controls.EdgeControl
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="baseEdge">یالی که می بایست کنترل (نمایشی) یال برای آن ایجاد شود</param>
        public EdgeControl(Edge baseEdge)
            : base(baseEdge.Source.RelatedVertexControl, baseEdge.Target.RelatedVertexControl, baseEdge, true, baseEdge.Direction != EdgeDirection.Bidirectional)
        {
            if (baseEdge == null)
                throw new ArgumentNullException("edgeToConstructControlFor");

            base.AlignLabelsToEdges = true;
            base.UpdateLabelPosition = true;
            IsReflexive = (baseEdge.Source == baseEdge.Target);
            UpdateDirection(baseEdge.Direction);
        }

        internal void UpdateDirection(EdgeDirection direction)
        {
            IsDirectedToTarget = (direction == EdgeDirection.FromSourceToTarget);
            IsDirectedToSource = (direction == EdgeDirection.FromTargetToSource);
        }

        public AttachableEdgeLabelControl LabelControl
        {
            get
            {
                if (EdgeLabelControl is AttachableEdgeLabelControl)
                {
                    return (AttachableEdgeLabelControl)EdgeLabelControl;
                }
                else
                {
                    return null;
                }
            }
        }

        public PathGeometry GetEdgePathGeometry()
        {
            return Linegeometry as PathGeometry;
        }

        /// <summary>
        /// نشان می دهد که یال نماینده‌ی یک لینک بازتابی هست یا خیر؛
        /// لینک بازتابی لینک یک شئ با خودش است
        /// </summary>
        public bool IsReflexive
        {
            get { return (bool)GetValue(IsReflexiveProperty); }
            set { SetValue(IsReflexiveProperty, value); }
        }
        public static readonly DependencyProperty IsReflexiveProperty =
            DependencyProperty.Register("IsReflexive", typeof(bool), typeof(EdgeControl));

        public ImageDetails ImageDetails
        {
            get { return (ImageDetails)GetValue(ImageDetailsProperty); }
            set
            {
                // از آنجایی که تنها استفاده کننده‌ی این ویژگی، شرط زیر را برای همه‌ی
                // یال‌ها بررسی می‌کند، برای جلوگیری از سربار پردازشی، این شرط غیرفعال
                // شده است
                //if ((ImageDetails)GetValue(ImageDetailsProperty) == value)
                //    return;
                SetValue(ImageDetailsProperty, value);
            }
        }
        public static readonly DependencyProperty ImageDetailsProperty =
            DependencyProperty.Register(nameof(ImageDetails), typeof(ImageDetails), typeof(EdgeControl));
        public static ImageDetails GetImageDetails(DependencyObject obj)
        {
            return (ImageDetails)obj.GetValue(ImageDetailsProperty);
        }

        public bool IsDirectedToSource
        {
            get { return (bool)GetValue(IsDirectedToSourceProperty); }
            set { SetValue(IsDirectedToSourceProperty, value); }
        }
        public static readonly DependencyProperty IsDirectedToSourceProperty =
            DependencyProperty.Register("IsDirectedToSource", typeof(bool), typeof(EdgeControl));

        public bool IsDirectedToTarget
        {
            get { return (bool)GetValue(IsDirectedToTargetProperty); }
            set { SetValue(IsDirectedToTargetProperty, value); }
        }
        public static readonly DependencyProperty IsDirectedToTargetProperty =
            DependencyProperty.Register("IsDirectedToTarget", typeof(bool), typeof(EdgeControl));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EdgeControl),
                new PropertyMetadata(false, OnSetIsSelectedChanged));

        private static void OnSetIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EdgeControl)d).OnSetIsSelectedChanged(e);
        }

        private void OnSetIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            OnIsSelectedChanged(e);
        }

        public event DependencyPropertyChangedEventHandler IsSelectedChanged;
        protected void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            IsSelectedChanged?.Invoke(this, e);
        }
    }
}
