using System;
using System.Runtime.Serialization;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    [Serializable]
    internal class NewerShowObjectsRequestRegisteredException : Exception
    {
        public NewerShowObjectsRequestRegisteredException()
        {
        }

        public NewerShowObjectsRequestRegisteredException(string message) : base(message)
        {
        }

        public NewerShowObjectsRequestRegisteredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NewerShowObjectsRequestRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}