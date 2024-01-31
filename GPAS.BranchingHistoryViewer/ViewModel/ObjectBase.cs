using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.BranchingHistoryViewer.ViewModel
{
    public class ObjectBase : FrameworkElement
    {
        public List<ObjectBase> Children
        {
            get;
            set;
        }
        static ObjectBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjectBase),
               new FrameworkPropertyMetadata(typeof(ObjectBase)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public static readonly RoutedEvent GetSequenceEvent =
         EventManager.RegisterRoutedEvent("GetSequence", RoutingStrategy.Bubble,
         typeof(RoutedEventHandler), typeof(ObjectBase));

        public event RoutedEventHandler GetSequence
        {
            add { AddHandler(GetSequenceEvent, value); }
            remove { RemoveHandler(GetSequenceEvent, value); }
        }

        public ObjectBase GetRoot()
        {
            if (ParentObject == null)
                return this;
            else
                return ParentObject.GetRoot();
        }

        public void AddChildren(List<ObjectBase> objectBases)
        {
            foreach (var item in objectBases)
            {
                item.ParentObject = this;
            }
        }

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(ObjectBase), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ObjectBase), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(ObjectBase), new PropertyMetadata(""));

        public bool IsInActiveSequence
        {
            get { return (bool)GetValue(IsInActiveSequenceProperty); }
            set { SetValue(IsInActiveSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInActiveSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInActiveSequenceProperty =
            DependencyProperty.Register("IsInActiveSequence", typeof(bool), typeof(ObjectBase), new PropertyMetadata(false));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ObjectBase), new PropertyMetadata(false));

        public virtual void OnGetSequence()
        {
            IsActive = true;
            foreach (var item in GetAllObjectsInTree(this))
            {
                item.IsInActiveSequence = false;

                if (item != this)
                    item.IsActive = false;
            }

            SelectSequence(this);
            RoutedEventArgs args = new RoutedEventArgs(ObjectBase.GetSequenceEvent);
            RaiseEvent(args);
        }

        public List<ObjectBase> GetAllObjectsInTree()
        {
            return GetAllObjectsInTree(this);
        }

        private List<ObjectBase> GetAllObjectsInTree(ObjectBase obj)
        {
            if (obj == null)
                return null;
            
            return GetAllChildren(obj.GetRoot());
        }

        public List<ObjectBase> GetAllChildren(ObjectBase obj)
        {
            if (obj == null)
                return null;

            List<ObjectBase> Result = new List<ObjectBase>();
            Result.Add(obj);

            if (obj.Children != null && obj.Children.Count > 0)
            {
                foreach (var child in obj.Children)
                {
                    Result.AddRange(GetAllChildren(child));
                }
            }

            return Result;
        }

        private void SelectSequence(ObjectBase obj)
        {
            if (obj == null)
                return;
            
            obj.IsInActiveSequence = true;
            SelectSequence(obj.ParentObject);
        }

        public List<ObjectBase> FindSequence()
        {
            List<ObjectBase> sequence = new List<ObjectBase>();

            sequence.Add(this);
            var par = ParentObject;
            while (true)
            {
                if (par == null)
                    return sequence;

                sequence.Add(par);
                par = par.ParentObject;
            }
        }

        public int VerticalDept
        {
            get { return (int)GetValue(VerticalDeptProperty); }
            set { SetValue(VerticalDeptProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalDept.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalDeptProperty =
            DependencyProperty.Register("VerticalDept", typeof(int), typeof(ObjectBase), new PropertyMetadata(0, OnSetVerticalDeptChanged));

        private static void OnSetVerticalDeptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ObjectBase).OnSetVerticalDeptChanged(e);
        }

        private void OnSetVerticalDeptChanged(DependencyPropertyChangedEventArgs e)
        {
            DeptChanged?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> DeptChanged;

        public int HorizontalDept
        {
            get { return (int)GetValue(HorizontalDeptProperty); }
            set { SetValue(HorizontalDeptProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalDept.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalDeptProperty =
            DependencyProperty.Register("HorizontalDept", typeof(int), typeof(ObjectBase), new PropertyMetadata(0, OnSetHorizontalDeptChanged));

        private static void OnSetHorizontalDeptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ObjectBase).OnSetHorizontalDeptChanged(e);
        }

        private void OnSetHorizontalDeptChanged(DependencyPropertyChangedEventArgs e)
        {
            DeptChanged?.Invoke(this, new EventArgs());
        }

        public bool IsDerivedObject
        {
            get { return (bool)GetValue(IsDerivedObjectProperty); }
            private set { SetValue(IsDerivedObjectProperty, value); }
        }

        public event EventHandler<RecomputeEventArgs> Recompute;

        public virtual void OnRecompute()
        {
            Recompute?.Invoke(this, new RecomputeEventArgs(this));
            OnGetSequence();
        }

        // Using a DependencyProperty as the backing store for IsDerivedObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDerivedObjectProperty =
            DependencyProperty.Register("IsDerivedObject", typeof(bool), typeof(ObjectBase), new PropertyMetadata(false));

        public ConnectionLink InputLink
        {
            get { return (ConnectionLink)GetValue(InputLinkProperty); }
            set { SetValue(InputLinkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputLinkProperty =
            DependencyProperty.Register("InputLink", typeof(ConnectionLink), typeof(ObjectBase), new PropertyMetadata(null, OnSetInputLinkChanged));

        private static void OnSetInputLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as ObjectBase;
            o.OnSetInputLinkChanged(e);
        }

        private void OnSetInputLinkChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        public ObjectBase ParentObject
        {
            get { return (ObjectBase)GetValue(ParentObjectProperty); }
            set { SetValue(ParentObjectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParentObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentObjectProperty =
            DependencyProperty.Register("ParentObject", typeof(ObjectBase), typeof(ObjectBase), new PropertyMetadata(null, OnSetParentObjectChanged));

        private static void OnSetParentObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var o = d as ObjectBase;
            o.OnSetParentObjectChanged(e);
        }

        private void OnSetParentObjectChanged(DependencyPropertyChangedEventArgs e)
        {
            if(ParentObject==null)
            {
                IsDerivedObject = false;
            }
            else
            {
                IsDerivedObject = true;
            }

            if (ParentObject?.Children == null)
                ParentObject.Children = new List<ObjectBase>();

            ParentObject?.Children?.Add(this);
        }

        public bool AllowDragDrop
        {
            get { return (bool)GetValue(AllowDragDropProperty); }
            set { SetValue(AllowDragDropProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowDragDrop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowDragDropProperty =
            DependencyProperty.Register("AllowDragDrop", typeof(bool), typeof(ObjectBase), new PropertyMetadata(false));
    }
}
