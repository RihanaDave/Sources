using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GPAS.Workspace.Logic.ETLProvider
{
    public class ChunkUploadedEventArgs : EventArgs
    {
        public ChunkUploadedEventArgs(Stream stream, int index)
        {
            Stream = stream;
            Index = index;
        }

        public Stream Stream { get; protected set; }
        public int Index { get; protected set; }
    }
}
