namespace GPAS.Horizon.ServerManager
{
    internal class ResetArguments : ConsoleInput
    {
        internal bool IsMaximumConcurrentSynchronizationsSet = false;
        internal byte MaximumConcurrentSynchronizations = 0;

        internal bool IsNumberOfObjectsForGetSequentialSet = false;
        internal int NumberOfObjectsForGetSequential = 0;
    }
}
