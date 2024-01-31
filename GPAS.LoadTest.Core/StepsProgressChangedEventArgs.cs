using System;
using System.Collections.Generic;

namespace GPAS.LoadTest.Core
{
    public class StepsProgressChangedEventArgs : EventArgs
    {
        public string TotalStepsCount { get; set; }

        public string CurrentStepNumber { get; set; }

        public string StepName { get; set; }

        public List<LoadTestResult> StepResult { get; set; }

        public StepsProgressChangedEventArgs(string totalStepsCount, string currentStepNumber, string stepName, List<LoadTestResult> stepResult)
        {
            TotalStepsCount = totalStepsCount;
            CurrentStepNumber = currentStepNumber;
            StepName = stepName;
            StepResult = stepResult;
        }
    }
}
