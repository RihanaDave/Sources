using System.Collections.ObjectModel;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public interface IResolvedVertex
    {
        int NumberOfInnerVertices { get; }
        MasterVertex Master { get; set; }
        ObservableCollection<SlaveVertex> Slaves { get; set; }
    }
}
