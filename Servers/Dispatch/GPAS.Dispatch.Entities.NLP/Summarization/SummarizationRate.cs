using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.NLP.Summarization
{
    [DataContract]
    public class SummarizationRate
    {
        [DataMember]
        public SummarizationRateType RateType { get; set; }
        [DataMember]
        public double RateValue { get; set; }
    }
}
