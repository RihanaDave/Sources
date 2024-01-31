using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GPAS.Ontology
{
    public class OntologyNode : BaseModel
    {
        #region Properties

        private OntologyNode parent;
        public virtual OntologyNode Parent
        {
            get { return parent; }
            set
            {
                if (SetValue(ref parent, value))
                {
                    if (Parent != null && !Parent.children.Contains(this))
                    {
                        Parent.Children.Add(this);
                    }

                    OnParentChanged();
                }
            }
        }

        private ObservableCollection<OntologyNode> children = new ObservableCollection<OntologyNode>();
        public ObservableCollection<OntologyNode> Children
        {
            get { return children; }
            set
            {
                object oldValue = children;
                if (SetValue(ref children, value))
                {
                    if (children != null)
                    {
                        children.CollectionChanged -= Children_CollectionChanged;
                        children.CollectionChanged += Children_CollectionChanged;
                    }

                    OnChildrenChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, children, oldValue));
                }
            }
        }

        private string typeUri = string.Empty;
        public string TypeUri
        {
            get { return typeUri; }
            set
            {
                if (SetValue(ref typeUri, value))
                {
                    OnTypeUriChanged();
                }
            }
        }

        public bool IsLeaf
        {
            get { return Children == null || Children.Count == 0; }
        }

        public bool IsRoot
        {
            get { return Parent == null; }
        }

        #endregion

        #region Methodes

        public OntologyNode()
        {
            children.CollectionChanged -= Children_CollectionChanged;
            children.CollectionChanged += Children_CollectionChanged;
        }

        /// <summary>
        /// تمامی فرزندان و فرزندهای فرندان گره را برمیگرداند
        /// </summary>
        /// <param name="allChildren"> خروجی تابع </param>
        public IList<OntologyNode> GetAllChildren()
        {
            List<OntologyNode> allChildren = new List<OntologyNode>();
            GetAllChildren(this, ref allChildren);
            return allChildren;
        }

        private void GetAllChildren(OntologyNode node, ref List<OntologyNode> allChildren)
        {
            if (node.Children?.Count > 0)
            {
                allChildren.AddRange(node.Children);
                foreach (var child in node.Children)
                {
                    GetAllChildren(child, ref allChildren);
                }
            }
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {

            }

            if (e.OldItems?.Count > 0)
            {
                foreach (OntologyNode child in e.OldItems)
                {
                    if (child.Parent.Equals(this))
                    {
                        child.Parent = null;
                    }
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (OntologyNode child in e.NewItems)
                {
                    child.Parent = this;
                }
            }

            OnChildrenChanged(e);
        }

        public OntologyNode Find(string typeUri)
        {
            if (TypeUri == typeUri)
                return this;

            foreach (OntologyNode child in GetAllChildren())
            {
                if (child.TypeUri == typeUri)
                    return child;
            }

            return null;
        }

        #endregion

        #region Events

        public event EventHandler ParentChanged;
        protected void OnParentChanged()
        {
            ParentChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler ChildrenChanged;
        protected void OnChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            ChildrenChanged?.Invoke(this, e);
        }

        public event EventHandler TypeUriChanged;
        protected void OnTypeUriChanged()
        {
            TypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
