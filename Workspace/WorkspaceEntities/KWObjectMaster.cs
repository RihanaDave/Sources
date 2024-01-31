namespace GPAS.Workspace.Entities
{
    public class KWObjectMaster
    {
        public long Id { get; set; }

        public long MasterId { get; set; }

        public long[] ResolveTo { get; set; }
    }
}
