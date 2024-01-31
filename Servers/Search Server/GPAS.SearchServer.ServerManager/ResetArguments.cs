namespace GPAS.SearchServer.ServerManager
{
    internal class ResetArguments : ConsoleInput
    {
        internal bool IsMaximumConcurrentSynchronizationsSet = false;
        internal byte MaximumConcurrentSynchronizations = 0;

        internal bool IsNumberOfObjectsForGetSequentialSet = false;
        internal int NumberOfObjectsForGetSequential = 0;

        internal bool IsFirstObjectIDSet = false;
        internal int FirstObjectID = 0;

        internal bool IsFirstDataSourceIDSet = false;
        internal int FirstDataSourceID = 0;

        internal bool IsLastObjectIDSet = false;
        internal int LastObjectID = 0;

        internal bool IsLastDataSourceIDSet = false;
        internal int LastDataSourceID = 0;
    }
}
