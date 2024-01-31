using System.Data.Entity;
using GPAS.JobServer.Data.Models;

namespace GPAS.JobServer.Data.Context
{
    public class JobsDBEntities : DbContext
    {
        public JobsDBEntities() : base("JobsDBEntities")
        {
            Database.SetInitializer<JobsDBEntities>(new CreateDatabaseIfNotExists<JobsDBEntities>());
        }
        public DbSet<JobsTable> JobsTables { get; set; }
    }
}
