using System;

namespace GPAS.JobServer.Logic.Entities
{
    public class JobRequest
    {
        public int ID
        {
            get;
            set;
        }

        public String RegisterTime
        {
            get;
            set;
        }

        public String BeginTime
        {
            get;
            set;
        }

        public String EndTime
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
        public String LastPublishedRelationIndex
        {
            get;
            set;
        }
        public String LastPublishedObjectIndex
        {
            get;
            set;
        }
    }
}