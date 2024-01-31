using System.ServiceProcess;


namespace GPAS.JobServer.JobMonitoringAgent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new JobMonitoringAgentService(args)
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
