using System;

namespace GPAS.DataImport.Publish
{
    public class FileAlreadyExistInFileRepositoryException : Exception
    {
        public FileAlreadyExistInFileRepositoryException(string message)
            : base(message)
        {   }
    }
}
