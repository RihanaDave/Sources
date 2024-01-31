using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Map.GeoSearch
{
    public class BufferChangedEventArgs
    {
        public BufferChangedEventArgs(double buffer, Scales scale)
        {
            if (buffer < 0)
                throw new ArgumentNullException("buffer");

            Buffer = buffer;
            Scale = scale;
        }

        public double Buffer
        {
            get;
            private set;
        }

        public Scales Scale
        {
            get;
            private set;
        }
    }
}
