using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    public class MinimumCrossingLayoutParameters : ILayoutParameters
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        internal MinimumCrossingLayoutParameters()
        {
        }

        public int Seed
        {
            get;
            set;
        }

        /// <summary>
        /// ایجاد یک رونوشت از نمونه کلاس
        /// </summary>
        public object Clone()
        {
            return new AutoLayoutParameters();
        }
    }
}
