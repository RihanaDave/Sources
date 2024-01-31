using GPAS.AccessControl;
using GPAS.SearchServer.Access;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Access.SearchEngine.ApacheSolr;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GPAS.SearchServer.Logic
{
    public class ImageProcessingProvider
    {
        public static bool IsVisonServiceInstalled = bool.Parse(ConfigurationManager.AppSettings["MachineVisonServiceInstalled"]);
        public ImageDocument GetImageDocument(AccessControled<SearchObject> addedDoc)
        {
            if (!IsVisonServiceInstalled)
                throw new InvalidOperationException("Machine-Vision service is not installed");

            var fileRepoDataClient = new FileRepositoryDataClient();
            byte[] documentContent = fileRepoDataClient.GetDocumentContentFromFileRepository(addedDoc.ConceptInstance.Id);
            List<FaceSpecification> faces = new List<FaceSpecification>();
            if (documentContent != null && documentContent.Length > 0)
            {
                Dictionary<BoundingBox, List<double>> imagesFaceSpecifications = ExtractFaceSpecifications(new List<byte[]> { documentContent }).FirstOrDefault();
                int counter = 0;
                foreach (var imageFaceSpecification in imagesFaceSpecifications)
                {
                    FaceSpecification face = new FaceSpecification()
                    {
                        BoundingBox = imageFaceSpecification.Key,
                        FaceId = addedDoc.ConceptInstance.Id.ToString() + "." + counter.ToString(),
                        VectorFeatue = imageFaceSpecification.Value
                    };
                    faces.Add(face);
                    counter++;
                }
            }
            SearchEngineDocumentConvertor convertor = new SearchEngineDocumentConvertor();
            ImageDocument imagesDocument = new ImageDocument()
            {
                ACL = convertor.GetSearchEngineDocumentAclFromConceptsAcl(addedDoc.Acl),
                Description = null,
                Faces = faces,
                ImageId = addedDoc.ConceptInstance.Id.ToString()
            };
            return imagesDocument;
        }

        private List<Dictionary<BoundingBox, List<double>>> ExtractFaceSpecifications(List<byte[]> imageFiles)
        {
            ImageProcessingServiceClient proxy = new ImageProcessingServiceClient();
            return proxy.ExtractFaceSpecifications(imageFiles);
        }

        public List<BoundingBox> FaceDetection(byte[] imageFile, string extention, AuthorizationParametters authorizationParametters)
        {
            ImageProcessingServiceClient proxy = new ImageProcessingServiceClient();
            return proxy.FaceDetection(imageFile, extention);
        }

        public List<RetrievedFace> FaceRecoginition(byte[] imageFile, string extention, List<BoundingBox> boundingBoxes, int count, AuthorizationParametters authorizationParametters)
        {
            ImageProcessingServiceClient proxy = new ImageProcessingServiceClient();
            Dictionary<BoundingBox, List<double>> embeddings = proxy.FaceRecoginition(imageFile, boundingBoxes, extention);
            List<RetrievedFace> retrivedFaces = new List<RetrievedFace>();
            AccessClient accessClient = new AccessClient();
            foreach (var embedding in embeddings)
            {
                retrivedFaces.AddRange(accessClient.RetrieveImageDocument(embedding, count, authorizationParametters));
            }
            return retrivedFaces.Distinct().ToList();
        }

        public bool IsMachneVisonServiceInstalled()
        {
            return IsVisonServiceInstalled;
        }
    }
}
