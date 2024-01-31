using GPAS.Dispatch.Entities.Concepts;
using GPAS.Utility;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Logic
{
    public class InvestigationProvider
    {       
        public async Task SaveInvestigation(string investigatioinTitle, string investigationDescription, InvestigationStatus investigationStatus, byte[] investigationImage)
        {
            if (investigatioinTitle == null)
                throw new ArgumentNullException(nameof(investigatioinTitle));
            if (investigationDescription == null)
                throw new ArgumentNullException(nameof(investigationDescription));
            if (investigationStatus == null)
                throw new ArgumentNullException(nameof(investigationStatus));
            if (investigationImage == null)
                throw new ArgumentNullException(nameof(investigationImage));

            MemoryStream streamWriter = new MemoryStream();
            InvestigationStatus.Serialize(streamWriter, investigationStatus);
            StreamUtility streamUtil = new StreamUtility();
            byte[] investigationStatusByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);
            DateTime saveGraphTime = DateTime.Now;
            string saveGraphTimeString = saveGraphTime.ToString(CultureInfo.InvariantCulture);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                long id = await remoteServiceClient.GetNewInvestigationIdAsync();
                GPAS.Workspace.ServiceAccess.RemoteService.KInvestigation kinvestigation =
                    new GPAS.Workspace.ServiceAccess.RemoteService.KInvestigation()
                    {
                        Title = investigatioinTitle,
                        Description = investigationDescription,
                        InvestigationImage = investigationImage,
                        InvestigationStatus = investigationStatusByteArray,
                        CreatedTime = saveGraphTimeString,
                        Id = id
                    };
                await remoteServiceClient.SaveInvestigationAsync(kinvestigation);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<List<GPAS.Workspace.ServiceAccess.RemoteService.InvestigationInfo>> GetSavedInvestigations()
        {            
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {                
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                
                List<GPAS.Workspace.ServiceAccess.RemoteService.InvestigationInfo> savedInvestigations =
                    (await remoteServiceClient.GetSavedInvestigationsAsync()).ToList();                              

                return savedInvestigations;
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<byte[]> GetSavedInvestigationImage(long investigationID)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();

                byte[] savedInvestigationImage =
                    await remoteServiceClient.GetSavedInvestigationImageAsync(investigationID);

                return savedInvestigationImage;
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<byte[]> GetSavedInvestigationStatus(long investigationID)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();

                byte[] savedInvestigationImage =
                    await remoteServiceClient.GetSavedInvestigationStatusAsync(investigationID);

                return savedInvestigationImage;
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public static async Task AddUnpublishedConceptsToCache(SaveInvestigationUnpublishedConcepts savedUnpublishedConcepts)
        {
            DataAccessManager.UnpublishedChangesManager unpublishedChangesManager = new DataAccessManager.UnpublishedChangesManager();
            await unpublishedChangesManager.AddUnpublishedConceptsToCache(savedUnpublishedConcepts);            
        }
    }
}
