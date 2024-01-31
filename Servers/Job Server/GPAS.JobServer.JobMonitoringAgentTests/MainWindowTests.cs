using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.JobServer.JobMonitoringAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using GPAS.JobServer.Logic.Fakes;
using GPAS.JobServer.Logic.Entities;
using GPAS.JobServer.JobMonitoringAgent.Fakes;

namespace GPAS.JobServer.JobMonitoringAgent.Tests
{
    [TestClass()]
    public class MainWindowTests
    {
        [ExpectedException(typeof(System.ArgumentNullException))]
        // این تست به خاطر وابستگی به کتابخانه‌ی دارای رابط کاربری غیرفعال شد
        //[TestMethod()]
        public void CheckoutJobsTest()
        {
            using (ShimsContext.Create())
            {
                ShimJobsStoreAndRetrieveProvider.AllInstances.GetJobRequests = (jdp) =>
                {
                    List<JobRequest> l = new List<JobRequest>();
                    l.Add(new JobRequest()
                    {
                        RegisterTime = null,
                        State = JobRequestStatus.Pending
                    });
                    return l;
                };

                ShimJobMonitoringAgentService.AllInstances.IsProcessAliveInt32 = (a, b) => { return true; };

                JobMonitoringAgentService jmaService = new JobMonitoringAgentService(new string[] { });
                jmaService.CheckoutJobs();
            }
        }

        [ExpectedException(typeof(System.FormatException))]
        // این تست به خاطر وابستگی به کتابخانه‌ی دارای رابط کاربری غیرفعال شد
        //[TestMethod()]
        public void CheckoutJobsTest_DateTimeInvalidFormat_ExpectedException()
        {
            using (ShimsContext.Create())
            {
                ShimJobsStoreAndRetrieveProvider.AllInstances.GetJobRequests = (jdp) =>
                {
                    List<JobRequest> l = new List<JobRequest>();
                    l.Add(new JobRequest()
                    {
                        RegisterTime = "abc",
                        State = JobRequestStatus.Pending
                    });
                    return l;
                };

                ShimJobMonitoringAgentService.AllInstances.IsProcessAliveInt32 = (a, b) => { return true; };

                JobMonitoringAgentService jmaService = new JobMonitoringAgentService(new string[] { });
                jmaService.CheckoutJobs();
            }
        }
    }
}