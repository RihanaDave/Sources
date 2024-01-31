using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.JobServer.JobWorkerProcess
{

    public class KMedia
    {
        public string URI { set; get; }

        public string Description { set; get; }

        public int Id { set; get; }

        public int ObjectId { set; get; }
    }

    public class KGraphArrangement
    {

        public string Title { set; get; }

        public string Description { set; get; }

        public int Id { set; get; }

        public string TimeCreated { set; get; }

        public byte[] GraphImage { set; get; }

        public byte[] GraphArrangement { set; get; }


        public int NodesCount { set; get; }
    }

    public class KObject
    {

        public string TypeUri { set; get; }

        public string DisplayName { set; get; }

        public int Id { set; get; }

        public Boolean IsGroup { set; get; }

    }

    public abstract class KLink
    {
        public KObject Source { set; get; }

        public KObject Target { set; get; }
    }

    
    public class KRelationship
    {
        
        public int Id { set; get; }

        
        public DateTime? TimeBegin { set; get; }

        
        public DateTime? TimeEnd { set; get; }

        
        public string Description { set; get; }

        
        public LinkDirection Direction { set; get; }
    }

    
    public class RelationshipBaseKlink : KLink
    {
        
        public KRelationship Relationship { set; get; }

        
        public string TypeURI { set; get; }

    }

    
    public class EventBaseKlink : KLink
    {
        
        public KObject SharedEvent { set; get; }

        
        public RelationshipBaseKlink SourceRelationship { set; get; }

        
        public RelationshipBaseKlink TargetRelationship { set; get; }
    }

    
    public class KLinkResult
    {
        
        public EventBaseKlink EventLink { set; get; }

        
        public RelationshipBaseKlink RelationshipLink { set; get; }
    }

    
    public enum LinkDirection
    {
        SourceToTarget,
        TargetToSource,
        Bidirectional
    }

    
    public class KProperty
    {
        
        public string TypeUri { set; get; }

        
        public string Value { set; get; }

        
        public int Id { set; get; }

        
        public KObject ObjectId { set; get; }
    }

    public struct SemiStructuredSearchResult
    {
        public List<List<string>> TotalList { set; get; }

    }
    
   

}
