using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.NLP.Summarization
{
    public enum SummarizationRateType
    {
        [EnumMember]
        Paragraph = 1,
        [EnumMember]
        Percent = 2
    }
}
