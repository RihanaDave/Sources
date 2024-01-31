using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FileRepository.Logic
{
    public class TimeoutBasedWebClient : WebClient
    {
        public int TimeoutMiliSeconds { get; set; }

        public TimeoutBasedWebClient()
            : base()
        {
            TimeoutMiliSeconds = 600000;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            // Code Source: https://stackoverflow.com/questions/1789627/how-to-change-the-timeout-on-a-net-webclient-object
            WebRequest instance = base.GetWebRequest(address);
            instance.Timeout = TimeoutMiliSeconds;
            return instance;
        }
    }
}
