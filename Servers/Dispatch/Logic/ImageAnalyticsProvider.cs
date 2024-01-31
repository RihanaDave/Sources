using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.ImageProcessing;
using GPAS.Dispatch.ServiceAccess.SearchService;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class ImageAnalyticsProvider
    {
        private string CallerUserName = "";
        public ImageAnalyticsProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }
        private List<Entities.Concepts.ImageProcessing.BoundingBox> ConvertSearchBoundingBoxToDispatchBoundingBox(List<ServiceAccess.SearchService.BoundingBox> faces)
        {
            List<Entities.Concepts.ImageProcessing.BoundingBox> boundingBoxs = new List<Entities.Concepts.ImageProcessing.BoundingBox>();
            foreach (var item in faces)
            {
                boundingBoxs.Add(
                    new Entities.Concepts.ImageProcessing.BoundingBox()
                    {
                        topLeft = new System.Drawing.Point(item.topLeft.x, item.topLeft.y),
                        width = item.width,
                        height = item.height,
                        landmarks = new Entities.Concepts.ImageProcessing.Landmarks()
                        {
                            marks = item.landmarks.marks.ToList()
                        }
                    }
                    );
            }
            return boundingBoxs;
        }
        private Entities.Concepts.ImageProcessing.BoundingBox ConvertSearchBoundingBoxToDispatchBoundingBox(ServiceAccess.SearchService.BoundingBox searchBoundingBox)
        {
            return new Entities.Concepts.ImageProcessing.BoundingBox()
            {
                topLeft = new System.Drawing.Point(searchBoundingBox.topLeft.x
                , searchBoundingBox.topLeft.y),
                height = searchBoundingBox.height,
                width = searchBoundingBox.width, 
                landmarks = new Entities.Concepts.ImageProcessing.Landmarks()
                {
                    marks = new List<double>()
                }
            };
        }
        private List<ServiceAccess.SearchService.BoundingBox> ConvertDispatchBoundingBoxToSearchBoundingBox(List<Entities.Concepts.ImageProcessing.BoundingBox> result)
        {
            List<ServiceAccess.SearchService.BoundingBox> boundingBoxs = new List<ServiceAccess.SearchService.BoundingBox>();
            foreach (var item in result)
            {
                boundingBoxs.Add(
                    new ServiceAccess.SearchService.BoundingBox()
                    {
                        topLeft = new Point()
                        {
                            x = item.topLeft.X,
                            y = item.topLeft.Y
                        },
                        width = item.width,
                        height = item.height,
                        landmarks = new ServiceAccess.SearchService.Landmarks()
                        {
                           marks = item.landmarks.marks.ToArray()
                        }
                    }
                    );
            }
            return boundingBoxs;
        }
        private List<RetrievedFaceKObject> GetRetrievedFaceKObjects(List<RetrievedFace> retrievedFaces)
        {
            AuthorizationParametters authorizationParametter
          = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            List<Entities.Concepts.KObject> kObjects = new List<Entities.Concepts.KObject>();
            RepositoryProvider repositoryProvider = null;
            List<RetrievedFaceKObject> retrievedFaceKObjects = new List<RetrievedFaceKObject>();
            try
            {
                repositoryProvider = new RepositoryProvider(CallerUserName);
                kObjects = repositoryProvider.GetObjects(retrievedFaces.Select(o => long.Parse(o.imageId)).ToArray());
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }

            foreach (var item in retrievedFaces)
            {
                retrievedFaceKObjects.Add(
                        new RetrievedFaceKObject()
                        {
                            boundingBox = ConvertSearchBoundingBoxToDispatchBoundingBox(item.boundingBox),
                            distance = item.distance,
                            kObject = kObjects.Where(o => o.Id == long.Parse(item.imageId)).FirstOrDefault()
                        }
                    );
            }
            return retrievedFaceKObjects;
        }

        public List<RetrievedFaceKObject> FaceRecognition(byte[] imageFile, string extention, List<Entities.Concepts.ImageProcessing.BoundingBox> boundingBoxs, int count)
        {
            AuthorizationParametters authorizationParametter
           = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                var retrievedFaces = proxy.FaceRecognition(imageFile, extention, ConvertDispatchBoundingBoxToSearchBoundingBox(boundingBoxs).ToArray(), count, authorizationParametter).ToList();
                return GetRetrievedFaceKObjects(retrievedFaces);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        public List<Entities.Concepts.ImageProcessing.BoundingBox> FaceDetection(byte[] imageFile, string extention)
        {
            AuthorizationParametters authorizationParametter
            = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                var faces = proxy.FaceDetection(imageFile, extention, authorizationParametter).ToList();
                return ConvertSearchBoundingBoxToDispatchBoundingBox(faces);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public bool IsMachneVisonServiceInstalled()
        {
            AuthorizationParametters authorizationParametter
          = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return (proxy.IsMachneVisonServiceInstalled());
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}
