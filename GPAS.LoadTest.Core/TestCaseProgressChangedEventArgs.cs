using System;

namespace GPAS.LoadTest.Core
{
    public class TestCaseProgressChangedEventArgs : EventArgs
    {
        public string BatchCount { get; set; }

        public string CurrentBatch { get; set; }

        public TestCaseProgressChangedEventArgs(string batchCount, string currentBatch)
        {
            BatchCount = batchCount;
            CurrentBatch = currentBatch;
        }
    }
}
