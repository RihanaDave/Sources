namespace GPAS.Dispatch.Entities.Jobs
{
    public class JobRequest
    {
        public int ID
        {
            get;
            set;
        }

        public string RegisterTime
        {
            get;
            set;
        }

        public string BeginTime
        {
            get;
            set;
        }

        public string EndTime
        {
            get;
            set;
        }

        public JobRequestStatus State
        {
            get;
            set;
        }

        public string StatusMeesage
        {
            get;
            set;
        }

        public JobRequestType Type
        {
            get;
            set;
        }
        public string LastPublishedRelationIndex
        {
            get;
            set;
        }
        public string LastPublishedObjectIndex
        {
            get;
            set;
        }
    }
}