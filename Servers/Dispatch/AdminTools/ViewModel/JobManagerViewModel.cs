using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Logic;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class JobManagerViewModel : BaseViewModel
    {
        public JobManagerViewModel()
        {
            JobsRequestMainList = new ObservableCollection<JobModel>();
            JobsRequestList = new ObservableCollection<JobModel>();
        }

        #region Property

        private long allItemNumber=1;
#pragma warning disable CS0108 // 'JobManagerViewModel.AllItemNumber' hides inherited member 'BaseViewModel.AllItemNumber'. Use the new keyword if hiding was intended.
        public long AllItemNumber
#pragma warning restore CS0108 // 'JobManagerViewModel.AllItemNumber' hides inherited member 'BaseViewModel.AllItemNumber'. Use the new keyword if hiding was intended.
        {
            get => allItemNumber;
            set
            {
                allItemNumber = value;
                OnPropertyChanged();
            }
        }

        private int itemPerPage = 10;
        public int ItemPerPage
        {
            get => itemPerPage;
            set
            {
                itemPerPage = value;
                OnPropertyChanged();
            }
        }

        private long pageNumber = 0;
        public long PageNumber
        {
            get => pageNumber;
            set
            {
                pageNumber = value;
                OnPropertyChanged();
            }
        }
        private bool isExistSelectedJobRequestAfterRefresh;
        public bool IsExistSelectedJobRequestAfterRefresh
        {
            get => isExistSelectedJobRequestAfterRefresh;
            set
            {
                isExistSelectedJobRequestAfterRefresh = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ObservableCollection<JobModel> JobsRequestMainList { get; set; }
        public ObservableCollection<JobModel> JobsRequestList { get; set; }

        public int SelectetItemId { get; set; }

        private async void RefreshStatusImport()
        {
            await GetJobsRequest();
        }

        public void GetStatusImportTimerOnTick(object sender, EventArgs e)
        {
            if (SelectedJobRequest != null)
            {
                SelectetItemId = SelectedJobRequest.Id;
            }

            RefreshStatusImport();
        }

        public async Task GetJobsRequest()
        {
            var jobsRequestList = await JobsProvider.GetJobsRequestAsync();
            PrepareJobsRequestListToShow(jobsRequestList);
        }

        public async Task RestartJob(object selectedJob)
        {
            await Task.Run(() =>
            {
                JobsProvider.SetPendingStateForJob(((JobModel)selectedJob).Id);
            });
        }

        public async Task StopJob(object selectedJob)
        {
            await Task.Run(() =>
            {
                JobsProvider.SetPauseStateForJob(((JobModel)selectedJob).Id);
            });
        }

        public async Task ResumeJob(object selectedJob)
        {
            await Task.Run(() =>
            {
                JobsProvider.SetResumedStateForJob(((JobModel)selectedJob).Id);
            });
        }

        private void PrepareJobsRequestListToShow(List<Entities.Jobs.JobRequest> jobList)
        {
            if (JobsRequestMainList.Count != 0)
                JobsRequestMainList.Clear();

            if (JobsRequestList.Count != 0)
                JobsRequestList.Clear();


            foreach (var job in jobList)
            {

                JobsRequestMainList.Add(new JobModel
                {
                    Id = job.ID,
                    Type = job.Type,
                    BeginTime = job.BeginTime,
                    EndTime = job.EndTime,
                    RegisterTime = job.RegisterTime,
                    State = job.State,
                    StatusMessage = job.StatusMeesage,
                    LastPublishedObjectIndex = job.LastPublishedObjectIndex,
                    LastPublishedRelationIndex = job.LastPublishedRelationIndex,
                    PercentageRateDone = GetPercentageRateDone(job.ID)
                });
            }

            SelectedJobRequest = JobsRequestMainList.FirstOrDefault(J => J.Id.Equals(SelectetItemId));
            if (SelectedJobRequest == null)
            {
                IsExistSelectedJobRequestAfterRefresh = false;
            }
            else
            {
                IsExistSelectedJobRequestAfterRefresh = true;
            }

            FillJobsRequestList();

        }
        private double GetPercentageRateDone(int jobId)
        {
            return JobsProvider.GetImprotingPercent(jobId);
        }

        public void FillJobsRequestList()
        {
            long startIndex = (PageNumber * ItemPerPage);
            long endIndex = (startIndex + ItemPerPage);
            AllItemNumber = JobsRequestMainList.Count();
            if (JobsRequestList.Count != 0)
                JobsRequestList.Clear();

            for (int index = (int)startIndex; index < endIndex; index++)
            {
                if (index >= JobsRequestMainList.Count) break;
                JobsRequestList.Add(JobsRequestMainList[index]);
            }
        }
      
    }
}
