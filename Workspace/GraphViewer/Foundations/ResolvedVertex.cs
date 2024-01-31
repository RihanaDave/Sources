using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public abstract class ResolvedVertex : Vertex, IResolvedVertex
    {
        protected internal ResolvedVertex(string vertexTitle, VertexControl relatedVertexControl = null)
            : base(vertexTitle, relatedVertexControl, true)
        {
            Slaves = new ObservableCollection<SlaveVertex>();
        }

        public int NumberOfInnerVertices { get; protected set; }

        private MasterVertex master;
        public MasterVertex Master
        {
            get => master;
            set
            {
                if (SetValue(ref master, value))
                {
                    NumberOfInnerVertices = GetNumberOfInnerVertices(Master, Slaves);
                }
            }
        }

        private ObservableCollection<SlaveVertex> slaves = new ObservableCollection<SlaveVertex>();
        public ObservableCollection<SlaveVertex> Slaves
        {
            get => slaves;
            set
            {
                ObservableCollection<SlaveVertex> oldVal = Slaves;
                if (SetValue(ref slaves, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= Slaves_CollectionChanged;
                    }
                    if (Slaves == null)
                    {
                        Slaves_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Slaves.CollectionChanged -= Slaves_CollectionChanged;
                        Slaves.CollectionChanged += Slaves_CollectionChanged;

                        if (oldVal == null)
                        {
                            Slaves_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Slaves));
                        }
                        else
                        {
                            Slaves_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Slaves, oldVal));
                        }
                    }
                }
            }
        }

        private void Slaves_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NumberOfInnerVertices = GetNumberOfInnerVertices(Master, Slaves);
        }

        private int GetNumberOfInnerVertices(MasterVertex master, ObservableCollection<SlaveVertex> slaves)
        {
            if (Slaves == null)
                return Master == null ? 0 : 1;

            return Master == null ? Slaves.Count : Slaves.Count + 1;
        }
    }
}
