using GPAS.SearchServer.Entities.SearchEngine.Documents;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace GPAS.SearchServer.Access
{
    public class ImageProcessingServiceClient
    {
        public static readonly string ImageProcessingServiceUrl = ConfigurationManager.AppSettings["ImageProcessingServiceURL"];
        public List<BoundingBox> FaceDetection(byte[] imageFile, string extention)
        {
            var client = new RestClient(ImageProcessingServiceUrl);
            var request = new RestRequest("/detect", Method.POST);
            request.AddFile("image", imageFile, "image");
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            var responseJson = JArray.Parse(response.Content);
            return ExractBoundigBoxesFromJsonResponse(responseJson);
        }

        private List<BoundingBox> ExractBoundigBoxesFromJsonResponse(JArray responseJson)
        {
            List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            foreach (var boundingBoxJson in responseJson)
            {
                BoundingBox boundingBox = new BoundingBox()
                {
                    height = (int)double.Parse(boundingBoxJson["height"].ToString()),
                    width = (int)double.Parse(boundingBoxJson["width"].ToString()),
                    topLeft = new Point()
                    {
                        X = (int)double.Parse(boundingBoxJson["x"].ToString()),
                        Y = (int)double.Parse(boundingBoxJson["y"].ToString())
                    },
                    landmarks = ConvertJTokenToLandmarks(boundingBoxJson["landmarks"])
                };
                boundingBoxes.Add(boundingBox);
            }
            return boundingBoxes;
        }

        public List<Dictionary<BoundingBox, List<double>>> ExtractFaceSpecifications(List<byte[]> imageFiles)
        {
            var client = new RestClient(ImageProcessingServiceUrl);
            var request = new RestRequest("/all-embeddings", Method.POST);

            foreach (var imageFile in imageFiles)
            {
                request.AddFile("image", imageFile, "image");
            }
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                throw new System.Runtime.Remoting.ServerException($"{response.StatusDescription} | Response content: {response.Content}");
            }

            List<Dictionary<BoundingBox, List<double>>> embeddingsAndBoundingBoxes = new List<Dictionary<BoundingBox, List<double>>>();
            if (response.Content != null)
            {
                JArray responseJson = JArray.Parse(response.Content);
                embeddingsAndBoundingBoxes = ExtractEmbeddingAndBoundinBoxesFromJsonResponse(responseJson);
            }
            return embeddingsAndBoundingBoxes;
        }

        private List<Dictionary<BoundingBox, List<double>>> ExtractEmbeddingAndBoundinBoxesFromJsonResponse(JArray responseJson)
        {
            List<Dictionary<BoundingBox, List<double>>> embeddingsAndBoundingBoxesMapping = new List<Dictionary<BoundingBox, List<double>>>();
            foreach (var bBoxAndEmbeddings in responseJson)
            {
                Dictionary<BoundingBox, List<double>> embeddingAndBoundingBoxMapping = new Dictionary<BoundingBox, List<double>>();
                foreach (var bBoxAndEmbedding in bBoxAndEmbeddings)
                {

                    BoundingBox boundingBox = new BoundingBox()
                    {
                        height = (int)double.Parse(bBoxAndEmbedding["box"]["height"].ToString()),
                        width = (int)double.Parse(bBoxAndEmbedding["box"]["width"].ToString()),
                        topLeft = new Point()
                        {
                            X = (int)double.Parse(bBoxAndEmbedding["box"]["x"].ToString()),
                            Y = (int)double.Parse(bBoxAndEmbedding["box"]["y"].ToString())
                        },
                        landmarks = ConvertJTokenToLandmarks(bBoxAndEmbedding["box"]["landmarks"])
                    };
                    List<double> embeddings = new List<double>();
                    foreach (var embedding in bBoxAndEmbedding["embedding"])
                    {
                        embeddings.Add(double.Parse(embedding.ToString()));
                    }
                    embeddingAndBoundingBoxMapping.Add(boundingBox, embeddings);

                }
                embeddingsAndBoundingBoxesMapping.Add(embeddingAndBoundingBoxMapping);
            }
            return embeddingsAndBoundingBoxesMapping;
        }

        public Dictionary<BoundingBox, List<double>> FaceRecoginition(byte[] imageFile, List<BoundingBox> boundingBoxes, string extention)
        {
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("boxes", SerializeBoundingBoxesToJArray(boundingBoxes)));
            var client = new RestClient(ImageProcessingServiceUrl);
            var request = new RestRequest("/embeddings", Method.POST);
            request.AddParameter("boxes", SerializeBoundingBoxesToJArray(boundingBoxes).ToString(), ParameterType.GetOrPost);
            //request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            request.AddFile("image", imageFile, "image");
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            Dictionary<BoundingBox, List<double>> embeddings = new Dictionary<BoundingBox, List<double>>();
            if (response.Content != null)
            {
                JArray responseJson = JArray.Parse(response.Content);
                embeddings = ExtractEmbeddingFromJsonResponse(responseJson);
            }
            return embeddings;
        }

        private Dictionary<BoundingBox, List<double>> ExtractEmbeddingFromJsonResponse(JArray responseJson)
        {
            Dictionary<BoundingBox, List<double>> embeddingAndBoundingBoxMapping = new Dictionary<BoundingBox, List<double>>();
            foreach (JToken bBoxAndEmbedding in responseJson)
            {
                BoundingBox boundingBox = new BoundingBox()
                {
                    height = (int)double.Parse(bBoxAndEmbedding["box"]["height"].ToString()),
                    width = (int)double.Parse(bBoxAndEmbedding["box"]["width"].ToString()),
                    topLeft = new Point()
                    {
                        X = (int)double.Parse(bBoxAndEmbedding["box"]["x"].ToString()),
                        Y = (int)double.Parse(bBoxAndEmbedding["box"]["y"].ToString())
                    },
                    landmarks = ConvertJTokenToLandmarks(bBoxAndEmbedding["box"]["landmarks"])
                };
                List<double> embeddings = new List<double>();
                foreach (var embedding in bBoxAndEmbedding["embedding"])
                {
                    embeddings.Add(double.Parse(embedding.ToString()));
                }
                embeddingAndBoundingBoxMapping.Add(boundingBox, embeddings);
            }
            return embeddingAndBoundingBoxMapping;
        }

        private Landmarks ConvertJTokenToLandmarks(JToken jToken)
        {
            List<double> result = new List<double>();
            foreach (var mark in jToken)
            {
                result.Add(
                    double.Parse(mark.ToString())
                    );
            }
            return new Landmarks()
            {
                marks = result
            };
        }

        private JArray SerializeBoundingBoxesToJArray(List<BoundingBox> boundingBoxes)
        {
            JArray boxes = new JArray();
            foreach (BoundingBox boundingBox in boundingBoxes)
            {
                JObject box = new JObject();
                box.Add(new JProperty("height", boundingBox.height));
                box.Add(new JProperty("width", boundingBox.width));
                box.Add(new JProperty("x", boundingBox.topLeft.X));
                box.Add(new JProperty("y", boundingBox.topLeft.Y));
                box.Add(new JProperty("confidence", 1));
                // box.Add(new JProperty("landmarks", JArray.FromObject(boundingBox.landmarks)));
                box.Add(new JProperty(new JProperty("landmarks", JArray.FromObject(boundingBox.landmarks.marks.ToArray()))));

                boxes.Add(box);
            }
            return boxes;
        }
    }
}
