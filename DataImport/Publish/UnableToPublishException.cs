using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Publish
{
    public class UnableToPublishException : Exception
    {
        public UnableToPublishException() { }
        public UnableToPublishException(string message) : base(message) { }
        public UnableToPublishException(string message, Exception inner) : base(message, inner) { }
        protected UnableToPublishException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}