using GPAS.TimelineViewer;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    public class TimelineProvider
    {
        private string ConvertBinSizesToString(BinScaleLevel binScaleLevel, double binFactor)
        {
            return string.Format("+{0}{1}", binFactor, binScaleLevel.ToString().ToUpper());
        }

        public long GetTimeLineMaxFrequecyCount(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            string binLevel = ConvertBinSizesToString(binScaleLevel, binFactor);

            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return remoteServiceClient.GetTimeLineMaxFrequecyCount(propertiesTypeUri, binLevel);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public DateTime GetTimeLineMaxDate(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            string binLevel = ConvertBinSizesToString(binScaleLevel, binFactor);

            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return remoteServiceClient.GetTimeLineMaxDate(propertiesTypeUri, binLevel);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public DateTime GetTimeLineMinDate(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            string binLevel = ConvertBinSizesToString(binScaleLevel, binFactor);

            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return remoteServiceClient.GetTimeLineMinDate(propertiesTypeUri, binLevel);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
    }
}
