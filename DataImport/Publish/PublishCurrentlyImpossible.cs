using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Publish
{
    public class PublishCurrentlyImpossible : Exception
    {
        public PublishCurrentlyImpossible() { }
        public PublishCurrentlyImpossible(string message) : base(message) { }
        public PublishCurrentlyImpossible(string message, Exception inner) : base(message, inner) { }
        protected PublishCurrentlyImpossible(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
