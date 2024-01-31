using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GPAS.LoadTest.Core
{
    public interface ILoadTestCategory
    {
        ServerType ServerType { get; set; }

        TestClassType TestClassType { get; set; }

        string Title { get; set; }

        Task<List<LoadTestResult>> RunTests(long startStore, long endStore, long startRetrieve, long endRetrieve, CancellationToken tokenSource);

        event EventHandler<StepsProgressChangedEventArgs> StepsProgressChanged;

        event EventHandler<TestCaseProgressChangedEventArgs> TestCaseProgressChanged;
    }
}
