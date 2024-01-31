using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.JobServer.JobWorkerProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.QualityTools.Testing.Fakes;
using GPAS.JobServer.Logic.Fakes;
using GPAS.JobsManagement;

namespace GPAS.JobServer.JobWorkerProcess.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        // این تست‌ها یاید با انتقال منطق به لایه منطق، اجرا شود و در حال حاضر به خاطر وابستگی به
        // کنسول قابلیت استفاده ندارد
        //[TestMethod()]
        public void StartJobWork_NonBusyJobIdInput_ReturnZero()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                int jobId = 15; // Not exist job with id 15 in DB
                int returnValue;

                ShimJobsStoreAndRetrieveProvider.IsBusyJobInt32 = (i) =>
                { return false; };
                Fakes.ShimProgram.JobIDGet = () => { return jobId; };
                // Act
                returnValue = Program.StartJobWork();
                // Assert
                Assert.AreEqual(0, returnValue);
            }
        }

        // این تست‌ها یاید با انتقال منطق به لایه منطق، اجرا شود و در حال حاضر به خاطر وابستگی به
        // کنسول قابلیت استفاده ندارد
        //[TestMethod()]
        public void StartJobWork_RequestRetrivationNullResult_ReturnZero()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                int jobId = 15; // The Job is Busy, but DB returns "null" for Request retrivation
                int returnValue;

                ShimJobsStoreAndRetrieveProvider.IsBusyJobInt32 = (i) =>
                { return true; };
                ShimJobsStoreAndRetrieveProvider.GetJobByIdInt32 = (i) =>
                { return null; };
                ShimJobsStoreAndRetrieveProvider.SetFailStateForJobInt32String = (i, s) =>
                { };
                Fakes.ShimProgram.JobIDGet = () => { return jobId; };
                // Act
                returnValue = Program.StartJobWork();
                // Assert
                Assert.AreEqual(0, returnValue);
            }
        }

        // این تست‌ها یاید با انتقال منطق به لایه منطق، اجرا شود و در حال حاضر به خاطر وابستگی به
        // کنسول قابلیت استفاده ندارد
        //[TestMethod()]
        public void StartJobWork_RequestRetrivationThrowsException_ReturnZero()
        {
            using (ShimsContext.Create())
            {
                // Arrange
                int jobId = 15; // The Job is Busy, but DB throws exception for Request retrivation
                int returnValue;

                ShimJobsStoreAndRetrieveProvider.IsBusyJobInt32 = (i) =>
                { return true; };
                ShimJobsStoreAndRetrieveProvider.GetJobByIdInt32 = (i) =>
                { throw new Exception(); };
                ShimJobsStoreAndRetrieveProvider.SetFailStateForJobInt32String = (i, s) =>
                { };
                Fakes.ShimProgram.JobIDGet = () => { return jobId; };
                // Act
                returnValue = Program.StartJobWork();
                // Assert
                Assert.AreEqual(0, returnValue);
            }
        }
    }
}