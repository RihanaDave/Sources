using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities.Concepts.ImageProcessing;
using GPAS.Workspace.Entities.ImageProcessing;
using GPAS.Workspace.Logic.EntityConvertors;

namespace GPAS.Workspace.Logic
{
    public class ImageProcessingProvider
    {
        public async Task<List<KWBoundingBox>> FaceDetection(byte[] imageFile, string extention)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                ImageAnalysisEntitiesConvertor entityConvertor = new ImageAnalysisEntitiesConvertor();
                return entityConvertor.ConvertBoundingBoxToKWBoundingBox(await remoteServiceClient.FaceDetectionAsync(imageFile, extention));
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }        

        public async Task<List<RetrievedFaceKWObject>> FaceRecognition(byte[] imageFile, string extention, KWBoundingBox[] selectedBoxes, int count = 10)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                ImageAnalysisEntitiesConvertor entityConvertor = new ImageAnalysisEntitiesConvertor();
                BoundingBox[] boundingBoxes = entityConvertor.ConvertKWBoundingBoxToBoundingBox(selectedBoxes.ToList());
                RetrievedFaceKObject[] retrievedFaceKObjects = await remoteServiceClient.FaceRecognitionAsync(imageFile, extention, boundingBoxes, count);
                List<RetrievedFaceKWObject> retrievedFaceKWObjects = await entityConvertor.ConvertRetrievedFaceKObjectToRetrievedFaceKWObject(retrievedFaceKObjects);
                return retrievedFaceKWObjects;
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }       
    }
}
