using GPAS.Dispatch.Entities.NLP.Summarization;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.NLP
{
    [DataContract]
    public class SummarizationRequest
    {
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public SummarizationRate Rate { get; set; }

    }
 
}
