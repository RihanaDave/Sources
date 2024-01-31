using GPAS.Dispatch.Entities.Concepts.ImageProcessing;
using GPAS.Workspace.Entities.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.EntityConvertors
{
    public class ImageAnalysisEntitiesConvertor
    {
        #region Image Analysis Entities
        internal List<KWBoundingBox> ConvertBoundingBoxToKWBoundingBox(BoundingBox[] boundingBoxs, string distance = "")
        {
            List<KWBoundingBox> result = new List<KWBoundingBox>();
            foreach (var currentBoundingBox in boundingBoxs)
            {
                result.Add(new KWBoundingBox()
                {
                    Height = currentBoundingBox.height,
                    Width = currentBoundingBox.width,
                    TopLeft = currentBoundingBox.topLeft,
                    Landmarks = ConvertLankmarkToKWLandmark(currentBoundingBox.landmarks),
                    Caption = distance
                });
            }
            return result;
        }

        internal KWLandmarks ConvertLankmarkToKWLandmark(Landmarks landmarks)
        {
            KWLandmarks result = new KWLandmarks();
            result.marks = landmarks.marks;
            return result;
        }

        internal BoundingBox[] ConvertKWBoundingBoxToBoundingBox(List<KWBoundingBox> boundingBoxs)
        {
            List<BoundingBox> result = new List<BoundingBox>();
            foreach (var currentBoundingBox in boundingBoxs)
            {
                result.Add(new BoundingBox()
                {
                    height = currentBoundingBox.Height,
                    width = currentBoundingBox.Width,
                    topLeft = currentBoundingBox.TopLeft,
                    landmarks = ConvertKWLandmarkToLandmark(currentBoundingBox.Landmarks)
                });
            }
            return result.ToArray();
        }

        internal Landmarks ConvertKWLandmarkToLandmark(KWLandmarks landmarks)
        {
            Landmarks result = new Landmarks();
            result.marks = landmarks.marks;
            return result;
        }

        internal async Task<List<RetrievedFaceKWObject>> ConvertRetrievedFaceKObjectToRetrievedFaceKWObject(RetrievedFaceKObject[] retrievedFaceKObjects)
        {
            List<RetrievedFaceKWObject> result = new List<RetrievedFaceKWObject>();
            foreach (var currentFace in retrievedFaceKObjects)
            {
                result.Add(new RetrievedFaceKWObject()
                {
                    distance = Math.Round(currentFace.distance, 3),
                    boundingBox = ConvertBoundingBoxToKWBoundingBox(new BoundingBox[] { currentFace.boundingBox }, Math.Round(currentFace.distance, 3).ToString()),
                    kwObject = await DataAccessManager.ObjectManager.GetObjectFromRetrievedDataAsync(currentFace.kObject)
                });
            }
            return result;
        } 
        #endregion
    }
}
