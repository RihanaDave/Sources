using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataSynchronization
{
    internal class SynchronizingConceptIDsBatch
    {
        internal SynchronizingConceptIDsBatch()
        {
            IDs = new SortedSet<long>();
        }
        internal SynchronizingConceptIDsBatch(IEnumerable<long> appendingIDs)
        {
            IDs = new SortedSet<long>(appendingIDs);
        }

        internal void AppendIDs(IEnumerable<long> appendingIDs)
        {
            foreach (long id in appendingIDs)
            {
                AppendID(id);
            }
        }
        internal void AppendID(long appendingID)
        {
            IDs.Add(appendingID);
        }

        private SortedSet<long> IDs { get; set; }

        internal IEnumerable<long> GetIDs()
        {
            return IDs;
        }

        internal int Size { get { return IDs.Count; } }

        internal string Description
        {
            get
            {
                if (IDs.Max == IDs.Min + IDs.Count - 1)
                {// IDs are range
                    return $"{IDs.Min} - {IDs.Max}";
                }
                else if (IDs.Count > 3)
                {
                    return $"{IDs.ElementAt(0)}, {IDs.ElementAt(1)} & {IDs.Count - 2} more ID(s)";
                }
                else if (IDs.Count == 3)
                {
                    return $"{IDs.ElementAt(0)}, {IDs.ElementAt(1)} & {IDs.ElementAt(2)}";
                }
                else if (IDs.Count == 2)
                {
                    return $"{IDs.ElementAt(0)} & {IDs.ElementAt(1)}";
                }
                else if (IDs.Count == 1)
                {
                    return $"{IDs.ElementAt(0)}";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
