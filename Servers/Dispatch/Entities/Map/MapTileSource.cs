using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Map
{
    [Serializable]
    public abstract class MapTileSource
    {
        public string UniqueName { get; set; }

        public abstract byte[] GetTileImage(int zoonLevel, long x, long y);
    }
}
